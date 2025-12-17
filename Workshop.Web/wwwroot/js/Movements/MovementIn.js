/* ==================================================
   MovementIn.js (FULL UPDATED)
   Adds:
   - FilePond preview fixes (safe init + plugin registration)
   - Time restriction:
     If GregorianMovementDate == today => ReceivedTime allowed only from (now - 3h) .. now
================================================== */
(function () {
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

    /* ==================================================
       Small utilities
    ================================================== */
    function domReady(fn) {
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', fn);
        else fn();
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
        var el = document.getElementById('GregorianMovementDate');
        if (!el) return null;
        return parseYMD(el.value);
    }

    function applyTimeWindowConstraint() {
        var dateEl = document.getElementById('GregorianMovementDate');
        var timeEl = document.getElementById('ReceivedTime');
        if (!dateEl || !timeEl) return;

        var selected = getSelectedMovementDate();
        var now = new Date();

        // If date isn't parsable, just clear constraints
        if (!selected) {
            timeEl.removeAttribute('min');
            timeEl.removeAttribute('max');
            return;
        }

        // Only constrain when selected date == today
        if (!sameDay(selected, now)) {
            timeEl.removeAttribute('min');
            timeEl.removeAttribute('max');
            timeEl.classList.remove('is-invalid');
            return;
        }

        var nowMins = now.getHours() * 60 + now.getMinutes();
        var minMins = Math.max(0, nowMins - 180); // last 3 hours (180 mins), clamp to midnight

        var minStr = minutesToTimeStr(minMins);
        var maxStr = minutesToTimeStr(nowMins);

        timeEl.setAttribute('min', minStr);
        timeEl.setAttribute('max', maxStr);

        // Clamp current value if user typed something outside range
        var curMins = timeStrToMinutes(timeEl.value);
        if (curMins == null) {
            // If empty/invalid, set to now
            timeEl.value = maxStr;
            timeEl.classList.remove('is-invalid');
            return;
        }

        if (curMins < minMins) {
            timeEl.value = minStr;
            timeEl.classList.add('is-invalid');
        } else if (curMins > nowMins) {
            timeEl.value = maxStr;
            timeEl.classList.add('is-invalid');
        } else {
            timeEl.classList.remove('is-invalid');
        }
    }

    function initializeDateTimeRestrictionHandlers() {
        var dateEl = document.getElementById('GregorianMovementDate');
        var timeEl = document.getElementById('ReceivedTime');
        if (!dateEl || !timeEl) return;

        // Re-apply on date changes (flatpickr usually triggers change/input)
        dateEl.addEventListener('change', applyTimeWindowConstraint);
        dateEl.addEventListener('input', applyTimeWindowConstraint);

        // Re-apply on time typing/picking
        timeEl.addEventListener('change', applyTimeWindowConstraint);
        timeEl.addEventListener('input', applyTimeWindowConstraint);

        // Apply once at startup
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

        // Crop depends on Transform
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

        var el1 = document.querySelector('#Uploadfile');
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

        var el2 = document.querySelector('#Uploadfile99');
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
       Boot
    ================================================== */
    domReady(function () {
        // date/time restriction doesn't need jQuery
        initializeDateTimeRestrictionHandlers();

        // FilePond might be loaded after this file if order is wrong; wait a bit
        waitForGlobal('FilePond', function (ok) {
            if (!ok) {
                console.error('[FilePond] core not found. Check script order / CSP / network.');
                return;
            }
            initializeFilePond();
        });

        // The rest of the page relies on jQuery
        waitForGlobal('jQuery', function (jqOk) {
            if (!jqOk) {
                console.error('[jQuery] not found. Page logic (except FilePond + time constraints) will not run.');
                return;
            }

            $(function () {
                initializeComponents();
                initializeScratches();
                initializejSignature();
                initializeEventHandlers();
                initializeValidation();
            });
        });
    });

    /* ==================================================
       Components
    ================================================== */
    function initializeComponents() {
        // Set time now
        var d = new Date(), h = d.getHours(), m = d.getMinutes();
        if (h < 10) h = '0' + h;
        if (m < 10) m = '0' + m;

        $('#ReceivedTime').val(h + ":" + m);

        // Apply time window after setting default time
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

        // native range slider
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
                    } catch (_) { }
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

        $('#DeepScratch-btn').click(function () { setMode('deep-scratch', 'DeepScratch', 'deep-scratch-cursor'); });
        $('#SmallScratch-btn').click(function () { setMode('small-scratch', 'SmallScratch', 'small-scratch-cursor'); });
        $('#VeryDeepScratch-btn').click(function () { setMode('very-deep-scratch', 'VeryDeepScratch', 'very-deep-cursor'); });
        $('#BendInBody-btn').click(function () { setMode('bend-in-body', 'BendInBody', 'bend-body-cursor'); });

        $('#panel').click(function (e) {
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
       Events
    ================================================== */
    function initializeEventHandlers() {
        $(document).on('DOMSubtreeModified', ".dm-valueP", function () { ChangeFuelSliderColor(); });

        $("#AddRow").click(function () { AddWorkOrderRow(); });

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

            if (hasValue) {
                var wo = WorkOrdersList.find(function (x) { return String(x.Id) === String(selectedId); });
                if (wo) row.find('.MaintenanceDesc').val(wo.Description || "");
            }
        });
    }

    /* ==================================================
       Validation (adds time window validation too)
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

        // ✅ Time rule validator: if date is today => time must be within [now-3h, now]
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
            // message
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

        $('form[id="movements"]').validate({
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
            },
            submitHandler: function (form) { handleFormSubmission(form); return false; }
        });
    }

    /* ==================================================
       Submit
    ================================================== */
    function handleFormSubmission(form) {
        // Ensure time is constrained right before submit too
        applyTimeWindowConstraint();

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
            }).then(function () {
                window.location.href = window.RazorVars.indexMovementsUrl;
            });
        } else {
            Swal.fire(RazorVars.swalErrorHappened, (data && data.message) || RazorVars.error, 'error');
            $('#submitMovement').prop("disabled", false);
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
        Etype = $("#VehicleTypeId").val();

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
                if (res && res.isSuccess && res.data && res.data.length > 0) {
                    var agr = res.data[0];
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

})();
