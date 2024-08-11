CREATE TABLE [dbo].[Order]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [PartId] NCHAR(100) NOT NULL, 
    [OrderNumber] NCHAR(100) NOT NULL, 
    [Qunatity] INT NULL DEFAULT 0,
    [Supplier] NCHAR(100) NOT NULL DEFAULT ('default'),
    [OrderDate] DATETIME NULL, 
    [HandOverDate] DATETIME NULL, 
    [UserId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [FK_Order_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([ID])


)
