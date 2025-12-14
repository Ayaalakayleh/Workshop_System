// =========================
// Select2 + Bootstrap 5 init
// =========================
$(document).ready(function () {
    $('.select2').select2({
        theme: 'bootstrap-5',
        width: '100%',
        allowClear: false
    });
});

// =========================
// Form submit + validation
// =========================
$(document).ready(function () {

    // Trigger validation when select2 changes
    $(document).on("change", ".select2", function () {
        $(this).valid();
    });

    var $form = $("#vehicleDefinitionForm");

    // jQuery Validate config
    $form.validate({
        ignore: ":hidden:not(.select2-hidden-accessible)", // keep Select2 in validation
        rules: {
            ManufacturerId: { required: true },
            VehicleModelId: { required: true },
            ManufacturingYear: {
                required: true,
                min: 1900,
                max: 2100
            },
            PlateNumber: { required: true },
            newColorId: { required: true },
            CurrentMeter: {
                required: true,
                number: true,
                min: 0
            },
            ChassisNo: { required: true },
            CompanyId: { required: false }
        },
        errorClass: "is-invalid",
        errorElement: "span",
        highlight: function (element, errorClass, validClass) {
            var $el = $(element);

            $el.addClass(errorClass).removeClass(validClass);

            // Special handling for Select2
            if ($el.hasClass("select2-hidden-accessible")) {
                var $container = $el.next(".select2-container");
                $container.find(".select2-selection")
                    .addClass("border border-danger")
                    .removeClass("border-success");
            }
        },
        unhighlight: function (element, errorClass, validClass) {
            var $el = $(element);

            $el.removeClass(errorClass).addClass(validClass);

            // Special handling for Select2
            if ($el.hasClass("select2-hidden-accessible")) {
                var $container = $el.next(".select2-container");
                $container.find(".select2-selection")
                    .removeClass("border border-danger")
                    .addClass("border-success");
            }
        },
        errorPlacement: function (error, element) {
            error.addClass("text-danger mt-1");

            // Handle Select2
            if (element.hasClass("select2-hidden-accessible")) {
                // Insert error AFTER the visible Select2 container
                error.insertAfter(element.next(".select2-container"));
                return;
            }

            // Handle normal inputs
            error.insertAfter(element);
        }
    });

    // Submit handler – prevents submit when validation fails
    $form.on('submit', function (e) {
        e.preventDefault();

        if (!$form.valid()) {
            // Form has errors; don't submit
            return;
        }

        var formData = $form.serialize();
        $("#btnCreate").prop("disabled", true);

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: formData,
            success: function (response) {
                $("#btnCreate").prop("disabled", false);

                if (response.isSuccess) {
                    Swal.fire({
                        icon: 'success',
                        title: RazorVars.msgSuccessTitle,
                        confirmButtonText: RazorVars.btnOk,
                        confirmButtonColor: 'var(--primary-600)',
                        timer: 3000,
                        timerProgressBar: true
                    }).then(() => {
                        window.location.href = RazorVars.indexUrl;
                    });
                } else {
                    Swal.fire(
                        RazorVars.swalErrorHappened,
                        response.message
                    );
                }
            },
            error: function () {
                $("#btnCreate").prop("disabled", false);
                Swal.fire(
                    RazorVars.swalErrorHappened,
                    'error'
                );
            }
        });
    });
});


// =========================
// Vehicle Model & Manufacturer logic
// =========================
$('#VehicleModelId').change(function () {
    setVehicleModelVal();
});

function setVehicleModelVal() {
    var vehicleModelId = $('#VehicleModelId').val();
    $('#VehicleModelId_Hidden').val(vehicleModelId);
}

$('#ManufacturerId').change(function () {

    var manufacturerId = $('#ManufacturerId').val();

    $("#VehicleModelId").empty();

    if (manufacturerId === "") {
        $('#VehicleModelId').prop('disabled', true).trigger('change');
    } else {
        $('#VehicleModelId').prop('disabled', false);
        var baseUrl = RazorVars.fillVehicleModel;

        $.ajax({
            type: 'GET',
            url: baseUrl + '/' + manufacturerId,
            contentType: 'application/json',
            dataType: 'json',
            data: {},
            success: function (data) {
                $.each(data, function (key, value) {
                    var modelname = lang == "en" ? value.vehicleModelPrimaryName : value.vehicleModelSecondaryName;
                    var selected = (RazorVars.vehicleModelId && value?.id == RazorVars.vehicleModelId) ? 'selected' : '';
                    $('#VehicleModelId').append('<option value="' + value.id + '" ' + selected + '>' + modelname + '</option>');
                });

                // Update hidden & Select2
                setVehicleModelVal();
                $('#VehicleModelId').trigger('change');
            },
            error: function (error) {
                Swal.fire(
                    RazorVars.swalErrorHappened,
                    'error'
                );
            }
        });
    }
});

// =========================
// SaveVehicleDetails (unused in current submit, kept as-is)
// =========================
function SaveVehicleDetails(obj) {
    $.ajax({
        url: RazorVars.editUrl,
        type: 'POST',
        data: obj,
        dataType: 'JSON',
        contentType: false,
        cache: false,
        async: false,
        processData: false,
        success: function (data) {
            if (data.IsSuccess) {
                Swal.fire({
                    icon: 'success',
                    title: RazorVars.msgSuccessTitle,
                    confirmButtonText: RazorVars.btnOk,
                    confirmButtonColor: 'var(--primary-600)',
                    timer: 3000,
                    timerProgressBar: true
                }).then(() => {
                    window.location.href = RazorVars.indexUrl;
                });
            } else {
                Swal.fire(
                    RazorVars.swalErrorHappened,
                    'error'
                );
            }
        },
        error: function (data) {
            Swal.fire(
                RazorVars.swalErrorHappened,
                'error'
            );
        }
    });
}

// =========================
// List / search logic (unchanged)
// =========================
$(document).ready(function () {
    getData(1);
});

$("#searchVehicle").click(function () {
    getData(1);
});

function getData(page) {

    var plateNumber = $("#PlateNumber").val();
    var manufacturerId = $("#ManufacturerId").val();

    plateNumber = plateNumber != '' ? plateNumber : null;
    manufacturerId = manufacturerId != '' ? manufacturerId : null;

    var model = {
        'PlateNumber': plateNumber, 'Page': page, 'ManufacturerId': manufacturerId
    };

    $.ajax({
        type: 'POST',
        url: RazorVars.listUrl,
        contentType: 'application/json',
        dataType: 'html',
        data: JSON.stringify(model),
        error: function (error) {
            Swal.fire(
                RazorVars.swalErrorHappened,
                'error'
            );
        }
    }).done(function (result) {
        $("#VehicleList").html(result);
        if ($("#total_pages").val() != undefined) {
            $("#contentPager").attr("hidden", false);
            $('#contentPager').pagination({
                items: $("#total_pages").val(),
                itemOnPage: 25,
                currentPage: page,
                cssStyle: 'light-theme',
                onInit: function () {
                },
                onPageClick: function (page, evt) {
                    getData(page);
                }
            });
        }
        else {
            $("#contentPager").attr("hidden", true);
        }
    });
}

$("#clearVehicleFilter").click(function () {
    $("#PlateNumber").val("");
    $("#ManufacturerId").val("").trigger("change"); // Select2 reacts to change
    getData(1);
});

// =========================
// Vehicles class dropdown
// =========================
$(document).ready(function () {
    $.getJSON('/RTSCode/GetVehiclesClass', function (data) {
        var select = $('#vehiclesTypeEditId');
        select.empty();
        $.each(data, function (index, item) {
            select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
        });
    });
});