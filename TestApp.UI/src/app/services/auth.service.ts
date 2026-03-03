import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, CreateUserRequest } from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly adminUrl = `${environment.apiUrl}/admin`;

  private readonly _token = signal<string | null>(localStorage.getItem('token'));
  private readonly _email = signal<string | null>(localStorage.getItem('email'));
  private readonly _fullName = signal<string | null>(localStorage.getItem('fullName'));
  private readonly _role = signal<string | null>(localStorage.getItem('role'));

  readonly isLoggedIn = computed(() => !!this._token());
  readonly isAdmin = computed(() => this._role() === 'Admin');
  readonly email = computed(() => this._email());
  readonly fullName = computed(() => this._fullName());
  readonly role = computed(() => this._role());
  readonly token = computed(() => this._token());

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest) {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request);
  }

  register(request: RegisterRequest) {
    return this.http.post(`${this.apiUrl}/register`, request);
  }

  createUser(request: CreateUserRequest) {
    return this.http.post(`${this.adminUrl}/users`, request);
  }

  saveSession(response: LoginResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('email', response.email);
    localStorage.setItem('fullName', response.fullName);
    localStorage.setItem('role', response.role);

    this._token.set(response.token);
    this._email.set(response.email);
    this._fullName.set(response.fullName);
    this._role.set(response.role);
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('email');
    localStorage.removeItem('fullName');
    localStorage.removeItem('role');

    this._token.set(null);
    this._email.set(null);
    this._fullName.set(null);
    this._role.set(null);

    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this._token();
  }
}
