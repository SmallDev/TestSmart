CREATE FUNCTION [dbo].[ClusterDistribution]
(
	@userId int
)
RETURNS @table TABLE
(
	UserId int,
	ClusterId int,
	Prob float
)
AS
BEGIN
	declare @userProb table(ClusterId int, Prob float)
    insert into @userProb
    select Id, Value from Cluster, Random

    insert into @table
    select @userId, ClusterId, Prob/(select Sum(Prob) from @userProb) from @userProb

	return
END
