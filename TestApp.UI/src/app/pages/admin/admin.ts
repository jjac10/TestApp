import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin',
  imports: [FormsModule],
  templateUrl: './admin.html',
  styleUrl: './admin.scss'
})
export class AdminPage {
  email = signal('');
  password = signal('');
  fullName = signal('');
  role = signal('User');
  creating = signal(false);
  message = signal('');
  messageType = signal<'success' | 'error'>('success');

  constructor(private authService: AuthService) {}

  createUser(): void {
    if (!this.email() || !this.password() || !this.fullName()) {
      this.showMessage('Todos los campos son obligatorios', 'error');
      return;
    }

    this.creating.set(true);
    this.authService.createUser({
      email: this.email(),
      password: this.password(),
      fullName: this.fullName(),
      role: this.role()
    }).subscribe({
      next: () => {
        this.showMessage(`Usuario ${this.email()} creado correctamente`, 'success');
        this.email.set('');
        this.password.set('');
        this.fullName.set('');
        this.role.set('User');
        this.creating.set(false);
      },
      error: (err) => {
        const errorMsg = err.error?.message || err.error || 'Error al crear el usuario';
        this.showMessage(typeof errorMsg === 'string' ? errorMsg : JSON.stringify(errorMsg), 'error');
        this.creating.set(false);
      }
    });
  }

  showMessage(msg: string, type: 'success' | 'error'): void {
    this.message.set(msg);
    this.messageType.set(type);
    setTimeout(() => this.message.set(''), 5000);
  }
}
