﻿CREATE PROCEDURE [dbo].[LearnIterate]
	@learning int
AS
BEGIN
	declare @H table(UserId int, ClusterId int, PropertyId int, NominalId int, Propability float)
	insert into @H(UserId, ClusterId, PropertyId, NominalId, Propability)
	select up.UserId, cp.ClusterId, cp.PropertyId, cp.NominalId, up.Probability * cp.Probability / ll.Probability
	from
		dbo.UnpivotData(@learning) unp
		inner join [User] u on u.Mac = unp.MAC
		inner join [Nominal] n on unp.PropertyId = n.PropertyId and unp.Value = n.Value
		inner join UserProfile up on u.Id = up.UserId
		inner join ClusterProfile cp on cp.ClusterId = up.ClusterId and cp.PropertyId = n.PropertyId and cp.NominalId = n.Id
		inner hash join Profiles_View ll WITH (NOEXPAND) on ll.UserId = up.UserId and cp.PropertyId = ll.PropertyId and cp.NominalId = ll.NominalId

	begin tran
		ALTER INDEX [PK_Profiles_View] ON [dbo].[Profiles_View] DISABLE

		update up set Probability = np.Propability
		from UserProfile up
			inner hash join (select h1.UserId, h1.ClusterId, SUM(h1.Propability) / h2.Propability as Propability from @H h1    
							inner join (select UserId, SUM(Propability) Propability from @H group by UserId) h2 on h1.UserId = h2.UserId
						group by h1.UserId, h1.ClusterId, h2.Propability) np 
				on np.UserId = up.UserId and np.ClusterId = up.ClusterId

		update cp set Probability = np.Probability
		from ClusterProfile cp
			inner hash join (select h1.ClusterId, h1.PropertyId, h1.NominalId, SUM(h1.Propability)/h2.Propability as Probability from @H h1    
							inner join (select ClusterId, PropertyId, SUM(Propability) Propability from @H group by ClusterId, PropertyId) h2 
							on h1.ClusterId = h2.ClusterId and h1.PropertyId = h2.PropertyId
						group by h1.ClusterId, h1.PropertyId, h1.NominalId, h2.Propability) np
				on np.ClusterId = cp.ClusterId and np.PropertyId = cp.PropertyId and np.NominalId = cp.NominalId

		ALTER INDEX [PK_Profiles_View] ON [dbo].[Profiles_View] REBUILD
	commit
END