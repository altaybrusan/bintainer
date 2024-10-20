CREATE TABLE [dbo].[BinSubspace]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(2000,1),    
    [BinId] INT NULL,
    [SubspaceIndex] INT NULL,
    [Label] NCHAR(100) NULL,
    CONSTRAINT [FK_BinSubspace_Bin] FOREIGN KEY ([BinId]) REFERENCES [Bin]([Id])

)
