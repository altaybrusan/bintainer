CREATE TABLE [dbo].[PartCategory]
(
	[Id] INT NOT NULL CONSTRAINT PK_PartCategory PRIMARY KEY IDENTITY(10000,1), 
    [Name] NVARCHAR(75) NULL, 
    [ParentCategoryId] INT NULL,   
    [UserId] NVARCHAR (450) NOT NULL,
    [GuidId] UNIQUEIDENTIFIER DEFAULT NEWID(),
    CONSTRAINT [FK_PartCategory_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([ID]),
    CONSTRAINT [FK_PartCategory_ParentCategory] FOREIGN KEY ([ParentCategoryId]) REFERENCES [PartCategory]([Id])
)
