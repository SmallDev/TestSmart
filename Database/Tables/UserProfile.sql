CREATE TABLE [dbo].[UserProfile]
(
	[UserId] INT NOT NULL , 
    [ClusterId] INT NOT NULL, 
    [Probability] FLOAT NOT NULL, 
    PRIMARY KEY ([UserId], [ClusterId]), 
    CONSTRAINT [FK_UserProfile_ToUser] FOREIGN KEY (UserId) REFERENCES [dbo].[User]([Id]), 
    CONSTRAINT [FK_UserProfile_ToCluster] FOREIGN KEY (ClusterId) REFERENCES [dbo].[Cluster]([Id])
)
