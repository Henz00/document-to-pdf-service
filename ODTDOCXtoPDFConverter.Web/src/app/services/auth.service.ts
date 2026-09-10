import { Injectable, signal } from '@angular/core';
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

  private readonly loggedIn = signal(false);
  readonly isLoggedIn = this.loggedIn.asReadonly();
  
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
      tap(() => 
        this.loggedIn.set(true)
      )
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/logout`,
      {},
      { withCredentials: true }
    ).pipe(
      tap(() =>
        this.loggedIn.set(false)
      )
    );
  }

  checkAuthentication(): Observable<boolean> {
    return this.http.get<void>(
      `${this.apiUrl}/me`,
      { withCredentials: true }
    ).pipe(
      map(() => true),
      catchError(() => {
        this.loggedIn.set(false);
        this.authenticationChecked = true;
        return of(false);
      }), tap(isAuthenticated => {
          this.loggedIn.set(isAuthenticated);
          this.authenticationChecked = true;
      })
    );
  }

  isAuthenticationChecked(): boolean {
    return this.authenticationChecked;
  }
}