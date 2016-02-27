SET NOCOUNT ON

SET IDENTITY_INSERT [Properties] ON

MERGE INTO [Properties] AS Target
USING (VALUES
  (1,'MSG_TYPE',N'Тип сообщения (K, Z, V, L, N, S)')
 ,(2,'STREAM_TYPE',N'Тип медиапотока')
 ,(3,'STREAM_ADDR',N'Адрес текущего медиапотока на приставке')
 ,(4,'RECEIVED_Rate',N'Скорость полученных UDP-пакетов')
 ,(5,'LINK_FAULTS_Rate',N'Скорость сбоев вещания TS-потока (CC discontinuity)')
 ,(6,'LOST_OVERFLOW',N'Флаг сигнализирующий о точности поля LOST. "=" - значение точно, "~" - возможна погрешность')
 ,(8,'LOST',N'Количество потерянных UDP-датаграмм, обнаруженных с помощью SmartTUBE PLC')
 ,(9,'RESTORED',N'Количество UDP-датаграмм, восстановленных SmartTUBE PLC')
 ,(10,'OVERFLOW',N'Количество событий переполнения виртуального буфера (overflow)')
 ,(11,'UNDERFLOW',N'Количество событий «антипереполнения» (опустошения) виртуального буфера (underflow)')
 ,(12,'MDI_DF',N'Delay Factor')
 ,(13,'MDI_MLR',N'Media Loss Rate - среднее количество пакетов потерянное в потоке за секунду, по данным PLC')
 ,(14,'PLC',N'Флаг использования SmartTUBE PLC на данной приставке')
 ,(15,'REGION_ID',N'Идентификатор региона приставки')
 ,(16,'PLAYER_URL',N'Текущий (последний запущенный) URL в плеере STB')
 ,(17,'CONTENT_TYPE',N'Тип контента')
 ,(18,'TRANSPORT_OUTER',N'Тип транспорта')
 ,(19,'TRANSPORT_INNER',N'Подтип транспорта (внутренний/дополнительный подтип)')
 ,(20,'CHANNEL_ID',N'Идентификатор канала (общий для live-вещания и TS/PVR/PL)')
 ,(21,'SCRAMBLED',N'В потоке есть скремблированные фреймы (CAS)')
 ,(22,'POWER_STATE',N'Режим работы приставки')
 ,(23,'CAS_TYPE',N'Тип CAS')
 ,(24,'CAS_KEY_TIME',N'Время выдачи текущего ключа CAS (time_t), в формате Unix time')
 ,(25,'VID_FRAMES',N'Количество декодированных видеокадров в интервал')
 ,(26,'VID_DECODE_ERRORS',N'Количество ошибочно декодированных кадров из предыдущего значения')
 ,(27,'VID_DATA_ERRORS',N'Количество ошибок в данных потока')
 ,(28,'AUD_FRAMES',N'Количество декодированных аудиофреймов')
 ,(29,'AUD_DATA_ERRORS',N'Количество ошибок в аудиоданных')
 ,(30,'AV_TIME_SKEW',N'Дельта от времени сообщения к точному времени получения AV-статистики (миллисекунды)')
 ,(31,'AV_PERIOD_SKEW',N'Дельта от периода сообщения к точному периоду AV-статистики (миллисекунды с точностью до 0.1 сек)')
 ,(34,'BUF_UNDERRUNS',N'Подсчет случаев фактической остановки воспроизведения из-за перебоев доставки медиапотока')
 ,(35,'BUF_OVERRUNS',N'Подсчет случаев фактического отброса медиаданных из-за переполнения буфера конвейера')
 ,(36,'SDP_OBJECT_ID',N'Идентификатор источника воспроизведения в SDP')
 ,(37,'DVB_LEVEL_GOOD',N'Признак нахождения уровня сигнала DVB-приемника не ниже допустимого порога (50 %)')
 ,(38,'DVB_LEVEL',N'Текущий уровень сигнала DVB-приемника в процентах')
 ,(39,'DVB_FREQUENCY',N'Текущая несущая частота DVB-приемника (KГц)')
 ,(40,'CUR_BITRATE',N'Текущий выбранный битрейт при адаптивном стриминге (HLS)')
 ,(41,'VID_MISSHOWN_FRAMES',N'Количество кадров возможно показанных с артефактами')
 ,(43,'SESSION_ON_PERIOD',N'Период текущей сессии – для отсева тех STB, которые включены, но не реально не используются')
 ,(44,'TV_ON_PERIOD',N'Время, в течение которого был включен телевизор')
 ,(45,'STB_ON_PERIOD',N'Время работы STB после Standby')
 ,(46,'USER_IDLE_PERIOD',N'Время, прошедшее с последнего нажатия на пульте')
) AS Source ([Id],[Name],[Description])
ON (Target.[Id] = Source.[Id])
WHEN MATCHED AND (
	NULLIF(Source.[Name], Target.[Name]) IS NOT NULL OR NULLIF(Target.[Name], Source.[Name]) IS NOT NULL OR 
	NULLIF(Source.[Description], Target.[Description]) IS NOT NULL OR NULLIF(Target.[Description], Source.[Description]) IS NOT NULL) THEN
 UPDATE SET
  [Name] = Source.[Name], 
  [Description] = Source.[Description]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([Id],[Name],[Description])
 VALUES(Source.[Id],Source.[Name],Source.[Description])
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