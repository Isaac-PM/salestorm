import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { ToastrModule } from 'ngx-toastr';
import { LoginFormComponent } from './login-form/login-form.component';
import { HubConnectionService } from './hub-connection/hub-connection.service';
import { HttpClientModule } from '@angular/common/http';
import { MainPanelComponent } from './main-panel/main-panel.component';
import { ReceiptComponent } from './receipt/receipt.component';

@NgModule({
    declarations: [
        AppComponent,
        LoginFormComponent,
        MainPanelComponent,
        ReceiptComponent
    ],
    imports: [
        BrowserModule,
        AppRoutingModule,
        FormsModule,
        ToastrModule.forRoot(),
        HttpClientModule,
        BrowserAnimationsModule
    ],
    providers: [
        HubConnectionService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
