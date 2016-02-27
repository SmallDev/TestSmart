CREATE TABLE [dbo].[Data]
(
	[Id] BIGINT NOT NULL IDENTITY,
	[Timestamp] TIME NOT NULL , 
    [Mac] CHAR(17) NOT NULL, 
    [MessageType] CHAR(1) NULL, 
    [StreamType] CHAR NULL, 
    [ReceivedRate] FLOAT NULL, 
    PRIMARY KEY ([Id])
)
