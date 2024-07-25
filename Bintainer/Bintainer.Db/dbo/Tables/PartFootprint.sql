CREATE TABLE [dbo].[PartFootprint]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name] NCHAR(100) NULL,
	[UserId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [FK_PartFootprint_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([ID])


)
