CREATE TABLE [dbo].[PartAttributeTemplateAssociation]
(
	[PartId] INT NOT NULL,
    [AttributeTemplateId] INT NOT NULL,
    CONSTRAINT [PK_Part_PartAttributeTemplate] PRIMARY KEY ([PartId], [AttributeTemplateId]),
    CONSTRAINT [FK_Part_PartAttributeTemplate_Part] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id]),
    CONSTRAINT [FK_Part_PartAttributeTemplate_PartAttributeTemplate] FOREIGN KEY ([AttributeTemplateId]) REFERENCES [PartAttributeTemplate]([Id]) ON DELETE SET NULL

)
