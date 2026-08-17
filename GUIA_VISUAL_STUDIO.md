# 🚀 Guia: Executar com Visual Studio 2022

Siga este guia **no seu PC em casa** (onde tem espaço).

---

## ✅ Pré-requisitos

- [ ] .NET 8 SDK
- [ ] Visual Studio 2022 (Community edition é gratuita)
- [ ] Node.js 18+
- [ ] Git

---

## 📥 Passo 1: Clonar o Repositório

Abra **PowerShell** e execute:

```powershell
cd $env:USERPROFILE\Documents

git clone https://github.com/AdrianEstevarengo/Korp_Teste_AdrianMatos.git

cd Korp_Teste_AdrianMatos
```

---

## 🖼️ Passo 2: Abrir a Solução no Visual Studio 2022

### Opção A: Abrir via Explorador de Arquivos (mais fácil)

1. Abra **Explorador de Arquivos**
2. Navegue para: `Korp_Teste_AdrianMatos`
3. **Clique duplo em `KorpTeste.sln`**
4. Visual Studio abrirá automaticamente

### Opção B: Abrir via Visual Studio

1. Abra **Visual Studio 2022**
2. Clique em **"File → Open → Project/Solution"**
3. Navegue até o arquivo `KorpTeste.sln`
4. Clique em **"Open"**

---

## 🏗️ Passo 3: Restaurar Dependências (Automático)

Quando a solução abrir, o Visual Studio automaticamente:
- ✅ Restaura pacotes NuGet
- ✅ Carrega os dois projetos (.csproj)

Aguarde até aparecer "Ready" na barra de status (inferior).

---

## ▶️ Passo 4: Configurar Múltiplos Projetos de Inicialização

Você vai rodar **dois serviços simultaneamente**. Para isso:

1. **Clique direito na solução** (Solution Explorer, lado esquerdo)
2. Selecione **"Set Startup Projects"**
3. Escolha **"Multiple startup projects"**
4. Defina:
   - `EstoqueService` → **Start** (verde ▶️)
   - `FaturamentoService` → **Start** (verde ▶️)
5. Clique em **OK**

---

## ▶️ Passo 5: Rodar os Serviços Backend

### Iniciar os dois serviços juntos:

Pressione **`F5`** ou clique no botão ▶️ verde na barra de ferramentas.

**O que deve acontecer:**
- Duas janelas de console aparecem (pretas)
- EstoqueService na porta 5000
- FaturamentoService na 5002
- Ambos mostram "listening on: http://localhost:5000" e "http://localhost:5002"

**Resultado esperado no console:**

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000

info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5002
```

---

## 🌐 Passo 6: Rodar o Frontend (em outro terminal)

Abra um **novo PowerShell** separado:

```powershell
cd $env:USERPROFILE\Documents\Korp_Teste_AdrianMatos\frontend

npm install   # (apenas primeira vez)
npm start
```

Angular compilará em ~30-60 segundos. Quando ver:

```
✔ Compiled successfully.
```

---

## 🎯 Passo 7: Acessar a Aplicação

Abra seu navegador:

| Componente | URL | Status |
|-----------|-----|--------|
| **Frontend** | http://localhost:4200 | ✅ Acesso completo |
| **Swagger Estoque** | http://localhost:5000/swagger | 📚 Documentação |
| **Swagger Faturamento** | http://localhost:5002/swagger | 📚 Documentação |

---

## 🧪 Passo 8: Testar o Fluxo Completo

1. **Cadastro de Produto:**
   - Clique em **"Produtos"**
   - **"+ Novo Produto"**
   - Código: `P001`, Saldo: `10`
   - Clique em **"Gerar descrição com IA"** (modo offline)

2. **Criar Nota Fiscal:**
   - Vá para **"Notas Fiscais"**
   - **"+ Nova Nota"**
   - Adicione o produto P001 com quantidade `2`

3. **Imprimir Nota:**
   - Clique **"Imprimir"**
   - Status deve virar **"Fechada"**
   - Saldo do produto deve cair de `10` para `8`

4. **Teste de Idempotência:**
   - Tente imprimir novamente
   - Deve retornar erro: **"Nota não está aberta"**
   - Saldo permanece `8` (não decrementou duplicadamente)

---

## 🛑 Parar os Serviços

- **Backend (Visual Studio):** Pressione **`Shift + F5`** ou clique no ⏹️ vermelho
- **Frontend:** Em PowerShell, pressione **`Ctrl + C`**

---

## 🔧 Troubleshooting

### "Porta 5000 já está em uso"
```powershell
netstat -ano | findstr ":5000"  # Encontra processo
taskkill /PID <PID> /F          # Mata processo
```

### "Erro ao restaurar pacotes NuGet"
- Vá em **Tools → NuGet Package Manager → Manage NuGet Packages for Solution**
- Clique em **"Restore"**

### "npm: comando não encontrado"
- Reinstale Node.js: https://nodejs.org/
- Reinicie PowerShell ou abra um novo terminal

### "Visual Studio não encontra .NET 8"
- Abra **Visual Studio Installer**
- Clique em **"Modify"** → **"Individual components"**
- Procure por **.NET 8.0 SDK** e instale

---

## 📚 Referências

- `README.md` — Visão geral do projeto
- `DETALHAMENTO_TECNICO.md` — Arquitetura e tecnologias
- `ROTEIRO_VIDEO.md` — Roteiro para gravar o vídeo de demonstração

---

**Você está pronto para desenvolver! 🚀**
