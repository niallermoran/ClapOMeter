$(document).ready(function () {

    PopulateAverages();
    PopulateChart();

    var lineChart;
    var gauge;
    var realtimeData = [];
    var maxGraphValues = 25;

    // setup click handler for reset link
    $("#resetdata").click(function () {
  
        $.ajax({
            url: "/ResetAggregates",
            type: "POST",
            dataType: "json"
        });

        realtimeData = [];

        alert('Data Reset Successfully');

    });

    function PopulateAverages() {
        $.ajax({
            url: "/api/SoundAggregatesData",
            type: "GET",
            dataType: "json",
            success: onDataReceived
        });

        function onDataReceived(data) {
            if (data != null) {

                try {
                    var max = data.Maximum;
                    var min = data.Minimum;
                    var avg = data.Average;
                    var timeFrame = data.TimeFrame;

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
                            end: { angle: 0, num: 1023 },
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
                }
                catch (e) {
                   // alert(e.message);
                }

            }

            setTimeout(PopulateAverages, 300);
        }

    }
    function PopulateChart() {

        $.ajax({
            url: "/api/SoundLatestData",
            type: "GET",
            dataType: "json",
            success: onDataReceived
        });

        function onDataReceived(soundData) {

            if (soundData != null) {

                try {
                   // gauge.val = soundData.sound;
                    gauge.SonicGauge('val', soundData.Sound);
                }
                catch (e) {
                   // alert(e.message);
                }

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
                            ymax: 1023
                        });
                    }

                    var mostRecentValue = null;
                    if (realtimeData.length > 0)
                        mostRecentValue = realtimeData[realtimeData.length - 1];


                    // add new value to array
                    if (mostRecentValue == null || mostRecentValue.Time != soundData.Time)
                        realtimeData.push(soundData);

                    // if there is too much data then remove some from the start
                    if (realtimeData.length > maxGraphValues)
                    {
                        realtimeData.splice(0, 1);
                    }

                    lineChart.setData(realtimeData, true);
                    lineChart.redraw();
                }
                catch (e) {
              //      alert(e.message);
                }

                setTimeout(PopulateChart, 100);
            }
        }

    }
});
