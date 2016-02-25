CREATE FUNCTION [dbo].[CalcLogLikelihood]
(
	@learning int
)
RETURNS FLOAT
AS
BEGIN
	RETURN 
	(select SUM(-LOG(Probability)) from
		UnpivotData(@learning) unp
		inner join [User] u on u.Mac = unp.MAC
		inner join [Nominal] n on unp.PropertyId = n.PropertyId and unp.Value = n.Value
		inner merge join Profiles_View ll WITH (NOEXPAND)
	on ll.UserId = u.Id and ll.PropertyId = n.PropertyId and ll.NominalId = n.Id)
END