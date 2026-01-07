// Service Reminder Edit Page JavaScript
$(document).ready(function() {
    // Show/Hide logic for dependent fields
    function ShowHideChilds(element, checked) {
        let $el = $(element);

        if (checked) {
            $el.removeClass('collapse').addClass('expanded');
            $el.show();
        }
        else {
            $el.removeClass('expanded').addClass('collapse');
            $el.hide();
        }
    }

    $("#IsManually").on("change", function () {
        ShowHideChilds($("#ManualControl"), $("#IsManually").prop("checked"));
    });

    $("#HasNotification").on("change", function () {
        ShowHideChilds($("#NoticationsControl"), $("#HasNotification").prop("checked"));
    });

    $("#UseSameStart").on("change", function () {
        var isChecked = $(this).prop("checked");
        ShowHideChilds($("#StartDateControl"), !isChecked);
    });

    // Initialize the state on page load
    ShowHideChilds($("#ManualControl"), $("#IsManually").prop("checked"));
    ShowHideChilds($("#NoticationsControl"), $("#HasNotification").prop("checked"));
    var useSameStartChecked = $("#UseSameStart").prop("checked");
    ShowHideChilds($("#StartDateControl"), !useSameStartChecked);

    // Manufacturer-Model dependent dropdown
    $("#ManufacturerId").on("change", function () {
        var manufacturerId = $(this).val();
        var $modelSelect = $('#VehicleModelId');

        // Clear current options
        $modelSelect.empty().append(
            $('<option>', { value: '', text: RazorVars.placeholderSelect || 'Select' })
        );

        if (manufacturerId) {
            // Make AJAX call to get models for selected manufacturer
            $.get(RazorVars.getVehicleByManufacturerIdURL, { manufacturerId: manufacturerId }, function (data) {
                $.each(data, function (_, item) {
                    $modelSelect.append(
                        $('<option>', { value: item.id, text: item.name })
                    );
                });
            }).fail(function() {
                console.error("Error loading vehicle models for manufacturer:", manufacturerId);
            });
        }
    });
});

// Global Select2 + Bootstrap 5 setup for Edit page
(function ($) {
    function initSelect2(context) {
        var $ctx = context ? $(context) : $(document);

        $ctx.find('select').each(function () {
            var $select = $(this);

            // Skip if explicitly disabled
            if ($select.hasClass('no-select2')) {
                return;
            }

            // Destroy any previous instance so we can reconfigure safely
            if ($select.data('select2')) {
                $select.select2('destroy');
            }

            var options = {
                theme: 'bootstrap-5', // requires Select2 Bootstrap 5 theme CSS
                width: '100%',
                placeholder: RazorVars.placeholderSelect || 'Select'
            };

            // Allow clear only for non-required single selects
            if (!$select.is('[multiple]') && !$select.prop('required')) {
                options.allowClear = true;
            }

            $select.select2(options);
        });
    }

    $(document).ready(function () {
        // Initial setup for all selects on the page
        initSelect2();

        // When any Bootstrap 5 modal is shown, re-init selects inside it
        $(document).on('shown.bs.modal', '.modal', function () {
            initSelect2(this);
        });

        // Fix Bootstrap's focus trap so the Select2 search field stays focusable & typeable
        document.addEventListener('focusin', function (e) {
            if (e.target.classList &&
                e.target.classList.contains('select2-search__field')) {
                e.stopPropagation();
            }
        }, true);
    });

})(jQuery);

// Schedule Preview functionality
$(document).ready(function() {
    // Function to calculate and display schedule preview
    function updateSchedulePreview() {
        var timeInterval = parseFloat($('#ReminderForm_TimeInterval').val()) || 0;
        var timeIntervalUnit = parseInt($('#ReminderForm_TimeIntervalUnit').val()) || 0;
        var timeDue = parseFloat($('#ReminderForm_TimeDue').val()) || 0;
        var timeDueUnit = parseInt($('#ReminderForm_TimeDueUnit').val()) || 0;
        var repeats = parseInt($('#ReminderForm_Repates').val()) || 1;
        var isManually = $('#IsManually').is(':checked');
        var useSameStart = $('#UseSameStart').is(':checked');
        var startDateValue = $('#ReminderForm_StartDate').val();

        // Clear previous preview
        $('#nextServiceDates').empty();
        $('#intervalDisplay').text('-');
        $('#repeatsDisplay').text('-');
        $('#nextDueDisplay').text('-');

        if (timeInterval <= 0 || isManually) {
            $('#nextServiceDates').append('<div class="list-group-item">No automatic schedule configured</div>');
            return;
        }

        // Determine base date for calculations
        var baseDate = new Date();
        if (!useSameStart && startDateValue) {
            // Use custom start date if provided and UseSameStart is unchecked
            baseDate = new Date(startDateValue);
        }
        var nextServiceDates = [];
        var intervalText = '';

        // Determine interval text
        switch (timeIntervalUnit) {
            case 1: // Days
                intervalText = timeInterval + ' ' + (timeInterval === 1 ? 'Day' : 'Days');
                break;
            case 2: // Weeks
                intervalText = timeInterval + ' ' + (timeInterval === 1 ? 'Week' : 'Weeks');
                break;
            case 3: // Months
                intervalText = timeInterval + ' ' + (timeInterval === 1 ? 'Month' : 'Months');
                break;
            case 4: // Years
                intervalText = timeInterval + ' ' + (timeInterval === 1 ? 'Year' : 'Years');
                break;
            default:
                intervalText = timeInterval + ' units';
        }

        // Calculate next dates
        for (var i = 0; i < Math.min(repeats, 5); i++) { // Show up to 5 upcoming dates
            var nextDate = new Date(baseDate);

            switch (timeIntervalUnit) {
                case 1: // Days
                    nextDate.setDate(nextDate.getDate() + (timeInterval * (i + 1)));
                    break;
                case 2: // Weeks
                    nextDate.setDate(nextDate.getDate() + (timeInterval * 7 * (i + 1)));
                    break;
                case 3: // Months
                    nextDate.setMonth(nextDate.getMonth() + (timeInterval * (i + 1)));
                    break;
                case 4: // Years
                    nextDate.setFullYear(nextDate.getFullYear() + (timeInterval * (i + 1)));
                    break;
            }

            nextServiceDates.push(nextDate);
        }

        // Display schedule summary
        $('#intervalDisplay').text(intervalText);
        $('#repeatsDisplay').text(repeats + ' ' + (repeats === 1 ? 'time' : 'times'));

        // Calculate next due date
        var nextDueDate = new Date(baseDate);
        switch (timeDueUnit) {
            case 1: // Days
                nextDueDate.setDate(nextDueDate.getDate() + timeDue);
                break;
            case 2: // Weeks
                nextDueDate.setDate(nextDueDate.getDate() + (timeDue * 7));
                break;
            case 3: // Months
                nextDueDate.setMonth(nextDueDate.getMonth() + timeDue);
                break;
            case 4: // Years
                nextDueDate.setFullYear(nextDueDate.getFullYear() + timeDue);
                break;
        }
        $('#nextDueDisplay').text(nextDueDate.toLocaleDateString());

        // Display next service dates
        nextServiceDates.forEach(function(date, index) {
            var dateItem = '<div class="list-group-item d-flex justify-content-between align-items-center">' +
                'Service ' + (index + 1) + ': ' + date.toLocaleDateString() +
                '<span class="badge badge-primary badge-pill">' + Math.round((date - baseDate) / (1000 * 60 * 60 * 24)) + ' days</span>' +
                '</div>';
            $('#nextServiceDates').append(dateItem);
        });
    }

    // Bind events to form inputs to update preview in real-time
    $('#ReminderForm_TimeInterval, #ReminderForm_TimeIntervalUnit, #ReminderForm_TimeDue, #ReminderForm_TimeDueUnit, #ReminderForm_Repates, #IsManually, #UseSameStart, #ReminderForm_StartDate').on('change input', function() {
        updateSchedulePreview();
    });

    // Initial preview update
    setTimeout(updateSchedulePreview, 1000); // Delay to ensure form is fully loaded

    // Helper function to convert time values to days for comparison
    function convertToDays(value, unit) {
        switch (parseInt(unit)) {
            case 1: // Days
                return value;
            case 2: // Weeks
                return value * 7;
            case 3: // Months (approximate as 30 days)
                return value * 30;
            case 4: // Years (approximate as 365 days)
                return value * 365;
            default:
                return value;
        }
    }

    // Real-time validation for Primary Meter Due vs Interval
    function validatePrimaryMeterFields() {
        var intervalField = $('#ReminderForm_PrimaryMeterInterval');
        var dueField = $('#ReminderForm_PrimaryMeterDue');
        var intervalVal = parseFloat(intervalField.val()) || 0;
        var dueVal = parseFloat(dueField.val()) || 0;

        // Remove existing error styling
        intervalField.removeClass('is-invalid');
        dueField.removeClass('is-invalid');
        $('.primary-meter-error').remove();

        // Check if due value exceeds interval value
        if (dueVal > intervalVal && intervalVal > 0) {
            // Add error styling
            dueField.addClass('is-invalid');
            intervalField.addClass('is-invalid');

            // Add error message below the input-group containers
            var errorMsg = '<div class="invalid-feedback primary-meter-error d-block">Primary Meter Due cannot be greater than Primary Meter Interval.</div>';

            // Add error message after the Primary Meter Due input-group
            dueField.closest('.input-group').after(errorMsg);

            return false;
        }

        return true;
    }

    // Real-time validation for Time Due vs Time Interval
    function validateTimeFields() {
        var intervalField = $('#ReminderForm_TimeInterval');
        var intervalUnitField = $('#ReminderForm_TimeIntervalUnit');
        var dueField = $('#ReminderForm_TimeDue');
        var dueUnitField = $('#ReminderForm_TimeDueUnit');

        var intervalVal = parseFloat(intervalField.val()) || 0;
        var intervalUnit = parseInt(intervalUnitField.val()) || 1;
        var dueVal = parseFloat(dueField.val()) || 0;
        var dueUnit = parseInt(dueUnitField.val()) || 1;

        // Remove existing error styling
        intervalField.removeClass('is-invalid');
        dueField.removeClass('is-invalid');
        $('.time-interval-error').remove();

        // Check if due value exceeds interval value (convert both to days for comparison)
        if (dueVal > 0 && intervalVal > 0) {
            var intervalInDays = convertToDays(intervalVal, intervalUnit);
            var dueInDays = convertToDays(dueVal, dueUnit);

            if (dueInDays > intervalInDays) {
                // Add error styling
                dueField.addClass('is-invalid');
                intervalField.addClass('is-invalid');

                // Add error message below the input-group containers
                var errorMsg = '<div class="invalid-feedback time-interval-error d-block">Time Due cannot be greater than Time Interval.</div>';

                // Add error message after the Time Due input-group
                dueField.closest('.input-group').after(errorMsg);

                return false;
            }
        }

        return true;
    }

    // Bind validation to input changes
    $('#ReminderForm_PrimaryMeterInterval, #ReminderForm_PrimaryMeterDue').on('input change', function() {
        validatePrimaryMeterFields();
    });

    $('#ReminderForm_TimeInterval, #ReminderForm_TimeIntervalUnit, #ReminderForm_TimeDue, #ReminderForm_TimeDueUnit').on('input change', function() {
        validateTimeFields();
    });

    // Form validation - ensure Manufacturing Year is required
    $('#serviceReminderForm').on('submit', function(e) {
        var manufacturingYear = $('#ReminderForm_ManufacturingYear').val();
        if (!manufacturingYear || manufacturingYear.trim() === '') {
            e.preventDefault();
            $('#ReminderForm_ManufacturingYear').focus();
            return false;
        }

        // Validate that it's a valid year (reasonable range)
        var yearValue = parseInt(manufacturingYear);
        var currentYear = new Date().getFullYear();
        if (isNaN(yearValue) || yearValue < 1900 || yearValue > currentYear + 1) {
            e.preventDefault();
            $('#ReminderForm_ManufacturingYear').focus();
            return false;
        }

        // Validate Primary Meter constraint
        if (!validatePrimaryMeterFields()) {
            e.preventDefault();
            $('#ReminderForm_PrimaryMeterDue').focus();
            return false;
        }

        // Validate Time constraint
        if (!validateTimeFields()) {
            e.preventDefault();
            $('#ReminderForm_TimeDue').focus();
            return false;
        }
    });
});
