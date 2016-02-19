CREATE TABLE [dbo].[ClusterProfile]
(
	[ClusterId] INT NOT NULL , 
    [PropertyId] INT NOT NULL, 
    [NominalId] INT NULL, 
    [Average] FLOAT NULL, 
    [Variance] FLOAT NULL, 
    PRIMARY KEY ([ClusterId], [PropertyId]), 
    CONSTRAINT [FK_ClusterProfile_ToCluster] FOREIGN KEY ([ClusterId]) REFERENCES [Cluster]([Id]), 
    CONSTRAINT [FK_ClusterProfile_ToProperty] FOREIGN KEY ([PropertyId]) REFERENCES [Properties]([Id]), 
    CONSTRAINT [FK_ClusterProfile_ToNominal] FOREIGN KEY ([NominalId]) REFERENCES [Nominal]([Id])
)
