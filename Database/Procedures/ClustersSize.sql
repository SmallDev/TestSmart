CREATE PROCEDURE [dbo].[ClustersSize]
	@id int = null
AS
	SELECT ClusterId, SUM(Probability) Probability FROM UserProfile with(nolock)
	WHERE @id IS NULL OR ClusterId = @id
	GROUP BY ClusterId
