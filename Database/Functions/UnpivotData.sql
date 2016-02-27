CREATE FUNCTION [dbo].[UnpivotData]
(
	@learning int
)
RETURNS TABLE AS RETURN
(
	select Mac, Value, 
		CASE Property
			WHEN 'MessageType' THEN 1
			WHEN 'StreamType' THEN 2         
		END PropertyId
	from LearningData(@learning)
	unpivot (Value FOR Property IN (MessageType, StreamType)) unpvt
)
