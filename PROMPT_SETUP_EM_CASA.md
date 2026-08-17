# 🏠 PROMPT PARA SETUP EM CASA — Claude Code

**Copie e cole este texto completo no Claude Code quando chegar em casa no seu PC pessoal.**

---

## 📋 **PROMPT COMPLETO**

```
Vou desenvolver um Sistema de Emissão de Notas Fiscais para um processo seletivo (vaga Desenvolvedor Web Fullstack C# + Angular).

O projeto já está 100% codificado e sincronizado no GitHub, mas preciso rodar ele localmente no meu PC em casa para:
1. Testar o fluxo completo (cadastro de produtos → notas → impressão)
2. Gravar um vídeo de demonstração
3. Apresentar o desafio

Aqui está o que preciso que você faça, passo-a-passo:

---

## 🏠 SETUP COMPLETO NO MEU PC EM CASA

### ✅ Pré-requisitos (verificar se estão instalados):
- Windows 10/11
- winget (gerenciador de pacotes Windows) — geralmente já vem instalado
- Git
- Espaço em disco: ~15-20 GB livres

---

### 📥 PASSO 1: Instalar Visual Studio 2022 Community

Verifique se está instalado. Se não tiver, execute este comando em PowerShell (como Administrador):

```powershell
winget install Microsoft.VisualStudio.2022.Community --override "--passive --installWhileDownloading --wait --norestart"
```

⏱️ **Vai levar ~20-30 minutos** (download + instalação)

Avise-me quando a instalação terminar.

---

### 📥 PASSO 2: Instalar Node.js (se não tiver)

Execute em PowerShell:

```powershell
winget install OpenJS.NodeJS.LTS
```

Confirme quando terminar.

---

### 📥 PASSO 3: Clonar o Repositório

Execute em PowerShell:

```powershell
cd $env:USERPROFILE\Documents
git clone https://github.com/AdrianEstevarengo/Korp_Teste_AdrianMatos.git
cd Korp_Teste_AdrianMatos
```

Verifique que você está no diretório correto (deve conter pastas `backend`, `frontend`, `infra`).

---

### 🖼️ PASSO 4: Abrir a Solução no Visual Studio

1. No Explorador de Arquivos, navegue até: `$env:USERPROFILE\Documents\Korp_Teste_AdrianMatos`
2. **Clique duplo em: `KorpTeste.sln`**
3. Visual Studio abrirá automaticamente
4. **Aguarde a restauração de dependências NuGet** — você verá na barra de status (inferior) algo como "Restoring NuGet packages..."
5. Quando terminar, deve aparecer "Ready"

Avise-me quando o VS terminar de carregar.

---

### ⚙️ PASSO 5: Configurar Múltiplos Projetos de Inicialização

Você precisa rodar **dois serviços simultaneamente**:

1. No **Solution Explorer** (lado esquerdo do VS), clique **direito na solução** (primeira linha, acima de "EstoqueService")
2. Selecione **"Set Startup Projects"**
3. Na janela que abrir, escolha **"Multiple startup projects"**
4. Para cada projeto, defina:
   - **EstoqueService** → Action: **Start** (dropdown deve estar em verde ▶️)
   - **FaturamentoService** → Action: **Start** (dropdown deve estar em verde ▶️)
5. Clique em **OK**

Confirme quando estiver configurado.

---

### ▶️ PASSO 6: Rodar os Serviços Backend

Pressione **F5** (ou clique no botão ▶️ verde na barra de ferramentas).

**O que deve acontecer:**
- Duas janelas de console (pretas) vão abrir
- Você verá mensagens de inicialização do .NET
- Procure pelas linhas:
  ```
  Now listening on: http://localhost:5000
  ```
  e
  ```
  Now listening on: http://localhost:5002
  ```

**Confirme quando ambos os serviços estiverem rodando.**

---

### 🌐 PASSO 7: Rodar o Frontend Angular

**Abra um novo PowerShell SEPARADO** (não feche o do VS):

```powershell
cd $env:USERPROFILE\Documents\Korp_Teste_AdrianMatos\frontend
npm install
npm start
```

Isso vai levar ~30-60 segundos compilando.

**Quando terminar, você verá:**
```
✔ Compiled successfully.
✔ Application bundle generation complete [123.456 seconds].
```

Confirme quando Angular estiver compilado.

---

### 🎯 PASSO 8: Acessar a Aplicação e Testar

Abra seu navegador em:
- **http://localhost:4200**

Você deve ver as telas: **"Produtos"** e **"Notas Fiscais"**

**Teste o fluxo completo:**

1. Clique em **"Produtos"** (aba superior)
2. Clique em **"+ Novo Produto"**
3. Preencha:
   - **Código:** `P001`
   - **Saldo:** `10`
4. Clique em **"Salvar"** ou **"Gerar descrição com IA"** (modo offline)
5. Clique em **"Notas Fiscais"**
6. Clique em **"+ Nova Nota"**
7. Adicione o produto `P001` com quantidade `2`
8. Clique em **"Salvar"**
9. Clique em **"Imprimir"**

**Resultado esperado:**
- Status da nota vira **"Fechada"**
- Saldo do produto cai de `10` para `8`
- Se tentar imprimir novamente, deve aparecer mensagem de erro: "Nota não está aberta"

**Avise-me quando o teste estiver completo!**

---

### 📚 Documentação Disponível no Repositório:

Se precisar de referência:
- **GUIA_VISUAL_STUDIO.md** — Guia detalhado passo-a-passo
- **SETUP_WINDOWS.md** — Instruções alternativas
- **README.md** — Visão geral do projeto
- **DETALHAMENTO_TECNICO.md** — Arquitetura, tecnologias e padrões
- **ROTEIRO_VIDEO.md** — Roteiro para gravar o vídeo de demonstração

---

### 📊 Confirmações Esperadas:

Ao final, você deve ter:
- ✅ Visual Studio 2022 Community instalado
- ✅ Node.js instalado
- ✅ Repositório clonado
- ✅ Solução aberta no VS com dois projetos
- ✅ EstoqueService rodando na porta 5000
- ✅ FaturamentoService rodando na porta 5002
- ✅ Frontend Angular rodando na porta 4200
- ✅ Fluxo completo testado (produto → nota → impressão)

---

### ⚠️ Se algo der erro:

**Avise-me com:**
1. A mensagem de erro exata
2. Em qual passo deu erro
3. Se possível, compartilhe prints do console

Vamos resolver junto!

---

## 🎥 Próximo Passo: Gravar o Vídeo

Quando tudo estiver rodando com sucesso, você estará pronto para:
- Demonstrar as telas funcionando
- Explicar o fluxo completo
- Demonstrar tratamento de falhas
- Apresentar idempotência e concorrência

Use o arquivo **ROTEIRO_VIDEO.md** como guia.

---

## 🚀 Vamos lá!

Mande este prompt e deixa comigo! Vou guiar você em cada passo até tudo estar rodando perfeitamente.
```

---

## 📝 **Como Usar Este Documento:**

1. **Salve este arquivo** em um local fácil de acessar (Desktop, Documentos, etc.)
2. **Quando chegar em casa**, abra o arquivo
3. **Copie todo o texto** (começando em "Vou desenvolver..." até "...tudo estar rodando perfeitamente.")
4. **Cole no Claude Code**
5. **Clique em Submit** e siga as instruções

---

## 💡 **Dica:**

Se quiser, pode enviar o arquivo por email para você mesmo, ou salvar em um pendrive para ter acesso fácil em casa!

**Boa sorte! 🚀**
