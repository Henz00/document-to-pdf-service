import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { documentConverter } from './document-converter/document-converter';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [
    {
        path:"login",
        component: LoginComponent
    },
    {
        path:"document-converter",
        component: documentConverter,
        canActivate: [AuthGuard]
    },
    {
        path: "",
        redirectTo: "document-converter",
        pathMatch: "full"
    }
];
