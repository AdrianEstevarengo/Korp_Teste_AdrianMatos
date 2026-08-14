import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CriarNota, NotaFiscal } from '../models/nota.model';

@Injectable({ providedIn: 'root' })
export class NotaService {
  private readonly base = `${environment.faturamentoApi}/notas`;

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.base);
  }

  criar(nota: CriarNota): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.base, nota);
  }

  imprimir(id: number): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(`${this.base}/${id}/imprimir`, {});
  }
}
