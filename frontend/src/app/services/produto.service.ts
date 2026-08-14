import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GerarDescricaoResponse, Produto, ProdutoCreate } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly base = `${environment.estoqueApi}/produtos`;

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.base);
  }

  criar(produto: ProdutoCreate): Observable<Produto> {
    return this.http.post<Produto>(this.base, produto);
  }

  atualizar(id: number, dados: Pick<Produto, 'descricao' | 'saldo'>): Observable<Produto> {
    return this.http.put<Produto>(`${this.base}/${id}`, dados);
  }

  gerarDescricao(codigo: string, palavrasChave?: string): Observable<GerarDescricaoResponse> {
    return this.http.post<GerarDescricaoResponse>(`${this.base}/gerar-descricao`, {
      codigo,
      palavrasChave: palavrasChave ?? null
    });
  }
}
