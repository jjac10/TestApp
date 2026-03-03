import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginPage {
  email = signal('');
  password = signal('');
  loading = signal(false);
  error = signal('');

  constructor(private authService: AuthService, private router: Router) {}

  login(): void {
    if (!this.email() || !this.password()) {
      this.error.set('Introduce email y contraseña');
      return;
    }
    this.loading.set(true);
    this.error.set('');

    this.authService.login({ email: this.email(), password: this.password() }).subscribe({
      next: (res) => {
        this.authService.saveSession(res);
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 0) {
          this.error.set('No se pudo conectar con el servidor. Inténtalo más tarde.');
        } else {
          this.error.set(err.error?.message || (typeof err.error === 'string' ? err.error : 'Credenciales incorrectas'));
        }
      }
    });
  }
}
