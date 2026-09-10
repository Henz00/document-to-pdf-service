import { Component } from "@angular/core";
import { AuthService } from '../services/auth.service';

@Component({
  selector:'logout',
  imports:[],
  templateUrl:'./logout.component.html',
  styleUrl:'./logout.component.scss'
})
export class Logout{

  constructor(
    private authService: AuthService,
  ) {}

  logout(){
    this.authService
      .logout()
      .subscribe({
        error: error => {
          console.error('Logout failed:', error);
        }
      });
  }
}