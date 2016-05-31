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

connectionString = 'HostName=ClapOMeterIoTHub.azure-devices.net;DeviceId=00255c2004;SharedAccessKey=SnaAxhEhhceU2IlagaTSwXtg2IcDaqbA1Sc6yk4eP1A=';

var iothub = Client.fromConnectionString(connectionString, Protocol);

// Define the sensors you will use
var sound;

// Define the board, which is an abstraction of the Intel Edison
var board = new Five.Board({
  io: new Edison()
});


//client.open(connectCallback);

// The board.on() executes the anonymous function when the
// board reports back that it is initialized and ready. 
board.on("ready", function () {

  console.log("Board connected...");

  var sound = new Five.Sensor("A1");

  this.loop(1000, function () {
    var soundValue = sound.value;
    console.log("Sound value: " + soundValue.toString());
  });


});


var connectCallback = function (err) {
  if (err) {
    console.error('Could not connect: ' + err.message);
  } else {
    console.log('Client connected');


    client.on('message', function (msg) {

      // get the action sent from the cloud
      var action = msg.data;


      if (action == "TurnOn" && isHeatingOn == 0) {

        MakeSound();
        // receives messages sent from the cloud
        console.log('Message received : ' + action);
        isHeatingOn = 1;
      }

      if (action == "TurnOff" && isHeatingOn == 1) {


        MakeSound();
        // receives messages sent from the cloud
        console.log('Message received : ' + action);
        isHeatingOn = 0;
      }

      client.complete(msg, printResultFor('Cloud to device message received: ' + action));
    });

    // Create a message and send it to the IoT Hub every second
    var sendInterval = setInterval(function () {
      var d = new Date();
      var data = JSON.stringify({
        DeviceId: '00255c2004',
        DeviceName: 'Factory 5',
        FloorArea: '10000',
        Internaltemp: temp.celsius,
        ExternalTemp: temp.celsius - 15,
        IsHeatingOn: isHeatingOn,
        NumberofPeople: Math.floor(50 + (Math.random() * 5)),
        Time: new Date()
      });

      var message = new Message(data);
      //   console.log('Sending message: ' + message.getData());
      client.sendEvent(message, printResultFor('sent temp of ' + temp.celsius.toString() + ((isHeatingOn == 1) ? "Heating ON" : "Heating Off")));
    }, 2000);

    client.on('error', function (err) {
      console.error(err.message);
    });

    client.on('disconnect', function () {
      clearInterval(sendInterval);
      client.removeAllListeners();
      client.connect(connectCallback);
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
