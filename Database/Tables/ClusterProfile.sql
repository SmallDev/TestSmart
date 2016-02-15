CREATE TABLE [dbo].[ClusterProfile]
(
	[ClusterId] INT NOT NULL , 
    [PropertyId] INT NOT NULL, 
    [NomValue] VARCHAR(10) NULL, 
    [Average] FLOAT NULL, 
    [Variance] FLOAT NULL, 
    PRIMARY KEY ([ClusterId], [PropertyId])
)
