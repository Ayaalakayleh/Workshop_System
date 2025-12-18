// ====== CONSTANTS / STATE ======
const SHIFT_START = "08:00";
const SHIFT_END = "17:00";
const SHIFT_MINS = 9 * 60; // 08:00 → 17:00

// ✅ Backend saves schedules on NEXT DAY => compensate by -1.
// If backend is fixed later, set this back to 0.
const SAVE_DATE_COMPENSATION_DAYS = -1;

const technicians = []; // [{ id,name,workingHoursList:[{date,startTime,endTime}], reservationsList:[...] }]
const AvailableTechnicians = [];
let unscheduled = [];
let events = [];
let AvailableTechniciansByDate = [];
let currentView = "day";
let currentDate = new Date();
let CurrentDuration = Number(0); // hours for API
let selectedTech = "";

// NEW: Groups state
let groupedServices = [];   // normalized groups from GetGroupedServices
let groupModal;             // bootstrap modal instance
let currentGroupId = null;  // selected WIP/group id in modal

// ====== HELPERS ======
const pad2 = n => n.toString().padStart(2, "0");

// tolerant time parser: HH:MM, HH:MM:SS, HH:MM:SS.fff...
const HHMMSS_to_min = (s) => {
    if (!s) return NaN;
    const str = String(s).trim();
    const m = /^(\d{1,2}):(\d{2})(?::(\d{2})(?:\.\d{1,9})?)?$/.exec(str);
    if (!m) return NaN;
    const h = Number(m[1]);
    const min = Number(m[2]);
    if (!Number.isFinite(h) || !Number.isFinite(min)) return NaN;
    return h * 60 + min;
};

function toMinutes(hhmm) { return HHMMSS_to_min(hhmm); }
function fromMinutes(mins) { const h = Math.floor(mins / 60), m = mins % 60; return `${pad2(h)}:${pad2(m)}`; }
function addMinutes(hhmm, add) { return fromMinutes(toMinutes(hhmm) + add); }

// use local date, not UTC ISO
function ymd(d) {
    if (!(d instanceof Date)) d = new Date(d);
    const year = d.getFullYear();
    const month = pad2(d.getMonth() + 1);
    const day = pad2(d.getDate());
    return `${year}-${month}-${day}`;
}

function parseYMD(s) {
    const iso = String(s).slice(0, 10);
    const [y, m, d] = iso.split("-").map(Number);
    return new Date(y, m - 1, d);
}

function between(min, max, val) { return Math.max(min, Math.min(max, val)); }

function fmtHuman(hhmm) {
    let [h, m] = hhmm.split(":").map(Number);
    const ampm = h >= 12 ? "PM" : "AM";
    if (h === 0) h = 12; else if (h > 12) h -= 12;
    return `${pad2(h)}:${pad2(m)} ${ampm}`;
}

// normalize API date into YYYY-MM-DD
const apiDateISO = (s) => {
    if (!s) return null;
    const str = String(s);
    if (/^\d{4}-\d{2}-\d{2}/.test(str)) return str.slice(0, 10);
    try {
        const d = new Date(s);
        if (!isNaN(d.getTime())) return ymd(d);
    } catch { }
    return null;
};

// ✅ UI-selected day key (use everywhere)
function getSelectedDayISO() {
    const v = document.getElementById("datePicker")?.value;
    return v ? convertDateFormat(v) : ymd(currentDate);
}

// ✅ Date-only normalizer for save
function normalizeISODateOnly(dateStr) {
    return convertDateFormat(dateStr);
}

// ✅ Compensate day locally
function shiftISODateLocal(dateISO, days) {
    const [y, m, d] = String(dateISO).slice(0, 10).split("-").map(Number);
    const dt = new Date(y, m - 1, d);
    dt.setDate(dt.getDate() + Number(days || 0));
    return ymd(dt);
}

// ✅ After Save: refetch same day so overlays update immediately
async function refetchDayAndRender(dayISO) {
    const iso = dayISO || getSelectedDayISO();
    await GetAllTechnicians(iso);

    currentDate = parseYMD(iso);
    const dp = document.getElementById("datePicker");
    if (dp) dp.value = iso;

    refreshAll(true);
}

// ====== OFF-BY-ONE DATE FIX HELPERS ======
function isoUTC(iso) {
    const [y, m, d] = String(iso).slice(0, 10).split("-").map(Number);
    return Date.UTC(y, m - 1, d);
}
function daysBetweenISO(aISO, bISO) {
    return Math.round((isoUTC(bISO) - isoUTC(aISO)) / 86400000);
}
function shiftISO(iso, days) {
    const dt = new Date(isoUTC(iso));
    dt.setUTCDate(dt.getUTCDate() + days);
    return dt.toISOString().slice(0, 10);
}

// ====== TIMEPICKER LOADER (jQuery DateTimePicker / xdsoft) ======
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
    style.textContent = `.xdsoft_datetimepicker{ z-index: 200000 !important; }`;
    document.head.appendChild(style);
}

// ====== HEADER RENDERING ======
function renderHeaderHours() {
    const hours = document.getElementById("hoursHeader");
    if (!hours) return;
    hours.innerHTML = "";
    for (let h = 8; h < 17; h++) {
        const div = document.createElement("div");
        div.className = "wl-hour";
        div.textContent = `${pad2(h)}:00`;
        hours.appendChild(div);
    }
}

function setRangeLabel() {
    const label = document.getElementById("rangeLabel");
    const d = new Date(currentDate);
    if (!label) return;

    if (currentView === "day") {
        label.textContent = d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "2-digit" });
    } else if (currentView === "week") {
        const dow = (d.getDay() + 6) % 7;
        const start = new Date(d); start.setDate(d.getDate() - dow);
        const end = new Date(start); end.setDate(start.getDate() + 6);
        label.textContent = `${start.toLocaleDateString(undefined, { month: "short", day: "2-digit" })} – ${end.toLocaleDateString(undefined, { month: "short", day: "2-digit", year: "numeric" })}`;
    } else {
        label.textContent = d.toLocaleDateString(undefined, { month: "long", year: "numeric" });
    }
}

function setDatePicker() {
    const el = document.getElementById("datePicker");
    if (el) el.value = ymd(currentDate);
}

// ====== INTERVAL UTILS ======
function overlaps(aStart, aEnd, bStart, bEnd) { return aStart < bEnd && bStart < aEnd; }

function sumIntervalsMins(list) {
    if (!Array.isArray(list) || !list.length) return 0;
    const sorted = [...list].sort((a, b) => a.start - b.start);
    let total = 0, curS = sorted[0].start, curE = sorted[0].end;
    for (let i = 1; i < sorted.length; i++) {
        const it = sorted[i];
        if (it.start <= curE) curE = Math.max(curE, it.end);
        else { total += (curE - curS); curS = it.start; curE = it.end; }
    }
    total += (curE - curS);
    return total;
}

function lanePosFromMinutes(startM, endM) {
    const shiftStart = toMinutes(SHIFT_START);
    const shiftEnd = toMinutes(SHIFT_END);
    const total = shiftEnd - shiftStart || 1;

    const startClamped = between(shiftStart, shiftEnd, startM);
    const endClamped = between(shiftStart, shiftEnd, endM);
    const visible = Math.max(0, endClamped - startClamped);

    const leftPct = ((startClamped - shiftStart) / total) * 100;
    const widthPct = (visible / total) * 100;

    return {
        left: `${between(0, 100, leftPct)}%`,
        width: `${between(0, 100, widthPct)}%`
    };
}

function mergeIntervals(list) {
    if (!Array.isArray(list) || list.length === 0) return [];
    const arr = list
        .filter(x => typeof x?.start === "number" && typeof x?.end === "number" && x.end > x.start)
        .slice()
        .sort((a, b) => a.start - b.start);

    if (!arr.length) return [];

    const merged = [{ start: arr[0].start, end: arr[0].end }];
    for (let i = 1; i < arr.length; i++) {
        const last = merged[merged.length - 1];
        const cur = arr[i];
        if (cur.start <= last.end) {
            if (cur.end > last.end) last.end = cur.end;
        } else {
            merged.push({ start: cur.start, end: cur.end });
        }
    }
    return merged;
}

function clipIntervalToShift(interval) {
    const shiftStart = toMinutes(SHIFT_START);
    const shiftEnd = toMinutes(SHIFT_END);
    const start = Math.max(interval.start, shiftStart);
    const end = Math.min(interval.end, shiftEnd);
    if (end <= start) return null;
    return { start, end };
}

function subtractIntervals(base, subtract) {
    if (!Array.isArray(base) || !base.length) return [];
    if (!Array.isArray(subtract) || !subtract.length) return base.map(i => ({ ...i }));

    const subMerged = mergeIntervals(subtract);
    const result = [];

    base.forEach(b => {
        let segments = [{ start: b.start, end: b.end }];

        subMerged.forEach(s => {
            const next = [];
            segments.forEach(seg => {
                if (s.end <= seg.start || s.start >= seg.end) {
                    next.push(seg);
                    return;
                }
                if (s.start > seg.start) next.push({ start: seg.start, end: s.start });
                if (s.end < seg.end) next.push({ start: s.end, end: seg.end });
            });
            segments = next;
            if (!segments.length) return;
        });

        result.push(...segments);
    });

    return result.filter(x => x.end > x.start);
}

function computeVisibleWorking(working, reserved) {
    let baseWorking = Array.isArray(working)
        ? working.map(clipIntervalToShift).filter(Boolean)
        : [];

    baseWorking = mergeIntervals(baseWorking);

    const reservedClipped = (Array.isArray(reserved) ? reserved : [])
        .map(clipIntervalToShift)
        .filter(Boolean);

    const reservedMerged = mergeIntervals(reservedClipped);

    const visibleWorking = baseWorking.length
        ? subtractIntervals(baseWorking, reservedMerged)
        : [];

    return { visibleWorking, reservedMerged };
}

// ====== BUILD MAPS ======
function buildIntervalMaps() {
    const working = new Map();
    const reserved = new Map();

    technicians.forEach(t => {
        const id = Number(t.id);
        const wList = Array.isArray(t.workingHoursList) ? t.workingHoursList : [];
        const rList = Array.isArray(t.reservationsList) ? t.reservationsList : [];

        wList.forEach(w => {
            const dISO = apiDateISO(w.date);
            const s = HHMMSS_to_min(w.startTime);
            let e = HHMMSS_to_min(w.endTime);

            if (!Number.isFinite(e) && Number.isFinite(s) && Number.isFinite(w.durationMinutes)) {
                e = s + Number(w.durationMinutes);
            }

            if (!dISO || !Number.isFinite(s) || !Number.isFinite(e) || e <= s) return;
            if (!working.has(dISO)) working.set(dISO, new Map());
            const m = working.get(dISO);
            if (!m.has(id)) m.set(id, []);
            m.get(id).push({ start: s, end: e });
        });

        rList.forEach(r => {
            const dISO = apiDateISO(r.date);
            const s = HHMMSS_to_min(r.startTime);
            let e = HHMMSS_to_min(r.endTime);

            if (!Number.isFinite(e) && Number.isFinite(s) && Number.isFinite(r.durationMinutes)) {
                e = s + Number(r.durationMinutes);
            } else if (!Number.isFinite(e) && Number.isFinite(s) && Number.isFinite(r.durationHours)) {
                e = s + Math.round(Number(r.durationHours) * 60);
            }

            if (!dISO || !Number.isFinite(s) || !Number.isFinite(e) || e <= s) return;
            if (!reserved.has(dISO)) reserved.set(dISO, new Map());
            const m = reserved.get(dISO);
            if (!m.has(id)) m.set(id, []);
            m.get(id).push({ start: s, end: e });
        });
    });

    const sortMap = (m) => m.forEach(inner => inner.forEach(arr => arr.sort((a, b) => a.start - b.start)));
    sortMap(working); sortMap(reserved);
    return { working, reserved };
}

// ====== RENDER GRID ======
function renderGrid() {
    const body = document.getElementById("gridBody");
    if (!body) return;
    body.innerHTML = "";

    const dayKey = getSelectedDayISO();
    const maps = buildIntervalMaps();

    technicians.forEach(t => {
        const row = document.createElement("div");
        row.className = "wl-row";

        const nameCell = document.createElement("div");
        nameCell.className = "wl-tech";
        nameCell.textContent = t.name;

        const laneCell = document.createElement("div");
        laneCell.className = "wl-lane";
        laneCell.dataset.techId = t.id;
        if (!laneCell.style.position) laneCell.style.position = "relative";

        drawLaneOverlays(laneCell, t, dayKey, maps);

        events
            .filter(e => Number(e.techId) === Number(t.id) && e.date === dayKey)
            .forEach(e => laneCell.appendChild(makePeriodElement(e)));

        row.appendChild(nameCell);
        row.appendChild(laneCell);
        body.appendChild(row);
    });
}

function drawLaneOverlays(lane, tech, dayISO, maps) {
    const techId = Number(tech.id);
    const rawWorking = maps.working.get(dayISO)?.get(techId) || [];
    const rawReserved = maps.reserved.get(dayISO)?.get(techId) || [];

    const { visibleWorking, reservedMerged } = computeVisibleWorking(rawWorking, rawReserved);

    visibleWorking.forEach(w => {
        const div = document.createElement("div");
        div.className = "wl-bg working";
        div.style.position = "absolute";
        div.style.top = "0";
        div.style.bottom = "0";
        div.style.pointerEvents = "none";
        div.style.zIndex = "1";

        const pos = lanePosFromMinutes(w.start, w.end);
        div.style.left = pos.left;
        div.style.width = pos.width;
        div.title = `${tech.name} • Working ${fromMinutes(w.start)}–${fromMinutes(w.end)}`;
        lane.appendChild(div);
    });

    reservedMerged.forEach(r => {
        const div = document.createElement("div");
        div.className = "wl-bg reserved";
        div.style.position = "absolute";
        div.style.top = "0";
        div.style.bottom = "0";
        div.style.pointerEvents = "none";
        div.style.zIndex = "2";

        const pos = lanePosFromMinutes(r.start, r.end);
        div.style.left = pos.left;
        div.style.width = pos.width;
        div.title = `${tech.name} • Reserved ${fromMinutes(r.start)}–${fromMinutes(r.end)}`;
        lane.appendChild(div);
    });
}

function makePeriodElement(e) {
    const el = document.createElement("div");
    el.className = "wl-period";
    el.title = `${e.ro} - ${e.rts}\n${e.title}`;

    const startMin = toMinutes(e.start) - toMinutes(SHIFT_START);
    const widthPct = (e.duration / SHIFT_MINS) * 100;
    const leftPct = (startMin / SHIFT_MINS) * 100;
    el.style.left = `${between(0, 100, leftPct)}%`;
    el.style.width = `${between(0, 100, widthPct)}%`;

    el.innerHTML = `
    <div>
      <div class="fw-semibold">${e.ro} • ${e.rts} • ${e.start}–${addMinutes(e.start, e.duration)}</div>
      <span class="wl-small">${e.title} • ${e.techName ?? ""}</span>
    </div>
  `;

    el.addEventListener("click", async () => {
        const res = await Swal.fire({
            icon: "warning",
            title: window.i18n.label_deleteTitle,
            text: window.i18n.label_deleteText,
            showCancelButton: true,
            confirmButtonText: window.i18n.label_yesDelete,
            cancelButtonText: window.i18n.label_cancel
        });
        if (res.isConfirmed) {
            const idx = events.findIndex(x => x === e);
            if (idx >= 0) events.splice(idx, 1);
            refreshAll();
            Swal.fire({ icon: "success", title: window.i18n.label_deleted, text: window.i18n.label_deletedMsg, timer: 1200, showConfirmButton: false });
        }
    });

    return el;
}

// ====== RIGHT SIDEBAR ======
function renderUnscheduled() {
    const box = document.getElementById("unscheduledList");
    if (!box) return;
    box.innerHTML = "";
    if (unscheduled.length === 0) {
        box.innerHTML = `<div class="text-muted">${window.i18n.label_empty || "—"}</div>`;
        return;
    }

    unscheduled.forEach((job, i) => {
        const card = document.createElement("div");
        card.className = "d-flex align-items-center justify-content-between border rounded p-2";

        const left = document.createElement("div");
        left.innerHTML = `
      <div class="fw-semibold">${job.ro} — <span class="text-info">${job.rts}</span></div>
      <div class="small text-muted">${job.title}</div>
    `;

        const btn = document.createElement("button");
        btn.className = "btn btn-sm btn-outline-primary";
        btn.textContent = window.i18n.label_schedule;
        btn.addEventListener("click", () => openScheduleModal(i));

        card.appendChild(left);
        card.appendChild(btn);
        box.appendChild(card);
    });
}

function renderAvailability() {
    const tbody = document.getElementById("availTbody");
    if (!tbody) return;
    tbody.innerHTML = "";

    const dayKey = getSelectedDayISO();
    const maps = buildIntervalMaps();

    technicians.forEach(t => {
        const id = Number(t.id);
        const assigned = events
            .filter(e => Number(e.techId) === id && e.date === dayKey)
            .reduce((a, b) => a + b.duration, 0);

        const w = maps.working.get(dayKey)?.get(id) || [];
        const workingMins = w.length ? sumIntervalsMins(w) : SHIFT_MINS;

        const tr = document.createElement("tr");
        tr.innerHTML = `
      <td>${t.name}</td>
      <td>${SHIFT_START}–${SHIFT_END}</td>
      <td>${(workingMins / 60).toFixed(1)}</td>
      <td>${(assigned / 60).toFixed(1)}</td>
      <td class="fw-bold">${((workingMins - assigned) / 60).toFixed(1)}</td>
    `;
        tbody.appendChild(tr);
    });
}

function renderStats() {
    const dayKey = getSelectedDayISO();
    const maps = buildIntervalMaps();
    const top3 = technicians.slice(0, 3);

    const capacity = top3.reduce((sum, t) => {
        const w = maps.working.get(dayKey)?.get(Number(t.id)) || [];
        const mins = w.length ? sumIntervalsMins(w) : SHIFT_MINS;
        return sum + mins;
    }, 0);

    const scheduled = events.filter(e => e.date === dayKey).reduce((a, b) => a + b.duration, 0);
    const util = capacity ? Math.round((scheduled / capacity) * 100) : 0;

    document.getElementById("statTechs").textContent = Math.max(0, top3.length);
    document.getElementById("statCapacity").textContent = `${capacity}m`;
    document.getElementById("statScheduled").textContent = `${scheduled}m`;
    document.getElementById("statUtil").textContent = `${util}%`;
}

// ============================================================================
// ✅ SCHEDULE MODAL 2 (SEQUENTIAL ENABLE + TIMEPICKER FOR schedStart)
// ============================================================================
let scheduleModal2 = null;
let scheduleModal2Bound = false;

function getSchedule2Els() {
    const modalEl = document.getElementById("scheduleModal2");
    if (!modalEl) return null;

    const q = (sel) => modalEl.querySelector(sel);

    const els = {
        modalEl,
        jobIndex: q("#jobIndex2"),
        date: q("#schedDate"),
        tech: q("#schedTech"),
        start: q("#schedStart"),
        duration: q("#schedDuration"),
        ends: q("#schedEnds"),
        jobTag: q("#schedJobTag"),
        allowedTag: q("#schedAllowedTag"),
        overdue: q("#schedStatusOverdue"),
        saveBtn: q("#btnSaveSchedule2"),
    };

    const missing = [];
    ["jobIndex", "date", "tech", "start", "duration", "ends", "saveBtn"].forEach(k => {
        if (!els[k]) missing.push(k);
    });

    if (missing.length) {
        console.error("[schedule2] Missing elements in #scheduleModal2:", missing, els);
        return null;
    }

    return els;
}

// Select2-safe enable/disable
function setDisabledSmart(el, disabled) {
    if (!el) return;
    $(el).prop("disabled", !!disabled);
    if ($(el).hasClass("select2-hidden-accessible")) {
        $(el).trigger("change.select2");
    }
}

function schedule2SetStep(els, step) {
    setDisabledSmart(els.date, false);

    setDisabledSmart(els.tech, step < 2);
    setDisabledSmart(els.start, step < 3);
    setDisabledSmart(els.duration, step < 4);
    setDisabledSmart(els.ends, step < 5);

    els.duration.readOnly = true;
    els.ends.readOnly = true;
}

// ---- timepicker helpers ----
function minutesToHHMM(total) {
    total = Math.max(0, total);
    const h = Math.floor(total / 60) % 24;
    const m = total % 60;
    return `${pad2(h)}:${pad2(m)}`;
}

function normalizeIntervalTime(v) {
    if (!v) return null;
    const s = String(v).trim();
    // accept "HH:MM", "HH:MM:SS"
    if (/^\d{1,2}:\d{2}/.test(s)) return s.slice(0, 5);
    return null;
}

function computeAllowedStartTimes(freeIntervals, durationMin, stepMin = 5) {
    if (!Array.isArray(freeIntervals) || !durationMin) return [];

    // accept different shapes from backend
    const ranges = freeIntervals
        .map(i => {
            const s = normalizeIntervalTime(i.startFree ?? i.StartFree ?? i.start ?? i.Start);
            const e = normalizeIntervalTime(i.endFree ?? i.EndFree ?? i.end ?? i.End);
            return [toMinutes(s), toMinutes(e)];
        })
        .filter(([s, e]) => Number.isFinite(s) && Number.isFinite(e) && e > s)
        .sort((a, b) => a[0] - b[0]);

    // merge
    const merged = [];
    for (const [s, e] of ranges) {
        if (!merged.length || s > merged[merged.length - 1][1]) merged.push([s, e]);
        else merged[merged.length - 1][1] = Math.max(merged[merged.length - 1][1], e);
    }

    // enumerate possible starts
    const out = [];
    for (const [s, e] of merged) {
        for (let t = s; t + durationMin <= e; t += stepMin) out.push(minutesToHHMM(t));
    }

    // unique + sorted
    return [...new Set(out)].sort((a, b) => toMinutes(a) - toMinutes(b));
}

function destroyStartPicker($start) {
    try { $start.datetimepicker('destroy'); } catch { }
}

// ✅ UPDATED: actually applies the timepicker to #schedStart safely
function initStartTimePicker($start, allowedTimes, defaultTime, onPicked) {
    destroyStartPicker($start);

    // if plugin not loaded -> fallback (no crash)
    if (typeof $start.datetimepicker !== "function") {
        $start.val(defaultTime || SHIFT_START);
        if (typeof onPicked === "function") onPicked();
        return;
    }

    const opts = {
        datepicker: false,
        format: 'H:i',
        step: 5,
        scrollInput: false,
        closeOnTimeSelect: true,
        onSelectTime: function () {
            // trigger change so your existing step logic + ends calc runs
            $start.trigger("change");
            if (typeof onPicked === "function") onPicked();
        },
        onChangeDateTime: function () {
            $start.trigger("change");
            if (typeof onPicked === "function") onPicked();
        }
    };

    if (Array.isArray(allowedTimes) && allowedTimes.length) {
        opts.allowTimes = allowedTimes;
    }

    $start.datetimepicker(opts);
    $start.val(defaultTime || (allowedTimes?.[0]) || SHIFT_START);
}

function schedule2UpdateEnds(els) {
    const startVal = (els.start.value || "").trim();
    const durationMins = Number(els.duration.value || 0);

    if (!startVal || !durationMins) {
        els.ends.value = "";
        return;
    }

    const end24 = addMinutes(startVal, durationMins);
    els.ends.value = fmtHuman(end24);

    if (els.overdue) {
        const idx = Number(els.jobIndex.value);
        const allowed = unscheduled[idx]?.allowed ?? 0;
        if (durationMins > allowed) els.overdue.classList.remove("d-none");
        else els.overdue.classList.add("d-none");
    }
}

function initModal2() {
    const els = getSchedule2Els();
    if (!els) return;

    scheduleModal2 = bootstrap.Modal.getOrCreateInstance(els.modalEl, { backdrop: true, keyboard: true });

    if (scheduleModal2Bound) return;
    scheduleModal2Bound = true;

    // block editing ends
    $(els.ends).off(".block2").on("keydown.block2 paste.block2", (e) => e.preventDefault());

    // Date change -> fetch tech -> enable tech
    $(els.date).off(".sched2").on("change.sched2", async function () {
        schedule2SetStep(els, 1);

        els.tech.value = "";
        els.start.value = "";
        els.ends.value = "";
        if (els.overdue) els.overdue.classList.add("d-none");

        // reset tech options
        $(els.tech).empty().append(new Option("Select", ""));

        // reset timepicker
        destroyStartPicker($(els.start));

        const schDate = convertDateFormat(els.date.value);
        if (!schDate) return;

        try {
            await GetAvailableTechsForLabour(schDate, CurrentDuration);
        } catch (e) {
            console.error("[sched2] GetAvailableTechsForLabour failed:", e);
        } finally {
            schedule2SetStep(els, 2);
        }
    });

    // Tech change -> enable start + init timepicker
    $(els.tech).off(".sched2").on("change.sched2", function () {
        els.start.value = "";
        els.ends.value = "";
        if (els.overdue) els.overdue.classList.add("d-none");

        // reset timepicker each time tech changes
        destroyStartPicker($(els.start));

        if (!els.tech.value) {
            schedule2SetStep(els, 2);
            return;
        }

        schedule2SetStep(els, 3);

        // read free intervals (if backend provides)
        let freeIntervals = [];
        try {
            const raw = $(els.tech).find("option:selected").attr("data-free-intervals");
            if (raw) freeIntervals = JSON.parse(raw);
        } catch { freeIntervals = []; }

        const durationMin = Number(els.duration.value || 0);
        const allowedTimes = computeAllowedStartTimes(freeIntervals, durationMin, 5);

        // init timepicker (restricted if allowedTimes exists)
        initStartTimePicker(
            $(els.start),
            allowedTimes,
            allowedTimes[0] || SHIFT_START,
            () => schedule2UpdateEnds(els)
        );

        // IMPORTANT: don't auto-advance to next step here.
        // user must change/pick start time -> then we enable next fields.
    });

    // Start picked/changed -> enable duration+ends and compute
    $(els.start).off(".sched2").on("change.sched2 input.sched2", function () {
        if (!els.tech.value) { schedule2SetStep(els, 2); return; }
        if (!els.start.value) { schedule2SetStep(els, 3); return; }

        schedule2SetStep(els, 5);
        schedule2UpdateEnds(els);
    });

    // duration (readonly but still recompute if changed by code)
    $(els.duration).off(".sched2").on("change.sched2 input.sched2", function () {
        schedule2UpdateEnds(els);
    });

    // Save
    $(els.saveBtn).off(".sched2").on("click.sched2", async function () {
        const idx = Number(els.jobIndex.value);
        const job = unscheduled[idx];
        if (!job) {
            Swal.fire({ icon: "error", title: window.i18n.label_invalid, text: window.i18n.label_fillAll });
            return;
        }

        const dateISO = convertDateFormat(els.date.value);
        const techId = Number(els.tech.value || 0);
        const start24 = (els.start.value || "").trim();
        const durationMins = Number(els.duration.value || 0);

        if (!dateISO || !techId || !start24 || !durationMins) {
            Swal.fire({ icon: "error", title: window.i18n.label_invalid, text: window.i18n.label_fillAll });
            return;
        }

        const uiDateISO = normalizeISODateOnly(dateISO);
        const apiDateISOFixed = shiftISODateLocal(uiDateISO, SAVE_DATE_COMPENSATION_DAYS);
        const end24 = addMinutes(start24, durationMins);

        const WIPSCheduleObject = {
            Id: Number(job.id),
            WIPId: Number(job.wipid),
            RTSId: job.rtsid ? Number(job.rtsid) : 0,
            TechnicianId: techId,
            Date: apiDateISOFixed,
            StartTime: `${start24}:00`,
            Duration: durationMins,
            EndTime: `${end24}:00`,
        };

        const request = await SaveSchedule(WIPSCheduleObject);
        if (!request?.success) {
            Swal.fire({ icon: "error", title: window.i18n.label_invalid, text: window.i18n.label_fillAll });
            return;
        }

        unscheduled.splice(idx, 1);
        renderUnscheduled();

        scheduleModal2.hide();

        await refetchDayAndRender(uiDateISO);

        Swal.fire({
            icon: "success",
            title: window.i18n.label_saved,
            text: window.i18n.label_savedMsg,
            timer: 1400,
            showConfirmButton: false
        });
    });
}

// This populates the dropdown AND stores freeIntervals (if provided)
function fillTechsForLabour(data) {
    AvailableTechniciansByDate = [];

    $.each(data || [], function (_ind, value) {
        AvailableTechniciansByDate.push({
            Id: value.technicianId,
            name: value.primaryName,
            secondaryName: value.secondaryName,
            freeIntervalsList: value.freeIntervalsList
        });
    });

    const els = getSchedule2Els();
    if (!els) return;

    const sel = els.tech;
    $(sel).empty();
    $(sel).append($('<option>', { value: "", text: "Select" }));

    if (AvailableTechniciansByDate.length > 0) {
        $.each(AvailableTechniciansByDate, function (_, t) {
            const opt = $('<option>', { value: t.Id, text: t.name });
            if (t.freeIntervalsList) opt.attr("data-free-intervals", JSON.stringify(t.freeIntervalsList));
            $(sel).append(opt);
        });
    } else {
        $(sel).append($('<option>', { value: "", text: "No Available Technicians", disabled: true }));
    }

    sel.value = "";
}

async function GetAvailableTechsForLabour(RequestedDate, duration) {
    var obj = { requestedDate: RequestedDate, duration: Number(duration) };
    return $.ajax({
        url: window.RazorVars.getAvailableTechniciansUrl,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(obj)
    }).then(function (res) {
        if (!res.success) alert("Error loading Available Technicians!");
        else fillTechsForLabour(res.data);
        return res;
    }).catch(function (xhr) {
        return xhr;
    });
}

// ✅ This is called by Schedule button
async function openScheduleModal(jobIndex) {
    const job = unscheduled[jobIndex];
    if (!job) return;

    if (groupModal && typeof groupModal.hide === "function") {
        try { groupModal.hide(); } catch (e) { }
    }

    const els = getSchedule2Els();
    if (!els) {
        Swal.fire("Error", "scheduleModal2 elements not found. Check IDs.", "error");
        return;
    }

    // duration used by API (hours)
    let Hours = fromMinutes(job.duration);
    Hours = timeToDecimalHours(Hours);
    CurrentDuration = Hours;

    const selectedISO = getSelectedDayISO();

    // tags (fix undefined)
    if (els.jobTag) els.jobTag.textContent = `${window.i18n.label_job}: ${job.rts} — ${job.title}`;
    if (els.allowedTag) els.allowedTag.textContent = `${window.i18n.label_allowed}: ${(job.allowed ?? 0)}m`;
    if (els.overdue) els.overdue.classList.add("d-none");

    // set fields
    els.jobIndex.value = jobIndex;
    els.date.value = selectedISO;

    // reset downstream
    $(els.tech).empty().append(new Option("Select", ""));
    els.tech.value = "";

    // reset timepicker
    destroyStartPicker($(els.start));
    els.start.value = "";

    els.duration.value = job.duration; // readonly but present
    els.ends.value = "";

    schedule2SetStep(els, 1);

    scheduleModal2 = bootstrap.Modal.getOrCreateInstance(els.modalEl, { backdrop: true, keyboard: true });
    scheduleModal2.show();

    $(els.date).trigger("change");
}

function timeToDecimalHours(timeStr) {
    if (typeof timeStr !== 'string') return null;
    const parts = timeStr.split(':').map(Number);
    if (parts.length < 2 || parts.some(isNaN)) return null;
    const hours = parts[0] || 0;
    const minutes = parts[1] || 0;
    const seconds = parts[2] || 0;
    const totalHours = hours + (minutes / 60) + (seconds / 3600);
    return Math.round(totalHours * 100) / 100;
}

// ====== CONTROLS ======
async function gotoDate(dateObj) {
    currentDate = new Date(dateObj);
    const iso = ymd(currentDate);

    const dp = document.getElementById("datePicker");
    if (dp) dp.value = iso;

    await GetAllTechnicians(iso);
    refreshAll();
}

function stepDaysByView(view) {
    if (view === "week") return 7;
    if (view === "month") return 30;
    return 1;
}

function wireControls() {
    const btnToday = document.getElementById("btnToday");
    const btnPrev = document.getElementById("btnPrev");
    const btnNext = document.getElementById("btnNext");
    const datePicker = document.getElementById("datePicker");

    if (btnToday) btnToday.addEventListener("click", async () => gotoDate(new Date()));

    if (btnPrev) btnPrev.addEventListener("click", async () => {
        const step = stepDaysByView(currentView);
        const d = new Date(currentDate);
        d.setDate(d.getDate() - step);
        await gotoDate(d);
    });

    if (btnNext) btnNext.addEventListener("click", async () => {
        const step = stepDaysByView(currentView);
        const d = new Date(currentDate);
        d.setDate(d.getDate() + step);
        await gotoDate(d);
    });

    if (datePicker) datePicker.addEventListener("change", async (e) => {
        const picked = convertDateFormat(e.target.value);
        await gotoDate(parseYMD(picked));
    });

    document.querySelectorAll("[data-view]").forEach(btn => {
        btn.addEventListener("click", () => {
            document.querySelectorAll("[data-view]").forEach(b => b.classList.remove("active"));
            btn.classList.add("active");
            currentView = btn.dataset.view;
            setRangeLabel();
        });
    });

    const btnOpenGroups = document.getElementById("btnOpenGroups");
    if (btnOpenGroups) btnOpenGroups.addEventListener("click", openGroupModal);
}

function refreshAll(scrollTop = false) {
    setRangeLabel();
    setDatePicker();
    renderHeaderHours();
    renderGrid();
    renderUnscheduled();
    renderAvailability();
    renderStats();
    if (scrollTop) window.scrollTo({ top: 0, behavior: "smooth" });
}

// ====== API ======
function GetAllTechnicians(Date) {
    const reqISO = apiDateISO(Date) || convertDateFormat(Date);
    if (reqISO) currentDate = parseYMD(reqISO);

    if (technicians.length > 0) technicians.splice(0, technicians.length);

    return $.ajax({
        url: window.RazorVars.getTechniciansScheduleUrl,
        type: 'GET',
        dataType: 'json',
        cache: false,
        data: { Date: reqISO || Date }
    }).then(function (res) {
        const data = Array.isArray(res) ? res : (Array.isArray(res?.data) ? res.data : []);

        const additions = data.map(item => {
            const id = item?.techId ?? item?.ID ?? item?.Id;
            const name = (item?.techName ?? item?.techSecondaryName ?? "").toString().trim();

            let workingHoursList = Array.isArray(item?.workingHoursList) ? item.workingHoursList : [];
            let reservationsList = Array.isArray(item?.reservationsList) ? item.reservationsList : [];

            if ((!workingHoursList || !workingHoursList.length) && typeof item?.workingHours === 'string') {
                try { workingHoursList = JSON.parse(item.workingHours) || []; } catch { workingHoursList = []; }
            }
            if ((!reservationsList || !reservationsList.length) && typeof item?.reservations === 'string') {
                try { reservationsList = JSON.parse(item.reservations) || []; } catch { reservationsList = []; }
            }

            const normW = (workingHoursList || []).map(w => ({
                date: apiDateISO(w.date),
                startTime: (w.startTime || '').toString(),
                endTime: (w.endTime || '').toString(),
                durationMinutes: w.durationMinutes ?? null
            })).filter(x => x.date && x.startTime && (x.endTime || Number.isFinite(x.durationMinutes)));

            const normR = (reservationsList || []).map(r => ({
                date: apiDateISO(r.date),
                startTime: (r.startTime || '').toString(),
                endTime: (r.endTime || '').toString(),
                durationHours: r.durationHours ?? null,
                durationMinutes: r.durationMinutes ?? null
            })).filter(x => x.date && x.startTime && (x.endTime || Number.isFinite(x.durationMinutes) || Number.isFinite(x.durationHours)));

            return (name && id != null)
                ? { id, name, workingHoursList: normW, reservationsList: normR }
                : null;
        }).filter(Boolean);

        // ✅ only apply off-by-one shift when response is uniformly off by 1 day
        if (reqISO) {
            const allDates = [];
            additions.forEach(t => {
                t.workingHoursList.forEach(x => x?.date && allDates.push(x.date));
                t.reservationsList.forEach(x => x?.date && allDates.push(x.date));
            });

            const uniqDates = [...new Set(allDates)];
            const hasReq = uniqDates.includes(reqISO);

            if (uniqDates.length === 1 && !hasReq) {
                const only = uniqDates[0];
                const diff = daysBetweenISO(only, reqISO);
                if (Math.abs(diff) === 1) {
                    additions.forEach(t => {
                        t.workingHoursList.forEach(x => x.date = shiftISO(x.date, diff));
                        t.reservationsList.forEach(x => x.date = shiftISO(x.date, diff));
                    });
                }
            }
        }

        if (additions.length) technicians.push(...additions);
    }).catch(function (xhr) {
        alert('Error occurred while loading technicians.');
        throw xhr;
    });
}

function GetServicesById(id, lang) {
    return $.ajax({
        url: window.RazorVars.getServicesByIdUrl,
        type: 'GET',
        dataType: 'json',
        cache: false,
        data: { id, lang }
    }).then(function (res) {
        $.each(res, function (_index, value) {
            unscheduled.push({
                id: value.id,
                ro: value.code,
                rts: value.code,
                duration: value.standardHours * 60,
                title: value.description,
                allowed: value.standardHours,
                wipid: value.wipId,
                rtsid: value.id
            });
        });
    }).catch(function () { });
}

function FetchServicesByIdReturnArray(id, lang) {
    return $.ajax({
        url: window.RazorVars.getServicesByIdUrl,
        type: 'GET',
        dataType: 'json',
        cache: false,
        data: { id, lang }
    }).then(function (res) {
        const result = [];
        $.each(res, function (_index, value) {
            result.push({
                id: value.id,
                ro: value.code,
                rts: value.code,
                duration: value.standardHours * 60,
                title: value.description,
                allowed: value.standardHours,
                wipid: value.wipId,
                rtsid: value.id
            });
        });
        return result;
    }).catch(function () { return []; });
}

function SaveSchedule(WIPSCheduleObject) {
    return $.ajax({
        url: window.RazorVars.wipScheduleUrl,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(WIPSCheduleObject),
        headers: { 'Accept': 'application/json' }
    }).then(function (res) {
        return res;
    }).catch(function (xhr) {
        return xhr;
    });
}

function TechnicianAvailabilty() {
    return $.ajax({
        url: window.RazorVars.getTechnicianAvailabilityUrl,
        type: 'GET',
        dataType: 'json',
        cache: false,
    }).then(function (res) {
        $.each(res, function (_index, value) {
            AvailableTechnicians.push({
                techId: value.techId,
                code: value.TechName,
                techName: value.techName,
                techSecondaryName: value.techSecondaryName,
                startWorkingTime: value.startWorkingTime,
                endWorkingTime: value.endWorkingTime,
                workingHours: value.workingHours,
                assigned: value.assigned,
                available: value.available
            });
        });
    }).catch(function (xhr) {
        console.error(xhr.responseText || xhr);
        alert('Error occurred while loading Technicians Availabilty.');
        throw xhr;
    });
}

// ====== NEW: Groups flow ======
function GetGroupedServices(Id) {
    return $.ajax({
        url: window.RazorVars.getGroupedServicesUrl,
        type: 'GET',
        dataType: 'json',
        data: { Id: Id },
    }).then(function (res) {
        if (!res?.success || !Array.isArray(res.data)) {
            groupedServices = [];
            return res;
        }

        groupedServices = res.data.map(g => {
            let itemsCount = 0;
            try {
                const parsed = typeof g.items === 'string' ? JSON.parse(g.items) : g.items;
                itemsCount = Array.isArray(parsed) ? parsed.length : 0;
            } catch { itemsCount = 0; }
            return {
                wipId: g.wipId ?? g.WIPId ?? g.Id ?? g.id,
                totalItemsPerWIPId: g.totalItemsPerWIPId ?? itemsCount,
                itemsCount
            };
        });

        return res;
    }).catch(function (xhr) {
        console.error('[GetGroupedServices] AJAX error:', xhr);
        groupedServices = [];
        return xhr;
    });
}

function initGroupModal() {
    const el = document.getElementById("groupModal");
    if (!el) return;
    groupModal = bootstrap.Modal.getOrCreateInstance(el, { backdrop: true, keyboard: true });
}

function openGroupModal() {
    renderGroupList();
    clearGroupItems();
    groupModal.show();
}

function clearGroupItems() {
    currentGroupId = null;
    document.getElementById("groupItemsSection").classList.add("d-none");
    document.getElementById("groupItems").innerHTML = "";
    document.getElementById("groupItemsHeader").textContent = window.i18n.label_items || "Items";
    document.getElementById("groupItemsMeta").textContent = "";
}

function renderGroupList() {
    const list = document.getElementById("groupList");
    const count = document.getElementById("groupCount");
    list.innerHTML = "";

    if (!Array.isArray(groupedServices) || groupedServices.length === 0) {
        count.textContent = "0";
        list.innerHTML = `<div class="text-muted small">${window.i18n.label_none || "No groups found."}</div>`;
        return;
    }

    count.textContent = `${groupedServices.length}`;

    groupedServices.forEach(g => {
        const wipId = g.wipId;
        const itemsCount = g.itemsCount ?? g.totalItemsPerWIPId ?? 0;

        const row = document.createElement("div");
        row.className = "list-group-item d-flex align-items-center justify-content-between";

        const left = document.createElement("div");
        left.innerHTML = `
      <div class="fw-semibold">WIP #${wipId}</div>
      <div class="small text-muted">${window.i18n.label_items || "Items"}: ${itemsCount}</div>
    `;

        const openBtn = document.createElement("button");
        openBtn.className = "btn btn-sm btn-primary";
        openBtn.textContent = window.i18n.label_open || "Open";
        openBtn.addEventListener("click", () => {
            loadGroupItems(wipId);
        });

        row.appendChild(left);
        row.appendChild(openBtn);
        list.appendChild(row);
    });
}

async function loadGroupItems(wipId) {
    currentGroupId = wipId;
    const jobs = await FetchServicesByIdReturnArray(wipId, null);
    renderJobsInGroupModal(jobs);
}

function renderJobsInGroupModal(jobs) {
    const wrap = document.getElementById("groupItems");
    const header = document.getElementById("groupItemsHeader");
    const meta = document.getElementById("groupItemsMeta");

    wrap.innerHTML = "";

    header.textContent = `WIP #${currentGroupId}`;
    meta.textContent = `${Array.isArray(jobs) ? jobs.length : 0} ${window.i18n.label_items || "items"}`;

    if (!Array.isArray(jobs) || jobs.length === 0) {
        wrap.innerHTML = `<div class="text-muted small">${window.i18n.label_none || "No unscheduled labour found for this group."}</div>`;
        document.getElementById("groupItemsSection").classList.remove("d-none");
        return;
    }

    jobs.forEach((job) => {
        const card = document.createElement("div");
        card.className = "d-flex align-items-center justify-content-between border rounded p-2 mb-2";

        const left = document.createElement("div");
        left.innerHTML = `
      <div class="fw-semibold">${job.ro} — <span class="text-info">${job.rts}</span></div>
      <div class="small text-muted">${job.title}</div>
      <div class="small">${window.i18n?.label_allowed || "Allowed"}: ${(job.allowed ?? 0)}h</div>
    `;
  
        const btns = document.createElement("div");
        btns.className = "d-flex gap-2";

        const scheduleBtn = document.createElement("button");
        scheduleBtn.className = "btn btn-sm btn-outline-primary";
        scheduleBtn.textContent = window.i18n?.label_schedule || "Schedule";
        scheduleBtn.addEventListener("click", () => {
            const idx = unscheduled.push(job) - 1;
            openScheduleModal(idx);
        });

        btns.appendChild(scheduleBtn);
        card.appendChild(left);
        card.appendChild(btns);
        wrap.appendChild(card);
    });

    document.getElementById("groupItemsSection").classList.remove("d-none");
}

// ====== DATE FORMATTER (supports dd/MM/yyyy too) ======
function convertDateFormat(dateStr) {
    if (typeof dateStr !== "string") return dateStr;
    const s = dateStr.trim();

    // already ISO
    const isoMatch = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s);
    if (isoMatch) return s;

    // dd-MM-yyyy
    const dmyDash = /^(\d{2})-(\d{2})-(\d{4})$/.exec(s);
    if (dmyDash) {
        const [, dd, MM, yyyy] = dmyDash;
        const day = Number(dd), month = Number(MM);
        if (month >= 1 && month <= 12 && day >= 1 && day <= 31) return `${yyyy}-${MM}-${dd}`;
    }

    // dd/MM/yyyy
    const dmySlash = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(s);
    if (dmySlash) {
        const [, dd, MM, yyyy] = dmySlash;
        const day = Number(dd), month = Number(MM);
        if (month >= 1 && month <= 12 && day >= 1 && day <= 31) return `${yyyy}-${MM}-${dd}`;
    }

    return dateStr;
}

// ====== BOOTSTRAP ======
document.addEventListener("DOMContentLoaded", async () => {
    // ✅ make sure timepicker is available + above modals
    ensureJQDateTimePickerZIndexPatch();
    try { await ensureJQDateTimePicker(); } catch (e) { console.warn("[sched2] timepicker failed to load", e); }

    const today = ymd(new Date());

    await Promise.all([
        GetAllTechnicians(today),
        GetServicesById(),
        TechnicianAvailabilty(),
    ]);

    initModal2();       // ✅ scheduleModal2 with timepicker for schedStart
    initGroupModal();
    wireControls();
    refreshAll();

    await GetGroupedServices(null);
});
