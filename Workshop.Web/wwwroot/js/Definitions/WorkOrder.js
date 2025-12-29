function loadEditModal(id) {
    $.ajax({
        type: 'GET',
        url: window.RazorVars.loadWorkOrderDataUrl,
        data: { id: id },
        dataType: 'json',
        success: function (data) {
            if (!data) {
                console.warn("No data returned from server.");
                return;
            }

            // Fill form fields
            $('#WorkOrderForm_Id').val(id).trigger('change');
            $('#WorkOrderForm_WorkOrderType').val(data.workOrderType).trigger('change');
            $("#WorkOrderForm_VehicleType").val(data.vehicleType).trigger('change');
            $("#WorkOrderForm_WorkOrderStatus").val(data.workOrderStatus).trigger('change');

            getManufacturerList(data.vehicleType)
                .then(() => {
                    $('#VehicleId').val(String(data.vehicleId)).trigger('change');
                });

            $('#SubStatus').val(data.wfstatus).trigger('change');
            $('#WorkOrderForm_AccidentDate').val(data.gregorianDamageDate).trigger('change');
            $('#WorkOrderForm_AccidentTime').val(data.accidentTime).trigger('change');
            $('#WorkOrderForm_Description').val(data.description).trigger('change');
            $('#WorkOrderForm_Note').val(data.note).trigger('change');
           // $("#WorkOrderForm_ImagesFilePath").attr("src", data.imagesFilePath).show();
            
            if (data.imagesFilePath && data.imagesFilePath.trim() !== "") {
                pond.setOptions({
                    files: [
                        {
                            source: data.imagesFilePath,
                            options: { type: "local" }
                        }
                    ]
                });
            } else {
                pond.removeFiles();
            }
            $("#Agreement" + data.relatedId).prop('checked', true);
            formChoice(data.relatedId);

            Get_Agreement_Movement(data.vehicleId);

            setTimeout(() => {
                $("#FK_AgreementId").val(data.fkAgreementId).trigger('change');
                $("#FkVehicleMovementId").val(data.fkVehicleMovementId).trigger('change');
            }, 500);

            if (data.vehicles && Array.isArray(data.vehicles)) {
                const lang = $('html').attr('lang') || 'en';
                const $vehicleSelect = $('#VehicleId');
                $vehicleSelect.empty().append('<option value="">Select</option>');

                $.each(data.vehicles, function (_, vehicle) {
                    const name = lang === 'en' ? vehicle.vehicleModelPrimaryName : vehicle.vehicleModelSecondaryName;
                    $vehicleSelect.append(`<option value="${vehicle.id}">${name}</option>`);
                });

                $vehicleSelect.val(data.vehicleId).trigger('change');
            }

            $('#FK_AgreementId').val("");

            // Show modal — Select2 inside will init on "shown.bs.modal"
            $('#workOrderModal').modal('show');
        },
        error: function (xhr, status, error) {
            console.error("Failed to load work order:", error);
            alert("Error loading work order data. Please try again.");
        }
    });
}

// =========================
// Select2 Initialization
// =========================
// 1) Initialize Select2 for selects NOT in the modal
$(document).ready(function () {
    $('select').not('#workOrderModal select').each(function () {
        const $sel = $(this);
        $sel.select2({
            theme: 'bootstrap-5',
            width: '100%',
            placeholder: function () {
                return $sel.data('placeholder') || 'Select an option';
            }
        });
    });
});

// 2) Initialize (or re-init) Select2 for selects INSIDE the modal when it opens
$('#workOrderModal').on('shown.bs.modal', function () {
    const $modal = $(this);
    $modal.find('select').each(function () {
        const $sel = $(this);
        if ($sel.data('select2')) $sel.select2('destroy');
        $sel.select2({
            theme: 'bootstrap-5',
            width: '100%',
            dropdownParent: $modal, // <-- the key fix
            placeholder: $sel.data('placeholder') || 'Select an option'
        });
    });
});

// =========================
// Reset / New Item Buttons
// =========================
$(document).ready(function () {
    $("#ResetBtn").on("click", function (e) {
        e.preventDefault();
        $("#WorkOrderFilter_VehicleID").val('').trigger('change');
        $("#WorkOrderFilter_WorkOrderTypeId").val('').trigger('change');
        $("#WorkOrderFilter_FromDate").val('').trigger('change');
        $("#WorkOrderFilter_ToDate").val('').trigger('change');
        $("#WorkOrderFilter_CreatedBy").val('').trigger('change');
        $("#WorkOrderFilter_WorkOrderStatus").val('').trigger('change');
        $("#WorkOrderFilter_InvoicingStatus").val('').trigger('change');
        $("#WorkOrderFilter_WorkOrderNo").val('').trigger('change');

        loadWorkOrders(1);
    });

    $('#BtnAddNew').on('click', function () {
        $('#WorkOrderForm_Id').val('').trigger('change')
        $("#VehicleId").val('');
        $('#SubStatus').val('').trigger('change');
        $('#WorkOrderForm_AccidentDate').val('').trigger('change');
        $('#WorkOrderForm_AccidentTime').val('').trigger('change');
        $('#WorkOrderForm_Description').val('').trigger('change');
        $('#WorkOrderForm_Note').val('').trigger('change');
        $("#FK_AgreementId").val('');
        $("#FkVehicleMovementId").val('').trigger('change');
        $("#WorkOrderForm_ImagesFilePath").val('').trigger('change');
        $("#Agreement1").prop('checked', true);
        formChoice(1);
    });
});

// =========================
// Filtering + Pagination
// =========================
const workOrderPageSize = 25;

function toNullableInt(val) {
    if (val === undefined || val === null || val === '') return null;
    const num = parseInt(val, 10);
    return Number.isNaN(num) ? null : num;
}

function buildWorkOrderFilter(page) {
    return {
        VehicleID: toNullableInt($('#WorkOrderFilter_VehicleID').val()),
        WorkOrderTypeId: toNullableInt($('#WorkOrderFilter_WorkOrderTypeId').val()),
        FromDate: $('#WorkOrderFilter_FromDate').val() || null,
        ToDate: $('#WorkOrderFilter_ToDate').val() || null,
        CreatedBy: toNullableInt($('#WorkOrderFilter_CreatedBy').val()),
        WorkOrderStatus: toNullableInt($('#WorkOrderFilter_WorkOrderStatus').val()),
        InvoicingStatus: toNullableInt($('#WorkOrderFilter_InvoicingStatus').val()),
        WorkOrderNo: toNullableInt($('#WorkOrderFilter_WorkOrderNo').val()),
        page: page || 1
    };
}

function applyWorkOrderPager(currentPage) {
    debugger
    const totalPages = parseInt($("#total_pages").val(), 10) || 0;
    const $pager = $('#contentPager');

    const pagerPlugin = typeof $pager.pagination === 'function';

    if (!pagerPlugin || totalPages <= 1) {
        $pager.attr("hidden", true).empty();
        return;
    }

    $pager.attr("hidden", false);

    try { $pager.pagination('destroy'); } catch (e) { }

    $pager.pagination({
        items: totalPages,                 
        itemsOnPage: 1,                   
        currentPage: parseInt(currentPage, 10) || 1,
        prevText: '&laquo;',
        nextText: '&raquo;',
        hrefTextPrefix: '',                
        onPageClick: function (pageNumber, evt) {
            if (evt && evt.preventDefault) evt.preventDefault();
            loadWorkOrders(pageNumber);    
            return false;
        }
    });

    $pager.off('click.pagerFix').on('click.pagerFix', 'a', function (e) {
        e.preventDefault();
        e.stopPropagation();
    });

    $pager.find('a').attr('href', 'javascript:void(0)').attr('role', 'button');

    $pager.find('ul').addClass('pagination');
    $pager.find('li').addClass('page-item');
    $pager.find('a, span').addClass('page-link');
}

function loadWorkOrders(page) {
    if (!window.RazorVars || !window.RazorVars.workOrderListUrl) {
        console.warn("workOrderListUrl is not configured on RazorVars.");
        return;
    }

    const filter = buildWorkOrderFilter(page);

    $.ajax({
        type: 'POST',
        url: window.RazorVars.workOrderListUrl,
        contentType: 'application/json',
        dataType: 'html',
        data: JSON.stringify(filter)
    }).done(function (html) {
        $("#contentList").html(html);
        applyWorkOrderPager(page);
    }).fail(function (xhr, status, error) {
        console.error("Failed to load work orders:", error);
    });
}

$(document).ready(function () {
    applyWorkOrderPager(1);

    $('#WorkOrderFilter').on('submit', function (e) {
        e.preventDefault();
        loadWorkOrders(1);
    });
});

// =========================
// Show/Hide related sections
// =========================
function formChoice(x) {
    if (x == 2) {
        $('#showAgreement').css('display', 'block');
        $('#showVehicleMovemen').css('display', 'none');
        if ($("#agreements").val() == 0) {
            $("#agreements").next().children(':first-child').children(':first-child').css("border-color", "red");
            isValid = false;
        } else {
            $("#agreements").next().children(':first-child').children(':first-child').css("border-color", "#e7eaec");
        }
    } else if (x == 3) {
        $('#showVehicleMovemen').css('display', 'block');
        $('#showAgreement').css('display', 'none');
    } else if (x == 1) {
        $('#showVehicleMovemen').css('display', 'none');
        $('#showAgreement').css('display', 'none');
    }
}

// =========================
// Data loaders
// =========================
function getManufacturerList(VehicleType) {
    const $vehicleSelect = $('#VehicleId');
    $vehicleSelect.empty().append('<option value="">Select</option>');
    $vehicleSelect.trigger('change');

    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'GET',
            url: window.RazorVars.vehicleListUrl,
            data: { VehicleTypeId: VehicleType },
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {
                if (!data || data.length === 0) {
                    console.warn("No data returned from server.");
                    resolve();
                    return;
                }

                $.each(data, function (_, item) {
                    $vehicleSelect.append(`<option value="${item.value}">${item.text}</option>`);
                });

                resolve();
            },
            error: function (xhr, status, error) {
                console.error("Failed to load vehicles:", error);
                reject(error);
            }
        });
    });
}

function GetAgreement(id) {
    var VehicleId = $('#VehicleId').val();
    $('#FK_AgreementId').html('<option value="">Select</option>');

    $.ajax({
        type: 'GET',
        url: window.RazorVars.getAgreementUrl,
        contentType: 'application/json',
        dataType: 'JSON',
        data: { VehicleDefinitionId: VehicleId }
    }).done(function (result) {
        $('#FK_AgreementId').val("");
        for (var i = 0; i < result.length; i++) {
            $('#FK_AgreementId').append('<option value="' + result[i].agreementId + '">' + result[i].customerName + '</option>');
        }
    });
}

function GetMovement(id) {
    var VehicleId = $('#VehicleId').val();
    $('#FkVehicleMovementId').html('<option value="">Select</option>');

    $.ajax({
        type: 'GET',
        url: window.RazorVars.getMovementUrl,
        contentType: 'application/json',
        dataType: 'JSON',
        data: { VehicleDefinitionId: VehicleId }
    }).done(function (result) {
        $('#FkVehicleMovementId').val("");
        for (var i = 0; i < result.length; i++) {
            $('#FkVehicleMovementId').append('<option value="' + result[i].agreementId + '">' + result[i].customerName + '</option>');
        }
    });
}

function Get_Agreement_Movement(id) {
    GetMovement(id);
    GetAgreement(id);
}

// =========================
// FilePond
// =========================
let pond;

$(document).ready(function () {

    FilePond.registerPlugin(
        FilePondPluginImagePreview,
        FilePondPluginFileValidateType,
        FilePondPluginFileValidateSize
    );

    if (!pond) {
        pond = FilePond.create(document.querySelector('#ImagesFile'), {
            storeAsFile: true,
            allowMultiple: false,
            credits: false,
            acceptedFileTypes: ['image/*'],
            allowImagePreview: true,
            labelIdle: resources.filepondDragDrop,
            server: {
                load: (source, load) => {
                    fetch(source)
                        .then(res => res.blob())
                        .then(load)
                        .catch(err => console.error("FilePond load error:", err));
                }
            }
        });
    }
});

// =========================
// Validation (jQuery Validate)
// =========================
$(document).ready(function () {
    const $form = $('#WorkOrderForm');

    function relatedChoice() {
        return $('input[name="WorkOrderForm.RelatedId"]:checked').val();
    }

    function placeError(error, element) {
        // Bootstrap-friendly error styling
        error.addClass('d-block'); // ensures display
        if (element.hasClass('select2-hidden-accessible')) {
            error.insertAfter(element.next('.select2'));
            return;
        }
        if (element.parent('.input-group').length) {
            error.insertAfter(element.parent());
            return;
        }
        if (element.is(':checkbox') || element.is(':radio')) {
            error.appendTo(element.closest('.form-group, fieldset, .form-check').first());
            return;
        }
        error.insertAfter(element);
    }

    $form.validate({
        // Keep hidden select2 fields validated
        ignore: [],
        errorElement: 'div',
        errorClass: 'invalid-feedback',
        errorPlacement: placeError,

        highlight: function (element) {
            var $el = $(element);
            if ($el.hasClass('select2-hidden-accessible')) {
                $el.next('.select2').find('.select2-selection').addClass('is-invalid');
            } else if ($el.parent('.input-group').length) {
                $el.addClass('is-invalid');
                $el.parent('.input-group').addClass('is-invalid');
            } else {
                $el.addClass('is-invalid');
            }
        },

        unhighlight: function (element) {
            var $el = $(element);
            if ($el.hasClass('select2-hidden-accessible')) {
                $el.next('.select2').find('.select2-selection').removeClass('is-invalid');
            } else if ($el.parent('.input-group').length) {
                $el.removeClass('is-invalid');
                $el.parent('.input-group').removeClass('is-invalid');
            } else {
                $el.removeClass('is-invalid');
            }
        },

        rules: {
            'WorkOrderForm.WorkOrderType': { required: true },
            'WorkOrderForm.VehicleType': { required: true },
            'WorkOrderForm.VehicleId': { required: true },
            'WorkOrderForm.Wfstatus': { required: false },
            'WorkOrderForm.AccidentDate': { required: true, date: true },
            'WorkOrderForm.AccidentTime': { required: true },
            'WorkOrderForm.Description': { required: true, minlength: 3 },
            'WorkOrderForm.Note': { required: true, minlength: 2, maxlength: 1000 },
            'WorkOrderForm.RelatedId': { required: true },

            'WorkOrderForm.FkAgreementId': {
                required: { depends: function () { return relatedChoice() === '2'; } }
            },
            'WorkOrderForm.FkVehicleMovementId': {
                required: { depends: function () { return relatedChoice() === '3'; } }
            },

            'WorkOrderForm.ImagesFilePath': {
                extension: "jpg|jpeg|png|gif|bmp|webp|heic|heif"
            }
        }
    });

    // Validate immediately when Select2 changes (delegated in case of re-init)
    $('body').on('change', 'select.select2-hidden-accessible', function () {
        $(this).valid();
    });

    // Public formChoice that also triggers validation of dependent fields
    window.formChoice = function (val) {
        const showAgreement = $('#showAgreement');
        const showMovement = $('#showVehicleMovemen');

        if (val === 2 || val === '2') {
            showAgreement.show();
            showMovement.hide();
            $('#FkVehicleMovementId').val('').trigger('change');
        } else if (val === 3 || val === '3') {
            showAgreement.hide();
            showMovement.show();
            $('#FK_AgreementId').val('').trigger('change');
        } else {
            showAgreement.hide();
            showMovement.hide();
            $('#FK_AgreementId, #FkVehicleMovementId').val('').trigger('change');
        }

        $('[name="WorkOrderForm.FkAgreementId"], [name="WorkOrderForm.FkVehicleMovementId"]').valid();
    };

    // Initialize related sections on first load
    const initRelated = relatedChoice();
    if (initRelated) window.formChoice(initRelated);
});

$(document).ready(function () {
        const params = new URLSearchParams(window.location.search);
    const openId = params.get("openId");

    if (openId) {
        // Delay ensures that Select2 + UI are ready
        setTimeout(() => {
            loadEditModal(openId);
        }, 300);
        }

    });

