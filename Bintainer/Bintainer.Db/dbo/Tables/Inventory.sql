CREATE TABLE [dbo].[Inventory]
(
	[Id] INT NOT NULL CONSTRAINT PK_Inventory PRIMARY KEY IDENTITY(1000,1), 
    [Admin] NVARCHAR(256) NOT NULL, 
    [Name] NCHAR(150) NULL,	
    [UserId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [FK_Inventory_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([ID])
)
