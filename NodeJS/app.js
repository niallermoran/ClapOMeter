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

// Define the board, which is an abstraction of the Intel Edison
var board = new Five.Board({
  io: new Edison()
});



// The board.on() executes the anonymous function when the
// board reports back that it is initialized and ready. 
board.on("ready", function () {

  console.log("Board connected...");

  sound = new Five.Sensor("A1");

  console.log("Opening IoT Hub connection");

  iothub.open(connectCallback);

});




var connectCallback = function (err) {
  if (err) {
    console.error('Could not connect: ' + err.message);
  } else {
    console.log('Client connected');


    iothub.on('message', function (msg) {

      // this is fired when a cloud to device message is received by the IoT hub for this device
    });

    // Create a message and send it to the IoT Hub every second
    var sendInterval = setInterval(function () {
      var d = new Date();
      var data = JSON.stringify({
        DeviceId: 'edison',
        Sound: sound.value,
        Time: d
      });

      var message = new Message(data);
         console.log('Sending message: ' + message.getData());
      iothub.sendEvent(message, printResultFor('sent sound of ' + sound.value.toString()));
    }, 100);

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
