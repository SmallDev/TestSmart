﻿CREATE TABLE [dbo].[Properties]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Code] VARCHAR(50) NOT NULL, 
    [IsNominal] BIT NULL 
)
