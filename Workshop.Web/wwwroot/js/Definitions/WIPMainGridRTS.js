// Grid ======================================================
$(function () {
    const LabourLineEnum = {
        1: { value: 1, text: "M-Draft" },
        2: { value: 2, text: "B-Booked" },
        3: { value: 3, text: "W-WIP" },
        4: { value: 4, text: "P-Waiting For Parts" },
        5: { value: 5, text: "A-Approval" },
        6: { value: 6, text: "L-Waiting for Labour" },
        7: { value: 7, text: "T-QA" },
        8: { value: 8, text: "C-Completed" },
        9: { value: 9, text: "X-Invoiced" }
    };
    let currentRowKey = null;
    let currentGrid = null;

    const descPopup = $("#descPopup").dxPopup({
        width: 600,
        height: 400,
        showTitle: true,
        title: "Edit Description",
        visible: false,
        dragEnabled: true,
        closeOnOutsideClick: true,
        contentTemplate: function (contentElement) {
            contentElement.empty();
            const textarea = $("<textarea>")
                .attr("id", "descEditor")
                .css({
                    width: "100%",
                    height: "200px",
                    fontSize: "14px"
                });

            const saveBtn = $("<button>")
                .addClass("dx-button dx-button-primary btn btn-primary")
                .text("Save")
                .on("click", function () {
                    const newDesc = textarea.val();

                    if (currentGrid && currentRowKey != null) {
                        const rowIndex = currentGrid.getRowIndexByKey(currentRowKey);

                        if (rowIndex >= 0) {
                            currentGrid.cellValue(rowIndex, "LongDescription", newDesc);
                            currentGrid.saveEditData();
                        }
                    }

                    descPopup.hide();
                });

            contentElement.append(textarea).append("<br/><br/>").append(saveBtn);
        }
    }).dxPopup("instance");

    function generateUniqueKeyId() {
        if (!window.nextKeyId) window.nextKeyId = 1;
        return window.nextKeyId++;
    }

    ServicesData.forEach(row => {
        if (!row.KeyId) row.KeyId = generateUniqueKeyId();
    });

    $("#mainRTSGrid").dxDataGrid({
        dataSource: ServicesData,
        keyExpr: "KeyId",
        noDataText: resources.NoDataInTable,
        showBorders: true,
        validateOnValueChange: false,
        remoteOperations: {
            filtering: true,
            sorting: true,
            paging: true
        },
        columns: [
            { dataField: "KeyId", caption: "#", visible: true, allowEditing: false, width: 50, },
            { dataField: "Id", caption: "ID", visible: false },
            {
                dataField: "WIPId", caption: "WIPId", dataType: "number", visible: false,
                calculateCellValue: function (rowData) {
                    var value = $('#Id').val();
                    var numValue = parseInt(value);
                    rowData.WIPId = numValue;
                    return numValue;
                }
            },
            { dataField: "Code", caption: window.RazorVars.DXCode, allowEditing: false },
            { dataField: "Description", caption: window.RazorVars.DXName, allowEditing: true, width: 120 },
            {
                dataField: "LongDescription",
                caption: window.RazorVars.DXLongDescription,
                allowEditing: false,
                width: 200,
                cellTemplate: function (container, options) {
                    const value = options.value || "";
                    const shortText = value.length > 25 ? value.substring(0, 25) + "..." : value;

                    $(container)
                        .empty()
                        .attr("title", value);

                    $("<span>")
                        .text(shortText)
                        .css({
                            "font-size": "12px",
                            "display": "inline-block",
                            "max-width": "100%"
                        })
                        .appendTo(container);
                    $("<span>")
                        .addClass("dx-link")
                        .text(" Edit")
                        .css({
                            "color": "#007bff",
                            "cursor": "pointer",
                            "margin-left": "10px"
                        })
                        .on("click", function () {
                            currentGrid = options.component;
                            currentRowKey = options.key;

                            descPopup.option("onShown", function () {
                                $("#descEditor").val(options.value || "");
                            });

                            descPopup.show();
                        })
                        .appendTo(container);
                }
            },
            { dataField: "StandardHours", dataType: "number", caption: window.RazorVars.DXStandardHours, width: 120, allowEditing: true, alignment: "left" },
            { dataField: "BaseRate", visible: false, allowEditing: false },
            {
                dataField: "Rate",
                caption: window.RazorVars.DXRate,
                dataType: "number",
                allowEditing: false,
                alignment: "left",
                calculateCellValue: function (rowData) {
                    rowData.Rate = ensureDiscountedRate(rowData);
                    return ensureDiscountedRate(rowData);
                }
            },
            {
                dataField: "Discount", caption: window.RazorVars.DXDiscount, dataType: "number", allowEditing: Permission_AddDiscount, alignment: "left",
                editorOptions: {
                    min: 0,
                    max: 100
                },
                customizeText: function (cellInfo) {
                    return (Number(cellInfo.value) || 0).toFixed(1) + " %";
                }
            },
            {
                dataField: "Tax",
                caption: window.RazorVars.DXTax,
                dataType: "number",
                allowEditing: false,
                alignment: "left",
                calculateCellValue: function (rowData) {
                    var vatId = getEffectiveVatId(rowData);

                    var vatValue = parseFloat(GetVatValueById(vatId)) || 0;
                    var vatPercent = vatValue > 1 ? vatValue / 100 : vatValue;

                    var hours = parseFloat(rowData.StandardHours) || 0;

                    var price = ensureDiscountedRate(rowData);
                    var taxAmount = hours * price * vatPercent;

                    rowData.Tax = +taxAmount.toFixed(2);
                    return rowData.Tax;
                }
            },
            {
                dataField: "Total",
                caption: window.RazorVars.DXTotal,
                dataType: "number",
                alignment: "left",
                allowEditing: false,
                calculateCellValue: function (rowData) {
                    var rate = ensureDiscountedRate(rowData);
                    var standardHours = parseFloat(rowData.StandardHours) || 1;
                    var tax = parseFloat(rowData.Tax) || 0;
                    var rowDiscount = parseFloat(rowData.Discount) || 0;

                    var totalValue = rate * standardHours;

                    if (rowDiscount > 0) {
                        totalValue -= totalValue * (rowDiscount / 100);
                    }

                    totalValue += tax;

                    rowData.Total = +totalValue.toFixed(2);
                    return rowData.Total;
                }
            },
            { dataField: "IsExternal", caption: "IsExternal", dataType: "boolean", allowEditing: false, visible: false },
            { dataField: "ExternalWorkshopId", caption: "ExternalWorkshopId", dataType: "number", allowEditing: false, visible: false },
            { dataField: "TimeTaken", caption: window.RazorVars.DXTimeTaken, dataType: "number", allowEditing: false, alignment: "left" },
            { dataField: "Status", caption: "Status", dataType: "number", visible: false, alignment: "left" },
            {
                dataField: "StatusText", caption: window.RazorVars.DXStatus, allowEditing: false, alignment: "left",
            },
            {
                dataField: "TechnicianId",
                caption: window.RazorVars.DXAssignTo,
                dataType: "number",
                allowEditing: false,
                alignment: "left",

                lookup: {
                    dataSource: TechniciansDDL,
                    valueExpr: "value",
                    displayExpr: "text"
                },

                calculateDisplayValue: function (row) {
                    if (row.TechnicianId == null) return "";

                    const tech = TechniciansDDL.find(
                        t => Number(t.value) === Number(row.TechnicianId)
                    );

                    return tech ? tech.text : row.TechnicianId;
                }
            },
            {
                dataField: "AccountType",
                caption: window.RazorVars.DXAccountType,
                dataType: "number",
                allowEditing: true,
                alignment: "left",
                lookup: {
                    dataSource: AccountTypes.map(x => ({
                        Value: parseInt(x.Value),
                        Text: x.Text
                    })),
                    valueExpr: "Value",
                    displayExpr: "Text"
                },
                calculateCellValue: function (rowData) {
                    const partialInvoicing = $("#optPartialInv").is(":checked");
                    const accountTypeVal = parseInt($("#AccountType").val()) || 0;

                    if (!partialInvoicing && (!rowData.AccountType || rowData.AccountType === 0)) {
                        rowData.AccountType = accountTypeVal;
                    }

                    return rowData.AccountType;
                }
            },
            {
                type: "buttons",
                width: 110,
                buttons: [
                    {
                        hint: "Assign",
                        icon: "fad fa-regular fa-user act-booking",
                        visible: function (e) {
                            return !(wipStatus === Gone || wipStatus === Invoiced) &&
                                e.row.data.Status != 1 && e.row.data.Status != 20 &&
                                parseInt(e.row.data.Status) !== 24 &&
                                parseInt(e.row.data.Status) !== 26;
                        },
                        onClick: function (e) {
                            console.log(e.row.data.Id);
                            $("#RTSId").val(e.row.data.Id);
                            openScheduleModal(e.row.data);
                        }
                    },
                    {
                        hint: "Delete",
                        icon: "fad fa-trash",
                        visible: function (e) {
                            return !(wipStatus === Gone || wipStatus === Invoiced) &&
                                parseInt(e.row.data.Status) !== 19 &&
                                parseInt(e.row.data.Status) !== 20 &&
                                parseInt(e.row.data.Status) !== 24 &&
                                parseInt(e.row.data.Status) !== 25 &&
                                parseInt(e.row.data.Status) !== 26;
                        },
                        onClick: function (e) {
                            var grid = e.component;
                            grid.getDataSource().store().remove(e.row.key);
                            grid.refresh();
                            DeleteService(e.row.data);
                        }
                    }
                ]
            },
        ],
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnAutoWidth: true,
        hoverStateEnabled: false,
        paging: {
            pageSize: 10
        },
        rowAlternationEnabled: true,
        pager: {
            visible: true,
            showPageSizeSelector: true,
            allowedPageSizes: [5, 10, 20, 50],
            showInfo: true,
            showNavigationButtons: true
        },
        editing: {
            mode: "cell",
            allowDeleting: true,
            allowUpdating: true
        },
        onCellValueChanged: function (e) {
            if (["Discount", "StandardHours", "Rate", "Total"].includes(e.column.dataField)) {
                e.component.refresh().done(function () {
                    updateTotalLabourFieldsFromGrid();
                });
            } else {
                updateTotalLabourFieldsFromGrid();
            }
        },
        onRowInserted: function () {
            updateTotalLabourFieldsFromGrid();
        },
        onRowRemoved: function () {
            updateTotalLabourFieldsFromGrid();
        },
        onContentReady: function () {
            updateTotalLabourFieldsFromGrid();
        },
        onInitNewRow: function (e) {
            const accountTypeVal = parseInt($("#AccountType").val());
            const partialInvoicing = $("#optPartialInv").is(":checked");

            if (!partialInvoicing) {
                e.data.AccountType = accountTypeVal;
            }
        },
        onRowInserted: function (e) {
            const grid = e.component;
            const store = grid.getDataSource().store();
            const accountTypeVal = parseInt($("#AccountType").val());
            const partialInvoicing = $("#optPartialInv").is(":checked");

            if (!partialInvoicing) {
                e.data.AccountType = accountTypeVal;
                store.update(e.key, e.data).then(() => grid.refresh());
            }

            updateTotalLabourFieldsFromGrid();
        },
        onEditorPrepared: function (e) {
            if (e.parentType !== "dataRow" || e.dataField !== "AccountType") return;

            const editorInstance = e.editorElement.dxSelectBox("instance");
            if (!editorInstance) return;

            editorInstance.option("onValueChanged", function (args) {
                e.setValue(args.value);

                const keyId = e.row.key;
                const rtsId = e.row.data.Id;
                const acc = args.value;

                console.log("AccountType changed:", keyId, rtsId, acc);
                getRateAmount(keyId, rtsId, acc);
            });
        }
    });
});

async function updateTotalLabourFieldsFromGrid() {
    const grid = $("#mainRTSGrid").dxDataGrid("instance");
    if (!grid) return;

    const ds = grid.getDataSource();
    if (!ds) return;

    const allRows = await ds.store().load();

    let totalBase = 0;
    let totalDiscountAmount = 0;
    let totalTaxAmount = 0;
    let totalAfterDiscount = 0;

    allRows.forEach(d => {
        d = d || {};

        const rate = ensureDiscountedRate(d);
        const hours = parseFloat(d.StandardHours) || 0;
        const pct = parseFloat(d.Discount) || 0;
        const tax = parseFloat(d.Tax) || 0;

        const lineBase = rate * hours;
        const lineDisc = lineBase * (pct / 100);
        const lineAfterDiscount = lineBase - lineDisc;

        totalBase += lineBase;
        totalDiscountAmount += lineDisc;
        totalTaxAmount += tax;
        totalAfterDiscount += lineAfterDiscount;
    });

    $("#totLabour").text("SAR " + totalAfterDiscount.toFixed(2));
    setAmount("#totLabour", totalAfterDiscount);

    $("#TotalDiscountsLabour").text("SAR " + totalDiscountAmount.toFixed(2));
    $("#TotalTaxLabour").text("SAR " + totalTaxAmount.toFixed(2));

    const currentVAT = getAmount("#totVAT");
    const combinedVAT = currentVAT + totalTaxAmount;
    setAmount("#totVAT", combinedVAT);

    updateSubtotal();
}

//============================================================================
// open booking from actions

const $schJobChip = $('#schJobChip');
const $schAllowedChip = $('#schAllowedChip');

// to remember existing scheduled time (if editing)
let scheduledStartHHMM = null; // stores 12h display like "8:00 AM"

function openScheduleModal(e) {
    const $tr = $(this).closest('tr');
    const KeyId = e.KeyId;
    scheduledStartHHMM = null;

    // reset fields
    $('#schDate, #schTech, #schStart, #schDuration, #schEnd, #KeyId').val('');
    $("#KeyId").val(KeyId);

    // default date = today
    const todayStr = new Date().toISOString().slice(0, 10);

    // init timepicker (12h)
    initSchStartTimepicker([], "8:00 AM");

    $('#schDate').val(todayStr).trigger('change');

    $('table tr').removeClass('selected-row');
    $tr.addClass('selected-row');

    if (e.StandardHours != null && e.StandardHours !== undefined && e.StandardHours > 0) {
        $('#schDuration').val(parseFloat(e.StandardHours));
    } else {
        $('#schDuration').val('1');
    }

    // Force end recalculation now
    recompute();

    const rts = e.Id;
    const desc = e.Description;
    const allowed = e.StandardHours;

    $schJobChip.text((resources.job || 'Job') + ': ' + rts + ' — ' + desc);
    $schAllowedChip.text((resources.allowed || 'Allowed') + ': ' + (allowed || 0) + 'h');

    const modal = new bootstrap.Modal('#scheduleModal');
    const order = ["#schDate", "#schTech", "#schStart", "#schDuration", "#schEnd"];

    for (let i = 1; i < order.length; i++) {
        $(order[i]).prop("disabled", true);
    }

    order.forEach(x => $(x).off(".seq"));
    order.forEach((selector, i) => {
        $(selector).on("change.seq input.seq", function () {
            const filled = $(this).val()?.trim().length > 0;
            if (filled && order[i + 1]) $(order[i + 1]).prop("disabled", false);
        });
    });

    modal.show();
}

function DeleteService(e) {
    var data = {
        Id: parseInt(e.Id),
        WIPId: parseInt(e.WIPId),
    };

    $.ajax({
        type: 'POST',
        url: window.RazorVars.deleteServiceUrl,
        dataType: 'json',
        data: data
    }).done(function (result) {
        if (result && result.success) {
            const grid = $('#mainRTSGrid').dxDataGrid('instance');
            grid.refresh();
        }
    }).fail(function (xhr, status, error) {
        console.error("Error:", error);
    });
}

/* -------------------- Schedule modal helpers -------------------- */
const $schDate = $('#schDate');
const $schStart = $('#schStart');
const $schDuration = $('#schDuration');
const $schEnd = $('#schEnd');

const today = new Date();
$schDate.val(today.toISOString().slice(0, 10));

/* ========= 12-hour time helpers (Option B) ========= */
function pad2(n) { return String(n).padStart(2, '0'); }

/**
 * IMPORTANT FIX:
 * If #schEnd is <input type="time">, setting "3:15 PM" will be rejected silently.
 * So we force #schStart/#schEnd to be text inputs (and #schEnd readonly).
 */
function ensureScheduleInputsAreText() {
    const tStart = ($schStart.attr('type') || '').toLowerCase();
    const tEnd = ($schEnd.attr('type') || '').toLowerCase();

    if (tStart === 'time') $schStart.attr('type', 'text');
    if (tEnd === 'time') $schEnd.attr('type', 'text');

    // Make end read-only (it's calculated)
    $schEnd.prop('readonly', true);
}
ensureScheduleInputsAreText();
$(document).on('shown.bs.modal', '#scheduleModal', function () {
    ensureScheduleInputsAreText();
    // Recompute on show to keep UI consistent
    recompute();
});

// parses BOTH "HH:mm" and "h:mm AM/PM" (also tolerant of "HH:mm:ss" and "....T....")
function toMinutes(timeStr) {
    if (!timeStr) return NaN;

    let s = String(timeStr).trim();

    // strip date part if any: "2026-01-01T08:00:00"
    if (s.indexOf('T') >= 0) s = s.split('T')[1].trim();

    // detect AM/PM
    let ampm = null;
    const ampmMatch = s.match(/\b(AM|PM)\b/i);
    if (ampmMatch) {
        ampm = ampmMatch[1].toUpperCase();
        s = s.replace(/\b(AM|PM)\b/i, '').trim();
    }

    // take HH:mm (ignore seconds if present)
    const parts = s.split(':');
    if (parts.length < 2) return NaN;

    let h = parseInt(parts[0], 10);
    let m = parseInt(parts[1], 10);

    if (!isFinite(h) || !isFinite(m)) return NaN;

    if (ampm) {
        h = h % 12;              // 12 -> 0
        if (ampm === "PM") h += 12;
    }

    return h * 60 + m;
}

function minutesTo12(total) {
    total = ((total % 1440) + 1440) % 1440;
    const h24 = Math.floor(total / 60);
    const m = total % 60;

    const ampm = h24 >= 12 ? "PM" : "AM";
    let h12 = h24 % 12;
    if (h12 === 0) h12 = 12;

    return `${h12}:${pad2(m)} ${ampm}`;
}

function minutesTo24HHMM(total) {
    total = ((total % 1440) + 1440) % 1440;
    const h = Math.floor(total / 60);
    const m = total % 60;
    return `${pad2(h)}:${pad2(m)}`;
}

function parseDurationHours(raw) {
    const s = (raw ?? '').toString().trim().replace(',', '.');
    const num = parseFloat(s);
    return isFinite(num) ? num : NaN;
}

function normalizeDurationToMinutes(rawDuration) {
    const h = parseDurationHours(rawDuration);
    if (!isFinite(h) || h <= 0) return 0;
    return Math.round(h * 60);
}

// Accepts "08:00", "08:00:00", "8:00 AM", "2026-..T08:00:00" -> returns "8:00 AM"
function normalizeTo12Display(timeStr) {
    if (!timeStr) return "";
    const s = String(timeStr).trim();
    if (/\b(AM|PM)\b/i.test(s)) {
        return s.replace(/\s+/g, ' ').trim().toUpperCase();
    }
    return minutesTo12(toMinutes(s));
}

/**
 * FIXED recompute:
 * - Always updates #schEnd on duration change.
 * - Works even if inputs are replaced / type="time" existed (we force type text).
 */
function recompute() {
    ensureScheduleInputsAreText();

    const startVal = ($schStart.val() || '').trim();
    const startMin = toMinutes(startVal);

    const durHours = parseDurationHours($schDuration.val());
    if (!startVal || !isFinite(startMin) || !isFinite(durHours) || durHours <= 0) {
        $schEnd.val('');
        return;
    }

    const durationMin = Math.round(durHours * 60);
    const endMin = startMin + durationMin;

    // Display end in 12-hour format
    $schEnd.val(minutesTo12(endMin));

    // Also keep a 24-hour value around (handy for debugging / if you want to use it later)
    $schEnd.attr('data-end-24', minutesTo24HHMM(endMin));
}

/**
 * Bind schedule handlers robustly:
 * - Delegated DOM events (works even if modal content gets rerendered)
 * - DevExtreme widget hooks (dxNumberBox/dxTextBox) if schDuration is a widget
 */
function bindScheduleHandlers() {
    // Delegated DOM events
    $(document).off('.schedRecompute');
    $(document).on('input.schedRecompute change.schedRecompute keyup.schedRecompute', '#schStart', recompute);
    $(document).on('input.schedRecompute change.schedRecompute keyup.schedRecompute', '#schDuration', recompute);

    // If schDuration is a DevExtreme editor, hook onValueChanged too (doesn't hurt if it's not)
    try {
        const nb = $('#schDuration').dxNumberBox('instance');
        if (nb) {
            const prev = nb.option('onValueChanged');
            nb.option('onValueChanged', function (e) {
                if (typeof prev === 'function') prev.call(this, e);
                recompute();
            });
        }
    } catch (e) { }

    try {
        const tb = $('#schDuration').dxTextBox('instance');
        if (tb) {
            const prev = tb.option('onValueChanged');
            tb.option('onValueChanged', function (e) {
                if (typeof prev === 'function') prev.call(this, e);
                recompute();
            });
        }
    } catch (e) { }
}
bindScheduleHandlers();

// ---- Timepicker init helper (using jQuery DateTimePicker) ----
function initSchStartTimepicker(allowedTimes, defaultTime) {
    if (!$schStart.length) return;

    ensureScheduleInputsAreText();

    // destroy previous instance if any
    try { $schStart.datetimepicker('destroy'); } catch (e) { }

    const opts = {
        datepicker: false,
        format: 'g:i A',       // 12-hour in input
        formatTime: 'g:i A',   // 12-hour in dropdown
        hours12: true,         // safe if ignored by plugin build
        step: 5,
        scrollInput: false,
        onSelectTime: recompute,
        onChangeDateTime: recompute,
        onClose: recompute,
        allowTimes: allowedTimes,
        validateOnBlur: true,
        closeOnWithoutClick: true
    };

    if (Array.isArray(allowedTimes) && allowedTimes.length > 0) {
        opts.allowTimes = allowedTimes; // e.g. ["8:00 AM","8:05 AM",...]
    }

    $schStart.datetimepicker(opts);

    let val = "";
    if (defaultTime != null && String(defaultTime).trim() !== "") {
        val = normalizeTo12Display(defaultTime);
    } else if (allowedTimes && allowedTimes.length) {
        val = allowedTimes[0];
    }

    $schStart.val(val);
    recompute();
}

// initial timepicker (no restrictions yet)
initSchStartTimepicker([], "8:00 AM");

// Keep your existing basic validate/hide handler
$('#btnSaveSchedule').on('click', function () {
    if (!$('#schDate').val() || !$('#schTech').val() || !$('#schStart').val() || !$('#schDuration').val()) {
        Swal.fire(theMainLang == "en" ? resources.fill_required || 'Please fill required fields' : "الرجاء ملئ الحقول", "", "warining");
        return;
    }
    $('#scheduleModal').modal('hide');
});

$("#btnSaveSchedule").on("click", function (e) {
    // BULLETPROOF: compute end from start + duration (don’t rely on whatever is inside #schEnd)
    const startMin = toMinutes($('#schStart').val());
    const durationMin = normalizeDurationToMinutes($('#schDuration').val());
    const endMin = startMin + durationMin;

    const start24 = minutesTo24HHMM(startMin);
    const end24 = minutesTo24HHMM(endMin);

    // Keep UI end time aligned too
    $('#schEnd').val(minutesTo12(endMin)).attr('data-end-24', end24);

    var WIPSChedule = {
        WIPId: parseInt($('#Id').val()),
        RTSId: parseInt($('#RTSId').val()),
        KeyId: parseInt($('#KeyId').val()),
        TechnicianId: parseInt($('#schTech').val()),
        Date: new Date($('#schDate').val()),
        StartTime: start24 + ":00",
        Duration: durationMin,              // minutes
        EndTime: end24 + ":00"
    };

    var $selected = $("#schTech").find('option:selected');
    var freeIntervals = [];

    try {
        freeIntervals = JSON.parse($selected.attr('data-free-intervals') || '[]');
    } catch { }

    let valid = false;

    for (const interval of freeIntervals) {
        const s = toMinutes(interval.startFree); // likely "HH:mm" (24h) from backend
        const e = toMinutes(interval.endFree);

        if (startMin >= s && (startMin + durationMin) <= e) {
            valid = true;
            break;
        }
    }

    if (!valid) {
        Swal.fire(
            theMainLang == "en" ? "Invalid time selection" : "الوقت المختار خارج الشفت",
            theMainLang == "en"
                ? "Selected time exceeds technician shift."
                : "الوقت المختار يتجاوز نهاية الشفت.",
            "error"
        );
        return;
    }

    $.ajax({
        type: 'POST',
        url: window.RazorVars.wipScheduleUrl,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(WIPSChedule)
    }).done(function (result) {
        if (result && result.success) {
            const grid = $('#mainRTSGrid').dxDataGrid('instance');

            const store = grid.getDataSource().store();
            store.update(result.keyId, {
                Status: result.status,
                StatusText: "B-Booked",
                TechnicianId: parseInt($('#schTech').val())
            }).then(() => {
                grid.cancelEditData();
                return grid.getDataSource().reload();
            }).then(() => {
                grid.refresh(true);
            });

            updateTotalLabourFieldsFromGrid();
            Swal.fire(theMainLang == "en" ? 'Success' : "تمت العملية بنجاح", "", "success");
            evaluateAndUpdateWIPStatus();
        }
    }).fail(function (xhr, status, error) {
        console.error("Error:", error);
    });
});

//----------------------------------------------------------
$("#schDate").on("change", function () {
    var date = $("#schDate").val();
    var duration = parseFloat($('#schDuration').val()) || 0;
    var parsedDuration = duration;

    $.ajax({
        type: 'GET',
        url: window.URL.getAvailableTechnicians + `?date=${date}&duration=${parsedDuration}`,
        contentType: 'application/json',
        dataType: 'json',
        success: function (result) {
            var ddl = $("#schTech");
            ddl.empty();

            ddl.append(`<option value="">Select</option>`);

            result.data.forEach(function (item) {
                var opt = $(`<option></option>`)
                    .val(item.value)
                    .text(item.text);

                if (item.freeIntervalsList) {
                    opt.attr('data-free-intervals', JSON.stringify(item.freeIntervalsList));
                }

                ddl.append(opt);
            });

            ddl.trigger("change");
        }
    });
});

function maxFreeMinutes(freeIntervals) {
    if (!Array.isArray(freeIntervals) || !freeIntervals.length) return 0;
    let max = 0;
    freeIntervals.forEach(i => {
        const s = toMinutes(i.startFree);
        const e = toMinutes(i.endFree);
        if (e > s) max = Math.max(max, e - s);
    });
    return max;
}

$("#schTech").on("change", function () {
    var $selected = $(this).find('option:selected');
    var techId = $selected.val();

    if (!techId) {
        $("#schStart").val('');
        $("#schEnd").val('');
        initSchStartTimepicker([], null);
        return;
    }

    var freeIntervals = [];
    try {
        var json = $selected.attr('data-free-intervals') || '[]';
        freeIntervals = JSON.parse(json);
    } catch (e) {
        console.error("Failed to parse free intervals:", e);
        freeIntervals = [];
    }

    var durationMin = normalizeDurationToMinutes($('#schDuration').val());
    const maxMin = maxFreeMinutes(freeIntervals);

    if (durationMin <= 0) {
        initSchStartTimepicker([], null);
        return;
    }

    if (maxMin < durationMin) {
        initSchStartTimepicker([], null);
        $("#schStart").val('');
        $("#schEnd").val('');

        Swal.fire(
            theMainLang == "en" ? "No slot fits duration" : "لا توجد فترة زمنية تكفي للمدة المطلوبة",
            theMainLang == "en"
                ? `Max available: ${maxMin} min, required: ${durationMin} min`
                : `أقصى مدة متاحة: ${maxMin} دقيقة، المطلوبة: ${durationMin} دقيقة`,
            "warning"
        );
        return;
    }

    if (!freeIntervals.length || durationMin <= 0) {
        $("#schStart").val('');
        $("#schEnd").val('');
        initSchStartTimepicker([], null);
        return;
    }

    var options = computeStartOptionsEnumerate(freeIntervals, durationMin, 5);

    if (!options.length) {
        $("#schStart").val('');
        $("#schEnd").val('');
        initSchStartTimepicker([], null);
        return;
    }

    // choose default: previously saved start if still valid, else first option
    var defaultStart = options[0];
    if (scheduledStartHHMM && options.indexOf(scheduledStartHHMM) !== -1) {
        defaultStart = scheduledStartHHMM;
    }

    initSchStartTimepicker(options, defaultStart);

    // IMPORTANT: no manual end-time set here; let recompute() do it consistently
    recompute();
});

function computeStartOptionsEnumerate(freeIntervals, durationMin, stepMin = 5) {
    if (!Array.isArray(freeIntervals) || durationMin <= 0) return [];

    const ranges = freeIntervals
        .map(i => [toMinutes(i.startFree), toMinutes(i.endFree)]) // backend likely 24h
        .filter(([s, e]) => Number.isFinite(s) && Number.isFinite(e) && e > s)
        .sort((a, b) => a[0] - b[0]);

    const merged = [];
    for (const [s, e] of ranges) {
        if (!merged.length || s > merged[merged.length - 1][1]) {
            merged.push([s, e]);
        } else {
            merged[merged.length - 1][1] = Math.max(merged[merged.length - 1][1], e);
        }
    }

    const out = [];
    for (const [s, e] of merged) {
        for (let t = s; t + durationMin <= e; t += stepMin) {
            out.push(minutesTo12(t)); // 12-hour display in dropdown
        }
    }

    return [...new Set(out)].sort((a, b) => toMinutes(a) - toMinutes(b));
}

//----------------------------------------------------------

function updateStatusInGrid(KeyId, newStatus, statusText) {
    const grid = $('#mainRTSGrid').dxDataGrid('instance');

    var data = grid.option("dataSource");
    if (Array.isArray(data)) {
        const target = data.find(x => x.KeyId === KeyId);
        if (target) {
            target.Status = newStatus;
            target.StatusText = statusText;
        }
    }

    const rowIndex = grid.getRowIndexByKey(KeyId);
    if (rowIndex >= 0) {
        grid.cellValue(rowIndex, "Status", newStatus);
        grid.cellValue(rowIndex, "StatusText", statusText);
    }

    grid.saveEditData();
    grid.refresh();

    $.ajax({
        type: 'POST',
        url: window.RazorVars.updateServiceStatusUrl,
        contentType: 'application/json',
        data: JSON.stringify({
            WIPId: $('#Id').val(),
            RTSId: RTSId,
            Status: newStatus
        }),
        success: function (res) {
            console.log("Status updated successfully:", res);
        },
        error: function (err) {
            console.error("Error updating status:", err);
        }
    });
}

function GetVatValueById(vatId) {
    var vatValue = 0;
    $.ajax({
        url: window.RazorVars.getVatValueByIdUrl,
        method: 'Get',
        dataType: 'json',
        data: { VatId: vatId },
        async: false,
        success: function (result) {
            vatValue = result;
        },
        error: function () {
            vatValue = 0;
        }
    });
    return vatValue;
}

function ensureDiscountedRate(rowData) {
    const discount = getEffectiveLabourDiscountPct(rowData);

    let rate = parseFloat(rowData.Rate) || 0;
    let base = parseFloat(rowData.BaseRate);

    if (!isFinite(base) || base <= 0) {
        rowData.BaseRate = rate;
        base = rowData.BaseRate;
    }

    const alreadyDiscounted = Math.abs((rate || 0) - (base || 0)) > 0.0001;
    if (alreadyDiscounted) {
        return +rate.toFixed(2);
    }

    const discounted = base - (base * (discount / 100));
    return discounted;
}

$("#Vat, #PartialVat").on("change", function () {
    const grid = $("#mainRTSGrid").dxDataGrid("instance");
    if (!grid) return;

    const rows = grid.getVisibleRows() || [];

    rows.forEach((r) => {
        const d = r.data || {};
        const accType = parseInt(d.AccountType) || 0;

        if (accType === 2) {
            const vatId = getEffectiveVatId(d);
            const vatValue = parseFloat(GetVatValueById(vatId)) || 0;
            const vatPercent = vatValue > 1 ? vatValue / 100 : vatValue;

            const rate = ensureDiscountedRate(d);
            const hours = parseFloat(d.StandardHours) || 0;

            const newTax = +(hours * rate * vatPercent).toFixed(2);
            grid.cellValue(r.rowIndex, "Tax", newTax);
            d.Tax = newTax;
        } else {
            grid.cellValue(r.rowIndex, "Tax", 0);
            d.Tax = 0;
        }
    });

    grid.saveEditData();
    grid.refresh();
    updateTotalLabourFieldsFromGrid();
});

function getRateAmount(keyId, RTSId, rowAccountType) {
    pendingRateCalls++;
    setSaveBusy(true);
    debugger
    const grid = $('#mainRTSGrid').dxDataGrid('instance');

    const effectiveAccountType = (rowAccountType != null && rowAccountType !== "")
        ? parseInt(rowAccountType)
        : parseInt($('#AccountType').val());

    const _data = grid.option("dataSource") || [];
    const _target = _data.find(r => r.KeyId === keyId);
    if (!_target) return;

    var model = {
        CustomerId: getEffectiveCustomerId(_target),
        RTSId: parseInt(RTSId),
        WIPId: $('#Id').val(),
        AccountType: effectiveAccountType,
        SalesType: parseInt($('#SalesType').val())
    };

    $.ajax({
        type: 'POST',
        url: window.RazorVars.getLabourRateUrl,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(model)
    }).then(function (result) {
        if (result == null || !grid) return;

        const data = grid.option("dataSource") || [];
        const target = data.find(r => r.KeyId === keyId);
        if (!target) return;

        const hours = parseFloat(target.StandardHours) || 0;
        const total = +(result * hours).toFixed(2);

        target.BaseRate = result;
        target.Rate = result;
        target.Total = total;

        const rowIndex = grid.getRowIndexByKey(keyId);

        grid.beginUpdate();
        try {
            if (rowIndex >= 0) {
                grid.cellValue(rowIndex, "BaseRate", target.BaseRate);
                grid.cellValue(rowIndex, "Rate", target.Rate);
                grid.cellValue(rowIndex, "Total", target.Total);
            }
        } finally {
            grid.endUpdate();
        }

        const ds = grid.getDataSource();
        const reloadPromise = ds ? ds.reload() : $.Deferred().resolve().promise();

        return reloadPromise
            .then(() => grid.saveEditData())
            .then(() => grid.refresh(true))
            .then(() => waitForGridIdle(grid));

    }).always(function () {
        pendingRateCalls--;
        if (pendingRateCalls <= 0) {
            pendingRateCalls = 0;
            setSaveBusy(false);
        }
    });
}

function isRowExternal(rowData) {
    return !!(rowData && (rowData.IsExternal === true || rowData.IsExternal === 1 || rowData.External === true));
}

function getEffectiveCustomerId(rowData) {
    const main = parseInt($("#CustomerId").val()) || 0;
    const partial = parseInt($("#PartialCustomerId").val()) || 0;

    return isRowExternal(rowData) ? (partial || main) : (main || partial);
}

function getEffectiveVatId(rowData) {
    const mainVat = parseInt($("#Vat").val()) || 0;
    const partialVat = parseInt($("#PartialVat").val()) || 0;

    return isRowExternal(rowData) ? (partialVat || mainVat) : (mainVat || partialVat);
}

function getEffectiveLabourDiscountPct(rowData) {
    const main = parseFloat($("#_DiscountPercentageLabor").val()) || 0;
    const partial = parseFloat($("#_DiscountPercentageLaborPartial").val()) || 0;

    return isRowExternal(rowData) ? (partial || main) : (main || partial);
}