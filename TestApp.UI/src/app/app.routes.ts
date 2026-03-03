import { Routes } from '@angular/router';
import { authGuard, adminGuard, loginGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [loginGuard],
    loadComponent: () => import('./pages/login/login').then(m => m.LoginPage)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/layout/layout').then(m => m.LayoutPage),
    children: [
      { path: '', redirectTo: 'decks', pathMatch: 'full' },
      {
        path: 'decks',
        loadComponent: () => import('./pages/decks/deck-list/deck-list').then(m => m.DeckListPage)
      },
      {
        path: 'decks/:id',
        loadComponent: () => import('./pages/decks/deck-detail/deck-detail').then(m => m.DeckDetailPage)
      },
      {
        path: 'decks/:deckId/files/:fileId/questions',
        loadComponent: () => import('./pages/questions/question-list/question-list').then(m => m.QuestionListPage)
      },
      {
        path: 'exam',
        loadComponent: () => import('./pages/exam/exam').then(m => m.ExamPage)
      },
      {
        path: 'decks/:deckId/statistics',
        loadComponent: () => import('./pages/statistics/deck-statistics/deck-statistics').then(m => m.DeckStatisticsPage)
      },
      {
        path: 'decks/:deckId/files/:fileId/statistics',
        loadComponent: () => import('./pages/statistics/file-statistics/file-statistics').then(m => m.FileStatisticsPage)
      },
      {
        path: 'admin',
        canActivate: [adminGuard],
        loadComponent: () => import('./pages/admin/admin').then(m => m.AdminPage)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
