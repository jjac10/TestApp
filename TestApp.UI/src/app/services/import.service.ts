import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PdfPreviewItem {
  originalFileName: string;
  storedFileName: string;
  questionCount: number;
  isValid: boolean;
  error?: string;
}

export interface PdfPreviewResponse {
  sessionId: string;
  totalFiles: number;
  validFiles: number;
  totalQuestions: number;
  files: PdfPreviewItem[];
}

export interface PdfImportResult {
  fileName: string;
  success: boolean;
  importedCount: number;
  error?: string;
}

@Injectable({ providedIn: 'root' })
export class ImportService {
  private readonly apiUrl = `${environment.apiUrl}/import`;

  constructor(private http: HttpClient) {}

  uploadPdf(deckId: number, file: File): Observable<PdfImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<PdfImportResult>(`${this.apiUrl}/pdf/${deckId}`, formData);
  }

  previewPdfs(files: File[]): Observable<PdfPreviewResponse> {
    const formData = new FormData();
    files.forEach(f => formData.append('files', f));
    return this.http.post<PdfPreviewResponse>(`${this.apiUrl}/pdf/preview`, formData);
  }

  confirmImport(deckId: number, sessionId: string): Observable<PdfImportResult[]> {
    return this.http.post<PdfImportResult[]>(`${this.apiUrl}/pdf/${deckId}/confirm/${sessionId}`, {});
  }

  cancelPreview(sessionId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/pdf/preview/${sessionId}`);
  }
}
