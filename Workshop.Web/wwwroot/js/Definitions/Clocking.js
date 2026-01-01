// Clocking UI behavior (Idle styling, solid Logs, DataTables i18n via Common, no responsive wrappers assumed)
(function ($, window, document) {
    "use strict";
    const R = window.resources || {};
    const $activeTbody = $('#activeTable tbody');
    const $historyTbody = $('#historyTable tbody');
    let seq = 1;
    const rows = {}; // { id: {tech,wip,rts,allowedM,startMs,breakMs,onBreak,breakStartMs,breakReason,periodStart} }

    function fmtMS(ms) {
        const total = Math.max(0, Math.floor(ms / 1000));
        const m = String(Math.floor(total / 60)).padStart(2, '0');
        const s = String(total % 60).padStart(2, '0');
        return `${m}:${s}`;
    }

    function ensureNoRowsPlaceholders() {
        if ($activeTbody.children('tr:not(#noActiveRow)').length) $('#noActiveRow').hide(); else $('#noActiveRow').show();
        if ($historyTbody.children('tr:not(#noHistoryRow)').length) $('#noHistoryRow').hide(); else $('#noHistoryRow').show();
    }

    function statusWorkingPill() {
        return `<span class="badge bg-success-subtle text-success-dark">
      <i class="fas fa-play"></i> ${R.label_working || 'Working'}
    </span>`;
    }

    function statusIdlePill(reason, elapsedText) {
        return `<div>
      <span class="badge bg-warning-subtle text-warning-dark">
        <i class="fas fa-pause"></i> ${(R.label_idle || R.button_idle || 'Idle')}${reason ? `: ${reason}` : ''}
      </span>
      ${elapsedText ? `<div class="small text-muted">(${elapsedText})</div>` : ''}
    </div>`;
    }

    function renderActions(id, onBreak) {
        if (onBreak) {
            return `
        <div class="d-flex justify-content-end flex-wrap gap-2">
          <button class="btn btn-primary act-resume" data-id="${id}">
            <i class="fas fa-play"></i> ${R.button_resume || 'Resume'}
          </button>
          <button class="btn btn-secondary act-log" data-id="${id}">
            <i class="fas fa-clipboard-list"></i> ${R.button_log || 'Log'}
          </button>
          <button class="btn btn-danger act-clockout" data-id="${id}">
            <i class="fas fa-stop-circle"></i> ${R.button_clock_out || 'Clock out'}
          </button>
        </div>`;
        }
        return `
      <div class="d-flex justify-content-end flex-wrap gap-2">
        <button class="btn btn-warning act-idle" data-id="${id}">
          <i class="fas fa-pause"></i> ${(R.button_idle || R.label_idle || 'Idle')}
        </button>
        <button class="btn btn-secondary act-log" data-id="${id}">
          <i class="fas fa-clipboard-list"></i> ${R.button_log || 'Log'}
        </button>
        <button class="btn btn-danger act-clockout" data-id="${id}">
          <i class="fas fa-stop-circle"></i> ${R.button_clock_out || 'Clock out'}
        </button>
      </div>`;
    }

    function addActiveRow(tech, wip, rts, allowedM) {
        const id = `r${seq++}`;
        rows[id] = {
            tech, wip, rts,
            allowedM: Number(allowedM || 0),
            startMs: Date.now(),
            periodStart: new Date(),
            breakMs: 0,
            onBreak: false,
            breakStartMs: 0,
            breakReason: ''
        };

        const $tr = $(`
      <tr id="${id}">
        <td>${tech}</td>
        <td>${String(wip).split('—')[0].trim()}</td>
        <td>${rts}</td>
        <td class="status">${statusWorkingPill()}</td>
        <td class="elapsed">00:00</td>
        <td>${rows[id].allowedM}m</td>
        <td class="text-end actions">${renderActions(id, false)}</td>
      </tr>`);
        $activeTbody.append($tr);
        ensureNoRowsPlaceholders();
        return id;
    }

    // ticker
    setInterval(function () {
        Object.keys(rows).forEach(id => {
            const r = rows[id];
            let workedMs = Date.now() - r.startMs - r.breakMs;

            if (r.onBreak) {
                const breakElapsed = fmtMS(Date.now() - r.breakStartMs);
                $(`#${id} .status`).html(statusIdlePill(r.breakReason, breakElapsed));
                workedMs -= (Date.now() - r.breakStartMs);
            } else {
                $(`#${id} .status`).html(statusWorkingPill());
            }
            $(`#${id} .elapsed`).text(fmtMS(workedMs));
        });
    }, 1000);

    // actions (no New-panel extra buttons)
    $('#ClockInBtn').on('click', function () {
        const tech = $('#techSelect option:selected').text();
        const wip = $('#wipSelect option:selected').text();
        const $rts = $('#labourSelect option:selected');
        const rts = $rts.text();
        const allowedM = $rts.data('allowed') || 0;
        if (!tech || !wip || !rts) return; // basic guard
        addActiveRow(tech, wip, rts, allowedM);
    });

    $(document).on('click', '.act-idle', function () {
        const id = $(this).data('id');
        $('#breakRowId').val(id);
        const modal = new bootstrap.Modal(document.getElementById('breakModal'));
        modal.show();
    });

    $('#btnStartBreak').on('click', function () {
        const id = $('#breakRowId').val();
        if (!rows[id]) return;
        const r = rows[id];
        if (r.onBreak) return;

        r.onBreak = true;
        r.breakStartMs = Date.now();
        // take chosen reason text; fallback to localized Idle
        const reasonText = $('#breakReason option:selected').text();
        r.breakReason = reasonText || (R.label_idle || 'IDLE');
        $(`#${id} .actions`).html(renderActions(id, true));
        $('#breakModal').modal('hide');
    });

    $(document).on('click', '.act-resume', function () {
        const id = $(this).data('id');
        const r = rows[id];
        if (!r || !r.onBreak) return;

        r.breakMs += (Date.now() - r.breakStartMs);
        r.onBreak = false;
        r.breakStartMs = 0;
        r.breakReason = '';
        $(`#${id} .actions`).html(renderActions(id, false));
    });

    $(document).on('click', '.act-log', function () {
        // hook to per-row logs if needed
        console.log('Log clicked for row', $(this).data('id'));
    });

    function moveToHistoryAndRemove(id) {
        const r = rows[id];
        if (!r) return;

        const start = r.periodStart;
        const end = new Date();
        const pad2 = n => String(n).padStart(2, '0');
        const hhmm = d => `${pad2(d.getHours())}:${pad2(d.getMinutes())}`;
        const period = `${hhmm(start)} ${R.label_from_to || 'to'} ${hhmm(end)}`;

        const workedMs = Date.now() - r.startMs - r.breakMs - (r.onBreak ? (Date.now() - r.breakStartMs) : 0);
        const breaksMs = r.breakMs + (r.onBreak ? (Date.now() - r.breakStartMs) : 0);

        const $htr = $(`
      <tr>
        <td>${r.tech}</td>
        <td>${String(r.wip).split('—')[0].trim()}</td>
        <td>${r.rts}</td>
        <td>${period}</td>
        <td>${fmtMS(workedMs)}</td>
        <td>${fmtMS(breaksMs)}</td>
        <td class="text-end">
          <button class="btn btn-secondary" disabled>${R.button_details || 'Details'}</button>
        </td>
      </tr>`);
        $historyTbody.append($htr);

        $(`#${id}`).remove();
        delete rows[id];
        ensureNoRowsPlaceholders();
    }

    $(document).on('click', '.act-clockout', function () {
        moveToHistoryAndRemove($(this).data('id'));
    });

})(jQuery, window, document);
$('#historyTable').DataTable({
    responsive: true,
    pageLength: 25,
    lengthChange: true,
    language: {
        search: R.dt_search || "Search:",
        searchPlaceholder: "",
        emptyTable: R.dt_empty_table,
        info: R.dt_info,
        infoEmpty: R.dt_info_empty,
        infoFiltered: R.dt_info_filtered,
        lengthMenu: R.dt_length_menu,
        loadingRecords: R.dt_loading_records,
        processing: R.dt_processing,
        zeroRecords: R.dt_zero_records,
        paginate: {
            first: R.dt_paginate_first,
            last: R.dt_paginate_last,
            next: R.dt_paginate_next,
            previous: R.dt_paginate_previous
        }
    }
});
// server helpers
function setClockStatus(status) {
    document.getElementById('ClockingForm_StatusID').value = status;
    return true;
}

function setListClockStatus(item, status) {
    const row = item.closest('tr');
    if (!row) return true;
    const statusInput = row.querySelector('input.Status');
    if (statusInput) statusInput.value = status;
    return true;
}


// elapsed display

setInterval(() => {
    document.querySelectorAll('.StartedAt').forEach(item => {
        const row = item.closest('tr');
        if (!row) return;

        // --- Session Start ---
        const startedVal = row.querySelector('input.StartedAt')?.value;
        const started = parseDateSafe(startedVal);
        if (!started) return;

        const now = new Date();

        // --- All breaks ---
        const breakStarts = row.querySelectorAll('input.break-start');
        const breakEnds = row.querySelectorAll('input.break-end');

        // --- Total Working Time (excluding all breaks) ---
        let totalBreakMs = 0;
        breakStarts.forEach((startEl, i) => {
            const start = parseDateSafe(startEl.value);
            const end = parseDateSafe(breakEnds[i]?.value) || now; // ongoing break counts as now
            if (start && end && end > start) totalBreakMs += (end - start);
        });

        const totalDiffMs = Math.max(0, now - started - totalBreakMs);

        const totalDisplay = row.querySelector('.total-elapsed');
        if (totalDisplay) {
            totalDisplay.textContent = msToHHMMSS(totalDiffMs);
        }

        // --- Last Break Elapsed ---
        if (breakStarts.length > 0) {
            const lastIndex = breakStarts.length - 1;
            const lastStart = parseDateSafe(breakStarts[lastIndex].value);
            const lastEnd = parseDateSafe(breakEnds[lastIndex]?.value) || now;

            const lastDiffMs = Math.max(0, lastEnd - lastStart);

            const lastBreakDisplay = row.querySelector('.last-break-elapsed');
            if (lastBreakDisplay) {
                lastBreakDisplay.textContent = msToHHMMSS(lastDiffMs);
            }
        }

    });
}, 1000);


// --- Helpers ---
function parseDateSafe(str) {
    if (!str) return null;
    const d = new Date(str);
    return isNaN(d) ? null : d;
}

function msToHHMMSS(ms) {
    const totalSeconds = Math.floor(ms / 1000);
    const hours = String(Math.floor(totalSeconds / 3600)).padStart(2, '0');
    const minutes = String(Math.floor((totalSeconds % 3600) / 60)).padStart(2, '0');
    const seconds = String(totalSeconds % 60).padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
}

// AJAX: WIP -> Labourlines
function WIPChange(controlItem) {
    $.ajax({
        url: window.appUrls.setWIP,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(controlItem.value),
        success: function (data) {
            if (data) {
                var labourControl = $('#labourSelect');
                labourControl.empty();
                $.each(data, function (i, item) {
                    labourControl.append($('<option>', {
                        value: item.value,
                        text: item.text
                    }));
                });
                labourControl.trigger('change');
            }
        },
        error: function () {
            alert("Failed to load labour data.");
        }
    });
}
function clockAllOut() {
    $.ajax({
        url: window.appUrls.clockAllOut,
        type: 'POST',
        contentType: 'application/json',
        success: function () {
            window.location.reload();

        },
        error: function () {
            return false;
        }
    });

}

function TechnicianChange(controlItem) {
    $.ajax({
        url: window.appUrls.setTechnician,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(controlItem.value),
        success: function (data) {
            if (data) {
                var labourControl = $('#wipSelect');
                labourControl.empty();
                labourControl.append($('<option> Select</option>'));
                $.each(data, function (i, item) {
                    labourControl.append($('<option>', {
                        value: item.value,
                        text: item.text
                    }));
                });
            }
        },
        error: function () {
            alert("Failed to load labour data.");
        }
    });
}
// Logs modal: i18n + date-only
function formatDateTime(value) {
    if (!value) return '';

    const d = value instanceof Date ? value : new Date(value);
    if (isNaN(d)) return '';

    const pad = n => String(n).padStart(2, '0');

    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ` +
        `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
}

function ShowLogs(item, ID) {
    $('#breakLogsContainer').html(`<p class="text-muted">${(window.resources && window.resources.loading) || 'Loading...'}</p>`);

    $.ajax({
        url: window.appUrls.getLogs,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(ID),
        success: function (data) {
            if (data && data.length > 0) {
                let html = `
          <table class="table table-bordered table-striped align-middle">
            <thead class="table-light">
              <tr>
                <th>${R.col_id || 'ID'}</th>
                <th>${R.col_reason || 'Reason'}</th>
                <th>${R.col_note || 'Note'}</th>
                <th>${R.col_period || 'Period'}</th>
                <th>${R.col_start_date || 'Start (Date)'}</th>
                <th>${R.col_end_date || 'End (Date)'}</th>
              </tr>
            </thead>
            <tbody>
        `;

                data.forEach(breakLog => {
                    const s = breakLog.startAt ? new Date(breakLog.startAt) : null;
                    const e = breakLog.endAt ? new Date(breakLog.endAt) : null;

                    const diffMs = (s && e) ? (e - s) : NaN;
                    const diffMinutes = isNaN(diffMs) ? null : Math.floor(diffMs / 60000);
                    const period = diffMinutes == null ? '' :
                        (Math.floor(diffMinutes / 60) > 0
                            ? `${Math.floor(diffMinutes / 60)}h ${diffMinutes % 60}m`
                            : `${diffMinutes}m`);

                    html += `
        <tr>
          <td>${breakLog.id ?? ''}</td>
          <td>${breakLog.reasonString ?? ''}</td>
          <td>${breakLog.note ?? ''}</td>
          <td>${period}</td>
          <td>${formatDateTime(s)}</td>
          <td>${formatDateTime(e)}</td>
        </tr>
    `;
                });


                html += '</tbody></table>';
                $('#breakLogsContainer').html(html);
            } else {
                $('#breakLogsContainer').html(`<p class="text-muted">${R.no_logs_found || 'No logs found for this record.'}</p>`);
            }
        },
        error: function () {
            $('#breakLogsContainer').html(`<p class="text-danger">${R.load_error || 'Failed to load logs.'}</p>`);
        }
    });

    const modal = new bootstrap.Modal(document.getElementById('breakLogsModal'));
    modal.show();
    return true;
}


function setBreak(item, status) {
    // Set status
    const statusField = document.getElementById('ClockingForm_StatusID');
    if (statusField) statusField.value = status;

    // Get the closest row to the clicked button
    const $row = $(item).closest('tr');
    if (!$row.length) {
        console.warn('No row found near the clicked element.');
        return false;
    }

    // Set the row ID in the modal hidden input
    const rowId = $row.attr('id');
    if (!rowId) {
        console.warn('The row has no ID attribute.');
        return false;
    }
    $('#breakRowId').val(rowId);

    // Reset modal inputs
    $('#breakReason').val('LUNCH');
    $('#breakNote').val('');

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('breakModal'));
    modal.show();


    return true;
}

function getDuration(start, end) {
    const startTime = new Date(start);
    const endTime = new Date(end);
    const diffMs = endTime - startTime; // difference in milliseconds
    if (isNaN(diffMs)) return '';

    const diffMinutes = Math.floor(diffMs / 60000);
    const hours = Math.floor(diffMinutes / 60);
    const minutes = diffMinutes % 60;

    return hours > 0
        ? `${hours}h ${minutes}m`
        : `${minutes}m`;
}



function parseDateSafe(v) {
    if (!v) return null;
    // Try standard ISO, or replace space with T for common MVC formats
    var d = new Date(v);
    if (isNaN(d)) {
        d = new Date(String(v).replace(' ', 'T'));
    }
    return isNaN(d) ? null : d;
}
// Make sure this runs after the DOM is ready
$(function () {
    $('.the-parent').hover(
        function () {
            // mouseenter: remove the class
            $(this).children().removeClass('rts-text');
        },
        function () {
            // mouseleave: add the class back
            $(this).children().addClass('rts-text');
        }
    );
});

$(document).on('change', '#labourSelect', function () {
    const txt = $('#labourSelect option:selected').text() || '';

    const lastPart = txt.split('-').pop().trim();
    const keyId = parseInt(lastPart, 10);

    $('#rtsKeyIdHidden').val(isNaN(keyId) ? '' : keyId);
});
//$(document).on('change', '#labourSelect', function () {
//    const val = $(this).val() || '';   
//    const parts = val.split('|');

//    const rtsId = parts[0] || '';
//    const keyId = parts[1] || '';

//    $('#ClockingForm_RTSID').val(rtsId);

//    $('#rtsKeyIdHidden').val(keyId);
//});


// Append this to Clocking.js (or include as a separate script after it)
// Computes "Duration From" = (now - StartedAt) - lastBreakDuration (if any) and updates the Active table every second.
//(function ($) {
//    "use strict";

//    function parseDateSafe(v) {
//        if (!v) return null;
//        // Try standard ISO, or replace space with T for common MVC formats
//        var d = new Date(v);
//        if (isNaN(d)) {
//            d = new Date(String(v).replace(' ', 'T'));
//        }
//        return isNaN(d) ? null : d;
//    }

//    function formatDuration(ms) {
//        if (ms < 0) ms = 0;
//        var totalSeconds = Math.floor(ms / 1000);
//        var hours = Math.floor(totalSeconds / 3600);
//        var minutes = Math.floor((totalSeconds % 3600) / 60);
//        var seconds = totalSeconds % 60;
//        return (hours.toString().padStart(2, '0')) + ':' +
//            minutes.toString().padStart(2, '0') + ':' +
//            seconds.toString().padStart(2, '0');
//    }

//    function updateActiveDurations() {
//        $('#activeTable tbody tr').each(function () {
//            var $tr = $(this);
//            // hidden inputs rendered with Html.Hidden - use classes present in view
//            var startedVal = $tr.find('input.StartedAt').val() || $tr.find('input[name="StartedAt"]').val();
//            var lStartedVal = $tr.find('input.LStartedAt').val() || $tr.find('input[name="LStartedAt"]').val();
//            var lEndVal = $tr.find('input.LEndedAt').val() || $tr.find('input[name="LEndAt"]').val();
//            var statusVal = $tr.find('input.Status').val() || $tr.find('input[name="Status"]').val();

//            var started = parseDateSafe(startedVal);
//            var lStarted = parseDateSafe(lStartedVal);
//            var lEnd = parseDateSafe(lEndVal);
//            var now = new Date();

//            var display = '-';
//            if (started) {
//                // base duration since start (for active items use now)
//                var baseMs = now - started;

//                // subtract last break duration if present
//                var breakMs = 0;
//                if (lStarted && lEnd) {
//                    breakMs += (lEnd - lStarted);
//                } else if (lStarted && !lEnd) {
//                    // break still ongoing - subtract duration up to now
//                    breakMs += (now - lStarted);
//                }

//                var durationMs = baseMs - breakMs;
//                display = formatDuration(durationMs);
//            }

//            // update the "Elapsed" cell - header places Elapsed as 5th column (0-based index 4)
//            var $elapsedCell = $tr.find('td').eq(4);
//            if ($elapsedCell.length) {
//                $elapsedCell.text(display);
//            } else {
//                // fallback: append a small span in the last cell
//                $tr.find('td').last().prepend('<div class="clocking-duration me-2">' + display + '</div>');
//            }
//        });
//    }

//    $(function () {
//        // initial update and continuous refresh for running timers
//        updateActiveDurations();
//        setInterval(updateActiveDurations, 1000);
//    });

//})(jQuery);
