CREATE TABLE [dbo].[Part]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(10001,1), 
    [Number] NCHAR(100) NOT NULL, 
    [Description] NCHAR(150) NULL, 
    [CategoryId] INT NULL,
    [PackageId] INT NULL,
    [Supplier] NVARCHAR(100) NOT NULL DEFAULT ('default'),
    [ImageUri] NVARCHAR(150) NULL, 
    [DatasheetUri] NVARCHAR(150) NULL,
    [SupplierUri] NVARCHAR(150) NULL,
    [UserId] NVARCHAR (450) NOT NULL,
    [OrderId] INT NULL,
    [TemplateId] INT NULL,
    [GuidId] UNIQUEIDENTIFIER DEFAULT NEWID(), 
    CONSTRAINT [FK_Part_PartCategory] FOREIGN KEY ([CategoryId]) REFERENCES [PartCategory]([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Part_PartPackage] FOREIGN KEY ([PackageId]) REFERENCES [PartPackage]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Part_PartAttributeTemplate] FOREIGN KEY ([TemplateId])  REFERENCES [PartAttributeTemplate]([Id])ON DELETE SET NULL,       
    CONSTRAINT [FK_Part_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([Id])ON DELETE NO ACTION,
)
GO
CREATE NONCLUSTERED INDEX IDX_Part_GuidId ON [dbo].[Part] ([GuidId]);
--CREATE UNIQUE INDEX [IX_Part_PackageId] ON [dbo].[Part]([PackageId])
