import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { Produto } from '../../models/produto.model';
import { ProdutoService } from '../../services/produto.service';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatTableModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  template: `
    <mat-card class="card">
      <mat-card-header><mat-card-title>Cadastro de Produtos</mat-card-title></mat-card-header>
      <mat-card-content>
        <div class="row">
          <mat-form-field appearance="outline">
            <mat-label>Código</mat-label>
            <input matInput [(ngModel)]="codigo" placeholder="Ex.: P001">
          </mat-form-field>

          <mat-form-field appearance="outline" style="flex:1; min-width:240px">
            <mat-label>Descrição</mat-label>
            <input matInput [(ngModel)]="descricao" placeholder="Nome do produto">
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Saldo</mat-label>
            <input matInput type="number" min="0" [(ngModel)]="saldo">
          </mat-form-field>
        </div>

        <div class="acoes">
          <button mat-stroked-button color="accent" (click)="gerarDescricao()" [disabled]="gerandoIa || !codigo">
            <mat-icon>auto_awesome</mat-icon>
            Gerar descrição com IA
          </button>
          <mat-spinner *ngIf="gerandoIa" diameter="22"></mat-spinner>

          <span class="spacer"></span>

          <button mat-raised-button color="primary" (click)="salvar()" [disabled]="salvando || !podeSalvar()">
            <mat-icon>save</mat-icon>
            Salvar produto
          </button>
        </div>
      </mat-card-content>
    </mat-card>

    <mat-card class="card">
      <mat-card-header><mat-card-title>Produtos cadastrados</mat-card-title></mat-card-header>
      <mat-card-content>
        <div *ngIf="carregando" class="acoes"><mat-spinner diameter="24"></mat-spinner> Carregando...</div>

        <table mat-table [dataSource]="produtos" *ngIf="!carregando">
          <ng-container matColumnDef="codigo">
            <th mat-header-cell *matHeaderCellDef>Código</th>
            <td mat-cell *matCellDef="let p">{{ p.codigo }}</td>
          </ng-container>
          <ng-container matColumnDef="descricao">
            <th mat-header-cell *matHeaderCellDef>Descrição</th>
            <td mat-cell *matCellDef="let p">{{ p.descricao }}</td>
          </ng-container>
          <ng-container matColumnDef="saldo">
            <th mat-header-cell *matHeaderCellDef>Saldo</th>
            <td mat-cell *matCellDef="let p">{{ p.saldo }}</td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="colunas"></tr>
          <tr mat-row *matRowDef="let row; columns: colunas;"></tr>
        </table>

        <p *ngIf="!carregando && produtos.length === 0">Nenhum produto cadastrado ainda.</p>
      </mat-card-content>
    </mat-card>
  `
})
export class ProdutosComponent implements OnInit, OnDestroy {
  // takeUntil + Subject: cancela as assinaturas quando o componente é destruído.
  private readonly destruir$ = new Subject<void>();

  codigo = '';
  descricao = '';
  saldo = 0;

  produtos: Produto[] = [];
  colunas = ['codigo', 'descricao', 'saldo'];

  carregando = false;
  salvando = false;
  gerandoIa = false;

  constructor(private readonly service: ProdutoService, private readonly snack: MatSnackBar) {}

  ngOnInit(): void {
    this.carregar();
  }

  ngOnDestroy(): void {
    this.destruir$.next();
    this.destruir$.complete();
  }

  podeSalvar(): boolean {
    return !!this.codigo.trim() && !!this.descricao.trim() && this.saldo >= 0;
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar().pipe(takeUntil(this.destruir$)).subscribe({
      next: (lista) => { this.produtos = lista; this.carregando = false; },
      error: () => { this.carregando = false; this.erro('Não foi possível carregar os produtos.'); }
    });
  }

  gerarDescricao(): void {
    this.gerandoIa = true;
    this.service.gerarDescricao(this.codigo, this.descricao).pipe(takeUntil(this.destruir$)).subscribe({
      next: (r) => {
        this.descricao = r.descricao;
        this.gerandoIa = false;
        this.snack.open(r.geradoPorIa ? 'Descrição gerada por IA.' : 'Descrição sugerida (modo offline).', 'OK', { duration: 3000 });
      },
      error: () => { this.gerandoIa = false; this.erro('Falha ao gerar a descrição.'); }
    });
  }

  salvar(): void {
    this.salvando = true;
    this.service.criar({ codigo: this.codigo.trim(), descricao: this.descricao.trim(), saldo: this.saldo })
      .pipe(takeUntil(this.destruir$))
      .subscribe({
        next: () => {
          this.salvando = false;
          this.snack.open('Produto cadastrado com sucesso.', 'OK', { duration: 3000 });
          this.codigo = ''; this.descricao = ''; this.saldo = 0;
          this.carregar();
        },
        error: (e) => {
          this.salvando = false;
          this.erro(e?.error?.detalhe ?? 'Não foi possível cadastrar o produto.');
        }
      });
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 5000, panelClass: 'erro' });
  }
}
