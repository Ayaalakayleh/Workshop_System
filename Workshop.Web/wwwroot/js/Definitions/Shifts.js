
$(function () {
    // ----- Validation -----
    $("#shiftForm").validate({
        rules: {
            Code: { required: true },
            Short: { required: true },
            PrimaryName: { required: true },

            WorkingFromTime: { required: true },
            WorkingToTime: { required: true },
            BreakFromTime: { required: false },
            BreakToTime: { required: false }
        },
        messages: {
            Code: { required: RazorVars.required_field },
            Short: { required: RazorVars.required_field },
            PrimaryName: { required: RazorVars.required_field },

            WorkingFromTime: { required: RazorVars.required_field },
            WorkingToTime: { required: RazorVars.required_field },
            BreakFromTime: { required: RazorVars.required_field },
            BreakToTime: { required: RazorVars.required_field }
        },

        // group each pair into a single validation message
        groups: {
            WorkingTimeGroup: "WorkingFromTime WorkingToTime",
            BreakTimeGroup: "BreakFromTime BreakToTime"
        },

        errorPlacement: function (error, element) {
            const name = element.attr("name");

            if (name === "WorkingFromTime" || name === "WorkingToTime") {
                // send the single group error to the range placeholder
                error.appendTo("#WorkingTimeError");
            } else if (name === "BreakFromTime" || name === "BreakToTime") {
                error.appendTo("#BreakTimeError");
            } else {
                // normal fields
                error.insertAfter(element);
            }
        },

        errorClass: "text-danger",
        errorElement: "span",
        highlight: function (element) { $(element).addClass("is-invalid"); },
        unhighlight: function (element) { $(element).removeClass("is-invalid"); }
    });


    // Initial load
    loadShifts();

    // Search: rebind safely
    $(document).off("click", "#btnSearch").on("click", "#btnSearch", function () {
        loadShifts();
    });

    // Cancel: just close the modal (no Swal)
    $(document).off("click", "#btnCancel").on("click", "#btnCancel", function (e) {
        e.preventDefault();
        $("#shiftModal").modal("hide");
        return false;
    });

    // Submit: prevent double binding + double submission
    $(document).off("submit", "#shiftForm").on("submit", "#shiftForm", function (e) {
        e.preventDefault();

        // Use jQuery Validate result
        if (!$("#shiftForm").valid()) return;

        // Single-flight guard (prevents duplicates on rapid clicks / multiple binds)
        const $form = $(this);
        if ($form.data("isSubmitting")) return;
        $form.data("isSubmitting", true);

        // Disable submit button to prevent double click
        const $submitBtn = $form.find('[type="submit"], #btnSave');
        $submitBtn.prop("disabled", true);

        // Keep DOW in sync right before submit
        updateDOWSummary();

        const formData = $form.serialize();

        $.ajax({
            url: RazorVars.editUrl,
            type: "POST",
            data: formData,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            }
        })
            .done(function (response) {
                if (response.success) {
                    $("#shiftModal").modal("hide");

                    Swal.fire({
                        icon: "success",
                        title: resources.success_msg,
                        confirmButtonText: RazorVars.btnOk,
                        confirmButtonColor: "var(--primary-600)",
                        timer: 2000,
                        timerProgressBar: true
                    }).then(() => {
                        loadShifts();
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: RazorVars.required_week_days,
                        confirmButtonText: RazorVars.btnOk,
                        confirmButtonColor: "var(--primary-600)"
                    });
                }
            })
            .fail(function () {
                Swal.fire(RazorVars.swalErrorHappened, "error");
            })
            .always(function () {
                $form.data("isSubmitting", false);
                $submitBtn.prop("disabled", false);
            });
    });
});

// ----- Helpers you already had -----
function resetFilter() {
    $('#Name').val('');
    $('#Code').val('');
    loadShifts();
}

function resetModal() {
    $('#shiftForm')[0].reset();
    $('#ShiftId').val('0');
    $('#DOW').val('');
    $('.day-checkbox').prop('checked', false);
    $('#SpanMidnightInput').prop('checked', false);
    $('#ColorInput').val('#ffcc00');
}

function loadShiftForEdit(id) {
    $.ajax({
        url: RazorVars.editUrl + '/' + id,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var shift = response.data;
                $('#ShiftId').val(shift.id);
                $('#CodeInput').val(shift.code);
                $('#ShortInput').val(shift.short);
                $('#PrimaryNameInput').val(shift.primaryName);
                $('#SecondaryNameInput').val(shift.secondaryName);
                $('#DOW').val(shift.dow);
                $('#ColorInput').val(shift.color || '#ffcc00');
                $('#WorkingFromTime').val(shift.workingFromTime);
                $('#WorkingToTime').val(shift.workingToTime);
                $('#BreakFromTime').val(shift.breakFromTime);
                $('#BreakToTime').val(shift.breakToTime);

                $('#SpanMidnightInput').prop('checked', shift.spanMidnight);

                // Set day checkboxes
                $('#SunInput').prop('checked', shift.sun);
                $('#MonInput').prop('checked', shift.mon);
                $('#TueInput').prop('checked', shift.tue);
                $('#WedInput').prop('checked', shift.wed);
                $('#ThuInput').prop('checked', shift.thu);
                $('#FriInput').prop('checked', shift.fri);
                $('#SatInput').prop('checked', shift.sat);

                $('#shiftModal').modal('show');
            } else {
                alert(response.message);
            }
        },
        error: function () {
            Swal.fire(RazorVars.swalErrorHappened, 'error');
        }
    });
}

function loadShifts() {
    var formData = $('#filterForm').serialize();
    $.ajax({
        url: RazorVars.indexUrl,
        type: 'GET',
        data: formData,
        success: function (result) {
            $('#shiftListContainer').html(result);
        },
        error: function () {
            Swal.fire(RazorVars.swalErrorHappened, 'error');
        }
    });
}

function deleteShift(id) {
    // Removed the extra native confirm() that could cause odd double flows
    Swal.fire({
        title: resources.areYouSure,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: 'var(--danger-600)',
        cancelButtonColor: 'var(--secondary-500)',
        confirmButtonText: resources.yes,
        cancelButtonText: resources.no
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: RazorVars.deleteUrl + '/' + id,
                type: 'POST',
                success: function (result) {
                    if (result.success) {
                        Swal.fire({
                            title: RazorVars.swalDeleted,
                            icon: 'success',
                            confirmButtonText: resources.ok
                        });
                        loadShifts();
                    } else {
                        alert(result.message);
                    }
                },
                error: function () {
                    Swal.fire(RazorVars.swalErrorHappened, 'error');
                }
            });
        }
    });
}

function updateDOWSummary() {
    $('#DOW').val(convertToDOW(
        $('#SunInput').is(':checked'),
        $('#MonInput').is(':checked'),
        $('#TueInput').is(':checked'),
        $('#WedInput').is(':checked'),
        $('#ThuInput').is(':checked'),
        $('#FriInput').is(':checked'),
        $('#SatInput').is(':checked')
    ));
}

function convertToDOW(sun, mon, tue, wed, thu, fri, sat) {
    var dowArray = [];
    if (sun) dowArray.push("SUN");
    if (mon) dowArray.push("MON");
    if (tue) dowArray.push("TUE");
    if (wed) dowArray.push("WED");
    if (thu) dowArray.push("THU");
    if (fri) dowArray.push("FRI");
    if (sat) dowArray.push("SAT");
    return dowArray.join(",");
}

