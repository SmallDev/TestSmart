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
	from LearningData(@learning)
	unpivot (Value FOR Property IN (MessageType, StreamType)) unpvt
	union all
	select Mac, null as NValue, Value as RValue,
		CASE Property
			WHEN 'ReceivedRate' THEN 3
			WHEN 'LinkFaultsRate' THEN 4
			WHEN 'LostRate' THEN 6
			WHEN 'RestoredRate' THEN 7
			WHEN 'OverflowRate' THEN 8
			WHEN 'UnderflowRate' THEN 9
			WHEN 'DelayFactor' THEN 10
			WHEN 'MediaLossRate' THEN 13
		END PropertyId
	from LearningData(@learning)
	unpivot (Value FOR Property IN (ReceivedRate, LinkFaultsRate, LostRate, RestoredRate, OverflowRate, UnderflowRate,
		DelayFactor, MediaLossRate)) unpvt
)
