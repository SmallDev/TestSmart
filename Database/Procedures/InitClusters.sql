CREATE PROCEDURE [dbo].[InitClusters]
	@count int
AS
BEGIN
	begin tran
		insert into ClusterProfile(ClusterId, PropertyId, NominalId, Probability)
		select c.Id, p.Id, n.Id, RAND(CHECKSUM(NEWID())) from Cluster c, Properties p
			join Nominal n on p.Id = n.PropertyId

		update cp
		set Probability = Probability / gr.[Sum]
		from ClusterProfile cp
			inner join (select ClusterId, PropertyId, sum(Probability) [Sum] from ClusterProfile
					 group by ClusterId, PropertyId) gr on cp.ClusterId = gr.ClusterId and cp.PropertyId = gr.PropertyId
	commit
END