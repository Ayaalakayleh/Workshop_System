flatpickr("#basic-datepicker", {
    dateFormat: "Y-m-d"
});

flatpickr("#datetime-picker", {
    enableTime: true,
    dateFormat: "Y-m-d H:i",
    time_24hr: true
});

flatpickr("#time-picker", {
    enableTime: true,
    noCalendar: true,
    dateFormat: "H:i",
    time_24hr: true
});

flatpickr("#range-picker", {
    mode: "range",
    dateFormat: "Y-m-d"
});