CREATE PROCEDURE [dbo].[Clear]
	@readData bit = 0,
	@calcData bit = 0
AS
BEGIN
	begin tran
		if (@readData = 1)
		begin
			truncate table dbo.[Data]

			update dbo.[Settings] set Value = NULL
			where Name = 'ReadTime'
			update dbo.[Settings] set Value = NULL
			where Name = 'ReadVelocity'
		end

		if (@calcData = 1)
		begin tran
			ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] DISABLE

			delete from dbo.[UserProfile]
			delete from dbo.[User];DBCC CHECKIDENT ('dbo.[User]',RESEED, 0)
			
			delete from dbo.[ClusterNProfile]
			delete from dbo.[ClusterRProfile]
			delete from dbo.[Cluster];DBCC CHECKIDENT ('dbo.[Cluster]',RESEED, 0)

			truncate table dbo.[Learning]
			ALTER INDEX [PK_Profiles_NView] ON [dbo].[Profiles_NView] REBUILD

			update dbo.[Settings] set Value = NULL
			where Name = 'CalcTime'
			update dbo.[Settings] set Value = NULL
			where Name = 'CalcVelocity'
		commit tran
	commit
END