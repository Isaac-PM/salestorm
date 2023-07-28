import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SignalrClient, SignalrConnection } from 'ngx-signalr-websocket';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { Worker, LoginStatus, Item, Receipt } from './messages';

@Injectable({
    providedIn: 'root'
})
export class HubConnectionService {
    private connection?: SignalrConnection;
    public static readonly signalrUrl = 'https://localhost:7183/inventoryHub';

    private loginStatusSubject = new BehaviorSubject<LoginStatus>({ status: 'idle' });
    loginStatus$ = this.loginStatusSubject.asObservable();

    private inventorySubject = new BehaviorSubject<Item[]>([]);
    inventory$ = this.inventorySubject.asObservable();

    private generateReceiptSubject = new BehaviorSubject<string>("idle");
    generateReceipt$ = this.generateReceiptSubject.asObservable();

    private userLoginPipe(connection: SignalrConnection) {
        let pending$ = connection.on<[]>('UserLogin').pipe(
            map((data: any[]) => {
                return data;
            })
        );
        pending$.subscribe(
            (data: any[]) => {
                const loginStatus: LoginStatus = {
                    status: data[0] !== null ? 'success' : 'failure',
                    worker: data[0] !== null ? data[0] as Worker : undefined
                };
                this.loginStatusSubject.next(loginStatus);
            },
            (error: any) => {
                console.error("Error occurred:", error);
            }
        );
    }

    private getInventoryPipe(connection: SignalrConnection) {
        let pending$ = connection.on<[]>('GetInventory').pipe(
            map((data: any[]) => {
                return data;
            })
        );
        pending$.subscribe(
            (data: any[]) => {
                const inventory: Item[] = data[0] !== null ? data[0] as Item[] : [];
                this.inventorySubject.next(inventory);
            },
            (error: any) => {
                console.error("Error occurred:", error);
            }
        );
    }

    private generateReceiptPipe(connection: SignalrConnection) {
        let pending$ = connection.on<[]>('GenerateReceipt').pipe(
            map((data: any[]) => {
                return data;
            })
        );
        pending$.subscribe(
            (data: any[]) => {
                const status: string = data[0] !== null ? data[0] as string : 'idle';
                this.generateReceiptSubject.next(status);
            },
            (error: any) => {
                console.error("Error occurred:", error);
            }
        );
    }

    constructor(httpClient: HttpClient) {
        const client: SignalrClient = SignalrClient.create(httpClient);
        client.connect(HubConnectionService.signalrUrl).subscribe(
            connection => {
                this.connection = connection;
                this.userLoginPipe(this.connection);
                this.getInventoryPipe(this.connection);
                this.generateReceiptPipe(this.connection);
            },
            error => {
                alert('Error connecting to the SignalR hub, make sure the server is running.');
            }
        );
        this.initialize();
    }

    static hashStr(str: string): string {
        let hashLow: number = 0;
        let hashHigh: number = 0;
        for (let i = 0; i < str.length; i++) {
            const character = str.charCodeAt(i);
            hashHigh += ((hashLow << 5) - hashLow) + character;
            hashLow += character;
        }
        let hashBigInt = BigInt(hashHigh) << BigInt(32) | BigInt(hashLow);
        if (hashBigInt < BigInt(0)) {
            hashBigInt = hashBigInt * BigInt(-1);
        }
        return hashBigInt.toString();
    }

    public initialize(): void {
        this.loginStatusSubject = new BehaviorSubject<LoginStatus>({ status: 'idle' });
        this.loginStatus$ = this.loginStatusSubject.asObservable();

        this.inventorySubject = new BehaviorSubject<Item[]>([]);
        this.inventory$ = this.inventorySubject.asObservable();

        this.generateReceiptSubject = new BehaviorSubject<string>("idle");
        this.generateReceipt$ = this.generateReceiptSubject.asObservable();
    }

    public userLogin(userName: string, userPassword: string): void {
        this.connection?.send('UserLogin', userName, userPassword);
    }

    public getInventory(): void {
        this.connection?.send('GetInventory');
    }

    public generateReceipt(receipt: Receipt): void {
        this.connection?.send('GenerateReceipt', receipt);
    }

}