import { Component, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { QuestionService, QuestionFilter } from '../../services/question.service';
import { Question } from '../../models/models';
import { DecimalPipe } from '@angular/common';

interface ShuffledQuestion {
  original: Question;
  displayOptions: { letter: string; text: string; originalLetter: string }[];
}

interface AnswerRecord {
  question: Question;
  userAnswer: string;       // original letter
  displayAnswer: string;    // display letter
  isCorrect: boolean;
}

@Component({
  selector: 'app-exam',
  imports: [DecimalPipe],
  templateUrl: './exam.html',
  styleUrl: './exam.scss'
})
export class ExamPage implements OnInit {
  // Config from route params
  fileId: number | null = null;
  deckId: number | null = null;
  filter: QuestionFilter = 'All';
  count = 10;
  random = true;
  randomAnswers = false;
  reviewMode = true;

  // State
  questions = signal<ShuffledQuestion[]>([]);
  currentIndex = signal(0);
  loading = signal(true);
  finished = signal(false);
  answers = signal<AnswerRecord[]>([]);
  selectedAnswer = signal<string | null>(null);
  answered = signal(false);
  showingResult = signal(false);
  showReview = signal(false);

  // Computed
  currentQuestion = computed(() => this.questions()[this.currentIndex()]);
  progress = computed(() => {
    const total = this.questions().length;
    return total > 0 ? ((this.currentIndex() + 1) / total) * 100 : 0;
  });
  correctCount = computed(() => this.answers().filter(a => a.isCorrect).length);
  incorrectCount = computed(() => this.answers().filter(a => !a.isCorrect).length);
  score = computed(() => {
    const total = this.answers().length;
    return total > 0 ? (this.correctCount() / total) * 100 : 0;
  });
  failedQuestions = computed(() => this.answers().filter(a => !a.isCorrect));

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private questionService: QuestionService
  ) {}

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    this.fileId = params['fileId'] ? Number(params['fileId']) : null;
    this.deckId = params['deckId'] ? Number(params['deckId']) : null;
    this.filter = (params['filter'] as QuestionFilter) || 'All';
    this.count = Number(params['count']) || 10;
    this.random = params['random'] !== 'false';
    this.randomAnswers = params['randomAnswers'] === 'true';
    this.reviewMode = params['reviewMode'] !== 'false';

    this.loadExam();
  }

  loadExam(): void {
    this.loading.set(true);
    this.answers.set([]);
    this.currentIndex.set(0);
    this.finished.set(false);
    this.selectedAnswer.set(null);
    this.answered.set(false);
    this.showingResult.set(false);
    this.showReview.set(false);

    const obs = this.fileId
      ? this.questionService.getExamFromFile(this.fileId, this.count, this.filter, this.random, this.randomAnswers)
      : this.questionService.getExamFromDeck(this.deckId!, this.count, this.filter, this.random, this.randomAnswers);

    obs.subscribe({
      next: (questions) => {
        const mapped = questions.map(q => this.mapQuestion(q));
        this.questions.set(mapped);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/decks']);
      }
    });
  }

  private mapQuestion(q: Question): ShuffledQuestion {
    // The backend already handles answer shuffling if randomAnswers is enabled.
    // We just map the options as they come from the API.
    const options = [
      { letter: 'A', text: q.optionA, originalLetter: 'A' },
      { letter: 'B', text: q.optionB, originalLetter: 'B' },
      { letter: 'C', text: q.optionC, originalLetter: 'C' },
      { letter: 'D', text: q.optionD, originalLetter: 'D' }
    ];

    return { original: q, displayOptions: options };
  }

  selectAnswer(displayLetter: string): void {
    if (this.answered()) return;
    this.selectedAnswer.set(displayLetter);
  }

  confirmAnswer(): void {
    if (!this.selectedAnswer() || this.answered()) return;

    const q = this.currentQuestion();
    const displayLetter = this.selectedAnswer()!;
    const selectedOption = q.displayOptions.find(o => o.letter === displayLetter);
    if (!selectedOption) return;

    const originalLetter = selectedOption.originalLetter;
    const isCorrect = originalLetter.toUpperCase() === q.original.correctAnswer.toUpperCase();

    // Record answer in backend
    this.questionService.recordAnswer(q.original.id, { userAnswer: originalLetter }).subscribe();

    const record: AnswerRecord = {
      question: q.original,
      userAnswer: originalLetter,
      displayAnswer: displayLetter,
      isCorrect
    };

    this.answers.update(arr => [...arr, record]);
    this.answered.set(true);

    if (this.reviewMode) {
      this.showingResult.set(true);
    } else {
      // Auto-advance
      setTimeout(() => this.nextQuestion(), 300);
    }
  }

  nextQuestion(): void {
    if (this.currentIndex() < this.questions().length - 1) {
      this.currentIndex.update(i => i + 1);
      this.selectedAnswer.set(null);
      this.answered.set(false);
      this.showingResult.set(false);
    } else {
      this.finished.set(true);
    }
  }

  previousQuestion(): void {
    // Only in review mode when showing result
    if (this.currentIndex() > 0) {
      this.currentIndex.update(i => i - 1);
      this.selectedAnswer.set(null);
      this.answered.set(true);
      this.showingResult.set(true);
    }
  }

  getOptionClass(q: ShuffledQuestion, opt: { letter: string; originalLetter: string }): string {
    if (!this.answered()) {
      return this.selectedAnswer() === opt.letter ? 'selected' : '';
    }

    const isCorrectOption = opt.originalLetter.toUpperCase() === q.original.correctAnswer.toUpperCase();
    const isSelected = this.selectedAnswer() === opt.letter;

    if (isCorrectOption) return 'correct';
    if (isSelected && !isCorrectOption) return 'incorrect';
    return '';
  }

  toggleReview(): void {
    this.showReview.set(!this.showReview());
  }

  retryFailed(): void {
    const failed = this.failedQuestions();
    if (failed.length === 0) return;

    // Restart exam with only failed questions
    const failedQuestions = failed.map(a => this.mapQuestion(a.question));
    this.questions.set(failedQuestions);
    this.currentIndex.set(0);
    this.answers.set([]);
    this.finished.set(false);
    this.selectedAnswer.set(null);
    this.answered.set(false);
    this.showingResult.set(false);
    this.showReview.set(false);
  }

  goHome(): void {
    this.router.navigate(['/decks', this.deckId]);
  }

  getCorrectDisplayLetter(q: ShuffledQuestion): string {
    const correctOpt = q.displayOptions.find(
      o => o.originalLetter.toUpperCase() === q.original.correctAnswer.toUpperCase()
    );
    return correctOpt?.letter || q.original.correctAnswer;
  }
}
