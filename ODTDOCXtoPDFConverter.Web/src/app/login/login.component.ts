import { Component } from "@angular/core";
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'login',
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class Login {
  username = '';
  password = '';

  constructor(
    private authService: AuthService,
  ) {}

  login() {
    this.authService
      .login(this.username, this.password)
      .subscribe({
        error: error => {
          console.error('Login failed:', error);
        }
      });
  }
}