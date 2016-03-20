CREATE FUNCTION [dbo].[CalcEvidence] 
(
	@UserId int,
	@PropertyId int,
	@NValue char = null,
	@RValue float = null
)
RETURNS float
AS
BEGIN
	declare @result float = null

	if (@NValue is not null)
	begin
		select @result = Probability from Profiles_NView ll WITH (NOEXPAND)
			join Nominal n on ll.NominalId = n.Id
		where ll.UserId = @UserId and ll.PropertyId = @PropertyId and n.Value = @NValue
	end

	else if (@RValue is not null)
	begin
		select @result = SUM(up.Probability * [dbo].[NormalDensity](@RValue, cp.Mean, cp.Variance)) from ClusterRProfile cp
			join UserProfile up on cp.ClusterId = up.ClusterId
		where up.UserId = @UserId and cp.PropertyId = @PropertyId
	end

	return @result
END