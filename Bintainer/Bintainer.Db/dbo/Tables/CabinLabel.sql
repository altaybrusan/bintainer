CREATE TABLE [dbo].[CabinLabel]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Value] NCHAR(50) NULL, 
    [CabinId] INT NOT NULL, 
    CONSTRAINT [FK_CabinLabel_Cabin] FOREIGN KEY ([Id]) REFERENCES [Cabin]([Id])
)
