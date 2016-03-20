CREATE FUNCTION [dbo].[CalcLogLikelihood]
(
	@learning int
)
RETURNS FLOAT
AS
BEGIN
	RETURN 
	(select SUM(-LOG(nullif(dbo.CalcEvidence(u.Id, unp.PropertyId, unp.NValue, unp.RValue), 0)))  from
	UnpivotData(@learning) unp
	inner join [User] u with(nolock) on u.Mac = unp.Mac)
END