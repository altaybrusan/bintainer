CREATE TABLE [dbo].[PartAttributeDefinition]
(
    [Id] INT NOT NULL CONSTRAINT PK_PartAttributeDefinition PRIMARY KEY IDENTITY(10000,1), 
    [Name] NVARCHAR(50) NULL, 
    [Value] NVARCHAR(150) NULL,
    [TemplateId] INT NOT NULL,
    [GuidId] UNIQUEIDENTIFIER DEFAULT NEWID(),
    CONSTRAINT [FK_PartAttributeDefinition_PartAttributeTemplate] FOREIGN KEY ([TemplateId]) REFERENCES [PartAttributeTemplate]([Id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX IDX_PartAttributeDefinition_GuidId ON [dbo].[PartAttributeDefinition] ([GuidId]);