CREATE TABLE [dbo].[PartAttribute]
(
	[Id] INT NOT NULL CONSTRAINT PK_PartAttribute PRIMARY KEY IDENTITY(10000,1), 
    [Name] NCHAR(50) NULL, 
    [Value] NCHAR(150) NULL,
    [TemplateId] INT NOT NULL,
    CONSTRAINT [FK_PartAttribute_PartAttributeTemplate] FOREIGN KEY ([TemplateId]) REFERENCES [PartAttributeTemplate]([Id]),    
)
