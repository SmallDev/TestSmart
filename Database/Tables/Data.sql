CREATE TABLE [dbo].[Data]
(
	[Id] BIGINT NOT NULL IDENTITY,
	[Timestamp] TIME NOT NULL , 
    [Mac] CHAR(17) NOT NULL, 
    [MessageType] CHAR(1) NULL, 
    [StreamType] CHAR NULL, 
    [ReceivedRate] FLOAT NULL, 
    [LinkFaultsRate] FLOAT NULL, 
    [LostRate] FLOAT NULL, 
    [RestoredRate] FLOAT NULL, 
    [OverflowRate] FLOAT NULL, 
    [UnderflowRate] FLOAT NULL, 
    [DelayFactor] FLOAT NULL, 
    [MediaLossRate] FLOAT NULL, 
    PRIMARY KEY ([Id])
)
