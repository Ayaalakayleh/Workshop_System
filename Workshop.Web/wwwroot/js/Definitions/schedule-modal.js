// ~/js/definitions/schedule-modal.js
(() => {
    function ensureFlatpickr() {
        return new Promise((resolve, reject) => {
            if (window.flatpickr && typeof window.flatpickr === 'function') return resolve();

            const cssId = 'flatpickr-css';
            if (!document.getElementById(cssId)) {
                const link = document.createElement('link');
                link.id = cssId;
                link.rel = 'stylesheet';
                link.href = 'https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css';
                document.head.appendChild(link);
            }

            const jsId = 'flatpickr-js';
            if (document.getElementById(jsId)) {
                const el = document.getElementById(jsId);
                el.addEventListener('load', () => resolve());
                el.addEventListener('error', reject);
                return;
            }

            const script = document.createElement('script');
            script.id = jsId;
            script.src = 'https://cdn.jsdelivr.net/npm/flatpickr';
            script.async = true;
            script.onload = () => resolve();
            script.onerror = (e) => reject(e);
            document.body.appendChild(script);
        });
    }

    function ensureFlatpickrZIndexPatch() {
        if (document.getElementById('flatpickr-zindex-patch')) return;
        const style = document.createElement('style');
        style.id = 'flatpickr-zindex-patch';
        style.textContent = `.flatpickr-calendar { z-index: 200000 !important; }`;
        document.head.appendChild(style);
    }

    // ==================== jQuery DateTimePicker (xdsoft) for schStart ====================
    function ensureJQDateTimePicker() {
        return new Promise((resolve, reject) => {
            if ($.fn && typeof $.fn.datetimepicker === "function") return resolve();

            const cssId = "jq-dtp-css";
            if (!document.getElementById(cssId)) {
                const link = document.createElement("link");
                link.id = cssId;
                link.rel = "stylesheet";
                link.href = "https://cdnjs.cloudflare.com/ajax/libs/jquery-datetimepicker/2.5.21/jquery.datetimepicker.min.css";
                document.head.appendChild(link);
            }

            const jsId = "jq-dtp-js";
            if (document.getElementById(jsId)) {
                const el = document.getElementById(jsId);
                el.addEventListener("load", () => resolve());
                el.addEventListener("error", reject);
                return;
            }

            const script = document.createElement("script");
            script.id = jsId;
            script.src = "https://cdnjs.cloudflare.com/ajax/libs/jquery-datetimepicker/2.5.21/jquery.datetimepicker.full.min.js";
            script.async = true;
            script.onload = () => resolve();
            script.onerror = (e) => reject(e);
            document.body.appendChild(script);
        });
    }

    function ensureJQDateTimePickerZIndexPatch() {
        if (document.getElementById("jq-dtp-zindex-patch")) return;
        const style = document.createElement("style");
        style.id = "jq-dtp-zindex-patch";
        style.textContent = `.xdsoft_datetimepicker { z-index: 200000 !important; }`;
        document.head.appendChild(style);
    }

    // Base allowed times (optional). If you later load business-hours times from API, set this array.
    let schStartBaseAllowedTimes = [];

    // optional: allowedTimes like ["08:00","08:05",...]
    // Updated: hides all times before current time ONLY if selected date is today
    function initSchStartTimepicker(allowedTimes = [], defaultTime = null) {
        const $schStart = $("#schStart");
        if (!$schStart.length) return;
        if (!($.fn && typeof $.fn.datetimepicker === "function")) return;

        // keep base times for dynamic filtering when "today"
        schStartBaseAllowedTimes = Array.isArray(allowedTimes) ? allowedTimes.slice() : [];

        // destroy previous instance if any
        try { $schStart.datetimepicker("destroy"); } catch (e) { }

        // always re-enable on init (in case we disabled it because no times left)
        $schStart.prop('disabled', false);

        const step = 5;

        const opts = {
            datepicker: false,
            format: "H:i",
            step: step,
            scrollInput: false,
            closeOnTimeSelect: true,
            onSelectTime: function () {
                recomputeEndTime();
                $schStart.trigger("change");
            },
            onChangeDateTime: function () {
                recomputeEndTime();
            }
        };

        // ---------- helpers for "today" filtering (kept inside to avoid side-effects elsewhere) ----------
        function localISODate(d = new Date()) {
            return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
        }

        function selectedDateISO() {
            const raw = ($('#schDate').val() || state.date || '').toString().trim();
            return formatDateISO(raw);
        }

        function isSelectedDateToday() {
            const sel = selectedDateISO();
            return !!sel && sel === localISODate();
        }

        function roundUpToStepMinutes(totalMinutes, stepMinutes) {
            return Math.ceil(totalMinutes / stepMinutes) * stepMinutes;
        }

        function nowRoundedHHMM(stepMinutes) {
            const d = new Date();
            const nowMins = d.getHours() * 60 + d.getMinutes();
            const rounded = roundUpToStepMinutes(nowMins, stepMinutes);
            return minutesToHHMM(rounded);
        }

        function buildTimesFromMinutes(startMinutes, stepMinutes) {
            const out = [];
            const last = 24 * 60 - stepMinutes; // 23:55 for step=5
            for (let m = startMinutes; m <= last; m += stepMinutes) out.push(minutesToHHMM(m));
            return out;
        }

        function computeAllowedTimesForToday() {
            const minHHMM = nowRoundedHHMM(step);
            const minMins = toMinutes(minHHMM);

            let times = [];
            if (Array.isArray(schStartBaseAllowedTimes) && schStartBaseAllowedTimes.length > 0) {
                times = schStartBaseAllowedTimes.filter(t => toMinutes(t) >= minMins);
            } else {
                times = buildTimesFromMinutes(minMins, step);
            }

            return { minHHMM, times };
        }
        // ---------------------------------------------------------------------------------------------

        const isToday = isSelectedDateToday();

        if (isToday) {
            const { minHHMM, times } = computeAllowedTimesForToday();

            // no times left today => disable the field safely
            if (!times.length) {
                $schStart.val('').prop('disabled', true);
                recomputeEndTime();
                return;
            }

            // key behavior: use allowTimes so earlier times are HIDDEN (not just disabled)
            opts.allowTimes = times;
            opts.minTime = minHHMM;

            // keep it accurate as time moves forward
            opts.onShow = function () {
                if (!isSelectedDateToday()) return;
                const { minHHMM: min2, times: times2 } = computeAllowedTimesForToday();
                this.setOptions({ minTime: min2, allowTimes: times2 });
            };
        } else {
            // future date: keep original behavior (unless you passed a fixed allowedTimes list)
            if (Array.isArray(schStartBaseAllowedTimes) && schStartBaseAllowedTimes.length > 0) {
                opts.allowTimes = schStartBaseAllowedTimes;
            }
        }

        $schStart.datetimepicker(opts);

        // Choose a safe value
        const currentVal = $schStart.val();
        let val = defaultTime || currentVal || (schStartBaseAllowedTimes && schStartBaseAllowedTimes[0]) || "08:00";

        if (isToday && Array.isArray(opts.allowTimes) && opts.allowTimes.length > 0) {
            // if the chosen value is not allowed anymore, pick the first allowed
            if (!val || !opts.allowTimes.includes(val)) val = opts.allowTimes[0];
        }

        $schStart.val(val);
        recomputeEndTime();
    }
    // ================================================================================

    function callApi({ url, type = 'GET', data = null, isFormData = false, onSuccess = null, onError = null }) {
        const ajaxOptions = {
            url, type, dataType: 'json',
            contentType: isFormData ? false : 'application/json; charset=utf-8',
            processData: !isFormData, cache: false,
            success: (response) => { if (onSuccess) onSuccess(response); },
            error: (xhr, _status, error) => {
                console.error("API Error:", xhr?.responseText || error);
                if (onError) onError(xhr);
                else Swal.fire('Error', 'Something went wrong while fetching data.', 'error');
            }
        };
        if (data) ajaxOptions.data = isFormData ? data : JSON.stringify(data);
        $.ajax(ajaxOptions);
    }

    const state = {
        date: null,
        dateTo: null,
        plate: null,
        vehicleId: null,
        customerId: null,
        duration: 0,
        startTime: null,
        endTime: null,
        isSaving: false
    };
    let allVehicleOptionsCache = [];
    let isVehicleChassisSyncing = false;
    let isCustomerSource = false;


    const pad2 = (n) => n.toString().padStart(2, '0');

    function toMinutes(hhmm) {
        if (!hhmm) return NaN;
        const parts = hhmm.split(':').map(x => parseInt(x, 10) || 0);
        const h = parts[0] || 0, m = parts[1] || 0;
        return h * 60 + m;
    }

    function minutesToHHMM(total) {
        total = Math.max(0, total);
        const h = Math.floor(total / 60) % 24;
        const m = total % 60;
        return `${pad2(h)}:${pad2(m)}`;
    }

    function normalizeDurationToMinutes(rawDuration) {
        const num = parseFloat(rawDuration || 0);
        if (!isFinite(num) || num <= 0) return 0;
        return Math.round(num);
    }

    function formatDateISO(d) {
        if (!d) return '';
        const sep = d.includes('/') ? '/' : '-';
        const parts = d.split(sep);
        if (parts.length !== 3) return d;

        if (/^\d{4}$/.test(parts[0])) {
            const [y, m, day] = parts;
            return `${y}-${pad2(parseInt(m, 10))}-${pad2(parseInt(day, 10))}`;
        }

        const [day, m, y] = parts;
        return `${y}-${pad2(parseInt(m, 10))}-${pad2(parseInt(day, 10))}`;
    }

    function ensureTimeWithSeconds(hhmmOrHhmmss) {
        if (!hhmmOrHhmmss) return '';
        const p = hhmmOrHhmmss.split(':');
        if (p.length >= 3) return `${pad2(p[0])}:${pad2(p[1])}:${pad2(p[2])}`;
        if (p.length === 2) return `${pad2(p[0])}:${pad2(p[1])}:00`;
        return hhmmOrHhmmss;
    }

    const toNumber = (v) => Number.isFinite(Number(v)) ? Number(v) : null;

    function select2Options($el) {
        return {
            theme: 'bootstrap-5',
            width: '100%',
            dropdownParent: $('#scheduleModal'),
            minimumResultsForSearch: 0,
            placeholder: $el.data('placeholder') || $el.attr('placeholder') || 'Select an option',
            allowClear: true
        };
    }

    function isSelect2($el) { return $el.hasClass('select2-hidden-accessible'); }

    function initSelect2($el) {
        if (!$el.length) return;
        if (isSelect2($el)) $el.select2('destroy');
        if ($el.find('option[value=""]').length === 0) {
            $el.prepend(new Option('', ''));
        }
        $el.select2(select2Options($el));
    }

    // flatpickr instance
    let datePickrInstance = null;

    function initDatePicker() {
        ensureFlatpickrZIndexPatch();

        const modal = document.getElementById('scheduleModal');
        const input = modal ? modal.querySelector('#schDate.flat-picker-future') : document.querySelector('#schDate.flat-picker-future');
        if (!input) return;

        if (datePickrInstance && datePickrInstance.destroy) {
            datePickrInstance.destroy();
            datePickrInstance = null;
        }

        const appendTarget = modal || document.body;

        datePickrInstance = flatpickr(input, {
            dateFormat: "Y-m-d",
            allowInput: false,
            clickOpens: true,
            minDate: "today",
            maxDate: "2100-12-31",
            disableMobile: true,
            defaultDate: "today",
            appendTo: appendTarget,
            onReady: (_sel, dateStr) => {
                state.date = dateStr || $('#schDate').val() || null;
                if (!state.dateTo) state.dateTo = state.date;
                $('#schDate').trigger('change');
            },
            onChange: (_selectedDates, dateStr) => {
                state.date = dateStr || null;
                if (!state.dateTo) state.dateTo = state.date;
                $('#schDate').trigger('change');
            }
        });
    }

    function bindEvents() {
        $('#vehicleTypeDropdown').off('change').on('change', handleVehicleTypeChange);
        $('#vehicleDropdown').off('change').on('change', handleVehicleChange);
        $('#chassisDropdown').off('change').on('change', handleChassisChange);
        $('#schDate').off('change').on('change', handleDateChange);
        $('#schStart').off('change').on('change', recomputeEndTime);
        $('#schDuration').off('input change').on('input change', recomputeEndTime);
        $('#CustomerId').off('change').on('change', handleCustomerChange);
    }

    function populateChassisForCurrentVehicleType(vehicleTypeId, selectedVehicleId) {
        const $chassis = $('#chassisDropdown');
        $chassis.empty().append('<option value="">Select</option>');

        if (!vehicleTypeId || !selectedVehicleId) return;

        $.ajax({
            type: 'GET',
            url: window.RazorVars.getChassisByVehicleTypeUrl,
            data: { vehicleTypeId },
            dataType: 'json',
            success: function (data) {
                if (!Array.isArray(data)) return;

                data
                    .filter(x => Number(x.id) === Number(selectedVehicleId))
                    .forEach(item => {
                        $chassis.append(`<option value="${item.id}">${item.text}</option>`);
                    });


                // 🔑 select chassis = vehicleId
                $chassis
                    .val(String(selectedVehicleId))
                    .trigger('change.select2')
                    .trigger('change');
            }
        });
    }

    function handleVehicleChange() {
        if (isVehicleChassisSyncing) return;

        const vehicleId = $(this).val();
        if (!vehicleId) return;

        const vehicleName = $(this).find('option:selected').text();

        state.vehicleId = vehicleId;
        state.plate = vehicleName;

        isVehicleChassisSyncing = true;

        // 1️⃣ Vehicle → chassis
        callApi({
            url: `${window.API_BASE.getVehicleDefentionById}?id=${vehicleId}&lang=en`,
            onSuccess: (res) => {
                if (res?.success && res.data?.vehicle?.id) {
                    const chassisId = res.data.vehicle.id;

                    $('#chassisDropdown')
                        .val(String(chassisId))
                        .trigger('change.select2'); // ❌ NO trigger('change')
                    updateRecallChip($('#chassisDropdown').find('option:selected').text());
                    state.chassisId = Number(chassisId);
                    $('#CompanyId').val(res.data.vehicle.companyId).trigger('change');



                }

                isVehicleChassisSyncing = false;
            },
            onError: () => {
                isVehicleChassisSyncing = false;
            }
        });

        callApi({
            url: `${window.RazorVars.getOpenAgreementInfoUrl}?vehicleId=${vehicleId}`,
            onSuccess: (res) => {
                if (isCustomerSource) return;

                if (!res?.isSuccess || !Array.isArray(res.data) || !res.data.length) {
                    $('#CustomerId').val(null).trigger('change.select2');
                    return;
                }

                const customerId = res.data[0]?.customerId;
                if (customerId) {
                    $('#CustomerId')
                        .val(String(customerId))
                        .trigger('change.select2');
                }
            }
        });

    }
    function updateRecallChip(chassis) {
        if (!chassis) {
            $("#recallChip").text("Recall: -");
            return;
        }

        $.ajax({
            url: window.API_BASE.hasRecallURL,
            type: 'GET',
            data: { chassis: chassis },
            success: function (result) {
                const hasRecall = result === true || result === "true";
                $("#recallChip").text("Recall: " + (hasRecall ? "Yes" : "No"));
            },
            error: function () {
                $("#recallChip").text("Recall: -");
            }
        });
    }
    function handleChassisChange() {
        if (isVehicleChassisSyncing) return;

        const chassisId = $(this).val();
        if (!chassisId) return;

        state.chassisId = Number(chassisId);
        state.vehicleId = Number(chassisId);

        isVehicleChassisSyncing = true;

       
        
        $('#vehicleDropdown')
            .val(String(chassisId))
            .trigger('change.select2');

        // Get vehicle info first
        callApi({
            url: `${window.API_BASE.getVehicleDefentionById}?id=${chassisId}&lang=en`,
            onSuccess: (res) => {
                if (res?.success && res.data) {

                    const vehicleCustomerId = res.data.customerId;

                    // Now get agreement info
                    callApi({
                        url: `${window.RazorVars.getOpenAgreementInfoUrl}?vehicleId=${chassisId}`,
                        onSuccess: (agreementRes) => {
                            if (isCustomerSource) return;

                            // Priority: agreement customer > vehicle customer
                            let finalCustomerId = null;

                            if (agreementRes?.isSuccess && Array.isArray(agreementRes.data) && agreementRes.data.length) {
                                finalCustomerId = agreementRes.data[0]?.customerId;
                            } else if (vehicleCustomerId) {
                                finalCustomerId = vehicleCustomerId;
                            }

                            // Single customer update
                            if (finalCustomerId) {
                                $('#CustomerId').val(String(finalCustomerId)).trigger('change.select2');
                            } else {
                                $('#CustomerId').val(null).trigger('change.select2');
                            }
                        },
                        onError: () => {
                            // If agreement call fails, use vehicle customer
                            if (!isCustomerSource && vehicleCustomerId) {
                                $('#CustomerId').val(String(vehicleCustomerId)).trigger('change.select2');
                            }
                        }
                    });
                }

                isVehicleChassisSyncing = false;
            },
            onError: () => {
                isVehicleChassisSyncing = false;
            }
        });
    }
    function handleCustomerChange() {
        const customerId = $('#CustomerId').val();
        state.customerId = toNumber(customerId);

        isCustomerSource = true;

        const $vehicle = $('#vehicleDropdown');
        const $chassis = $('#chassisDropdown');

        $vehicle.empty().append('<option value="">Select</option>');
        $chassis.empty().append('<option value="">Select</option>').trigger('change.select2');

        if (!customerId) {
            isCustomerSource = false;

            allVehicleOptionsCache.forEach(o => {
                if (o.value) {
                    $vehicle.append(`<option value="${o.value}">${o.text}</option>`);
                }
            });

            $vehicle.trigger('change.select2');

            $.ajax({
                type: 'GET',
                url: window.RazorVars.getChassisByVehicleTypeUrl,
                data: { vehicleTypeId: state.vehicleType },
                dataType: 'json',
                success: function (data) {
                    $chassis.empty().append('<option value="">Select</option>');
                    if (Array.isArray(data)) {
                        data.forEach(item => {
                            $chassis.append(`<option value="${item.id}">${item.text}</option>`);
                        });
                    }
                    $chassis.trigger('change.select2');
                }
            });

            return;
        }

        callApi({
            url: `${window.RazorVars.getOpenAgreementInfoUrl}?customerId=${customerId}`,
            onSuccess: (res) => {
                if (!res?.isSuccess || !Array.isArray(res.data)) return;

                const allowedVehicleIds = new Set(
                    res.data.map(x => Number(x.vehicleDefinitionId)).filter(Boolean)
                );

                let firstVehicleId = null;

                // Populate vehicle dropdown
                allVehicleOptionsCache.forEach(o => {
                    if (allowedVehicleIds.has(Number(o.value))) {
                        $vehicle.append(`<option value="${o.value}">${o.text}</option>`);
                        if (!firstVehicleId) firstVehicleId = o.value;
                    }
                });

                $vehicle.trigger('change.select2');

                // ✅ FIX: Populate chassis dropdown with ALL customer vehicles
                if (state.vehicleType && allowedVehicleIds.size > 0) {
                    $.ajax({
                        type: 'GET',
                        url: window.RazorVars.getChassisByVehicleTypeUrl,
                        data: { vehicleTypeId: state.vehicleType },
                        dataType: 'json',
                        success: function (data) {
                            if (!Array.isArray(data)) return;

                            // ✅ Filter to show only chassis matching customer's vehicles
                            data
                                .filter(item => allowedVehicleIds.has(Number(item.id)))
                                .forEach(item => {
                                    $chassis.append(`<option value="${item.id}">${item.text}</option>`);
                                });

                            $chassis.trigger('change.select2');

                            // Set first vehicle as selected
                            if (firstVehicleId) {
                                isVehicleChassisSyncing = true;
                                $vehicle.val(firstVehicleId).trigger('change.select2');
                                $chassis.val(firstVehicleId).trigger('change.select2');
                                isVehicleChassisSyncing = false;
                            }
                        }
                    });
                }
            }
        });
    }

    function handleDateChange() {
        state.date = $(this).val() || null;
        if (!state.dateTo) state.dateTo = state.date;

        // Re-init timepicker so past times are hidden only for "today"
        // Safe: initSchStartTimepicker() no-ops if the plugin isn't loaded yet.
        initSchStartTimepicker(schStartBaseAllowedTimes, $('#schStart').val() || null);
    }

    function handleVehicleTypeChange() {
        const vehicleTypeId = $('#vehicleTypeDropdown').val();
        state.vehicleType = Number(vehicleTypeId);

        const $vehicle = $('#vehicleDropdown');
        const $chassis = $('#chassisDropdown');

        const existingCustomer = ($('#CustomerName').val() || '').toString().trim();
        allVehicleOptionsCache = [];
        $vehicle.empty().append('<option value="">Select</option>').trigger('change');
        $chassis.empty().append('<option value="">Select</option>').trigger('change');

        if (!existingCustomer) $('#CustomerName').val('');

        if (!vehicleTypeId) return;

        $.ajax({
            type: 'GET',
            url: window.RazorVars.vehicleListUrl,
            data: { VehicleTypeId: vehicleTypeId },
            dataType: 'json',
            success: function (data) {
                if (Array.isArray(data)) {
                    data.forEach(item => {
                        $vehicle.append(`<option value="${item.value}">${item.text}</option>`);
                        allVehicleOptionsCache.push({
                            value: String(item.value),
                            text: item.text
                        });
                    });
                    $vehicle.trigger('change.select2');
                }
            }
        });

        $.ajax({
            type: 'GET',
            url: window.RazorVars.getChassisByVehicleTypeUrl,
            data: { vehicleTypeId: vehicleTypeId },
            dataType: 'json',
            success: function (data) {
                if (!Array.isArray(data)) return;

                data.forEach(item => {
                    $chassis.append(`<option value="${item.id}">${item.text}</option>`);
                });

                $chassis.trigger('change.select2');
            },
            error: function () {
                Swal.fire('Error', 'Failed to load chassis.', 'error');
            }
        });
    }
    

    function recomputeEndTime() {
        const startHHMM = $('#schStart').val();
        const rawDuration = $('#schDuration').val();

        state.startTime = startHHMM || null;
        state.duration = normalizeDurationToMinutes(rawDuration);

        if (!state.startTime || !state.duration) {
            state.endTime = null;
            $('#schEnd').val('');
            return;
        }

        const startMin = toMinutes(state.startTime);
        const endMin = startMin + state.duration;
        const endHHMM = minutesToHHMM(endMin);

        state.endTime = endHHMM;
        $('#schEnd').val(endHHMM).trigger('change');
    }

    function initValidation() {
        const $form = $('#scheduleForm');
        if (!$form.length) return;

        if ($form.data('validator')) return;

        $form.validate({
            // IMPORTANT for select2: don't ignore hidden select2 originals
            ignore: ":hidden:not(.select2-hidden-accessible)",

            rules: {
                Date: { required: true },
                VehicleTypeId: { required: true },
                VehicleId: { required: true },
                CustomerId: { required: false },
                ChassisId: { required: true },
                Start_Time: { required: true },
                Duration: { required: true, min: 1 },
                End_Time: { required: true }
                // Description is OPTIONAL (no rule)
            },

            errorClass: 'is-invalid',
            errorPlacement: function (error, element) {
                error.addClass('invalid-feedback');

                // If select2, place error after its container
                if (element.hasClass('select2-hidden-accessible')) {
                    error.insertAfter(element.next('.select2'));
                    return;
                }

                if (element.parent('.input-group').length) {
                    error.insertAfter(element.parent());
                } else {
                    error.insertAfter(element);
                }
            },

            highlight: function (element) {
                const $el = $(element);
                $el.addClass('is-invalid');

                if ($el.hasClass('select2-hidden-accessible')) {
                    $el.next('.select2').find('.select2-selection').addClass('is-invalid');
                }
            },

            unhighlight: function (element) {
                const $el = $(element);
                $el.removeClass('is-invalid');

                if ($el.hasClass('select2-hidden-accessible')) {
                    $el.next('.select2').find('.select2-selection').removeClass('is-invalid');
                }
            }
        });
    }

    function bindSaveHandlerOnce() {
        $(document).off('click.save', '#btnSaveSchedule').on('click.save', '#btnSaveSchedule', function () {
            const $form = $('#scheduleForm');

            // recompute end before validation (so End_Time becomes filled)
            recomputeEndTime();

            if ($form.length && !$form.valid()) return;

            if (state.isSaving) return;
            state.isSaving = true;

            const $btn = $('#btnSaveSchedule');
            const originalText = $btn.text();
            $btn.prop('disabled', true).text(originalText || 'Saving...');

            if (!state.date) state.date = $('#schDate').val() || null;
            if (!state.dateTo) state.dateTo = state.date;

            state.startTime = $('#schStart').val() || state.startTime;
            state.duration = normalizeDurationToMinutes($('#schDuration').val());
            recomputeEndTime();

            if (!state.vehicleId) {
                const v = $('#vehicleDropdown').val();
                if (v) state.vehicleId = v;
            }
            if (!state.chassisId) {
                const c = $('#chassisDropdown').val();
                if (c) state.chassisId = Number(c);
            }

            const scheduleData = {
                Date: formatDateISO($('#schDate').val()),
                DateTo: formatDateISO($('#schDate').val()),

                VehicleTypeId: toNumber($('#vehicleTypeDropdown').val()),
                VehicleId: toNumber($('#vehicleDropdown').val()),
                ChassisId: toNumber($('#chassisDropdown').val()),
                CustomerId: toNumber($('#CustomerId').val()),

                PlateNumber: $('#vehicleDropdown').find(':selected').text()?.trim() || '',

                Start_Time: ensureTimeWithSeconds($('#schStart').val()),
                End_Time: ensureTimeWithSeconds($('#schEnd').val()),
                Duration: toNumber($('#schDuration').val()),

                Description: $('#descriptionInput').val() ?? '',
                Status: 44
            };
            debugger;


            const isEdit = $('#scheduleModal').data('mode') === 'edit';
            const reservationId = $('#scheduleModal').data('reservationId');

            const url = isEdit ? window.API_BASE.updateReservation : window.RazorVars.insertReservationUrl;
            if (isEdit) scheduleData.Id = reservationId;

            $.ajax({
                type: 'POST',
                url: url,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify(scheduleData),
                success: function (res) {
                    if (res?.isActive) {
                        Swal.fire({
                            title: window.RazorVars.warning,
                            text: window.RazorVars.reservationAlreadyExist,
                            icon: 'warning'
                        });
                        return;
                    }

                    if (res?.isSuccess) {
                        Swal.fire({
                            title: window.RazorVars.doneSuccessfully,
                            text: window.RazorVars.reservationInserted,
                            icon: 'success'
                        }).then(() => location.reload());
                        return;
                    }

                    Swal.fire('Error', 'Failed to insert reservation.', 'error');
                },
                error: function (xhr, _s, err) {
                    console.error("Insert failed:", xhr?.responseText || err);
                    Swal.fire('Error', 'Failed to insert reservation.', 'error');
                },
                complete: function () {
                    state.isSaving = false;
                    $btn.prop('disabled', false).text(originalText);
                }
            });
        });
    }

    $(document).on('shown.bs.modal', '#scheduleModal', async function () {
        bindEvents();

        // schStart: keep enabled so picker can open, but make it readonly (no manual typing)
        $('#schStart')
            .prop('disabled', false)
            .prop('readonly', true)
            .off('keydown.readonly paste.readonly')
            .on('keydown.readonly paste.readonly', (e) => e.preventDefault());

        // Duration stays editable
        $('#schDuration')
            .prop('disabled', false)
            .prop('readonly', false);

        // End time should be calculated only
        $('#schEnd').prop('readonly', true).on('keydown paste', (e) => e.preventDefault());

        // init selects
        initSelect2($('#vehicleDropdown'));
        initSelect2($('#chassisDropdown'));
        initSelect2($('#vehicleTypeDropdown'));
        initSelect2($('#CustomerId'));

        initValidation();

        try {
            await ensureFlatpickr();
            initDatePicker();
        } catch (e) {
            console.error('flatpickr failed to load', e);
        }

        // init schStart timepicker with current time as default
        try {
            await ensureJQDateTimePicker();
            ensureJQDateTimePickerZIndexPatch();

            // Get current time rounded up to next 5-minute interval as default
            const now = new Date();
            const currentMinutes = now.getHours() * 60 + now.getMinutes();
            const roundedMinutes = Math.ceil(currentMinutes / 5) * 5;
            const currentTimeDefault = minutesToHHMM(roundedMinutes);

            // Initialize with current time as default
            initSchStartTimepicker([], currentTimeDefault);
        } catch (e) {
            console.warn("jQuery DateTimePicker failed to load", e);
        }

        state.duration = normalizeDurationToMinutes($('#schDuration').val());
        recomputeEndTime();

        bindSaveHandlerOnce();
    });

    // optional cleanup when modal closes
    $(document).on('hidden.bs.modal', '#scheduleModal', function () {
        const $schStart = $("#schStart");
        try { $schStart.datetimepicker("destroy"); } catch (e) { }
    });

    $(document).ready(async function () {
        ensureFlatpickrZIndexPatch();
        ensureJQDateTimePickerZIndexPatch();

        try {
            await ensureFlatpickr();
        } catch (e) {
            console.warn('flatpickr not available yet', e);
        }

        try {
            await ensureJQDateTimePicker();
        } catch (e) {
            console.warn("jQuery DateTimePicker not available yet", e);
        }
    });
})();
