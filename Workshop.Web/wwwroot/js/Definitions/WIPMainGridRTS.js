// Grid ======================================================

let scheduleIsMenu = false;
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
            { dataField: "ISMenu", caption: "ISMenu", visible: false, dataType: "boolean", },
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
                dataField: "Discount",
                caption: window.RazorVars.DXDiscount,
                dataType: "number",
                allowEditing: false,
                visible: false,
                defaultValue: 0
            },
            {
                dataField: "DiscountPct",
                caption: window.RazorVars.DXDiscount,
                dataType: "number", allowEditing: Permission_AddDiscount,
                alignment: "left",
                editorOptions: {
                    min: 0,
                    max: 100
                },
                calculateCellValue: function (rowData) {
                    const hours = parseFloat(rowData.StandardHours) || 0;
                    const rate = parseFloat(rowData.Rate) || ensureDiscountedRate(rowData) || 0;
                    const base = +(hours * rate).toFixed(4);

                    const discAmt = +((Number(rowData.Discount) || 0).toFixed(5));
                    if (base === 0) {
                        rowData.DiscountPct = 0;
                        return 0;
                    }

                    const pct = (discAmt / base) * 100;
                    rowData.DiscountPct = pct;
                    return pct;
                },
                setCellValue: function (newData, value, currentRowData) {
                    const pct = +value || 0;
                    const hours = parseFloat(currentRowData.StandardHours) || 0;
                    const rate = parseFloat(currentRowData.Rate) || ensureDiscountedRate(currentRowData) || 0;
                    const base = +(hours * rate).toFixed(4);

                    newData.Discount = base * (pct / 100);
                    newData.DiscountPct = pct;
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
                    var rate = parseFloat(rowData.Rate) || ensureDiscountedRate(rowData) || 0;
                    var discAmt = parseFloat(rowData.Discount) || 0;

                    var taxable = (hours * rate) - discAmt;
                    if (taxable < 0) taxable = 0;

                    var taxAmount = taxable * vatPercent;

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
                    var rate = parseFloat(rowData.Rate) || ensureDiscountedRate(rowData) || 0;
                    var hours = parseFloat(rowData.StandardHours) || 0;
                    var tax = parseFloat(rowData.Tax) || 0;
                    var discAmt = parseFloat(rowData.Discount) || 0;

                    var totalValue = (hours * rate) + tax;

                    if (discAmt > 0) {
                        totalValue -= discAmt;
                        if (totalValue < 0) totalValue = 0;
                    }

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
                            return !(wipStatus === Complete || wipStatus === Invoiced) &&
                                e.row.data.Status != 1 && e.row.data.Status != 20 && //&& e.row.data.Status === 23;
                                //parseInt(e.row.data.Status) !== 24 &&
                                parseInt(e.row.data.Status) !== 26;
                        },
                        onClick: function (e) {
                            console.log(e.row.data.Id);
                            const row = e.row.data;

                            $("#RTSId").val(row.Id);
                            scheduleIsMenu = row.ISMenu === true || row.ISMenu === 1;
                            openScheduleModal(row);
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
        //columnMinWidth: 50,if the comment removed, make the columnAutoWidth
        //wordWrapEnabled: false,
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

            const rowIndex = e.component.getRowIndexByKey(e.key);

            if (rowIndex >= 0) {
                e.component.cellValue(rowIndex, "Rate", ensureDiscountedRate(e.data));
            }

            if (!partialInvoicing) {
                e.data.AccountType = accountTypeVal;
                store.update(e.key, e.data).then(() => grid.refresh());
            }

            e.component.refresh();
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

        const status = parseInt(d.Status) || 0;
        if (status === 24 || status === 26) {
            return;
        }

        const rate = ensureDiscountedRate(d);
        const hours = parseFloat(d.StandardHours) || 0;
        //const pct = parseFloat(d.Discount) || 0;
        const tax = parseFloat(d.Tax) || 0;

        const lineBase = rate * hours;
        //const lineDisc = lineBase * (pct / 100);

        const lineDisc = parseFloat(d.Discount) || 0;
        const lineAfterDiscount = lineBase - lineDisc;
        const lineTotal = lineAfterDiscount + tax;

        totalBase += lineBase;
        totalDiscountAmount += lineDisc;
        totalTaxAmount += tax;
        //totalAfterDiscount += lineTotal;
        totalAfterDiscount += lineAfterDiscount;
    });

    const totalDiscountPct = totalBase > 0 ? (totalDiscountAmount / totalBase) * 100 : 0;

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
let scheduledStartHHMM = null;

function openScheduleModal(e) {
    console.log("//Start ////////////////////");
    console.log(e);
    console.log(e.StandardHours);
    console.log(e.StandardHours * 60);
    console.log("// End ////////////////////");
    const $tr = $(this).closest('tr');
    const rtsId = e.Id;
    const KeyId = e.KeyId;
    const wipId = e.WIPId;
    const keyId = e.KeyId;
    scheduledStartHHMM = null;

    // reset fields
    $('#schDate, #schTech, #schStart, #schDuration, #schEnd, #KeyId').val('');
    $("#KeyId").val(KeyId);

    // default date = today
    const todayStr = new Date().toISOString().slice(0, 10);
    // default start = 08:00, no allowed times yet
    initSchStartTimepicker([], "08:00");

    $('#schDate').val(todayStr).trigger('change');

    $('table tr').removeClass('selected-row');
    $tr.addClass('selected-row');

    //$.get(window.RazorVars.scheduleGetByIdUrl, { RTSId: rtsId, WIPId: wipId, KeyId:keyId }, function (data) {
    //    if (data) {
    //        // handle date, ignore 0001-01-01T00:00:00
    //        if (data.date && !data.date.startsWith("0001-01-01")) {
    //            $('#schDate').val(data.date.split('T')[0]).trigger('change');
    //        }
    //
    //        if (data.technicianId) {
    //            $('#schTech').val(data.technicianId).trigger('change');
    //        }
    //
    //        if (data.startTime) {
    //            // normalize to HH:mm
    //            let timeStr = data.startTime;
    //            if (timeStr.indexOf('T') >= 0) {
    //                timeStr = timeStr.split('T')[1];
    //            }
    //            const parts = timeStr.split(':');
    //            const hh = (parts[0] || "00").padStart(2, '0');
    //            const mm = (parts[1] || "00").padStart(2, '0');
    //            scheduledStartHHMM = `${hh}:${mm}`;
    //            $('#schStart').val(hhmm24To12(scheduledStartHHMM)); // ✅ display 12h
    //        }
    //
    //        if (data.duration && data.duration > 0) {
    //            $('#schDuration').val(data.duration);
    //        }
    //
    //        recompute();
    //    }
    //});

    if (e.StandardHours != null && e.StandardHours !== undefined && e.StandardHours > 0) {
        $('#schDuration').val(parseFloat(e.StandardHours));
    } else {
        $('#schDuration').val('1');
    }
    recompute();

    const rts = e.Id;/*($tr.children().eq(0).text() || '').trim();*/
    const desc = e.Description;/* ($tr.children().eq(1).text() || '').trim();*/
    const allowTxt = ($tr.children().eq(2).text() || '').trim();
    const allowed = e.StandardHours;//parseInt((allowTxt.match(/(\d+)\s*m/i) || [])[1] || '0', 10);

    $schJobChip.text((resources.job || 'Job') + ': ' + rts + ' — ' + desc);
    $schAllowedChip.text((resources.allowed || 'Allowed') + ': ' + (allowed || 0) + 'h');

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

function pad2(n) {
    return n.toString().padStart(2, '0');
}

/* ===================== 12H StartTime helpers (NEW) ===================== */
// يقبل: "08:00" / "8:00 AM" / "08:00 PM" / "08:00:00" ... ويرجع دقائق
function parseTimeToMinutes(val) {
    if (!val) return null;
    val = String(val).trim();

    const m = val.match(/^(\d{1,2})\s*:\s*(\d{2})(?:\s*:\s*\d{2})?\s*([AP]M)?$/i);
    if (!m) return null;

    let hh = parseInt(m[1], 10);
    const mm = parseInt(m[2], 10);
    const ap = (m[3] || "").toUpperCase();

    if (ap) {
        // 12-hour -> 24-hour
        if (hh < 1 || hh > 12) return null;
        if (hh === 12) hh = 0;
        if (ap === "PM") hh += 12;
    } else {
        // 24-hour
        if (hh < 0 || hh > 23) return null;
    }

    if (mm < 0 || mm > 59) return null;

    return hh * 60 + mm;
}

function minutesTo12H(totalMinutes) {
    totalMinutes = ((totalMinutes % 1440) + 1440) % 1440;
    const hh24 = Math.floor(totalMinutes / 60);
    const mm = totalMinutes % 60;

    const ap = hh24 >= 12 ? "PM" : "AM";
    let hh12 = hh24 % 12;
    if (hh12 === 0) hh12 = 12;

    return `${hh12}:${pad2(mm)} ${ap}`;
}

function hhmm24To12(hhmm24) {
    const m = parseTimeToMinutes(hhmm24);
    if (m == null) return "";
    return minutesTo12H(m);
}

// يرجع HH:mm (24h) مهما كانت قيمة #schStart (12h/24h)
function getSchStart24() {
    const m = parseTimeToMinutes($schStart.val());
    if (m == null) return "";
    return minutesToHHMM(m);
}
/* ===================================================================== */

function mins(hm) {
    const m = parseTimeToMinutes(hm);
    return m == null ? 0 : m;
}
function fromM(n) {
    n = ((n % 1440) + 1440) % 1440;
    const h = String(Math.floor(n / 60)).padStart(2, '0');
    const m = String(n % 60).padStart(2, '0');
    return `${h}:${m}`;
}

// ✅ recompute صار يتعامل مع 12h بدون ما يخرب الـ EndTime
function recompute() {
    const sMin = parseTimeToMinutes($schStart.val());
    const hours = parseFloat($schDuration.val() || '0');
    const dMin = Math.round((isFinite(hours) ? hours : 0) * 60);

    if (sMin == null || dMin <= 0) {
        $schEnd.val('');
        return;
    }

    $schEnd.val(fromM(sMin + dMin)); // EndTime يظل 24h
}

// recompute end time when user changes start or duration
$schStart.on('change input', recompute);
$schDuration.on('change input', recompute);
$schStart.off('change.startTime');

// ---- Timepicker init helper (using jQuery DateTimePicker) ----
// ✅ StartTime صار 12-hour display (AM/PM) + كل اللوجيك يضل 24-hour داخليًا
function initSchStartTimepicker(allowedTimes, defaultTime) {
    if (!$schStart.length) return;

    try { $schStart.datetimepicker('destroy'); } catch (e) { }

    const allowed24 = Array.isArray(allowedTimes) ? allowedTimes : []; // ["07:00","07:05",...]

    $schStart.datetimepicker({
        datepicker: false,
        format: 'h:i A',       
        formatTime: 'h:i A',   
        step: 5,
        scrollInput: false,
        scrollTime: false,     
        validateOnBlur: false,
        closeOnWithoutClick: true,
        allowTimes: allowed24, 
        onClose: function () { recompute(); }
    });

    const val24 = defaultTime || allowed24[0] || "08:00";
    $schStart.val(hhmm24To12(val24));

    recompute();
}

// initial timepicker (no restrictions yet)
initSchStartTimepicker([], "08:00");

$('#btnSaveSchedule').on('click', function () {
    if (!$('#schDate').val() || !$('#schTech').val() || !$('#schStart').val() || !$('#schDuration').val()) {
        Swal.fire(theMainLang == "en" ? resources.fill_required || 'Please fill required fields' : "الرجاء ملئ الحقول", "", "warining");
        return;
    }
    $('#scheduleModal').modal('hide');
});

$("#btnSaveSchedule").on("click", function (e) {
    var WIPSChedule = {
        WIPId: parseInt($('#Id').val()),
        RTSId: parseInt($('#RTSId').val()),
        KeyId: parseInt($('#KeyId').val()),
        TechnicianId: parseInt($('#schTech').val()),
        Date: new Date($('#schDate').val()),
        StartTime: getSchStart24() + ":00",         // ✅ حفظ 24h للسيرفر
        //Duration: parseFloat($('#schDuration').val()),//For Hours
        Duration: Math.round(parseFloat($('#schDuration').val()) * 60), //For Minutes
        EndTime: $('#schEnd').val() + ":00",
        ISMenu: scheduleIsMenu
    };

    // ✅ startMin صار يفهم 12h مباشرة
    var startMin = parseTimeToMinutes($('#schStart').val()) || 0;
    var endMin = toMinutes($('#schEnd').val());
    var durationMin = normalizeDurationToMinutes($('#schDuration').val());

    var $selected = $("#schTech").find('option:selected');
    var freeIntervals = [];

    try {
        freeIntervals = JSON.parse($selected.attr('data-free-intervals') || '[]');
    } catch { }

    let valid = false;

    for (const interval of freeIntervals) {
        const s = toMinutes(interval.startFree);
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
    var parsedDuration = duration;// / 60;

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

    const formatHours = (minutes) => {
        const hours = minutes / 60;
        return Number.isInteger(hours)
            ? hours.toString()
            : hours.toFixed(1).replace(/\.0$/, '');
    };

    if (durationMin <= 0) {
        initSchStartTimepicker([], null);
        return;
    }

    if (maxMin < durationMin) {
        initSchStartTimepicker([], null);
        $("#schStart").val('');
        $("#schEnd").val('');

        Swal.fire(
            theMainLang == "en" ? "No suitable time slot" : "لا توجد فترة مناسبة",
            theMainLang == "en"
                ? `Available: ${formatHours(maxMin)} hours, required: ${formatHours(durationMin)} hours`
                : `المتاح: ${formatHours(maxMin)} ساعة، المطلوب: ${formatHours(durationMin)} ساعة`,
            "warning"
        );
        return;
    }

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

function toMinutes(hhmm) {
    const m = parseTimeToMinutes(hhmm);
    return m == null ? 0 : m;
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
    return Math.round(num * 60);
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
            out.push(minutesToHHMM(t)); // يظل 24h داخليًا
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
//function ensureDiscountedRate(rowData) {
//    //const discount = parseFloat($("#_DiscountPercentageLabor").val()) || 0;
//    const discount = getEffectiveLabourDiscountPct(rowData);

//    let rate = parseFloat(rowData.Rate) || 0;
//    let base = parseFloat(rowData.BaseRate);

//    if (!isFinite(base) || base <= 0) {
//        rowData.BaseRate = rate;
//        base = rowData.BaseRate;
//    }

//    const alreadyDiscounted = Math.abs((rate || 0) - (base || 0)) > 0.0001;
//    if (alreadyDiscounted) {
//        return +rate.toFixed(2);
//    }

//    const discounted = base - (base * (discount / 100));
//    //rowData.Rate = discounted.toFixed(2);
//    //return rowData.Rate;
//    return discounted;
//}

function ensureDiscountedRate(rowData) {

    const discount = getEffectiveLabourDiscountPct(rowData);

    const base = parseFloat(rowData.BaseRate) || 0;

    const discounted = base - (base * (discount / 100));

    return +discounted.toFixed(2);
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
        debugger
        const hours = parseFloat(target.StandardHours) || 0;
        //const total = +(result * hours).toFixed(2);

        target.BaseRate = result;
        target.Rate = ensureDiscountedRate(target);
        target.Total = +(target.Rate * hours).toFixed(2);

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