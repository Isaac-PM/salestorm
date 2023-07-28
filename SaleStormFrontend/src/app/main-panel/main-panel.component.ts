import { Component, ElementRef, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { Item, Receipt, ReceiptLine, Worker } from '../hub-connection/messages';
import { Router } from '@angular/router';
import { HubConnectionService } from '../hub-connection/hub-connection.service';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-main-panel',
    templateUrl: './main-panel.component.html',
    styleUrls: ['./main-panel.component.css']
})
export class MainPanelComponent implements OnInit {
    worker!: Worker;
    inventory: Item[] = [];

    // Seller related properties.
    shoppingCart = new Map<number, number>(); // <itemId, quantity>
    shoppingCartTotal: number = 0;
    receiptId: number = 0;
    pendingReceipt!: Receipt;

    // Admin related properties.
    // None.

    constructor(
        private router: Router,
        private hub: HubConnectionService,
        private toastr: ToastrService,
        private elementRef: ElementRef
    ) { }

    ngOnInit(): void {
        this.hub.inventory$?.subscribe(
            (inventory$: Item[]) => {
                this.inventory = inventory$;
                sessionStorage.setItem('inventory', JSON.stringify(this.inventory));
            }
        );
        this.hub.generateReceipt$?.subscribe(
            (generateReceipt$: string) => {
                if (generateReceipt$ === 'success') {
                    this.toastr.success(
                        'Receipt generated successfully.', 'Success',
                        { positionClass: 'toast-bottom-right' }
                    );
                    this.worker.receipts.push(this.pendingReceipt);
                    this.shoppingCart.clear();
                    this.shoppingCartTotal = 0;
                    this.receiptId = 0;
                } else if (generateReceipt$ === 'failure') {
                    this.toastr.error(
                        'Receipt generation failed.', 'Error',
                        { positionClass: 'toast-bottom-right' }
                    );
                }
            }
        );
        const workerData = sessionStorage.getItem('worker');
        this.worker = workerData !== null ? JSON.parse(workerData) : undefined;
        if (this.worker === undefined) {
            this.router.navigate(['/login']);
        } else {
            this.hub.getInventory();
        }
    }

    ngAfterViewInit(): void { }

    logout(): void {
        sessionStorage.clear();
        this.hub.initialize();
        this.toastr.success(
            'Logout successful.', 'Success',
            { positionClass: 'toast-bottom-right' }
        )
        this.router.navigate(['']);
    }

    // Seller related methods.
    calculateShoppingCartTotal(): number {
        let total: number = 0;
        this.shoppingCart.forEach((quantity, itemId) => {
            let item = this.inventory.find(item => item.id === itemId);
            if (item !== undefined) {
                total += item.salePrice * quantity;
            }
        });
        return total;
    }

    addToCart(event: any): void {
        const quantityInputElement: HTMLInputElement = event.target as HTMLInputElement;
        let itemId: number = parseInt(quantityInputElement.getAttribute('data-itemId') ?? '0');
        let quantity: number = Number(quantityInputElement.value);
        let item = this.inventory.find(item => item.id === itemId);

        if (item !== undefined) {
            if (quantity < 0 || quantity > item.currentQuantity) {
                this.toastr.error(
                    'Invalid quantity.', 'Error',
                    { positionClass: 'toast-bottom-right' }
                );
                quantityInputElement.value = '0';
                this.addToCart(event);
                return;
            }
            this.shoppingCart.set(itemId, quantity);
            const totalTextElement = this.elementRef.nativeElement.querySelector(`#itemTotal${itemId}`);
            totalTextElement.textContent = `$${(item.salePrice * quantity).toFixed(2)}`;
            this.shoppingCartTotal = this.calculateShoppingCartTotal();
        }
    }

    private get receiptLines(): ReceiptLine[] {
        let receiptLines: ReceiptLine[] = [];
        this.shoppingCart.forEach((quantity, itemId) => {
            let item = this.inventory.find(item => item.id === itemId);
            if (item !== undefined) {
                let receiptLine: ReceiptLine = {
                    receiptId: this.receiptId,
                    quantity: quantity,
                    item: item,
                    saleStatus: 'open',
                    lineTotal: item.salePrice * quantity
                }
                receiptLines.push(receiptLine);
            }
        });
        return receiptLines;
    }

    private computeReceiptId(): void {
        let completeShoppingCart = JSON.stringify(this.shoppingCart);
        let workerName = this.worker.userName;
        let workerId = this.worker.id.toString();
        let currentTime = Date.now().toString();
        let str: string =
            completeShoppingCart +
            workerName +
            workerId +
            currentTime +
            Math.floor(Math.random() * 1000).toString();
        this.receiptId = parseInt(HubConnectionService.hashStr(str));
    }

    generateReceipt(): void {
        if (this.shoppingCart.size === 0) {
            this.toastr.error(
                'Shopping cart is empty.', 'Error',
                { positionClass: 'toast-bottom-right' }
            );
        } else {
            this.computeReceiptId();
            let receipt: Receipt = {
                id: this.receiptId,
                workerId: this.worker.id,
                grandTotal: this.shoppingCartTotal,
                lines: this.receiptLines
            }
            this.pendingReceipt = receipt;
            this.hub.generateReceipt(receipt);
        }
    }

    get totalSelledFromWorker(): number {
        return this.worker.receipts.reduce((sum, receipt) => sum + receipt.grandTotal, 0)
    }

    // Admin related methods.
    // None.
}
