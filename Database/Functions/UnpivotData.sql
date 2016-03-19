CREATE FUNCTION [dbo].[UnpivotData]
(
	@learning int
)
RETURNS TABLE AS RETURN
(
	select Mac, Value as NValue, null as RValue,
		CASE Property
			WHEN 'MessageType' THEN 1
			WHEN 'StreamType' THEN 2
		END PropertyId
	from LearningData(1)
	unpivot (Value FOR Property IN (MessageType, StreamType)) unpvt
	union all
	select Mac, null as NValue, Value as RValue,
		CASE Property
			WHEN 'ReceivedRate' THEN 3
		END PropertyId
	from LearningData(1)
	unpivot (Value FOR Property IN (ReceivedRate)) unpvt
)
