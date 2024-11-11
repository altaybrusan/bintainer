CREATE TABLE [dbo].[PartAttributeTemplate]
(
	[Id] INT NOT NULL CONSTRAINT PK_PartAttributeTemplate PRIMARY KEY IDENTITY(10000,1), 
    [TemplateName] NCHAR(50) NOT NULL,
	[UserId] NVARCHAR (450) NOT NULL,
	[GuidId] UNIQUEIDENTIFIER DEFAULT NEWID(), 
    CONSTRAINT [FK_PartAttributeTemplate_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([ID])

)
GO
CREATE NONCLUSTERED INDEX IDX_PartAttributeTemplate_GuidId ON [dbo].[PartAttributeTemplate] ([GuidId]);

