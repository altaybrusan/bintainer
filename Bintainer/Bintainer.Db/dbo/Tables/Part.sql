CREATE TABLE [dbo].[Part]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(10001,1), 
    [Name] NCHAR(100) NOT NULL, 
    [Description] NCHAR(150) NULL, 
    [CategoryId] INT NULL,
    [PackageId] INT NOT NULL,
    [Supplier] NCHAR(100) NOT NULL DEFAULT ('default'),
    [ImageUri] NCHAR(150) NULL, 
    [DatasheetUri] NCHAR(150) NULL,
    [SupplierUri] NCHAR(150) NULL,
    [UserId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [FK_Part_PartCategory] FOREIGN KEY ([CategoryId]) REFERENCES [PartCategory]([Id]),
    CONSTRAINT [FK_Part_PartPackage] FOREIGN KEY ([PackageId]) REFERENCES [PartPackage]([Id]),
    CONSTRAINT [FK_Part_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([Id]),       
)
GO

CREATE UNIQUE INDEX [IX_Part_PackageId] ON [dbo].[Part]([PackageId])
