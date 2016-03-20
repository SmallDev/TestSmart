CREATE TABLE [dbo].[Learning]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [From] TIME NOT NULL, 
    [To] TIME NOT NULL, 
    [StartLikelihood] FLOAT NULL, 
    [EndLikelihood] FLOAT NULL, 
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
)
