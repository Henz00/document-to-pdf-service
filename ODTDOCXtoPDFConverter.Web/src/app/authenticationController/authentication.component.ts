import { Component, OnInit } from "@angular/core";
import { AuthService } from '../services/auth.service';
import { Logout } from "../logout/logout.component";
import { Login } from "../login/login.component";

@Component({
  selector:'authentication',
  imports:[Login, Logout],
  templateUrl:'./authentication.component.html',
  styleUrl:'./authentication.component.scss'
})
export class Authentication implements OnInit{
    authenticationState = false;
    
    constructor(
        private authService: AuthService,
    ) {}
    
    ngOnInit(){
        this.authenticationState = this.authService.isLoggedIn();
    }
    
}