import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, forkJoin, takeUntil } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { Produto } from '../../models/produto.model';
import { ItemNota, NotaFiscal } from '../../models/nota.model';
import { ProdutoService } from '../../services/produto.service';
import { NotaService } from '../../services/nota.service';

@Component({
  selector: 'app-notas',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatTableModule, MatListModule,
    MatProgressSpinnerModule, MatSnackBarModule
  ],
  template: `
    <mat-card class="card">
      <mat-card-header><mat-card-title>Nova Nota Fiscal</mat-card-title></mat-card-header>
      <mat-card-content>
        <div class="row">
          <mat-form-field appearance="outline" style="flex:1; min-width:260px">
            <mat-label>Produto</mat-label>
            <mat-select [(ngModel)]="produtoSelecionadoId">
              <mat-option *ngFor="let p of produtos" [value]="p.id">
                {{ p.codigo }} — {{ p.descricao }} (saldo: {{ p.saldo }})
              </mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Quantidade</mat-label>
            <input matInput type="number" min="1" [(ngModel)]="quantidade">
          </mat-form-field>

          <button mat-stroked-button color="primary" (click)="adicionarItem()"
                  [disabled]="!produtoSelecionadoId || quantidade < 1">
            <mat-icon>add</mat-icon> Adicionar
          </button>
        </div>

        <mat-list *ngIf="itens.length > 0">
          <div mat-subheader>Itens da nota</div>
          <mat-list-item *ngFor="let it of itens; let i = index">
            <span>{{ it.codigo }} — {{ it.descricao }} &times; {{ it.quantidade }}</span>
            <span class="spacer"></span>
            <button mat-icon-button color="warn" (click)="removerItem(i)"><mat-icon>delete</mat-icon></button>
          </mat-list-item>
        </mat-list>

        <div class="acoes">
          <button mat-raised-button color="primary" (click)="criarNota()"
                  [disabled]="criando || itens.length === 0">
            <mat-icon>note_add</mat-icon> Criar nota (Aberta)
          </button>
          <mat-spinner *ngIf="criando" diameter="22"></mat-spinner>
        </div>
      </mat-card-content>
    </mat-card>

    <mat-card class="card">
      <mat-card-header><mat-card-title>Notas Fiscais</mat-card-title></mat-card-header>
      <mat-card-content>
        <div *ngIf="carregando" class="acoes"><mat-spinner diameter="24"></mat-spinner> Carregando...</div>

        <table mat-table [dataSource]="notas" *ngIf="!carregando">
          <ng-container matColumnDef="numero">
            <th mat-header-cell *matHeaderCellDef>Nº</th>
            <td mat-cell *matCellDef="let n">{{ n.numero }}</td>
          </ng-container>
          <ng-container matColumnDef="itens">
            <th mat-header-cell *matHeaderCellDef>Itens</th>
            <td mat-cell *matCellDef="let n">
              <span *ngFor="let it of n.itens; let last = last">
                {{ it.codigo }}&times;{{ it.quantidade }}<span *ngIf="!last">, </span>
              </span>
            </td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let n">
              <span [class.status-aberta]="n.status === 'Aberta'"
                    [class.status-fechada]="n.status === 'Fechada'">{{ n.status }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef>Ações</th>
            <td mat-cell *matCellDef="let n">
              <div class="acoes">
                <button mat-raised-button color="accent"
                        (click)="imprimir(n)"
                        [disabled]="n.status !== 'Aberta' || estaImprimindo(n.id)">
                  <mat-icon>print</mat-icon> Imprimir
                </button>
                <mat-spinner *ngIf="estaImprimindo(n.id)" diameter="22"></mat-spinner>
              </div>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="colunas"></tr>
          <tr mat-row *matRowDef="let row; columns: colunas;"></tr>
        </table>

        <p *ngIf="!carregando && notas.length === 0">Nenhuma nota fiscal cadastrada ainda.</p>
      </mat-card-content>
    </mat-card>
  `
})
export class NotasComponent implements OnInit, OnDestroy {
  private readonly destruir$ = new Subject<void>();

  produtos: Produto[] = [];
  notas: NotaFiscal[] = [];
  colunas = ['numero', 'itens', 'status', 'acoes'];

  produtoSelecionadoId: number | null = null;
  quantidade = 1;
  itens: ItemNota[] = [];

  carregando = false;
  criando = false;
  private imprimindoIds = new Set<number>();

  constructor(
    private readonly produtoService: ProdutoService,
    private readonly notaService: NotaService,
    private readonly snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.carregando = true;
    // forkJoin (RxJS): aguarda produtos e notas em paralelo antes de renderizar.
    forkJoin({
      produtos: this.produtoService.listar(),
      notas: this.notaService.listar()
    }).pipe(takeUntil(this.destruir$)).subscribe({
      next: ({ produtos, notas }) => { this.produtos = produtos; this.notas = notas; this.carregando = false; },
      error: () => { this.carregando = false; this.erro('Não foi possível carregar os dados.'); }
    });
  }

  ngOnDestroy(): void {
    this.destruir$.next();
    this.destruir$.complete();
  }

  adicionarItem(): void {
    const produto = this.produtos.find((p) => p.id === this.produtoSelecionadoId);
    if (!produto) { return; }

    const existente = this.itens.find((i) => i.produtoId === produto.id);
    if (existente) {
      existente.quantidade += this.quantidade;
    } else {
      this.itens = [...this.itens, {
        produtoId: produto.id,
        codigo: produto.codigo,
        descricao: produto.descricao,
        quantidade: this.quantidade
      }];
    }
    this.produtoSelecionadoId = null;
    this.quantidade = 1;
  }

  removerItem(indice: number): void {
    this.itens = this.itens.filter((_, i) => i !== indice);
  }

  criarNota(): void {
    this.criando = true;
    this.notaService.criar({ itens: this.itens }).pipe(takeUntil(this.destruir$)).subscribe({
      next: () => {
        this.criando = false;
        this.itens = [];
        this.snack.open('Nota criada com status Aberta.', 'OK', { duration: 3000 });
        this.recarregarNotas();
      },
      error: (e) => { this.criando = false; this.erro(e?.error?.detalhe ?? 'Falha ao criar a nota.'); }
    });
  }

  imprimir(nota: NotaFiscal): void {
    this.imprimindoIds.add(nota.id);
    this.notaService.imprimir(nota.id).pipe(takeUntil(this.destruir$)).subscribe({
      next: (atualizada) => {
        this.imprimindoIds.delete(nota.id);
        this.snack.open(`Nota ${atualizada.numero} impressa e fechada. Estoque atualizado.`, 'OK', { duration: 3500 });
        this.recarregarNotas();
        this.recarregarProdutos();
      },
      error: (e) => {
        this.imprimindoIds.delete(nota.id);
        // 503 = Estoque indisponível; 409 = regra de negócio (ex.: saldo insuficiente).
        const msg = e?.error?.detalhe
          ?? (e?.status === 0 ? 'Serviço de Faturamento indisponível.' : 'Não foi possível imprimir a nota.');
        this.erro(msg);
        this.recarregarNotas();
      }
    });
  }

  estaImprimindo(id: number): boolean {
    return this.imprimindoIds.has(id);
  }

  private recarregarNotas(): void {
    this.notaService.listar().pipe(takeUntil(this.destruir$)).subscribe((n) => (this.notas = n));
  }

  private recarregarProdutos(): void {
    this.produtoService.listar().pipe(takeUntil(this.destruir$)).subscribe((p) => (this.produtos = p));
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 6000, panelClass: 'erro' });
  }
}
