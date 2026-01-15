USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'PetShopDB')
BEGIN
    ALTER DATABASE PetShopDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PetShopDB;
END
GO

CREATE DATABASE PetShopDB;
GO

USE PetShopDB;
GO

DROP TABLE IF EXISTS OrderProduct;
DROP TABLE IF EXISTS OrderHeader;
DROP TABLE IF EXISTS OrderStatus;
DROP TABLE IF EXISTS PickupPoint;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS Supplier;
DROP TABLE IF EXISTS Manufacturer;
DROP TABLE IF EXISTS AnimalType;
DROP TABLE IF EXISTS ProductCategory;
DROP TABLE IF EXISTS AppUser;
DROP TABLE IF EXISTS Role;
GO

CREATE TABLE Role (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE AppUser (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    UserSurname NVARCHAR(100) NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    UserPatronymic NVARCHAR(100) NULL,
    UserLogin NVARCHAR(MAX) NOT NULL, 
    UserPassword NVARCHAR(MAX) NOT NULL,
    RoleID INT NOT NULL,
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES Role(RoleID)
);
GO

CREATE TABLE ProductCategory (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE AnimalType (
    AnimalTypeID INT PRIMARY KEY IDENTITY(1,1),
    AnimalTypeName NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Manufacturer (
    ManufacturerID INT PRIMARY KEY IDENTITY(1,1),
    ManufacturerName NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE Supplier (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    SupplierName NVARCHAR(100) NOT NULL,
    ContactPhone NVARCHAR(20) NULL
);
GO

CREATE TABLE Product (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductArticleNumber NVARCHAR(100) NOT NULL,
    ProductName NVARCHAR(MAX) NOT NULL,
    UnitDescription NVARCHAR(50) NOT NULL,
    ProductCost DECIMAL(19,4) NOT NULL,
    ProductDiscountAmount TINYINT NULL,
    ProductQuantityInStock INT NOT NULL,
    ProductPhoto NVARCHAR(MAX) NULL,
    ProductDescription NVARCHAR(MAX) NULL,
    CategoryID INT NOT NULL,
    AnimalTypeID INT NULL,
    ManufacturerID INT NOT NULL,
    SupplierID INT NOT NULL,
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryID) REFERENCES ProductCategory(CategoryID),
    CONSTRAINT FK_Product_Animal FOREIGN KEY (AnimalTypeID) REFERENCES AnimalType(AnimalTypeID),
    CONSTRAINT FK_Product_Manuf FOREIGN KEY (ManufacturerID) REFERENCES Manufacturer(ManufacturerID),
    CONSTRAINT FK_Product_Supplier FOREIGN KEY (SupplierID) REFERENCES Supplier(SupplierID)
);
GO

CREATE TABLE PickupPoint (
    PickupPointID INT PRIMARY KEY IDENTITY(1,1),
    PostalCode NVARCHAR(10) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Street NVARCHAR(100) NOT NULL,
    HouseNumber NVARCHAR(20) NULL
);
GO

CREATE TABLE OrderStatus (
    OrderStatusID INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE OrderHeader (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATETIME NOT NULL,
    OrderDeliveryDate DATETIME NOT NULL,
    OrderPickupCode INT NOT NULL,
    OrderStatusID INT NOT NULL,
    PickupPointID INT NOT NULL,
    UserID INT NULL,
    CONSTRAINT FK_Order_Status FOREIGN KEY (OrderStatusID) REFERENCES OrderStatus(OrderStatusID),
    CONSTRAINT FK_Order_Pickup FOREIGN KEY (PickupPointID) REFERENCES PickupPoint(PickupPointID),
    CONSTRAINT FK_Order_User FOREIGN KEY (UserID) REFERENCES AppUser(UserID)
);
GO

CREATE TABLE OrderProduct (
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    CONSTRAINT PK_OrderProduct PRIMARY KEY (OrderID, ProductID),
    CONSTRAINT FK_OrderProduct_Order FOREIGN KEY (OrderID) REFERENCES OrderHeader(OrderID) ON DELETE CASCADE,
    CONSTRAINT FK_OrderProduct_Product FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
);
GO
