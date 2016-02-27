CREATE TABLE [dbo].[ClusterNProfile]
(
	[ClusterId] INT NOT NULL , 
    [PropertyId] INT NOT NULL, 
    [NominalId] INT NOT NULL , 
	[Probability] FLOAT NULL, 
    PRIMARY KEY ([ClusterId], [PropertyId], [NominalId]), 
    CONSTRAINT [FK_ClusterNProfile_ToCluster] FOREIGN KEY ([ClusterId]) REFERENCES [Cluster]([Id]), 
    CONSTRAINT [FK_ClusterNProfile_ToProperty] FOREIGN KEY ([PropertyId]) REFERENCES [Properties]([Id]), 
    CONSTRAINT [FK_ClusterNProfile_ToNominal] FOREIGN KEY ([NominalId]) REFERENCES [Nominal]([Id])
)
