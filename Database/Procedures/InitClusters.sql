CREATE PROCEDURE [dbo].[InitClusters]
	@count int
AS
BEGIN
	begin tran
		insert into ClusterNProfile(ClusterId, PropertyId, NominalId, Probability)
		select c.Id, p.Id, n.Id, RAND(CHECKSUM(NEWID())) from Cluster c, Properties p
			join Nominal n on p.Id = n.PropertyId

		update cp
		set Probability = Probability / gr.[Sum]
		from ClusterNProfile cp
			inner join (select ClusterId, PropertyId, sum(Probability) [Sum] from ClusterNProfile
					 group by ClusterId, PropertyId) gr on cp.ClusterId = gr.ClusterId and cp.PropertyId = gr.PropertyId
	commit
END