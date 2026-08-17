# 🚀 Guia de Setup — Windows (Execução Manual)

**Status:** ✅ Preparado para execução quando chegar em casa  
**Ambiente:** Windows 10 Home  
**Objetivo:** Rodar o Sistema de Emissão de Notas Fiscais localmente

---

## ✅ Já Instalado (PC do Trabalho)

- ✅ **.NET 8.0.424 SDK**
- ✅ **Node.js v24.19.0 + npm v11.17.0**
- ✅ **PostgreSQL 15.19** (instalado, não inicializado)
- ✅ **Dependências .NET restauradas** (EstoqueService + FaturamentoService)
- ✅ **Dependências npm instaladas** (frontend Angular)

**Arquivos modificados:**
- ✅ `backend/EstoqueService/EstoqueService.csproj` — adicionado `Microsoft.EntityFrameworkCore.Sqlite`
- ✅ `backend/FaturamentoService/FaturamentoService.csproj` — adicionado `Microsoft.EntityFrameworkCore.Sqlite`

---

## 🏠 O que fazer EM CASA (seu PC pessoal)

### Passo 1: Liberar Espaço em Disco (5-10 min)

**Problema identificado:** Seu disco C está 100% cheio (0 bytes livres).

**Culpados principais:**
- Android SDK Emulator cache: ~3-5 GB
- Claude VM bundles: ~5-10 GB
- VS Code CachedExtensionVSIXs: ~1-2 GB
- GitHub Desktop packages: ~500 MB

**Execute em PowerShell (como Administrador):**

```powershell
Write-Host "Limpando caches e arquivos temporários..." -ForegroundColor Cyan

# 1. Android SDK temp (seguro, ~2-3 GB)
Write-Host "1. Limpando Android SDK..." -ForegroundColor Yellow
Remove-Item -Path "$env:USERPROFILE\AppData\Local\Android\Sdk\.temp" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:USERPROFILE\AppData\Local\Android\Sdk\.downloadIntermediates" -Recurse -Force -ErrorAction SilentlyContinue

# 2. WinGet instaladores (seguro, ~500 MB)
Write-Host "2. Limpando WinGet..." -ForegroundColor Yellow
Remove-Item -Path "$env:USERPROFILE\AppData\Local\Temp\WinGet" -Recurse -Force -ErrorAction SilentlyContinue

# 3. VS Code caches (seguro, ~1-2 GB)
Write-Host "3. Limpando VS Code..." -ForegroundColor Yellow
Remove-Item -Path "$env:USERPROFILE\AppData\Roaming\Code\CachedExtensionVSIXs" -Recurse -Force -ErrorAction SilentlyContinue

# 4. GitHub Desktop packages (seguro, ~500 MB)
Write-Host "4. Limpando GitHub Desktop..." -ForegroundColor Yellow
Remove-Item -Path "$env:USERPROFILE\AppData\Local\GitHubDesktop\packages" -Recurse -Force -ErrorAction SilentlyContinue

# 5. Verificar espaço
Write-Host ""
Write-Host "✅ Limpeza concluída!" -ForegroundColor Green
Get-PSDrive C | Select-Object Name, @{Name="EspaçoLivre(GB)"; Expression={[math]::Round($_.Free/1GB, 2)}}
```

**Resultado esperado:** 4-6 GB liberados

---

### Passo 2: Inicializar PostgreSQL (5 min)

**Abra PowerShell como Administrador** e execute:

```powershell
# 1. Atualizar PATH
$env:Path = "C:\Program Files\PostgreSQL\15\bin" + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# 2. Criar diretório de dados
$pgDataDir = "C:\Users\$env:USERNAME\pgdata"
Remove-Item $pgDataDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $pgDataDir -Force | Out-Null

Write-Host "Inicializando banco de dados..." -ForegroundColor Cyan
initdb --username=postgres --locale=en_US.UTF-8 --encoding=UTF-8 -D "$pgDataDir"

# 3. Iniciar servidor
Write-Host "Iniciando servidor PostgreSQL..." -ForegroundColor Cyan
pg_ctl -D "$pgDataDir" -l "$env:TEMP\postgres.log" start -w

Write-Host "✅ PostgreSQL iniciado!" -ForegroundColor Green

# 4. Testar conexão
psql -U postgres -c "SELECT version();"
```

**Resultado esperado:**
```
PostgreSQL 15.19 on x86_64-pc-windows-msvc, compiled by Visual C++ build 1937, 64-bit
(1 row)
```

---

### Passo 3: Criar Bancos de Dados (2 min)

**Ainda em PowerShell:**

```powershell
$env:Path = "C:\Program Files\PostgreSQL\15\bin" + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

Write-Host "Criando bancos de dados..." -ForegroundColor Cyan

# Conecta como postgres e cria os bancos
psql -U postgres -c "CREATE DATABASE db_estoque;"
psql -U postgres -c "CREATE DATABASE db_faturamento;"

# Criar usuário 'korp' com a senha conforme appsettings.json
psql -U postgres -c "CREATE USER korp WITH PASSWORD 'korp123';"
psql -U postgres -c "ALTER USER korp SUPERUSER;"

Write-Host "✅ Bancos criados!" -ForegroundColor Green
psql -U postgres -c "SELECT datname FROM pg_database WHERE datname IN ('db_estoque', 'db_faturamento');"
```

---

### Passo 4: Abrir 3 Terminais (PowerShell) — um para cada serviço

**Terminal 1 — Serviço de Estoque:**

```powershell
$env:Path = "C:\Program Files\PostgreSQL\15\bin" + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

cd "C:\Users\$env:USERNAME\Documents\GitHub\Korp_Teste_AdrianMatos\backend\EstoqueService"

Write-Host "🟢 Iniciando Estoque Service na porta 5001..." -ForegroundColor Green
dotnet run --configuration Release

# Resultado esperado:
# info: Microsoft.Hosting.Lifetime[14]
#   Now listening on: http://localhost:5001
```

**Terminal 2 — Serviço de Faturamento (aguarde ~5 segundos após iniciar Estoque):**

```powershell
$env:Path = "C:\Program Files\PostgreSQL\15\bin" + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

cd "C:\Users\$env:USERNAME\Documents\GitHub\Korp_Teste_AdrianMatos\backend\FaturamentoService"

Write-Host "🟢 Iniciando Faturamento Service na porta 5002..." -ForegroundColor Green
dotnet run --configuration Release

# Resultado esperado:
# info: Microsoft.Hosting.Lifetime[14]
#   Now listening on: http://localhost:5002
```

**Terminal 3 — Frontend Angular:**

```powershell
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

cd "C:\Users\$env:USERNAME\Documents\GitHub\Korp_Teste_AdrianMatos\frontend"

Write-Host "🟢 Iniciando Angular na porta 4200..." -ForegroundColor Cyan
npm start

# Resultado esperado: 
# ✔ Compiled successfully. 
# ✔ Application bundle generation complete...
```

---

## 🌐 Verificar se Tudo Está Rodando

Abra seu navegador e acesse:

| Serviço | URL | Esperado |
|---------|-----|----------|
| **Frontend** | http://localhost:4200 | Telas de Produtos e Notas Fiscais |
| **Estoque API** | http://localhost:5001/swagger | Swagger UI com endpoints |
| **Faturamento API** | http://localhost:5002/swagger | Swagger UI com endpoints |
| **PostgreSQL** | localhost:5432 | Conexão ativa (use `psql` para verificar) |

---

## ✅ Fluxo de Teste Completo

1. **Cadastre um produto:**
   - Abra http://localhost:4200
   - Vá em **"Produtos"**
   - Clique **"+ Novo Produto"**
   - Código: `P001`, Saldo: `10`
   - Experimente **"Gerar descrição com IA"** (modo offline)

2. **Crie uma nota fiscal:**
   - Vá em **"Notas Fiscais"**
   - Clique **"+ Nova Nota"**
   - Adicione o produto `P001` com quantidade `2`
   - Status deve estar **"Aberta"**

3. **Imprima a nota:**
   - Clique **"Imprimir"** (aparece um spinner)
   - Status deve mudar para **"Fechada"**
   - Saldo do produto deve cair de `10` para `8`

4. **Teste de idempotência:**
   - Tente imprimir novamente
   - Deve retornar erro: **"Nota não está aberta"**
   - Saldo permanece `8` (não decrementou de novo)

5. **Teste de falha de microsserviço:**
   - Em um terminal, pare o Estoque Service (Ctrl+C)
   - Crie outra nota e tente imprimir
   - Deve exibir: **"Serviço de Estoque indisponível. Tente novamente em alguns momentos."**
   - Reinicie o Estoque Service
   - Reimprima a nota — deve funcionar (idempotência garante sem duplicação)

---

## 🛑 Parar os Serviços

Em cada terminal, pressione **Ctrl+C** para parar.

Para parar PostgreSQL:

```powershell
$env:Path = "C:\Program Files\PostgreSQL\15\bin" + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$pgDataDir = "C:\Users\$env:USERNAME\pgdata"
pg_ctl -D "$pgDataDir" stop
```

---

## 📝 Troubleshooting

### Erro: "No space left on device"
→ Limpar espaço (Passo 1 acima)

### Erro: "Connection refused" ao conectar PostgreSQL
→ Verificar se PostgreSQL foi iniciado: `pg_ctl -D "$pgDataDir" status`

### Erro: "Cannot find path"
→ Verificar caminho do repositório (substitua o caminho completo corretamente)

### Erro: "Serviço de Estoque indisponível"
→ Normal! Significa que o Estoque está offline. Reinicie-o.

---

## 📚 Documentação Completa

Veja também:
- `README.md` — Visão geral do projeto
- `DETALHAMENTO_TECNICO.md` — Arquitetura, tecnologias, padrões
- `ROTEIRO_VIDEO.md` — Roteiro sugerido para o vídeo de apresentação

---

**Pronto! Quando chegar em casa, execute os passos acima na ordem. Qualquer dúvida, abra um terminal e tente de novo. 🚀**
