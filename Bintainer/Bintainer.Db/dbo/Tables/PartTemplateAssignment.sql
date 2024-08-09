CREATE TABLE [dbo].[PartTemplateAssignment]
(
	[PartId] INT NOT NULL,
	[PartTemplateId] INT NOT NULL,
	PRIMARY KEY (PartId, PartTemplateId),
	FOREIGN KEY ([PartId]) REFERENCES [Part]([Id]),
	FOREIGN KEY ([PartTemplateId]) REFERENCES [PartTemplate]([Id])
)
