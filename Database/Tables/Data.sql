CREATE TABLE [dbo].[Data]
(
	[Id] BIGINT NOT NULL IDENTITY,
	[Timestamp] TIME NOT NULL , 
    [MAC] CHAR(17) NOT NULL, 
    [MessageType] CHAR(1) NULL, 
    [ContentType] CHAR NULL, 
    PRIMARY KEY ([Id])
)
