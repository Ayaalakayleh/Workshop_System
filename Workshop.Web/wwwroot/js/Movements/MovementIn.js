/* ==================================================
   Global state
================================================== */
var cuspond, cuspondVehicle;
var validPhoto = true;
var rowCount = 0;
var $myFuelMeter;
var selectCount = 0;
var WorkOrdersList = [];
var Etype = 1;
var shapesJson = [];

/* ==================================================
   FilePond plugin register (safe)
================================================== */
if (window.FilePond) {
    const plugins = [];

    if (window.FilePondPluginFileValidateType) plugins.push(window.FilePondPluginFileValidateType);
    if (window.FilePondPluginImagePreview) plugins.push(window.FilePondPluginImagePreview);
    if (window.FilePondPluginImageExifOrientation) plugins.push(window.FilePondPluginImageExifOrientation);

    // Crop needs Transform. Only register crop if transform exists.
    if (window.FilePondPluginImageTransform) plugins.push(window.FilePondPluginImageTransform);
    if (window.FilePondPluginImageCrop && window.FilePondPluginImageTransform) {
        plugins.push(window.FilePondPluginImageCrop);
    }

    FilePond.registerPlugin(...plugins);
}


/* ==================================================
   Ready
================================================== */
$(function () {
    initializeComponents();
    initializeScratches();
    initializejSignature();
    initializeFilePond();
    initializeEventHandlers();
    initializeValidation();
    //attachInitialRowRules();
});

/* ==================================================
   Components
================================================== */
function initializeComponents() {
    // time now
    var d = new Date(), h = d.getHours(), m = d.getMinutes();
    if (h < 10) h = '0' + h;
    if (m < 10) m = '0' + m;
    $('#ReceivedTime').val(h + ":" + m);

    ChangeWorkOrderType();
    ChangesMaintenanceType();
    rowCount = $('#tblMainteneance > tbody > tr').length;

    if (window.RazorVars && RazorVars.VehicleID) { selectCount += 1; }

    //GetMBranches();

    if ($("#GettingVehicleId").val() > 0) {
        GetVehicleData($("#GettingVehicleId").val());
    }

    const incomingVehicle = Number($("#VehicleFromReservation").val() || 0);
    debugger;
    if (incomingVehicle > 0) {
        selectCount = 1;
        $("#VehicleID").val(incomingVehicle);
        $("#GettingVehicleId").val(incomingVehicle);

        GetVehicleData(incomingVehicle);
        GetWIPByVehicleId(incomingVehicle);

    } else {
        if (window.RazorVars && RazorVars.RefVehicledefinitionsVehicleId === '0') {
            OpenChangeCarModal();
        }
    }

    // ---- DynaMeter init ----
    if ($.fn && $.fn.dynameter) {
        $myFuelMeter = $("div#fuelMeterDiv").dynameter({
            width: 200,
            label: 'fuel level',
            value: 50,
            min: 0,
            max: 100,
            unit: ' ',
            regions: { 0: 'error', 10: 'warn', 20: 'normal' }
        });
    } else {
        console.error('DynaMeter plugin not loaded');
    }

    // native range slider to control meter (no jQuery UI)
    var sliderEl = document.getElementById('fuelSlider');
    var sliderValEl = document.getElementById('fuelSliderValue');
    if (sliderEl) {
        sliderEl.addEventListener('input', function (e) {
            var val = Number(e.target.value);
            if (sliderValEl) sliderValEl.textContent = String(val);
            if ($myFuelMeter && typeof $myFuelMeter.changeValue === 'function') {
                $myFuelMeter.changeValue(val.toFixed(1));
            } else {
                try {
                    $("div#fuelMeterDiv").dynameter && $("div#fuelMeterDiv").dynameter('setValue', val);
                } catch (_) { /* noop */ }
            }
        });
    }
}

/* ==================================================
   jSignature
================================================== */
function initializejSignature() {
    if ($.fn && $.fn.jSignature) {
        $("#DriverSignature").jSignature({ 'UndoButton': false, 'width': '100%', 'height': '300px', 'background-color': 'lightgrey' });
        $("#EmployeeSignature").jSignature({ 'UndoButton': false, 'width': '100%', 'height': '300px', 'background-color': 'lightgrey' });
        $('#resetSignatureBtn').on('click', function () {
            $("#DriverSignature").jSignature('reset');
            $("#EmployeeSignature").jSignature('reset');
        });
    } else {
        console.error('jSignature plugin not loaded');
    }
}

/* ==================================================
   Scratches
================================================== */
function initializeScratches() {
    var shapeType;
    var shapeCount = 0;

    function setMode(type, label, cursorClass) {
        shapeType = type;
        $('#coordinates').text(label + ' coordinates: ');
        $("#panel")
            .removeClass("deep-scratch-cursor small-scratch-cursor very-deep-cursor bend-body-cursor")
            .addClass(cursorClass);
    }

    $('#DeepScratch-btn').click(function () {
        setMode('deep-scratch', 'DeepScratch', 'deep-scratch-cursor');
    });
    $('#SmallScratch-btn').click(function () {
        setMode('small-scratch', 'SmallScratch', 'small-scratch-cursor');
    });
    $('#VeryDeepScratch-btn').click(function () {
        setMode('very-deep-scratch', 'VeryDeepScratch', 'very-deep-cursor');
    });
    $('#BendInBody-btn').click(function () {
        setMode('bend-in-body', 'BendInBody', 'bend-body-cursor');
    });

    $('#panel').click(function (e) {
        if (!shapeType) return;
        var id = ++shapeCount;
        var $this = $(this);
        var offset = $this.offset();
        var xCoord = e.pageX - offset.left;
        var yCoord = e.pageY - offset.top;

        // Use unique id and data-type; avoid duplicate IDs on the page
        var $shape = $('<div class="shape shape-' + id + '" id="shape-' + id + '" data-type="' + shapeType + '"></div>');
        $shape.css({ 'left': xCoord, 'top': yCoord });
        $this.append($shape);

        $('#coordinates').append(shapeType + ': (' + xCoord + ', ' + yCoord + ') ');
        shapesJson.push({ type: String(shapeType), x: xCoord, y: yCoord });
        $('#strikes').val(JSON.stringify(shapesJson));
    });

    $('#undo-button').on('click', function () {
        var last = shapesJson.pop();
        if (last) $('.shape:last-child').remove();
    });

    $('#clear-button').on('click', function () {
        shapesJson = [];
        $("#panel").empty();
    });

    $(document).on("contextmenu", ".shape", function () {
        $(this).remove();
        return false;
    });
}

/* ==================================================
   FilePond
================================================== */
function initializeFilePond() {
    if (!window.FilePond) return;

    const baseOptions = {
        allowImagePreview: true,
        imagePreviewHeight: 150,
        acceptedFileTypes: ['image/*', 'application/pdf'],
        labelIdle: window.RazorVars ? RazorVars.filepondDragDrop : 'Drop files here',
        allowMultiple: true,
        maxFiles: 5,
        onprocessfile: isLoadingCheck,
        onaddfile: isLoadingCheck,
        onremovefile: isLoadingCheck
    };
    const el1 = document.querySelector('#Uploadfile');
    if (el1) cuspond = FilePond.create(el1, { ...baseOptions });
    const el2 = document.querySelector('#Uploadfile99');
    if (el2) cuspondVehicle = FilePond.create(el2, { ...baseOptions });
}

/* ==================================================
   Events
================================================== */
function initializeEventHandlers() {
    // keep color in sync with meter text
    $(document).on('DOMSubtreeModified', ".dm-valueP", function () { ChangeFuelSliderColor(); });

    $("#AddRow").click(function () { AddWorkOrderRow(); });

    // Open vehicle selection modal
    $("#SelectVehicle").click(function () { OpenChangeCarModal(); });
    $(document).on('vehicleSelected', function (event, vehicleId) { SelectVehicle(vehicleId); });

    $("input:radio[name=group1]:first").prop('checked', true).trigger('change');
    $('input[name="Type"]').on("change", function () { ChangesMaintenanceType(); });

    $(document).on("change", "input[name=fk_VehicleId]", function () {
        $('.checkedCard').removeClass('border border-primary');
        $(this).closest('.checkedCard').addClass('border border-primary');
        $('.card-overlay').removeClass('card-overlay-active');
        $(this).closest('.card-overlay').addClass('card-overlay-active');
    });

    $("#btnAddNewVC").click(function () { AddNewCustomerAndVehicle(); });

    $(document).on('change', '.WorkOrderSelect', function () {
        var selectedId = $(this).val();
        var row = $(this).closest('tr');

        var hasValue = selectedId && selectedId !== "0";
        row.find('.Status')
            .text(hasValue ? RazorVars.workOrderMaintenance : RazorVars.regularMaintenance);
        debugger;
        if (hasValue) {
            var wo = WorkOrdersList.find(x => String(x.Id) === String(selectedId));
            if (wo) {
                row.find('.MaintenanceDesc').val(wo.Description || "");
            }
        }
    });
}

function initializeValidation() {
    if (!$.validator) return;

    $.validator.addMethod("valueNotEquals", function (value, element, arg) {
        return arg !== value && value !== "" && value !== "0";
    }, RazorVars.requiredField);

    $.validator.addMethod("greaterOrEqualTo", function (value, element, selector) {
        var other = parseInt($(selector).val(), 10);
        var current = parseInt(value, 10);
        if (isNaN(other) || isNaN(current)) return true;
        return current >= other;
    }, function () { return RazorVars.odometerError + " : " + $("#VehicleOdoMeter").val() + " " + RazorVars.kiloMeter; });

    // At least one of two fields must be filled
    $.validator.addMethod("requireOneGroup", function (value, element, params) {
        function hasVal(v) {
            return v != null && $.trim(String(v)) !== "" && $.trim(String(v)) !== "0";
        }

        var otherSelector = params && params.other ? params.other : null;
        var thisHas = hasVal(value);
        var otherHas = otherSelector ? hasVal($(otherSelector).val()) : false;

        return thisHas || otherHas;
    }, RazorVars.requiredField);

    $.validator.setDefaults({
        errorElement: "div",
        errorClass: "field-error",
        highlight: function (element) {
            var $el = $(element);
            if ($el.hasClass("select2-hidden-accessible")) {
                $el.next(".select2").find(".select2-selection").addClass("is-invalid");
            } else {
                $el.addClass("is-invalid");
            }
        },
        unhighlight: function (element) {
            var $el = $(element);
            if ($el.hasClass("select2-hidden-accessible")) {
                $el.next(".select2").find(".select2-selection").removeClass("is-invalid");
            } else {
                $el.removeClass("is-invalid");
            }
        },
        errorPlacement: function (error, element) {
            if (element.hasClass("select2-hidden-accessible")) { error.insertAfter(element.next(".select2")); return; }
            if (element.hasClass("filepond--browser") || element.attr("id") === "Uploadfile") {
                error.insertAfter($(element).closest(".filepond--root")); return;
            }
            error.insertAfter(element);
        }
    });

    $('form[id="movements"]').validate({
        ignore: ':hidden:not(.select2-hidden-accessible)',
        rules: {
            ReceivedBranchId: { valueNotEquals: "0" },
            ReceivedTime: { required: true },
            ReceivedMeter: { required: true, min: 1, greaterOrEqualTo: "#VehicleOdoMeter" },
            ResivedDriverId: { required: true },
            GregorianMovementDate: { required: true },
            VehicleSubStatusId: { valueNotEquals: "0" },
            WorkOrderId: {
                requireOneGroup: { other: "#Complaint" }
            },
            Complaint: {
                requireOneGroup: { other: "#WorkOrderId" }
            }
        },
        messages: {
            ReceivedBranchId: { valueNotEquals: RazorVars.requiredField },
            ReceivedTime: { required: RazorVars.requiredField },
            ReceivedMeter: { required: RazorVars.requiredField, min: RazorVars.moreThan + ' ' + 0 },
            ResivedDriverId: { required: RazorVars.requiredField },
            GregorianMovementDate: { required: RazorVars.requiredField },
            VehicleSubStatusId: { valueNotEquals: RazorVars.requiredField },
            WorkOrderId: { requireOneGroup: RazorVars.requiredField },
            Complaint: { requireOneGroup: RazorVars.requiredField }
        },
        submitHandler: function (form) { handleFormSubmission(form); return false; }
    });
}

function attachInitialRowRules() {
    // left intentionally commented
}

/* ==================================================
   Submit
================================================== */
function handleFormSubmission(form) {
    if (!$(form).valid()) return false;
    $('#submitMovement').prop("disabled", true);

    var files = (cuspond && typeof cuspond.getFiles === 'function') ? cuspond.getFiles() : [];
    if (files.length <= 0) {
        Swal.fire(RazorVars.addFilesForVehicle, '', 'error');
        $('#submitMovement').prop("disabled", false);
        return false;
    }
    if (!validPhoto) { $('#submitMovement').prop("disabled", false); return false; }

    if (selectCount === 0 || !($("#VehicleID").val() > 0)) {
        Swal.fire(RazorVars.chooseVehicle, '', 'error');
        $('#submitMovement').prop("disabled", false);
        return false;
    }

    if (!$("input[name='Type']:checked").val()) {
        Swal.fire(RazorVars.chooseMovementType, '', 'error');
        $('#submitMovement').prop("disabled", false);
        return false;
    }

    if (!validateOdometer()) { $('#submitMovement').prop("disabled", false); return false; }

    var fd = new FormData(form);
    for (var i = 0; i < files.length; i++) fd.append('Photos', files[i].file);

    if ($.fn && $.fn.jSignature) {
        fd.append('driverSignatureBase64', $("#DriverSignature").jSignature("getData", "image"));
        fd.append('employeeSignatureBase64', $("#EmployeeSignature").jSignature("getData", "image"));
    } else {
        fd.append('driverSignatureBase64', '');
        fd.append('employeeSignatureBase64', '');
    }
    fd.append('EType', Etype);
    debugger;
    $.ajax({
        url: RazorVars.vehicleMovementInInsertUrl,
        type: 'POST',
        data: fd,
        dataType: 'JSON',
        contentType: false,
        cache: false,
        processData: false
    }).done(CreateMovementSuccess)
        .fail(function () {
            Swal.fire(RazorVars.swalErrorHappened, RazorVars.error, 'error');
            $('#submitMovement').prop("disabled", false);
        });
}

/* ==================================================
   Helpers
================================================== */
function validateOdometer() {
    var $rm = $('#ReceivedMeter');
    var receivedMeter = parseInt($rm.val(), 10);
    var vehicleOdoMeter = parseInt($("#VehicleOdoMeter").val(), 10);
    if (!isFinite(receivedMeter) || !isFinite(vehicleOdoMeter)) return true;
    if (receivedMeter < vehicleOdoMeter) {
        $rm.addClass("is-invalid");
        Swal.fire(RazorVars.error, RazorVars.odometerError + " : " + vehicleOdoMeter + " " + RazorVars.kiloMeter, 'error');
        return false;
    }
    $rm.removeClass("is-invalid");
    return true;
}

function CreateMovementSuccess(data) {
    if (data && data.isSuccess) {
        Swal.fire({
            icon: 'success',
            title: RazorVars.doneSuccessfully,
            text: '',
            confirmButtonText: RazorVars.btnOk,
            confirmButtonColor: 'var(--primary-600)',
            timer: 3000,
            timerProgressBar: true
        }).then(function () { window.location.href = window.RazorVars.indexMovementsUrl; });
    } else {
        Swal.fire(RazorVars.swalErrorHappened, (data && data.message) || RazorVars.error, 'error');
        $('#submitMovement').prop("disabled", false);
    }
}

function OpenChangeCarModal() { $("#OpenAddVehicleModal").modal('show'); }

function ChangeWorkOrderType() {
    // no-op for now
}

function ChangesMaintenanceType() {
    var selectedType = $('input[name="Type"]:checked').val();
    if (selectedType == "option1") {
        $('#AddRow').show();
        $('#IsRegularMaintenance').val(true);
    } else if (selectedType == "option2") {
        $('#AddRow').hide();
        $("#tblMainteneance").find("tbody tr:gt(0)").remove();
        $('#IsRegularMaintenance').val(false);
    }
    ChangeWorkOrderType();
}

function isLoadingCheck() {
    if (!cuspond || typeof cuspond.getFiles !== 'function') {
        $('#submitMovement').prop("disabled", false);
        validPhoto = true;
        return;
    }
    var isLoading = cuspond.getFiles().filter(function (x) { return x.status !== 2; }).length !== 0;
    if (isLoading) {
        $('#submitMovement').prop("disabled", true);
        validPhoto = false;
    } else {
        $('#submitMovement').prop("disabled", false);
        validPhoto = true;
    }
}

function ChangeFuelSliderColor() {
    var $val = $(".dm-valueP");
    if (!$val.length) return;
    var value = parseInt($val.html(), 10);
    if (!isFinite(value)) return;
    var color = value <= 10 ? "red" : value <= 20 ? "var(--theme-warning)" : "#228b22";
    $val.css("color", color);
}

/* ==================================================
   Data (AJAX)
================================================== */
function GetMBranches() {
    $.ajax({
        type: 'GET',
        url: RazorVars.branchesUrl,
        dataType: 'json'
    }).done(function (result) {
        var $sel = $('#ReceivedBranchId');
        $sel.empty();

        for (var i = 0; i < result.length; i++) {
            $sel.append(
                '<option value="' + result[i].BranchID + '">' +
                result[i].Name +
                '</option>'
            );
        }

        $sel.val(RazorVars.ReceivedBranchId);
    });
}

/* -------- Vehicle Sub Status (robust, no undefined) -------- */
function firstArrayIn(obj) {
    if (Array.isArray(obj)) return obj;
    if (!obj || typeof obj !== 'object') return [];
    const preferredKeys = ['data', 'items', 'result', 'results', 'list', 'value', 'values'];
    for (const k of preferredKeys) {
        if (Array.isArray(obj[k])) return obj[k];
    }
    for (const k in obj) {
        if (Array.isArray(obj[k])) return obj[k];
    }
    return [];
}
function pickId(item) {
    if (item == null || typeof item !== 'object') return null;
    const idKeys = [
        'id', 'Id', 'ID', 'subStatusId', 'SubStatusId', 'VehicleSubStatusId',
        'SubStatusID', 'VehicleSubStatusID', 'Value', 'value', 'Key', 'key'
    ];
    for (const k of idKeys) if (k in item && item[k] != null && item[k] !== '') return item[k];
    for (const k in item) {
        const v = item[k];
        if (typeof v === 'number') return v;
        if (typeof v === 'string' && /^\d+$/.test(v)) return v;
    }
    return null;
}
function pickText(item) {
    if (item == null) return null;
    if (typeof item === 'string') return item;
    const textKeys = [
        'text', 'name', 'Name', 'label', 'Label', 'title', 'Title',
        'VehicleSubStatusName', 'SubStatusName', 'EnglishName', 'ArabicName',
        'PrimaryName', 'DisplayName', 'EnName', 'ArName', 'Description', 'desc', 'Desc'
    ];
    for (const k of textKeys) if (k in item && item[k]) return String(item[k]);
    for (const k in item) {
        const v = item[k];
        if (typeof v === 'string' && v.trim()) return v.trim();
    }
    return null;
}
function normalizeSubStatusList(list) {
    const out = [];
    for (let i = 0; i < list.length; i++) {
        const raw = list[i];
        const id = pickId(raw);
        const text = pickText(raw);
        out.push({
            id: id != null ? id : i,
            text: text != null ? text : '(item #' + (i + 1) + ')'
        });
    }
    return out;
}
function PopulateVehicleSubStatus(statusId) {
    const $sel = $('#VehicleSubStatusId');
    if (!$sel.length) return;

    if (!statusId) {
        $sel.empty().append('<option value="" selected hidden>' + RazorVars.select + '</option>');
        if ($.fn.select2) {
            if ($sel.hasClass('select2-hidden-accessible')) $sel.select2('destroy');
            $sel.select2({ width: '100%' });
        }
        return;
    }

    $.ajax({
        type: 'GET',
        url: RazorVars.vehicleSubStatusesUrl + '?statusId=' + encodeURIComponent(statusId),
        dataType: 'json'
    }).done(function (resp) {
        const list = firstArrayIn(resp);
        try { console.debug('SubStatus raw payload:', resp, 'resolved list:', list); } catch (_) { }

        const normalized = normalizeSubStatusList(list);

        $sel.empty().append('<option value="" selected hidden>' + RazorVars.select + '</option>');
        normalized.forEach(function (opt) {
            $sel.append(
                '<option value="' + String(opt.id) + '">' +
                String(opt.text) +
                '</option>'
            );
        });

        if ($.fn.select2) {
            if ($sel.hasClass('select2-hidden-accessible')) $sel.select2('destroy');
            $sel.select2({
                width: '100%',
                templateResult: function (data) { return document.createTextNode(data.text || data.label || String(data.id || '')); },
                templateSelection: function (data) { return data.text || data.label || String(data.id || ''); },
                escapeMarkup: function (m) { return m; }
            });
        }
    }).fail(function () {
        Swal.fire(RazorVars.swalErrorHappened, RazorVars.error, 'error');
    });
}
/* --------------------------------------------------- */

function GetVehicleData(vehicleId) {
    $('.MaintenanceDesc').val("");
    $('#WorkOrderId').html("");
    $('input[name="MaintenanceDesc"]').val("");
    let reservationType = Number($("#VehicleTypeFromReservation").val() || 0);
    Etype = reservationType > 0
        ? reservationType
        : Number($("#VehicleTypeId").val());

    if (Etype == 2) { $('input[type="radio"][value="option2"][id="Type"]').prop('disabled', true); }

    $.ajax({
        type: "GET",
        url: RazorVars.getDetailsUrl + '?id=' + vehicleId + "&Type=" + Etype,
        dataType: "JSON"
    }).done(function (Data) {

        $("#ReceivedMeter").val(Data.CurrentMeter + 1);
        $("#VehicleOdoMeter").val(Data.CurrentMeter);
        $("#VehicleManufacturer").text(Data.RefManufacturers.ManufacturerPrimaryName);
        $("#VehicleClass").text(Data.RefVehicleClasses.VehicleClassPrimaryName);
        $("#VehiclePlateNo").text(Data.PlateNumber);
        $("#VehicleModel").text(Data.RefVehicleModels.VehicleModelPrimaryName);
        $("#LastVehicleStatus").val(Data.VehicleStatusId);

        $.ajax({
            type: "GET",
            url: RazorVars.getAgreementByVehicleIdUrl + '?id=' + vehicleId,
            dataType: "json"
        }).done(function (res) {
            debugger;
            if (res && res.isSuccess && res.data && res.data.length > 0) {
                let agr = res.data[0];
                $("#customerName").text(agr.customerName ?? "");
            } else {
                $("#customerName").text("");
            }
        });

        if (Data.VehicleStatusId == 2 && Etype == 1) {
            $.ajax({
                type: "GET",
                url: RazorVars.getLastOutMaintenanceUrl + '?vehicleId=' + $("#VehicleID").val(),
                dataType: "html"
            }).done(function (html) { $('#tblData').html(html); });
        } else {
            $('[name=Type][value="option1"]').prop('checked', true).trigger('change');
            $('.WorkOrderSelect, #WorkOrderId').prop('disabled', false);
            $('.MaintenanceDesc').prop('readonly', false);
            getVehicleWorkOrders($("#VehicleID").val(), 2, Etype);
            $('.Status').text(RazorVars.regularMaintenance);
        }
    });
}

function SelectVehicle(selectedVehicleId) {
    if (!selectedVehicleId) return;
    selectCount += 1;
    $("#VehicleID").val(selectedVehicleId);
    $("#GettingVehicleId").val(selectedVehicleId);
    GetVehicleData(selectedVehicleId);
    $("#ModalOutsideSearch").modal('hide');
    debugger;
    GetWIPByVehicleId(selectedVehicleId);
}

function GetWIPByVehicleId(vehicleId) {
    $.ajax({
        type: 'GET',
        url: RazorVars.GetWIPByVehicleIdUrl + '?id=' + vehicleId,
        dataType: 'json'
    }).done(function (result) {
        Swal.fire({
            title: 'Warning',
            html: "You have already - " + result.data.openWIPCount + " - open WIP<br>" +
                "And - " + result.data.previous + " - WIP from last month",
            icon: 'warning',
            confirmButtonText: 'OK'
        });
    });
}

function getVehicleWorkOrders(vehicleId, type, external) {
    if (vehicleId > 0) {
        $('#tblMainteneance tbody tr:not(:first)').remove();
        rowCount = $('#tblMainteneance > tbody > tr').length;
        WorkOrdersList = [];
        $.ajax({
            type: 'GET',
            url: RazorVars.vehicleWorkOrderListUrl + '?id=' + vehicleId + '&type=' + type + '&isExternal=' + external,
            dataType: 'json'
        }).done(function (result) {
            WorkOrdersList = result || [];
            var $first = $('#WorkOrderId');
            if ($first.length) {
                $first.empty();
                $first.append('<option value="" selected hidden>' + RazorVars.select + '</option>');
                for (var i = 0; i < WorkOrdersList.length; i++) {
                    $first.append(
                        '<option value="' + WorkOrdersList[i].Id + '">' +
                        WorkOrdersList[i].WorkOrderTitle +
                        '</option>'
                    );
                }
                $first.addClass('WorkOrderSelect');
                if ($.fn.select2 && !$first.hasClass('select2-hidden-accessible')) $first.select2({ width: '100%' });
                if ($.isFunction($first.rules)) $first.rules('add', { required: false, messages: { required: RazorVars.requiredField } });
            }
        });
    }
}

function AddWorkOrderRow() {
    var $last = $('#tblMainteneance tbody tr:last');
    var lastDescInput = $last.find('.MaintenanceDesc');
    var lastDesc = lastDescInput.val();
    // if (!lastDesc) { lastDescInput.addClass("is-invalid"); return; } else { lastDescInput.removeClass("is-invalid"); }

    var idx = rowCount++;

    var ddl =
        "<select class='form-control WorkOrderSelect' name='Cards[" + idx + "][WorkOrderId]' data-index='" + idx + "'>" +
        "<option value='' selected hidden>" + RazorVars.select + "</option>" +
        "</select>";

    var desc =
        "<input type='text' class='form-control MaintenanceDesc' " +
        "name='Cards[" + idx + "][Description]' autocomplete='off'>";

    var row =
        "<tr>" +
        "<td>" + ddl + "</td>" +
        "<td>" + desc + "</td>" +
        "<td><label class='Status'>" + RazorVars.regularMaintenance + "</label></td>" +
        "</tr>";

    $("#tblData").append(row);

    var $select = $("#tblData .WorkOrderSelect").last();
    $select.append('<option value="" selected hidden>' + RazorVars.select + '</option>');
    for (var i = 0; i < WorkOrdersList.length; i++) {
        $select.append(
            '<option value="' + WorkOrdersList[i].Id + '">' +
            WorkOrdersList[i].WorkOrderTitle +
            '</option>'
        );
    }
    if ($.fn.select2) $select.select2({ width: '100%' });

    $select.on('change', function () {
        var hasValue = $(this).val() && $(this).val() !== "0";
        $(this).closest('tr').find('.Status')
            .text(hasValue ? RazorVars.workOrderMaintenance : RazorVars.regularMaintenance);
    });

    ChangesMaintenanceType();
}

function AddNewCustomerAndVehicle() {
    var model = {
        ManufacturerId: $("#newManufacturerId").val(),
        VehicleModelId: 1,
        ManufacturingYear: $("#newManufacturerYear").val(),
        PlateNumber: $("#newPlateNumber").val(),
        FuleLevel: 20,
        Color: $("#newColorId").val(),
        ChassisNo: $("#newChassisNumber").val()
    };
    $.ajax({
        url: RazorVars.insertNewVehicleUrl,
        type: 'POST',
        data: JSON.stringify(model),
        dataType: 'JSON',
        contentType: 'application/json'
    }).fail(function () {
        Swal.fire(RazorVars.swalErrorHappened, RazorVars.error, 'error');
    });
}

function onWorkOrderSelected(selectedId, row) {
    debugger;
    var wo = WorkOrdersList.find(x => String(x.Id) === String(selectedId));
    if (!wo) return;

    $(row).find(".MaintenanceDesc").val(wo.Description || "");
    $(row).find(".Status").text(RazorVars.workOrderMaintenance);
}

$(document).ready(function () {
    const incomingVehicle = Number($("#VehicleFromReservation").val() || 0);
    if (incomingVehicle == 0) {
        OpenChangeCarModal();
    }

    // When WorkOrderId or MaintenanceDesc has a value, clear Complaint side
    $('#WorkOrderId, #MaintenanceDesc').on('input change', function () {
        let workOrderVal = ($('#WorkOrderId').val() || '').toString().trim();
        let maintenanceDescVal = ($('#MaintenanceDesc').val() || '').toString().trim();

        let hasValue1 = (workOrderVal !== "" && workOrderVal !== "0") ||
            maintenanceDescVal !== "";

        if (hasValue1) {
            $('#Complaint').val('');
            $('#ComplaintNote').val('');
        }
    });

    // When Complaint / ComplaintNote has a value, clear WorkOrder side
    $('#Complaint, #ComplaintNote').on('input change', function () {
        let complaintVal = ($('#Complaint').val() || '').toString().trim();
        let complaintNoteVal = ($('#ComplaintNote').val() || '').toString().trim();

        let hasValue2 = complaintVal !== "" || complaintNoteVal !== "";

        if (hasValue2) {
            $('#WorkOrderId').val('');
            if ($('#WorkOrderId').hasClass('select2-hidden-accessible')) {
                $('#WorkOrderId').trigger('change');
            }
            $('#MaintenanceDesc').val('');
        }
    });

});
