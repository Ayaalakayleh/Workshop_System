// Technician Available Hours Chart - Workshop System

import ApexCharts from '../thirdparty/apexchartsWrapper.js';

document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    if (document.getElementById('development-phases-chart')) {

        /***************************************************************/
        /* Color helpers (robust to theme differences)                 */
        /***************************************************************/
        const COLOR_AVAILABLE =
            (window.colorMap?.primary?.[400]?.hex) ||
            (window.colorMap?.primary?.hex) ||
            '#0d6efd';

        const COLOR_BUSY =
            (window.colorMap?.warning?.[400]?.hex) ||
            (window.colorMap?.warning?.hex) ||
            '#ffc107';

        // neutral gray for breaks; fallbacks avoid the earlier errors
        const COLOR_BREAK =
            (window.colorMap?.secondary?.hex) ||
            (window.colorMap?.bootstrapVars?.bodyColor?.hex) ||
            '#6c757d';

        const getStatusColor = (status) => {
            switch (status) {
                case 'available': return COLOR_AVAILABLE;
                case 'break': return COLOR_BREAK;
                case 'busy': return COLOR_BUSY;
                default: return null; // "off" or unknown → no bar
            }
        };

        /***************************************************************/
        /* Generate Technician Availability Data                       */
        /***************************************************************/
        const generateTechnicianData = () => {
            // Create 30 technicians
            const technicians = Array.from({ length: 30 }, (_, i) => ({
                id: i + 1,
                name: `Technician ${i + 1}`
            }));

            const timelineData = [];
            let taskId = 1;

            const baseDate = new Date();
            baseDate.setMinutes(0, 0, 0);

            // track min/max to set x-axis range dynamically
            let earliestHour = 24;
            let latestHour = 0;

            technicians.forEach(tech => {
                // different business hours per technician:
                // start between 7–10, shift length 6–10 hours, cap at 20:00
                const startHour = 7 + Math.floor(Math.random() * 4);      // 7..10
                const shiftLength = 6 + Math.floor(Math.random() * 5);    // 6..10
                const endHour = Math.min(startHour + shiftLength, 20);    // end ≤ 20

                // lunch break ~ middle of shift (1 hour)
                const mid = startHour + Math.floor(shiftLength / 2);
                const lunchStart = Math.min(Math.max(startHour + 3, mid - 1), endHour - 1);
                const lunchEnd = Math.min(lunchStart + 1, endHour);

                // busy window (1–3 hours) after lunch, but before end
                const afterLunchSpan = Math.max(0, endHour - lunchEnd - 2);
                const busyStart = afterLunchSpan > 0
                    ? lunchEnd + Math.floor(Math.random() * afterLunchSpan)
                    : lunchEnd;
                const busyDuration = Math.min(3, Math.max(1, endHour - busyStart));
                const busyEnd = Math.min(endHour, busyStart + busyDuration);

                // Compose non-overlapping segments inside working hours.
                // Anything outside [startHour, endHour] is OFF (no bars).
                const segments = [];
                if (startHour < lunchStart)
                    segments.push({ start: startHour, end: lunchStart, status: 'available' });
                if (lunchStart < lunchEnd)
                    segments.push({ start: lunchStart, end: lunchEnd, status: 'break' });
                if (lunchEnd < busyStart)
                    segments.push({ start: lunchEnd, end: busyStart, status: 'available' });
                if (busyStart < busyEnd)
                    segments.push({ start: busyStart, end: busyEnd, status: 'busy' });
                if (busyEnd < endHour)
                    segments.push({ start: busyEnd, end: endHour, status: 'available' });

                earliestHour = Math.min(earliestHour, startHour);
                latestHour = Math.max(latestHour, endHour);

                timelineData.push({
                    id: taskId++,
                    name: tech.name,
                    data: segments.map(s => {
                        const start = new Date(baseDate); start.setHours(s.start, 0, 0, 0);
                        const end = new Date(baseDate); end.setHours(s.end, 0, 0, 0);
                        return {
                            x: tech.name,
                            y: [start.getTime(), end.getTime()],
                            fillColor: getStatusColor(s.status),
                            status: s.status
                        };
                    })
                });
            });

            const xMin = (() => {
                const d = new Date(baseDate);
                d.setHours(Math.max(earliestHour, 7), 0, 0, 0);
                return d.getTime();
            })();

            const xMax = (() => {
                const d = new Date(baseDate);
                d.setHours(Math.min(Math.max(latestHour, earliestHour + 1), 20), 0, 0, 0);
                return d.getTime();
            })();

            return { timelineData, xMin, xMax };
        };

        const { timelineData, xMin, xMax } = generateTechnicianData();

        // make chart tall enough for 30 rows
        const chartHeight = Math.max(600, timelineData.length * 34);

        /***************************************************************/
        /* Chart Options                                               */
        /***************************************************************/
        const technicianHoursOptions = {
            series: timelineData,
            chart: {
                height: chartHeight,
                type: 'rangeBar',
                fontFamily: 'inherit',
                parentHeightOffset: 0,
                animations: { enabled: false },
                zoom: { enabled: false },
                toolbar: { show: false }
            },
            plotOptions: {
                bar: {
                    horizontal: true,
                    rangeBarGroupRows: true,
                    dataLabels: { hideOverflowingLabels: true }
                }
            },
            dataLabels: { enabled: false },
            xaxis: {
                type: 'datetime',
                position: 'top',
                labels: {
                    style: {
                        colors: window.colorMap.bootstrapVars.bodyColor.hex,
                        fontSize: '11px'
                    },
                    datetimeUTC: false,
                    format: 'HH:mm'
                },
                min: xMin,
                max: xMax
            },
            yaxis: {
                labels: {
                    style: {
                        colors: window.colorMap.bootstrapVars.bodyColor.hex,
                        fontSize: '11px'
                    },
                    maxWidth: 220
                }
            },
            grid: {
                borderColor: window.colorMap.bootstrapVars.bodyColor.rgba(0.1),
                strokeDashArray: 3,
                xaxis: { lines: { show: true } },
                yaxis: { lines: { show: false } }
            },
            tooltip: {
                custom: function (opts) {
                    const data = opts.w.config.series[opts.seriesIndex].data[opts.dataPointIndex];
                    const from = new Date(opts.y1).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                    const to = new Date(opts.y2).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

                    return (
                        '<div class="arrow_box p-2">' +
                        '<div class="fw-bold mb-1">' + opts.w.config.series[opts.seriesIndex].name + '</div>' +
                        '<div>Status: <span class="fw-bold text-uppercase">' + data.status + '</span></div>' +
                        '<div><small>From: ' + from + '</small></div>' +
                        '<div><small>To: ' + to + '</small></div>' +
                        '</div>'
                    );
                }
            },
            legend: { show: false },
            annotations: { xaxis: [] } // no milestone labels
        };

        /***************************************************************/
        /* Render Chart                                                */
        /***************************************************************/
        const technicianHoursChart = new ApexCharts(
            document.getElementById('development-phases-chart'),
            technicianHoursOptions
        );

        technicianHoursChart.render();
    }
});
