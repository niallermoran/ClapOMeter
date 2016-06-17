$(document).ready(function () {

    var lineChart;
    var gauge;

    setInterval(PopulateAverages, 500);
    setInterval(PopulateChart, 500);

    // setup click handler for reset link
    $("#resetdata").click(function () {
  
        $.ajax({
            url: "/ResetAggregates",
            type: "POST",
            dataType: "json"
        });

        realtimeData = [];
        messages = [];

        alert('Data Reset Successfully');

    });

    function PopulateAverages() {
        $.ajax({
            url: "/api/SoundAggregatesData",
            type: "GET",
            dataType: "json",
            success: onDataReceived,
            async:false
        });

        function onDataReceived(data) {
            if (data != null) {

                try {
                    var max = data.Maximum;
                    var min = data.Minimum;
                    var avg = data.Average;
                    var timeFrame = data.TimeFrame;
                    var current = data.Current;
                    $("#soundMaximum").text(max.Sound);
                    $("#soundMaximumTime").text(max.DateTimeLabel);
                    $("#soundMinimum").text(min.Sound);
                    $("#soundMinimumTime").text(min.DateTimeLabel);
                    $("#soundAverage").text(avg);
                    $("#soundAverageTimeFrame").text(timeFrame);
                }
                catch (e) {
                 //   alert(e.message);
                }

                try {
                    if (gauge == null) {

                        gauge = $('#defaultGauge').SonicGauge({
                            label: 'Sound',
                            start: { angle: -180, num: 0 },
                            end: { angle: 0, num: 100 },
                            style: {
                                "outline": { "fill": "r#f46a3a-#890b0b", "stroke": "#590303", "stroke-width": 4 },
                              //  "center": { "fill": "#ae1e1e", "diameter": 8, "stroke": "#590303", "stroke-width": 6 },
                                "needle": { "fill": "#fbdbdb", },
                                "label": { "font-size": 14 }
                            },
                            markers: [
                              {
                                  gap: 90,
                                  line: { "width": 8, "stroke": "none", "fill": "#999999" }
                              }
                            ],
                            animation_speed: 200
                        });
                    }
                    else {

                        gauge.options.style.needle.width = 100;
                        gauge.options.markers.gap = 50;
                        gauge.options.end.num = max.sound;
                    }

                    gauge.SonicGauge('val', current.Sound);

                }
                catch (e) {
             //      alert(e.message);
                }

            }
        }
    }

    function LogMessage( message )
    {
        var date = new Date();

        // create a table row for this message
        try {
            $('#messages').html(date.toGMTString() + ' - ' + message );
        }
        catch (e) {
            alert(e.message);
        }
    }

    function PopulateChart() {

        LogMessage("call Ajax for sound data");

        try{
            $.ajax({
                url: "/api/SoundData",
                type: "GET",
                dataType: "json",
                success: onDataReceived,
                error: onError,
                async:false
            });
        }
        catch(e)
        {
            LogMessage('Error calling Ajax: ' + e.message);
        }

        function onError(data)
        {
            LogMessage('Error calling Ajax /sounddata: ' + e.message);
        }

        function onDataReceived( data ) {

            LogMessage("received sound data");

            if (data != null && data.length != 0) {
                var length = data.length;

                try {

                    if (lineChart == null) {
                        lineChart = new Morris.Area({
                            element: 'morris-area-chart',
                            xkey: 'TimeLocal',
                            ykeys: ['Sound'],
                            labels: ['Sound'],
                            pointSize: 2,
                            hideHover: 'auto',
                            resize: true,
                            ymax: 100
                        });
                    }

                    lineChart.setData(data, true);
                    lineChart.redraw();
                }
                catch (e) {
                }

            }
            else
            {
                LogMessage("No data returned from /SoundData")
            }
        }

    }
});
