CREATE TABLE [dbo].[Order]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [OrderNumber] NCHAR(100) NULL DEFAULT ('default'), 
    [Qunatity] INT NULL DEFAULT 0,
    [Supplier] NCHAR(100) NOT NULL DEFAULT ('default'),
    [OrderDate] DATETIME NULL, 
    [HandOverDate] DATETIME NULL, 
    [UserId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [FK_Order_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([ID])


)
