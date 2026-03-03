import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Deck } from '../models/models';
import { CreateDeckRequest } from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class DeckService {
  private readonly apiUrl = `${environment.apiUrl}/decks`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Deck[]> {
    return this.http.get<Deck[]>(this.apiUrl);
  }

  getById(id: number): Observable<Deck> {
    return this.http.get<Deck>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateDeckRequest): Observable<Deck> {
    return this.http.post<Deck>(this.apiUrl, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
