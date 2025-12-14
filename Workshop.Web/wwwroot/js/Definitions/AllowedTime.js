//#region Index
function resetFilter() {
    $('#Make').val('');
    $('#Model').val('');
    $('#Year').val('');
    $('#Engine').val('');
    $('#RTSCode').val('');

    getData(1);
}

function resetModal() {
    // Load empty form for creating new item
    loadAllowedTimeForEdit(0);
}

function loadAllowedTimeForEdit(id) {

    $.ajax({
        url: `${RazorVars.editUrl}/` + id,
        type: 'GET',
        dataType: 'html',
        success: function (result) {

            $('#allowedTimeModalBody').html(result);

            if (id > 0)
                onChangeMake('#MakeEditId', '#ModelEditId');

            $('#allowedTimeModal').modal('show');
        },
        error: function (error) {

            alert('Internal Server Error 500');
        }
    });
}

$(document).ready(function () {

    //Handle form submission for filter form
    $('#btnSearch').click(function () {
        getData();
    });
});

function getData(page) {

    page = page || 1;
    var make = $('#Make').val();
    var model = $('#Model').val();
    var year = $('#Year').val();
    var engine = $('#Engine').val();
    var rtsCode = $('#RTSCode').val();

    var formData = {
        'Make': make != "" ? make : null,
        'Model': model != "" ? model : null,
        'Year': year != "" ? year : null,
        'Engine': engine != "" ? engine : null,
        'RTSCode': rtsCode != "" ? rtsCode : null,
        'PageNumber': page
    };

    $.ajax({
        url: RazorVars.indexUrl,
        type: 'GET',
        contentType: 'application/json',
        dataType: 'html',
        data: formData,
        success: function (result) {
            $('#allowedTimeListContainer').html(result);
        },
        error: function () {
            alert('Internal Server Error 500');
        }
    });
}
function deleteAllowedTime(id) {

    Swal.fire({
        title: RazorVars.swalDeleteTitle,
        text: '',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: 'var(--danger-600)',
        cancelButtonColor: 'var(--secondary-500)',
        confirmButtonText: RazorVars.swalDeleteConfirm,
        cancelButtonText: RazorVars.swalDeleteCancel
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'POST',
                url: `${RazorVars.deleteUrl}/` + id,
                contentType: 'application/json',
                dataType: 'json',
                data: null,
                success: function (result) {
                    if (result.success) {
                        getData();
                    } else {
                        Swal.fire(
                            RazorVars.swalErrorHappened,
                            'error'
                        );
                    }
                },
                error: function () {
                    Swal.fire(
                        RazorVars.swalErrorHappened,
                        'error'
                    );
                }
            });
        }
    });

}
//#endregion

//#region CreateEdit

$(document).ready(function () {

    // Filter models based on selected make
    $('#Make').on('change', function () {

        onChangeMake('#Make', '#Model');
    });

    $('#MakeEditId').on('change', function () {

        onChangeMake('#MakeEditId', '#ModelEditId');
    });

    // Handle form submission
    $('#allowedTimeForm').on('submit', function (e) {

        e.preventDefault();

        if (this.checkValidity()) {

            var formData = $(this).serialize();

            $.ajax({
                url: $(this).attr('action'),
                type: 'POST',
                data: formData,
                success: function (response) {
                    if (response.success) {
                        $('#allowedTimeModal').modal('hide');

                        Swal.fire({
                            icon: 'success',
                            title: resources.success_msg,
                            confirmButtonText: RazorVars.btnOk,
                            confirmButtonColor: 'var(--primary-600)',
                            timer: 3000,
                            timerProgressBar: true
                        }).then(() => {
                            resetFilter();
                            
                        });
                    } else {
                        alert(response.message);
                    }
                },
                error: function () {
                    alert('Internal Server Error 500');
                }
            });
        }

        //this.classList.add('was-validated');
    });
});

function onChangeMake(makeId, modelId) {
    var id = $(makeId).val();
    $(`${modelId} option`).remove();
    $(modelId).append('<option value="">Select</option>');
    if (id == "") {
        $(modelId).prop('disabled', true);
    } else {
        $(modelId).prop('disabled', false);

        $.ajax({
            type: 'GET',
            url: `${RazorVars.fillVehicleModelUrl}/` + id,
            contentType: 'application/json',
            dataType: 'json',
            data: {},
            success: function (data) {
               
                $.each(data, function (key, value) {
                    var modelname = lang == "en" ? value.vehicleModelPrimaryName : value.vehicleModelSecondaryName;
                    var selected = (vehicleModelId && value?.id == vehicleModelId) ? 'selected' : '';
                    $(modelId).append('<option value="' + value.id + '" ' + selected + '>' + modelname + '</option>');
                });
            },
            error: function (error) {
                alert('error FillVehicleModel');
            }
        });
    }
}
$(document).ready(function () {

    // Initialize validation
    $("#allowedTimeForm").validate({
        ignore: [],
        errorClass: "is-invalid",
        //validClass: "is-valid",

        rules: {
            Make: {
                required: true
            },
            Model: {
                required: true
            },
            VehicleClass :{
                required: true
            },
            Year: {
                required: true,
            },
            RTSCode: {
                required: true
            },
            AllowedHours: {
                required: true,
            },
            SupervisorOverride: {
                required: false
            }
        },

        messages: {
            Make: { required: resources.required_field },
            Model: { required: resources.required_field },
            VehicleClass: { required: resources.required_field },
            Year: { required: resources.required_field },
            RTSCode: { required: resources.required_field },
            AllowedHours: { required: resources.required_field }
        },

        errorPlacement: function (error, element) {
            // Bootstrap 5 style placement
            error.addClass("invalid-feedback");
            if (element.parent(".input-group").length) {
                error.insertAfter(element.parent());
            } else if (element.hasClass("select2-hidden-accessible")) {
                error.insertAfter(element.next(".select2"));
            } else {
                error.insertAfter(element);
            }
        },

        highlight: function (element) {
            $(element).addClass("is-invalid").removeClass("is-valid");
        },

        unhighlight: function (element) {
            $(element).removeClass("is-invalid")//.addClass("is-valid");
        },

        submitHandler: function (form) {
            Swal.fire({
                icon: 'success',
                title: resources.success_msg,
                confirmButtonText: resources.ok,
                confirmButtonColor: 'var(--primary-600)',
                timer: 3000,
                timerProgressBar: true
            }).then(() => {
                form.submit();
            });
            
        }
    });
    $('select').select2({
        theme: 'bootstrap-5',
        width: '100%',
        placeholder: function () {
            return $(this).data('placeholder') || 'Select an option';
        }
    });
});