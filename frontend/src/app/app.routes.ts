import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  {
    path: 'produtos',
    loadComponent: () =>
      import('./pages/produtos/produtos.component').then((m) => m.ProdutosComponent)
  },
  {
    path: 'notas',
    loadComponent: () =>
      import('./pages/notas/notas.component').then((m) => m.NotasComponent)
  },
  { path: '**', redirectTo: 'produtos' }
];
