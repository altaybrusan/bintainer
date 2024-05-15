CREATE TABLE [dbo].[InventorySection]
(
	[Id] INT NOT NULL CONSTRAINT PK_InventorySection PRIMARY KEY IDENTITY(1000,1), 
    [SectionName] NCHAR(150) NULL, 
    [Width] INT NULL, 
    [Height] INT NULL, 
    [InventoryId] INT NOT NULL,
    [Subsection] INT NULL, 
    CONSTRAINT [FK_InventorySection_Inventory] FOREIGN KEY ([InventoryId]) REFERENCES [Inventory]([Id]),
)