-- get 15 minute averages every minute. This levels out any spikes and this data can be used for monitoring sound levels
SELECT
    DeviceId,
   Avg( Sound) as Sensorreading,
   System.TimeStamp AS Time,
   DateAdd(minute,-15,System.TimeStamp) AS WinStartTime
INTO
    [Averages]
FROM
    [IotHub]
GROUP BY
    DeviceId,
    HoppingWindow(minute, 15, 1)    
    
-- to monitor for spikes, e.g. loud cheers, store the max value every minute
SELECT
    DeviceId,
   Max( Sound) as Sensorreading,
   System.TimeStamp AS Time
INTO
    [Spikes]
FROM
    [IotHub]
GROUP BY
    DeviceId,
    TumblingWindow(Minute,1)    
    
-- to action something during an alert condition send a message to an event hub
-- e.g. if the sound value is greater than 80 for any 10 second period
-- send an alert
SELECT
    DeviceId,
   Avg( Sound) as Sensorreading,
   System.TimeStamp AS Time
INTO
    [AlertQueue]
FROM
    [IotHub]
GROUP BY
    DeviceId,
    slidingwindow(second, 5)
    having    Avg( Sound) > 50
    
-- store the alert data in a database too for recrod
SELECT
DeviceId,
Avg( Sound) as Sensorreading,
System.TimeStamp AS Time
INTO
    [Alerts]
FROM
    [IotHub]
GROUP BY
    DeviceId,
    slidingwindow(second, 10)
    having    Avg( Sound) > 50
    
    
    
    
    