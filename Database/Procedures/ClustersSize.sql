CREATE PROCEDURE [dbo].[ClustersSize]
	@id int = null
AS
	SELECT ClusterId, SUM(Probability) Probability FROM UserProfile
	WHERE @id IS NULL OR ClusterId = @id
	GROUP BY ClusterId
