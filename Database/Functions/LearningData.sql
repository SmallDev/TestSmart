CREATE FUNCTION [dbo].[LearningData]
(
	@learning int
)
RETURNS TABLE AS RETURN
(
	SELECT d.* FROM Learning l
	   inner join Data d with(nolock) on l.Id = @learning
    WHERE d.[Timestamp] between l.[From] and l.[To]
)
