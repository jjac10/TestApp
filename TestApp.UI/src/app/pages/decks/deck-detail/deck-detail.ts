import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DeckService } from '../../../services/deck.service';
import { QuestionService, QuestionFilter } from '../../../services/question.service';
import { ImportService, PdfPreviewResponse } from '../../../services/import.service';
import { Deck, QuestionFile } from '../../../models/models';

@Component({
  selector: 'app-deck-detail',
  imports: [FormsModule, RouterLink],
  templateUrl: './deck-detail.html',
  styleUrl: './deck-detail.scss'
})
export class DeckDetailPage implements OnInit {
  deck = signal<Deck | null>(null);
  loading = signal(true);
  statusMessage = signal('');

  // Import
  showImportModal = signal(false);
  importFiles = signal<File[]>([]);
  importPreview = signal<PdfPreviewResponse | null>(null);
  importing = signal(false);
  importStep = signal<'select' | 'preview' | 'done'>('select');

  // Exam options
  showExamModal = signal(false);
  examFileId = signal<number | null>(null);
  examIsFileBased = signal(true);
  examQuestionCount = signal(10);
  examFilter = signal<QuestionFilter>('All');
  examRandom = signal(true);
  examRandomAnswers = signal(false);
  examReviewMode = signal(true);
  examForceRandom = signal(false);
  availableQuestionCount = signal(0);

  // Delete file
  showDeleteFileModal = signal(false);
  fileToDelete = signal<QuestionFile | null>(null);
  deletingFile = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private deckService: DeckService,
    private questionService: QuestionService,
    private importService: ImportService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadDeck(id);
  }

  loadDeck(id: number): void {
    this.loading.set(true);
    this.deckService.getById(id).subscribe({
      next: (deck) => {
        this.deck.set(deck);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/decks']);
      }
    });
  }

  getTotalQuestions(): number {
    return this.deck()?.files?.reduce((sum, f) => sum + (f.questions?.length || 0), 0) || 0;
  }

  // --- Import ---
  openImportModal(): void {
    this.importStep.set('select');
    this.importFiles.set([]);
    this.importPreview.set(null);
    this.showImportModal.set(true);
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.importFiles.set(Array.from(input.files));
    }
  }

  previewImport(): void {
    if (this.importFiles().length === 0) return;
    this.importing.set(true);

    this.importService.previewPdfs(this.importFiles()).subscribe({
      next: (preview) => {
        this.importPreview.set(preview);
        this.importStep.set('preview');
        this.importing.set(false);
      },
      error: (err) => {
        this.showStatus('Error al procesar los archivos: ' + (err.error?.message || err.message));
        this.importing.set(false);
      }
    });
  }

  confirmImport(): void {
    const preview = this.importPreview();
    const deck = this.deck();
    if (!preview || !deck) return;

    this.importing.set(true);
    this.importService.confirmImport(deck.id, preview.sessionId).subscribe({
      next: () => {
        this.importStep.set('done');
        this.importing.set(false);
        this.showImportModal.set(false);
        this.showStatus('Importación completada correctamente');
        this.loadDeck(deck.id);
      },
      error: (err) => {
        this.showStatus('Error en la importación: ' + (err.error?.message || err.message));
        this.importing.set(false);
      }
    });
  }

  cancelImport(): void {
    const preview = this.importPreview();
    if (preview) {
      this.importService.cancelPreview(preview.sessionId).subscribe();
    }
    this.showImportModal.set(false);
  }

  // --- Exam ---
  openFileExam(file: QuestionFile): void {
    this.examFileId.set(file.id);
    this.examIsFileBased.set(true);
    this.examQuestionCount.set(file.questions?.length || 10);
    this.examFilter.set('All');
    this.examRandom.set(false);
    this.examRandomAnswers.set(false);
    this.examReviewMode.set(true);
    this.examForceRandom.set(false);
    this.updateAvailableCount();
    this.showExamModal.set(true);
  }

  openDeckExam(): void {
    this.examIsFileBased.set(false);
    this.examFileId.set(null);
    this.examQuestionCount.set(20);
    this.examFilter.set('All');
    this.examRandom.set(true);
    this.examRandomAnswers.set(true);
    this.examReviewMode.set(false);
    this.examForceRandom.set(true);
    this.updateAvailableCount();
    this.showExamModal.set(true);
  }

  updateAvailableCount(): void {
    if (this.examIsFileBased() && this.examFileId()) {
      this.questionService.countInFile(this.examFileId()!, this.examFilter()).subscribe({
        next: (count) => this.availableQuestionCount.set(count)
      });
    } else if (this.deck()) {
      this.questionService.countInDeck(this.deck()!.id, this.examFilter()).subscribe({
        next: (count) => this.availableQuestionCount.set(count)
      });
    }
  }

  onFilterChange(filter: string): void {
    this.examFilter.set(filter as QuestionFilter);
    this.updateAvailableCount();
  }

  startExam(): void {
    this.showExamModal.set(false);
    const params: any = {
      filter: this.examFilter(),
      count: this.examQuestionCount(),
      random: this.examRandom(),
      randomAnswers: this.examRandomAnswers(),
      reviewMode: this.examReviewMode()
    };

    params.deckId = this.deck()!.id;
    if (this.examIsFileBased()) {
      params.fileId = this.examFileId();
    }

    this.router.navigate(['/exam'], { queryParams: params });
  }

  // --- Delete File ---
  openDeleteFile(file: QuestionFile, event: Event): void {
    event.stopPropagation();
    this.fileToDelete.set(file);
    this.showDeleteFileModal.set(true);
  }

  confirmDeleteFile(): void {
    const file = this.fileToDelete();
    if (!file) return;

    this.deletingFile.set(true);
    this.questionService.deleteFile(file.id).subscribe({
      next: () => {
        this.showDeleteFileModal.set(false);
        this.fileToDelete.set(null);
        this.deletingFile.set(false);
        this.showStatus('Tema eliminado correctamente');
        this.loadDeck(this.deck()!.id);
      },
      error: () => {
        this.deletingFile.set(false);
      }
    });
  }

  cancelDeleteFile(): void {
    this.showDeleteFileModal.set(false);
    this.fileToDelete.set(null);
  }

  showStatus(msg: string): void {
    this.statusMessage.set(msg);
    setTimeout(() => this.statusMessage.set(''), 4000);
  }
}
