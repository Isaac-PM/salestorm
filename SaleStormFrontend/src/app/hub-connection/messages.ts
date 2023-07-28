
export interface Item {
    id: number;
    itemDescription: string;
    currentQuantity: number;
    minimumQuantity: number;
    reorderQuantity: number;
    purchasePrice: number;
    profitMargin: number;
    salePrice: number;
}

export interface ReceiptLine {
    receiptId: number;
    quantity: number;
    item: Item;
    saleStatus: string;
    lineTotal: number;
}

export interface Receipt {
    id: number;
    workerId: number;
    grandTotal: number;
    lines: ReceiptLine[];
}

export interface Worker {
    id: number;
    userName: string;
    userPassword: string;
    userType: string;
    receipts: Receipt[];
    items: Item[];
}

export interface LoginStatus {
    status: 'idle' | 'success' | 'failure';
    worker?: Worker;
}