CREATE FUNCTION [dbo].[LearningData]
(
	@learning int
)
RETURNS TABLE AS RETURN
(
	SELECT d.* FROM Data d
	   join Learning l ON l.Id = @learning
    WHERE d.[Timestamp] between l.[From] and l.[To]
)
