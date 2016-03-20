CREATE PROCEDURE [dbo].[InitClusters]
	@count int
AS
BEGIN
	begin tran
		insert into ClusterNProfile(ClusterId, PropertyId, NominalId, Probability)
		select c.Id, p.Id, n.Id, RAND(CHECKSUM(NEWID())) from Cluster c, Properties p
			join Nominal n on p.Id = n.PropertyId
		where p.IsNominal = 1

		ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] DISABLE

		update cp
		set Probability = Probability / gr.[Sum]
		from ClusterNProfile cp
			inner join (select ClusterId, PropertyId, sum(Probability) [Sum] from ClusterNProfile
					 group by ClusterId, PropertyId) gr on cp.ClusterId = gr.ClusterId and cp.PropertyId = gr.PropertyId

		insert into ClusterRProfile(ClusterId, PropertyId, Mean, Variance)
		select c.Id, p.Id, 0, 1 from Cluster c, Properties p
		where p.IsNominal = 0

		ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] REBUILD
	commit
END