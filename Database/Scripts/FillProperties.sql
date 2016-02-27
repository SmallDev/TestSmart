SET NOCOUNT ON

SET IDENTITY_INSERT [Properties] ON

MERGE INTO [Properties] AS Target
USING (VALUES
  (1,'MSG_TYPE')
 ,(2,'STREAM_TYPE')
 ,(3,'STREAM_ADDR')
 ,(4,'RECEIVED_Rate')
 ,(5,'LINK_FAULTS_Rate')
 ,(6,'LOST_OVERFLOW')
 ,(8,'LOST')
 ,(9,'RESTORED')
 ,(10,'OVERFLOW')
 ,(11,'UNDERFLOW')
 ,(12,'MDI_DF')
 ,(13,'MDI_MLR')
 ,(14,'PLC')
 ,(15,'REGION_ID')
 ,(16,'PLAYER_URL')
 ,(17,'CONTENT_TYPE')
 ,(18,'TRANSPORT_OUTER')
 ,(19,'TRANSPORT_INNER')
 ,(20,'CHANNEL_ID')
 ,(21,'SCRAMBLED')
 ,(22,'POWER_STATE')
 ,(23,'CAS_TYPE')
 ,(24,'CAS_KEY_TIME')
 ,(25,'VID_FRAMES')
 ,(26,'VID_DECODE_ERRORS')
 ,(27,'VID_DATA_ERRORS')
 ,(28,'AUD_FRAMES')
 ,(29,'AUD_DATA_ERRORS')
 ,(30,'AV_TIME_SKEW')
 ,(31,'AV_PERIOD_SKEW')
 ,(34,'BUF_UNDERRUNS')
 ,(35,'BUF_OVERRUNS')
 ,(36,'SDP_OBJECT_ID')
 ,(37,'DVB_LEVEL_GOOD')
 ,(38,'DVB_LEVEL')
 ,(39,'DVB_FREQUENCY')
 ,(40,'CUR_BITRATE')
 ,(41,'VID_MISSHOWN_FRAMES')
 ,(43,'SESSION_ON_PERIOD')
 ,(44,'TV_ON_PERIOD')
 ,(45,'STB_ON_PERIOD')
 ,(46,'USER_IDLE_PERIOD')
) AS Source ([Id],[Code])
ON (Target.[Id] = Source.[Id])
WHEN MATCHED AND (
	NULLIF(Source.[Code], Target.[Code]) IS NOT NULL OR NULLIF(Target.[Code], Source.[Code]) IS NOT NULL) THEN
 UPDATE SET
  [Code] = Source.[Code]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([Id],[Code])
 VALUES(Source.[Id],Source.[Code])
WHEN NOT MATCHED BY SOURCE THEN 
 DELETE
;
GO
DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [Properties]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Properties] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO

SET IDENTITY_INSERT [Properties] OFF
GO
SET NOCOUNT OFF
GO