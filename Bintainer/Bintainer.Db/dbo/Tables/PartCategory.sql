CREATE TABLE [dbo].[PartCategory]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name] NCHAR(75) NULL, 
    [ParentCategoryId] INT NULL,   
    
    CONSTRAINT [FK_PartCategory_ParentCategory] FOREIGN KEY ([ParentCategoryId]) REFERENCES [PartCategory]([Id])
)
