// ~/js/definitions/reservations.js
(function () {
    // tiny DOM helpers that DO NOT override jQuery
    const qs = (s, r = document) => r.querySelector(s);
    const qsa = (s, r = document) => Array.from(r.querySelectorAll(s));

    // -------- Data from server (reservations only) --------
    let RESERVATIONS = [];

    // Page calls this with response.data
    window.updateTechSchedule = function (dataArray) {
        RESERVATIONS = Array.isArray(dataArray) ? dataArray : [];
        build();
    };

    // ---------- i18n ----------
    const i18nEl = qs("#i18n");
    const i18n = i18nEl ? JSON.parse(i18nEl.textContent || "{}") : {};
    const labelReserved = i18n.label_reserved || "Reserved";

    // ---------- state ----------
    const state = {
        view: "Day"   // "Day" | "Week" | "Month"
    };

    // ---------- elements ----------
    const thead = qs(".resv-grid thead");
    const elBody = qs("#resvBody");
    const elDate = qs("#dcDate");
    const elStart = qs("#slotStart");
    const elEnd = qs("#slotEnd");
    const btnClear = qs("#btnClear");
    const viewTitle = qs("#viewTitle");
    const viewDay = qs("#viewDay");
    const viewWeek = qs("#viewWeek");
    const viewMonth = qs("#viewMonth");

    const pad = (n) => (n < 10 ? "0" + n : "" + n);
    const DOW = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];

    // Default day bounds (will auto-expand to include reservations)
    const DAY_START_MIN = 8 * 60;   // 480
    const DAY_END_MIN = 16 * 60;    // 960

    // ---------- generic helpers ----------
    function parseDateInput(val) {
        if (!val || typeof val !== "string") return new Date();
        const s = val.trim().replace(/\//g, "-");
        let d;
        if (/^\d{4}-\d{2}-\d{2}$/.test(s)) {
            d = new Date(s + "T00:00:00");
        } else if (/^\d{2}-\d{2}-\d{4}$/.test(s)) {
            const [dd, mm, yyyy] = s.split("-");
            d = new Date(`${yyyy}-${mm}-${dd}T00:00:00`);
        } else {
            d = new Date(s);
        }
        return isNaN(d) ? new Date() : d;
    }

    const toISODate = (d) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;

    function dateISOfromApi(s) {
        if (!s) return null;
        return String(s).slice(0, 10);
    }

    function toMinutesApi(hhmmss) {
        if (!hhmmss) return NaN;
        const m = String(hhmmss).match(/^(\d{2}):(\d{2})(?::\d{2})?$/);
        if (!m) return NaN;
        return parseInt(m[1], 10) * 60 + parseInt(m[2], 10);
    }

    function formatHourLabel(mins) {
        const h24 = Math.floor(mins / 60);
        const ampm = h24 >= 12 ? "PM" : "AM";
        let h12 = h24 % 12;
        if (h12 === 0) h12 = 12;
        return `${h12} ${ampm}`;
    }

    function dayAbbr(d) {
        return DOW[d.getDay()];
    }

    function sameDate(a, b) {
        return a.getFullYear() === b.getFullYear() &&
            a.getMonth() === b.getMonth() &&
            a.getDate() === b.getDate();
    }

    function getStartOfWeek(date, weekStart = 6) { // 6 => Saturday
        const d = new Date(date);
        const day = d.getDay();
        const diff = (day - weekStart + 7) % 7;
        d.setDate(d.getDate() - diff);
        return d;
    }

    function timesBetween(start, end, step) {
        const arr = [];
        if (!Number.isFinite(start) || !Number.isFinite(end) || !Number.isFinite(step) || step <= 0) return arr;
        for (let t = start; t < end; t += step) arr.push(t);
        return arr;
    }

    // ✅ FIX: don't wrap <tr> inside another <tr> (this breaks Day header selection in some DOMs)
    function setThead(rowsHtml) {
        if (!thead) return;
        thead.innerHTML = rowsHtml.join("");
    }

    function ensureDate() {
        if (!elDate) return;
        if (!elDate.value) {
            const t = new Date();
            elDate.value = `${t.getFullYear()}-${pad(t.getMonth() + 1)}-${pad(t.getDate())}`;
        }
    }

    function setViewTitle() {
        if (!viewTitle) return;
        const d = parseDateInput(elDate?.value || "");
        if (state.view === "Day") {
            viewTitle.textContent = d.toLocaleDateString(undefined, {
                year: "numeric",
                month: "long",
                day: "numeric"
            });
        } else if (state.view === "Week") {
            const s = getStartOfWeek(d, 6); // Saturday
            const e = new Date(s);
            e.setDate(s.getDate() + 6);
            const sTxt = s.toLocaleDateString(undefined, { month: "long", day: "numeric" });
            const eTxt = e.toLocaleDateString(undefined, { month: "long", day: "numeric", year: "numeric" });
            viewTitle.textContent = `${sTxt} – ${eTxt}`;
        } else {
            viewTitle.textContent = d.toLocaleDateString(undefined, { year: "numeric", month: "long" });
        }
    }

    function getMinMaxFromReservations(list) {
        if (!list || list.length === 0) return null;
        let min = Infinity;
        let max = -Infinity;
        list.forEach(r => {
            const s = toMinutesApi(r.start_Time || r.startTime);
            const e = toMinutesApi(r.end_Time || r.endTime);
            if (Number.isFinite(s) && s < min) min = s;
            if (Number.isFinite(e) && e > max) max = e;
        });
        if (!Number.isFinite(min) || !Number.isFinite(max) || max <= min) return null;
        return { minStart: min, maxEnd: max };
    }

    function clamp(v, a, b) {
        return Math.max(a, Math.min(b, v));
    }

    const floorToHour = (m) => Math.floor(m / 60) * 60;
    const ceilToHour = (m) => Math.ceil(m / 60) * 60;

    // Map dateISO -> [{ id, start, end, description, plate }]
    function buildIntervalsMap() {
        const map = new Map();
        RESERVATIONS.forEach(r => {
            const dISO = dateISOfromApi(r.date);
            if (!dISO) return;
            const s = toMinutesApi(r.start_Time || r.startTime);
            const e = toMinutesApi(r.end_Time || r.endTime);
            if (!Number.isFinite(s) || !Number.isFinite(e) || e <= s) return;
            if (!map.has(dISO)) map.set(dISO, []);
            map.get(dISO).push({
                id: r.id,
                start: s,
                end: e,
                description: r.description || "",
                plate: r.plate_Number || r.plateNumber || ""
            });
        });

        map.forEach(list => list.sort((a, b) => a.start - b.start));
        return map;
    }

    // ---------- modal helpers ----------
    function formatDateDisplay(dateStr) {
        if (!dateStr) return "";
        const d = new Date(dateStr);
        if (isNaN(d)) return dateStr;
        return d.toLocaleDateString();
    }

    function formatTimeDisplay(time) {
        if (!time) return "";
        const str = String(time);
        const parts = str.split(':');
        if (parts.length >= 2) {
            return `${parts[0].padStart(2, '0')}:${parts[1].padStart(2, '0')}`;
        }
        return str;
    }

    function openReservationDetailsModal(idsArray) {
        if (!window.$ || !$.ajax) {
            console.error("jQuery is not available for openReservationDetailsModal");
            return;
        }

        if (!window.API_BASE || !window.API_BASE.getReservationsByIds) {
            console.error("API_BASE.getReservationsByIds is not configured");
            if (window.Swal && Swal.fire) {
                Swal.fire('Error', 'Reservations details endpoint is not configured on the page.', 'error');
            }
            return;
        }

        const idsCsv = idsArray
            .map(x => String(x).trim())
            .filter(Boolean)
            .join(',');

        if (!idsCsv) return;

        $.ajax({
            url: window.API_BASE.getReservationsByIds,
            type: 'GET',
            data: { ids: idsCsv },
            success: function (resp) {
                if (!resp || resp.isSuccess === false) {
                    const msg = resp && resp.message ? resp.message : 'Failed to load reservations.';
                    if (window.Swal && Swal.fire) Swal.fire('Error', msg, 'error');
                    return;
                }
                buildReservationDetailsModal(resp.data || []);
            },
            error: function (xhr, status, error) {
                console.error("Error loading reservation details:", error || xhr?.responseText);
                if (window.Swal && Swal.fire) Swal.fire('Error', 'Failed to load reservation details.', 'error');
            }
        });
    }

    function buildReservationDetailsModal(list) {
        const container = $('#reservationDetailsModalContainer');
        if (!container.length) return;

        $('#reservationDetailsModal').remove();

        const many = list.length > 1;
        const title = many ? 'Reservations' : 'Reservation details';

        let bodyHtml = '';

        if (!list.length) {
            bodyHtml = `<p class="text-muted mb-0">No reservation details found.</p>`;
        } else if (!many) {
            const r = list[0];

            const dateText = formatDateDisplay(r.date);
            const timeText = `${formatTimeDisplay(r.start_Time || r.startTime)} – ${formatTimeDisplay(r.end_Time || r.endTime)}`;
            const plate = r.plate_Number || r.plateNumber || '—';
            const chassis = r.chassis || r.Chassis || '—';
            const company = r.customerName || r.customerName || r.customerName || '—';
            const status = r.statusPrimaryName || r.statusSecondaryName || '—';
            const duration = r.duration != null ? `${r.duration} min` : '—';
            const description = (r.description || '').trim() || '<span class="text-muted">No description provided</span>';

            bodyHtml = `
            <div class="resv-modal-summary mb-3">
                <div>
                    <div class="resv-modal-plate">${plate}</div>
                    <div class="text-muted small">
                        <i class="bi bi-calendar3 me-1"></i>${dateText}
                        <span class="mx-1">•</span>
                        <i class="bi bi-clock me-1"></i>${timeText}
                    </div>
                    <div class="text-muted small mt-1">
                        <i class="bi bi-hash me-1"></i>ID ${r.id}
                    </div>
                </div>
                <div class="text-end">
                    <div class="mb-2">
                        <span class="badge resv-status-badge">${status}</span>
                    </div>
                    <div class="small text-muted">Duration</div>
                    <div class="fw-semibold">${duration}</div>
                </div>
            </div>

            <div class="resv-modal-grid mb-3">
                <div class="resv-field">
                    <div class="resv-label">Chassis</div>
                    <div class="resv-value">${chassis}</div>
                </div>
                <div class="resv-field">
                    <div class="resv-label">Company</div>
                    <div class="resv-value">${company}</div>
                </div>
                <div class="resv-field">
                    <div class="resv-label">Start time</div>
                    <div class="resv-value">${formatTimeDisplay(r.start_Time || r.startTime)}</div>
                </div>
                <div class="resv-field">
                    <div class="resv-label">End time</div>
                    <div class="resv-value">${formatTimeDisplay(r.end_Time || r.endTime)}</div>
                </div>
            </div>

            <div class="resv-description-card">
                <div class="resv-label mb-1">Description</div>
                <div class="resv-description text-truncate">${description.replace(/\n/g, '<br>')}</div>
            </div>
        `;
        } else {
            bodyHtml += `<div class="accordion" id="resDetailsAccordion">`;
            list.forEach((r, idx) => {
                const idSafe = r.id || idx;
                const collapseId = `resDetails-${idSafe}`;
                const headingId = `heading-${idSafe}`;

                const plate = r.plate_Number || r.plateNumber || 'No plate';
                const dateText = formatDateDisplay(r.date);
                const timeText = `${formatTimeDisplay(r.start_Time || r.startTime)} – ${formatTimeDisplay(r.end_Time || r.endTime)}`;
                const status = r.statusPrimaryName || r.statusSecondaryName || '—';
                const duration = r.duration != null ? `${r.duration} min` : '—';
                const chassis = r.chassis || r.Chassis || '—';
                const company = r.companyName || r.companyId || r.CompanyId || '—';
                const description = (r.description || '').trim() || '<span class="text-muted">No description provided</span>';

                const showClass = idx === 0 ? 'show' : '';
                const expanded = idx === 0 ? 'true' : 'false';
                const collapsedClass = idx === 0 ? '' : 'collapsed';

                bodyHtml += `
                <div class="accordion-item mb-1 resv-accordion-item">
                    <h2 class="accordion-header" id="${headingId}">
                        <button class="accordion-button ${collapsedClass}" type="button"
                                data-bs-toggle="collapse" data-bs-target="#${collapseId}"
                                aria-expanded="${expanded}" aria-controls="${collapseId}">
                            <div class="w-100 d-flex justify-content-between align-items-center">
                                <div>
                                    <div class="fw-semibold">${plate}</div>
                                    <div class="small text-muted">${dateText} • ${timeText}</div>
                                </div>
                                <div class="text-end">
                                    <div><span class="badge resv-status-badge">${status}</span></div>
                                    <div class="small text-muted mt-1">Duration: ${duration}</div>
                                </div>
                            </div>
                        </button>
                    </h2>
                    <div id="${collapseId}" class="accordion-collapse collapse ${showClass}"
                         aria-labelledby="${headingId}" data-bs-parent="#resDetailsAccordion">
                        <div class="accordion-body">
                            <div class="resv-modal-grid mb-3">
                                <div class="resv-field">
                                    <div class="resv-label">Reservation ID</div>
                                    <div class="resv-value">${r.id}</div>
                                </div>
                                <div class="resv-field">
                                    <div class="resv-label">Chassis</div>
                                    <div class="resv-value">${chassis}</div>
                                </div>
                                <div class="resv-field">
                                    <div class="resv-label">Company</div>
                                    <div class="resv-value">${company}</div>
                                </div>
                                <div class="resv-field">
                                    <div class="resv-label">Start time</div>
                                    <div class="resv-value">${formatTimeDisplay(r.start_Time || r.startTime)}</div>
                                </div>
                                <div class="resv-field">
                                    <div class="resv-label">End time</div>
                                    <div class="resv-value">${formatTimeDisplay(r.end_Time || r.endTime)}</div>
                                </div>
                            </div>

                            <div class="resv-description-card">
                                <div class="resv-label mb-1">Description</div>
                                <div class="resv-description text-truncate">${description.replace(/\n/g, '<br>')}</div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            });
            bodyHtml += `</div>`;
        }

        const modalHtml = `
<div class="modal fade resv-details-modal" id="reservationDetailsModal" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header border-0 pb-0">
        <div>
            <h5 class="modal-title fw-semibold">${title}</h5>
            ${many ? `<div class="text-muted small">${list.length} reservations in this slot</div>` : ''}
        </div>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body pt-3">
        ${bodyHtml}
      </div>
      <div class="modal-footer border-0 justify-content-center pt-0">
        <button type="button" class="btn btn-outline-secondary px-4" data-bs-dismiss="modal">
            Close
        </button>
      </div>
    </div>
  </div>
</div>`;

        container.html(modalHtml);
        const modalEl = document.getElementById('reservationDetailsModal');
        if (!modalEl) return;
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    }

    // ---------- Overlay helpers (vertical bars) ----------
    function getResGridBits() {
        const scroll = qs('.resv-scroll');
        const table = qs('.resv-grid');
        const tbody = qs('#resvBody');
        return { scroll, table, tbody };
    }

    function ensureOverlay() {
        const { scroll } = getResGridBits();
        if (!scroll) return null;

        let ov = qs('.resv-overlay', scroll);

        // dispose existing tooltips to avoid ghost tooltips
        if (ov && window.bootstrap && bootstrap.Tooltip) {
            qsa('[data-bs-toggle="tooltip"]', ov).forEach(node => {
                try {
                    const inst = bootstrap.Tooltip.getInstance(node);
                    if (inst) inst.dispose();
                } catch { /* ignore */ }
            });
        }

        if (!ov) {
            ov = document.createElement('div');
            ov.className = 'resv-overlay';
            scroll.appendChild(ov);
        } else {
            ov.innerHTML = '';
        }
        return ov;
    }

    function centersFromSelector(sel, scroll) {
        const nodes = qsa(sel, scroll).filter(n => n && n.getClientRects && n.getClientRects().length);
        const sRect = scroll.getBoundingClientRect();
        return nodes.map(th => {
            const r = th.getBoundingClientRect();
            return (r.left - sRect.left) + scroll.scrollLeft + r.width / 2;
        });
    }

    // group intervals by same start/end → one bar per group
    function groupIntervalsByTime(list) {
        const groupsMap = new Map();
        list.forEach(p => {
            const key = `${p.start}-${p.end}`;
            if (!groupsMap.has(key)) {
                groupsMap.set(key, { start: p.start, end: p.end, items: [] });
            }
            groupsMap.get(key).items.push(p);
        });
        return Array.from(groupsMap.values());
    }

    // ✅ Tooltip: show PLATE numbers (not description)
    function buildTooltipHtmlForGroup(group) {
        const plates = group.items
            .map(item => (item.plate || "").trim())
            .filter(Boolean);

        const uniquePlates = [...new Set(plates)];
        return uniquePlates.length ? uniquePlates.join('<br>') : labelReserved;
    }

    function renderOverlayDay({ startM, endM, dateISO, intervalsMap }) {
        const { scroll, table, tbody } = getResGridBits();
        if (!scroll || !table || !tbody) return;

        const overlay = ensureOverlay();
        if (!overlay) return;

        const sRect = scroll.getBoundingClientRect();
        const tbRect = tbody.getBoundingClientRect();
        const bodyTop = (tbRect.top - sRect.top) + scroll.scrollTop;
        const bodyH = tbody.scrollHeight;

        const range = Math.max(1, endM - startM);
        const timeToY = (mins) => bodyTop + ((mins - startM) / range) * bodyH;

        // Day has ONE schedule column
        const centers = centersFromSelector('.resv-grid thead th.schedule-col', scroll);
        if (!centers.length) return;
        const cx = centers[0];

        const list = intervalsMap.get(dateISO) || [];
        const groups = groupIntervalsByTime(list);
        const toInit = [];

        groups.forEach(group => {
            const cStart = clamp(group.start, startM, endM);
            const cEnd = clamp(group.end, startM, endM);
            if (cEnd <= cStart) return;

            const y1 = timeToY(cStart);
            const y2 = timeToY(cEnd);
            const h = Math.max(4, y2 - y1);

            const ids = group.items
                .map(x => x.id)
                .filter(id => id !== null && id !== undefined);

            const count = group.items.length;

            const el = document.createElement('div');
            el.className = 'period-vert reserved';
            el.style.left = `${cx}px`;
            el.style.top = `${y1}px`;
            el.style.height = `${h}px`;
            if (ids.length) el.setAttribute('data-ids', ids.join(','));

            const tip = buildTooltipHtmlForGroup(group);

            el.innerHTML = `
                <span class="sr">${tip}</span>
                <span class="badge bg-primary position-absolute top-50 start-50 translate-middle">${count}</span>
            `;

            el.setAttribute('data-bs-toggle', 'tooltip');
            el.setAttribute('data-bs-html', 'true');
            el.setAttribute('data-bs-title', tip);

            el.addEventListener('click', function (ev) {
                ev.preventDefault();
                ev.stopPropagation();
                const idsAttr = el.getAttribute('data-ids');
                if (!idsAttr) return;
                const ids = idsAttr.split(',').map(s => s.trim()).filter(Boolean);
                if (!ids.length) return;
                openReservationDetailsModal(ids);
            });

            overlay.appendChild(el);
            toInit.push(el);
        });

        toInit.forEach(el => {
            try { new bootstrap.Tooltip(el); } catch { /* ignore */ }
        });
    }

    function renderOverlayWeek({ startM, endM, weekStartDate, intervalsMap }) {
        const { scroll, table, tbody } = getResGridBits();
        if (!scroll || !table || !tbody) return;

        const overlay = ensureOverlay();
        if (!overlay) return;

        const sRect = scroll.getBoundingClientRect();
        const tbRect = tbody.getBoundingClientRect();
        const bodyTop = (tbRect.top - sRect.top) + scroll.scrollTop;
        const bodyH = tbody.scrollHeight;

        const range = Math.max(1, endM - startM);
        const timeToY = (mins) => bodyTop + ((mins - startM) / range) * bodyH;

        const centers = centersFromSelector('.resv-grid thead th.day-col', scroll);
        if (!centers.length) return;

        const dayIndexByISO = new Map();
        for (let i = 0; i < 7; i++) {
            const d = new Date(weekStartDate);
            d.setDate(weekStartDate.getDate() + i);
            dayIndexByISO.set(toISODate(d), i);
        }

        const toInit = [];

        intervalsMap.forEach((rawList, dISO) => {
            const idx = dayIndexByISO.get(dISO);
            if (idx === undefined) return;
            const cx = centers[idx];
            if (typeof cx !== "number") return;

            const groups = groupIntervalsByTime(rawList);

            groups.forEach(group => {
                const cStart = clamp(group.start, startM, endM);
                const cEnd = clamp(group.end, startM, endM);
                if (cEnd <= cStart) return;

                const y1 = timeToY(cStart);
                const y2 = timeToY(cEnd);
                const h = Math.max(4, y2 - y1);

                const ids = group.items
                    .map(x => x.id)
                    .filter(id => id !== null && id !== undefined);

                const count = group.items.length;

                const el = document.createElement('div');
                el.className = 'period-vert reserved';
                el.style.left = `${cx}px`;
                el.style.top = `${y1}px`;
                el.style.height = `${h}px`;
                if (ids.length) el.setAttribute('data-ids', ids.join(','));

                const tip = buildTooltipHtmlForGroup(group);

                el.innerHTML = `
                    <span class="sr">${tip}</span>
                    <span class="badge bg-primary position-absolute top-50 start-50 translate-middle">${count}</span>
                `;

                el.setAttribute('data-bs-toggle', 'tooltip');
                el.setAttribute('data-bs-html', 'true');
                el.setAttribute('data-bs-title', tip);

                el.addEventListener('click', function (ev) {
                    ev.preventDefault();
                    ev.stopPropagation();
                    const idsAttr = el.getAttribute('data-ids');
                    if (!idsAttr) return;
                    const ids = idsAttr.split(',').map(s => s.trim()).filter(Boolean);
                    if (!ids.length) return;
                    openReservationDetailsModal(ids);
                });

                overlay.appendChild(el);
                toInit.push(el);
            });
        });

        toInit.forEach(el => {
            try { new bootstrap.Tooltip(el); } catch { /* ignore */ }
        });
    }

    // ---------- DAY VIEW ----------
    function buildDay() {
        const intervalsMap = buildIntervalsMap();
        const chosen = parseDateInput(elDate?.value || "");
        const dateISO = toISODate(chosen);
        const today = new Date();

        // ✅ Auto-expand Day bounds to include reservations (otherwise you see nothing if 예약 is outside 8–16)
        const dayList = RESERVATIONS.filter(r => dateISOfromApi(r.date) === dateISO);
        const mm = getMinMaxFromReservations(dayList);

        let startM = DAY_START_MIN;
        let endM = DAY_END_MIN;

        // also respect user inputs if present
        const inStart = toMinutesApi(((elStart?.value) || "") + ":00");
        const inEnd = toMinutesApi(((elEnd?.value) || "") + ":00");
        if (Number.isFinite(inStart) && Number.isFinite(inEnd) && inEnd > inStart) {
            startM = Math.min(startM, inStart);
            endM = Math.max(endM, inEnd);
        }

        if (mm) {
            startM = Math.min(startM, floorToHour(mm.minStart));
            endM = Math.max(endM, ceilToHour(mm.maxEnd));
        }

        startM = clamp(startM, 0, 24 * 60);
        endM = clamp(endM, 0, 24 * 60);
        if (endM <= startM) { startM = DAY_START_MIN; endM = DAY_END_MIN; }

        const step = 60;
        const times = [];
        for (let t = startM; t <= endM; t += step) times.push(t);

        setThead([`
            <tr class="head-row-1">
                <th class="text-uppercase time-cell">Time</th>
                <th class="schedule-col">
                    <div class="day-card text-center">
                        <div class="date-pill ${sameDate(chosen, today) ? "today" : ""}">
                            <div>${chosen.getDate()}</div>
                            <small>${dayAbbr(chosen)}</small>
                        </div>
                    </div>
                </th>
            </tr>
        `]);

        if (!elBody) return;
        elBody.innerHTML = "";

        times.forEach(mins => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td class="time-cell align-middle">${formatHourLabel(mins)}</td>
                <td class="schedule-cell"></td>
            `;
            elBody.appendChild(tr);
        });

        requestAnimationFrame(() => {
            renderOverlayDay({ startM, endM, dateISO, intervalsMap });
        });
    }

    // ---------- WEEK VIEW ----------
    function buildWeek() {
        const intervalsMap = buildIntervalsMap();
        const chosen = parseDateInput(elDate?.value || "");
        const weekStart = getStartOfWeek(chosen, 6);
        const today = new Date();

        const weekDates = [];
        for (let i = 0; i < 7; i++) {
            const d = new Date(weekStart);
            d.setDate(weekStart.getDate() + i);
            weekDates.push(d);
        }
        const weekISOset = new Set(weekDates.map(d => toISODate(d)));

        const weekReservations = RESERVATIONS.filter(r => weekISOset.has(dateISOfromApi(r.date)));
        let minMax = getMinMaxFromReservations(weekReservations);
        if (!minMax) {
            const startM = toMinutesApi(((elStart?.value) || "08:00") + ":00");
            const endM = toMinutesApi(((elEnd?.value) || "17:00") + ":00");
            minMax = { minStart: startM, maxEnd: endM };
        }

        const step = 60;
        const times = timesBetween(minMax.minStart, minMax.maxEnd, step);

        let header = `<th class="text-uppercase time-cell">Time</th>`;
        weekDates.forEach(d => {
            header += `
                <th class="day-col">
                    <div class="day-card text-center">
                        <div class="date-pill ${sameDate(d, today) ? "today" : ""}">
                            <div>${d.getDate()}</div>
                            <small>${dayAbbr(d)}</small>
                        </div>
                    </div>
                </th>`;
        });
        setThead([`<tr class="head-row-1">${header}</tr>`]);

        if (!elBody) return;
        elBody.innerHTML = "";

        times.forEach(mins => {
            const tr = document.createElement("tr");
            let rowHtml = `<td class="time-cell align-middle">${formatHourLabel(mins)}</td>`;
            for (let i = 0; i < 7; i++) rowHtml += `<td class="schedule-cell"></td>`;
            tr.innerHTML = rowHtml;
            elBody.appendChild(tr);
        });

        requestAnimationFrame(() => {
            renderOverlayWeek({
                startM: minMax.minStart,
                endM: minMax.maxEnd,
                weekStartDate: weekStart,
                intervalsMap
            });
        });
    }

    // ---------- MONTH VIEW ----------
    function buildMonth() {
        const base = parseDateInput(elDate?.value || "");
        const first = new Date(base.getFullYear(), base.getMonth(), 1);
        const dow = first.getDay(); // 0=Sun
        const start = new Date(first);
        start.setDate(first.getDate() - dow);
        const today = new Date();

        const header = [`<th class="text-uppercase time-cell"></th>`]
            .concat(DOW.map(d => `<th class="day-hdr text-center"><div class="fw-bold">${d}</div></th>`))
            .join("");

        setThead([`<tr>${header}</tr>`]);

        if (!elBody) return;
        elBody.innerHTML = "";

        for (let r = 0; r < 6; r++) {
            const tr = document.createElement("tr");
            tr.innerHTML = `<td class="time-cell"></td>`;
            for (let c = 0; c < 7; c++) {
                const cellDate = new Date(start);
                cellDate.setDate(start.getDate() + r * 7 + c);

                const td = document.createElement("td");
                const isToday = sameDate(cellDate, today);
                const inMonth = cellDate.getMonth() === base.getMonth();

                td.className = "month-cell" +
                    (isToday ? " today" : "") +
                    (inMonth ? "" : " text-muted");

                td.innerHTML = `<div class="month-daynum">${cellDate.getDate()}</div>`;
                td.addEventListener("click", () => {
                    if (elDate) elDate.value = toISODate(cellDate);
                    switchView("Day");
                });
                tr.appendChild(td);
            }
            elBody.appendChild(tr);
        }

        const grid = qs(".resv-grid");
        if (grid) grid.classList.add("month-table");
    }

    // ---------- router / events ----------
    function build() {
        ensureDate();

        const grid = qs(".resv-grid");
        if (grid) grid.classList.remove("month-table");

        // clear overlay container when rebuilding (scoped)
        const { scroll } = getResGridBits();
        const ov = scroll ? qs('.resv-overlay', scroll) : qs('.resv-overlay');
        if (ov) ov.innerHTML = '';

        if (state.view === "Day") buildDay();
        else if (state.view === "Week") buildWeek();
        else buildMonth();

        setViewTitle();

        if (btnClear) btnClear.style.display = "none";
    }

    function switchView(v) {
        state.view = v;
        [viewDay, viewWeek, viewMonth].forEach(b => b?.classList.remove("active"));
        if (v === "Day") viewDay?.classList.add("active");
        else if (v === "Week") viewWeek?.classList.add("active");
        else viewMonth?.classList.add("active");
        build();
    }

    // ✅ rebuild on date change (not only title)
    if (elDate) elDate.addEventListener("change", build);
    if (elStart) elStart.addEventListener("change", build);
    if (elEnd) elEnd.addEventListener("change", build);
    if (viewDay) viewDay.addEventListener("click", () => switchView("Day"));
    if (viewWeek) viewWeek.addEventListener("click", () => switchView("Week"));
    if (viewMonth) viewMonth.addEventListener("click", () => switchView("Month"));

    // ---- flatpickr for dcDate ----
    if (elDate && window.flatpickr) {
        flatpickr(elDate, {
            dateFormat: "Y-m-d",
            minDate: "today",
            onChange: function (selectedDates, dateStr) {
                elDate.value = dateStr;
                elDate.dispatchEvent(new Event("change", { bubbles: true }));
            }
        });
    }

    // Initial boot
    ensureDate();
    switchView("Day");
})();
