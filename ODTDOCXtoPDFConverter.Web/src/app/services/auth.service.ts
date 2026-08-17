import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment'

interface LoginRequest {
  username: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/auth`;

  private loggedIn = false;
  private authenticationChecked = false;

  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<void> {
    const credentials: LoginRequest = {
      username,
      password
    };

    return this.http.post<void>(
      `${this.apiUrl}/login`,
      credentials,
      { withCredentials: true }
    ).pipe(
      tap(() => {
        this.loggedIn = true;
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/logout`,
      {},
      { withCredentials: true }
    ).pipe(
      tap(() => {
        this.loggedIn = false;
      })
    );
  }

  checkAuthentication(): Observable<boolean> {
    return this.http.get<void>(
      `${this.apiUrl}/me`,
      { withCredentials: true }
    ).pipe(
      map(() => true),
      catchError(() => {
        this.loggedIn = false;
        this.authenticationChecked = true;
        return of(false);
      })
    );
  }

  isAuthenticationChecked(): boolean {
    return this.authenticationChecked;
  }

  isLoggedIn(): boolean {
    return this.loggedIn;
  }
}