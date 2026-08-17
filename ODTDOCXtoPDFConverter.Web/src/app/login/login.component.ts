import { Component } from "@angular/core";
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'login',
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username = '';
  password = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  login() {
    this.authService
      .login(this.username, this.password)
      .subscribe({
        next: () => {
          this.router.navigate(['/document-converter']);
        },
        error: error => {
          console.error('Login failed:', error);
        }
      });
  }
}