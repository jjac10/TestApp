import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { FileStatistics, DeckStatistics, QuestionStatistics, ProgressHistory } from '../models/statistics';

@Injectable({ providedIn: 'root' })
export class StatisticsService {
  private readonly apiUrl = `${environment.apiUrl}/statistics`;

  constructor(private http: HttpClient) {}

  getFileStatistics(fileId: number): Observable<FileStatistics> {
    return this.http.get<FileStatistics>(`${this.apiUrl}/file/${fileId}`);
  }

  getDeckStatistics(deckId: number): Observable<DeckStatistics> {
    return this.http.get<DeckStatistics>(`${this.apiUrl}/deck/${deckId}`);
  }

  getMostFailed(fileId: number, count: number = 5): Observable<QuestionStatistics[]> {
    const params = new HttpParams().set('count', count);
    return this.http.get<QuestionStatistics[]>(`${this.apiUrl}/file/${fileId}/most-failed`, { params });
  }

  getQuestionStatistics(questionId: number): Observable<QuestionStatistics> {
    return this.http.get<QuestionStatistics>(`${this.apiUrl}/question/${questionId}`);
  }

  getFileProgress(fileId: number, days: number = 30): Observable<ProgressHistory> {
    const params = new HttpParams().set('days', days);
    return this.http.get<ProgressHistory>(`${this.apiUrl}/file/${fileId}/progress`, { params });
  }

  getDeckProgress(deckId: number, days: number = 30): Observable<ProgressHistory> {
    const params = new HttpParams().set('days', days);
    return this.http.get<ProgressHistory>(`${this.apiUrl}/deck/${deckId}/progress`, { params });
  }
}
