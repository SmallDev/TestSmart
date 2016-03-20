CREATE FUNCTION [dbo].[NormalDensity]
(
	@x float,
	@mean float,
	@variance float
)
RETURNS FLOAT
AS
BEGIN
	RETURN EXP(-0.5*(@x-@mean)*(@x-@mean) / @variance) /(SQRT(2.*PI()*@variance))
END