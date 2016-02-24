CREATE FUNCTION [dbo].[UnpivotData]
(
	@learning int
)
RETURNS TABLE AS RETURN
(
	select MAC, Value, 
		CASE Property
			WHEN 'MessageType' THEN 1
			WHEN 'ContentType' THEN 2         
		END PropertyId
	from LearningData(@learning)
	unpivot (Value FOR Property IN (MessageType, ContentType)) unpvt
)
