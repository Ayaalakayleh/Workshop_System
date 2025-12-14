$(document).ready(function () {
    getData(1);
    $(".select2").select2();

});
function getData(page) {
    page = page || 1;
    var FilterModel = {
        'Name': $('#_Name').val(),
        'Date': $('#selectedDate').val(),
        'PageNumber': page
    };
    debugger;
    console.log(FilterModel);
    $.ajax({
        type: 'GET',
        url: window.RazorVars.getWorkScheduleIndexUrl,
        dataType: 'html',
        data: FilterModel
    }).done(function (result) {
        $("#contentList").html(result);
        if ($("#total_pages").val() != undefined) {
            $("#contentPager").attr("hidden", false);
            $('#contentPager').pagination('destroy');
            $('#contentPager').pagination({
                items: $("#total_pages").val(),
                itemOnPage: 25,
                currentPage: page,
                prevText: '&laquo;',
                nextText: '&raquo;',
                onPageClick: function (page, evt) {
                    getData(page);
                }
            });

            // Force Bootstrap classes
            $('#contentPager ul').addClass('pagination');
            $('#contentPager li').addClass('page-item');
            $('#contentPager a, #contentPager span').addClass('page-link');
            $('.main-table').DataTable({
                responsive: true,
                pageLength: 10,
                info: false,
                paging: false,
                dom: 'tp'
            });
        }
        else {
            $("#contentPager").attr("hidden", true);
        }
    });

}
$("#btnSearch").click(function () {
    getData();
});


$(document).ready(function () {
    // Initialize jQuery validation for the work schedule form
    $("#workshopform").validate({
        rules: {
            // Technician selection rules
            TechnicianIds_List: {
                required: function () {
                    // Only required when adding new schedule (FK_TechnicianId is 0)
                    return $("#FK_TechnicianId").val() == "0";
                }
            },
            Name: {
                required: function () {
                    // Only required when editing existing schedule (FK_TechnicianId > 0)
                    return $("#FK_TechnicianId").val() != "0";
                }
            },

            // Date validation rules
            WorkingDateFrom: {
                required: true,
                date: true
            },
            WorkingDateTo: {
                required: true,
                date: true,
                dateGreaterThan: "#WorkingDateFrom"
            },

            // Time validation rules
            WorkingTimeFrom: {
                required: true,
                time: true
            },
            WorkingTimeTo: {
                required: true,
                time: true,
                timeGreaterThan: "#WorkingTimeFrom"
            },

            // Slot selection
            FK_SlotMinutes_Id: {
                required: true,
                min: 1
            }
        },

        messages: {
            TechnicianIds_List: {
                required: "Please select at least one technician"
            },
            Name: {
                required: "Technician name is required"
            },
            WorkingDateFrom: {
                required: "Working start date is required",
                date: "Please enter a valid date"
            },
            WorkingDateTo: {
                required: "Working end date is required",
                date: "Please enter a valid date",
                dateGreaterThan: "End date must be after start date"
            },
            WorkingTimeFrom: {
                required: "Working start time is required",
                time: "Please enter a valid time"
            },
            WorkingTimeTo: {
                required: "Working end time is required",
                time: "Please enter a valid time",
                timeGreaterThan: "End time must be after start time"
            },
            FK_SlotMinutes_Id: {
                required: "Please select a time slot",
                min: "Please select a valid time slot"
            }
        },

        errorElement: "span",
        errorClass: "text-danger",
        errorPlacement: function (error, element) {
            // Handle phone input error placement
            if (element.attr("name") === "Phone") {
                error.insertAfter(element.closest('.iti'));
            }
            // Handle Select2 error placement
            else if (element.hasClass("select2-hidden-accessible")) {
                error.insertAfter(element.next('.select2')); // insert after Select2 container
            }
            // Default placement
            else {
                error.insertAfter(element);
            }
        },

        highlight: function (element, errorClass, validClass) {
            $(element).addClass("is-invalid").removeClass("is-valid");
        },

        unhighlight: function (element, errorClass, validClass) {
            $(element).removeClass("is-invalid").addClass("is-valid");
        },

        submitHandler: function (form) {
            // Additional validation before submit
            if (validateTimeRange() && validateDateRange()) {
                form.submit();
            }
            return false;
        }
    });

    // Custom validation methods

    // Date comparison validator
    $.validator.addMethod("dateGreaterThan", function (value, element, param) {
        if (!value || !$(param).val()) return true;

        var startDate = new Date($(param).val());
        var endDate = new Date(value);

        return endDate >= startDate;
    }, "End date must be equal to or after start date");

    // Time comparison validator
    $.validator.addMethod("timeGreaterThan", function (value, element, param) {
        if (!value || !$(param).val()) return true;

        var startTime = $(param).val();
        var endTime = value;

        // Convert time strings to minutes for comparison
        var startMinutes = timeToMinutes(startTime);
        var endMinutes = timeToMinutes(endTime);

        return endMinutes > startMinutes;
    }, "End time must be after start time");

    // Time format validator
    $.validator.addMethod("time", function (value, element) {
        if (!value) return true;
        return /^([01]?[0-9]|2[0-3]):[0-5][0-9]$/.test(value);
    }, "Please enter a valid time in HH:MM format");

    // Helper functions
    function timeToMinutes(timeString) {
        if (!timeString) return 0;
        var parts = timeString.split(':');
        return parseInt(parts[0]) * 60 + parseInt(parts[1]);
    }

    function validateTimeRange() {
        var startTime = $("#WorkingTimeFrom").val();
        var endTime = $("#WorkingTimeTo").val();

        if (startTime && endTime) {
            var startMinutes = timeToMinutes(startTime);
            var endMinutes = timeToMinutes(endTime);

            if (endMinutes <= startMinutes) {
                showValidationError("Working end time must be after start time");
                return false;
            }

            // Check if time range is reasonable (at least 30 minutes)
            if ((endMinutes - startMinutes) < 30) {
                showValidationError("Working time range must be at least 30 minutes");
                return false;
            }
        }
        return true;
    }

    function validateDateRange() {
        var startDate = $("#WorkingDateFrom").val();
        var endDate = $("#WorkingDateTo").val();

        if (startDate && endDate) {
            var start = new Date(startDate);
            var end = new Date(endDate);
            var today = new Date();
            today.setHours(0, 0, 0, 0);

            // Check if start date is not in the past
            if (start < today) {
                showValidationError("Working start date cannot be in the past");
                return false;
            }

            // Check if date range is not too long (e.g., max 1 year)
            var maxDate = new Date(start);
            maxDate.setFullYear(maxDate.getFullYear() + 1);

            if (end > maxDate) {
                showValidationError("Working period cannot exceed 1 year");
                return false;
            }
        }
        return true;
    }

    function showValidationError(message) {
        // You can customize this to show errors in your preferred way
        alert(message);
        // Or use a toast notification, modal, etc.
    }

    // Real-time validation on field changes
    $("#WorkingDateFrom, #WorkingDateTo").on('change', function () {
        $("#workshopform").valid();
        validateDateRange();
    });

    $("#WorkingTimeFrom, #WorkingTimeTo").on('change', function () {
        $("#workshopform").valid();
        validateTimeRange();
    });

    // Initialize Select2 validation
    $('.select2').on('change', function () {
        $(this).valid();
    });
});

$(document).ready(function () {
    // Initialize the date-time range picker
    initializeDateTimeRangePicker();
});

function initializeDateTimeRangePicker() {
    $('#workingDateTimeRange').daterangepicker({
        timePicker: true,
        timePicker24Hour: true,
        timePickerIncrement: 15,
        startDate: moment().add(1, 'hour'),
        endDate: moment().add(9, 'hour'),
        locale: {
            format: 'YYYY-MM-DD HH:mm'
        },
        autoUpdateInput: false
    }, function (start, end, label) {
        handleDateTimeRangeChange(start, end);
    });

    // Handle the apply event
    $('#workingDateTimeRange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY-MM-DD HH:mm') + ' - ' + picker.endDate.format('YYYY-MM-DD HH:mm'));
        handleDateTimeRangeChange(picker.startDate, picker.endDate);
    });

    // Handle the cancel event
    $('#workingDateTimeRange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        clearDisplayValues();
        clearHiddenFields();
    });

    // Load existing values if available
    loadExistingValues();
}

function handleDateTimeRangeChange(startMoment, endMoment) {
    // Extract date and time components
    const startDate = startMoment.format('YYYY-MM-DD');
    const endDate = endMoment.format('YYYY-MM-DD');
    const startTime = startMoment.format('HH:mm:ss');
    const endTime = endMoment.format('HH:mm:ss');

    // Update display values in p tags
    updateDisplayValues(startDate, endDate, startTime, endTime);

    // Update hidden fields
    updateHiddenFields(startDate, endDate, startTime, endTime);
}

function updateDisplayValues(startDate, endDate, startTime, endTime) {
    $('#selectedFromDate').text(moment(startDate).format('MMM DD, YYYY'));
    $('#selectedToDate').text(moment(endDate).format('MMM DD, YYYY'));
    $('#selectedFromTime').text(moment(startTime, 'HH:mm:ss').format('HH:mm'));
    $('#selectedToTime').text(moment(endTime, 'HH:mm:ss').format('HH:mm'));
}

function clearDisplayValues() {
    $('#selectedFromDate').text('Not selected');
    $('#selectedToDate').text('Not selected');
    $('#selectedFromTime').text('Not selected');
    $('#selectedToTime').text('Not selected');
}

function loadExistingValues() {
    const workingDateFrom = $('#WorkingDateFrom').val();
    const workingDateTo = $('#WorkingDateTo').val();
    const workingTimeFrom = $('#WorkingTimeFrom').val();
    const workingTimeTo = $('#WorkingTimeTo').val();

    if (workingDateFrom && workingDateTo && workingTimeFrom && workingTimeTo) {
        // Parse existing values
        const startDateTime = moment(`${workingDateFrom} ${workingTimeFrom}`, 'YYYY-MM-DD HH:mm:ss');
        const endDateTime = moment(`${workingDateTo} ${workingTimeTo}`, 'YYYY-MM-DD HH:mm:ss');

        // Set the daterangepicker value
        const picker = $('#workingDateTimeRange').data('daterangepicker');
        if (picker) {
            picker.setStartDate(startDateTime);
            picker.setEndDate(endDateTime);
            $('#workingDateTimeRange').val(startDateTime.format('YYYY-MM-DD HH:mm') + ' - ' + endDateTime.format('YYYY-MM-DD HH:mm'));

            // Update display values
            updateDisplayValues(
                workingDateFrom,
                workingDateTo,
                workingTimeFrom.substring(0, 5), // Remove seconds for display
                workingTimeTo.substring(0, 5)    // Remove seconds for display
            );
        }
    }
}

function updateHiddenFields(startDate, endDate, startTime, endTime) {
    $('#WorkingDateFrom').val(startDate);
    $('#WorkingDateTo').val(endDate);
    $('#WorkingTimeFrom').val(startTime);
    $('#WorkingTimeTo').val(endTime);
}

function clearHiddenFields() {
    $('#WorkingDateFrom').val('');
    $('#WorkingDateTo').val('');
    $('#WorkingTimeFrom').val('');
    $('#WorkingTimeTo').val('');
}

// Utility function to get the current daterangepicker instance
function getDateTimeRangePicker() {
    return $('#workingDateTimeRange').data('daterangepicker');
}

// Function to programmatically set the date range (useful for external calls)
function setDateTimeRange(startDate, endDate, startTime, endTime) {
    const startDateTime = moment(`${startDate} ${startTime}`, 'YYYY-MM-DD HH:mm:ss');
    const endDateTime = moment(`${endDate} ${endTime}`, 'YYYY-MM-DD HH:mm:ss');

    const picker = getDateTimeRangePicker();
    if (picker && startDateTime.isValid() && endDateTime.isValid()) {
        picker.setStartDate(startDateTime);
        picker.setEndDate(endDateTime);
        $('#workingDateTimeRange').val(startDateTime.format('YYYY-MM-DD HH:mm') + ' - ' + endDateTime.format('YYYY-MM-DD HH:mm'));
        updateDisplayValues(startDate, endDate, startTime, endTime);
    }
}

// Function to clear the date range selection
function clearDateTimeRange() {
    const picker = getDateTimeRangePicker();
    if (picker) {
        picker.setStartDate(moment());
        picker.setEndDate(moment().add(1, 'hour'));
        $('#workingDateTimeRange').val('');
    }
    clearHiddenFields();
    clearDisplayValues();
}