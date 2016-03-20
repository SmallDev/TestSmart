CREATE PROCEDURE [dbo].[LearnIterate]
	@learning int
AS
BEGIN
	declare @H table(UserId int, ClusterId int, PropertyId int, NominalId int, Probability float, Value float)
	insert into @H(UserId, ClusterId, PropertyId, NominalId, Probability, Value)
	select up.UserId, up.ClusterId, unp.PropertyId, n.Id,
		up.Probability * ISNULL(ISNULL(cnp.Probability, dbo.NormalDensity(unp.RValue, crp.Mean, crp.Variance)) / 
								NULLIF(dbo.CalcEvidence(u.Id, unp.PropertyId, unp.NValue, unp.RValue), 0), 0),
		unp.RValue
		from
			dbo.UnpivotData(1) unp
			inner join [User] u on u.Mac = unp.Mac
			inner join UserProfile up on u.Id = up.UserId
			left join [Nominal] n on unp.PropertyId = n.PropertyId and unp.NValue = n.Value
			left join ClusterNProfile cnp on cnp.ClusterId = up.ClusterId and cnp.PropertyId = unp.PropertyId and cnp.NominalId = n.Id
			left join ClusterRProfile crp on crp.ClusterId = up.ClusterId and crp.PropertyId = unp.PropertyId

	begin tran
		ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] DISABLE

		update up set Probability = np.Probability
		from UserProfile up
			inner hash join (select h1.UserId, h1.ClusterId, SUM(h1.Probability) / h2.Probability as Probability from @H h1    
							inner join (select UserId, SUM(Probability) Probability from @H group by UserId) h2 on h1.UserId = h2.UserId
						group by h1.UserId, h1.ClusterId, h2.Probability) np 
				on np.UserId = up.UserId and np.ClusterId = up.ClusterId

		update cp set Probability = np.Probability
		from ClusterNProfile cp
			inner hash join (select h1.ClusterId, h1.PropertyId, h1.NominalId, SUM(h1.Probability)/h2.Probability as Probability from @H h1    
							inner join (select ClusterId, PropertyId, SUM(Probability) Probability from @H group by ClusterId, PropertyId) h2 
							on h1.ClusterId = h2.ClusterId and h1.PropertyId = h2.PropertyId
						group by h1.ClusterId, h1.PropertyId, h1.NominalId, h2.Probability) np
				on np.ClusterId = cp.ClusterId and np.PropertyId = cp.PropertyId and np.NominalId = cp.NominalId
		
		update cp set Mean = MeanSum/[Sum], Variance = VarianceSum/[Sum] - SQUARE(MeanSum/[Sum])		
		from ClusterRProfile cp
			inner hash join (select ClusterId, PropertyId, SUM(Probability) [Sum], SUM(Value * Probability) MeanSum,
								SUM(Value * Value * Probability) VarianceSum from @H group by ClusterId, PropertyId ) np
				on np.ClusterId = cp.ClusterId and np.PropertyId = cp.PropertyId

		ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] REBUILD
	commit
END