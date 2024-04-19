CREATE TABLE [dbo].[Component]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(10001,1), 
    [Name] NCHAR(60) NOT NULL, 
    [Description] NCHAR(150) NULL, 
    [Category] NCHAR(100) NULL, 
    [Count] INT NULL, 
    [InventoryId] INT NOT NULL,
    CONSTRAINT [FK_Component_Inventory] FOREIGN KEY ([InventoryId]) REFERENCES [Inventory]([Id]),   
)
