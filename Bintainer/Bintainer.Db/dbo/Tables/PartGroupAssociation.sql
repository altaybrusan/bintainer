CREATE TABLE [dbo].[PartGroupAssociation]
(
	[PartId] INT NOT NULL,
    [GroupId] INT NOT NULL,
    CONSTRAINT [PK_Part_PartGroup] PRIMARY KEY ([PartId], [GroupId]),
    CONSTRAINT [FK_Part_PartGroup_Part] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id]),
    CONSTRAINT [FK_Part_PartGroup_PartGroup] FOREIGN KEY ([GroupId]) REFERENCES [PartGroup]([Id])
)
