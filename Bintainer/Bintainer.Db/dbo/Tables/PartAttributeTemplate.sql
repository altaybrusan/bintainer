CREATE TABLE [dbo].[PartAttributeTemplate]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [TemplateName] NCHAR(50) NULL, 
    [AttributeId] INT NOT NULL, 
    CONSTRAINT [FK_PartAttributeTemplate_PartAttribute] FOREIGN KEY ([AttributeId]) REFERENCES [PartAttribute]([Id]),     
)
