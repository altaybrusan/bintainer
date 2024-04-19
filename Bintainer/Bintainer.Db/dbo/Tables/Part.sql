CREATE TABLE [dbo].[Part]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(10001,1), 
    [Name] NCHAR(60) NOT NULL, 
    [Description] NCHAR(150) NULL, 
    [CategoryId]INT NULL,
    [ImageUri]NCHAR(100) NULL,
    [DatasheetUri]NCHAR(100) NULL, 
    [FootPrint] INT NULL, 
    [Package] INT NULL,
    CONSTRAINT [FK_Part_PartFootprint] FOREIGN KEY ([FootPrint]) REFERENCES [PartFootprint]([Id]),
    CONSTRAINT [FK_Part_PartPackage] FOREIGN KEY ([Package]) REFERENCES [PartPackage]([Id]),

)
