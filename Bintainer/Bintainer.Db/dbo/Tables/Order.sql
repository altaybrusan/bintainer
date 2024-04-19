CREATE TABLE [dbo].[Order]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [PartId] INT NOT NULL, 
    [Qunatity] INT NULL DEFAULT 0, 
    [DateTime] DATETIME NULL, 
    [Number] NCHAR(50) NULL

)
