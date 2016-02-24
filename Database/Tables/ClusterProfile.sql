﻿CREATE TABLE [dbo].[ClusterProfile]
(
	[ClusterId] INT NOT NULL , 
    [PropertyId] INT NOT NULL, 
    [NominalId] INT NOT NULL , 
	[Probability] FLOAT NULL, 
    [Average] FLOAT NULL, 
    [Variance] FLOAT NULL,     
    PRIMARY KEY ([ClusterId], [PropertyId], [NominalId]), 
    CONSTRAINT [FK_ClusterProfile_ToCluster] FOREIGN KEY ([ClusterId]) REFERENCES [Cluster]([Id]), 
    CONSTRAINT [FK_ClusterProfile_ToProperty] FOREIGN KEY ([PropertyId]) REFERENCES [Properties]([Id]), 
    CONSTRAINT [FK_ClusterProfile_ToNominal] FOREIGN KEY ([NominalId]) REFERENCES [Nominal]([Id])
)
