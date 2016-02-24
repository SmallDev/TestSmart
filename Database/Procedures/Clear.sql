CREATE PROCEDURE [dbo].[Clear]
	@users bit = 0,
	@data bit = 0
AS
BEGIN
	begin tran
		if (@users = 1)
		begin
			delete from dbo.[UserProfile]
			delete from dbo.[User]
		end

		if (@data = 1)
		begin
			delete from dbo.[Data]
			update dbo.[Settings] set Value = NULL
			where Name = 'ReadTime'
		end
	commit
END