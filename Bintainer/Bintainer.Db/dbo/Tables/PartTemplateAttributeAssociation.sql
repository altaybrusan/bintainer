CREATE TABLE [dbo].[PartTemplateAttributeAssociation]
(
	[PartTemplateId] INT NOT NULL,
	[AttributeTemplateId] INT NOT NULL,
	PRIMARY KEY (PartTemplateId, AttributeTemplateId),
	CONSTRAINT [FK_PartTemplateAttributeAssociation_PartTemplate] FOREIGN KEY ([PartTemplateId]) REFERENCES [PartTemplate]([Id]),
	CONSTRAINT [FK_PartTemplateAttributeAssociation_PartAttributeTemplate] FOREIGN KEY ([AttributeTemplateId]) REFERENCES [PartAttributeTemplate]([Id])
)
