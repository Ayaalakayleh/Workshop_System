// ~/js/definitions/schedule-modal.js
(() => {
    function lockCustomerDropdown() {
        const $c = $('#CustomerId');

        $c.prop('disabled', true);

        if ($c.hasClass('select2-hidden-accessible')) {
            // force Select2 UI to refresh
            $c.trigger('change.select2');
        }
    }

    function unlockCustomerDropdown() {
        const $c = $('#CustomerId');

        $c.prop('disabled', false)
            .removeAttr('disabled');

        if ($c.hasClass('select2-hidden-accessible')) {
            $c.select2('close');
            $c.trigger('change.select2');
        }
    }

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

    function callApi({ url, type = 'GET', data = null, isFormData = false, onSuccess = null, onError = null }) {
        const ajaxOptions = {
            url, type, dataType: 'json',
            contentType: isFormData ? false : 'application/json; charset=utf-8',
            processData: !isFormData, cache: false,
            success: (response) => { if (onSuccess) onSuccess(response); },
            error: (xhr, _status, error) => {
                console.error("API Error:", xhr?.responseText || error);
                if (onError) onError(xhr);
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
        duration: 0,      // minutes (computed)
        startTime: null,  // 24h HH:MM (stored)
        endTime: null,    // 24h HH:MM (stored)
        isSaving: false
    };

    let allVehicleOptionsCache = [];
    let isVehicleChassisSyncing = false;
    let isCustomerSource = false;

    // =====================================================================================
    // ✅ TIME MODE: show 12h in UI, keep 24h internally (stable with xdsoft)
    // =====================================================================================
    const UI_USE_12H = true;
    const pad2 = (n) => n.toString().padStart(2, '0');

    function normalizeHHMM24(v) {
        if (!v) return '';
        const s = String(v).trim();
        const m = /^(\d{1,2}):(\d{2})/.exec(s); // HH:MM or HH:MM:SS
        if (!m) return '';
        const h = Number(m[1]), min = Number(m[2]);
        if (!Number.isFinite(h) || !Number.isFinite(min)) return '';
        return `${pad2(h)}:${pad2(min)}`;
    }

    function time24To12(hhmm24) {
        const t = normalizeHHMM24(hhmm24);
        if (!t) return '';

        let [h, m] = t.split(':').map(Number);

        // Convert to 12-hour format BUT ALWAYS PM
        if (h === 0) h = 12;
        else if (h > 12) h -= 12;

        return `${pad2(h)}:${pad2(m)} PM`;
    }

    function time12To24(v) {
        if (!v) return '';
        const s0 = String(v).trim();
        if (!s0) return '';

        // already 24h
        if (/^\d{1,2}:\d{2}/.test(s0) && !/[AaPp][Mm]/.test(s0)) {
            return normalizeHHMM24(s0);
        }

        const s = s0.replace(/\s+/g, ' ').toUpperCase();
        const m = /^(\d{1,2}):(\d{2})\s*(AM|PM)$/.exec(s);
        if (!m) return '';

        let h = Number(m[1]);
        const min = Number(m[2]);
        const ampm = m[3];

        if (!Number.isFinite(h) || !Number.isFinite(min) || h < 1 || h > 12 || min < 0 || min > 59) return '';

        if (h === 12) h = 0;
        // FORCE PM logic
        if (ampm === 'PM' || true) {
            if (h !== 12) h += 12;
        }

        return `${pad2(h)}:${pad2(min)}`;
    }

    function toUITime(hhmm24) {
        const t24 = normalizeHHMM24(hhmm24);
        if (!t24) return hhmm24 ? String(hhmm24) : '';
        return UI_USE_12H ? time24To12(t24) : t24;
    }

    function toMinutes24(hhmm24) {
        const t = normalizeHHMM24(hhmm24);
        if (!t) return NaN;
        const [h, m] = t.split(':').map(n => parseInt(n, 10));
        if (!Number.isFinite(h) || !Number.isFinite(m)) return NaN;
        return h * 60 + m;
    }

    function minutesToHHMM(total) {
        total = Math.max(0, total);
        const h = Math.floor(total / 60) % 24;
        const m = total % 60;
        return `${pad2(h)}:${pad2(m)}`;
    }

    // robust duration: accepts "1.5", "1,5", "HH:MM"
    function normalizeDurationToMinutes(rawDuration) {
        if (rawDuration == null) return 0;
        const s = String(rawDuration).trim();
        if (!s) return 0;

        if (s.includes(':')) {
            const [h, m] = s.split(':').map(x => parseInt(x, 10));
            if (!Number.isFinite(h) || !Number.isFinite(m)) return 0;
            const mins = h * 60 + m;
            return mins > 0 ? mins : 0;
        }

        const n = parseFloat(s.replace(',', '.'));
        if (!isFinite(n) || n <= 0) return 0;

        return Math.round(n * 60); // hours -> minutes
    }

    function ensureTimeWithSecondsFrom24(hhmm24) {
        const t = normalizeHHMM24(hhmm24);
        if (!t) return '';
        return `${t}:00`;
    }

    // === schStart: store true 24h value here (NEVER trust display string for math)
    function getSchStart24() {
        const $s = $('#schStart');
        const stored = $s.data('t24');
        if (stored) return normalizeHHMM24(stored);

        // fallback: try parse current value (may be 12h)
        const raw = ($s.val() || '').toString().trim();
        const t24 = time12To24(raw) || normalizeHHMM24(raw);
        if (t24) $s.data('t24', t24);
        return t24 || '';
    }

    function setSchStart24(t24) {
        const $s = $('#schStart');
        const norm = normalizeHHMM24(t24);
        if (!norm) return;

        $s.data('t24', norm);

        // show user 12h
        $s.val(toUITime(norm));
    }

    // =====================================================================================
    // Base allowed times (store 24h HH:MM)
    let schStartBaseAllowedTimes = [];

    // ✅ stable implementation:
    // - picker runs in 24h (format H:i)
    // - input displays 12h after close (no parser bugs, no -1 hour)
    function initSchStartTimepicker(allowedTimes = [], defaultTime24 = null) {
        const $schStart = $('#schStart');
        if (!$schStart.length) return;
        if (!($.fn && typeof $.fn.datetimepicker === 'function')) return;

        // normalize base allowed times to 24h
        schStartBaseAllowedTimes = Array.isArray(allowedTimes)
            ? allowedTimes.map(t => normalizeHHMM24(t) || time12To24(t)).filter(Boolean)
            : [];

        try { $schStart.datetimepicker('destroy'); } catch (e) { }

        $schStart.prop('disabled', false);

        const step = 5;

        // helpers
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

        function nowRoundedMinutes(stepMinutes) {
            const d = new Date();
            const nowMins = d.getHours() * 60 + d.getMinutes();
            return roundUpToStepMinutes(nowMins, stepMinutes);
        }

        function buildTimesFromMinutes24(startMinutes, stepMinutes) {
            const out = [];
            const last = 24 * 60 - stepMinutes;
            for (let m = startMinutes; m <= last; m += stepMinutes) out.push(minutesToHHMM(m));
            return out;
        }

        function computeAllowedTimesForToday24() {
            const minMins = nowRoundedMinutes(step);

            if (schStartBaseAllowedTimes.length) {
                return schStartBaseAllowedTimes.filter(t => toMinutes24(t) >= minMins);
            }

            return buildTimesFromMinutes24(minMins, step);
        }

        const isToday = isSelectedDateToday();

        // ✅ Picker must always be 24h for stability
        const opts = {
            datepicker: false,
            format: 'H:i',
            formatTime: 'H:i',
            step: step,
            scrollInput: false,
            closeOnTimeSelect: true,

            // Before showing picker, switch input temporarily to stored 24h
            onShow: function () {
                const t24 = getSchStart24();
                if (t24) $schStart.val(t24);

                // keep "today" filtering fresh
                if (isSelectedDateToday()) {
                    const times24 = computeAllowedTimesForToday24();
                    try { this.setOptions({ allowTimes: times24 }); } catch { }
                }
            },

            // After close, restore 12h display
            onClose: function () {
                const val24 = normalizeHHMM24($schStart.val()) || getSchStart24();
                if (val24) {
                    $schStart.data('t24', val24);
                    $schStart.val(toUITime(val24));
                }
            },

            onSelectTime: function (_ct, $input) {
                const picked24 = normalizeHHMM24(($input && $input.val) ? $input.val() : $schStart.val());
                if (picked24) $schStart.data('t24', picked24);

                recomputeEndTime();
                $schStart.trigger('change');
            },

            onChangeDateTime: function (_ct, $input) {
                const picked24 = normalizeHHMM24(($input && $input.val) ? $input.val() : $schStart.val());
                if (picked24) $schStart.data('t24', picked24);

                recomputeEndTime();
            }
        };

        // apply allowTimes in 24h only (stable)
        if (isToday) {
            const times24 = computeAllowedTimesForToday24();
            if (!times24.length) {
                $schStart.val('').prop('disabled', true).data('t24', '');
                recomputeEndTime();
                return;
            }
            opts.allowTimes = times24;
        } else if (schStartBaseAllowedTimes.length) {
            opts.allowTimes = schStartBaseAllowedTimes.slice();
        }

        // init picker
        $schStart.datetimepicker(opts);

        // pick initial time (24h)
        const currentStored = getSchStart24();
        let chosen24 = normalizeHHMM24(defaultTime24) || currentStored || schStartBaseAllowedTimes[0] || '08:00';

        // if today with allowTimes, ensure chosen exists
        if (isToday && Array.isArray(opts.allowTimes) && opts.allowTimes.length) {
            if (!opts.allowTimes.includes(chosen24)) chosen24 = opts.allowTimes[0];
        }

        setSchStart24(chosen24);
        recomputeEndTime();
    }
    // ================================================================================

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

    const toNumber = (v) => Number.isFinite(Number(v)) ? Number(v) : null;

    function select2Options($el) {
        return {
            theme: 'bootstrap-5',
            width: '100%',
            dropdownParent: $('#scheduleModal'),
            minimumResultsForSearch: 0,
            placeholder: $el.data('placeholder') || $el.attr('placeholder'),
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

        // ✅ duration must ALWAYS recompute
        $('#schDuration').off('input change keyup').on('input change keyup', recomputeEndTime);

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
        const vehicleType = $("#vehicleTypeDropdown").val();

        if (!vehicleId) return;

        const vehicleName = $(this).find('option:selected').text();

        state.vehicleId = vehicleId;
        state.plate = vehicleName;

        isVehicleChassisSyncing = true;

        callApi({
            url: `${window.API_BASE.getVehicleDefentionById}?id=${vehicleId}&vehicleType=${vehicleType}&lang=en`,
            onSuccess: (res) => {
                if (res?.success && res.data?.vehicle?.id) {
                    const chassisId = res.data.vehicle.id;

                    $('#chassisDropdown')
                        .val(String(chassisId))
                        .trigger('change.select2');
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
            url: `${window.RazorVars.getOpenAgreementInfoUrl}?vehicleId=${vehicleId}&VehicleTypeId=${vehicleType}`,
            onSuccess: (res) => {
                if (isCustomerSource) return;

                if (!res?.isSuccess || !res.data) {
                    $('#CustomerId').val(null).trigger('change.select2');
                    return;
                }

                const dataObj = Array.isArray(res.data) ? (res.data[0] || null) : res.data;

                if (!dataObj) {
                    $('#CustomerId').val(null).trigger('change.select2');
                    return;
                }

                const idToSet = (Number(vehicleType) === 2) ? dataObj.companyId : dataObj.customerId;

                if (!idToSet || Number(idToSet) <= 0) {
                    $('#CustomerId').val(null).trigger('change.select2');
                    unlockCustomerDropdown();
                    return;
                }

                $('#CustomerId').val(String(idToSet)).trigger('change.select2');
                lockCustomerDropdown();
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

        const vehicleType = $("#vehicleTypeDropdown").val();

        $('#vehicleDropdown')
            .val(String(chassisId))
            .trigger('change.select2');

        callApi({
            url: `${window.API_BASE.getVehicleDefentionById}?id=${chassisId}&vehicleType=${vehicleType}&lang=en`,
            onSuccess: (res) => {
                if (res?.success && res.data) {

                    const vehicleCustomerId = res.data.customerId;

                    callApi({
                        url: `${window.RazorVars.getOpenAgreementInfoUrl}?vehicleId=${chassisId}&VehicleTypeId=${vehicleType}`,
                        onSuccess: (agreementRes) => {
                            if (isCustomerSource) return;

                            let finalCustomerId = null;

                            if (agreementRes?.isSuccess && Array.isArray(agreementRes.data) && agreementRes.data.length) {
                                finalCustomerId = (Number(vehicleType) === 2)
                                    ? agreementRes.data[0]?.companyId
                                    : agreementRes.data[0]?.customerId;
                            } else if (vehicleCustomerId) {
                                finalCustomerId = vehicleCustomerId;
                            }

                            if (finalCustomerId) {
                                $('#CustomerId')
                                    .val(String(finalCustomerId))
                                    .trigger('change.select2');

                                lockCustomerDropdown();
                            } else {
                                $('#CustomerId').val(null).trigger('change.select2');
                                unlockCustomerDropdown();
                            }

                        },
                        onError: () => {
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
        const vehicleType = $("#vehicleTypeDropdown").val();

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
            unlockCustomerDropdown();
            return;
        }

        callApi({
            url: `${window.RazorVars.getOpenAgreementInfoUrl}?customerId=${customerId}&VehicleTypeId=${vehicleType}`,
            onSuccess: (res) => {
                if (!res?.isSuccess || !Array.isArray(res.data)) return;

                const allowedVehicleIds = new Set(
                    res.data.map(x => Number(x.vehicleDefinitionId)).filter(Boolean)
                );

                let firstVehicleId = null;

                allVehicleOptionsCache.forEach(o => {
                    if (allowedVehicleIds.has(Number(o.value))) {
                        $vehicle.append(`<option value="${o.value}">${o.text}</option>`);
                        if (!firstVehicleId) firstVehicleId = o.value;
                    }
                });

                $vehicle.trigger('change.select2');

                if (state.vehicleType && allowedVehicleIds.size > 0) {
                    $.ajax({
                        type: 'GET',
                        url: window.RazorVars.getChassisByVehicleTypeUrl,
                        data: { vehicleTypeId: state.vehicleType },
                        dataType: 'json',
                        success: function (data) {
                            if (!Array.isArray(data)) return;

                            data
                                .filter(item => allowedVehicleIds.has(Number(item.id)))
                                .forEach(item => {
                                    $chassis.append(`<option value="${item.id}">${item.text}</option>`);
                                });

                            $chassis.trigger('change.select2');

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

        // re-init timepicker so past times are hidden only for "today"
        initSchStartTimepicker(schStartBaseAllowedTimes, getSchStart24() || null);
    }

    function handleVehicleTypeChange() {
        const vehicleTypeId = $('#vehicleTypeDropdown').val();
        state.vehicleType = Number(vehicleTypeId);

        const $vehicle = $('#vehicleDropdown');
        const $chassis = $('#chassisDropdown');

        state.vehicleId = null;
        state.chassisId = null;
        state.plate = null;

        isCustomerSource = false;

        $('#CustomerId').val(null).trigger('change.select2');
        unlockCustomerDropdown();

        $('#CompanyId').val(null).trigger('change');

        allVehicleOptionsCache = [];
        $vehicle.empty().append('<option value="">Select</option>').trigger('change.select2');
        $chassis.empty().append('<option value="">Select</option>').trigger('change.select2');

        if (!vehicleTypeId) return;

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
            }
        });
    }

    // ✅ End time: keep 24h HH:MM (prevents "damaged" issues on type="time")
    function recomputeEndTime() {
        const start24 = getSchStart24();
        const rawDuration = $('#schDuration').val();

        state.startTime = start24 || null;
        state.duration = normalizeDurationToMinutes(rawDuration);

        if (!state.startTime || !state.duration) {
            state.endTime = null;
            $('#schEnd').val('');
            return;
        }

        const startMin = toMinutes24(state.startTime);
        const endMin = startMin + state.duration;
        const end24 = minutesToHHMM(endMin);

        state.endTime = end24;

        // keep it 24h in field
        $('#schEnd').val(end24).trigger('change');
    }

    function initValidation() {
        const $form = $('#scheduleForm');
        if (!$form.length) return;

        if ($form.data('validator')) return;

        $form.validate({
            ignore: ":hidden:not(.select2-hidden-accessible)",

            rules: {
                Date: { required: true },
                VehicleTypeId: { required: true },
                VehicleId: { required: true },
                CustomerId: { required: false },
                ChassisId: { required: true },
                Start_Time: { required: true },
                Duration: { required: true, min: 0 },
                End_Time: { required: true }
            },

            errorClass: 'is-invalid',
            errorPlacement: function (error, element) {
                error.addClass('invalid-feedback');

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

            // recompute end before validation
            recomputeEndTime();

            if ($form.length && !$form.valid()) return;

            if (state.isSaving) return;
            state.isSaving = true;

            const $btn = $('#btnSaveSchedule');
            const originalText = $btn.text();
            $btn.prop('disabled', true).text(originalText || 'Saving...');

            if (!state.date) state.date = $('#schDate').val() || null;
            if (!state.dateTo) state.dateTo = state.date;

            // always use stored 24h
            state.startTime = getSchStart24() || state.startTime;
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

                // ✅ send 24h + seconds
                Start_Time: ensureTimeWithSecondsFrom24(getSchStart24()),
                End_Time: ensureTimeWithSecondsFrom24($('#schEnd').val()),
                Duration: toNumber($('#schDuration').val()),

                Description: $('#descriptionInput').val() ?? '',
                Status: 44
            };

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

        // ✅ MUST be text because we display "hh:mm AM/PM"
        $('#schStart').attr('type', 'text');
        // schEnd can stay as-is; we write 24h so it won't break even if type="time"

        // schStart: readonly (no manual typing)
        $('#schStart')
            .prop('disabled', false)
            .prop('readonly', true)
            .off('keydown.readonly paste.readonly')
            .on('keydown.readonly paste.readonly', (e) => e.preventDefault());

        $('#schDuration')
            .prop('disabled', false)
            .prop('readonly', false);

        $('#schEnd').prop('readonly', true).on('keydown paste', (e) => e.preventDefault());

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

        try {
            await ensureJQDateTimePicker();
            ensureJQDateTimePickerZIndexPatch();

            // current time rounded up to next 5 minutes (24h)
            const now = new Date();
            const currentMinutes = now.getHours() * 60 + now.getMinutes();
            const roundedMinutes = Math.ceil(currentMinutes / 5) * 5;
            const currentTimeDefault24 = minutesToHHMM(roundedMinutes);

            // init timepicker (internal 24h + UI display 12h)
            initSchStartTimepicker([], currentTimeDefault24);
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