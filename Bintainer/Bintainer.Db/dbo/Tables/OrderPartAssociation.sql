CREATE TABLE [dbo].[OrderPartAssociation]
(	
    [OrderId] INT NOT NULL,
    [PartId] INT NOT NULL,
    [Quantity] INT NULL DEFAULT 0,
    CONSTRAINT [PK_OrderPart] PRIMARY KEY ([OrderId], [PartId]),
    CONSTRAINT [FK_OrderPart_Order] FOREIGN KEY ([OrderId]) REFERENCES [Order]([Id]),
    CONSTRAINT [FK_OrderPart_Part] FOREIGN KEY ([PartId]) REFERENCES [Part]([Id])
)
