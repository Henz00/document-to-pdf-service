import { Component } from "@angular/core";
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector:'logout',
  imports:[],
  templateUrl:'./logout.component.html',
  styleUrl:'./logout.component.scss'
})
export class Logout{

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  logout(){
    this.authService
      .logout()
      .subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: error => {
          console.error('Logout failed:', error);
        }
      });
  }
}