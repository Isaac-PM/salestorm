-- SQL Server 2019 & SQL Server Management Studio 19.1

-- Database: INVENTORY

-- Tables: Worker, Item, Receipt, ReceiptLine

DROP TABLE IF EXISTS INVENTORY.dbo.ReceiptLine;
DROP TABLE IF EXISTS INVENTORY.dbo.Receipt;
DROP TABLE IF EXISTS INVENTORY.dbo.Item;
DROP TABLE IF EXISTS INVENTORY.dbo.Worker;

DROP TABLE IF EXISTS INVENTORY.dbo.Worker;
CREATE TABLE INVENTORY.dbo.Worker (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_name VARCHAR(28) NOT NULL,
    user_password VARCHAR(28) NOT NULL,
    user_type VARCHAR(28) NOT NULL CHECK (user_type IN ('admin', 'seller'))
);

DROP TABLE IF EXISTS INVENTORY.dbo.Item;
CREATE TABLE INVENTORY.dbo.Item (
    id INT IDENTITY(1,1) PRIMARY KEY,
    item_description VARCHAR(28) NOT NULL,
    current_quantity INT NOT NULL,
    minimum_quantity INT NOT NULL,
    reorder_quantity INT NOT NULL,
    purchase_price FLOAT NOT NULL,
    profit_margin FLOAT NOT NULL,
    sale_price AS purchase_price * (1 + profit_margin)
);

DROP TABLE IF EXISTS INVENTORY.dbo.Receipt;
CREATE TABLE INVENTORY.dbo.Receipt (
    id BIGINT PRIMARY KEY,
    worker_id INT NOT NULL,
    grand_total FLOAT DEFAULT 0,
    CONSTRAINT fk_worker_id FOREIGN KEY (worker_id) REFERENCES INVENTORY.dbo.Worker(id)
);

DROP TABLE IF EXISTS INVENTORY.dbo.ReceiptLine;
CREATE TABLE INVENTORY.dbo.ReceiptLine (
    receipt_id BIGINT NOT NULL,
    item_id INT NOT NULL,
    quantity INT NOT NULL,
    sale_status VARCHAR(28) DEFAULT 'open' CHECK (sale_status IN ('open', 'closed')),
    line_total FLOAT DEFAULT 0,
    CONSTRAINT fk_receipt_id FOREIGN KEY (receipt_id) REFERENCES INVENTORY.dbo.Receipt(id),
    CONSTRAINT fk_item_id FOREIGN KEY (item_id) REFERENCES INVENTORY.dbo.Item(id),
    PRIMARY KEY (receipt_id, item_id)
);
GO
-- Triggers

CREATE TRIGGER dbo.ComputeLineTotalTrigger
ON INVENTORY.dbo.ReceiptLine
AFTER INSERT
AS
BEGIN
    DECLARE @receipt_id BIGINT;
    DECLARE @item_id INT;
    DECLARE @quantity INT;
    DECLARE @line_total FLOAT;
    DECLARE @grand_total FLOAT;

    SELECT @receipt_id = receipt_id FROM inserted;
    SELECT @item_id = item_id FROM inserted;
    SELECT @quantity = quantity FROM inserted;

    SET @line_total = @quantity * (SELECT sale_price FROM INVENTORY.dbo.Item WHERE id = @item_id);
    UPDATE INVENTORY.dbo.ReceiptLine SET line_total = @line_total WHERE receipt_id = @receipt_id AND item_id = @item_id;

    SELECT @grand_total = grand_total FROM INVENTORY.dbo.Receipt WHERE id = @receipt_id;
    SET @grand_total = @grand_total + @line_total;
    UPDATE INVENTORY.dbo.Receipt SET grand_total = @grand_total WHERE id = @receipt_id;
END;
GO

CREATE TRIGGER dbo.PreventNegativeItemQuantityTrigger
ON INVENTORY.dbo.Item
AFTER UPDATE
AS
BEGIN
    DECLARE @item_id INT;
    DECLARE @current_quantity INT;
    DECLARE @minimum_quantity INT;
    DECLARE @reorder_quantity INT;

    SELECT @item_id = id FROM inserted;
    SELECT @current_quantity = current_quantity FROM inserted;
    SELECT @minimum_quantity = minimum_quantity FROM inserted;
    SELECT @reorder_quantity = reorder_quantity FROM inserted;

    IF @current_quantity < @minimum_quantity
    BEGIN
        UPDATE INVENTORY.dbo.Item SET current_quantity = @reorder_quantity WHERE id = @item_id;
    END;
END;
GO

-- Stored procedures

/*
This stored procedure takes the ID of a worker as input and performs the following actions:

1. It searches for every open receipt line associated with the worker's receipts.
2. For each open receipt line found, it reduces the quantity of the corresponding item in the inventory by the quantity mentioned in the receipt line.
3. After adjusting the inventory quantity, it closes the receipt line.

To execute, use the following SQL command:

EXEC INVENTORY.dbo.ApplyReceipts @worker_id = <worker_id_value>;
*/

CREATE PROCEDURE dbo.ApplyReceipts
    @worker_id INT
AS
BEGIN
    DECLARE @receipt_id BIGINT;
    DECLARE @item_id INT;
    DECLARE @quantity INT;
    DECLARE @current_quantity INT;
    DECLARE @sale_status VARCHAR(28);

    DECLARE receipt_line_cursor CURSOR FOR
    SELECT receipt_id, item_id, quantity 
    FROM INVENTORY.dbo.ReceiptLine 
    WHERE sale_status = 'open' 
    AND receipt_id 
    IN (SELECT id FROM INVENTORY.dbo.Receipt WHERE worker_id = @worker_id);

    OPEN receipt_line_cursor;

    FETCH NEXT FROM receipt_line_cursor INTO @receipt_id, @item_id, @quantity;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @current_quantity = current_quantity 
        FROM INVENTORY.dbo.Item 
        WHERE id = @item_id;

        SET @current_quantity = @current_quantity - @quantity;

        UPDATE INVENTORY.dbo.Item 
        SET current_quantity = @current_quantity 
        WHERE id = @item_id;

        UPDATE INVENTORY.dbo.ReceiptLine 
        SET sale_status = 'closed' 
        WHERE receipt_id = @receipt_id 
        AND item_id = @item_id;

        FETCH NEXT FROM receipt_line_cursor INTO @receipt_id, @item_id, @quantity;
    END;

    CLOSE receipt_line_cursor;

    DEALLOCATE receipt_line_cursor;
END;
GO

-- Test data

DELETE FROM INVENTORY.dbo.ReceiptLine;
DELETE FROM INVENTORY.dbo.Receipt;
DELETE FROM INVENTORY.dbo.Item;
DELETE FROM INVENTORY.dbo.Worker;
GO

DBCC CHECKIDENT (INVENTORY.dbo.Receipt, RESEED, 0)
DBCC CHECKIDENT (INVENTORY.dbo.Item, RESEED, 0)
DBCC CHECKIDENT (INVENTORY.dbo.Worker, RESEED, 0)
GO

INSERT INTO INVENTORY.dbo.Worker (user_name, user_password, user_type) VALUES ('admin', 'admin', 'admin');

INSERT INTO INVENTORY.dbo.Worker (user_name, user_password, user_type) VALUES ('john', 'john', 'seller');

INSERT INTO INVENTORY.dbo.Worker (user_name, user_password, user_type) VALUES ('jane', 'jane', 'seller');

INSERT INTO INVENTORY.dbo.Item (item_description, current_quantity, minimum_quantity, reorder_quantity, purchase_price, profit_margin) VALUES ('White paint', 16, 7, 15, 10, 0.33);

INSERT INTO INVENTORY.dbo.Item (item_description, current_quantity, minimum_quantity, reorder_quantity, purchase_price, profit_margin) VALUES ('10mm screw', 100, 50, 100, 0.1, 0.15);

INSERT INTO INVENTORY.dbo.Item (item_description, current_quantity, minimum_quantity, reorder_quantity, purchase_price, profit_margin) VALUES ('Hammer', 41, 3, 20, 7, 0.45);

INSERT INTO INVENTORY.dbo.Receipt (id, worker_id) VALUES (1, 2);
INSERT INTO INVENTORY.dbo.ReceiptLine (receipt_id, item_id, quantity) VALUES (1, 1, 3);
INSERT INTO INVENTORY.dbo.ReceiptLine (receipt_id, item_id, quantity) VALUES (1, 2, 27);

INSERT INTO INVENTORY.dbo.Receipt (id, worker_id) VALUES (2, 3);
INSERT INTO INVENTORY.dbo.ReceiptLine (receipt_id, item_id, quantity) VALUES (2, 1, 2);
INSERT INTO INVENTORY.dbo.ReceiptLine (receipt_id, item_id, quantity) VALUES (2, 3, 1);

INSERT INTO INVENTORY.dbo.Receipt (id, worker_id) VALUES (3, 3);
INSERT INTO INVENTORY.dbo.ReceiptLine (receipt_id, item_id, quantity) VALUES (3, 3, 7);
INSERT INTO INVENTORY.dbo.ReceiptLine (receipt_id, item_id, quantity) VALUES (3, 2, 31);

EXEC INVENTORY.dbo.ApplyReceipts @worker_id = 2;
EXEC INVENTORY.dbo.ApplyReceipts @worker_id = 3;