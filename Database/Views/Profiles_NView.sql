CREATE VIEW [dbo].[Profiles_NView]
WITH SCHEMABINDING
AS
	SELECT	up.UserId
			, cp.PropertyId
			, cp.NominalId
			, SUM(ISNULL(up.Probability * cp.Probability, 0)) Probability
			, COUNT_BIG(*) as COUNT
	   FROM dbo.ClusterNProfile cp inner join dbo.UserProfile up on up.ClusterId = cp.ClusterId
    GROUP BY up.UserId, cp.PropertyId, cp.NominalId
GO

CREATE UNIQUE CLUSTERED INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] (
	[UserId] ASC,
	[PropertyId] ASC,
	[NominalId] ASC
)
GO