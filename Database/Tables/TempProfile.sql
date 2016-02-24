CREATE TABLE [dbo].[TempProfile]
(
    [UserId] INT NOT NULL, 
    [ClusterId] INT NOT NULL, 
    [PropertyId] INT NOT NULL, 
    [NominalId] INT NOT NULL, 
    [Average] FLOAT NULL, 
    [Variance] FLOAT NULL, 
    CONSTRAINT [PK_TempProfile] PRIMARY KEY ([UserId], [ClusterId], [PropertyId], [NominalId])
)
