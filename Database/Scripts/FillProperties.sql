SET NOCOUNT ON

SET IDENTITY_INSERT [Properties] ON

MERGE INTO [Properties] AS Target
USING (VALUES
  (1,'MSG_TYPE','Тип сообщения (K, Z, V, L, N, S)')
 ,(2,'STREAM_TYPE','Тип медиапотока')
 ,(3,'STREAM_ADDR','Адрес текущего медиапотока на приставке')
 ,(4,'RECEIVED_Rate','Скорость полученных UDP-пакетов')
 ,(5,'LINK_FAULTS_Rate','Скорость сбоев вещания TS-потока (CC discontinuity)')
 ,(6,'LOST_OVERFLOW','Флаг сигнализирующий о точности поля LOST. "=" - значение точно, "~" - возможна погрешность')
 ,(8,'LOST','Количество потерянных UDP-датаграмм, обнаруженных с помощью SmartTUBE PLC')
 ,(9,'RESTORED','Количество UDP-датаграмм, восстановленных SmartTUBE PLC')
 ,(10,'OVERFLOW','Количество событий переполнения виртуального буфера (overflow)')
 ,(11,'UNDERFLOW','Количество событий «антипереполнения» (опустошения) виртуального буфера (underflow)')
 ,(12,'MDI_DF','Delay Factor')
 ,(13,'MDI_MLR','Media Loss Rate - среднее количество пакетов потерянное в потоке за секунду, по данным PLC')
 ,(14,'PLC','Флаг использования SmartTUBE PLC на данной приставке')
 ,(15,'REGION_ID','Идентификатор региона приставки')
 ,(16,'PLAYER_URL','Текущий (последний запущенный) URL в плеере STB')
 ,(17,'CONTENT_TYPE','Тип контента')
 ,(18,'TRANSPORT_OUTER','Тип транспорта')
 ,(19,'TRANSPORT_INNER','Подтип транспорта (внутренний/дополнительный подтип)')
 ,(20,'CHANNEL_ID','Идентификатор канала (общий для live-вещания и TS/PVR/PL)')
 ,(21,'SCRAMBLED','В потоке есть скремблированные фреймы (CAS)')
 ,(22,'POWER_STATE','Режим работы приставки')
 ,(23,'CAS_TYPE','Тип CAS')
 ,(24,'CAS_KEY_TIME','Время выдачи текущего ключа CAS (time_t), в формате Unix time')
 ,(25,'VID_FRAMES','Количество декодированных видеокадров в интервал')
 ,(26,'VID_DECODE_ERRORS','Количество ошибочно декодированных кадров из предыдущего значения')
 ,(27,'VID_DATA_ERRORS','Количество ошибок в данных потока')
 ,(28,'AUD_FRAMES','Количество декодированных аудиофреймов')
 ,(29,'AUD_DATA_ERRORS','Количество ошибок в аудиоданных')
 ,(30,'AV_TIME_SKEW','Дельта от времени сообщения к точному времени получения AV-статистики (миллисекунды)')
 ,(31,'AV_PERIOD_SKEW','Дельта от периода сообщения к точному периоду AV-статистики (миллисекунды с точностью до 0.1 сек)')
 ,(34,'BUF_UNDERRUNS','Подсчет случаев фактической остановки воспроизведения из-за перебоев доставки медиапотока')
 ,(35,'BUF_OVERRUNS','Подсчет случаев фактического отброса медиаданных из-за переполнения буфера конвейера')
 ,(36,'SDP_OBJECT_ID','Идентификатор источника воспроизведения в SDP')
 ,(37,'DVB_LEVEL_GOOD','Признак нахождения уровня сигнала DVB-приемника не ниже допустимого порога (50 %)')
 ,(38,'DVB_LEVEL','Текущий уровень сигнала DVB-приемника в процентах')
 ,(39,'DVB_FREQUENCY','Текущая несущая частота DVB-приемника (KГц)')
 ,(40,'CUR_BITRATE','Текущий выбранный битрейт при адаптивном стриминге (HLS)')
 ,(41,'VID_MISSHOWN_FRAMES','Количество кадров возможно показанных с артефактами')
 ,(43,'SESSION_ON_PERIOD','Период текущей сессии – для отсева тех STB, которые включены, но не реально не используются')
 ,(44,'TV_ON_PERIOD','Время, в течение которого был включен телевизор')
 ,(45,'STB_ON_PERIOD','Время работы STB после Standby')
 ,(46,'USER_IDLE_PERIOD','Время, прошедшее с последнего нажатия на пульте')
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