CREATE PROCEDURE [dbo].[InitUsers]
	@learning int
AS
BEGIN
	declare @users table (Id int)

	begin tran
		insert into [User](Mac)
		output inserted.Id INTO @users
		select distinct d.Mac from [User] u
			right join Data d on u.Mac = d.MAC
			join Learning l on l.Id = @learning
		where d.Timestamp between l.[From] and l.[To] and u.Id is null

		insert into UserProfile(UserId, ClusterId, Probability)
		select d.UserId, d.ClusterId, d.Prob from @users u
			cross apply ClusterDistribution(u.Id) d
	commit
END