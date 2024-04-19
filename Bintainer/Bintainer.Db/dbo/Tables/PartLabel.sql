CREATE TABLE [dbo].[PartLabel]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [PartId] INT NOT NULL, 
    [Value] NCHAR(50) NULL, 
    CONSTRAINT [FK_PartLabel_Part] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id])
)
