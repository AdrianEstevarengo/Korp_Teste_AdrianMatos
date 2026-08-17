// URLs das APIs (portas expostas pelos containers no host).
// O navegador roda no host, então "localhost" aponta para as portas mapeadas.
export const environment = {
  producao: false,
  estoqueApi: 'http://localhost:5000/api',
  faturamentoApi: 'http://localhost:5002/api'
};
