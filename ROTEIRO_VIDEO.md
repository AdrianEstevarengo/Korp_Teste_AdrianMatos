# Roteiro do Vídeo de Apresentação

Duração sugerida: **8 a 12 minutos**. Grave a tela (OBS, Loom ou a gravação do próprio sistema operacional) com áudio narrado. Ao final, suba em Google Drive/OneDrive e coloque o link **público** no e-mail para rh@korp.com.br.

O vídeo deve cobrir três eixos: **telas**, **funcionalidades** e **detalhamento técnico**.

---

## 1. Abertura (30s)
- Diga seu nome e a vaga (Desenvolvedor Fullstack C# + Angular).
- Resuma a solução em uma frase: "Sistema de emissão de notas fiscais em arquitetura de microsserviços, C#/.NET no backend, Angular no frontend e PostgreSQL."
- Mostre o diagrama de arquitetura do README.

## 2. Subindo o ambiente (1min)
- Mostre o terminal rodando `docker compose up --build`.
- Abra o Swagger dos dois serviços (localhost:5001/swagger e localhost:5002/swagger) para evidenciar os microsserviços separados.

## 3. Cadastro de Produtos (2min)
- Abra a tela **Produtos**.
- Cadastre um produto (ex.: código `P001`, saldo `10`).
- **Destaque a IA**: digite só o código, clique em **"Gerar descrição com IA"**, mostre a descrição preenchida e o spinner de processamento.
- Salve e mostre o produto na tabela.

## 4. Cadastro de Nota Fiscal (2min)
- Abra a tela **Notas Fiscais**.
- Selecione o produto, informe quantidade `2`, adicione à nota (adicione um segundo item se quiser).
- Crie a nota e mostre que ela nasce com **status Aberta** e numeração sequencial.

## 5. Impressão + baixa de estoque (2min)
- Clique em **Imprimir**: mostre o indicador de processamento.
- Mostre o status virar **Fechada**.
- Volte em Produtos e mostre o saldo caindo de 10 para 8 (baixa de estoque).
- Tente imprimir a mesma nota de novo: o botão fica desabilitado / retorna erro (não é Aberta).

## 6. Tratamento de falhas — requisito obrigatório (1min30)
- No terminal: `docker compose stop estoque-service`.
- Crie/tente imprimir outra nota → mostre a mensagem de erro amigável (Estoque indisponível) e que a nota **continua Aberta**.
- Suba de novo: `docker compose start estoque-service`, reimprima → funciona, sem baixa dupla (idempotência).

## 7. Opcionais: concorrência e idempotência (1min)
- Explique verbalmente (ou mostre no Swagger) o cenário de saldo 1 com duas notas: o controle otimista via `xmin` impede saldo negativo.
- Mostre o header `Idempotency-Key` sendo usado / explique o registro de idempotência.

## 8. Detalhamento técnico (2min)
Com o `DETALHAMENTO_TECNICO.md` na tela, percorra rapidamente:
- Ciclos de vida Angular (`ngOnInit`, `ngOnDestroy`).
- RxJS (`Observable`, `Subject` + `takeUntil`, `forkJoin`).
- Angular Material como biblioteca visual.
- Frameworks C#: ASP.NET Core + EF Core.
- Tratamento de erros: middleware + exceções tipadas + Polly.
- LINQ: mostre um exemplo no código (`Where`, `Select`, `AnyAsync`, `MaxAsync`).

## 9. Encerramento (30s)
- Recapitule os requisitos atendidos (obrigatórios + os 3 opcionais).
- Agradeça e cite o link do repositório.

---

### Dica
Deixe alguns produtos e uma nota já cadastrados antes de gravar para agilizar, mas grave pelo menos **um** cadastro e **uma** impressão ao vivo para demonstrar o fluxo real.
