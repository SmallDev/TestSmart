CREATE TABLE [dbo].[ClusterSize]
(
	[LearningId] INT NOT NULL , 
    [ClusterId] INT NOT NULL, 
    [Size] FLOAT NOT NULL, 
    PRIMARY KEY ([LearningId], [ClusterId]), 
    CONSTRAINT [FK_ClusterSize_Learning] FOREIGN KEY ([LearningId]) REFERENCES dbo.[Learning]([Id]), 
    CONSTRAINT [FK_ClusterSize_Cluster] FOREIGN KEY ([ClusterId]) REFERENCES dbo.[Cluster]([Id])
)
