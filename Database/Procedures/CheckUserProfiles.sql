CREATE PROCEDURE [dbo].[CheckUserProfiles]
AS
	select u.Id, u.Mac, Sum(Probability) ProbSum from UserProfile up
		join [User] u on up.UserId = u.Id
	group by u.Id, u.Mac
	having Abs(Sum(Probability) - 1) > 1e-10
RETURN 0
