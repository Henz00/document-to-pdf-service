import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {

  private apiUrl = `${environment.apiUrl}/document`;

  constructor(private http: HttpClient) {}

  convert(document: File, variables: Record<string, string | null>) {
    const formData = new FormData();

    formData.append('document', document);

    const json = JSON.stringify(variables);
    const variablesBlob = new Blob([json], {
      type: 'application/json'
    });

    formData.append('variables', variablesBlob, 'variables.json');

    return this.http.post(this.apiUrl, formData, {
      responseType: 'blob',
      withCredentials: true
    });
  }

  fetchVariables(document: File | undefined){
    const formData = new FormData();

    if (document) {
        formData.append("document", document);
    }

    let url = this.apiUrl + "/extract";
    return this.http.post<string[]>(url, formData);
  }
}