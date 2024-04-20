CREATE TABLE [dbo].[PartTemplate]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1000,1),	
    [Supplier] NCHAR(100) NULL, 
    [PartNumber] NCHAR(150) NULL, 
    [ImageUri] NCHAR(100) NULL, 
    [DatasheetUri] NCHAR(100) NULL    
)
