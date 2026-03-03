import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuestionService } from '../../../services/question.service';
import { Question } from '../../../models/models';

@Component({
  selector: 'app-question-list',
  imports: [FormsModule, RouterLink],
  templateUrl: './question-list.html',
  styleUrl: './question-list.scss'
})
export class QuestionListPage implements OnInit {
  deckId = 0;
  fileId = 0;
  questions = signal<Question[]>([]);
  loading = signal(true);
  editMode = signal(false);
  statusMessage = signal('');

  // Delete question
  showDeleteModal = signal(false);
  questionToDelete = signal<Question | null>(null);
  deleting = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private questionService: QuestionService
  ) {}

  ngOnInit(): void {
    this.deckId = Number(this.route.snapshot.paramMap.get('deckId'));
    this.fileId = Number(this.route.snapshot.paramMap.get('fileId'));
    this.loadQuestions();
  }

  loadQuestions(): void {
    this.loading.set(true);
    this.questionService.getAllFromFile(this.fileId).subscribe({
      next: (questions) => {
        this.questions.set(questions);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  toggleEditMode(): void {
    this.editMode.set(!this.editMode());
  }

  updateCorrectAnswer(question: Question, newAnswer: string): void {
    this.questionService.updateCorrectAnswer(question.id, { newCorrectAnswer: newAnswer }).subscribe({
      next: () => {
        question.correctAnswer = newAnswer;
        this.showStatus('Respuesta correcta actualizada');
      },
      error: () => {
        this.showStatus('Error al actualizar');
      }
    });
  }

  openDeleteQuestion(question: Question): void {
    this.questionToDelete.set(question);
    this.showDeleteModal.set(true);
  }

  confirmDeleteQuestion(): void {
    const q = this.questionToDelete();
    if (!q) return;

    this.deleting.set(true);
    this.questionService.deleteQuestion(q.id).subscribe({
      next: () => {
        this.showDeleteModal.set(false);
        this.questionToDelete.set(null);
        this.deleting.set(false);
        this.showStatus('Pregunta eliminada');
        this.loadQuestions();
      },
      error: () => {
        this.deleting.set(false);
      }
    });
  }

  cancelDeleteQuestion(): void {
    this.showDeleteModal.set(false);
    this.questionToDelete.set(null);
  }

  getAnswerClass(question: Question, letter: string): string {
    if (question.correctAnswer.toUpperCase() === letter.toUpperCase()) {
      return 'correct';
    }
    return '';
  }

  showStatus(msg: string): void {
    this.statusMessage.set(msg);
    setTimeout(() => this.statusMessage.set(''), 3000);
  }
}
