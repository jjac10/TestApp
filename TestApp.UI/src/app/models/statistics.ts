export interface FileStatistics {
  fileId: number;
  fileName: string;
  totalQuestions: number;
  totalAttempts: number;
  correctAttempts: number;
  incorrectAttempts: number;
  successRate: number;
  questionsNeverAnswered: number;
  questionsAlwaysCorrect: number;
  questionsAlwaysWrong: number;
  mostFailedQuestions: QuestionStatistics[];
}

export interface DeckStatistics {
  deckId: number;
  deckName: string;
  totalFiles: number;
  totalQuestions: number;
  totalAttempts: number;
  correctAttempts: number;
  incorrectAttempts: number;
  successRate: number;
  filesSummary: FileStatisticsSummary[];
}

export interface FileStatisticsSummary {
  fileId: number;
  fileName: string;
  totalQuestions: number;
  totalAttempts: number;
  correctAttempts: number;
  successRate: number;
}

export interface QuestionStatistics {
  questionId: number;
  questionNumber: number;
  statement: string;
  timesAnswered: number;
  timesCorrect: number;
  timesFailed: number;
  successRate: number;
  failureRate: number;
}

export interface ProgressDataPoint {
  date: string;
  totalAttempts: number;
  correctAttempts: number;
  successRate: number;
  dateLabel: string;
}

export interface ProgressHistory {
  dailyProgress: ProgressDataPoint[];
  totalDaysStudied: number;
  firstStudyDate?: string;
  lastStudyDate?: string;
  overallTrend: number;
}
