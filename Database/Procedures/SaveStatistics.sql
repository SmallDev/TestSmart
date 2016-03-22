CREATE PROCEDURE [dbo].[SaveStatistics]
	@learning int
AS
	delete from dbo.ClusterSize
	where LearningId = @learning
	
	insert into dbo.ClusterSize(LearningId, ClusterId, Size)
	SELECT @learning, ClusterId, SUM(Probability) FROM UserProfile with(nolock)
	GROUP BY ClusterId