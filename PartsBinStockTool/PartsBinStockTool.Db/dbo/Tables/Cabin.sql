CREATE TABLE [dbo].[Cabin]
(
	[Id] INT NOT NULL PRIMARY KEY,    
    [SectionId] INT NULL,    
    [CoordinateX] INT NOT NULL, 
    [CoordinateY] INT NOT NULL,
    [Capacity] INT  NULL,
    CONSTRAINT [FK_Cabin_InventorySection] FOREIGN KEY ([SectionId]) REFERENCES [InventorySection]([Id])
)

GO

CREATE TRIGGER [dbo].[CheckCabinCoordinates]
ON [dbo].[Cabin]
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN InventorySection inv ON i.SectionId = inv.Id
        WHERE i.CoordinateX < 0 OR i.CoordinateX > inv.Width OR
              i.CoordinateY < 0 OR i.CoordinateY > inv.Height
    )
    BEGIN
        RAISERROR ('Cabin coordinates must be within the bounds of the corresponding inventory section.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;