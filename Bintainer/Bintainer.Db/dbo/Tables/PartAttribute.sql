CREATE TABLE [dbo].[PartAttribute]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [PartId] INT NOT NULL, 
    [Name] NCHAR(50) NULL, 
    [Value] NCHAR(150) NULL, 
    CONSTRAINT [FK_PartAttribute_Part] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id])

)
