# Documentation for PetShopDB

## Overview
The **PetShopDB** database is designed to support the Pet Shop management system, handling users, products, orders, and their relationships.

## Schema Diagram
*(Refer to `schema.png` in this directory for the visual diagram)*

## Tables

### 1. Role
Defines user roles within the system.
- **RoleID** (PK, INT, Identity): Unique identifier.
- **RoleName** (NVARCHAR(50), Unique): Name of the role (e.g., Administrator, Manager, User, Guest).

### 2. AppUser
Stores user account information.
- **UserID** (PK, INT, Identity): Unique identifier.
- **UserSurname** (NVARCHAR(100)): User's surname.
- **UserName** (NVARCHAR(100)): User's first name.
- **UserPatronymic** (NVARCHAR(100), Nullable): User's patronymic.
- **UserLogin** (NVARCHAR(MAX)): Login identifier (email/phone).
- **UserPassword** (NVARCHAR(MAX)): Hashed password.
- **RoleID** (FK, INT): Reference to `Role`.

### 3. ProductCategory
Categorizes products (e.g., Food, Toys).
- **CategoryID** (PK, INT, Identity): Unique identifier.
- **CategoryName** (NVARCHAR(100)): Name of the category.

### 4. AnimalType
Specifies the type of animal the product is for.
- **AnimalTypeID** (PK, INT, Identity): Unique identifier.
- **AnimalTypeName** (NVARCHAR(50)): Name of the animal type.

### 5. Manufacturer
Stores product manufacturer details.
- **ManufacturerID** (PK, INT, Identity): Unique identifier.
- **ManufacturerName** (NVARCHAR(100)): Name of the manufacturer.

### 6. Supplier
Stores supplier information.
- **SupplierID** (PK, INT, Identity): Unique identifier.
- **SupplierName** (NVARCHAR(100)): Name of the supplier.
- **ContactPhone** (NVARCHAR(20), Nullable): Contact phone number.

### 7. Product
The core inventory table.
- **ProductID** (PK, INT, Identity): Unique identifier.
- **ProductArticleNumber** (NVARCHAR(100)): SKU/Article number.
- **ProductName** (NVARCHAR(MAX)): Name of the product.
- **UnitDescription** (NVARCHAR(50)): Unit of measure (e.g., pcs, kg).
- **ProductCost** (DECIMAL(19,4)): Cost per unit.
- **ProductDiscountAmount** (TINYINT, Nullable): Discount percentage.
- **ProductQuantityInStock** (INT): Stock quantity.
- **ProductPhoto** (NVARCHAR(MAX), Nullable): Path or filename of the product image.
- **ProductDescription** (NVARCHAR(MAX), Nullable): Detailed description.
- **CategoryID** (FK): Reference to `ProductCategory`.
- **AnimalTypeID** (FK, Nullable): Reference to `AnimalType`.
- **ManufacturerID** (FK): Reference to `Manufacturer`.
- **SupplierID** (FK): Reference to `Supplier`.

### 8. PickupPoint
Locations where orders can be picked up.
- **PickupPointID** (PK, INT, Identity): Unique identifier.
- **PostalCode** (NVARCHAR(10)): Postal code.
- **City** (NVARCHAR(100)): City name.
- **Street** (NVARCHAR(100)): Street name.
- **HouseNumber** (NVARCHAR(20), Nullable): House number.

### 9. OrderStatus
Possible states of an order (e.g., New, Processing, Completed).
- **OrderStatusID** (PK, INT, Identity): Unique identifier.
- **StatusName** (NVARCHAR(50)): Status name.

### 10. OrderHeader
Represents a customer order.
- **OrderID** (PK, INT, Identity): Unique identifier.
- **OrderDate** (DATETIME): Date when the order was placed.
- **OrderDeliveryDate** (DATETIME): Expected delivery date.
- **OrderPickupCode** (INT): Code for picking up the order.
- **OrderStatusID** (FK): Reference to `OrderStatus`.
- **PickupPointID** (FK): Reference to `PickupPoint`.
- **UserID** (FK, Nullable): Reference to `AppUser` (can be null for guest orders if allowed, or system constraints).

### 11. OrderProduct
Many-to-Many relationship table between Orders and Products.
- **OrderID** (PK, FK): Reference to `OrderHeader`.
- **ProductID** (PK, FK): Reference to `Product`.
- **Quantity** (INT): Quantity of the product in the order.
