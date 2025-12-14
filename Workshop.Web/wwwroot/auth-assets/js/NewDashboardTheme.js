$('#js-page-content-demopanels').smartPanel(
    {
        localStorage: false,
        onChange: function () { },
        onSave: function () { },
        opacity: 1,
        deleteSettingsKey: '#deletesettingskey-options',
        settingsKeyLabel: 'Reset settings?',
        deletePositionKey: '#deletepositionkey-options',
        positionKeyLabel: 'Reset position?',
        sortable: true,
        buttonOrder: '%collapse% %fullscreen% %close%',
        buttonOrderDropdown: '%refresh% %locked% %color% %custom% %reset%',
        customButton: false,
        closeButton: false,
        fullscreenButton: false,
        collapseButton: false,
        lockedButton: true,
        lockedButtonLabel: "Lock Position",
        onLocked: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onLocked")
        },
        refreshButton: true,
        refreshButtonLabel: "Refresh Content",
        onRefresh: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onRefresh")
        },
        colorButton: true,
        colorButtonLabel: "Panel Style",
        onColor: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onColor")
        },
        panelColors: ['bg-primary-700 bg-success-gradient',
            'bg-primary-500 bg-info-gradient',
            'bg-primary-600 bg-primary-gradient',
            'bg-info-600 bg-primray-gradient',
            'bg-info-600 bg-info-gradient',
            'bg-info-700 bg-success-gradient',
            'bg-success-900 bg-info-gradient',
            'bg-success-700 bg-primary-gradient',
            'bg-success-600 bg-success-gradient',
            'bg-danger-900 bg-info-gradient',
            'bg-fusion-400 bg-fusion-gradient',
            'bg-faded'
        ],
        resetButton: true,
        resetButtonLabel: "Reset Panel",
        onReset: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onReset callback")
        }
    });
$('#js-page-content-demopanels2').smartPanel(
    {
        localStorage: false,
        onChange: function () { },
        onSave: function () { },
        opacity: 1,
        deleteSettingsKey: '#deletesettingskey-options',
        settingsKeyLabel: 'Reset settings?',
        deletePositionKey: '#deletepositionkey-options',
        positionKeyLabel: 'Reset position?',
        sortable: true,
        buttonOrder: '%collapse% %fullscreen% %close%',
        buttonOrderDropdown: '%refresh% %locked% %color% %custom% %reset%',
        customButton: false,
        closeButton: false,
        fullscreenButton: false,
        collapseButton: false,
        lockedButton: true,
        lockedButtonLabel: "Lock Position",
        onLocked: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onLocked")
        },
        refreshButton: true,
        refreshButtonLabel: "Refresh Content",
        onRefresh: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onRefresh")
        },
        colorButton: true,
        colorButtonLabel: "Panel Style",
        onColor: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onColor")
        },
        panelColors: ['bg-primary-700 bg-success-gradient',
            'bg-primary-500 bg-info-gradient',
            'bg-primary-600 bg-primary-gradient',
            'bg-info-600 bg-primray-gradient',
            'bg-info-600 bg-info-gradient',
            'bg-info-700 bg-success-gradient',
            'bg-success-900 bg-info-gradient',
            'bg-success-700 bg-primary-gradient',
            'bg-success-600 bg-success-gradient',
            'bg-danger-900 bg-info-gradient',
            'bg-fusion-400 bg-fusion-gradient',
            'bg-faded'
        ],
        resetButton: true,
        resetButtonLabel: "Reset Panel",
        onReset: function () {
            if (myapp_config.debugState)
                console.log($(this).closest(".panel").attr('id') + " onReset callback")
        }
    });



//$(document).ready(() => {
//    //AgreementsSelectProject();
//    ReservationSelectPeriod();
//   // VehiclesSelectProject();
//})


////Start Reservation Dashboard Code

//function AgreementsSelectProject() {
//    let projectId = $('#agreementSelected').val() | null;
//    console.log(projectId)
//   $.ajax({
//       type: 'Get',
//       url: "/Dashboard/GetAgreementStatistics?projectId=" + projectId,
//       success: function (res) {
//            var agreementschartDom = document.getElementById('main2');
//            var agreementschart = echarts.init(agreementschartDom);
//            //var agreementTodayChartDom = document.getElementById('main44');
//            //var agreementTodayChart = echarts.init(agreementTodayChartDom);

//           if (res.Status.length > 0) {
//               InitAgreementsStatistics(agreementschart, res.Status);
//               //InitAgreementsStatisticsToday(agreementTodayChart, res.StatusToday);

//               $('#countAgreements').text(`${res.Count} `);
//               $('#AgreementsTextNoData').hide();
//               $('#countAgreementsText').show();
//           }
//           else {
//               $('#AgreementsTextNoData').show();
//               $('#countAgreementsText').hide();
//               agreementschart.dispose();
//               agreementTodayChart.dispose();
//           }
//       },
//       error: function (err) {

//       }
//    })
//}

//function InitAgreementsStatistics(chart, data) {
//    var agreementsOption;
//    agreementsOption = {
//        legend: {},
//        tooltip: {
//            trigger: 'axis',
//            showContent: false
//        },
//        dataset: {
//            source: data
//        },
//        xAxis: { type: 'category' },
//        yAxis: { gridIndex: 0 },
//        grid: { top: '55%' },
//        series: [
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'pie',
//                id: 'pie',
//                radius: '30%',
//                center: ['50%', '25%'],
//                emphasis: {
//                    focus: 'self'
//                },
//                label: {
//                    formatter: '{b}: {@2012} ({d}%)'
//                },
//                encode: {
//                    itemName: 'Statedate',
//                    value: data[0][1],
//                    tooltip: data[0][1]
//                }
//            }
//        ]
//    };
//      chart.on('updateAxisPointer', function (event) {
//        const xAxisInfo = event.axesInfo[0];
//        if (xAxisInfo) {
//            const dimension = xAxisInfo.value + 1;
//            chart.setOption({
//                series: {
//                    id: 'pie',
//                    label: {
//                        formatter: '{b}: {@[' + dimension + ']} ({d}%)'
//                    },
//                    encode: {
//                        value: dimension,
//                        tooltip: dimension
//                    }
//                }
//            });
//        }
//    });
//    chart.setOption(agreementsOption);
//    agreementsOption && chart.setOption(agreementsOption);
//};

//function InitAgreementsStatisticsToday(agreementTodayChart, resData) {

//    var agreementsOptionToday;
//    agreementsOptionToday = {
//        title: [
//            {
//                text: 'Today',
//                left: 'center'
//            }
//        ],
//        series: [
//            {
//                type: 'pie',
//                radius: '25%',
//                center: ['50%', '50%'],
//                data: resData,
//                label: {
//                    position: 'outer',
//                    alignTo: 'labelLine',
//                    bleedMargin: 5
//                },
//                left: 0,
//                right: 0,
//                top: 0,
//                bottom: 0
//            }
//        ]
//    };

//    agreementsOptionToday && agreementTodayChart.setOption(agreementsOptionToday);
//}

///*End Agreement Dashboard Code*/


//function CustomData(data) {
//    var dataMap = new Map();
//    var responseData = [];
//    debugger

//    for (var i = 0; i < data.length ; i++) {

//        if (dataMap.get('DateStatus')) {
//            dataMap.get('DateStatus').push(data[i].Date);
//        } else {
//            dataMap.set('DateStatus', [data[i].Date]);
//        }

//        if (dataMap.get(data[i].Name)) {
//            dataMap.get(data[i].Name).push(data[i].Value);
//        } else {
//            dataMap.set(data[i].Name, [data[i].Value]);
//        }
//    }
//    var get_keys = dataMap.keys()
//    for (var e of get_keys) {
//        let listData = [e];
//        for (var value of dataMap.get(e)) {
//            listData.push(value);
//        }
//        responseData.push(listData);
//    }
//   return responseData;
//}

///*Start Reservation Dashboard Code*/

// function ReservationSelectPeriod() {

//    let periodId = $('#reservationSelected').val() | null;
//    console.log(periodId);

//     $.ajax({
//        type: 'Get',
//         url: "/Dashboard/GetReservationStatusStatistics?periodId=" + periodId,
//         success: function (res) {
//             try {
//                 var reservationChartDom = document.getElementById('main23');
//                 var reservationChart = echarts.init(reservationChartDom);
//                 debugger
//                 //var reservationTodaychartDom = document.getElementById('main33');
//                 //var reservationTodayChart = echarts.init(reservationTodaychartDom);
//                 if (res.Status.length > 0) {
//                     CustomData(res.Status);
//                     InitReservationStatisticsChart(reservationChart, CustomData(res.Status));
//                     //InitReservationStatisticsTodayChart(reservationTodayChart, res.StatusToday);

//                     $('#countBookingVehicles').text(res.Count);
//                     $('#ReservationTextNoData').hide();
//                     $('#BookingVehiclesText').show();
//                 }
//                 else {
//                     $('#ReservationTextNoData').show();
//                     $('#BookingVehiclesText').hide();
//                     reservationChart.dispose();
//                     reservationTodayChart.dispose();
//                 }
//             }
//             catch { }
//        },
//        error: function (err) {

//        }
//    });
//}

//function InitReservationStatisticsChart(chart, data) {

//    var reservationOption;

//    reservationOption = {
//        legend: {},
//        tooltip: {
//            trigger: 'axis',
//            showContent: false
//        },
//        dataset: {
//            source: data
//        },
//        xAxis: { type: 'category' },
//        yAxis: { gridIndex: 0 },
//        grid: { top: '55%' },
//        series: [
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'pie',
//                id: 'pie',
//                radius: '30%',
//                center: ['50%', '25%'],
//                emphasis: {
//                    focus: 'self'
//                },
//                label: {
//                    formatter: '{b}: {@2012} ({d}%)'
//                },
//                encode: {
//                    itemName: data[0][0],
//                    value: data[0][1],
//                    tooltip: data[0][1]
//                }
//            }
//        ]
//    };
//    chart.on('updateAxisPointer', function (event) {
//        const xAxisInfo = event.axesInfo[0];
//        if (xAxisInfo) {
//            const dimension = xAxisInfo.value + 1;
//            chart.setOption({
//                series: {
//                    id: 'pie',
//                    label: {
//                        formatter: '{b}: {@[' + dimension + ']} ({d}%)'
//                    },
//                    encode: {
//                        value: dimension,
//                        tooltip: dimension
//                    }
//                }
//            });
//        }
//    });
//    chart.setOption(reservationOption);
//    reservationOption && chart.setOption(reservationOption);

//}

//function InitReservationStatisticsTodayChart(chart, resData) {

//    var ReservationOption;

//    //const data33 = [
//    //    {
//    //        name: 'Due Checkout',
//    //        value: 20
//    //    },
//    //    {
//    //        name: 'Confirmed',
//    //        value: 20
//    //    },
//    //    {
//    //        name: 'Canceled',
//    //        value: 40
//    //    },
//    //    {
//    //        name: 'Overdue checkout',
//    //        value: 10
//    //    }
//    //];
//    ReservationOption = {
//        title: [
//            {
//                text: 'Today',
//                left: 'center'
//            }
//        ],
//        series: [
//            {
//                type: 'pie',
//                radius: '25%',
//                center: ['50%', '50%'],
//                data: resData,
//                label: {
//                    position: 'outer',
//                    alignTo: 'labelLine',
//                    bleedMargin: 5
//                },
//                left: 0,
//                right: 0,
//                top: 0,
//                bottom: 0
//            }
//        ]
//    };

//    ReservationOption && chart.setOption(ReservationOption);
//}

///*End Reservation Dashboard Code*/


///*Start Vehicles Dashboard Code*/

//function VehiclesSelectProject() {
//    let projectId = $('#vehiclesSelected').val() | null;
//    console.log(projectId);

//    $.ajax({
//        type: 'Get',
//        url: "/Dashboard/GetVehiclesStatusStatistics?projectId=" + projectId,
//        success: function (res) {
//            var vehiclesChartDom = document.getElementById('main3');
//            var vehiclesChart = echarts.init(vehiclesChartDom);
//            if (res.Status.length > 0) {
//                InitVehiclesStatisticsChart(vehiclesChart, res.Status);
//                $('#vehiclesText').show();
//                $('#countVehicles').text(res.Count);
//                $('#vehiclesTextNoData').hide();
//            }
//            else {
//                $('#vehiclesTextNoData').show();
//                $('#vehiclesText').hide();
//                vehiclesChart.dispose();
//            }
//        },
//        error: function (xhr, status, error) {
//            alert(error);
//        }
//    });
//}

//function InitVehiclesStatisticsChart(chart, data) {
//    var vehiclesOption;

//    vehiclesOption = {
//        legend: {},
//        tooltip: {
//            trigger: 'axis',
//            showContent: false
//        },
//        dataset: {
//            source: data
//        },
//        xAxis: { type: 'category' },
//        yAxis: { gridIndex: 0 },
//        grid: { top: '55%' },
//        series: [
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'line',
//                smooth: true,
//                seriesLayoutBy: 'row',
//                emphasis: { focus: 'series' }
//            },
//            {
//                type: 'pie',
//                id: 'pie',
//                radius: '30%',
//                center: ['50%', '25%'],
//                emphasis: {
//                    focus: 'self'
//                },
//                label: {
//                    formatter: '{b}: {@2012} ({d}%)'
//                },
//                encode: {
//                    itemName: data[0][0],
//                    value:    data[0][1], 
//                    tooltip: data[0][1]
//                }
//            }
//        ]
//    };
//    chart.on('updateAxisPointer', function (event) {
//        const xAxisInfo = event.axesInfo[0];
//        if (xAxisInfo) {
//            const dimension = xAxisInfo.value + 1;
//            chart.setOption({
//                series: {
//                    id: 'pie',
//                    label: {
//                        formatter: '{b}: {@[' + dimension + ']} ({d}%)'
//                    },
//                    encode: {
//                        value: dimension,
//                        tooltip: dimension
//                    }
//                }
//            });
//        }
//    });
//    chart.setOption(vehiclesOption);
//vehiclesOption && chart.setOption(vehiclesOption);
//};

///*End Vehicles Dashboard Code*/

///*Start Replacement Dashboart Code*/
//function ReplacementSelectProject() {

//    let projectId = $('#reservationSelected').val() | null;
//    console.log(projectId);

//    $.ajax({
//        type: 'Get',
//        url: "/Dashboard/GetReplacementStatusStatistics?projectId=" + projectId,
//        success: function (res) {
//            var replacementchartDom = document.getElementById('main8');
//            var replacementChart = echarts.init(replacementchartDom);

//            if (res.Status.length > 0) {
//                InitReplacementStatisticsChart(replacementChart, res.Status);
//            }
//            else {
//                $('#countVehicles').text('There are no data');
//                vehiclesChart.dispose();
//            }
//        },
//        error: function (xhr, status, error) {
//            alert(error);
//        }
//    });

//}

//function InitReplacementStatisticsChart(chart, data) {
//    var replacementOption;

//    replacementOption = {
//        color: ['#67F9D8', '#FFE434', '#56A3F1', '#FF917C'],
//        legend: {},
//        radar: [
//            {
//                indicator: [
//                    { text: 'Maintenance' },
//                    { text: 'Regular Maintenance' },
//                    { text: 'Acording Customer Request' },
//                    { text: 'Indicator4' },
//                    { text: 'Indicator5' }
//                ],
//                radius: 120,
//                startAngle: 90,
//                splitNumber: 4,
//                shape: 'circle',
//                axisName: {
//                    formatter: '【{value}】',
//                    color: '#428BD4'
//                },
//                splitArea: {
//                    areaStyle: {
//                        color: ['#77EADF', '#26C3BE', '#64AFE9', '#428BD4'],
//                        shadowColor: 'rgba(0, 0, 0, 0.2)',
//                        shadowBlur: 10
//                    }
//                },
//                axisLine: {
//                    lineStyle: {
//                        color: 'rgba(211, 253, 250, 0.8)'
//                    }
//                },
//                splitLine: {
//                    lineStyle: {
//                        color: 'rgba(211, 253, 250, 0.8)'
//                    }
//                }
//            }
//        ],
//        series: [
//            {
//                type: 'radar',
//                emphasis: {
//                    lineStyle: {
//                        width: 4
//                    }
//                },
//                data: [
//                    {
//                        value: [100, 8, 0.4, -80, 2000],
//                        name: 'Permanent'
//                    },
//                    {
//                        value: [60, 5, 0.3, -100, 1500],
//                        name: 'Temporary',
//                        areaStyle: {
//                            color: 'rgba(255, 228, 52, 0.6)'
//                        }
//                    }
//                ]
//            }
//        ]
//    };

//    replacementOption && chart.setOption(replacementOption);
//}




///*End Replacement Dashboart Code*/




///*Start Profit Dashboart Code*/
//var chartDom5 = document.getElementById('main5');
//var myChart5 = echarts.init(chartDom5);
//var option5;

//option5 = {
//    title: {
//        text: profitTitle,
//        left: 'center'
//    },
//    tooltip: {
//        trigger: 'item',
//        formatter: '{a} <br/>{b} : {c} ({d}%)'
//    },
//    legend: {
//        bottom: 10,
//        left: 'center',
//        data: [Profit, Loss, Revenue, Cost]
//    },
//    series: [
//        {
//            type: 'pie',
//            radius: '65%',
//            center: ['50%', '50%'],
//            selectedMode: 'single',
//            data: [
//                { value: 735, name: Profit },
//                { value: 510, name: Loss },
//                { value: 434, name: Revenue },
//                { value: 335, name: Cost }
//            ]
//        }
//    ]
//};

//option5 && myChart5.setOption(option5);
///*End Profit Dashboart Code*/
///*Start Financial Dashboart Code*/
//var chartDom10 = document.getElementById('main10');
//var myChart10 = echarts.init(chartDom10);
//var option10;

//option10 = {
//    tooltip: {
//        trigger: 'axis',
//        axisPointer: {
//            type: 'cross',
//            label: {
//                backgroundColor: '#6a7985'
//            }
//        }
//    },
//    legend: {
//        data: [Payment, Profit]
//    },
//    locale: 'AR',
//    toolbox: {
//        feature: {
//            saveAsImage: {}
//        }
//    },
//    grid: {
//        left: '3%',
//        right: '4%',
//        bottom: '3%',
//        containLabel: true
//    },
//    xAxis: [
//        {
//            type: 'category',
//            boundaryGap: false,
//            data: ['2/2/2022', '10/3/2022', '23/4/2022', '25/5/2022', '22/7/2022', '3/8/2022']
//        }
//    ],
//    yAxis: [
//        {
//            type: 'value'
//        }
//    ],
//    series: [
//        //{
//        //    name: Investment,
//        //    type: 'line',
//        //    stack: 'Total',
//        //    areaStyle: {},
//        //    emphasis: {
//        //        focus: 'series'
//        //    },
//        //    data: [120, 132, 101, 134, 90, 230, 210]
//        //},
//        {
//            name: Payment,
//            type: 'line',
//            stack: 'Total',
//            areaStyle: {},
//            //emphasis: {
//            //    focus: 'series'
//            //},
//            data: [220, 182, 191, 234, 290, 330, 310]
//        },
//        {
//            name: Profit,
//            type: 'line',
//            stack: 'Total',
//            areaStyle: {},
//            //emphasis: {
//            //    focus: 'series'
//            //},
//            data: [150, 232, 201, 154, 190, 330, 410]
//        }
//    ]
//};

//option10 && myChart10.setOption(option10);

///*End Financial Dashboart Code*/


//try {
//    var chartDom4 = document.getElementById('main4');
//    var myChart4 = echarts.init(chartDom4);
//    var option4;

//    setTimeout(function () {
//        option4 = {
//            legend: {},
//            tooltip: {
//                trigger: 'axis',
//                showContent: false
//            },
//            dataset: {
//                source: [
//                    ['product3', '2/2/2022', '10/3/2022', '23/4/2022', '25/5/2022', '22/7/2022', '3/8/2022'],
//                    ['Vehicles In', 25.2, 37.1, 41.2, 18, 33.9, 49.1],
//                    ['Vehicles Out', 66, 44, 50, 44, 60, 70],
//                    ['Transfer Requests', 25.2, 37.1, 41.2, 18, 33.9, 49.1],
//                    ['Replacement', 40.1, 62.2, 69.5, 36.4, 45.2, 32.5]
//                ]
//            },
//            xAxis: { type: 'category' },
//            yAxis: { gridIndex: 0 },
//            grid: { top: '55%' },
//            series: [
//                {
//                    type: 'line',
//                    smooth: true,
//                    seriesLayoutBy: 'row',
//                    emphasis: { focus: 'series' }
//                },
//                {
//                    type: 'line',
//                    smooth: true,
//                    seriesLayoutBy: 'row',
//                    emphasis: { focus: 'series' }
//                },
//                {
//                    type: 'line',
//                    smooth: true,
//                    seriesLayoutBy: 'row',
//                    emphasis: { focus: 'series' }
//                },
//                {
//                    type: 'line',
//                    smooth: true,
//                    seriesLayoutBy: 'row',
//                    emphasis: { focus: 'series' }
//                },
//                {
//                    type: 'line',
//                    smooth: true,
//                    seriesLayoutBy: 'row',
//                    emphasis: { focus: 'series' }
//                },
//                {
//                    type: 'pie',
//                    id: 'pie',
//                    radius: '30%',
//                    center: ['50%', '25%'],
//                    emphasis: {
//                        focus: 'self'
//                    },
//                    label: {
//                        formatter: '{b}: {@2012} ({d}%)'
//                    },
//                    encode: {
//                        itemName: 'product3',
//                        value: '2/2/2022',
//                        tooltip: '2/2/2022'
//                    }
//                }
//            ]
//        };
//        myChart4.on('updateAxisPointer', function (event) {
//            const xAxisInfo = event.axesInfo[0];
//            if (xAxisInfo) {
//                const dimension = xAxisInfo.value + 1;
//                myChart4.setOption({
//                    series: {
//                        id: 'pie',
//                        label: {
//                            formatter: '{b}: {@[' + dimension + ']} ({d}%)'
//                        },
//                        encode: {
//                            value: dimension,
//                            tooltip: dimension
//                        }
//                    }
//                });
//            }
//        });
//        myChart4.setOption(option4);
//    });

//    option4 && myChart4.setOption(option4);
//}
//catch { }
///*Start Revenue Dashboart Code*/

//try {
//    var chartDom = document.getElementById('main');
//    var myChart = echarts.init(chartDom);
//    var option;

//    option = {
//        color: ['#80FFA5', '#00DDFF', '#37A2FF', '#FF0087', '#FFBF00'],
//        title: {
//            text: OverAllText
//        },
//        tooltip: {
//            trigger: 'axis',
//            axisPointer: {
//                type: 'cross',
//                label: {
//                    backgroundColor: '#6a7985'
//                }
//            }
//        },
//        legend: {
//            data: ['Revenue', 'Reservation']
//        },
//        toolbox: {
//            feature: {
//                saveAsImage: {}
//            }
//        },
//        grid: {
//            left: '3%',
//            right: '4%',
//            bottom: '3%',
//            containLabel: true
//        },
//        xAxis: [
//            {
//                type: 'category',
//                boundaryGap: false,
//                data: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
//            }
//        ],
//        yAxis: [
//            {
//                type: 'value'
//            }
//        ],
//        series: [
//            {
//                name: 'Revenue',
//                type: 'line',
//                stack: 'Total',
//                smooth: true,
//                lineStyle: {
//                    width: 0
//                },
//                showSymbol: false,
//                areaStyle: {
//                    opacity: 0.8,
//                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
//                        {
//                            offset: 0,
//                            color: 'rgb(128, 255, 165)'
//                        },
//                        {
//                            offset: 1,
//                            color: 'rgb(1, 191, 236)'
//                        }
//                    ])
//                },
//                emphasis: {
//                    focus: 'series'
//                },
//                data: [140, 232, 101, 264, 90, 340, 250]
//            },
//            {
//                name: 'Reservation',
//                type: 'line',
//                stack: 'Total',
//                smooth: true,
//                lineStyle: {
//                    width: 0
//                },
//                showSymbol: false,
//                areaStyle: {
//                    opacity: 0.8,
//                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
//                        {
//                            offset: 0,
//                            color: 'rgb(0, 221, 255)'
//                        },
//                        {
//                            offset: 1,
//                            color: 'rgb(77, 119, 255)'
//                        }
//                    ])
//                },
//                emphasis: {
//                    focus: 'series'
//                },
//                data: [120, 282, 111, 234, 220, 340, 310]
//            }
//        ]
//    };

//    option && myChart.setOption(option);
//    /*End Revenue Dashboart Code*/
//}
//catch
//{ }

//try { 
///*Start Collection Dashboart Code*/
//var chartDom6 = document.getElementById('main6');
//var myChart6 = echarts.init(chartDom6);
//var option6;

//option6 = {
//    tooltip: {
//        trigger: 'item'
//    },
//    legend: {
//        top: '5%',
//        left: 'center'
//    },
//    series: [
//        {
//            name: 'Access From',
//            type: 'pie',
//            radius: ['40%', '70%'],
//            avoidLabelOverlap: false,
//            itemStyle: {
//                borderRadius: 10,
//                borderColor: '#fff',
//                borderWidth: 2
//            },
//            label: {
//                show: false,
//                position: 'center'
//            },
//            emphasis: {
//                label: {
//                    show: true,
//                    fontSize: '40',
//                    fontWeight: 'bold'
//                }
//            },
//            labelLine: {
//                show: false
//            },
//            data: [
//                { value: 1048, name: 'Collected' },
//                { value: 735, name: 'Under Collection' },
//                { value: 580, name: 'Delayed' },
//                { value: 484, name: 'etc' }
//            ]
//        }
//    ]
//};

//option6 && myChart6.setOption(option6);
//}
//catch
//{ }

///*End Collection Dashboart Code*/



///*Start Insurance Dashboart Code*/

//var app = {};
//try { 
//var chartDom9 = document.getElementById('main9');
//var myChart9 = echarts.init(chartDom9);
//var option9;

//const posList = [
//    'left',
//    'right',
//    'top',
//    'bottom',
//    'inside',
//    'insideTop',
//    'insideLeft',
//    'insideRight',
//    'insideBottom',
//    'insideTopLeft',
//    'insideTopRight',
//    'insideBottomLeft',
//    'insideBottomRight'
//];
//app.configParameters = {
//    rotate: {
//        min: -90,
//        max: 90
//    },
//    align: {
//        options: {
//            left: 'left',
//            center: 'center',
//            right: 'right'
//        }
//    },
//    verticalAlign: {
//        options: {
//            top: 'top',
//            middle: 'middle',
//            bottom: 'bottom'
//        }
//    },
//    position: {
//        options: posList.reduce(function (map, pos) {
//            map[pos] = pos;
//            return map;
//        }, {})
//    },
//    distance: {
//        min: 0,
//        max: 100
//    }
//};
//app.config = {
//    rotate: 90,
//    align: 'left',
//    verticalAlign: 'middle',
//    position: 'insideBottom',
//    distance: 15,
//    onChange: function () {
//        const labelOption = {
//            rotate: app.config.rotate,
//            align: app.config.align,
//            verticalAlign: app.config.verticalAlign,
//            position: app.config.position,
//            distance: app.config.distance
//        };
//        myChart.setOption({
//            series: [
//                {
//                    label: labelOption
//                },
//                {
//                    label: labelOption
//                }
//            ]
//        });
//    }
//};
//const labelOption = {
//    show: true,
//    position: app.config.position,
//    distance: app.config.distance,
//    align: app.config.align,
//    verticalAlign: app.config.verticalAlign,
//    rotate: app.config.rotate,
//    formatter: '{c}  {name|{a}}',
//    fontSize: 16,
//    rich: {
//        name: {}
//    }
//};
//option9 = {
//    tooltip: {
//        trigger: 'axis',
//        axisPointer: {
//            type: 'shadow'
//        }
//    },
//    legend: {
//        data: ['Insurance', 'Workshop']
//    },
//    toolbox: {
//        show: true,
//        orient: 'vertical',
//        left: 'right',
//        top: 'center',
//        feature: {
//            mark: { show: true },
//            dataView: { show: true, readOnly: false },
//            magicType: { show: true, type: ['line', 'bar', 'stack'] },
//            restore: { show: true },
//            saveAsImage: { show: true }
//        }
//    },
//    xAxis: [
//        {
//            type: 'category',
//            axisTick: { show: false },
//            data: ['10/3/2022', '23/4/2022', '25/5/2022', '22/7/2022', '3/8/2022']
//        }
//    ],
//    yAxis: [
//        {
//            type: 'value'
//        }
//    ],
//    series: [
//        {
//            name: 'Insurance',
//            type: 'bar',
//            label: labelOption,
//            emphasis: {
//                focus: 'series'
//            },
//            data: [150, 232, 201, 154, 190]
//        },
//        {
//            name: 'Workshop',
//            type: 'bar',
//            label: labelOption,
//            emphasis: {
//                focus: 'series'
//            },
//            data: [98, 77, 101, 99, 40]
//        }
//    ]
//};

//option9 && myChart9.setOption(option9);
//}
//catch
//{ }

///*End Insurance Dashboart Code*/

///*Start Payments Due Dashboart Code*/
//try { 
//var chartDom11 = document.getElementById('main11');
//var myChart11 = echarts.init(chartDom11);
//var option11;

//option11 = {
//    tooltip: {
//        trigger: 'axis',
//        axisPointer: {
//            // Use axis to trigger tooltip
//            type: 'shadow' // 'shadow' as default; can also be 'line' or 'shadow'
//        }
//    },
//    legend: {},
//    grid: {
//        left: '3%',
//        right: '4%',
//        bottom: '3%',
//        containLabel: true
//    },
//    xAxis: {
//        type: 'value'
//    },
//    yAxis: {
//        type: 'category',
//        data: [
//            '14/01/2022',
//            '20/02/2022',
//            '05/03/2022',
//            '22/04/2022',
//            '14/05/2022',
//            '18/06/2022',
//            '14/07/2022'
//        ]
//    },
//    series: [
//        {
//            name: 'Abdallah Labib',
//            type: 'bar',
//            stack: 'total',
//            label: {
//                show: true
//            },
//            emphasis: {
//                focus: 'series'
//            },
//            data: [120, 132, 101, 134, 90, 230, 210]
//        },
//        {
//            name: 'Malak Majed',
//            type: 'bar',
//            stack: 'total',
//            label: {
//                show: true
//            },
//            emphasis: {
//                focus: 'series'
//            },
//            data: [220, 182, 191, 234, 290, 330, 310]
//        },
//        {
//            name: 'Ghadeer Khaled',
//            type: 'bar',
//            stack: 'total',
//            label: {
//                show: true
//            },
//            emphasis: {
//                focus: 'series'
//            },
//            data: [150, 212, 201, 154, 190, 330, 410]
//        },
//        {
//            name: 'Yousef Waleed',
//            type: 'bar',
//            stack: 'total',
//            label: {
//                show: true
//            },
//            emphasis: {
//                focus: 'series'
//            },
//            data: [820, 832, 901, 934, 1290, 1330, 1320]
//        },
//        {
//            name: 'Kia Picanto',
//            type: 'bar',
//            stack: 'total',
//            label: {
//                show: true
//            },
//            emphasis: {
//                focus: 'series'
//            },
//            data: [320, 302, 301, 334, 390, 330, 320]
//        }
//    ]
//};

//option11 && myChart11.setOption(option11);
//}
//catch
//{ }

