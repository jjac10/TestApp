import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Question } from '../models/models';
import { RecordAnswerRequest, UpdateCorrectAnswerRequest, ImportQuestionsRequest } from '../models/dtos';

export type QuestionFilter = 'All' | 'NeverAnswered' | 'Failed';

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private readonly apiUrl = `${environment.apiUrl}/questions`;

  constructor(private http: HttpClient) {}

  getExamFromDeck(deckId: number, count: number = 10, filter: QuestionFilter = 'All', random: boolean = true, randomAnswers: boolean = false): Observable<Question[]> {
    const params = new HttpParams()
      .set('count', count)
      .set('filter', filter)
      .set('random', random)
      .set('randomAnswers', randomAnswers);
    return this.http.get<Question[]>(`${this.apiUrl}/deck/${deckId}/exam`, { params });
  }

  getExamFromFile(fileId: number, count: number = 10, filter: QuestionFilter = 'All', random: boolean = true, randomAnswers: boolean = false): Observable<Question[]> {
    const params = new HttpParams()
      .set('count', count)
      .set('filter', filter)
      .set('random', random)
      .set('randomAnswers', randomAnswers);
    return this.http.get<Question[]>(`${this.apiUrl}/file/${fileId}/exam`, { params });
  }

  getAllFromFile(fileId: number): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.apiUrl}/file/${fileId}`);
  }

  countInFile(fileId: number, filter: QuestionFilter = 'All'): Observable<number> {
    const params = new HttpParams().set('filter', filter);
    return this.http.get<number>(`${this.apiUrl}/file/${fileId}/count`, { params });
  }

  countInDeck(deckId: number, filter: QuestionFilter = 'All'): Observable<number> {
    const params = new HttpParams().set('filter', filter);
    return this.http.get<number>(`${this.apiUrl}/deck/${deckId}/count`, { params });
  }

  recordAnswer(questionId: number, request: RecordAnswerRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${questionId}/answer`, request);
  }

  updateCorrectAnswer(questionId: number, request: UpdateCorrectAnswerRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${questionId}/correct-answer`, request);
  }

  deleteFile(fileId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/file/${fileId}`);
  }

  deleteQuestion(questionId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/question/${questionId}`);
  }

  importQuestions(deckId: number, request: ImportQuestionsRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/deck/${deckId}/import`, request);
  }
}
