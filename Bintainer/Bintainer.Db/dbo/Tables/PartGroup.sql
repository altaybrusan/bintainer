CREATE TABLE [dbo].[PartGroup]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [PartId] INT NOT NULL, 
    [Name] NCHAR(10) NULL, 
    CONSTRAINT [FK_PartGroup_Part] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id])

)
