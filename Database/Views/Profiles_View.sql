CREATE VIEW [dbo].[Profiles_View]
WITH SCHEMABINDING
AS
	SELECT	up.UserId
			, cp.PropertyId
			, cp.NominalId
			, SUM(IsNULL(up.Probability * cp.Probability, 0)) Probability
			, COUNT_BIG(*) as COUNT
	   FROM dbo.ClusterProfile cp inner join dbo.UserProfile up on up.ClusterId = cp.ClusterId
    GROUP BY up.UserId, cp.PropertyId, cp.NominalId
GO

CREATE UNIQUE CLUSTERED INDEX [PK_Profiles_View] ON [dbo].[Profiles_View] (
	[UserId] ASC,
	[PropertyId] ASC,
	[NominalId] ASC
)
GO