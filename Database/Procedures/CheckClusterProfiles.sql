CREATE PROCEDURE [dbo].[CheckClusterProfiles]
AS
	select cp.ClusterId, p.Id, p.Name, Sum(Probability) ProbSum from ClusterProfile cp
		join Properties p on cp.PropertyId = p.Id
	where Probability is not null
	group by cp.ClusterId, p.Id, p.Name
	having Abs(Sum(Probability) - 1) > 1e-10
RETURN 0
