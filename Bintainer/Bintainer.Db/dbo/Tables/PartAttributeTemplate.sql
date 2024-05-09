CREATE TABLE [dbo].[PartAttributeTemplate]
(
	[Id] INT NOT NULL CONSTRAINT PK_PartAttributeTemplate PRIMARY KEY IDENTITY(10000,1), 
    [TemplateName] NCHAR(50) NULL,  
)
