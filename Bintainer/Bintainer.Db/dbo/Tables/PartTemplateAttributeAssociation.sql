CREATE TABLE [dbo].[PartTemplateAttributeAssociation]
(
	[TemplateId] INT NOT NULL,
	[AttributeTemplateId] INT NOT NULL,
	PRIMARY KEY (TemplateId, AttributeTemplateId),
	CONSTRAINT [FK_PartTemplateAttributeAssociation_PartTemplate] FOREIGN KEY ([TemplateId]) REFERENCES [PartTemplate]([Id]),
	CONSTRAINT [FK_PartTemplateAttributeAssociation_PartAttributeTemplate] FOREIGN KEY ([AttributeTemplateId]) REFERENCES [PartAttributeTemplate]([Id])
)
