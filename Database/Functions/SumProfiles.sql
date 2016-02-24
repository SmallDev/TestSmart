CREATE FUNCTION [dbo].[SumProfiles]
(
)
RETURNS TABLE AS RETURN
(
	select up.UserId, cp.PropertyId, cp.NominalId, SUM(up.Probability * cp.Probability) Prob 
		from UserProfile up inner join ClusterProfile cp on up.ClusterId = cp.ClusterId
	group by up.UserId, cp.PropertyId, cp.NominalId
)
