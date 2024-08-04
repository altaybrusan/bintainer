CREATE TABLE [dbo].[Part]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(10001,1), 
    [Name] NCHAR(60) NOT NULL, 
    [Description] NCHAR(150) NULL, 
    [CategoryId]INT NULL,
    [FootPrint] INT NOT NULL, 
    [Package] INT NULL,
    [UserId] NVARCHAR (450) NOT NULL,
    [ImageSource] NVARCHAR (200) NULL,
    CONSTRAINT [FK_Part_PartCategory] FOREIGN KEY ([CategoryId]) REFERENCES [PartCategory]([Id]),
    CONSTRAINT [FK_Part_PartFootprint] FOREIGN KEY ([FootPrint]) REFERENCES [PartFootprint]([Id]),
    CONSTRAINT [FK_Part_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([ID])
)
GO

CREATE UNIQUE INDEX [IX_Part_FootPrintId] ON [dbo].[Part]([FootPrint])
