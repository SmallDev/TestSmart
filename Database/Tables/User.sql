CREATE TABLE [dbo].[User]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Mac] CHAR(17) NOT NULL
)

GO

CREATE INDEX [IX_User_Mac] ON [dbo].[User] ([Mac] ASC)
