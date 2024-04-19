CREATE TABLE [dbo].[BinSubspace]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Capacity] INT NULL DEFAULT 0, 
    [BinId] INT NULL, 
    [Label] NCHAR(100) NULL, 
    CONSTRAINT [FK_BinSubspace_Bin] FOREIGN KEY ([BinId]) REFERENCES [Bin]([Id])

)
