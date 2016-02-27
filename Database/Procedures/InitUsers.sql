CREATE PROCEDURE [dbo].[InitUsers]
	@learning int
AS
BEGIN
	declare @users table (Id int)

	begin tran
		insert into [User](Mac)
		output inserted.Id INTO @users
		select distinct l.Mac from [User] u
			right join LearningData(@learning) l on l.Mac = u.Mac
		where u.Id is null

		ALTER INDEX [PK_Profiles_View] ON [dbo].[Profiles_View] DISABLE

		insert into UserProfile(UserId, ClusterId, Probability)
		select u.Id, c.Id, RAND(CHECKSUM(NEWID()))
		from  Cluster c, @users u

		update up
		set Probability = Probability / gr.[Sum]
		from UserProfile up
			inner join (select u.Id UserId, sum(Probability) [Sum] from UserProfile up
				  inner join @users u on up.UserId = u.Id
				 group by u.Id) gr on up.UserId = gr.UserId

		ALTER INDEX [PK_Profiles_View] ON [dbo].[Profiles_View] REBUILD
	commit
END