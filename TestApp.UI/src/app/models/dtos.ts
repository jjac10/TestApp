export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  fullName: string;
  role: string;
  expiration: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  fullName: string;
  role: string;
}

export interface CreateDeckRequest {
  name: string;
}

export interface RecordAnswerRequest {
  userAnswer: string;
}

export interface UpdateCorrectAnswerRequest {
  newCorrectAnswer: string;
}

export interface ImportQuestionsRequest {
  fileName: string;
  questions: QuestionImportDto[];
}

export interface QuestionImportDto {
  numero: number;
  enunciado: string;
  opcionA: string;
  opcionB: string;
  opcionC: string;
  opcionD: string;
  respuestaCorrecta: string;
  fuente?: string;
}
