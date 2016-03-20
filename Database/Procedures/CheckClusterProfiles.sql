CREATE PROCEDURE [dbo].[CheckClusterProfiles]
AS
	select cp.ClusterId, p.Id, p.Code, Sum(Probability) ProbSum from ClusterNProfile cp
		join Properties p on cp.PropertyId = p.Id
	where Probability is not null
	group by cp.ClusterId, p.Id, p.Code
	having Abs(Sum(Probability) - 1) > 1e-10
RETURN 0
