export interface User {
  id: string;
  email: string;
  fullName: string;
}

export interface Deck {
  id: number;
  name: string;
  createdAt: string;
  userId: string;
  files: QuestionFile[];
}

export interface QuestionFile {
  id: number;
  name: string;
  importedAt: string;
  deckId: number;
  questions: Question[];
}

export interface Question {
  id: number;
  number: number;
  statement: string;
  optionA: string;
  optionB: string;
  optionC: string;
  optionD: string;
  correctAnswer: string;
  source?: string;
  fileId: number;
  answers: Answer[];
  timesAnswered: number;
  timesCorrect: number;
  timesFailed: number;
  neverAnswered: boolean;
}

export interface Answer {
  id: number;
  userAnswer: string;
  isCorrect: boolean;
  answeredAt: string;
  questionId: number;
}
