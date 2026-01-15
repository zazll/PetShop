# Документация БД PetShopDB

## Обзор
База данных **PetShopDB** разработана для поддержки системы управления зоомагазином, обработки пользователей, товаров, заказов и их взаимосвязей.

## Схема БД
*(См. `schema.png` в этой директории для визуальной диаграммы)*

## Таблицы

### 1. Role (Роли)
Определяет роли пользователей в системе.
- **RoleID** (PK, INT, Identity): Уникальный идентификатор.
- **RoleName** (NVARCHAR(50), Unique): Название роли (например, Администратор, Менеджер, Пользователь, Гость).

### 2. AppUser (Пользователи)
Хранит информацию об учетных записях пользователей.
- **UserID** (PK, INT, Identity): Уникальный идентификатор.
- **UserSurname** (NVARCHAR(100)): Фамилия пользователя.
- **UserName** (NVARCHAR(100)): Имя пользователя.
- **UserPatronymic** (NVARCHAR(100), Nullable): Отчество пользователя.
- **UserLogin** (NVARCHAR(MAX)): Логин (email/телефон).
- **UserPassword** (NVARCHAR(MAX)): Хешированный пароль.
- **RoleID** (FK, INT): Ссылка на таблицу `Role`.

### 3. ProductCategory (Категории товаров)
Категоризирует товары (например, Корм, Игрушки).
- **CategoryID** (PK, INT, Identity): Уникальный идентификатор.
- **CategoryName** (NVARCHAR(100)): Название категории.

### 4. AnimalType (Типы животных)
Указывает, для какого вида животного предназначен товар.
- **AnimalTypeID** (PK, INT, Identity): Уникальный идентификатор.
- **AnimalTypeName** (NVARCHAR(50)): Название вида животного.

### 5. Manufacturer (Производители)
Хранит данные о производителях товаров.
- **ManufacturerID** (PK, INT, Identity): Уникальный идентификатор.
- **ManufacturerName** (NVARCHAR(100)): Название производителя.

### 6. Supplier (Поставщики)
Хранит информацию о поставщиках.
- **SupplierID** (PK, INT, Identity): Уникальный идентификатор.
- **SupplierName** (NVARCHAR(100)): Название поставщика.
- **ContactPhone** (NVARCHAR(20), Nullable): Контактный телефон.

### 7. Product (Товары)
Основная таблица инвентаря.
- **ProductID** (PK, INT, Identity): Уникальный идентификатор.
- **ProductArticleNumber** (NVARCHAR(100)): Артикул товара.
- **ProductName** (NVARCHAR(MAX)): Название товара.
- **UnitDescription** (NVARCHAR(50)): Единица измерения (шт, кг и т.д.).
- **ProductCost** (DECIMAL(19,4)): Стоимость за единицу.
- **ProductDiscountAmount** (TINYINT, Nullable): Размер скидки (%).
- **ProductQuantityInStock** (INT): Количество на складе.
- **ProductPhoto** (NVARCHAR(MAX), Nullable): Путь или имя файла изображения.
- **ProductDescription** (NVARCHAR(MAX), Nullable): Подробное описание.
- **CategoryID** (FK): Ссылка на `ProductCategory`.
- **AnimalTypeID** (FK, Nullable): Ссылка на `AnimalType`.
- **ManufacturerID** (FK): Ссылка на `Manufacturer`.
- **SupplierID** (FK): Ссылка на `Supplier`.

### 8. PickupPoint (Пункты выдачи)
Места, где можно получить заказ.
- **PickupPointID** (PK, INT, Identity): Уникальный идентификатор.
- **PostalCode** (NVARCHAR(10)): Почтовый индекс.
- **City** (NVARCHAR(100)): Город.
- **Street** (NVARCHAR(100)): Улица.
- **HouseNumber** (NVARCHAR(20), Nullable): Номер дома.

### 9. OrderStatus (Статусы заказов)
Возможные состояния заказа (например, Новый, В обработке, Выполнен).
- **OrderStatusID** (PK, INT, Identity): Уникальный идентификатор.
- **StatusName** (NVARCHAR(50)): Название статуса.

### 10. OrderHeader (Заголовки заказов)
Представляет заказ клиента.
- **OrderID** (PK, INT, Identity): Уникальный идентификатор.
- **OrderDate** (DATETIME): Дата оформления заказа.
- **OrderDeliveryDate** (DATETIME): Ожидаемая дата доставки.
- **OrderPickupCode** (INT): Код для получения заказа.
- **OrderStatusID** (FK): Ссылка на `OrderStatus`.
- **PickupPointID** (FK): Ссылка на `PickupPoint`.
- **UserID** (FK, Nullable): Ссылка на `AppUser` (может быть NULL для гостевых заказов).

### 11. OrderProduct (Состав заказа)
Таблица связи "Многие-ко-Многим" между Заказами и Товарами.
- **OrderID** (PK, FK): Ссылка на `OrderHeader`.
- **ProductID** (PK, FK): Ссылка на `Product`.
- **Quantity** (INT): Количество товара в заказе.
