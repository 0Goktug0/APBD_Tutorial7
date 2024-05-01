-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2021-04-05 12:56:53.13
CREATE TABLE Product (
    IdProduct int NOT NULL IDENTITY,
    Name nvarchar(200) NOT NULL,
    Description nvarchar(200) NOT NULL,
    Price numeric(25,2) NOT NULL,
    CONSTRAINT Product_pk PRIMARY KEY (IdProduct)
);
GO

-- Table: Warehouse
CREATE TABLE Warehouse (
    IdWarehouse int NOT NULL IDENTITY,
    Name nvarchar(200) NOT NULL,
    Address nvarchar(200) NOT NULL,
    CONSTRAINT Warehouse_pk PRIMARY KEY (IdWarehouse)
);
GO

-- Table: Order
CREATE TABLE "Order" (
    IdOrder int NOT NULL IDENTITY,
    IdProduct int NOT NULL,
    IdWarehouse int NOT NULL,
    Amount int NOT NULL,
    CreatedAt datetime NOT NULL,
    FulfilledAt datetime NULL,
    CONSTRAINT Order_pk PRIMARY KEY (IdOrder)
);
GO

CREATE TABLE Product_Warehouse (
    IdProductWarehouse int NOT NULL IDENTITY,
    IdWarehouse int NOT NULL,
    IdProduct int NOT NULL,
    IdOrder int NOT NULL,
    Amount int NOT NULL,
    Price numeric(25,2) NOT NULL,
    CreatedAt datetime NOT NULL,
    CONSTRAINT Product_Warehouse_pk PRIMARY KEY (IdProductWarehouse)
);
GO

ALTER TABLE "Order" ADD CONSTRAINT FK_Order_Warehouse
    FOREIGN KEY (IdWarehouse)
    REFERENCES Warehouse (IdWarehouse);
GO

ALTER TABLE "Order" ADD CONSTRAINT Receipt_Product
    FOREIGN KEY (IdProduct)
    REFERENCES Product (IdProduct);
GO

ALTER TABLE Product_Warehouse ADD CONSTRAINT _Product
    FOREIGN KEY (IdProduct)
    REFERENCES Product (IdProduct);
GO

ALTER TABLE Product_Warehouse ADD CONSTRAINT Product_Warehouse_Order
    FOREIGN KEY (IdOrder)
    REFERENCES "Order" (IdOrder);
GO

ALTER TABLE Product_Warehouse ADD CONSTRAINT _Warehouse
    FOREIGN KEY (IdWarehouse)
    REFERENCES Warehouse (IdWarehouse);
GO


INSERT INTO Warehouse(Name, Address)
VALUES
    ('Warsaw', 'Kwiatowa 12'),
    ('Turkey', 'Bizim 123'),
    ('Poland', 'Mokotow 456');
GO


INSERT INTO Product(Name, Description, Price)
VALUES
    ('Abacavir', 'Antiviral drug', 25.5),
    ('deneme', 'Test product', 45.0),
    ('isim', 'Example product', 30.8);
GO

INSERT INTO "Order"(IdProduct, IdWarehouse, Amount, CreatedAt)
VALUES
    ((SELECT IdProduct FROM Product WHERE Name='Abacavir'), (SELECT IdWarehouse FROM Warehouse WHERE Name='Warsaw'), 125, GETDATE()),
    ((SELECT IdProduct FROM Product WHERE Name='deneme'), (SELECT IdWarehouse FROM Warehouse WHERE Name='Turkey'), 75, GETDATE()), 
    ((SELECT IdProduct FROM Product WHERE Name='isim'), (SELECT IdWarehouse FROM Warehouse WHERE Name='Poland'), 100, GETDATE());
GO

INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
SELECT 
    o.IdWarehouse, 
    o.IdProduct, 
    o.IdOrder, 
    o.Amount, 
    p.Price, 
    o.CreatedAt
FROM 
    "Order" o
JOIN 
    Product p ON o.IdProduct = p.IdProduct
WHERE 
    o.IdOrder IN (SELECT TOP 3 IdOrder FROM "Order" ORDER BY CreatedAt DESC);
GO







CREATE PROCEDURE AddProductToWarehouse @IdProduct INT, @IdWarehouse INT, @Amount INT,  
@CreatedAt DATETIME
AS  
BEGIN  
   
 DECLARE @IdProductFromDb INT, @IdOrder INT, @Price DECIMAL(5,2);  
  
 SELECT TOP 1 @IdOrder = o.IdOrder  FROM "Order" o   
 LEFT JOIN Product_Warehouse pw ON o.IdOrder=pw.IdOrder  
 WHERE o.IdProduct=@IdProduct AND o.Amount=@Amount AND pw.IdProductWarehouse IS NULL AND  
 o.CreatedAt<@CreatedAt;  
  
 SELECT @IdProductFromDb=Product.IdProduct, @Price=Product.Price FROM Product WHERE IdProduct=@IdProduct  
   
 IF @IdProductFromDb IS NULL  
 BEGIN  
  RAISERROR('Invalid parameter: Provided IdProduct does not exist', 18, 0);  
  RETURN;  
 END;  
  
 IF @IdOrder IS NULL  
 BEGIN  
  RAISERROR('Invalid parameter: There is no order to fullfill', 18, 0);  
  RETURN;  
 END;  
   
 IF NOT EXISTS(SELECT 1 FROM Warehouse WHERE IdWarehouse=@IdWarehouse)  
 BEGIN  
  RAISERROR('Invalid parameter: Provided IdWarehouse does not exist', 18, 0);  
  RETURN;  
 END;  
  
 SET XACT_ABORT ON;  
 BEGIN TRAN;  
   
 UPDATE "Order" SET  
 FulfilledAt=@CreatedAt  
 WHERE IdOrder=@IdOrder;  
  
 INSERT INTO Product_Warehouse(IdWarehouse,   
 IdProduct, IdOrder, Amount, Price, CreatedAt)  
 VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Amount*@Price, @CreatedAt);  
   
 SELECT @@IDENTITY AS NewId;
   
 COMMIT;  
END










SELECT * FROM "Order";

SELECT * FROM Product_Warehouse;

SELECT * FROM Warehouse;

SELECT * FROM Product;





DROP TABLE IF EXISTS Product_Warehouse;

DROP TABLE IF EXISTS "Order";

DROP TABLE IF EXISTS Product;

DROP TABLE IF EXISTS Warehouse;