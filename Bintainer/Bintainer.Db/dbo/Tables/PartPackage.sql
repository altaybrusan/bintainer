CREATE TABLE [dbo].[PartPackage]
(	
    [Id] INT NOT NULL CONSTRAINT PK_PartPackage PRIMARY KEY IDENTITY(2000,1), 
	[Name] NCHAR(100) NULL Default('undefined'), 
	[Url] NVARCHAR(250) NULL,
	[FullFileName] NVARCHAR(250) NULL,
	[UserId] NVARCHAR (450) NOT NULL,
	CONSTRAINT [FK_PartPackage_AspNetUsers] FOREIGN KEY ([UserId])  REFERENCES [AspNetUsers]([Id])
)
