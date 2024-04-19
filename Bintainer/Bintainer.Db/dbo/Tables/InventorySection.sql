CREATE TABLE [dbo].[InventorySection]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [SectionName] NCHAR(150) NULL, 
    [Width] INT NULL, 
    [Height] INT NULL, 
    [InventoryId] INT NOT NULL,
    CONSTRAINT [FK_InventorySection_Inventory] FOREIGN KEY ([InventoryId]) REFERENCES [Inventory]([Id]),
)