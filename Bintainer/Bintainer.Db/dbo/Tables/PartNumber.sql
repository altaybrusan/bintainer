CREATE TABLE [dbo].[PartNumber]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Supplier] NCHAR(100) NULL, 
    [Number] NCHAR(100) NULL, 
    [PartId] INT NOT NULL, 
    CONSTRAINT [FK_PartNumber_Part] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id]),


)
