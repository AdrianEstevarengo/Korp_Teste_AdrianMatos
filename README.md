# Korp_Teste_AdrianMatos — Sistema de Emissão de Notas Fiscais

Solução para o desafio técnico da **KORP ERP** (vaga Desenvolvedor Web Fullstack C# + Angular).

Arquitetura de **microsserviços** em **C# / .NET 8**, frontend em **Angular 17** e persistência real em **PostgreSQL**. Todo o ambiente sobe com um único `docker compose up`.

---

## 🏗️ Arquitetura

```
┌─────────────────┐        HTTP (REST)          ┌────────────────────────┐
│                 │  ─────────────────────────▶ │   Serviço de Estoque   │
│  Angular (SPA)  │                             │      (porta 5001)      │
│   porta 4200    │                             │   Produtos + Saldos    │
│                 │        HTTP (REST)          └───────────┬────────────┘
│                 │  ─────────────────────────▶             │ HTTP + Polly
│                 │  ┌────────────────────────┐             │ (retry / circuit breaker)
└─────────────────┘  │ Serviço de Faturamento │ ◀───────────┘
                     │      (porta 5002)      │
                     │      Notas Fiscais     │
                     └───────────┬────────────┘
                                 │
                    ┌────────────┴─────────────┐
                    │        PostgreSQL        │
                    │  db_estoque │ db_faturamento │
                    └──────────────────────────┘
```

Dois microsserviços independentes, cada um com seu próprio banco (*database-per-service*):

| Serviço | Responsabilidade | Porta | Banco |
|---|---|---|---|
| **Estoque** | Cadastro de produtos e controle de saldo | 5001 | `db_estoque` |
| **Faturamento** | Emissão e impressão de notas fiscais | 5002 | `db_faturamento` |

O serviço de **Faturamento** consome o de **Estoque** via HTTP para dar baixa no estoque no momento da impressão.

---

## ✅ Requisitos atendidos

### Funcionalidades
- [x] **Cadastro de Produtos** — código, descrição e saldo.
- [x] **Cadastro de Notas Fiscais** — numeração sequencial, status `Aberta`/`Fechada`, múltiplos produtos com quantidades.
- [x] **Impressão de Notas Fiscais** — botão com indicador de processamento, muda status para `Fechada`, bloqueia impressão de notas não-`Aberta` e dá baixa no saldo dos produtos.

### Requisitos obrigatórios
- [x] **Microsserviços** — Estoque + Faturamento, independentes.
- [x] **Tratamento de falhas** — quando o Serviço de Estoque está fora do ar, o Faturamento aplica *retry* + *circuit breaker* (Polly), mantém a nota como `Aberta` e devolve mensagem clara ao usuário (a UI exibe um alerta).
- [x] **Conexão real com banco** — PostgreSQL via Entity Framework Core.

### Requisitos opcionais
- [x] **Tratamento de concorrência** — controle otimista (coluna de sistema `xmin` do PostgreSQL) impede saldo negativo quando duas notas usam o mesmo produto simultaneamente.
- [x] **Idempotência** — a impressão usa uma *Idempotency-Key*; cliques repetidos não dão baixa dupla no estoque.
- [x] **Uso de IA** — geração automática da descrição do produto a partir do código (endpoint no Estoque, botão na tela de produtos).

---

## 🚀 Como executar

### Opção A — Docker (recomendado, sobe tudo de uma vez)

Pré-requisito: **Docker** e **Docker Compose**.

```bash
docker compose up --build
```

Aguarde os serviços subirem e acesse:

- **Frontend:** http://localhost:4200
- **API Estoque (Swagger):** http://localhost:5001/swagger
- **API Faturamento (Swagger):** http://localhost:5002/swagger

Para testar o cenário de **falha de microsserviço**, derrube o Estoque e tente imprimir uma nota:

```bash
docker compose stop estoque-service
```

A UI mostrará a mensagem de erro e a nota permanecerá `Aberta`. Suba de novo com `docker compose start estoque-service` e reimprima — a idempotência garante baixa única.

### Opção B — Execução manual (desenvolvimento)

Pré-requisitos: **.NET 8 SDK**, **Node 18+ / Angular CLI**, **PostgreSQL** rodando local.

```bash
# 1. Banco: crie os bancos db_estoque e db_faturamento
#    e ajuste a connection string em cada appsettings.json

# 2. Serviço de Estoque
cd backend/EstoqueService
dotnet run              # http://localhost:5001

# 3. Serviço de Faturamento (em outro terminal)
cd backend/FaturamentoService
dotnet run              # http://localhost:5002

# 4. Frontend (em outro terminal)
cd frontend
npm install
npm start               # http://localhost:4200
```

> As tabelas são criadas automaticamente na inicialização de cada serviço.

---

## 📁 Estrutura do repositório

```
Korp_Teste_AdrianMatos/
├── backend/
│   ├── EstoqueService/          # Microsserviço de Estoque (produtos + saldo + IA)
│   └── FaturamentoService/      # Microsserviço de Faturamento (notas fiscais)
├── frontend/                    # SPA Angular 17
├── docker-compose.yml           # Orquestra Postgres + 2 serviços + Angular
├── DETALHAMENTO_TECNICO.md      # Detalhamento técnico exigido pelo desafio
├── ROTEIRO_VIDEO.md             # Roteiro sugerido para o vídeo de apresentação
└── README.md
```

---

## 🧪 Fluxo de teste sugerido

1. Cadastre um produto (ex.: código `P001`, saldo `10`). Experimente o botão **"Gerar descrição com IA"**.
2. Crie uma nota fiscal adicionando esse produto com quantidade `2`.
3. Clique em **Imprimir**: o status vira `Fechada` e o saldo do produto cai para `8`.
4. Tente imprimir a mesma nota de novo → bloqueado (não está mais `Aberta`).
5. Derrube o serviço de Estoque e tente imprimir outra nota → mensagem de erro amigável, nota continua `Aberta`.

---

## 👤 Autor

**Adrian Matos** — desafio técnico KORP ERP.
