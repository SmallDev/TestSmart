﻿SET NOCOUNT ON

SET IDENTITY_INSERT [Properties] ON

MERGE INTO [Properties] AS Target
USING (VALUES
  (1,'MSG_TYPE',1)
 ,(2,'STREAM_TYPE',1)
 ,(3,'RECEIVED_RATE',0)
 ,(4,'LINK_FAULTS_Rate',0)
 ,(5,'LOST_OVERFLOW',NULL)
 ,(6,'LOST',0)
 ,(7,'RESTORED',0)
 ,(8,'OVERFLOW',0)
 ,(9,'UNDERFLOW',0)
 ,(10,'MDI_DF',0)
 ,(13,'MDI_MLR',0)
 ,(14,'PLC',NULL)
 ,(15,'REGION_ID',NULL)
 ,(16,'PLAYER_URL',NULL)
 ,(17,'CONTENT_TYPE',NULL)
 ,(18,'TRANSPORT_OUTER',NULL)
 ,(19,'TRANSPORT_INNER',NULL)
 ,(20,'CHANNEL_ID',NULL)
 ,(21,'SCRAMBLED',NULL)
 ,(22,'POWER_STATE',NULL)
 ,(23,'CAS_TYPE',NULL)
 ,(24,'CAS_KEY_TIME',NULL)
 ,(25,'VID_FRAMES',NULL)
 ,(26,'VID_DECODE_ERRORS',NULL)
 ,(27,'VID_DATA_ERRORS',NULL)
 ,(28,'AUD_FRAMES',NULL)
 ,(29,'AUD_DATA_ERRORS',NULL)
 ,(30,'AV_TIME_SKEW',NULL)
 ,(31,'AV_PERIOD_SKEW',NULL)
 ,(34,'BUF_UNDERRUNS',NULL)
 ,(35,'BUF_OVERRUNS',NULL)
 ,(36,'SDP_OBJECT_ID',NULL)
 ,(37,'DVB_LEVEL_GOOD',NULL)
 ,(38,'DVB_LEVEL',NULL)
 ,(39,'DVB_FREQUENCY',NULL)
 ,(40,'CUR_BITRATE',NULL)
 ,(41,'VID_MISSHOWN_FRAMES',NULL)
 ,(43,'SESSION_ON_PERIOD',NULL)
 ,(44,'TV_ON_PERIOD',NULL)
 ,(45,'STB_ON_PERIOD',NULL)
 ,(46,'USER_IDLE_PERIOD',NULL)
) AS Source ([Id],[Code],[IsNominal])
ON (Target.[Id] = Source.[Id])
WHEN MATCHED AND (
	NULLIF(Source.[Code], Target.[Code]) IS NOT NULL OR NULLIF(Target.[Code], Source.[Code]) IS NOT NULL OR 
	NULLIF(Source.[IsNominal], Target.[IsNominal]) IS NOT NULL OR NULLIF(Target.[IsNominal], Source.[IsNominal]) IS NOT NULL) THEN
 UPDATE SET
  [Code] = Source.[Code], 
  [IsNominal] = Source.[IsNominal]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([Id],[Code],[IsNominal])
 VALUES(Source.[Id],Source.[Code],Source.[IsNominal])
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