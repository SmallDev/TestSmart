SET NOCOUNT ON

SET IDENTITY_INSERT [Nominal] ON

MERGE INTO [Nominal] AS Target
USING (VALUES
  (1,1,'S')
 ,(2,1,'V')
 ,(3,1,'K')
 ,(4,1,'Z')
 ,(5,1,'N')
 ,(6,1,'L')
 ,(7,2,'C')
 ,(8,2,'M')
 ,(9,2,'U')
) AS Source ([Id],[PropertyId],[Value])
ON (Target.[Id] = Source.[Id])
WHEN MATCHED AND (
	NULLIF(Source.[PropertyId], Target.[PropertyId]) IS NOT NULL OR NULLIF(Target.[PropertyId], Source.[PropertyId]) IS NOT NULL OR 
	NULLIF(Source.[Value], Target.[Value]) IS NOT NULL OR NULLIF(Target.[Value], Source.[Value]) IS NOT NULL) THEN
 UPDATE SET
  [PropertyId] = Source.[PropertyId], 
  [Value] = Source.[Value]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([Id],[PropertyId],[Value])
 VALUES(Source.[Id],Source.[PropertyId],Source.[Value])
WHEN NOT MATCHED BY SOURCE THEN 
 DELETE
;
GO
DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [Nominal]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Nominal] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO

SET IDENTITY_INSERT [Nominal] OFF
GO
SET NOCOUNT OFF
GO