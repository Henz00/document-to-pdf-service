import { Routes } from '@angular/router';
import { documentConverter } from './document-converter/document-converter';

export const routes: Routes = [
    {
        path:"document-converter",
        component: documentConverter,
    },
    {
        path: "",
        redirectTo: "document-converter",
        pathMatch: "full"
    }
];
