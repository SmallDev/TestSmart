CREATE PROCEDURE [dbo].[LearnIterate]
	@learning int
AS
BEGIN
	declare @H table(UserId int, ClusterId int, PropertyId int, NominalId int, Propability float)
	insert into @H(UserId, ClusterId, PropertyId, NominalId, Propability)
	select H.UserId, H.ClusterId, H.PropertyId, H.NominalId, Prob
	from
		dbo.UnpivotData(@learning) unp
		inner join [User] u on u.Mac = unp.MAC
		inner join [Nominal] n on unp.PropertyId = n.PropertyId and unp.Value = n.Value
		inner hash join (select up.UserId, cp.PropertyId, cp.NominalId, cp.ClusterId, up.Probability * cp.Probability / ll.Prob as Prob 
						from UserProfile up
						   inner join ClusterProfile cp on cp.ClusterId = up.ClusterId
						   inner hash join dbo.SumProfiles() ll 
							  on ll.UserId = up.UserId and cp.PropertyId = ll.PropertyId and cp.NominalId = ll.NominalId) H 
		   on H.UserId = u.Id and H.PropertyId = n.PropertyId and H.NominalId = n.Id

	begin tran
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
	commit
END