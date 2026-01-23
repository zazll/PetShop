USE PetShopDB;
GO

-- 1. Cleanup existing data to start fresh and avoid duplicates/conflicts
DELETE FROM Review;
DELETE FROM OrderProduct;
DELETE FROM OrderHeader;
DELETE FROM Product;
DELETE FROM AppUser; 
DELETE FROM Supplier;
DELETE FROM Manufacturer;
DELETE FROM AnimalType;
DELETE FROM ProductCategory;
DELETE FROM PickupPoint;
DELETE FROM OrderStatus;
DELETE FROM Role;

-- Reset Identity counters so IDs start from 1
DBCC CHECKIDENT ('Product', RESEED, 0);
DBCC CHECKIDENT ('AppUser', RESEED, 0);
DBCC CHECKIDENT ('Manufacturer', RESEED, 0);
DBCC CHECKIDENT ('Supplier', RESEED, 0);
DBCC CHECKIDENT ('ProductCategory', RESEED, 0);
DBCC CHECKIDENT ('Role', RESEED, 0);

-- 2. Insert Roles (Using IDENTITY_INSERT to ensure IDs match logic)
SET IDENTITY_INSERT Role ON;
INSERT INTO Role (RoleID, RoleName) VALUES (1, N'Администратор'), (2, N'Менеджер'), (3, N'Пользователь');
SET IDENTITY_INSERT Role OFF;

-- 3. Insert Users (With N prefix for Russian text!)
INSERT INTO AppUser (UserSurname, UserName, UserPatronymic, UserLogin, UserPassword, RoleID) VALUES
(N'Админов', N'Петр', N'Сергеевич', 'admin@petshop.com', 'admin', 1),
(N'Менеджерова', N'Мария', N'Ивановна', 'manager@petshop.com', 'manager', 2),
(N'Петров', N'Иван', N'Сидорович', 'user@petshop.com', 'user', 3),
(N'Сидоров', N'Алексей', N'Петрович', 'alex@gmail.com', '12345', 3),
(N'Иванова', N'Елена', N'Дмитриевна', 'elena@mail.ru', 'pass', 3);

-- 4. Dictionaries
INSERT INTO ProductCategory (CategoryName) VALUES (N'Корм сухой'), (N'Консервы'), (N'Игрушки'), (N'Витамины'), (N'Одежда'), (N'Аксессуары'), (N'Лакомства'), (N'Наполнители');
INSERT INTO AnimalType (AnimalTypeName) VALUES (N'Кошки'), (N'Собаки'), (N'Грызуны'), (N'Птицы'), (N'Рыбки');
INSERT INTO Manufacturer (ManufacturerName) VALUES (N'Royal Canin'), (N'Purina'), (N'Whiskas'), (N'Titbit'), (N'Trixie'), (N'Flexi'), (N'Acana'), (N'Hills');
INSERT INTO Supplier (SupplierName, ContactPhone) VALUES (N'ООО ЗооМир', '89001112233'), (N'ИП Иванов', '89998887766'), (N'PetSupply Global', '+79990000000');
INSERT INTO PickupPoint (PostalCode, City, Street, HouseNumber) VALUES ('100000', N'Москва', N'ул. Ленина', '10'), ('200000', N'Санкт-Петербург', N'Невский пр.', '20');
INSERT INTO OrderStatus (StatusName) VALUES (N'Новый'), (N'Обработан'), (N'Доставлен'), (N'Отменен');

-- Variables for IDs
DECLARE @CatFood INT = (SELECT TOP 1 CategoryID FROM ProductCategory WHERE CategoryName = N'Корм сухой');
DECLARE @CatToys INT = (SELECT TOP 1 CategoryID FROM ProductCategory WHERE CategoryName = N'Игрушки');
DECLARE @CatClothes INT = (SELECT TOP 1 CategoryID FROM ProductCategory WHERE CategoryName = N'Одежда');
DECLARE @CatTreats INT = (SELECT TOP 1 CategoryID FROM ProductCategory WHERE CategoryName = N'Лакомства');

DECLARE @AnimCat INT = (SELECT TOP 1 AnimalTypeID FROM AnimalType WHERE AnimalTypeName = N'Кошки');
DECLARE @AnimDog INT = (SELECT TOP 1 AnimalTypeID FROM AnimalType WHERE AnimalTypeName = N'Собаки');

DECLARE @ManRoyal INT = (SELECT TOP 1 ManufacturerID FROM Manufacturer WHERE ManufacturerName = N'Royal Canin');
DECLARE @ManTrixie INT = (SELECT TOP 1 ManufacturerID FROM Manufacturer WHERE ManufacturerName = N'Trixie');
DECLARE @ManAcana INT = (SELECT TOP 1 ManufacturerID FROM Manufacturer WHERE ManufacturerName = N'Acana');

DECLARE @Sup1 INT = (SELECT TOP 1 SupplierID FROM Supplier);

-- 5. Products (Rich data)
INSERT INTO Product (ProductArticleNumber, ProductName, UnitDescription, ProductCost, ProductDiscountAmount, ProductQuantityInStock, ProductDescription, CategoryID, AnimalTypeID, ManufacturerID, SupplierID, ProductPhoto) VALUES
('A001', N'Royal Canin Sterilised 37', N'шт', 1500.00, 10, 50, N'Корм для стерилизованных кошек, 2кг', @CatFood, @AnimCat, @ManRoyal, @Sup1, 'cat_food.jpg'),
('A002', N'Мячик резиновый Trixie', N'шт', 250.00, 0, 100, N'Прочный мяч для активных игр с собакой', @CatToys, @AnimDog, @ManTrixie, @Sup1, 'ball.jpg'),
('A003', N'Домик-когтеточка "Башня"', N'шт', 5000.00, 15, 10, N'Большой игровой комплекс для кошек с гамаком', @CatToys, @AnimCat, @ManTrixie, @Sup1, 'house.jpg'),
('A004', N'Комбинезон зимний утепленный', N'шт', 2500.00, 5, 20, N'Теплый комбинезон для мелких пород собак, водонепроницаемый', @CatClothes, @AnimDog, @ManTrixie, @Sup1, 'clothes.jpg'),
('A005', N'Лакомство Dreamies с курицей', N'уп', 80.00, 0, 200, N'Хрустящие подушечки с нежной начинкой', @CatTreats, @AnimCat, @ManRoyal, @Sup1, 'treats.jpg'),
('A006', N'Royal Canin Giant Adult', N'мешок', 8000.00, 20, 5, N'Для собак гигантских пород, 15кг. Поддержка суставов.', @CatFood, @AnimDog, @ManRoyal, @Sup1, 'dog_food_big.jpg'),
('A007', N'Ошейник кожаный Premium', N'шт', 1200.00, 0, 30, N'Натуральная кожа, коричневый, размер M', @CatClothes, @AnimDog, @ManTrixie, @Sup1, 'collar.jpg'),
('A008', N'Acana Grasslands Cat', N'шт', 3500.00, 0, 15, N'Беззерновой корм для кошек всех пород, ягненок и утка', @CatFood, @AnimCat, @ManAcana, @Sup1, 'acana_cat.jpg'),
('A009', N'Когтерезка Trixie', N'шт', 450.00, 0, 40, N'Удобная когтерезка-секатор для собак и кошек', @CatToys, @AnimCat, @ManTrixie, @Sup1, 'cutter.jpg'),
('A010', N'Лежак мягкий "Облако"', N'шт', 1800.00, 10, 12, N'Супермягкий лежак для кошек и мелких собак', @CatToys, @AnimCat, @ManTrixie, @Sup1, 'bed.jpg');

-- 6. Reviews
DECLARE @U1 INT = (SELECT TOP 1 UserID FROM AppUser WHERE UserLogin = 'user@petshop.com');
DECLARE @U2 INT = (SELECT TOP 1 UserID FROM AppUser WHERE UserLogin = 'alex@gmail.com');
DECLARE @U3 INT = (SELECT TOP 1 UserID FROM AppUser WHERE UserLogin = 'elena@mail.ru');

DECLARE @P1 INT = (SELECT TOP 1 ProductID FROM Product WHERE ProductArticleNumber = 'A001');
DECLARE @P3 INT = (SELECT TOP 1 ProductID FROM Product WHERE ProductArticleNumber = 'A003');
DECLARE @P6 INT = (SELECT TOP 1 ProductID FROM Product WHERE ProductArticleNumber = 'A006');

INSERT INTO Review (ProductID, UserID, Rating, Comment, ReviewDate) VALUES
(@P1, @U1, 5, N'Отличный корм, кошка ест с удовольствием!', GETDATE()),
(@P1, @U2, 4, N'Хороший, но цена кусается.', GETDATE()-1),
(@P3, @U3, 5, N'Когтеточка просто супер! Кот не слазит уже неделю.', GETDATE()-5),
(@P6, @U2, 5, N'Собака довольна, шерсть блестит.', GETDATE()-10),
(@P1, @U3, 3, N'Упаковка была повреждена при доставке.', GETDATE()-2);

-- 7. Orders (Simulation for reports)
DECLARE @StatusNew INT = (SELECT TOP 1 OrderStatusID FROM OrderStatus WHERE StatusName = N'Новый');
DECLARE @Pick1 INT = (SELECT TOP 1 PickupPointID FROM PickupPoint);

INSERT INTO OrderHeader (OrderDate, OrderDeliveryDate, OrderPickupCode, OrderStatusID, PickupPointID, UserID) VALUES
(GETDATE()-2, GETDATE()+1, 123, @StatusNew, @Pick1, @U1),
(GETDATE()-5, GETDATE()-2, 456, @StatusNew, @Pick1, @U2);

DECLARE @O1 INT = (SELECT TOP 1 OrderID FROM OrderHeader WHERE OrderPickupCode = 123);
DECLARE @O2 INT = (SELECT TOP 1 OrderID FROM OrderHeader WHERE OrderPickupCode = 456);

INSERT INTO OrderProduct (OrderID, ProductID, Quantity) VALUES
(@O1, @P1, 2),
(@O1, @P3, 1),
(@O2, @P6, 1);

GO
