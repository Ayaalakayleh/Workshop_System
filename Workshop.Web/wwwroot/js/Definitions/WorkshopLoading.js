// ====== CONSTANTS / STATE ======
const SHIFT_START = "08:00";
const SHIFT_END = "17:00";
const SHIFT_MINS = 9 * 60; // 08:00 → 17:00

const technicians = [];                 // [{ id,name,workingHoursList:[{date,startTime,endTime}], reservationsList:[...] }]
const AvailableTechnicians = [];
let unscheduled = [];
let events = [];
let AvailableTechniciansByDate = [];
let currentView = "day";
let currentDate = new Date();
let CurrentDuration = Number(0);
let selectedTech = "";

// NEW: Groups state
let groupedServices = [];   // normalized groups from GetGroupedServices
let groupModal;             // bootstrap modal instance
let currentGroupId = null;  // selected WIP/group id in modal

// ====== HELPERS ======
const pad2 = n => n.toString().padStart(2, "0");
function toMinutes(hhmm) { const [h, m] = hhmm.split(":").map(Number); return h * 60 + m; }
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

function parseYMD(s) { const [y, m, d] = s.split("-").map(Number); return new Date(y, m - 1, d); }
function between(min, max, val) { return Math.max(min, Math.min(max, val)); }
function fmtHuman(hhmm) {
    let [h, m] = hhmm.split(":").map(Number);
    const ampm = h >= 12 ? "PM" : "AM";
    if (h === 0) h = 12; else if (h > 12) h -= 12;
    return `${pad2(h)}:${pad2(m)} ${ampm}`;
}

// tolerant time parser so working/reserved blocks are not dropped
const HHMMSS_to_min = (s) => {
    if (!s) return NaN;
    const m = /^(\d{1,2}):(\d{2})(?::(\d{2}))?$/.exec(String(s).trim());
    if (!m) return NaN;
    const h = Number(m[1]);
    const min = Number(m[2]);
    if (!Number.isFinite(h) || !Number.isFinite(min)) return NaN;
    return h * 60 + min;
};

const apiDateISO = (s) => s ? String(s).slice(0, 10) : null;

// ====== HEADER RENDERING ======
function renderHeaderHours() {
    const hours = document.getElementById("hoursHeader");
    if (!hours) return;
    hours.innerHTML = "";
    // 08:00 → 17:00 (9 hours, 9 columns: 08..16)
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

function setDatePicker() { const el = document.getElementById("datePicker"); if (el) el.value = ymd(currentDate); }

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

// position helper (all in minutes from midnight)
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

// merge a list of {start,end} (in minutes) into non-overlapping intervals
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

// clip interval to visible shift (08–17)
function clipIntervalToShift(interval) {
    const shiftStart = toMinutes(SHIFT_START);
    const shiftEnd = toMinutes(SHIFT_END);
    const start = Math.max(interval.start, shiftStart);
    const end = Math.min(interval.end, shiftEnd);
    if (end <= start) return null;
    return { start, end };
}

// base \ subtract (all minute intervals)
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
                // no overlap
                if (s.end <= seg.start || s.start >= seg.end) {
                    next.push(seg);
                    return;
                }
                // left piece
                if (s.start > seg.start) next.push({ start: seg.start, end: s.start });
                // right piece
                if (s.end < seg.end) next.push({ start: s.end, end: seg.end });
            });
            segments = next;
            if (!segments.length) return;
        });

        result.push(...segments);
    });

    return result.filter(x => x.end > x.start);
}

// Compute "visible" working (green) and reservations (red) for a tech/day
// - working: intervals from API (or full shift if empty)
// - reserved: reservation intervals from API
// The green bars become "working BUT NOT reserved".
// Use ONLY real working intervals. If none exist, show no green bar.
function computeVisibleWorking(working, reserved) {
    const shiftStart = toMinutes(SHIFT_START);
    const shiftEnd = toMinutes(SHIFT_END);

    // 1) Take working intervals from API and clip them to the visible shift
    let baseWorking = Array.isArray(working)
        ? working.map(clipIntervalToShift).filter(Boolean)
        : [];

    baseWorking = mergeIntervals(baseWorking);

    // 2) Take reservations and clip/merge them
    const reservedClipped = (Array.isArray(reserved) ? reserved : [])
        .map(clipIntervalToShift)
        .filter(Boolean);

    const reservedMerged = mergeIntervals(reservedClipped);

    // 3) Green = working minus reservations. If there was no working, it's simply empty.
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
            const e = HHMMSS_to_min(w.endTime);
            if (!dISO || !Number.isFinite(s) || !Number.isFinite(e) || e <= s) return;
            if (!working.has(dISO)) working.set(dISO, new Map());
            const m = working.get(dISO);
            if (!m.has(id)) m.set(id, []);
            m.get(id).push({ start: s, end: e });
        });

        rList.forEach(r => {
            const dISO = apiDateISO(r.date);
            const s = HHMMSS_to_min(r.startTime);
            const e = HHMMSS_to_min(r.endTime);
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

// Unavailable means: outside working OR collides with a reservation
function isUnavailable({ dateISO, techId, startM, endM, maps }) {
    const { working, reserved } = maps;
    const wDay = working.get(dateISO);
    const rDay = reserved.get(dateISO);

    let covered = false;
    if (wDay && wDay.get(techId)) {
        covered = wDay.get(techId).some(w => w.start <= startM && w.end >= endM);
    }
    if (!covered) return true;

    if (rDay && rDay.get(techId)) {
        const bad = rDay.get(techId).some(r => overlaps(startM, endM, r.start, r.end));
        if (bad) return true;
    }
    return false;
}

// ====== RENDER GRID ======
function renderGrid() {
    const body = document.getElementById("gridBody");
    if (!body) return;
    body.innerHTML = "";

    const dayKey = ymd(currentDate);
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

        // draw availability overlays first
        drawLaneOverlays(laneCell, t, dayKey, maps);

        // then scheduled jobs (events) so they sit above overlays
        events
            .filter(e => e.techId === t.id && e.date === dayKey)
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

    // Green: working/available periods (working minus reservations)
    visibleWorking.forEach(w => {
        const div = document.createElement("div");
        div.className = "wl-bg working";
        const pos = lanePosFromMinutes(w.start, w.end);
        div.style.left = pos.left;
        div.style.width = pos.width;
        div.title = `${tech.name} • Working ${fromMinutes(w.start)}–${fromMinutes(w.end)}`;
        lane.appendChild(div);
    });

    // Red: reserved periods
    reservedMerged.forEach(r => {
        const div = document.createElement("div");
        div.className = "wl-bg reserved";
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
    const dayKey = ymd(currentDate);
    const maps = buildIntervalMaps();

    technicians.forEach(t => {
        const id = Number(t.id);
        const assigned = events
            .filter(e => e.techId === id && e.date === dayKey)
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
    const dayKey = ymd(currentDate);
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

// ====== SCHEDULING MODAL ======
let scheduleModal;
function initModal() {
    scheduleModal = new bootstrap.Modal(document.getElementById("scheduleModal"));
    const sel = document.getElementById("schedTech");
    const date = document.getElementById("schedDate");
    const start = document.getElementById("schedStart");
    const dur = document.getElementById("schedDuration");
    const ends = document.getElementById("schedEnds");
    const overdueBadge = document.getElementById("schedStatusOverdue");

    function updateEnds() {
        const endVal = addMinutes(start.value || SHIFT_START, Number(dur.value || 0));
        ends.value = fmtHuman(endVal);
        const idx = Number(document.getElementById("jobIndex").value);
        const allowed = unscheduled[idx]?.allowed ?? 0;
        if (Number(dur.value) > allowed) {
            overdueBadge.classList.remove("d-none");
        } else {
            overdueBadge.classList.add("d-none");
        }
    }
    start.addEventListener("input", updateEnds);
    dur.addEventListener("input", updateEnds);
    ends.addEventListener("input", updateEnds);

    document.getElementById("btnSaveSchedule").addEventListener("click", async () => {
        const idx = Number(document.getElementById("jobIndex").value);
        const job = unscheduled[idx];
        if (!job || !date.value || !sel.value || !start.value || !dur.value) {
            Swal.fire({ icon: "error", title: window.i18n.label_invalid, text: window.i18n.label_fillAll });
            return;
        }

        var TechId = $('#schedTech').find(":selected").val();
        let EndTime = $('#schedEnds').val();
        EndTime = EndTime.replace('AM', '').replace('PM', '').trim();

        const WIPSCheduleObject = {
            Id: Number(job.id),
            WIPId: Number(job.wipid),
            RTSId: job.rtsid ? Number(job.rtsid) : 0,
            TechnicianId: Number(TechId),
            Date: date.value,
            StartTime: `${start.value}:00`,
            Duration: Number(dur.value),
            EndTime: EndTime,
        };
        WIPSCheduleObject.Date = convertDateFormat(WIPSCheduleObject.Date);

        const request = await SaveSchedule(WIPSCheduleObject);
        if (!request?.success) {
            Swal.fire({ icon: "error", title: window.i18n.label_invalid, text: window.i18n.label_fillAll });
            return;
        } else {
            $('#unscheduledList').html('');
            unscheduled.splice(idx, 1);
        }

        scheduleModal.hide();
        refreshAll(true);
        Swal.fire({ icon: "success", title: window.i18n.label_saved, text: window.i18n.label_savedMsg, timer: 1400, showConfirmButton: false });
    });
}

function fillTechsForLabour(data) {
    AvailableTechniciansByDate = [];
    $.each(data, function (ind, value) {
        AvailableTechniciansByDate.push({ Id: value.technicianId, name: value.primaryName, secondaryName: value.secondaryName });
    });
    const sel = document.getElementById("schedTech");
    $(sel).empty();
    if (AvailableTechniciansByDate.length > 0) {
        $.each(AvailableTechniciansByDate, function (_, t) {
            $(sel).append($('<option>', { value: t.Id, text: t.name }));
        });
    } else {
        $(sel).append($('<option>', { value: null, text: "No Available Technicians" }));
    }
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
        if (!res.success) {
            alert("Error loading Available Technicians!");
        } else {
            fillTechsForLabour(res.data);
        }
        return res;
    }).catch(function (xhr) {
        return xhr;
    });
}

// >>> UPDATED FUNCTION HERE <<<
async function openScheduleModal(jobIndex) {
    const job = unscheduled[jobIndex];
    if (!job) return;

    // if Groups modal is open, hide it first so schedule modal is on top
    if (groupModal && typeof groupModal.hide === "function") {
        try {
            groupModal.hide();
        } catch (e) {
            console.warn('[openScheduleModal] Failed to hide groupModal:', e);
        }
    }

    let Hours = fromMinutes(job.duration);
    Hours = timeToDecimalHours(Hours);
    let TodayDate = ymd(currentDate);
    await GetAvailableTechsForLabour(TodayDate, Hours);
    CurrentDuration = Hours;

    document.getElementById("jobIndex").value = jobIndex;
    document.getElementById("schedDate").value = ymd(currentDate);
    document.getElementById("schedStart").value = SHIFT_START;
    document.getElementById("schedDuration").value = job.duration;
    document.getElementById("schedEnds").value = fmtHuman(addMinutes(SHIFT_START, job.duration));
    document.getElementById("schedJobTag").textContent = `${window.i18n.label_job}: ${job.rts} — ${job.title}`;
    document.getElementById("schedAllowedTag").textContent = `${window.i18n.label_allowed}: ${job.allowed}m`;
    document.getElementById("schedStatusOverdue").classList.add("d-none");
    scheduleModal.show();
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
function wireControls() {
    document.getElementById("btnToday").addEventListener("click", () => {
        currentDate = new Date();
        document.getElementById("datePicker").value = ymd(currentDate);
        refreshAll();
    });
    document.getElementById("btnPrev").addEventListener("click", () => {
        currentDate.setDate(currentDate.getDate() - (currentView === "day" ? 1 : currentView === "week" ? 7 : 30));
        refreshAll();
    });
    document.getElementById("btnNext").addEventListener("click", () => {
        currentDate.setDate(currentDate.getDate() + (currentView === "day" ? 1 : currentView === "week" ? 7 : 30));
        refreshAll();
    });
    document.getElementById("datePicker").addEventListener("change", (e) => {
        currentDate = parseYMD(e.target.value);
        refreshAll();
    });

    document.querySelectorAll("[data-view]").forEach(btn => {
        btn.addEventListener("click", () => {
            document.querySelectorAll("[data-view]").forEach(b => b.classList.remove("active"));
            btn.classList.add("active");
            currentView = btn.dataset.view;
            setRangeLabel();
        });
    });

    // NEW: Groups button
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
    if (technicians.length > 0) technicians.splice(0, technicians.length);

    return $.ajax({
        url: window.RazorVars.getTechniciansScheduleUrl,
        type: 'GET',
        dataType: 'json',
        cache: false,
        data: { Date }
    }).then(function (res) {
        const data = Array.isArray(res) ? res : (Array.isArray(res?.data) ? res.data : []);
        const additions = data.map(item => {
            const id = item?.techId ?? item?.ID ?? item?.Id;
            const name = (item?.techName ?? item?.techSecondaryName ?? item)?.toString().trim();

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
                endTime: (w.endTime || '').toString()
            })).filter(x => x.date && x.startTime && x.endTime);

            const normR = (reservationsList || []).map(r => ({
                date: apiDateISO(r.date),
                startTime: (r.startTime || '').toString(),
                endTime: (r.endTime || '').toString(),
                durationHours: r.durationHours ?? null,
                durationMinutes: r.durationMinutes ?? null
            })).filter(x => x.date && x.startTime && x.endTime);

            return (name && id != null)
                ? { id, name, workingHoursList: normW, reservationsList: normR }
                : null;
        }).filter(Boolean);

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
        $.each(res, function (index, value) {
            unscheduled.push({
                id: value.id,
                ro: value.code,
                rts: value.code,
                duration: value.standardHours * 60,
                title: value.description,
                allowed: value.allowed,
                wipid: value.wipId,
                rtsid: value.id
            });
        });
    }).catch(function () { });
}

// fetch services but return array (do not mutate global `unscheduled`)
function FetchServicesByIdReturnArray(id, lang) {
    return $.ajax({
        url: window.RazorVars.getServicesByIdUrl,
        type: 'GET',
        dataType: 'json',
        cache: false,
        data: { id, lang }
    }).then(function (res) {
        const result = [];
        $.each(res, function (index, value) {
            result.push({
                id: value.id,
                ro: value.code,
                rts: value.code,
                duration: value.standardHours * 60,
                title: value.description,
                allowed: value.allowed,
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
        $.each(res, function (index, value) {
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
        console.log('[GetGroupedServices] RAW response:', res);

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

        console.table(groupedServices);
        return res;
    }).catch(function (xhr) {
        console.error('[GetGroupedServices] AJAX error:', xhr);
        groupedServices = [];
        return xhr;
    });
}

// Groups Modal init/show
function initGroupModal() {
    groupModal = new bootstrap.Modal(document.getElementById("groupModal"));
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
          <div class="small">${window.i18n?.label_allowed || "Allowed"}: ${job.allowed}m</div>
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

// ====== DATE FORMATTER ======
function convertDateFormat(dateStr) {
    if (typeof dateStr !== "string") return dateStr;
    const s = dateStr.trim();
    const isoMatch = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s);
    if (isoMatch) return s;
    const dmyMatch = /^(\d{2})-(\d{2})-(\d{4})$/.exec(s);
    if (dmyMatch) {
        const [, dd, MM, yyyy] = dmyMatch;
        const day = Number(dd), month = Number(MM);
        if (month >= 1 && month <= 12 && day >= 1 && day <= 31) {
            return `${yyyy}-${MM}-${dd}`;
        }
    }
    return dateStr;
}

// ====== BOOTSTRAP ======
document.addEventListener("DOMContentLoaded", async () => {
    const today = ymd(new Date());

    await Promise.all([
        GetAllTechnicians(today),
        GetServicesById(),          // baseline unscheduled
        TechnicianAvailabilty(),
    ]);

    initModal();
    initGroupModal();             // NEW
    wireControls();
    refreshAll();

    $('#btnToday').on('click', async function () {
        await GetAllTechnicians(today);
        //refreshAll();
    });
    $('#btnNext, #btnPrev').on('click', async function () {
        const dateIso = ymd(currentDate);
        await GetAllTechnicians(dateIso);
        //refreshAll();
    });
    $('#schedDate').on('change', async () => {
        let schDate = $("#schedDate").val();
        schDate = convertDateFormat(schDate);
        await GetAvailableTechsForLabour(schDate, CurrentDuration);
    });

    // Load groups so the modal is ready
    await GetGroupedServices(null);
});