/* ==================================================
   MovementIn.js (FULL UPDATED) — jQuery version
   Fixes kept:
   - ✅ AJAX Save (Swal + redirect) and prevent normal POST navigation
   - ✅ Modal opens on page load
   - FilePond preview fixes (safe init + plugin registration)
   - Time restriction:
     If GregorianMovementDate == today => ReceivedTime allowed only from (now - 3h) .. now
   - ✅ .validate() initialized inside $(document).ready
================================================== */
(function ($) {
    'use strict';

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

    // prevent double submit
    var isSubmitting = false;
    // Show WIP swal only once per vehicle (prevents spam on repeated Search clicks)
    var wipSwalShownByVehicle = Object.create(null);
    var wipRequestInFlightByVehicle = Object.create(null);

    /* ==================================================
       Small utilities
    ================================================== */
    function formatYMD(dt) {
        return dt.getFullYear() + '-' + pad2(dt.getMonth() + 1) + '-' + pad2(dt.getDate());
    }

    function clampDateToAllowedRange(dateObj, minDate, maxDate) {
        if (!dateObj) return null;
        var t = dateObj.getTime();
        if (t < minDate.getTime()) return new Date(minDate.getTime());
        if (t > maxDate.getTime()) return new Date(maxDate.getTime());
        return dateObj;
    }

    function waitForGlobal(name, cb, maxTries, delayMs) {
        maxTries = maxTries || 60;
        delayMs = delayMs || 100;

        var tries = 0;
        var timer = setInterval(function () {
            tries++;
            if (window[name]) { clearInterval(timer); cb(true); return; }
            if (tries >= maxTries) { clearInterval(timer); cb(false); }
        }, delayMs);
    }

    function pad2(n) { return (n < 10 ? '0' : '') + n; }

    function endSubmit() {
        isSubmitting = false;
        $('#submitMovement').prop("disabled", false);
    }

    /* ==================================================
       Date/Time restriction helpers
       Rule: if date is today => allow only [now-3h, now]
    ================================================== */
    function parseYMD(str) {
        if (!str) return null;
        var m = String(str).trim().match(/^(\d{4})-(\d{2})-(\d{2})$/);
        if (!m) return null;
        var y = +m[1], mo = +m[2] - 1, d = +m[3];
        var dt = new Date(y, mo, d);
        if (isNaN(dt.getTime())) return null;
        return dt;
    }

    function sameDay(a, b) {
        return a && b &&
            a.getFullYear() === b.getFullYear() &&
            a.getMonth() === b.getMonth() &&
            a.getDate() === b.getDate();
    }

    function timeStrToMinutes(t) {
        if (!t) return null;
        var parts = String(t).split(':');
        if (parts.length < 2) return null;
        var hh = parseInt(parts[0], 10);
        var mm = parseInt(parts[1], 10);
        if (!isFinite(hh) || !isFinite(mm)) return null;
        if (hh < 0 || hh > 23 || mm < 0 || mm > 59) return null;
        return hh * 60 + mm;
    }

    function minutesToTimeStr(mins) {
        mins = Math.max(0, Math.min(23 * 60 + 59, mins));
        var hh = Math.floor(mins / 60);
        var mm = mins % 60;
        return pad2(hh) + ':' + pad2(mm);
    }

    function getSelectedMovementDate() {
        var $el = $('#GregorianMovementDate');
        if (!$el.length) return null;
        return parseYMD($el.val());
    }

    function applyTimeWindowConstraint() {
        var $dateEl = $('#GregorianMovementDate');
        var $timeEl = $('#ReceivedTime');
        if (!$dateEl.length || !$timeEl.length) return;

        var selected = getSelectedMovementDate();
        var now = new Date();

        if (!selected) {
            $timeEl.removeAttr('min').removeAttr('max');
            return;
        }

        if (!sameDay(selected, now)) {
            $timeEl.removeAttr('min').removeAttr('max').removeClass('is-invalid');
            return;
        }

        var nowMins = now.getHours() * 60 + now.getMinutes();
        var minMins = Math.max(0, nowMins - 180);

        var minStr = minutesToTimeStr(minMins);
        var maxStr = minutesToTimeStr(nowMins);

        $timeEl.attr('min', minStr).attr('max', maxStr);

        var curMins = timeStrToMinutes($timeEl.val());
        if (curMins == null) {
            $timeEl.val(maxStr).removeClass('is-invalid');
            return;
        }

        if (curMins < minMins) {
            $timeEl.val(minStr).addClass('is-invalid');
        } else if (curMins > nowMins) {
            $timeEl.val(maxStr).addClass('is-invalid');
        } else {
            $timeEl.removeClass('is-invalid');
        }
    }

    function initializeDateTimeRestrictionHandlers() {
        var $dateEl = $('#GregorianMovementDate');
        var $timeEl = $('#ReceivedTime');
        if (!$dateEl.length || !$timeEl.length) return;

        $dateEl.on('change input', applyTimeWindowConstraint);
        $timeEl.on('change input', applyTimeWindowConstraint);

        applyTimeWindowConstraint();
    }

    /* ==================================================
       FilePond: register plugins + init (safe)
    ================================================== */
    function registerFilePondPlugins() {
        if (!window.FilePond) return false;

        var plugins = [];
        if (window.FilePondPluginFileValidateType) plugins.push(window.FilePondPluginFileValidateType);
        if (window.FilePondPluginImagePreview) plugins.push(window.FilePondPluginImagePreview);
        if (window.FilePondPluginImageExifOrientation) plugins.push(window.FilePondPluginImageExifOrientation);

        if (window.FilePondPluginImageTransform) plugins.push(window.FilePondPluginImageTransform);
        if (window.FilePondPluginImageCrop && window.FilePondPluginImageTransform) plugins.push(window.FilePondPluginImageCrop);

        try {
            FilePond.registerPlugin.apply(FilePond, plugins);
            return true;
        } catch (e) {
            console.error('[FilePond] registerPlugin failed:', e);
            return false;
        }
    }

    function initializeFilePond() {
        if (!window.FilePond) return;

        registerFilePondPlugins();

        var baseOptions = {
            allowImagePreview: true,
            imagePreviewHeight: 150,
            imagePreviewMaxFileSize: '50MB',

            allowMultiple: true,
            maxFiles: 5,

            allowFileTypeValidation: true,
            acceptedFileTypes: ['image/*', 'application/pdf'],

            credits: false,
            labelIdle: (window.RazorVars && RazorVars.filepondDragDrop) ? RazorVars.filepondDragDrop : 'Drop files here',

            onaddfile: function () { isLoadingCheck(); },
            onremovefile: function () { isLoadingCheck(); }
        };

        var el1 = $('#Uploadfile')[0];
        if (el1) {
            if (!el1._pondInstance) {
                cuspond = FilePond.create(el1, baseOptions);
                el1._pondInstance = cuspond;
            } else {
                cuspond = el1._pondInstance;
            }
        } else {
            console.error('[FilePond] #Uploadfile not found');
        }

        var el2 = $('#Uploadfile99')[0];
        if (el2) {
            if (!el2._pondInstance) {
                cuspondVehicle = FilePond.create(el2, baseOptions);
                el2._pondInstance = cuspondVehicle;
            } else {
                cuspondVehicle = el2._pondInstance;
            }
        }
    }

    /* ==================================================
       Components
    ================================================== */
    function initializeMovementDatePicker() {
        var $el = $('#GregorianMovementDate');
        if (!$el.length) return;

        // Permission flag coming from Razor
        var canBackTime = !!(window.RazorVars && RazorVars.canMovementInBackTime);

        // Today (start-of-day)
        var today = new Date();
        today.setHours(0, 0, 0, 0);

        // Allowed range
        var maxDate = new Date(today.getTime()); // today
        var minDate = canBackTime
            ? new Date(today.getTime() - (2 * 24 * 60 * 60 * 1000)) // today - 2 days (last 3 days inclusive)
            : new Date(today.getTime()); // today only

        // If there is already a value, clamp it; otherwise default to today
        var current = parseYMD($el.val());
        current = current ? clampDateToAllowedRange(current, minDate, maxDate) : maxDate;
        $el.val(formatYMD(current));

        // If flatpickr exists, use it
        if (window.flatpickr) {
            // If already initialized, destroy to avoid duplicate instances
            if ($el[0]._flatpickr) {
                try { $el[0]._flatpickr.destroy(); } catch (_) { }
            }

            window.flatpickr($el[0], {
                dateFormat: "Y-m-d",
                defaultDate: current,
                minDate: minDate,
                maxDate: maxDate,
                allowInput: false,
                disableMobile: true,

                onReady: function () {
                    // make sure time rule applies immediately
                    $el.trigger('change');
                },
                onChange: function () {
                    $el.trigger('change');
                }
            });

            return;
        }

        // Fallback (if flatpickr is not loaded): enforce by resetting invalid input
        $el.on('change input', function () {
            var d = parseYMD($el.val());
            d = d ? clampDateToAllowedRange(d, minDate, maxDate) : maxDate;
            $el.val(formatYMD(d));
            $el.trigger('change');
        });
    }

    function initializeComponents() {
        var d = new Date(), h = d.getHours(), m = d.getMinutes();
        if (h < 10) h = '0' + h;
        if (m < 10) m = '0' + m;

        $('#ReceivedTime').val(h + ":" + m);
        applyTimeWindowConstraint();

        ChangeWorkOrderType();
        ChangesMaintenanceType();
        rowCount = $('#tblMainteneance > tbody > tr').length;

        if (window.RazorVars && RazorVars.VehicleID) { selectCount += 1; }

        if ($("#GettingVehicleId").val() > 0) {
            GetVehicleData($("#GettingVehicleId").val());
        }

        var incomingVehicle = Number($("#VehicleFromReservation").val() || 0);
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

        $('#fuelSlider').on('input', function () {
            var val = Number($(this).val());
            $('#fuelSliderValue').text(String(val));
            if ($myFuelMeter && typeof $myFuelMeter.changeValue === 'function') {
                $myFuelMeter.changeValue(val.toFixed(1));
            } else {
                try {
                    $("div#fuelMeterDiv").dynameter && $("div#fuelMeterDiv").dynameter('setValue', val);
                } catch (_) { }
            }
        });
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

        $('#DeepScratch-btn').on('click', function () { setMode('deep-scratch', 'DeepScratch', 'deep-scratch-cursor'); });
        $('#SmallScratch-btn').on('click', function () { setMode('small-scratch', 'SmallScratch', 'small-scratch-cursor'); });
        $('#VeryDeepScratch-btn').on('click', function () { setMode('very-deep-scratch', 'VeryDeepScratch', 'very-deep-cursor'); });
        $('#BendInBody-btn').on('click', function () { setMode('bend-in-body', 'BendInBody', 'bend-body-cursor'); });

        $('#panel').on('click', function (e) {
            if (!shapeType) return;

            var id = ++shapeCount;
            var $this = $(this);
            var offset = $this.offset();
            var xCoord = e.pageX - offset.left;
            var yCoord = e.pageY - offset.top;

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
            $('#strikes').val(JSON.stringify(shapesJson));
        });

        $('#clear-button').on('click', function () {
            shapesJson = [];
            $("#panel").empty();
            $('#strikes').val(JSON.stringify(shapesJson));
        });

        $(document).on("contextmenu", ".shape", function (e) {
            e.preventDefault();
            $(this).remove();
            return false;
        });
    }

    /* ==================================================
       Events
    ================================================== */
    function initializeEventHandlers() {
        $(document).on('DOMSubtreeModified', ".dm-valueP", function () { ChangeFuelSliderColor(); });

        $("#AddRow").on('click', function () { AddWorkOrderRow(); });

        $("#SelectVehicle").on('click', function () { OpenChangeCarModal(); });
        $(document).on('vehicleSelected', function (event, vehicleId) { SelectVehicle(vehicleId); });

        $("input:radio[name=group1]:first").prop('checked', true).trigger('change');
        $('input[name="Type"]').on("change", function () { ChangesMaintenanceType(); });

        $(document).on("change", "input[name=fk_VehicleId]", function () {
            $('.checkedCard').removeClass('border border-primary');
            $(this).closest('.checkedCard').addClass('border border-primary');
            $('.card-overlay').removeClass('card-overlay-active');
            $(this).closest('.card-overlay').addClass('card-overlay-active');
        });

        $("#btnAddNewVC").on('click', function () { AddNewCustomerAndVehicle(); });

        $(document).on('change', '.WorkOrderSelect', function () {
            var selectedId = $(this).val();
            var row = $(this).closest('tr');

            var hasValue = selectedId && selectedId !== "0";
            row.find('.Status')
                .text(hasValue ? RazorVars.workOrderMaintenance : RazorVars.regularMaintenance);

            if (hasValue) {
                var wo = WorkOrdersList.find(function (x) { return String(x.Id) === String(selectedId); });
                if (wo) row.find('.MaintenanceDesc').val(wo.Description || "");
            }
        });

        // ✅ Critical: STOP normal navigation submit, ALWAYS go through AJAX
        $(document).on('submit', 'form#movements', function (e) {
            e.preventDefault();
            handleFormSubmission(this);
            return false;
        });

        // ✅ If your Save button is not type="submit", this makes it work
        $(document).on('click', '#submitMovement', function (e) {
            e.preventDefault();
            var $form = $('form#movements');
            if ($form.length) $form.trigger('submit');
        });

        $("#hasRecall").on("change", function () {
            if ($(this).is(":checked")) {
                $("#recallList").prop('disabled', false);
            } else {
                $("#recallList").prop('disabled', true);
                $("#recallList").val("").trigger("change");
            }
        });
        $("#hasRecall").trigger("change");
    }

    /* ==================================================
       Validation (adds time window validation too)
       ✅ .validate() is initialized in $(document).ready below
    ================================================== */
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

        $.validator.addMethod("requireOneGroup", function (value, element, params) {
            function hasVal(v) {
                return v != null && $.trim(String(v)) !== "" && $.trim(String(v)) !== "0";
            }

            var otherSelector = params && params.other ? params.other : null;
            var thisHas = hasVal(value);
            var otherHas = otherSelector ? hasVal($(otherSelector).val()) : false;

            return thisHas || otherHas;
        }, RazorVars.requiredField);

        $.validator.addMethod("timeWithinLast3HoursIfToday", function (value) {
            var selected = getSelectedMovementDate();
            if (!selected) return true;

            var now = new Date();
            if (!sameDay(selected, now)) return true;

            var nowMins = now.getHours() * 60 + now.getMinutes();
            var minMins = Math.max(0, nowMins - 180);

            var tMins = timeStrToMinutes(value);
            if (tMins == null) return false;

            return tMins >= minMins && tMins <= nowMins;
        }, function () {
            return "Time must be within the last 3 hours (and not in the future) when the date is today.";
        });

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

        // ✅ Initialize validate on the form
        $('form#movements').validate({
            ignore: ':hidden:not(.select2-hidden-accessible)',
            rules: {
                ReceivedBranchId: { valueNotEquals: "0" },
                ReceivedTime: { required: true, timeWithinLast3HoursIfToday: true },
                ReceivedMeter: { required: true, min: 1, greaterOrEqualTo: "#VehicleOdoMeter" },
                ResivedDriverId: { required: true },
                GregorianMovementDate: { required: true },
                VehicleSubStatusId: { valueNotEquals: "0" },
                WorkOrderId: { requireOneGroup: { other: "#Complaint" } },
                Complaint: { requireOneGroup: { other: "#WorkOrderId" } }
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
            }
        });
    }

    /* ==================================================
       Submit (AJAX only)
    ================================================== */
    function handleFormSubmission(form) {
        applyTimeWindowConstraint();

        // validate first (don’t lock submit if invalid)
        if ($.validator && $(form).data('validator')) {
            if (!$(form).valid()) return false;
        }

        if (isSubmitting) return false;
        isSubmitting = true;

        $('#submitMovement').prop("disabled", true);

        var files = (cuspond && typeof cuspond.getFiles === 'function') ? cuspond.getFiles() : [];
        if (files.length <= 0) {
            Swal.fire(RazorVars.addFilesForVehicle, '', 'error');
            endSubmit();
            return false;
        }
        if (!validPhoto) { endSubmit(); return false; }

        if (selectCount === 0 || !($("#VehicleID").val() > 0)) {
            Swal.fire(RazorVars.chooseVehicle, '', 'error');
            endSubmit();
            return false;
        }

        if (!$("input[name='Type']:checked").val()) {
            Swal.fire(RazorVars.chooseMovementType, '', 'error');
            endSubmit();
            return false;
        }

        if (!validateOdometer()) { endSubmit(); return false; }

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
                endSubmit();
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
            }).then(function () {
                window.location.href = window.RazorVars.indexMovementsUrl;
            });
        } else {
            Swal.fire(RazorVars.swalErrorHappened, (data && data.message) || RazorVars.error, 'error');
            endSubmit();
        }
    }

    function OpenChangeCarModal() { $("#OpenAddVehicleModal").modal('show'); }

    function ChangeWorkOrderType() { /* no-op */ }

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

        var files = cuspond.getFiles();
        var hasFiles = files && files.length > 0;

        if (!hasFiles) {
            $('#submitMovement').prop("disabled", false);
            validPhoto = true;
            return;
        }

        $('#submitMovement').prop("disabled", false);
        validPhoto = true;
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

            $("#ReceivedMeter").val(Data.CurrentMeter);
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
                if (res && res.isSuccess && res.data && res.data.length > 0) {
                    var agr = res.data[0];
                    $("#customerName").text(agr.customerName ?? "");
                } else {
                    $("#customerName").text("");
                }
            });

            if (Data.VehicleStatusId == 2 && Etype == 1) {
                getVehicleWorkOrders($("#VehicleID").val(), 1, Etype);
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
        GetWIPByVehicleId(selectedVehicleId);
        window.location.href = RazorVars.selectVehicleUrl + "?vehicleId=" + $("#GettingVehicleId").val();
    }

    function GetWIPByVehicleId(vehicleId) {
        vehicleId = Number(vehicleId || 0);
        if (!vehicleId) return;

        if (wipSwalShownByVehicle[vehicleId]) return;

        if (wipRequestInFlightByVehicle[vehicleId]) return;
        wipRequestInFlightByVehicle[vehicleId] = true;

        $.ajax({
            type: 'GET',
            url: RazorVars.GetWIPByVehicleIdUrl + '?id=' + vehicleId,
            dataType: 'json'
        }).done(function (result) {
            wipRequestInFlightByVehicle[vehicleId] = false;

            if (wipSwalShownByVehicle[vehicleId]) return;
            wipSwalShownByVehicle[vehicleId] = true;

            Swal.fire({
                title: 'Warning',
                html: "You have already - " + result.data.openWIPCount + " - open WIP<br>" +
                    "And - " + result.data.previous + " - WIP from last month",
                icon: 'warning',
                confirmButtonText: 'OK'
            });
        }).fail(function () {
            wipRequestInFlightByVehicle[vehicleId] = false;
        });
    }


    function getVehicleWorkOrders(vehicleId, type, external) {
        if (vehicleId > 0) {
            $('#tblMainteneance tbody tr:not(:first)').remove();
            rowCount = $('#tblMainteneance > tbody > tr').length;
            WorkOrdersList = [];

            $.ajax({
                type: 'GET',
                url: RazorVars.vehicleWorkOrderListUrl + '?id=' + vehicleId + '&type=' + type + '&isExternal=' + (external != 1),
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

    /* ==================================================
       BOOT (jQuery ready)
       ✅ validate() is initialized here via initializeValidation()
    ================================================== */
    $(document).ready(function () {
        initializeMovementDatePicker();          // ✅ set allowed date range first
        initializeDateTimeRestrictionHandlers(); // ✅ then bind time restriction rules

        waitForGlobal('FilePond', function (ok) {
            if (!ok) {
                console.error('[FilePond] core not found. Check script order / CSP / network.');
                return;
            }
            initializeFilePond();
        });

        initializeComponents();
        initializeScratches();
        initializejSignature();
        initializeEventHandlers();
        initializeValidation();

        var incomingVehicle = Number($("#VehicleFromReservation").val() || 0);
        if (incomingVehicle === 0) {
            OpenChangeCarModal();
        }
    });


})(jQuery);


