import { Component, inject } from "@angular/core";
import { AuthService } from '../services/auth.service';
import { Logout } from "../logout/logout.component";
import { Login } from "../login/login.component";

@Component({
  selector:'authentication',
  imports:[Login, Logout],
  templateUrl:'./authentication.component.html',
  styleUrl:'./authentication.component.scss'
})
export class Authentication {
    protected readonly authenticationState = inject(AuthService).isLoggedIn;
    
    constructor(
        private authService: AuthService,
    ) {}    
}