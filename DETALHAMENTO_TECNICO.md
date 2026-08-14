# Detalhamento Técnico — Sistema de Emissão de Notas Fiscais

Autor: **Adrian Matos** · Vaga: Desenvolvedor Web Fullstack (C# + Angular) · KORP ERP

Este documento responde, ponto a ponto, aos itens de detalhamento técnico solicitados na especificação do desafio, e descreve as decisões de arquitetura da solução.

---

## Visão geral da arquitetura

A solução segue uma arquitetura de **microsserviços** com **dois serviços de backend independentes** e um **frontend SPA**:

- **Serviço de Estoque** (`C# / ASP.NET Core`) — cadastro de produtos e controle de saldo. Banco próprio `db_estoque`.
- **Serviço de Faturamento** (`C# / ASP.NET Core`) — emissão e impressão de notas fiscais. Banco próprio `db_faturamento`.
- **Frontend** (`Angular 17`) — telas de produtos e notas fiscais.

Cada serviço tem seu próprio banco PostgreSQL (padrão *database-per-service*), reforçando o baixo acoplamento. O Faturamento se comunica com o Estoque por **HTTP/REST** para dar baixa no estoque no momento da impressão. Todo o ambiente é orquestrado por Docker Compose.

---

## Ciclos de vida do Angular utilizados

Foram utilizados dois hooks de ciclo de vida, ambos nos componentes de página (`ProdutosComponent` e `NotasComponent`):

- **`ngOnInit`** — usado para a carga inicial de dados assim que o componente é inicializado (busca de produtos e notas via HTTP). É o momento adequado porque as dependências injetadas já estão disponíveis e evita efeitos colaterais no construtor.
- **`ngOnDestroy`** — usado para liberar recursos quando o componente é destruído. Um `Subject` (`destruir$`) é completado nesse hook e combinado com o operador `takeUntil`, garantindo o **cancelamento automático de todas as assinaturas** (Observables) e prevenindo *memory leaks*.

---

## Uso da biblioteca RxJS

Sim, o RxJS é usado de forma central no frontend:

- **Observables do `HttpClient`** — todas as chamadas às APIs retornam `Observable`, consumidos com `.subscribe()`.
- **`Subject` + `takeUntil`** — padrão de *unsubscribe* declarativo. Cada componente mantém um `Subject<void>` que emite em `ngOnDestroy`; o operador `takeUntil(this.destruir$)` encadeado em cada stream encerra as assinaturas automaticamente.
- **`forkJoin`** — em `NotasComponent.ngOnInit`, produtos e notas são buscados **em paralelo** e a tela só é renderizada quando ambos chegam, simplificando o controle de estado de carregamento.

---

## Outras bibliotecas utilizadas e finalidade

**Frontend**
- **@angular/router** — roteamento SPA com *lazy loading* (`loadComponent`) das páginas.
- **@angular/forms** — *two-way binding* (`ngModel`) nos formulários.
- **RxJS** — programação reativa (detalhada acima).

**Backend**
- **Entity Framework Core** + **Npgsql.EntityFrameworkCore.PostgreSQL** — ORM e provider PostgreSQL (persistência real).
- **Swashbuckle.AspNetCore (Swagger)** — documentação/execução interativa das APIs.
- **Polly** (`Microsoft.Extensions.Http.Polly` + `Polly.Extensions.Http`) — resiliência na comunicação entre microsserviços (*retry* + *circuit breaker*).

---

## Bibliotecas de componentes visuais

Foi utilizado o **Angular Material** (`@angular/material` + `@angular/cdk`), com o tema pré-construído *indigo-pink*. Componentes usados: `MatToolbar`, `MatCard`, `MatTable`, `MatFormField`, `MatInput`, `MatSelect`, `MatButton`, `MatIcon`, `MatList`, `MatProgressSpinner` (indicador de processamento) e `MatSnackBar` (feedback de sucesso/erro ao usuário). Ícones via *Material Icons*.

---

## Gerenciamento de dependências no Golang (se aplicável)

**Não aplicável** — o backend foi implementado em **C#**, não em Go. No ecossistema .NET, o gerenciamento de dependências é feito via **NuGet**, declarado nos arquivos `.csproj` através de elementos `<PackageReference>` com versão fixada, e restaurado com `dotnet restore` (executado automaticamente no build da imagem Docker). No frontend, o gerenciamento é via **npm** (`package.json`).

---

## Frameworks utilizados no C#

- **ASP.NET Core 8 (Web API)** — camada HTTP: controllers, injeção de dependência, middlewares, CORS e configuração.
- **Entity Framework Core 8** — mapeamento objeto-relacional, *migrations*/criação de schema, LINQ-to-Entities e controle de concorrência.

---

## Tratamento de erros e exceções no backend

O tratamento é centralizado e em camadas:

1. **Exceções de domínio tipadas** — `NaoEncontradoException` (→ HTTP 404), `RegraNegocioException` (→ HTTP 409) e, no Faturamento, `EstoqueIndisponivelException` (→ HTTP 503). O código de negócio lança a exceção semântica em vez de lidar com status HTTP.
2. **Middleware global** (`ExceptionHandlingMiddleware`) — intercepta as exceções e as converte em respostas JSON padronizadas `{ status, titulo, detalhe }`, com o status HTTP correto. Exceções não previstas viram 500 com mensagem genérica (sem vazar *stack trace*), mas são logadas.
3. **Resiliência entre serviços (Polly)** — no Faturamento, o cliente HTTP do Estoque aplica *retry* com *backoff* exponencial (3 tentativas) para falhas transientes e um *circuit breaker* (abre após 5 falhas por 15s). Se o Estoque estiver indisponível, o cliente lança `EstoqueIndisponivelException`, a nota **permanece Aberta** e o usuário recebe feedback claro na tela — atendendo ao requisito de *tratamento de falhas* (o sistema se recupera e informa o erro).

No frontend, os erros HTTP são capturados nos `subscribe({ error })` e exibidos via `MatSnackBar`, lendo o campo `detalhe` retornado pelo backend.

---

## Uso de LINQ (implementação em C#)

Sim, **LINQ** é utilizado extensivamente com o EF Core (LINQ-to-Entities, traduzido para SQL):

- **Projeção**: `_db.Produtos.OrderBy(p => p.Codigo).Select(p => new ProdutoResponseDto(...))` — projeta direto no banco, sem materializar a entidade inteira.
- **Existência**: `await _db.Produtos.AnyAsync(p => p.Codigo == codigo)` — traduzido para `EXISTS`, valida código duplicado.
- **Agregação**: `await _db.Notas.MaxAsync(n => (int?)n.Numero)` — calcula a numeração sequencial da nota.
- **Filtro em conjunto**: `_db.Produtos.Where(p => ids.Contains(p.Id))` — traduzido para `WHERE id IN (...)`, carrega em uma consulta todos os produtos de uma baixa de estoque.
- **LINQ-to-Objects** em memória para mapear entidades → DTOs (`.Select(...)`) na resposta das notas.

---

## Requisitos opcionais implementados

### Tratamento de concorrência
Cenário previsto: um produto com saldo 1 sendo consumido por duas notas ao mesmo tempo. A entidade `Produto` usa **controle de concorrência otimista** via a coluna de sistema **`xmin`** do PostgreSQL (`UseXminAsConcurrencyToken()`). Se duas transações tentam decrementar o mesmo produto, a segunda recebe `DbUpdateConcurrencyException`; o serviço recarrega o saldo atual e repete a operação (até 5 tentativas). Como a validação `saldo >= quantidade` é reavaliada a cada tentativa, **o saldo nunca fica negativo** — a segunda nota recebe erro de saldo insuficiente.

### Idempotência
A impressão gera uma **chave de idempotência** (GUID) persistida na nota na primeira tentativa e reenviada no header `Idempotency-Key` a cada chamada ao Estoque. O Estoque registra as chaves já processadas (`registros_idempotencia`) e, se a mesma chave chegar de novo (ex.: clique duplo ou *retry* após timeout), **retorna o resultado anterior sem debitar de novo**. O registro de idempotência é gravado na mesma transação da baixa de saldo, garantindo atomicidade.

### Uso de Inteligência Artificial
A tela de produtos oferece **"Gerar descrição com IA"**: o Estoque expõe `POST /api/produtos/gerar-descricao`, que gera a descrição a partir do código do produto. Se a chave `Ia:ApiKey` estiver configurada, a geração usa a API de chat da OpenAI; caso contrário, opera em **modo simulado (offline)**, montando uma descrição plausível localmente — assim o recurso funciona na avaliação mesmo sem chave, e a resposta indica se veio de IA real (`geradoPorIa`).

---

## Modelo de dados

**Estoque** — `produtos` (`id`, `codigo` único, `descricao`, `saldo`, `xmin` para concorrência) e `registros_idempotencia` (`chave` única, `resultado_json`).

**Faturamento** — `notas_fiscais` (`id`, `numero` sequencial único, `status`, `data_criacao`, `data_impressao`, `chave_idempotencia`) e `itens_nota` (`id`, `nota_fiscal_id`, `produto_id`, `codigo`, `descricao`, `quantidade`), com relação 1-N e *cascade delete*.

---

## Fluxo da impressão (passo a passo)

1. Usuário clica em **Imprimir** (indicador de processamento aparece).
2. Faturamento valida que a nota está **Aberta** (senão, 409).
3. Gera/reutiliza a chave de idempotência e chama o Estoque para dar baixa nos saldos (com *retry*/*circuit breaker*).
4. Estoque valida saldos, decrementa com controle de concorrência e registra a idempotência.
5. Em caso de sucesso, o Faturamento muda o status para **Fechada** e grava a data de impressão.
6. Em caso de indisponibilidade do Estoque, a nota **continua Aberta** e a UI exibe o erro; o usuário pode tentar de novo (a idempotência evita baixa dupla).
