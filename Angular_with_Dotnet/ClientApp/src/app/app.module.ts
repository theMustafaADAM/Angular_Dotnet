import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';
import { HttpClientInMemoryWebApiModule } from 'angular-in-memory-web-api';
import { InMemoryDataService } from './in-memory-data.service';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ValidateCodeComponent } from './validate-code/validate-code.component';
import { SendCodeComponent } from './send-code/send-code.component';
import { AboutUsComponent } from './about-us/about-us.component';
import { TermsComponent } from './terms/terms.component';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { CookieService } from 'ngx-cookie-service';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { ToastrModule } from 'ngx-toastr';
import { AuthGuardService } from './services/auth-guard.service';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { UserComponent } from './user/user.component';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        CounterComponent,
        FetchDataComponent,
        LoginComponent,
        RegisterComponent,
        ValidateCodeComponent,
        SendCodeComponent,
        AboutUsComponent,
        TermsComponent,
        UserComponent,
        ContactUsComponent,
        ForgotPasswordComponent
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        BrowserAnimationsModule,
        HttpClientModule,

        // The HttpClientInMemoryWebApiModule module intercepts HTTP requests
        // and returns simulated server responses.
        // Remove it when a real server is ready to receive requests.
        HttpClientInMemoryWebApiModule.forRoot(InMemoryDataService, { dataEncapsulation: false }),
        ToastrModule.forRoot(),
        RouterModule.forRoot([
            { path: 'home', component: HomeComponent },
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent },
            { path: 'forgot-password', component: ForgotPasswordComponent },
            { path: 'validate-code', component: ValidateCodeComponent },
            { path: 'myaccount', loadChildren: () => import(`./user/user.module`).then((m) => m.UserModule) },
            { path: 'send-code', component: SendCodeComponent },
            { path: 'about-us', component: AboutUsComponent },
            { path: 'terms', component: TermsComponent },
            { path: 'contact-us', component: ContactUsComponent },
            { path: '**', component: HomeComponent }
        ])
    ],
    providers: [AuthGuardService, CookieService],
    bootstrap: [AppComponent]
})
export class AppModule {}
