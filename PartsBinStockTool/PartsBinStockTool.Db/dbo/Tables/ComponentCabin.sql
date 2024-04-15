CREATE TABLE [dbo].[ComponentCabin]
(	
    [ComponentId] INT NOT NULL, 
    [CabinId] INT NOT NULL, 
    CONSTRAINT [PK_ComponentCabin] PRIMARY KEY ([ComponentId], [CabinId]),
    CONSTRAINT [FK_ComponentCabin_Cabin] FOREIGN KEY ([CabinId]) REFERENCES [Cabin]([Id]),
    CONSTRAINT [FK_ComponentCabin_Component] FOREIGN KEY ([ComponentId]) REFERENCES [Component]([Id])

)
