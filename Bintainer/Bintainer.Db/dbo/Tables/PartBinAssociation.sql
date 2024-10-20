CREATE TABLE [dbo].[PartBinAssociation]
(	
    [PartId] INT NOT NULL, 
    [BinId] INT NOT NULL,
    [SubspaceId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    CONSTRAINT [PK_PartBin] PRIMARY KEY ([PartId], [BinId],[SubspaceId]),
    CONSTRAINT [FK_PartBin_Bin] FOREIGN KEY ([BinId]) REFERENCES [Bin]([Id]),
    CONSTRAINT [FK_PartBin_Component] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id]),
    CONSTRAINT [FK_PartBin_Subspace] FOREIGN KEY ([SubspaceId]) REFERENCES [BinSubspace]([Id])

)
