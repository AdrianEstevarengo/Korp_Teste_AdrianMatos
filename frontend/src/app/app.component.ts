import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary">
      <mat-icon>receipt_long</mat-icon>
      <span style="margin-left:8px">KORP — Notas Fiscais</span>
      <span class="spacer"></span>
      <a mat-button routerLink="/produtos" routerLinkActive="mat-mdc-raised-button">Produtos</a>
      <a mat-button routerLink="/notas" routerLinkActive="mat-mdc-raised-button">Notas Fiscais</a>
    </mat-toolbar>

    <div class="container">
      <router-outlet></router-outlet>
    </div>
  `
})
export class AppComponent {}
