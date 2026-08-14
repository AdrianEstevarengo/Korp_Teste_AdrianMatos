export interface ItemNota {
  produtoId: number;
  codigo: string;
  descricao: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: number;
  numero: number;
  status: 'Aberta' | 'Fechada';
  dataCriacao: string;
  dataImpressao?: string | null;
  itens: ItemNota[];
}

export interface CriarNota {
  itens: ItemNota[];
}
