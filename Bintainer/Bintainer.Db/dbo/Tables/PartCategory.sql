CREATE TABLE [dbo].[PartCategory]
(
	[Id] INT NOT NULL CONSTRAINT PK_PartCategory PRIMARY KEY IDENTITY(10000,1), 
    [Name] NCHAR(75) NULL, 
    [ParentCategoryId] INT NULL,   
    
    CONSTRAINT [FK_PartCategory_ParentCategory] FOREIGN KEY ([ParentCategoryId]) REFERENCES [PartCategory]([Id])
)
