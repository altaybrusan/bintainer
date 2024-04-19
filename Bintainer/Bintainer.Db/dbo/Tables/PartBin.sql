CREATE TABLE [dbo].[PartBin]
(	
    [PartId] INT NOT NULL, 
    [BinId] INT NOT NULL, 
    CONSTRAINT [PK_PartBin] PRIMARY KEY ([PartId], [BinId]),
    CONSTRAINT [FK_PartBin_Bin] FOREIGN KEY ([BinId]) REFERENCES [Bin]([Id]),
    CONSTRAINT [FK_PartBin_Component] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id])

)
