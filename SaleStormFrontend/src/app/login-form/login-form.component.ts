import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { HubConnectionService } from '../hub-connection/hub-connection.service';
import { LoginStatus, Worker } from '../hub-connection/messages';
import { Router } from '@angular/router';

@Component({
    selector: 'app-login-form',
    templateUrl: './login-form.component.html',
    styleUrls: ['./login-form.component.css']
})
export class LoginFormComponent implements OnInit {
    userName!: string;
    userPassword!: string;
    loginStatus?: LoginStatus = { status: 'idle', worker: undefined }

    constructor(
        private toastr: ToastrService,
        private hub: HubConnectionService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.hub.loginStatus$?.subscribe(
            (loginStatus$: LoginStatus) => {
                this.loginStatus = loginStatus$;
                if (this.loginStatus.status === 'success') {
                    this.toastr.success(
                        'Login successful.', 'Success',
                        { positionClass: 'toast-bottom-right' }
                    );
                    let worker: Worker = this.loginStatus.worker as Worker;
                    sessionStorage.setItem('worker', JSON.stringify(worker));
                    this.router.navigate(['/mainPanel']);
                }
                if (this.loginStatus.status === 'failure') {
                    this.toastr.error(
                        'Login failed, invalid username or password.', 'Error',
                        { positionClass: 'toast-bottom-right' }
                    );
                }
            }
        );
    }

    // Automated login for testing purposes.
    // Throws error NG0100, don't worry about it.
    // ngAfterViewInit(): void {
    //     this.userName = 'jane';
    //     this.userPassword = 'jane';
    //     this.login();
    // }

    login(): void {
        if (
            this.userName === undefined ||
            this.userPassword === undefined ||
            this.userName === '' ||
            this.userPassword === '') {
            this.toastr.error(
                'Please enter a valid username and password.', 'Error',
                { positionClass: 'toast-bottom-right' }
            );
            return;
        } else {
            this.hub.userLogin(this.userName, this.userPassword);
        }
        this.userName = "";
        this.userPassword = "";
    }
}
