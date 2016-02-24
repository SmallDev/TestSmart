CREATE PROCEDURE [dbo].[InitUsers]
	@learning int
AS
BEGIN
	declare @users table (Id int)

	begin tran
		insert into [User](Mac)
		output inserted.Id INTO @users
		select distinct d.MAC from [User] u
			right join Data d on u.Mac = d.MAC
			join Learning l on l.Id = @learning
		where d.Timestamp between l.[From] and l.[To] and u.Id is null

		insert into UserProfile(UserId, ClusterId, Probability)
		select u.Id, c.Id, RAND(CHECKSUM(NEWID()))
		from  Cluster c, @users u

		update up
		set Probability = Probability / gr.[Sum]
		from UserProfile up
			inner join (select u.Id UserId, sum(Probability) [Sum] from UserProfile up
				  inner join @users u on up.UserId = u.Id
				 group by u.Id) gr on up.UserId = gr.UserId
	commit
END