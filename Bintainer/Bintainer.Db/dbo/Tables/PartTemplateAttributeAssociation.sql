CREATE TABLE [dbo].[PartTemplateAttributeAssociation]
(
	[TemplateId] INT NOT NULL,
	[AttributeId] INT NOT NULL,
	PRIMARY KEY (TemplateId, AttributeId),
	FOREIGN KEY ([TemplateId]) REFERENCES [PartTemplate]([Id]),
	FOREIGN KEY ([AttributeId]) REFERENCES [PartAttribute]([Id])
)
