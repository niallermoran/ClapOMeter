# ClapOMeter
The ClapOMeter is a pretty useless example of some pretty cool technology and quite frankly a bit of fun.

The demo uses an Intel Edison kit with Arduion exapnsion board and Grove Base shield to push sound intensity data into Azure IoT Hub every second. This data is then read in realtime and displayed on a web dashboard using jQuery calls to a web api that surfaces the data read from the IoT Hub. We then can use a Stream Analytics job to push this data, or some aggregations of this data, to other data stores like PowerBI or SQL Database.

# Components You Will Need

1. The Intel Edison Kit which includes the Intel Edison and Arduino Expansion Board. [You can find it here] (http://www.intel.com/buy/us/en/product/emergingtechnologies/edison-kit-arduino-462187)
2. A Grove Base Shield with Sensors. [You can find it here] (http://www.mouser.ie/new/seeedstudio/seeed-grove-kit-iot/)
3. An Azure subscription [You can find it here] (http://www.azure.com)

# Setup Azure
1. In Azure create an IoT Hub using the free or standard tier and store the following details for later:
 * The host name, e.g. ClapOMeterIoTHub
 * From the 'Shared Access Policies' section store the policy name (maybe create a new one), e.g. IoTHubOwner
 * From the 'Shared Access Policies' section click the policy name and store the primary key. 
 * From the 'Messaging' tab take note of the 'Event Hub-compatible name' and 'Event Hub-compatible endpoint' values

# Setup the Edison

The Edison is used to read sound intensity data from a sensor and send that data to an IoT Hub which you created in the previous step.


