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


    $("#mainRTSGrid").dxDataGrid({
        dataSource: ServicesData,
        keyExpr: "Id",
        noDataText: resources.NoDataInTable,
        showBorders: true,
        validateOnValueChange: false,
        remoteOperations: {
            filtering: true,
            sorting: true,
            paging: true
        },
        columns: [
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
            { dataField: "Description", caption: window.RazorVars.DXName, allowEditing: true },
            {
                dataField: "LongDescription",
                caption: window.RazorVars.DXLongDescription,
                allowEditing: false,
                width: 300,
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
            { dataField: "StandardHours", dataType: "number", caption: window.RazorVars.DXStandardHours, allowEditing: true, alignment: "left" },
            { dataField: "BaseRate", visible: false, allowEditing: false },
            { dataField: "Rate",
                caption: window.RazorVars.DXRate,
                dataType: "number",
                allowEditing: false,
                calculateCellValue: function (rowData) {
                    return ensureDiscountedRate(rowData);
                }
            },
            {
                dataField: "Tax",
                caption: window.RazorVars.DXTax,
                dataType: "number",
                allowEditing: false,
                alignment: "left",
                calculateCellValue: function (rowData) {
                    var vatId = $("#Vat").val();
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
                dataField: "Discount", caption: window.RazorVars.DXDiscount, dataType: "number", allowEditing: true, alignment: "left",
                editorOptions: {
                    min: 0,
                    max: 100
                }
            },
            { dataField: "TimeTaken", caption: window.RazorVars.DXTimeTaken, dataType: "number", allowEditing: false, alignment: "left" },
            { dataField: "Status", caption: "Status", dataType: "number", visible: false, alignment: "left" },
            {
                dataField: "StatusText", caption: window.RazorVars.DXStatus, allowEditing: false, alignment: "left",
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
            //{
            //    dataField: "AccountType", caption: window.RazorVars.DXAccountType, dataType: "number", allowEditing: true,
            //    lookup: {
            //        dataSource: AccountTypes.map(x => ({
            //            Value: parseInt(x.Value),
            //            Text: x.Text
            //        })),
            //        valueExpr: "Value",
            //        displayExpr: "Text"
            //    },
            //    calculateCellValue: function (rowData) {
            //        if (!rowData.AccountType) {
            //            rowData.AccountType = 1;
            //        }
            //        return rowData.AccountType;
            //    }
            //},
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
                    } else if (partialInvoicing) {

                        rowData.AccountType = null;
                    }

                    return rowData.AccountType;
                }
            },
            {
                type: "buttons",
                width: 110,
                buttons: [
                    {
                        hint: "Add",
                        icon: "fad fa-regular fa-user act-booking",
                        visible: function (e) {
                            return !(wipStatus === Gone || wipStatus === Invoiced) && e.row.data.Status === 23;
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
                        visible: function () {
                            return !(wipStatus === Gone || wipStatus === Invoiced);
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


    });
});

function updateTotalLabourFieldsFromGrid() {
    const grid = $("#mainRTSGrid").dxDataGrid("instance");
    if (!grid) return;

    const rows = grid.getVisibleRows() || [];

    let totalAfterDiscount = 0;
    let totalDiscountAmount = 0;
    debugger
    rows.forEach(r => {
        const d = r.data || {};
        const rate = parseFloat(d.Rate) || 0;
        debugger
        const hours = parseFloat(d.StandardHours) || 0;
        const pct = parseFloat(d.Discount) || 0;

        const lineBase = rate * hours;
        const lineDisc = lineBase * (pct / 100);
        const lineTotal = lineBase - lineDisc;

        totalDiscountAmount += lineDisc;
        totalAfterDiscount += lineTotal;

        d.Total = +lineTotal.toFixed(2);
    });

    $("#totLabour").text("SAR " + totalAfterDiscount.toFixed(2));
    setAmount("#totLabour", totalAfterDiscount);
    $("#TotalDiscountsLabour").text("SAR " + totalDiscountAmount.toFixed(2));
    updateSubtotal();
}

//============================================================================
// open booking from actions

const $schJobChip = $('#schJobChip');
const $schAllowedChip = $('#schAllowedChip');

// to remember existing scheduled time (if editing)
let scheduledStartHHMM = null;

function openScheduleModal(e) {
    console.log("//Start ////////////////////");
    console.log(e);
    console.log(e.StandardHours);
    console.log(e.StandardHours * 60);
    console.log("// End ////////////////////");
    const $tr = $(this).closest('tr');
    const rtsId = e.Id;
    const wipId = e.WIPId;

    scheduledStartHHMM = null;

    // reset fields
    $('#schDate, #schTech, #schStart, #schDuration, #schEnd').val('');

    // default date = today
    const todayStr = new Date().toISOString().slice(0, 10);
    $('#schDate').val(todayStr);

    // default start = 08:00, no allowed times yet
    initSchStartTimepicker([], "08:00");

    $('table tr').removeClass('selected-row');
    $tr.addClass('selected-row');

    $.get(window.RazorVars.scheduleGetByIdUrl, { RTSId: rtsId, WIPId: wipId }, function (data) {
        if (data) {
            // handle date, ignore 0001-01-01T00:00:00
            if (data.date && !data.date.startsWith("0001-01-01")) {
                $('#schDate').val(data.date.split('T')[0]);
            }

            if (data.technicianId) {
                $('#schTech').val(data.technicianId);
            }

            if (data.startTime) {
                // normalize to HH:mm
                let timeStr = data.startTime;
                if (timeStr.indexOf('T') >= 0) {
                    timeStr = timeStr.split('T')[1];
                }
                const parts = timeStr.split(':');
                const hh = (parts[0] || "00").padStart(2, '0');
                const mm = (parts[1] || "00").padStart(2, '0');
                scheduledStartHHMM = `${hh}:${mm}`;
                $('#schStart').val(scheduledStartHHMM);
            }

            if (data.duration && data.duration > 0) {
                $('#schDuration').val(data.duration);
            }

            recompute();
        }
    });

    if (e.StandardHours != null && e.StandardHours !== undefined && e.StandardHours > 0) {
        $('#schDuration').val(parseFloat(e.StandardHours * 60));
    } else {
        $('#schDuration').val('30');
    }
    recompute();

    const rts = ($tr.children().eq(0).text() || '').trim();
    const desc = ($tr.children().eq(1).text() || '').trim();
    const allowTxt = ($tr.children().eq(2).text() || '').trim();
    const allowed = parseInt((allowTxt.match(/(\d+)\s*m/i) || [])[1] || '0', 10);

    $schJobChip.text((resources.job || 'Job') + ': ' + rts + ' — ' + desc);
    $schAllowedChip.text((resources.allowed || 'Allowed') + ': ' + (allowed || 0) + 'm');

    const modal = new bootstrap.Modal('#scheduleModal');
    const order = [
        "#schDate",
        "#schTech",
        "#schStart",
        "#schDuration",
        "#schEnd"
    ];


    for (let i = 1; i < order.length; i++) {
        $(order[i]).prop("disabled", true);
    }


    order.forEach(x => $(x).off(".seq"));

    order.forEach((selector, i) => {
        $(selector).on("change.seq input.seq", function () {
            const filled = $(this).val()?.trim().length > 0;
            if (filled && order[i + 1]) {
                $(order[i + 1]).prop("disabled", false);
            }
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

function mins(hm) {
    if (!hm) return 0;
    const parts = hm.split(':').map(Number);
    const h = parts[0] || 0;
    const m = parts[1] || 0;
    return h * 60 + m;
}
function fromM(n) {
    n = ((n % 1440) + 1440) % 1440;
    const h = String(Math.floor(n / 60)).padStart(2, '0');
    const m = String(n % 60).padStart(2, '0');
    return `${h}:${m}`;
}
function recompute() {
    const s = mins($schStart.val());
    const d = parseInt($schDuration.val() || '0', 10);
    $schEnd.val(s && d ? fromM(s + d) : '');
}

// recompute end time when user changes start or duration
$schStart.on('change input', recompute);
$schDuration.on('change input', recompute);
$schStart.off('change.startTime');

// ---- Timepicker init helper (using jQuery DateTimePicker) ----
function initSchStartTimepicker(allowedTimes, defaultTime) {
    if (!$schStart.length) return;

    // destroy previous instance if any
    try {
        $schStart.datetimepicker('destroy');
    } catch (e) { }

    const opts = {
        datepicker: false,
        format: 'H:i',
        step: 5,
        scrollInput: false,
        onSelectTime: function () {
            recompute();
        },
        onChangeDateTime: function () {
            recompute();
        }
    };

    if (Array.isArray(allowedTimes) && allowedTimes.length > 0) {
        opts.allowTimes = allowedTimes; // e.g. ["08:00","08:05",...]
    }

    $schStart.datetimepicker(opts);

    const val = defaultTime || (allowedTimes && allowedTimes[0]) || "08:00";
    $schStart.val(val);
    recompute();
}

// initial timepicker (no restrictions yet)
initSchStartTimepicker([], "08:00");

$('#btnSaveSchedule').on('click', function () {
    if (!$('#schDate').val() || !$('#schTech').val() || !$('#schStart').val() || !$('#schDuration').val()) {
        alert(resources.fill_required || 'Please fill required fields');
        return;
    }
    $('#scheduleModal').modal('hide');
});

$("#btnSaveSchedule").on("click", function () {
    var WIPSChedule = {
        WIPId: parseInt($('#Id').val()),
        RTSId: parseInt($('#RTSId').val()),
        TechnicianId: parseInt($('#schTech').val()),
        Date: new Date($('#schDate').val()),
        StartTime: $('#schStart').val() + ":00",
        Duration: parseFloat($('#schDuration').val()),
        EndTime: $('#schEnd').val() + ":00"
    };

    $.ajax({
        type: 'POST',
        url: window.RazorVars.wipScheduleUrl,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(WIPSChedule)
    }).done(function (result) {
        if (result && result.success) {
            const grid = $('#mainRTSGrid').dxDataGrid('instance');
            const rowIndex = grid.getRowIndexByKey(result.rtsId);
            if (rowIndex >= 0) {
                var data = grid.option("dataSource");
                const target = data.find(x => x.Id === result.rtsId);
                if (target) {
                    target.Status = result.status;
                    target.StatusText = "Booked";
                }

                //getRate(WIPSChedule.RTSId, WIPSChedule.TechnicianId);
                grid.refresh();
            }
            updateTotalLabourFieldsFromGrid();
            Swal.fire("Success", "The schedule was saved successfully", "success");
        }
    }).fail(function (xhr, status, error) {
        console.error("Error:", error);
    });
});

//----------------------------------------------------------
$("#schDate").on("change", function () {
    var date = $("#schDate").val();
    var duration = parseFloat($('#schDuration').val()) || 0;
    var parsedDuration = duration / 60;

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
    var $schEndLocal = $("#schEnd");

    if (!freeIntervals.length || durationMin <= 0) {
        $("#schStart").val('');
        $schEndLocal.val('');
        initSchStartTimepicker([], null);
        return;
    }

    var options = computeStartOptionsEnumerate(freeIntervals, durationMin, 5);

    if (!options.length) {
        $("#schStart").val('');
        $schEndLocal.val('');
        initSchStartTimepicker([], null);
        return;
    }

    // choose default: previously saved start if still valid, else first option
    var defaultStart = options[0];
    if (scheduledStartHHMM && options.indexOf(scheduledStartHHMM) !== -1) {
        defaultStart = scheduledStartHHMM;
    }

    initSchStartTimepicker(options, defaultStart);

    var startMinutes = toMinutes(defaultStart);
    var endHHMM = minutesToHHMM(startMinutes + durationMin);
    $schEndLocal.val(endHHMM);
});

function pad2(n) {
    return n.toString().padStart(2, '0');
}

function toMinutes(hhmm) {
    if (!hhmm) return 0;
    const parts = hhmm.split(':').map(x => parseInt(x, 10) || 0);
    const h = parts[0] || 0;
    const m = parts[1] || 0;
    return h * 60 + m;
}

function minutesToHHMM(total) {
    total = Math.max(0, total);
    const h = Math.floor(total / 60) % 24;
    const m = total % 60;
    return pad2(h) + ':' + pad2(m);
}

function normalizeDurationToMinutes(rawDuration) {
    const num = parseFloat(rawDuration || 0);
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
        if (!merged.length || s > merged[merged.length - 1][1]) {
            merged.push([s, e]);
        } else {
            merged[merged.length - 1][1] = Math.max(merged[merged.length - 1][1], e);
        }
    }

    const out = [];
    for (const [s, e] of merged) {
        for (let t = s; t + durationMin <= e; t += stepMin) {
            out.push(minutesToHHMM(t));
        }
    }

    return [...new Set(out)].sort((a, b) => toMinutes(a) - toMinutes(b));
}

//----------------------------------------------------------

function updateStatusInGrid(RTSId, newStatus, statusText) {
    const grid = $('#mainRTSGrid').dxDataGrid('instance');

    var data = grid.option("dataSource");
    if (Array.isArray(data)) {
        const target = data.find(x => x.Id === RTSId);
        if (target) {
            target.Status = newStatus;
            target.StatusText = statusText;
        }
    }

    const rowIndex = grid.getRowIndexByKey(RTSId);
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
    const discount = parseFloat($("#_DiscountPercentageLabor").val()) || 0;

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
    rowData.Rate = +discounted.toFixed(2);
    return rowData.Rate;
}
