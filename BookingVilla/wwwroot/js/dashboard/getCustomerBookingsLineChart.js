$(document).ready(function () {
    loadCustomerBookingLineChart();
})

function loadCustomerBookingLineChart() {
    $(".chart-spinner").show();
    $.ajax({
        url: "/Dashboard/GetMemberAndBookingLineChartData",
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            loadLineChart("newMembersAndBookingsLineChart", data);
            $(".chart-spinner").hide();
        }
    })
}

function loadLineChart(id, data) {
    var chartColors = getChartColorsArray(id);

    options = {
        series: data.series,
        colors: chartColors,
        chart: {
            type: 'line',
            height: 350
        },
        stroke: {
            curve: 'smooth',
            width: 2
        },
        markers: {
            size: 3,
            strokeWidth: 0,
            hover: {
                size: 7
            }
        },
        xaxis: {
            categories: data.categories,
            labels: {
                style: {
                    colors: "#ddd"
                }
            }
        },
        yaxis: {
            labels: {
                style: {
                    colors: "#fff"
                }
            }
        },
        legend: {
            labels: {
                colors: "#fff"
            }
        },
        tooltip: {
            theme: 'dark'
        }
            
          
    };

    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render(); 
}