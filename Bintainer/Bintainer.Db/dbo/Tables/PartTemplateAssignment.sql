CREATE TABLE [dbo].[PartTemplateAssignment]
(
	[PartId] INT NOT NULL,
	[TemplateId] INT NOT NULL,
	PRIMARY KEY (PartId, TemplateId),
	FOREIGN KEY ([PartId]) REFERENCES [Part]([Id]),
	FOREIGN KEY ([TemplateId]) REFERENCES [PartTemplate]([Id])
)
