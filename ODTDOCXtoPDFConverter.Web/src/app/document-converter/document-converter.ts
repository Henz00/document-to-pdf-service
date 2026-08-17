import { Component, signal } from "@angular/core";
import { DocumentService } from "../services/document.service";
import { Logout } from "../logout/logout.component";

@Component({
  selector: 'document-converter',
  imports: [Logout],
  templateUrl: './document-converter.html',
  styleUrl: './document-converter.scss'
})
export class documentConverter {
  protected readonly title = signal('ODTDOCXtoPDFConverter.Web');
  documentFile?: File;
  variablesFile?: File;

  constructor(private documentService: DocumentService) {}

  onDocumentSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.documentFile = input.files?.[0];
  }

  onVariablesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.variablesFile = input.files?.[0];
  }

  convert(event: Event) {
    event.preventDefault();

    if (!this.documentFile || !this.variablesFile)
      return;

    this.documentService
    .convert(this.documentFile, this.variablesFile)
    .subscribe({
      next: pdf => {
        console.log('PDF received!', pdf);

        const url = URL.createObjectURL(pdf);

        const link = document.createElement('a');
        link.href = url;
        link.download = 'converted_file.pdf';
        link.click();

        URL.revokeObjectURL(url);
      },
      error: error => {
        console.error('Conversion failed:', error);
      }
    });
  }
}
