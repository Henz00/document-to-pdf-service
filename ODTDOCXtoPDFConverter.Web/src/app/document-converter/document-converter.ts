import { Component, signal, ChangeDetectorRef } from "@angular/core";
import { DocumentService } from "../services/document.service";
import { Logout } from "../logout/logout.component";
import { FormGroup, FormControl, ReactiveFormsModule } from  "@angular/forms";

@Component({
  selector: 'document-converter',
  templateUrl: './document-converter.html',
  styleUrl: './document-converter.scss',
  imports: [Logout, ReactiveFormsModule]
})
export class documentConverter {
  protected readonly title = signal('ODTDOCXtoPDFConverter.Web');
  documentFile?: File;
  variablesFile?: File;
  variables: string[] = [];
  form = new FormGroup({});

  constructor(
    private documentService: DocumentService,
    private cdr: ChangeDetectorRef
  ) {}

  onDocumentSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.documentFile = input.files?.[0];
    this.documentService.fetchVariables(this.documentFile)
    .subscribe({
      next: extracted => {
        console.log(extracted);
        this.variables = extracted;
        
        for (const control of Object.keys(this.form.controls)) {
          this.form.removeControl(control);
        }

        for (const variable of extracted) {
          this.form.addControl(variable, new FormControl(''));
        }

        this.cdr.detectChanges();
      }
    });

    
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
