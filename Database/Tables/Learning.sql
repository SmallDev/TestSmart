CREATE TABLE [dbo].[Learning]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [From] TIME NOT NULL, 
    [To] TIME NOT NULL, 
    [StartLikelihood] FLOAT NULL, 
    [EndLikelihood] FLOAT NULL, 
    [Iterations] INT NOT NULL DEFAULT 0,
	[Timestamp] DATETIME NOT NULL DEFAULT GETDATE(), 
)
