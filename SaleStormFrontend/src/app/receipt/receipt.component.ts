import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { Receipt } from '../hub-connection/messages';

@Component({
    selector: 'tr[app-receipt]',
    templateUrl: './receipt.component.html',
    styleUrls: ['./receipt.component.css']
})
export class ReceiptComponent {
    @Input() receipt!: Receipt;
    displayDetails: boolean = false;
    @ViewChild('detailsDialog') detailsDialog!: ElementRef<HTMLDialogElement>;

    constructor() { }

    showReceiptDetails() {
        this.displayDetails = !this.displayDetails;
        if (this.displayDetails) {
            this.detailsDialog.nativeElement.showModal();
        } else {
            this.detailsDialog.nativeElement.close();
        }
    }
}
