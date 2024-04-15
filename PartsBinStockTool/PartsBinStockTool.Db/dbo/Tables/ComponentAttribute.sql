CREATE TABLE [dbo].[ComponentAttribute]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [ComponentId] INT NOT NULL, 
    [Name] NCHAR(50) NULL, 
    [Value] NCHAR(150) NULL, 
    CONSTRAINT [FK_ComponentAttribute_Component] FOREIGN KEY ([ComponentId]) REFERENCES [Component]([Id])

)
