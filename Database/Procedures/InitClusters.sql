CREATE PROCEDURE [dbo].[InitClusters]
	@count int
AS
BEGIN
	declare @clusters table (Id int)

	begin tran
		insert into [Cluster](Name)
		output inserted.Id INTO @clusters
		select distinct n = number 
		from master..[spt_values] 
		where number BETWEEN 1 AND @count

		insert into ClusterRProfile(ClusterId, PropertyId, Mean, Variance)
		select c.Id, p.Id, 0, 1000 from @clusters c, Properties p
		where p.IsNominal = 0

		ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] DISABLE

		insert into ClusterNProfile(ClusterId, PropertyId, NominalId, Probability)
		select c.Id, p.Id, n.Id, RAND(CHECKSUM(NEWID())) from @clusters c, Properties p
			join Nominal n on p.Id = n.PropertyId
		where p.IsNominal = 1

		update cp
		set Probability = Probability / gr.[Sum]
		from ClusterNProfile cp
			inner join (select ClusterId, PropertyId, sum(Probability) [Sum] from ClusterNProfile
					 group by ClusterId, PropertyId) gr on cp.ClusterId = gr.ClusterId and cp.PropertyId = gr.PropertyId

		ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] REBUILD
	commit
END