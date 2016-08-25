# ClapOMeter
The ClapOMeter is a pretty useless example of some pretty cool technology and quite frankly a bit of fun.

The demo uses an Intel Edison kit with Arduion expansion board and Grove Base shield to push sound intensity data into Azure IoT Hub every second. This data is then read in realtime and displayed on a web dashboard using jQuery calls to a web api that surfaces the data read from the IoT Hub. We then can use a Stream Analytics job to push this data, or some aggregations of this data, to other data stores like PowerBI or SQL Database.

# Components You Will Need

1. The Intel Edison Kit which includes the Intel Edison and Arduino Expansion Board and a Linux firmware. [You can find it here] (http://www.intel.com/buy/us/en/product/emergingtechnologies/edison-kit-arduino-462187)
2. A Grove Base Shield with Sensors. [You can find it here] (http://www.mouser.ie/new/seeedstudio/seeed-grove-kit-iot/)
3. An Azure subscription [You can find it here] (http://www.azure.com)
4. If you want to edit and debug the website code locally you will need [Visual Studio](https://www.visualstudio.com/). 
5. Putty or some other SSH client.
6. FileZilla or some other ftp client.

# Setup Azure
1. In Azure create an IoT Hub using the free or standard tier and take a note of the following details for later:
 * The host name, e.g. ClapOMeterIoTHub
 * From the 'Shared Access Policies' section store the policy name (maybe create a new one), e.g. IoTHubOwner
 * From the 'Shared Access Policies' section click the policy name and store the primary key. 
 * From the 'Messaging' tab take note of the 'Event Hub-compatible name' and 'Event Hub-compatible endpoint' values
2. Create a storage account and create 3 tables, Averages, Spikes and Alerts. We will use stream analytics to analyse data in realtime and populate these tables. Use the [Azure Storage Explorer] (http://storageexplorer.com/) to create and view the tables.
3. Create a stream analytics job.
 * Define a single input pointing to the IoT Hub just created.
 * Create an output for each of the tables just created. Name the outputs the same as the table names.
 * Use the sql code located in 'streamanalytics.sql' for the query. Make sure the input and output names map to the parameters used in this query.

# Register a Device
One of the beneifts of using Azure IoT hub is that it supports device registration and management. Each device registered with the hub gets a unique ID and key and can be disabled or enabled from the Azure portal. You can register a device either by writing some code or using the IoT Hub Explorer application found [here](https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md). Simply run this app and enter the details noted above to build the correct connection string and click 'Update'. Then in the management tab click 'Create' to create a new device registration with a device id that you define. You will notice that this form then displays two keys, take note of the primary key.

# Update the Code Configuration
There are only two parts to the code, the Node.js app that runs on the Edison and sends data to Azure IoT Hub and then the WebAPI and MVC .net and JQuery code that displays the realtime data as a web page.

1. Edit the web.config file under website and update the appsettings values with the values noted in the previous step.
2. Edit the node.js file and update the IoT connectionString variable with the values from the Device Explorer used when registering the device.

# Setup the Edison

The Edison is used to read sound intensity data from a sensor and send that data to an IoT Hub which you created in the previous step. The steps to setup the Edison are as follows:

1. Unbox and assemble the Edison Kit for Arduino. [This video from Intel shows how](https://software.intel.com/en-us/videos/intel-edison-kit-for-arduino-unboxing-and-assembly)
2. Attach the base shield to the board and the Sound sensor to one of the 'A' connectors. Make sure this matches the app.js file later, which is normally A1.
3. Update the firmware on the Edison using the Intel tool [found here](https://software.intel.com/en-us/iot/hardware/edison/downloads)
4. Power the Edison using the power cable and connect the edison to your computer using the serial micro USB cable.
5. Find out which serial port your Edison is connected to on your computer so you can connect to the device using SSH [see this video for Windows](https://software.intel.com/en-us/videos/shell-access-windows) and [this video for MAC}(https://software.intel.com/en-us/videos/shell-access-mac)
6. Now open [Putty] and connect to your Edison as per the previous video. The steps you need to take are as follows:
 * Run configure_edison --setup. This will allow you to rename your Edison and define a root password
 * Run configure_edison --wifi. This will allow you to configure a wifi connection. The Edison has a built in wifi chip. Once this connects take not of the IP address that the Edison gets on your network. This will be displayed within the console.
7. Connect your PC to the same Wifi network as the edison so that you can connect via FTP to transfer files. This is only needed to transfer files. If you are demoing this solution without copying files you won't need to do this.
8. Connect to the Edison using FileZilla or some other ftp client.
9. Off the root create a folder called ClapOMeter
10. Copy the app.js and package.json files from yourn local NodeJS folder to this new folder on your edison.
11. Return to the putty client and issue the command cd ClapOMeter to make sure you are issuing commands within your new folder.
12. Execute the comman npm -install. This will analyse the package.json file and download any dependencies required, e.g. the Johnny-Five and Azure SDKs.
13. Now you should be able to run the app.js file. Execute the command node app.js in the correct folder (Use ls command to ensure you are in the correct folder) 
14. If this works then you will start to see success messages on the console. If it doesn't work ensure your connection string is correct, that the Edison does have internet connectivity and that the correct packages have been downloaded.



