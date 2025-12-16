// schedule-modal-2.js
(() => {
    const modalSel = '#scheduleModal2';

    // --------- helpers ----------
    const pad2 = (n) => String(n).padStart(2, '0');

    function toMinutes(hhmm) {
        if (!hhmm) return NaN;
        const [h, m] = hhmm.split(':').map(x => parseInt(x, 10) || 0);
        return h * 60 + m;
    }

    function minutesToHHMM(total) {
        total = Math.max(0, total);
        const h = Math.floor(total / 60) % 24;
        const m = total % 60;
        return `${pad2(h)}:${pad2(m)}`;
    }

    function normalizeDurationToMinutes(v) {
        const num = parseFloat(v || 0);
        if (!isFinite(num) || num <= 0) return 0;
        return Math.round(num);
    }

    function computeStartOptionsEnumerate(freeIntervals, durationMin, stepMin = 5) {
        if (!Array.isArray(freeIntervals) || durationMin <= 0) return [];

        const ranges = freeIntervals
            .map(i => [toMinutes(i.startFree), toMinutes(i.endFree)])
            .filter(([s, e]) => Number.isFinite(s) && Number.isFinite(e) && e > s)
            .sort((a, b) => a[0] - b[0]);

        const merged = [];
        for (const [s, e] of ranges) {
            if (!merged.length || s > merged[merged.length - 1][1]) merged.push([s, e]);
            else merged[merged.length - 1][1] = Math.max(merged[merged.length - 1][1], e);
        }

        const out = [];
        for (const [s, e] of merged) {
            for (let t = s; t + durationMin <= e; t += stepMin) out.push(minutesToHHMM(t));
        }

        return [...new Set(out)].sort((a, b) => toMinutes(a) - toMinutes(b));
    }

    function getEls() {
        const $m = $(modalSel);
        return {
            $m,
            $date: $m.find('#schedDate'),
            $tech: $m.find('#schedTech'),
            $start: $m.find('#schedStart'),
            $dur: $m.find('#schedDuration'),
            $ends: $m.find('#schedEnds'),
            $jobTag: $m.find('#schedJobTag'),
            $allowedTag: $m.find('#schedAllowedTag')
        };
    }

    function setEnabled($el, enabled) {
        $el.prop('disabled', !enabled);
    }

    function recomputeEnds($start, $dur, $ends) {
        const startHHMM = ($start.val() || '').trim();
        const durationMin = normalizeDurationToMinutes($dur.val());

        if (!startHHMM || !durationMin) {
            $ends.val('');
            return;
        }
        $ends.val(minutesToHHMM(toMinutes(startHHMM) + durationMin));
    }

    function initStartTimepicker($start, allowedTimes, defaultTime, onChange) {
        // destroy previous instance
        try { $start.datetimepicker('destroy'); } catch (e) { }

        // fallback if plugin missing
        if (typeof $start.datetimepicker !== 'function') {
            $start.val(defaultTime || allowedTimes?.[0] || '08:00');
            onChange?.();
            return;
        }

        const opts = {
            datepicker: false,
            format: 'H:i',
            step: 5,
            scrollInput: false,
            onSelectTime: onChange,
            onChangeDateTime: onChange
        };

        if (Array.isArray(allowedTimes) && allowedTimes.length) {
            opts.allowTimes = allowedTimes;
        }

        $start.datetimepicker(opts);
        $start.val(defaultTime || allowedTimes?.[0] || '08:00');
        onChange?.();
    }

    function resetLockStepUI() {
        const { $date, $tech, $start, $dur, $ends } = getEls();

        // only date enabled
        setEnabled($date, true);
        setEnabled($tech, false);
        setEnabled($start, false);
        setEnabled($dur, false);
        setEnabled($ends, false);

        // reset values
        $tech.empty().append(new Option('Select', ''));
        $start.val('');
        $ends.val('');

        // ends readonly always
        $ends.prop('readonly', true).off('.block').on('keydown.block paste.block', (e) => e.preventDefault());
    }

    function loadTechsByDate() {
        const { $date, $tech, $start, $dur, $ends } = getEls();

        const date = ($date.val() || '').trim();

        // reset downstream
        setEnabled($tech, false);
        setEnabled($start, false);
        setEnabled($dur, false);
        setEnabled($ends, false);

        $tech.empty().append(new Option('Select', ''));
        $start.val('');
        $ends.val('');

        if (!date) return;

        const durationMin = normalizeDurationToMinutes($dur.val());
        const durationHours = durationMin ? (durationMin / 60) : 0;

        $.ajax({
            type: 'GET',
            url: window.URL.getAvailableTechnicians +
                `?date=${encodeURIComponent(date)}&duration=${encodeURIComponent(durationHours)}`,
            dataType: 'json',
            success: function (result) {
                const list = result?.data ?? result ?? [];

                $tech.empty().append(new Option('Select', ''));

                (Array.isArray(list) ? list : []).forEach(item => {
                    const opt = new Option(item.text, item.value);
                    if (item.freeIntervalsList) {
                        $(opt).attr('data-free-intervals', JSON.stringify(item.freeIntervalsList));
                    }
                    $tech.append(opt);
                });

                setEnabled($tech, true); // enable next step
            },
            error: function (xhr, _s, err) {
                console.error("getAvailableTechnicians failed:", xhr?.responseText || err);
                // enable tech anyway so UI doesn't get stuck
                setEnabled($tech, true);
            }
        });
    }

    function handleTechChange() {
        const { $tech, $start, $dur, $ends } = getEls();

        const $opt = $tech.find('option:selected');
        const techId = ($opt.val() || '').trim();

        // reset downstream
        setEnabled($start, false);
        setEnabled($dur, false);
        setEnabled($ends, false);

        $start.val('');
        $ends.val('');

        if (!techId) return;

        let freeIntervals = [];
        try {
            freeIntervals = JSON.parse($opt.attr('data-free-intervals') || '[]');
        } catch {
            freeIntervals = [];
        }

        const durationMin = normalizeDurationToMinutes($dur.val());
        const allowedStarts = computeStartOptionsEnumerate(freeIntervals, durationMin, 5);

        // enable start no matter what
        setEnabled($start, true);

        // init picker with restrictions if available
        initStartTimepicker(
            $start,
            allowedStarts,
            allowedStarts[0] || '08:00',
            () => recomputeEnds($start, $dur, $ends)
        );
    }

    function handleStartChange() {
        const { $start, $dur, $ends } = getEls();

        recomputeEnds($start, $dur, $ends);

        if (($start.val() || '').trim()) {
            setEnabled($dur, true);   // still readonly
            setEnabled($ends, true);  // readonly
        } else {
            setEnabled($dur, false);
            setEnabled($ends, false);
        }
    }

    // --------- public opener (call this from anywhere) ----------
    // openScheduleModal2({ durationMin: 60, jobText: "Job: X", allowedMin: 90, jobIndex: 2 })
    window.openScheduleModal2 = function ({ durationMin, jobText, allowedMin, jobIndex } = {}) {
        const el = document.querySelector(modalSel);
        if (!el) {
            console.error('Modal not found:', modalSel);
            return;
        }

        const { $m, $date, $dur, $jobTag, $allowedTag } = getEls();

        if (Number.isFinite(Number(jobIndex))) $m.find('#jobIndex2').val(jobIndex);

        const dur = Number.isFinite(Number(durationMin)) ? Number(durationMin) : 30;
        $dur.val(dur);

        $jobTag.text(jobText || '');

        // FIX #1: undefined -> 0
        const allowed = Number.isFinite(Number(allowedMin)) ? Number(allowedMin) : 0;
        $allowedTag.text(`Allowed: ${allowed}m`);

        resetLockStepUI();

        if (!$date.val()) {
            $date.val(new Date().toISOString().slice(0, 10));
        }

        // show modal safely (NO backdrop error)
        bootstrap.Modal.getOrCreateInstance(el).show();

        // trigger API load -> enables technician
        $date.trigger('change');
    };

    // --------- bind events when modal opens ----------
    $(document).on('shown.bs.modal', modalSel, function () {
        const { $date, $tech, $start, $dur } = getEls();

        resetLockStepUI();

        // default duration if not set from opener
        if (!$dur.val()) $dur.val(30);

        // default date today
        if (!$date.val()) $date.val(new Date().toISOString().slice(0, 10));

        // bind (no duplicates)
        $date.off('.sched2').on('change.sched2', loadTechsByDate);
        $tech.off('.sched2').on('change.sched2', handleTechChange);
        $start.off('.sched2').on('change.sched2 input.sched2', handleStartChange);
        $dur.off('.sched2').on('change.sched2 input.sched2', handleStartChange);

        // kick flow
        $date.trigger('change');
    });

    // save click (simple required check)
    $(document).off('click.sched2', '#btnSaveSchedule2').on('click.sched2', '#btnSaveSchedule2', function () {
        const { $date, $tech, $start, $dur, $ends } = getEls();

        const date = ($date.val() || '').trim();
        const techId = ($tech.val() || '').trim();
        const start = ($start.val() || '').trim();
        const duration = normalizeDurationToMinutes($dur.val());
        const ends = ($ends.val() || '').trim();

        if (!date || !techId || !start || !duration || !ends) {
            alert('Please fill required fields');
            return;
        }

        // TODO: post your payload here

        const el = document.querySelector(modalSel);
        bootstrap.Modal.getInstance(el)?.hide();
    });

})();
