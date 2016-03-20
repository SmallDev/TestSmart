CREATE TABLE [dbo].[ClusterRProfile]
(
	[ClusterId] INT NOT NULL , 
    [PropertyId] INT NOT NULL, 
    [Mean] FLOAT NOT NULL DEFAULT 0, 
    [Variance] FLOAT NOT NULL DEFAULT 1,     
    PRIMARY KEY ([ClusterId], [PropertyId]), 
    CONSTRAINT [FK_ClusterRProfile_ToCluster] FOREIGN KEY ([ClusterId]) REFERENCES [Cluster]([Id]), 
    CONSTRAINT [FK_ClusterRProfile_ToProperty] FOREIGN KEY ([PropertyId]) REFERENCES [Properties]([Id])
)
