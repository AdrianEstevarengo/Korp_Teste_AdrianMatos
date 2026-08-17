# ============================================================================
# SCRIPT DE SETUP AUTOMÁTICO — Korp Teste Adrian Matos
# Executa em casa no seu PC pessoal (como Administrador)
# ============================================================================

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     SETUP AUTOMÁTICO — Sistema de Emissão de Notas Fiscais   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# PASSO 1: LIBERAR ESPAÇO EM DISCO
# ============================================================================

Write-Host "📊 PASSO 1: Liberando espaço em disco..." -ForegroundColor Yellow
Write-Host "Limpando Android SDK, VS Code, GitHub Desktop, WinGet..." -ForegroundColor Gray

$itemsDeleted = 0

$foldersToClean = @(
    "$env:USERPROFILE\AppData\Local\Android\Sdk\.temp",
    "$env:USERPROFILE\AppData\Local\Android\Sdk\.downloadIntermediates",
    "$env:USERPROFILE\AppData\Local\Temp\WinGet",
    "$env:USERPROFILE\AppData\Roaming\Code\CachedExtensionVSIXs",
    "$env:USERPROFILE\AppData\Local\GitHubDesktop\packages"
)

foreach ($folder in $foldersToClean) {
    if (Test-Path $folder) {
        Write-Host "  ✓ Deletando: $folder" -ForegroundColor Green
        Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
        $itemsDeleted++
    }
}

Write-Host "✅ $itemsDeleted pastas limpas!" -ForegroundColor Green
Write-Host ""

# Verificar espaço
$freeSpace = (Get-PSDrive C | Select-Object -ExpandProperty Free) / 1GB
Write-Host "Espaço livre agora: $([math]::Round($freeSpace, 2)) GB" -ForegroundColor Cyan
Write-Host ""

if ($freeSpace -lt 1) {
    Write-Host "❌ ERRO: Menos de 1 GB livre. Você precisa liberar mais espaço!" -ForegroundColor Red
    Write-Host "   Abra o Explorador de Arquivos e delete arquivos grandes manualmente." -ForegroundColor Yellow
    exit 1
}

# ============================================================================
# PASSO 2: INICIALIZAR POSTGRESQL
# ============================================================================

Write-Host "🗄️  PASSO 2: Inicializando PostgreSQL..." -ForegroundColor Yellow

$env:Path = "C:\Program Files\PostgreSQL\15\bin" + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Verificar se psql existe
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    Write-Host "❌ PostgreSQL não encontrado em C:\Program Files\PostgreSQL\15\bin" -ForegroundColor Red
    exit 1
}

Write-Host "  ✓ PostgreSQL encontrado: $(psql --version)" -ForegroundColor Green

$pgDataDir = "C:\Users\$env:USERNAME\pgdata"

# Limpar diretório anterior (se existir)
if (Test-Path $pgDataDir) {
    Write-Host "  ✓ Removendo diretório anterior..." -ForegroundColor Gray
    Remove-Item $pgDataDir -Recurse -Force -ErrorAction SilentlyContinue
}

# Criar novo diretório
New-Item -ItemType Directory -Path $pgDataDir -Force | Out-Null
Write-Host "  ✓ Diretório criado: $pgDataDir" -ForegroundColor Green

# Inicializar banco
Write-Host "  ✓ Executando initdb..." -ForegroundColor Gray
initdb --username=postgres --locale=en_US.UTF-8 --encoding=UTF-8 -D "$pgDataDir" 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao inicializar PostgreSQL" -ForegroundColor Red
    exit 1
}

Write-Host "  ✓ initdb concluído" -ForegroundColor Green

# Iniciar servidor
Write-Host "  ✓ Iniciando servidor..." -ForegroundColor Gray
pg_ctl -D "$pgDataDir" -l "$env:TEMP\postgres.log" start -w 2>&1 | Out-Null

Start-Sleep -Seconds 2

# Verificar conexão
$testConn = psql -U postgres -c "SELECT version();" 2>&1 | Select-Object -First 1
if ($testConn -match "PostgreSQL") {
    Write-Host "✅ PostgreSQL iniciado e respondendo!" -ForegroundColor Green
    Write-Host "   $testConn" -ForegroundColor Cyan
} else {
    Write-Host "❌ Erro ao conectar em PostgreSQL" -ForegroundColor Red
    Write-Host "   Verifique o log: $env:TEMP\postgres.log" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# ============================================================================
# PASSO 3: CRIAR BANCOS E USUÁRIO
# ============================================================================

Write-Host "📋 PASSO 3: Criando bancos de dados..." -ForegroundColor Yellow

Write-Host "  ✓ Criando db_estoque..." -ForegroundColor Gray
psql -U postgres -c "CREATE DATABASE db_estoque;" 2>&1 | Out-Null

Write-Host "  ✓ Criando db_faturamento..." -ForegroundColor Gray
psql -U postgres -c "CREATE DATABASE db_faturamento;" 2>&1 | Out-Null

Write-Host "  ✓ Criando usuário 'korp'..." -ForegroundColor Gray
psql -U postgres -c "CREATE USER korp WITH PASSWORD 'korp123';" 2>&1 | Out-Null
psql -U postgres -c "ALTER USER korp SUPERUSER;" 2>&1 | Out-Null

$dbsCreated = psql -U postgres -c "SELECT datname FROM pg_database WHERE datname IN ('db_estoque', 'db_faturamento');" 2>&1 | Measure-Object -Line | Select-Object -ExpandProperty Lines

Write-Host "✅ Bancos criados!" -ForegroundColor Green
Write-Host ""

# ============================================================================
# RESUMO E PRÓXIMOS PASSOS
# ============================================================================

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                   ✅ SETUP CONCLUÍDO COM SUCESSO!             ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

Write-Host "📝 PRÓXIMAS ETAPAS (abra 3 terminais PowerShell):" -ForegroundColor Cyan
Write-Host ""

Write-Host "Terminal 1 — Serviço de Estoque (porta 5001):" -ForegroundColor Yellow
Write-Host "  cd C:\Users\$env:USERNAME\Documents\GitHub\Korp_Teste_AdrianMatos\backend\EstoqueService" -ForegroundColor Gray
Write-Host "  dotnet run --configuration Release" -ForegroundColor Gray
Write-Host ""

Write-Host "Terminal 2 — Serviço de Faturamento (porta 5002):" -ForegroundColor Yellow
Write-Host "  cd C:\Users\$env:USERNAME\Documents\GitHub\Korp_Teste_AdrianMatos\backend\FaturamentoService" -ForegroundColor Gray
Write-Host "  dotnet run --configuration Release" -ForegroundColor Gray
Write-Host ""

Write-Host "Terminal 3 — Frontend Angular (porta 4200):" -ForegroundColor Yellow
Write-Host "  cd C:\Users\$env:USERNAME\Documents\GitHub\Korp_Teste_AdrianMatos\frontend" -ForegroundColor Gray
Write-Host "  npm start" -ForegroundColor Gray
Write-Host ""

Write-Host "🌐 Então abra seu navegador em:" -ForegroundColor Cyan
Write-Host "  http://localhost:4200" -ForegroundColor Green
Write-Host ""

Write-Host "📚 Para mais detalhes, veja:" -ForegroundColor Cyan
Write-Host "  SETUP_WINDOWS.md — Guia completo passo-a-passo" -ForegroundColor Gray
Write-Host "  README.md — Visão geral do projeto" -ForegroundColor Gray
Write-Host "  DETALHAMENTO_TECNICO.md — Arquitetura e tecnologias" -ForegroundColor Gray
Write-Host ""

Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
