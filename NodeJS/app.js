'use strict';

console.log("Starting ..");

// Opensource API for interacting with boards like Arduino
var Five = require("johnny-five");
var Edison = require("edison-io");

// Iot Hub objects
var Protocol = require('azure-iot-device-amqp').Amqp;
var Client = require('azure-iot-device').Client;
var Message = require('azure-iot-device').Message;

// need a better way to store this in reality, this is just for demo
var connectionString = 'HostName=[eventhubname].azure-devices.net;DeviceId=[device id];SharedAccessKeyName=[shared access key];SharedAccessKey=[key]';

var iothub = Client.fromConnectionString(connectionString, Protocol);

// Define the sensors you will use
var sound;
var buzzer;

// Define the board, which is an abstraction of the Intel Edison
var board = new Five.Board({
    io: new Edison()
});


function MakeSound() {
    buzzer.frequency(587, 1000);
    setTimeout(function () {
        buzzer.off();
    }, 500);

}

// The board.on() executes the anonymous function when the
// board reports back that it is initialized and ready. 
board.on("ready", function () {

    console.log("Board connected...");

    sound = new Five.Sensor({
        pin: "A0",
        freq: 50,
        threshold: 5
    });

    // remove this if you don't have a buzzer, and remove code from makesound
    buzzer = new Five.Piezo(4); // add buzzer sensor to the D4 connector

    // just do this to make sure your connected and working
    MakeSound();

    // open a two way connection with IoT Hub
    // Once the connction is successful the call back will be called
    console.log("Opening IoT Hub connection");
    iothub.open(connectCallback);

});

var connectCallback = function (err) {
    if (err) {
        console.error('Could not connect: ' + err.message);
    } else {

        // connected, if there are errors, check WIFI and connectionstring
        // these are working make sure you have run npm -install to install 
        // Azure SDKs referenced above
        console.log('IoT Hub connected');

        // once we have successfully connected to IoT HUb
        // setup the sound monitoring to scale values between 0 and 100
        sound.scale(0, 100).on("change", function () {

            var val = Math.floor(sound.value);
            var d = new Date();
            var data = JSON.stringify({
                DeviceId: 'edison',
                Sound: val,
                Time: d
            });

            var message = new Message(data);
            console.log('Sending message: ' + message.getData());

            iothub.sendEvent(message, printResultFor('sent sound of ' + val.toString()));

        });

        iothub.on('message', function (msg) {
            // this is fired when a cloud to device message is received by the IoT hub for this device
            MakeSound();
        });

        iothub.on('error', function (err) {
            console.error(err.message);
        });

        iothub.on('disconnect', function () {
            clearInterval(sendInterval);
            iothub.removeAllListeners();
            iothub.connect(connectCallback);
        });
    }
};


// Helper function to print results in the console
function printResultFor(op) {
    return function printResult(err, res) {
        if (err) console.log(op + ' error: ' + err.toString());
        if (res) console.log(op + ' status: ' + res.constructor.name);
    };
}
