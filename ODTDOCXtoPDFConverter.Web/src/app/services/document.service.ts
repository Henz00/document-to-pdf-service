import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {

  private apiUrl = `${environment.apiUrl}/document`;

  constructor(private http: HttpClient) {}

  convert(document: File, variables: File) {
    const formData = new FormData();

    formData.append('document', document);
    formData.append('variables', variables);

    return this.http.post(this.apiUrl, formData, {
      responseType: 'blob',
      withCredentials: true
    });
  }
}