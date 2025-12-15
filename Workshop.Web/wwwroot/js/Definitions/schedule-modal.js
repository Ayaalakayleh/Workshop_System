// ~/js/definitions/schedule-modal.js
(() => {
    // --- tiny loader to ensure flatpickr is available before init ---
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

    // z-index patch so calendar shows above Bootstrap modal/backdrop
    function ensureFlatpickrZIndexPatch() {
        if (document.getElementById('flatpickr-zindex-patch')) return;
        const style = document.createElement('style');
        style.id = 'flatpickr-zindex-patch';
        style.textContent = `
            .flatpickr-calendar { z-index: 200000 !important; }
        `;
        document.head.appendChild(style);
    }

    // --- helpers ---
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
        // NEW: optional dateTo for range-based APIs (e.g. GetAllReservationsFilteredJSON)
        dateTo: null,
        plate: null,
        vehicleId: null,
        customerId: null,
        duration: 0,      // minutes
        startTime: null,  // "HH:mm"
        endTime: null,    // "HH:mm"
        isSaving: false
    };

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

    // Duration is in MINUTES
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

    function refreshSelect2($el) {
        isSelect2($el) ? $el.trigger('change.select2') : initSelect2($el);
    }

    // flatpickr instance & initializer
    let datePickrInstance = null;

    function initDatePicker() {
        ensureFlatpickrZIndexPatch();
        const modal = document.getElementById('scheduleModal');
        const input = modal ? modal.querySelector('#schDate.flat-picker') : document.querySelector('#schDate.flat-picker');
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
            minDate: "1950-01-01",
            maxDate: "2100-12-31",
            disableMobile: true,
            appendTo: appendTarget,
            onChange: (_selectedDates, dateStr) => {
                state.date = dateStr || null;
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

        // NEW: optional #dateTo field (e.g. in a filter modal) to store dateTo
        $('#dateTo').off('change').on('change', function () {
            state.dateTo = $(this).val() || null;
        });
    }

    function handleVehicleChange() {
        const vehicleId = $(this).val();
        const vehicleName = $(this).find('option:selected').text();
        if (!vehicleId) return;

        state.vehicleId = vehicleId;
        state.plate = vehicleName;

        callApi({
            url: `${window.API_BASE.getVehicleDefentionById}?id=${vehicleId}&lang=en`,
            onSuccess: (res) => {
                if (res.success && res.data) {
                    const data = res.data;
                    $('#chassisDropdown').val(data.vehicle?.id).trigger('change.select2');
                    state.chassisId = Number(data.vehicle?.id);
                    // only overwrite CustomerName when API returned a non-empty value
                    if (data.customerName && String(data.customerName).trim().length) {
                        $('#CustomerName').val(data.customerName);
                    }
                    $('#CompanyId').val(data.vehicle?.companyId).trigger('change');
                } else {
                    console.warn('Failed to load vehicle details:', res.message);
                }
            },
            onError: () => Swal.fire('Error', 'Failed to load vehicle details.', 'error')
        });
    }
    function handleChassisChange() {
        const chassisId = $(this).val();
        const chassisText = $(this).find('option:selected').text();
        //debugger;
        state.chassisId = Number(chassisId);


        if (!chassisId) return;

        // sync into state
        state.vehicleId = chassisId;
        state.chassisId = Number(chassisId);
        // Call the same API used for vehicle dropdown
        callApi({
            url: `${window.API_BASE.getVehicleDefentionById}?id=${chassisId}&lang=en`,
            onSuccess: (res) => {
                if (res.success && res.data) {
                    const data = res.data;

                    $("#vehicleDropdown").val(chassisId).trigger("change.select2").trigger("change");

                    // Reflect fields
                    $('#chassisInput').val(data.vehicle?.chassisNo || '');
                    // only overwrite CustomerName when API returned a non-empty value
                    if (data.customerName && String(data.customerName).trim().length) {
                        $('#CustomerName').val(data.customerName);
                    }
                    $('#CompanyId').val(data.vehicle?.companyId).trigger('change');

                    const vehicleOptionText = $("#vehicleDropdown").find('option:selected').text();
                    state.plate = (vehicleOptionText && vehicleOptionText.trim()) ? vehicleOptionText : (data.vehicle?.plateNumber || '');
                } else {
                    console.warn("Failed to load chassis vehicle details:", res.message);
                }
            },
            onError: () => Swal.fire("Error", "Failed to load vehicle data from chassis.", "error")
        });
    }


    function handleDateChange() {
        state.date = $(this).val() || null;

        // If dateTo is not explicitly chosen anywhere, default it to same date
        if (!state.dateTo) {
            state.dateTo = state.date;
        }
    }
    function handleVehicleTypeChange() {
        const vehicleTypeId = $('#vehicleTypeDropdown').val();
        state.vehicleType = Number(vehicleTypeId);

        const $vehicle = $('#vehicleDropdown');
        const $chassis = $('#chassisDropdown');

        // retain current customer value if present
        const existingCustomer = ($('#CustomerName').val() || '').toString().trim();

        // reset
        $vehicle.empty().append('<option value="">Select</option>').trigger('change');
        $chassis.empty().append('<option value="">Select</option>').trigger('change');
        // only clear CustomerName if it was empty before — avoid overwriting a programmatically set value
        if (!existingCustomer) {
            $('#CustomerName').val('');
        }

        if (!vehicleTypeId) return;

        // 1️⃣ Load VEHICLES
        $.ajax({
            type: 'GET',
            url: window.RazorVars.vehicleListUrl,
            data: { VehicleTypeId: vehicleTypeId },
            dataType: 'json',
            success: function (data) {
                if (Array.isArray(data)) {
                    data.forEach(item => {
                        $vehicle.append(
                            `<option value="${item.value}">${item.text}</option>`
                        );
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
                    $chassis.append(
                        `<option value="${item.id}">${item.text}</option>`
                    );
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
        $('#schEnd').val(endHHMM);
    }

    function bindSaveHandlerOnce() {
        debugger;
        $(document).off('click.save', '#btnSaveSchedule').on('click.save', '#btnSaveSchedule', function () {
            const $form = $('#scheduleForm'); // or $('#scheduleModal form')

            // 🔹 First: run validation
            if ($form.length && !$form.valid()) {
                // form has validation errors -> don't submit / save
                return;
            }

            if (state.isSaving) return;
            state.isSaving = true;

            const $btn = $('#btnSaveSchedule');
            const originalText = $btn.text();
            $btn.prop('disabled', true).text(originalText || 'Saving...');

            // sync latest values from inputs
            if (!state.date) state.date = $('#schDate').val() || null;

            // Ensure dateTo is captured if there's a #dateTo input
            if (!state.dateTo) {
                const dt = $('#dateTo').val();
                state.dateTo = dt || state.date || null;
            }

            state.startTime = $('#schStart').val() || state.startTime;
            state.duration = normalizeDurationToMinutes($('#schDuration').val());
            recomputeEndTime(); // recompute end from current start/duration

            // Ensure we have latest vehicle/chassis ids in state — fallback to DOM values
            try {
                if (!state.vehicleId) {
                    const v = $('#vehicleDropdown').val();
                    if (v) state.vehicleId = v;
                }
                if (!state.chassisId) {
                    const c = $('#chassisDropdown').val();
                    if (c) state.chassisId = Number(c);
                }
            } catch (e) {
                console.warn('Failed to sync vehicle/chassis ids from DOM', e);
            }

            const scheduleData = {
                Date: formatDateISO(state.date),
                DateTo: formatDateISO(state.dateTo || state.date),
                VehicleId: toNumber(state.vehicleId),
                PlateNumber: state.plate ?? '',
                Start_Time: ensureTimeWithSeconds(state.startTime),
                End_Time: ensureTimeWithSeconds(state.endTime),
                Duration: toNumber($('#schDuration').val()),
                Description: $('#descriptionInput').val() ?? '',
                Chassis: $('#chassisDropdown option:selected').text() || null,
                Status: 44,
                CustomerName: $('#CustomerName').val() ?? '',
                VehicleTypeId: toNumber(state.vehicleType),
                ChassisId: toNumber(state.chassisId),

            };
            const isEdit = $('#scheduleModal').data('mode') === 'edit';
            const reservationId = $('#scheduleModal').data('reservationId');

            const url = isEdit
                ? window.API_BASE.updateReservation
                : window.RazorVars.insertReservationUrl;

            if (isEdit) {
                scheduleData.Id = reservationId;
            }
            $.ajax({
                type: 'POST',
                url: url,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify(scheduleData),
                success: function (res) {
                    debugger;
                    if (res?.isActive) {
                        Swal.fire({
                            title: window.RazorVars.Warning,
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

        // ✅ make sure start time & duration are enabled and editable
        $('#schStart, #schDuration')
            .prop('disabled', false)
            .prop('readonly', false);

        try {
            await ensureFlatpickr();
            initDatePicker();
        } catch (e) {
            console.error('flatpickr failed to load', e);
        }

        // End time should be calculated only
        $('#schEnd').prop('readonly', true).on('keydown paste', (e) => e.preventDefault());

        // init selects
        initSelect2($('#vehicleDropdown'));
        initSelect2($('#chassisDropdown'));
        initSelect2($('#vehicleTypeDropdown'));



        // initialize internal duration and end time once
        state.duration = normalizeDurationToMinutes($('#schDuration').val());
        recomputeEndTime();

        // 🔹 INIT VALIDATION HERE
        const $form = $('#scheduleForm'); // or $('#scheduleModal form') if you prefer
        if ($form.length && !$form.data('validator')) {
            $form.validate({
                ignore: [], // also validate hidden fields if needed
                rules: {
                    CustomerName: {
                        required: true
                    }
                },
                errorClass: 'is-invalid',
                errorPlacement: function (error, element) {
                    error.addClass('invalid-feedback');
                    if (element.parent('.input-group').length) {
                        error.insertAfter(element.parent());
                    } else {
                        error.insertAfter(element);
                    }
                },
                highlight: function (element) {
                    $(element).addClass('is-invalid')
                },
                unhighlight: function (element) {
                    $(element).removeClass('is-invalid')
                }
            });
        }

        bindSaveHandlerOnce();
    });


    // Optional preload of flatpickr if modal is already in DOM
    $(document).ready(async function () {
        ensureFlatpickrZIndexPatch();
        try {
            await ensureFlatpickr();
            if (document.querySelector('#schDate.flat-picker') && !$('#scheduleModal').hasClass('show')) {
                flatpickr('#schDate.flat-picker', {
                    dateFormat: "Y-m-d",
                    allowInput: false,
                    clickOpens: true,
                    minDate: "1950-01-01",
                    maxDate: "2100-12-31",
                    disableMobile: true
                });
            }
        } catch (e) {
            console.warn('flatpickr not available yet', e);
        }
    });
})();
