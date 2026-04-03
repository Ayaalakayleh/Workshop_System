window.transferLocatorsCache = [];
const PriceWorkflowStatusEnum = {
    None: 0,
    Pending: 1,
    Approved: 2,
    Rejected: 3,
    Failed: 9
};

let _tmpKeyId = -1; 
function ensureKeyId(row) {
    if (row.KeyId == null || row.KeyId === "") row.KeyId = _tmpKeyId--;
}
function fixedNum(v, digits = 2) {
    const n = Number(v);
    return +((Number.isFinite(n) ? n : 0).toFixed(digits));
}

// --- Smooth refresh batching (one repaint for many updates) ---
let _gridRepaintTimer = null;

function scheduleGridRepaint() {
    clearTimeout(_gridRepaintTimer);
    _gridRepaintTimer = setTimeout(() => {
        const grid = $("#mainItemsGrid").dxDataGrid("instance");
        if (grid) grid.repaint(); 
    }, 50);
}

function updateRowInGrid(row) {
    ensureKeyId(row);
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    const store = grid.getDataSource().store();

    return store.update(row.KeyId, row).then(() => {
            const idx = grid.getRowIndexByKey(row.KeyId);
            if (idx >= 0) grid.repaintRows([idx]);
            safeUpdateFieldsFromGrid();
        });
}

function getEffectiveQty(d) {
    const rq = Number(d.RequestQuantity) || 0;
    const q = Number(d.Quantity) || 0;
    const baseQty = (q > 0) ? q : rq;

    const u = d.UsedQuantity;
    const hasUsed = (u !== null && u !== undefined && u !== "" && isFinite(Number(u)));

    return hasUsed ? Number(u) : baseQty;
}

function getQtyNumberBoxOptions(row) {
    const allowDecimal = !!row?.isDecimalUnit;

    return {
        allowDecimal,
        precision: allowDecimal ? 2 : 0,
        step: allowDecimal ? 0.01 : 1,
        format: { type: "fixedPoint", precision: allowDecimal ? 2 : 0 }
    };
}

function normalizeQtyValue(v, allowDecimal, digits = 2) {
    let n = Number(v);
    if (!isFinite(n)) n = 0;

    if (!allowDecimal) {
        n = Math.floor(n);             
    } else {
        n = +n.toFixed(digits);        
    }
    return n;
}

//===========================================================
function getStatusEstimateText(forPopup = false) {
    const t =
        window.RazorVars?.DXPartWaitingApproval ||
        window.RazorVars?.DXPartApprovalReq ||
        window.RazorVars?.DXPartRequestApproval;

    if (t) return t;

    // fallback
    if (theMainLang === "en") return "Estimate";
    return "Estimate";
}

function isRowApproved(row) {
    const st = Number(row?.Status);
    if (st === 36) return true;

    const txt = String(row?.StatusText || "").trim().toLowerCase();
    if (txt === "approved") return true;

    const approvedTxt = String(window.RazorVars?.DXPartApproved || "").trim().toLowerCase();
    if (approvedTxt && txt === approvedTxt) return true;

    return false;
}

function resetRowToEstimateIfApproved(row, grid, rowIndex) {
    if (!row) return false;

    if (isRowApproved(row)) {
        row.Status = 35;
        row.StatusText = getStatusEstimateText(false);

        if (grid && rowIndex >= 0) {
            grid.cellValue(rowIndex, "Status", 35);
            grid.cellValue(rowIndex, "StatusText", row.StatusText);
        }
        return true;
    }
    return false;
}
function isRowRejected(row) {
    return Number(row?.PriceWorkflowStatus) === PriceWorkflowStatusEnum.Rejected ||
        Number(row?.Status) === 37;
}
//===========================================================
$(function () {
    let locatorsLoadedOnInit = false;
    let unitsLoadedOnce = false;
    $("#mainItemsGrid").dxDataGrid({
        dataSource: ItemsData,
        keyExpr: "KeyId",
        noDataText: resources.NoDataInTable,
        showBorders: true,
        remoteOperations: {
            filtering: false,
            sorting: true,
            paging: true
        },
        columns: [
            { dataField: "KeyId", caption: "#", visible: true, allowEditing: false, width: 50 },
            {
                caption: theMainLang === "en" ? "Price WF" : "إجراءات اعتماد السعر",
                alignment: "center",
                allowEditing: false,
                width: 60,
                visible: Permission_PriceApproval,
                cellTemplate: function (container, options) {
                    const row = options.data || {};

                    if (!row.PriceWorkflowMasterId) return;

                    const status = Number(row.PriceWorkflowStatus ?? 0);
                    const isPending = status === PriceWorkflowStatusEnum.Pending;

                    if (!isPending) return;

                    const items = [
                        { id: "approve", text: theMainLang === "en" ? "Approve" : "اعتماد", icon: "check" },
                        { id: "reject", text: theMainLang === "en" ? "Reject" : "رفض", icon: "close" }
                    ];

                    $("<div>").dxDropDownButton({
                        icon: "overflow",
                        text: "",
                        hint: theMainLang === "en" ? "Actions" : "إجراءات",
                        stylingMode: "contained",
                        type: "default",
                        items: items,
                        displayExpr: "text",
                        keyExpr: "id",
                        useItemTextAsTitle: false,
                        dropDownOptions: { width: 180 },
                        onItemClick: function (e) {
                            const id = e.itemData?.id;

                            if (id === "approve") priceWfApprove(row);
                            else if (id === "reject") {
                                priceWfReject(row).then(() => {
                                    try {
                                        row.Status = 37;
                                        row.StatusText = statusTextById(37, true);

                                        updateRowInGrid(row);
                                    } catch {
                                        location.reload();
                                    }
                                });
                            }
                        }
                    }).appendTo(container);
                }
            },
            { dataField: "Id", caption: "ID", visible: false, alignment: "left" },
            { dataField: "ItemId", caption: "ID", visible: false, alignment: "left" },
            {
                dataField: "WIPId",
                caption: "WIPId",
                dataType: "number",
                visible: false,
                alignment: "left",
                calculateCellValue: function (rowData) {
                    var value = $('#Id').val();
                    var numValue = parseInt(value);
                    rowData.WIPId = numValue;
                    return numValue;
                }
            },
            { dataField: "Code", caption: window.RazorVars.DXCode, allowEditing: false, alignment: "left" },
            { dataField: "Name", caption: window.RazorVars.DXName, allowEditing: false, alignment: "left" },
            { dataField: "FK_SubCategoryId", caption: "SubCategoryId", allowEditing: false, visible: false },
            {
                dataField: "RequiresApproval",
                caption: theMainLang == "en" ? "Price Approval" : "اعتماد السعر",
                visible:  false,          
                allowEditing: false
            },
            {
                dataField: "PriceWorkflowStatusText",
                caption: theMainLang === "en" ? "Price WF Status" : "حالة اعتماد السعر",
                allowEditing: false,
                width: 110,
                cellTemplate: function (container, options) {
                    const row = options.data || {};

                    const st = Number(row.PriceWorkflowStatus ?? 0);     // 0/1/2/3/9
                    let txt = (row.PriceWorkflowStatusText || "").trim();

                    if (st === 0) {
                        return;
                    }

                    if (!txt) {
                        if (st === 1) txt = (theMainLang === "en" ? "Pending" : "قيد الانتظار");
                        else if (st === 2) txt = (theMainLang === "en" ? "Approved" : "تم الاعتماد");
                        else if (st === 3) txt = (theMainLang === "en" ? "Rejected" : "مرفوض");
                        else if (st === 9) txt = (theMainLang === "en" ? "Failed" : "فشل");
                        else txt = (theMainLang === "en" ? "N/A" : "لا يوجد");
                    } else {
                        const t = txt.toLowerCase();
                        if (!st) {
                            if (t.includes("pending")) row.PriceWorkflowStatus = 1;
                            else if (t.includes("approved")) row.PriceWorkflowStatus = 2;
                            else if (t.includes("rejected")) row.PriceWorkflowStatus = 3;
                        }
                    }

                    const cls =
                        st === 1 ? "pwf-badge pwf-pending" :
                            st === 2 ? "pwf-badge pwf-approved" :
                                st === 3 ? "pwf-badge pwf-rejected" :
                                    st === 9 ? "pwf-badge pwf-failed" :
                                        "pwf-badge";

                    $("<span>")
                        .addClass(cls)
                        .text(txt)
                        .appendTo(container);
                }
            },
            {
                dataField: "RequestQuantity",
                caption: window.RazorVars.DXRequestQuantity,
                width: 70,
                dataType: "number",
                //allowEditing: generalRequest == false ? true : false,
                allowEditing: true,
                alignment: "left",
                editCellTemplate: function (cellElement, cellInfo) {
                    const row = cellInfo.data;
                    const canEdit = ![42, 37].includes(Number(row.Status));

                    const available = Number(row.AvailableQty) || 0;
                    const initialVal = cellInfo.value != null ? cellInfo.value : 0;

                    $("<div>").dxNumberBox({
                        value: initialVal,
                        min: 0,
                        max: available,
                        showSpinButtons: true,
                        readOnly: !canEdit,
                        disabled: !canEdit,
                        inputAttr: { "autocomplete": "off" },
                        onValueChanged: function (e) {
                            const allowDecimal = !!row.isDecimalUnit;
                            let v = Number(e.value);

                            if (!isFinite(v)) v = 0;

                            if (!allowDecimal) {
                                v = Math.floor(v);
                            } else {
                                v = +v.toFixed(2);
                            }

                            if (v < 0) {
                                v = 0;
                                e.component.option("value", 0);
                            }

                            if (v > available) {
                                v = available;
                                e.component.option("value", available);
                            }

                            cellInfo.setValue(v);
                            row.RequestQuantity = v;
                            row.Quantity = v;
                            if ((Number(row.Quantity) || 0) > v) {
                                row.Quantity = v;
                                const grid = $("#mainItemsGrid").dxDataGrid("instance");
                                const idx = grid.getRowIndexByKey(row.KeyId);
                                if (idx >= 0) grid.cellValue(idx, "Quantity", v);
                            }
                        },
                        onKeyDown: function (e) {
                            if (e.event?.key === "-") e.event.preventDefault();
                        },


                    }).appendTo(cellElement);
                }

            },
            {
                dataField: "WarehouseId",
                caption: theMainLang == "en" ? "Warehouse" : "المستودع",
                visible: true,
                allowEditing: false,
                alignment: "left",
                lookup: {
                    dataSource: WarehouseData,
                    valueExpr: "Value",
                    displayExpr: function (item) {
                        return item && item.Text ? item.Text : "";
                    }
                },
                calculateCellValue: function (rowData) {
                    if (rowData.WarehouseId) {
                        rowData.WarehouseId = parseInt(rowData.WarehouseId);
                    }
                    return rowData.WarehouseId;
                },
                editCellTemplate: function (cellElement, cellInfo) {
                    $("<div>").dxSelectBox({
                        dataSource: WarehouseData,
                        valueExpr: "Value",
                        displayExpr: "Text",
                        value: cellInfo.value ? parseInt(cellInfo.value) : null,
                        onValueChanged: function (e) {
                            const val = parseInt(e.value);

                            cellInfo.setValue(val);
                            cellInfo.data.WarehouseId = val;

                            cellInfo.data.LocatorId = null;
                            cellInfo.data.AvailableLocators = [];

                            fillLocators([cellInfo.data]);
                        }
                    }).appendTo(cellElement);
                }
            },
            {
                dataField: "LocatorId",
                width: 180,
                caption: window.RazorVars.DXLocator,
                visible: true,
                allowEditing: false,
                alignment: "left",
                calculateDisplayValue: function (rowData) {
                    if (!rowData.LocatorId) return "";

                    let code = rowData.LocatorCode || "";
                    let qty = rowData.AvailableQty || rowData.onHandQtyInUnit || 0;

                    return `${code} (Available: ${qty})`;
                }
            },
            {
                dataField: "fk_UnitId",
                caption: window.RazorVars.DXUnit,
                visible: true,
                alignment: "left",
                lookup: {
                    dataSource: UnitsData,
                    valueExpr: "Value",
                    displayExpr: function (item) {
                        return item && item.Text ? item.Text : "";
                    }
                },
                editCellTemplate: function (cellElement, cellInfo) {
                    const row = cellInfo.data || {};
                    const status = Number(row.Status ?? row.status ?? 0);

                    if ([42, 37].includes(status)) {
                        const selectedUnit = (row.ItemUnits || []).find(x => x.unitId == row.fk_UnitId);

                        const text = selectedUnit
                            ? ((lang === "en") ? selectedUnit.unitPrimaryName : selectedUnit.unitSecondaryName)
                            : "";

                        $("<div>").text(text).appendTo(cellElement);
                        return;
                    }

                    $("<div>").dxSelectBox({
                        dataSource: row.ItemUnits || [],
                        valueExpr: "unitId",
                        displayExpr: function (u) {
                            if (!u) return "";
                            return (lang === "en") ? u.unitPrimaryName : u.unitSecondaryName;
                        },
                        value: row.fk_UnitId,
                        onValueChanged: function (e) {

                            const grid = cellInfo.component;
                            const rowIndex = cellInfo.row.rowIndex;
                            const selectedUnitId = Number(e.value) || 0;

                            row.fk_UnitId = selectedUnitId;

                            const ensureUnits = (row.ItemUnits && row.ItemUnits.length)
                                ? $.Deferred().resolve(row.ItemUnits).promise()
                                : fetchItemUnitsCached(row.ItemId).then(units => {
                                    row.ItemUnits = Array.isArray(units) ? units : [];
                                    return row.ItemUnits;
                                });

                            ensureUnits.then(() => {

                                const selectedUnit = (row.ItemUnits || []).find(x => x.unitId == selectedUnitId);

                                row.isDecimalUnit = !!(selectedUnit?.isDecimalUnit ?? selectedUnit?.isDecimalUnit);

                                const oldFactor = Number(row.UnitFactor) || 1;
                                row.UnitFactor = Number(selectedUnit?.conversionFactor) || 1;

                                if (!row.isDecimalUnit) {
                                    row.RequestQuantity = Math.floor(Number(row.RequestQuantity) || 0);
                                    row.Quantity = Math.floor(Number(row.Quantity) || 0);

                                    if (row.UsedQuantity !== null && row.UsedQuantity !== undefined && row.UsedQuantity !== "") {
                                        row.UsedQuantity = Math.floor(Number(row.UsedQuantity));
                                    }
                                }

                                if (!row.BaseCostPrice || Number(row.BaseCostPrice) === 0) {
                                    row.BaseCostPrice = +((Number(row.CostPrice) || 0) * oldFactor).toFixed(2);
                                }

                                recalcCostPriceForUnit(row);

                                // reset locator
                                row.LocatorId = null;
                                row.LocatorCode = null;
                                row.AvailableQty = null;
                                row.AvailableLocators = [];

                                cellInfo.setValue(selectedUnitId);

                                resetRowToEstimateIfApproved(row, grid, rowIndex);

                                grid.beginUpdate();
                                try {
                                    grid.cellValue(rowIndex, "fk_UnitId", selectedUnitId);
                                    grid.cellValue(rowIndex, "isDecimalUnit", row.isDecimalUnit);

                                    grid.cellValue(rowIndex, "RequestQuantity", row.RequestQuantity);
                                    grid.cellValue(rowIndex, "Quantity", row.Quantity);
                                    grid.cellValue(rowIndex, "UsedQuantity", row.UsedQuantity);

                                    grid.cellValue(rowIndex, "CostPrice", row.CostPrice);
                                    grid.cellValue(rowIndex, "Price", row.Price);
                                    grid.cellValue(rowIndex, "LocatorId", row.LocatorId);
                                } finally {
                                    grid.endUpdate();
                                }

                                grid.closeEditCell();

                                updateRowInGrid(row).then(() => {
                                    const freshRow =
                                        grid.getDataSource().items().find(x => x.KeyId === row.KeyId) || row;

                                    fillLocators([freshRow]);
                                    safeUpdateFieldsFromGrid();
                                });

                            });
                        }
                    }).appendTo(cellElement);
                }
            },
            {
                caption: "isDecimalUnit",
                visible: false,
                allowEditing: false,
                width: 120,
                alignment: "center",
                calculateCellValue: function (row) {
                    if (row.isDecimalUnit !== undefined && row.isDecimalUnit !== null) {
                        return !!row.isDecimalUnit;
                    }

                    const u = (row.ItemUnits || []).find(x => x.unitId == row.fk_UnitId);
                    return !!u?.isDecimalUnit;   
                },
                cellTemplate: function (container, options) {
                    $(container).text(options.value ? "TRUE" : "FALSE");
                }
            },
            {
                dataField: "MaxQty",
                caption: " MaxQty",
                dataType: "number",
                visible: false,
                alignment: "left"
            },
            {
                dataField: "Quantity",
                caption: window.RazorVars.DXQuantity,
                width: 70,
                dataType: "number",
                allowEditing: Permission_AddParts,
                alignment: "left",
                editCellTemplate: function (cellElement, cellInfo) {
                    const row = cellInfo.data;
                    const canEdit = Permission_AddParts && ![42, 37].includes(Number(row.Status));

                    const reqQty = Number(row.RequestQuantity) || 0;
                    const maxQty = row.MaxQty != null ? Number(row.MaxQty) : null;

                    const finalMax = (maxQty != null) ? Math.min(reqQty, maxQty) : reqQty;

                    const initialVal = cellInfo.value != null && Number(cellInfo.value) !== 0
                            ? cellInfo.value
                            : Number(row.RequestQuantity) || 0;

                    $("<div>").dxNumberBox({
                        value: initialVal,
                        min: 0,
                        max: finalMax,
                        showSpinButtons: true,
                        readOnly: !canEdit,
                        disabled: !canEdit,
                        inputAttr: { "autocomplete": "off" },
                        onValueChanged: function (e) {
                            //let v = Number(e.value) || 0;
                            const opt = getQtyNumberBoxOptions(row);
                            let v = normalizeQtyValue(e.value, opt.allowDecimal, 2);

                            if (v > finalMax) {
                                v = finalMax;
                                e.component.option("value", finalMax);
                            }

                            if (v !== e.value) e.component.option("value", v);

                            cellInfo.setValue(v);
                            row.Quantity = v;
                        }
                    }).appendTo(cellElement);
                }
            },
            {
                dataField: "UsedQuantity",
                caption: window.RazorVars.DXUsedQuantity,
                width: 70,
                dataType: "number",
                allowEditing: true,
                alignment: "left",
                editCellTemplate: function (cellElement, cellInfo) {
                    const row = cellInfo.data;
                    const canEdit = Permission_Approve && Number(row.Status) === 42;
                    const opt = getQtyNumberBoxOptions(row);

                    $("<div>").dxNumberBox({
                        value: cellInfo.value,         
                        min: 0,
                        max: row.Quantity,      
                        step: opt.step,
                        format: opt.format,
                        showSpinButtons: true,
                        readOnly: !canEdit, 
                        disabled: !canEdit,
                        valueChangeEvent: "input",
                        onValueChanged: function (e) {
                            if (!canEdit) return;
                            //const v = e.value;
                            let v = normalizeQtyValue(e.value, opt.allowDecimal, 2);
                            const maxV = Number(row.Quantity) || 0;
                            debugger
                            // clamp
                            if (v > maxV) v = maxV;
                            if (v < 0) v = 0;

                            const isEmpty = v === null || v === undefined || Number.isNaN(v);

                            if (isEmpty) {
                                e.component.option("value", null);
                                cellInfo.setValue(null);
                                row.UsedQuantity = null;

                                $("#optReturnParts").prop("checked", true);
                            } else {
                                if (v !== e.value) e.component.option("value", v);

                                cellInfo.setValue(v);
                                row.UsedQuantity = v;

                                const isLess = v < maxV;
                                $("#optReturnParts").prop("checked", isLess);
                            }

                            const grid = $("#mainItemsGrid").dxDataGrid("instance");
                            const idx = grid.getRowIndexByKey(row.KeyId);
                            if (idx >= 0) grid.repaintRows([idx]);  

                            safeUpdateFieldsFromGrid();
                        }
                    }).appendTo(cellElement);
                }
            },
            { dataField: "CostPrice", caption: "Cost Price", dataType: "number", visible: false, alignment: "left" },
            { dataField: "SalePrice", caption: "SalePrice", dataType: "number", visible: false, alignment: "left" },
            {
                dataField: "Price",
                caption: window.RazorVars.DXPrice,
                dataType: "number",
                allowEditing: true,
                alignment: "left",
                calculateCellValue: function (rowData) {

                    if (isRowRejected(rowData)) {
                        rowData.Price = 0;
                        return 0;
                    }

                    const type = (rowData.AccountType != null && rowData.AccountType !== 0 && rowData.AccountType !== "")
                        ? String(rowData.AccountType)
                        : String($("#AccountType").val() || "");

                    let discount = parseFloat($("#_DiscountPercentagePart").val());
                    if (isNaN(discount)) discount = 0;

                    let basePrice = 0;

                    if (type === "1") {
                        basePrice = parseFloat(rowData.CostPrice);
                        if (isNaN(basePrice)) basePrice = 0;

                    } else if (type === "2") {
                        const baseSale = parseFloat(rowData.SalePrice) || 0;
                        const factor = parseFloat(rowData.UnitFactor) || 1;

                        basePrice = baseSale / factor;

                        basePrice = basePrice - (basePrice * (discount / 100));
                    }

                    rowData.Price = Number(basePrice) || 0;
                    return Number(rowData.Price.toFixed(2));
                }
            },
            {
                dataField: "DiscountPct",
                caption: window.RazorVars.DXDiscount,
                width: 70,
                dataType: "number",
                allowEditing: Permission_AddDiscount,
                alignment: "left",
                editorOptions: {
                    min: 0,
                    max: 100,
                    showSpinButtons: true
                },
                calculateCellValue: function (rowData) {
                    if (isRowRejected(rowData)) {
                        rowData.Discount = 0;
                        rowData.DiscountPct = 0;
                        return 0;
                    } 

                    //const qty = +rowData.Quantity || +rowData.RequestQuantity || 0;
                    const qty = getEffectiveQty(rowData);
                    const price = +rowData.Price || 0;
                    const base = +(qty * price).toFixed(4);   

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
                    //const qty = +currentRowData.Quantity || +currentRowData.RequestQuantity || 0;
                    const qty = getEffectiveQty(currentRowData);

                    const price = +currentRowData.Price || 0;
                    const base = +(qty * price).toFixed(4);

                    const discAmt = base * (pct / 100);

                    newData.Discount = discAmt;
                },
                customizeText: function (cellInfo) {
                    return (Number(cellInfo.value) || 0).toFixed(1) + " %";
                }
            },

            {
                dataField: "Discount",
                caption: window.RazorVars.DXDiscount,     
                dataType: "number",
                allowEditing: false,                     
                defaultValue: 0,
                alignment: "left",
                visible: false
            },
            {
                dataField: "Tax",
                caption: window.RazorVars.DXTax,
                dataType: "number",
                allowEditing: false, 
                alignment: "left",
                calculateCellValue: function (rowData) {

                    if (isRowRejected(rowData)) {
                        rowData.Tax = 0;
                        return 0;
                    } 

                    if (Number(rowData.AccountType) === 1) {
                        rowData.Tax = 0;
                        return 0;
                    }

                    var vatId = getActiveVatId();
                    if (!vatId) {
                        rowData.Tax = 0;
                        return 0;
                    }

                    var vatPercent = parseFloat(GetVatValueById(vatId)) || 0;
                    vatPercent = vatPercent > 1 ? vatPercent / 100 : vatPercent; 

                    //var qty = parseFloat(rowData.Quantity) || 0;
                    var qty = getEffectiveQty(rowData);

                    var price = parseFloat(rowData.Price) || 0;
                    var discountPct = parseFloat(rowData.DiscountPct) || 0;

                    var discountedPrice = price - (price * (discountPct / 100));

                    var taxAmount = qty * discountedPrice * vatPercent;

                    rowData.Tax = +taxAmount.toFixed(2);

                    return rowData.Tax;
                }
            } ,

            {
                dataField: "Total",
                caption: window.RazorVars.DXTotal,
                dataType: "number",
                allowEditing: false,
                alignment: "left",
                calculateCellValue: function (rowData) {

                    if (isRowRejected(rowData)) {
                        rowData.Total = 0;
                        return 0;
                    }

                    var requestQuantity = parseFloat(rowData.RequestQuantity) || 0;
                    var quantity = parseFloat(rowData.Quantity) || 0;
                    var price = parseFloat(rowData.Price) || 0;
                    var tax = parseFloat(rowData.Tax) || 0;
                    var _rowDiscount = parseFloat(rowData.Discount) || 0;
                    var _rowDiscountAmt = parseFloat(rowData.Discount) || 0;
                    //var Qty = quantity > 0 ? quantity : requestQuantity;
                    var Qty = getEffectiveQty(rowData);

                    var totalValue = (Qty * price) + tax;
                    if (_rowDiscountAmt > 0) {
                        totalValue -= _rowDiscountAmt;   
                        if (totalValue < 0) totalValue = 0;
                    }

                    rowData.Total = totalValue;
                    return parseFloat(totalValue.toFixed(2));
                }
            },
            {
                dataField: "PartsIssueId", allowEditing: false,
                caption: theMainLang == "en" ? "Ref. No.": "رقم المرجع",
                visible: true,
                alignment: "left"
            },
            {
                dataField: "Status",
                caption: "Stats",
                visible: false,
                alignment: "left"
            },
            {
                dataField: "StatusText",
                caption: window.RazorVars.DXStatus,
                allowEditing: false,
                alignment: "left"
            },
            {
                dataField: "AccountType",
                caption: window.RazorVars.DXAccountType,
                width: 90,
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
                        return accountTypeVal;
                    }

                    return rowData.AccountType;
                }
            },
            {
                type: "buttons",
                width: 150,
                alignment: "left",
                buttons: [
                    {
                        hint: "Approve",
                        icon: "check",
                        type: "success",
                        stylingMode: "contained",
                        visible: function (e) {
                            debugger
                            return (e.row.data.PriceWorkflowStatus == 2 || e.row.data.PriceWorkflowStatus == 0) &&
                                (Permission_Approve && (AllowActions &&
                                    (parseInt(e.row.data.Status) == 35 || parseInt(e.row.data.Status) == 50 || parseInt(e.row.data.Status) == 51)
                                ));
                        },
                        onClick: function (e) {
                            console.log("Approve clicked", e.row.data);
                            updateStatusItem(e.row.data, 36);
                        }
                    },
                    {
                        hint: "Reject",
                        icon: "close",
                        type: "danger",
                        stylingMode: "contained",
                        visible: function (e) {
                            return (e.row.data.PriceWorkflowStatus == 2 || e.row.data.PriceWorkflowStatus == 0) &&
                                (Permission_Approve && (AllowActions && 
                                    (parseInt(e.row.data.Status) == 35 || parseInt(e.row.data.Status) == 50 || parseInt(e.row.data.Status) == 51)
                                ));
                        },
                        onClick: function (e) {
                            console.log("Reject clicked", e.row.data);
                            updateStatusItem(e.row.data, 37);
                        }
                    },
                    {
                        hint: "issue",
                        icon: "box",
                        type: "success",
                        stylingMode: "contained",
                        visible: function (e) {
                            const stRaw = e?.row?.data?.Status;
                            if (stRaw === undefined || stRaw === null || stRaw === "") return false;

                            return !(wipStatus === Complete || wipStatus === Invoiced) &&
                                (e.row.data.PriceWorkflowStatus == 2 || e.row.data.PriceWorkflowStatus == 0) &&
                                (Permission_Issue && (
                                    AllowActions &&
                                parseInt(e.row.data.Status) !== 41 && parseInt(e.row.data.Status) !== 35 &&
                                parseInt(e.row.data.Status) !== 37 && OurWarehouses.includes(parseInt(e.row.data.WarehouseId))
                                ));
                        },
                        disabled: function (e) {
                            return parseInt(e.row.data.Status) === 42;
                        },
                        onClick: function (e) {
                            if (parseInt(e.row.data.Status) !== 36) {
                                Swal.fire({
                                    icon: "warning",
                                    title: theMainLang == "en" ? 'Please request approval first' : "قم بطلب الموافقة أولاً",
                                    text: theMainLang == "en" ? 'You cannot issue this item until it is approved.' : "لا يمكنك إصدار هذا المنتج حتى تتم الموافقة عليه.",
                                });
                                return;
                            }
                            CreateIssueVoucher(e.row.data);
                        }
                    },
                    {
                        hint: "Undo",
                        icon: "undo",
                        type: "success",
                        stylingMode: "contained",
                        visible: function (e) {
                            const stRaw = e?.row?.data?.Status;
                            if (stRaw === undefined || stRaw === null || stRaw === "") return false;

                            return !(wipStatus === Complete || wipStatus === Invoiced) &&
                                (e.row.data.PriceWorkflowStatus == 2 || e.row.data.PriceWorkflowStatus == 0) &&
                                (Permission_Issue && (
                                    AllowActions &&
                                    parseInt(e.row.data.Status) === 42 &&
                                    OurWarehouses.includes(parseInt(e.row.data.WarehouseId))
                                ));
                        },
                        onClick: function (e) {
                            UndoIssueVoucher(e.row.data);
                        }
                    },
                    {
                        hint: "Transfer",
                        icon: "repeat",
                        type: "default",
                        visible: function (e) {
                            return (e.row.data.PriceWorkflowStatus == 2 || e.row.data.PriceWorkflowStatus == 0) &&
                                (Permission_AddParts && (
                                AllowActions &&
                                //parseInt(e.row.data.Status) !== 35 &&
                                //parseInt(e.row.data.Status) !== 42 &&
                                //parseInt(e.row.data.Status) !== 41 &&
                                parseInt(e.row.data.Status) == 36 &&
                                !OurWarehouses.includes(parseInt(e.row.data.WarehouseId))
                            ));
                        },
                        onClick: function (e) {
                            if (parseInt(e.row.data.Status) !== 36) {
                                Swal.fire({
                                    icon: "warning",
                                    title: theMainLang == "en" ? 'Please request approval first' : "قم بطلب الموافقة أولاً",
                                    text: theMainLang == "en" ? 'You cannot issue this item until it is approved.' : "لا يمكنك إصدار هذا المنتج حتى تتم الموافقة عليه.",
                                });
                                return;
                            }
                            TransferItem(e.row.data);
                        }
                    },
                    {
                        hint: theMainLang == "en" ? "Reserve" : "حجز قطع من المستودع",
                        icon: "bookmark",
                        text: theMainLang == "en" ? "Reserve" : "حجز",
                        type: "success",
                        stylingMode: "contained",
                        visible: function (e) {
                            const stRaw = e?.row?.data?.Status;
                            if (stRaw === undefined || stRaw === null || stRaw === "") return false;

                            return !(wipStatus === Complete || wipStatus === Invoiced) &&
                                (Permission_Issue && (
                                    AllowActions &&
                                    parseInt(e.row.data.Status) !== 41 &&
                                    parseInt(e.row.data.Status) !== 37 &&
                                    parseInt(e.row.data.Status) !== 42 &&
                                    parseInt(e.row.data.Status) !== 50 &&
                                    parseInt(e.row.data.LocatorId) > 0 &&
                                    OurWarehouses.includes(parseInt(e.row.data.WarehouseId))
                                ));
                        },
                        disabled: function (e) {
                            return parseInt(e.row.data.Status) === 42;
                        },
                        onClick: function (e) {
                            CreateIssueVoucher(e.row.data, 0, 50, 11);
                        
                        }
                    },
                    {
                        hint: theMainLang == "en" ? "Cancel Reserve" : "الغاء حجز قطع من المستودع",
                        icon: "clear",
                        text: theMainLang == "en" ? "Cancel Reserve" : "الغاء حجز",
                        type: "success",
                        stylingMode: "contained",
                        visible: function (e) {
                            const stRaw = e?.row?.data?.Status;
                            if (stRaw === undefined || stRaw === null || stRaw === "") return false;

                            return !(wipStatus === Complete || wipStatus === Invoiced) &&
                                (Permission_Issue && (
                                AllowActions &&
                                    parseInt(e.row.data.Status) !== 35 &&
                                    parseInt(e.row.data.Status) !== 51 &&
                                    parseInt(e.row.data.Status) !== 37 &&
                                    parseInt(e.row.data.Status) !== 42 &&
                                    parseInt(e.row.data.LocatorId) > 0 &&
                                    OurWarehouses.includes(parseInt(e.row.data.WarehouseId))
                                ));
                        },
                        disabled: function (e) {
                            return parseInt(e.row.data.Status) === 42;
                        },
                        onClick: function (e) {
                            UndoReservation(e.row.data);

                        }
                    },
                    {
                        hint: theMainLang === "en" ? "Price WF History" : "سجل اعتماد السعر",
                        icon: "info",
                        type: "default",
                        stylingMode: "contained",
                        visible: function (e) {
                            const row = e?.row?.data || {};
                            return !!row.PriceWorkflowMasterId;
                        },
                        onClick: function (e) {
                            priceWfHistory(e.row.data);
                        }
                    },
                    {
                        hint: theMainLang === "en" ? "Item Details" : "تفاصيل الصنف",
                        icon: "menu",            
                        type: "default",
                        stylingMode: "contained",
                        visible: function (e) { return Permission_StockCard; },
                        onClick: function (e) {
                            const itemId = e?.row?.data?.ItemId || e?.row?.data?.Id;
                            if (!itemId) {
                                Swal.fire({
                                    icon: "warning",
                                    title: theMainLang == "en" ? "Missing Item ID" : "رقم الصنف غير موجود",
                                });
                                return;
                            }

                            openInventoryItemTabsModal(itemId);
                        }
                    },
                    {
                        hint: "Delete",
                        icon: "fad fa-trash",
                        visible: function (e) {
                            return Permission_AddParts && (
                                parseInt(e.row.data.Status) !== 42 && parseInt(e.row.data.Status) !== 41 &&
                                !(wipStatus === Gone || wipStatus === Invoiced)
                            );
                        },
                        onClick: function (e) {
                            var grid = e.component;
                            grid.getDataSource().store().remove(e.row.key);
                            grid.refresh();
                        }
                    }
                ]
            }
        ],
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnAutoWidth: true,
        //columnMinWidth: 50,if the comment removed, make the columnAutoWidth is true
        //wordWrapEnabled: false,
        hoverStateEnabled: true,
        paging: {
            pageSize: 10
        },
        rowAlternationEnabled: true,
        //pager: {
        //    visible: true,
        //    showPageSizeSelector: true,
        //    allowedPageSizes: [5, 10, 20, 50],
        //    showInfo: true,
        //    showNavigationButtons: true
        //},
        editing: {
            mode: "cell",
            allowDeleting: true,
            allowUpdating: true
        },
        onCellValueChanged: function (e) {
            if (["Quantity", "Price", "DiscountPct", "UsedQuantity"].includes(e.column.dataField)) {
                e.component.updateDimensions();
            }

            if (["Quantity", "Price", "DiscountPct", "UsedQuantity", "RequestQuantity"].includes(e.column.dataField)) {
                const idx = e.row?.rowIndex;
                if (idx != null && idx >= 0) e.component.repaintRows([idx]); 
            }

            if (e.column.dataField === "AccountType") {
                e.component.refresh().done(safeUpdateFieldsFromGrid);
                return;
            }

            if (["Price", "DiscountPct"].includes(e.column.dataField)) {
                const row = e.data;
                if (row && row.requiresPriceApproval) {
                    resetRowToEstimateIfApproved(row, e.component, e.row?.rowIndex ?? -1);
                }
            }

            safeUpdateFieldsFromGrid();
        },
        onRowRemoved: function () {
            safeUpdateFieldsFromGrid();
        },
        onContentReady: function (e) {
            safeUpdateFieldsFromGrid();

            if (!locatorsLoadedOnInit) {
                const grid = e.component;
                const rows = grid.getVisibleRows().map(r => r.data);
                fillLocators(rows);
                locatorsLoadedOnInit = true;
            }

            if (!unitsLoadedOnce) {
                unitsLoadedOnce = true;
                const grid = e.component;
                const allRows = grid.getDataSource().items();
                loadUnitsForRows(allRows);
            }
        },
        onInitNewRow: function (e) {
            if (e.data.KeyId == null || e.data.KeyId === "") {
                e.data.KeyId = _tmpKeyId--;
            }

            const accountTypeVal = parseInt($("#AccountType").val());
            const partialInvoicing = $("#optPartialInv").is(":checked");

            if (!partialInvoicing) {
                e.data.AccountType = accountTypeVal;
            }
        },
        onRowInserted: function (e) {
            if (typeof safeUpdateFieldsFromGrid === "function") {
                safeUpdateFieldsFromGrid();
            }

            const grid = e.component;
            const store = grid.getDataSource().store();
            const accountTypeVal = parseInt($("#AccountType").val());
            const partialInvoicing = $("#optPartialInv").is(":checked");

            if (!partialInvoicing) {
                e.data.AccountType = accountTypeVal;
                store.update(e.key, e.data).then(() => grid.refresh());
            }
        },
        onEditorPreparing: function (e) {                                   // need Testing
            debugger
            if (e.parentType === "dataRow" && e.dataField === "DiscountPct") {

                var pageAccountType = parseInt($("#AccountType").val()) || 0;

                var rowAccountType = 0;
                if (e.row && e.row.data && e.row.data.AccountType != null) {
                    rowAccountType = parseInt(e.row.data.AccountType) || 0;
                }

                if (pageAccountType === 1 || rowAccountType === 1) {
                    e.editorOptions.readOnly = true;

                } else {
                    e.editorOptions.readOnly = false;
                }
            }

            if (e.parentType === "dataRow" && e.dataField === "RequestQuantity") {
                const status = Number(e.row?.data?.Status);
                if (status === 36 || status === 42) {
                    e.cancel = true;
                }
            }

                
        },
    });
});

$(function () {
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    const rows = grid.getDataSource().items();
    //fillLocators(rows);
});

window.locatorsCache = window.locatorsCache || {}; 

function getLocatorsCached(dto) {
    const key = `${dto.ItemId}|${dto.Fk_UnitId}|${dto.Fk_WarehouseId}`;
    if (window.locatorsCache[key]) {
        return $.Deferred().resolve(window.locatorsCache[key]).promise();
    }

    return $.ajax({
        url: window.RazorVars.getAvailableLocatorsUrl,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(dto)
    }).then(function (res) {
        const locs = (res && res.success && Array.isArray(res.data)) ? res.data : [];
        window.locatorsCache[key] = locs;
        return locs;
    });
}

function fillLocators(rows) {
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid || !rows || !rows.length) return;

    const updates = [];

    rows.forEach(function (row) {
        const dto = {
            ItemId: row.ItemId,
            Fk_UnitId: Number(row.fk_UnitId) || 0,
            Fk_WarehouseId: Number(row.WarehouseId) || 0
        };
        if (!dto.ItemId || !dto.Fk_UnitId || !dto.Fk_WarehouseId) return;

        const p = getLocatorsCached(dto).then(function (locators) {
            row.AvailableLocators = locators || [];

            if (!row.LocatorId && row.AvailableLocators.length > 0) {
                row.LocatorId = row.AvailableLocators[0].locatorId;
                row.LocatorCode = row.AvailableLocators[0].locatorCode;
                row.AvailableQty = row.AvailableLocators[0].onHandQtyInUnit;
                row.MaxQty = Number(row.AvailableQty) || 0;
            }

            const match = row.AvailableLocators.find(x => x.locatorId == row.LocatorId);
            if (match) {
                row.LocatorCode = match.locatorCode;
                row.AvailableQty = match.onHandQtyInUnit;
                row.MaxQty = Number(row.AvailableQty) || 0;
            }
            if (row.isDecimalUnit == null) {
                const u = (row.ItemUnits || []).find(x => x.unitId == row.fk_UnitId);
                row.isDecimalUnit = !!u?.isDecimalUnit;
            }

            return updateRowInGrid(row);
        });

        updates.push(p);
        console.log("DTO sent:", dto);
    });

    $.when.apply($, updates).always(function () {
        scheduleGridRepaint(); 
    });
}


$('#AccountType, #Vat, #PartialVat').on('change', safeRefreshMainItemsGrid);

function updateFieldsFromGrid() {
    var grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid) return;

    var rows = grid.getVisibleRows() || [];
    var totalSum = 0;
    var totalDiscountsPart = 0;
    var totalVAT = 0; 

    rows.forEach(function (r) {
        var d = r.data || {};
        //var qty = parseFloat(d.Quantity) || parseFloat(d.RequestQuantity) || 0;
        var qty = getEffectiveQty(d);

        var price = parseFloat(d.Price) || 0;
        var disc = parseFloat(d.Discount) || 0;
        var tax = parseFloat(d.Tax) || 0;

        var base = qty * price;

        var discAmt = parseFloat(d.Discount) || 0;
        //var discAmt = base * (disc / 100);

        var lineTot = base - discAmt;

        totalDiscountsPart += discAmt;
        totalSum += lineTot;

        totalVAT += tax; 
    });

    $("#totParts").text("SAR " + totalSum.toFixed(2));
    setAmount("#totParts", totalSum);
    $("#TotalDiscountsPart").text("SAR " + totalDiscountsPart.toFixed(2));

    const currentVAT = getAmount("#totVAT");
    const combinedVAT = currentVAT + totalVAT;
    setAmount("#totVAT", combinedVAT);


    //const items = rows.map(r => r.data || {});
    //const hasReturn = items.some(x =>
    //    Number(x.UsedQuantity) < Number(x.Quantity)
    //);

    //$("#optReturnParts").prop("checked", hasReturn);

    updateSubtotal();
}

function getAmount(sel) {
    return +($(sel).data("value") ?? 0) || 0;
}

function setAmount(sel, n) {
    const value = Number(n) || 0;
    const $el = $(sel);

    $el.data("value", value);
    $el.attr("data-value", value);
    $el.text("SAR " + value.toFixed(2));
}

function getActiveVatId() {
    const v = $.trim($("#Vat").val() || "");
    if (v) return v;

    const pv = $.trim($("#PartialVat").val() || "");
    if (pv) return pv;

    return "";
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

function getVatPercent() {
    const v = getActiveVatId();
    return v === "" ? 0 : (Number(GetVatValueById(v)) || 0);
}

function setVatPercentageText(pct) {
    $("#VatPercentage").text("% " + Number(pct).toFixed(0));
}

function updateSubtotal() {
    const labour = getAmount("#totLabour");
    const parts = getAmount("#totParts");

    const internalSubtotal = labour + parts;
    const transferSubtotal = Number(window.RazorVars.transferSubtotal || 0);

    const subtotal = internalSubtotal + transferSubtotal;

    setAmount("#totSubtotal", subtotal);
    updateVatAndTotal();
}


function updateVatAndTotal() {
    const labour = getAmount("#totLabour");
    const parts = getAmount("#totParts");

    const subtotal = getAmount("#totSubtotal");
    const vatPct = getVatPercent();

    const internalVat =
        (labour * (vatPct / 100)) + (parts * (vatPct / 100));

    const transferVat = Number(window.RazorVars.transferVatAmount || 0);

    const vatAmt = internalVat + transferVat;

    const transferTotal = Number(window.RazorVars.transferTotal || 0);

    const internalSubtotal = (labour + parts);
    const internalTotal = internalSubtotal + internalVat;

    const total = internalTotal + transferTotal;

    setVatPercentageText(vatPct);
    setAmount("#totVAT", vatAmt);
    setAmount("#totTotal", total);
}

$(document).on("input change", "#Vat", "#PartialVat", function () {
    updateVatAndTotal();
});

$(function () {
    updateSubtotal();
});

function GetReturnParts() {
    $.ajax({
        url: window.RazorVars.getReturnPartsUrl,
        method: 'Get',
        dataType: 'json',
        success: function (result) {
            Swal.fire({
                icon: "success",
                title: theMainLang == "en" ? 'Success' : "تمت العملية بنجاح",
            });
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: "error",
                title: theMainLang == "en" ? 'Error Happened' : "حصل خطأ ما",
                showConfirmButton: true,
                confirmButtonText: theMainLang == "en" ? 'Ok' : "حسناً",
            });
        }
    });
}

$("#ApproveRequestPartsBTN").on("click", function () {
    UpdatePartStatus(36, "Approved");
});

$("#FK_WarehouseId").on("change", function () {
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    //const data = grid.option("dataSource");
    const data = grid.getDataSource().items();

    fillLocators(data);
});

function UpdatePartStatus(newStatus, statusText) {
    const grid = $('#mainItemsGrid').dxDataGrid('instance');
    var data = grid.option("dataSource");

    const rowIndex = grid.getRowIndexByKey(RTSId);
    if (rowIndex >= 0) {
        grid.cellValue(rowIndex, "Status", newStatus);
        grid.cellValue(rowIndex, "StatusText", statusText);
    }

    grid.saveEditData();
    grid.refresh();
    $.ajax({
        type: 'POST',
        url: window.RazorVars.updatePartStatusUrl,
        contentType: 'application/json',
        data: JSON.stringify({
            WIPId: $('#Id').val(),
            StatusId: newStatus
        }),
        success: function (res) {
            Swal.fire({
                icon: "success",
                title: statusTextById(newStatus, true)
            }).then(() => {
                //location.reload();
                $("#mainItemsGrid").dxDataGrid("instance")?.repaint();
            });
        },
        error: function (err) {
            console.error("Error updating status:", err);
        }
    });
}

function statusTextById(statusId, forPopup = false) {
    if (forPopup) {
        return statusId == 35 ? getStatusEstimateText(true) :
         statusId == 36 ? "Approved" :
            statusId == 37 ? "Rejected" :
                statusId == 41 ? "Waiting Part" :
                    statusId == 42 ? "Part Received" :
                        statusId == 50 ? "Reserved" :
                            statusId == 51 ? "Cancel Reserve" :
                        "Updated";
    } else {
        return statusId == 35 ? getStatusEstimateText(false) :
        statusId == 36 ? window.RazorVars.DXPartApproved :
            statusId == 37 ? window.RazorVars.DXPartRejected :
                statusId == 41 ? window.RazorVars.DXPartTransferReq :
                    statusId == 42 ? window.RazorVars.DXPartReceived :
                        "Updated";
    }
}


function updateStatusItem(row, statusId) {
    const dto = {
        WIPId: Number($("#Id").val()),
        Id: row.Id,
        StatusId: statusId
    };
    debugger
    return $.ajax({
        url: window.RazorVars.updatePartStatusWithSingleItem,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(dto)
    }).done(function (res) {
        debugger
        if (res && res.success) {
           
            row.Status = statusId;
            row.StatusText = statusTextById(statusId, true);

            updateRowInGrid(row)
                Swal.fire({
                    icon: "success",
                    title: statusTextById(statusId, false)
                });
            location.reload();
        }
    }).fail(function (xhr, st, err) {
        console.error("Error updating status:", err);
    });
}


function IssueParts() {
    const grid = $('#mainItemsGrid').dxDataGrid('instance');
    var GridData = grid.option("dataSource");
    var WIPId = $("#Id").val();
    
    Items = GridData.map(x => ({
        KeyId: `${x.Id}`,
        FK_ItemId: x.ItemId,
        FK_UnitId: x.fk_UnitId,
        Quantity: x.RequestQuantity,
        UnitQuantity: x.Quantity,
        Price: x.Price,
        Total: x.Total,
        FK_LocatorId: x.LocatorId,
        FK_WarehouseId: x.WarehouseId
    }));

    data = {
        TransactionReferenceNo: WIPId,
        FK_WarehouseId: Items[0].WarehouseId,
        FK_TransactionTypeId: 1,
        FK_TransactionStatusId: 2,
        StockType: -1,
        WhrHouse: 0,
        Details: Items
    };
    $.ajax({
        type: 'POST',
        url: window.RazorVars.createInventoryTransactionUrl,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (res) {
             ;
            if (res.success) {
                UpdatePartStatus(42, "Part Received");
            }
        },
        error: function (err) {
            console.error("Error updating status:", err);
        }
    });
}

function TransferParts() {
    const row = window.transferRow;
    const WIPId = $("#Id").val();

    const fromWh = $("#transferFromWarehouse").val();
    const toWh = $("#transferToWarehouse").val();

    const item = {
        KeyId: `${row.Id}`,
        FK_ItemId: row.ItemId,
        FK_UnitId: row.fk_UnitId,
        UnitQuantity: row.RequestQuantity,
        Price: 0,
        Total: 0,
        FK_LocatorId: row.LocatorId,
        FK_WarehouseId: fromWh
    };

    const data = {
        TransactionReferenceNo: WIPId,
        FK_TransactionReferenceTypeId: 1015,
        FK_TransactionTypeId: 10,
        FK_TransactionStatusId: 6,
        WhrHouse: 0,
        FK_FromWarehouseId: fromWh,
        FK_ToWarehouseId: toWh,
        FK_WarehouseId: fromWh,
        Details: [item]
    };

    const btn = $("#btnConfirmTransfer");
    const oldTxt = btn.html();
    btn.prop("disabled", true).html('<i class="fa fa-spinner fa-spin me-1"></i>');

    const saveReq = saveData?.() || $.Deferred().resolve().promise();

    $.when(saveReq).always(function () {
        $.ajax({
            type: 'POST',
            url: window.RazorVars.createInventoryTransactionUrl,
            contentType: 'application/json',
            data: JSON.stringify(data)
        }).done(function (res) {
            if (res && res.success) {

                row.WarehouseId = fromWh;

                updateRowInGrid(row).then(() => {

                    updateStatusItem(row, 41).then(() => {
                        Swal.fire({
                            icon: "success",
                            title: theMainLang == "en" ? 'Transfer request prepared' : "تم إعداد طلب التحويل",
                            text: theMainLang == "en" ? 'You can now proceed with stock transfer' : "يمكنك الآن المتابعة في عملية نقل المخزون",
                        });
                    });
                });
            } else {
                //Swal.fire(theMainLang == "en" ? 'Error Happened' : "حصل خطأ ما", "", "error");
                handleFailure(res);
            }
        }).fail(function (err) {
            console.error("Error updating status:", err);

            let data = xhr.responseJSON;
            if (!data && xhr.responseText) {
                try { data = JSON.parse(xhr.responseText); } catch (e) { data = null; }
            }
            const isConflict = (xhr.status === 409);
            handleFailure(data, isConflict);

            //Swal.fire(theMainLang == "en" ? 'Error Happened' : "حصل خطأ ما", "", "error");
        }).always(function () {
            btn.prop("disabled", false).html(oldTxt);
        });
    });
}
function saveData() {
    var Services_grid = $('#mainRTSGrid').dxDataGrid('instance');
    var Services_Items = Services_grid.getDataSource().items();

    var ServicesJson = JSON.stringify(Services_Items);
    $("#Services").val(ServicesJson);

    var grid = $('#mainItemsGrid').dxDataGrid('instance');
    var gridItems = grid.getDataSource().items().map(x => ({
        ...x,
        requiresPriceApproval: !!x.requiresPriceApproval,
        RequiresPriceApproval: !!x.requiresPriceApproval
    }));

    var itemsJson = JSON.stringify(gridItems);
    $("#Items").val(itemsJson);

    var WIPId = $('#Id').val();
    var WsId = 10;
    var VehId = $('#_vehicleId').val();
    var MovId = $('#_movementId').val();
    var accountType = $("#AccountType").val();
    var salesType = $("#SalesType").val();
    var customer = $("#CustomerId").val();
    var currency = $("#CurrencyId").val();
    var terms = $("#TermsId").val();
    var vat = $("#Vat").val();
    var partialAccountType = $("#PartialAccountType").val();
    var partialSalesType = $("#PartialSalesType").val();
    var partialCustomer = $("#PartialCustomerId").val();
    var partialCurrency = $("#PartialCurrencyId").val();
    var partialTerms = $("#PartialTermsId").val();
    var partialVat = $("#PartialVat").val();
    var status = $("#statusId").val();
    var wipDate = $("#WipDate").val();
    var note = $("#WipNote").val();
    var dep = $("#DepartmentId").val();
    var bark = $("#CarPark").val();
    var vehServiceDesc = $("#VehServiceDesc").val();
    var vehConcerns = $("#VehConcerns").val();
    var vehAdvisorNotes = $("#VehAdvisorNotes").val();
    var odometerPrevious = $('#OdoPrev').val();
    var odometerCurrentIN = $('#OdoCurrentIn').val();
    var odometerCurrentOUT = $('#OdoCurrentOut').val();
    var optPartialInv = $("#optPartialInv").is(":checked");
    var optReturnParts = $("#optReturnParts").is(":checked");
    var optRepeatRepair = $("#optRepeatRepair").is(":checked");
    var optUpdateDemand = $("#optUpdateDemand").is(":checked");

    var accountDetails = {
        WIPId: WIPId ?? 0,
        AccountType: accountType,
        SalesType: salesType,
        CustomerId: customer,
        CurrencyId: currency,
        TermsId: terms,
        Vat: vat,
        PartialAccountType: partialAccountType,
        PartialSalesType: partialSalesType,
        PartialCustomerId: partialCustomer,
        PartialCurrencyId: partialCurrency,
        PartialTermsId: partialTerms,
        PartialVat: partialVat
    };

    var vehicleTab = {
        WIPId: WIPId,
        VehicleId: VehId,
        PlateNumber: $('#VehPlate').val(),
        ManufacturerId: $('#VehMakeModel').val(),
        ModelId: $('#VehModel').val(),
        ClassId: $('#VehClass').val(),
        ManufacturingYear: $('#VehYear').val(),
        Color: $('#VehColor').val(),
        ChassisNo: $('#VehVIN').val(),
        VehServiceDesc: vehServiceDesc,
        VehConcerns: vehConcerns,
        VehAdvisorNotes: vehAdvisorNotes,
        DepartmentId: dep,
        CarPark: bark,
        OdometerPrevious: odometerPrevious,
        OdometerCurrentIN: odometerCurrentIN,
        OdometerCurrentOUT: odometerCurrentOUT
    };

    var optionsTab = {
        WIPId: WIPId,
        PartialInvoicing: optPartialInv,
        ReturnParts: optReturnParts,
        RepeatRepair: optRepeatRepair,
        UpdateDemand: optUpdateDemand
    };

    var model = {
        Id: WIPId,
        VehicleId: VehId,
        MovementId: MovId,
        Items: itemsJson,
        FK_WarehouseId: WsId,
        Services: ServicesJson,
        AccountDetails: accountDetails,
        VehicleTab: vehicleTab,
        Status: status,
        WipDate: wipDate,
        Note: note,
        Options: optionsTab
    };

    return $.ajax({
        type: 'POST',
        url: window.URLs.editPostUrl,
        dataType: 'json',
        data: model
    });
}
function CreateIssueVoucher(row, stockType = -1, nextStatus = 42, TransactionType = 1) {
    var valselect = "";
    let isValid = true;
    let message = "";
    var wipid = $("#Id").val();

    let formData = new FormData();

    const detailsList = [{
        FK_ItemId: row.ItemId,
        FK_UnitId: row.fk_UnitId,
        KeyId: row.Id,
        Quantity: row.RequestQuantity,
        UnitQuantity: row.Quantity,
        Price: row.Price || 0,
        Total: row.CostPrice * row.Quantity,
        FK_LocatorId: row.LocatorId,
        FK_WarehouseId: row.WarehouseId
    }];

    formData.append("Details", JSON.stringify(detailsList));

    var submitBtn = $("#IssueRequestPartsBTN");
    var originalText = submitBtn.html();
    submitBtn.html('<i class="fa fa-spinner fa-spin me-1"></i>').prop('disabled', true);
    const warehouseId = row.WarehouseId;

    formData.append("TransactionDate", Date.now);
    formData.append("TransactionReferenceNo", wipid);
    formData.append("RequestId", null);
    formData.append("FK_TransactionReferenceTypeId", 1006);
    formData.append("FK_WarehouseId", warehouseId);
    formData.append("FK_TransactionTypeId", TransactionType);
    formData.append("FK_TransactionStatusId", 2);
    formData.append("AttachmentPath", "1");
    formData.append("FK_FromWarehouseId", "");
    formData.append("FK_ToWarehouseId", "");
    formData.append("Fk_FinancialTransactionMasterId", "1");
    formData.append("FinancialTransactionNo", "1");
    formData.append("FinancialTransactionTypeNo", "1");
    formData.append("Fk_InvoiceType", 0);
    formData.append("StockType", stockType);
    formData.append("WIPId", wipid);

    const pondFiles = window.cuspond ? window.cuspond.getFiles() : [];
    if (pondFiles.length > 0) {
        formData.append("AttachmentFile", pondFiles[0].file);
    }

    $.ajax({
        type: "POST",
        url: window.URLs.createIssueVoucher,
        data: formData,
        processData: false,
        contentType: false,
        dataType: "json",
        success: function (data, textStatus, xhr) {
            submitBtn.html(originalText).prop('disabled', false);
            if (typeof data === "string") {
                try { data = JSON.parse(data); } catch (e) { }
            }

            if (data && (data.success === true || data.Success === true)) {
                row.PartsIssueId = data.partsIssueId || data.PartsIssueId;

                const grid = $("#mainItemsGrid").dxDataGrid("instance");
                grid.getDataSource().store().update(row.KeyId, row).then(() => grid.refresh());

                updateStatusItem(row, nextStatus);
                return;
            }
            handleFailure(data);
        },
        error: function (xhr) {
            submitBtn.html(originalText).prop('disabled', false);

            let data = xhr.responseJSON;
            if (!data && xhr.responseText) {
                try { data = JSON.parse(xhr.responseText); } catch (e) { }
            }

            if (xhr.status === 409) {
                handleFailure(data, true);
                return;
            }

            const errorMessage =
                (data && (data.message || data.Message)) ||
                resources.error_msg;

            Swal.fire({
                icon: 'error',
                title: resources.error,
                text: errorMessage,
                confirmButtonText: theMainLang == "en" ? 'Ok' : "حسناً",
            });
        }
    });
}
function handleFailure(data, isConflict) {
    const errorMessage =
        (data && (data.message || data.Message)) ||
        (isConflict ? (theMainLang == "en" ? "Insufficient stock for one or more items." : "المخزون غير كافٍ لعنصر/عناصر.")
            : (theMainLang == "en" ? "Validation error." : "خطأ تحقق."));

    // shortages 
    const shortages = (data && (data.shortages || data.Shortages)) || [];

    if (shortages && shortages.length) {
        const html = shortages.map(s => {
            const item = `${s.itemName || s.ItemName || ""} (${s.itemCode || s.ItemCode || ""})`;
            const reqQty = (s.requestedQty ?? s.RequestedQty ?? 0);
            const unit = (s.requestedUnitName || s.RequestedUnitName || "");
            const avlQty = (s.availableQty ?? s.AvailableQty ?? 0);
            const shQty = (s.shortageQty ?? s.ShortageQty ?? 0);
            const wh = (s.warehouseName || s.WarehouseName || "");
            const loc = (s.locatorCode || s.LocatorCode || "-");

            return `
                <div style="text-align:${theMainLang == "en" ? "left" : "right"}; margin-bottom:8px;">
                    <div><b>${item}</b></div>
                    <div>${theMainLang == "en" ? "Requested" : "المطلوب"}: ${reqQty} ${unit}</div>
                    <div>${theMainLang == "en" ? "Available" : "المتاح"}: ${avlQty}</div>
                    <div>${theMainLang == "en" ? "Shortage" : "العجز"}: ${shQty}</div>
                    <div>${theMainLang == "en" ? "Warehouse/Locator" : "المستودع/الموقع"}: ${wh} / ${loc}</div>
                </div>
            `;
        }).join("<hr/>");

        Swal.fire({
            icon: 'warning',
            title: theMainLang == "en" ? "Insufficient Stock" : "المخزون غير كافٍ",
            html: `<div>${errorMessage}</div><br/>${html}`,
            confirmButtonText: theMainLang == "en" ? "Ok" : "حسناً"
        });
        return;
    }

    // without shortages
    Swal.fire({
        icon: 'error',
        title: theMainLang == "en" ? 'Error Happened' : "حصل خطأ ما",
        text: errorMessage,
        confirmButtonText: theMainLang == "en" ? 'Ok' : "حسناً",
    });
}
function UndoIssueVoucher(row) {
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    const wipId = Number($("#Id").val() || 0);

    if (!(Number(row?.PartsIssueId || 0) > 0)) {
        Swal.fire({
            icon: "warning",
            title: theMainLang == "en" ? "Issue required first" : "يجب تنفيذ الصرف أولاً",
            text: theMainLang == "en"
                ? "You cannot undo before creating the issue voucher."
                : "لا يمكنك تنفيذ التراجع قبل إنشاء سند الصرف."
        });
        return $.Deferred().reject("missing PartsIssueId").promise();
    }

    return $.ajax({
        type: 'POST',
        url: window.RazorVars.undoIssueVoucherUrl,
        dataType: 'json',
        data: {
            PartsIssueId: row.PartsIssueId,
            WIPId: wipId,
            Id: row.Id
        }
    }).done(function (res) {
        if (!res || !res.success) {
            Swal.fire({
                icon: "error",
                title: (res && (res.message || res.errorMessage))
                    ? (res.message || res.errorMessage)
                    : (theMainLang == "en" ? "Error Happened" : "حصل خطأ ما")
            });
            return;
        }

        row.PartsIssueId = null;
        row.Status = 36;
        row.StatusText = statusTextById(36, true);

        updateRowInGrid(row).then(function () {
            grid.refresh();

            Swal.fire({
                icon: "success",
                title: theMainLang == "en" ? "Undo completed" : "تم إلغاء الصرف بنجاح"
            }).then(() => {
                location.reload();
            });
        });
    }).fail(function (xhr, status, error) {
        console.error("Undo failed:", status, error, xhr?.responseText);

        Swal.fire({
            icon: "error",
            title: theMainLang == "en" ? "Error Happened" : "حصل خطأ ما",
            text: xhr?.responseJSON?.message || xhr?.responseText || error || status || ""
        });
    });
}

function TransferItem(row) {
    window.transferRow = row;
    fillWarehouseDropdowns();
    $("#transferPartModal").modal("show");
}
function fillWarehouseDropdowns() {
    const ddlFrom = $("#transferFromWarehouse");
    const ddlTo = $("#transferToWarehouse");

    ddlFrom.empty();
    ddlTo.empty();

    const ourSet = new Set((OurWarehouses || []).map(x => Number(x)));

    WarehouseData.forEach(w => {
        const id = Number(w.Value);

        // From: NOT in our warehouses
        if (!ourSet.has(id)) {
            ddlFrom.append(`<option value="${w.Value}">${w.Text}</option>`);
        }

        // To: ONLY in our warehouses
        if (ourSet.has(id)) {
            ddlTo.append(`<option value="${w.Value}">${w.Text}</option>`);
        }
    });

    if (window.transferRow) {
        ddlFrom.val(window.transferRow.WarehouseId);
    }
}

function loadTransferLocators() {
    const ddlLoc = $("#transferFromLocator");
    const fromWh = Number($("#transferFromWarehouse").val() || 0);

    ddlLoc.empty().append(`<option value="">Select locator</option>`);
    window.transferLocatorsCache = [];

    if (!fromWh || !window.transferRow) return;

    const dto = {
        ItemId: window.transferRow.ItemId,
        Fk_UnitId: Number(window.transferRow.fk_UnitId || 0),
        Fk_WarehouseId: fromWh
    };

    $.ajax({
        url: window.RazorVars.getAvailableLocatorsUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(dto),
        success: function (res) {
            if (!res || !res.success || !Array.isArray(res.data)) return;

            window.transferLocatorsCache = res.data.map(x => ({
                id: Number(x.locatorId ?? x.LocatorId),
                code: (x.locatorCode ?? x.LocatorCode ?? ""),
                qty: Number(x.onHandQtyInUnit ?? x.OnHandQtyInUnit ?? 0)
            }));

            window.transferLocatorsCache.forEach(l => {
                ddlLoc.append(`<option value="${l.id}">${l.code} (Available: ${l.qty})</option>`);
            });

            const preferredId = Number(window.transferRow.LocatorId || 0);
            const exists = window.transferLocatorsCache.some(l => l.id === preferredId);

            let valueToSet = "";
            if (exists) {
                valueToSet = String(preferredId);
            } else {
                const firstVal = ddlLoc.find("option:eq(1)").val(); 
                valueToSet = firstVal ? String(firstVal) : "";
            }

            ddlLoc.val(valueToSet).trigger("change"); 
        },
        error: function (xhr) {
            console.error("Failed to load transfer locators", xhr);
        }
    });
}



$(document).on("change", "#transferFromWarehouse", loadTransferLocators);

$("#transferPartModal").on("shown.bs.modal", loadTransferLocators);

//$(document).on("change", "#transferFromLocator", function () {
//    if (!window.transferRow) return;
//    window.transferRow.LocatorId = Number($(this).val() || 0) || null;
//});

$(document).on("change", "#transferFromLocator", function () {
    if (!window.transferRow) return;

    const selectedId = Number($(this).val() || 0) || null;

    const loc = window.transferLocatorsCache.find(x => x.id === selectedId);

    window.transferRow.LocatorId = selectedId;
    window.transferRow.LocatorCode = loc ? loc.code : null;
    window.transferRow.AvailableQty = loc ? loc.qty : null;
    window.transferRow.MaxQty = Number(window.transferRow.AvailableQty) || 0;

    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid) return;

    grid.getDataSource().store().update(window.transferRow.KeyId, window.transferRow)
        .then(() => grid.refresh());
});


$("#btnConfirmTransfer").on("click", function () {
    const fromWh = $("#transferFromWarehouse").val();
    const toWh = $("#transferToWarehouse").val();
    const fromLocator = $("#transferFromLocator").val();

    if (!fromWh || !toWh) {
        Swal.fire({ icon: "warning", title: theMainLang == "en" ? 'Missing data' : "بعض البيانات مفقودة", text: theMainLang == "en" ? "Please select both warehouses" : "يرجى تحديد كلا المستودعين" });
        return;
    }

    //if (!fromLocator) {
    //    Swal.fire({ icon: "warning", title: "Missing data", text: "Please select locator" });
    //    return;
    //}

    if (fromWh === toWh) {
        Swal.fire({ icon: "warning", title: theMainLang == "en" ? 'Invalid Selection' : "اختيار غير صالح", text: theMainLang == "en" ? "From and To warehouses must be different" : 'يجب أن تكون مستودعات "من" و"إلى" مختلفة.' });
        return;
    }

    $("#transferPartModal").modal("hide");
    TransferParts();
});
function safeRefreshMainItemsGrid() {
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid) return;

    grid.closeEditCell();
    grid.saveEditData().then(() => grid.refresh());
}

function safeUpdateFieldsFromGrid() {
    try {
        updateFieldsFromGrid();
    } catch (err) {
        console.error("updateFieldsFromGrid crashed:", err);
    }
}

//============================================================================================
function priceWfHistory(row) {
    return $.ajax({
        url: window.RazorVars.priceWfHistoryUrl,
        type: "POST",
        dataType: "json",
        data: { MasterId: row.PriceWorkflowMasterId }
    }).done(hist => {

        function parseNetDate(val) {
            if (!val) return null;

            if (val instanceof Date) {
                if (val.getFullYear() <= 1900) return null;
                return val;
            }

            if (typeof val === "number") {
                const d = new Date(val);
                if (isNaN(d) || d.getFullYear() <= 1900) return null;
                return d;
            }

            if (typeof val === "string") {
                const m = /\/Date\((\d+)(?:[+-]\d+)?\)\//.exec(val);
                if (m) {
                    const d = new Date(parseInt(m[1], 10));
                    if (d.getFullYear() <= 1900) return null;
                    return d;
                }

                const d = new Date(val);
                if (!isNaN(d) && d.getFullYear() > 1900) return d;
            }

            return null;
        }

        function formatDateTime(d) {
            const dd = (d.getDate() < 10 ? "0" : "") + d.getDate();
            const mm = (d.getMonth() + 1 < 10 ? "0" : "") + (d.getMonth() + 1);
            const yyyy = d.getFullYear();

            const hh = (d.getHours() < 10 ? "0" : "") + d.getHours();
            const min = (d.getMinutes() < 10 ? "0" : "") + d.getMinutes();

            return { date: `${dd}-${mm}-${yyyy}`, time: `${hh}:${min}` };
        }


        const timeline = (hist || []).map((h, i) => {
            const actionName = h.ActionName ?? h.actionName ?? "";
            const stateName = h.StateName ?? h.stateName ?? "";
            const groupName = h.GroupName ?? h.groupName ?? "";
            const userName = h.UserName ?? h.userName ?? "";
            const reason = h.Reason ?? h.reason ?? "";

            const created = h.CreatedDate ?? h.createdDate ?? h.CreatedAt ?? h.createdAt ?? null;
            const updated = h.UpdatedDate ?? h.updatedDate ?? null;

            const when = updated || created;

            const title = stateName
                ? `${actionName} - ${stateName}`
                : actionName;

            return {
                order: i,
                date: when,
                noDateNormal: false,
                StateName: title,
                GroupName: groupName,
                UserName: userName ? ` - ${userName}` : "",
                Reason: reason
            };
        });

        timeline.sort((a, b) => {
            const da = parseNetDate(a.date);
            const db = parseNetDate(b.date);
            if (!da && !db) return a.order - b.order;
            if (!da) return 1;
            if (!db) return -1;
            return da - db;
        });

        let html = "";
        for (let i = 0; i < timeline.length; i++) {
            const item = timeline[i];
            const d = parseNetDate(item.date);

            if (!d) {
                html += `
                  <section class="${item.noDateNormal ? "year" : "notE year"}">
                    <section>
                      <ul>
                        <li style="${item.noDateNormal ? "" : "color:#ccc;"}">
                          <b>${item.StateName}</b><br>
                          ${item.GroupName || ""}
                        </li>
                      </ul>
                    </section>
                  </section>
                `;
                continue;
            }

            const dt = formatDateTime(d);

            html += `
                <section class="year">
                  <h3>${dt.date} ${dt.time}</h3>
                  <section>
                    <ul>
                      <li>
                        <b>${item.StateName}</b>
                        ${item.UserName || ""}
                        ${item.GroupName ? `<br><small>${item.GroupName}</small>` : ""}
                      </li>
                    </ul>
                  </section>
                </section>
              `;
        }

        const emptyMsg = (theMainLang === "en" ? "No history" : "لا يوجد سجل");
        const align = (theMainLang === "en" ? "left" : "right");
        const dir = (theMainLang === "en" ? "ltr" : "rtl");

        Swal.fire({
            title: theMainLang === "en" ? "Workflow History" : "سجل الاعتماد",
            html: `
                <div style="text-align:${align}; direction:${dir}; max-height:70vh; overflow:auto;">
                  <div id="WfTrackHistory">
                    ${html || `<div style="padding:10px;">${emptyMsg}</div>`}
                  </div>
                </div>
              `,
            width: 800
     
        });

    });
}
function priceWfApprove(row) {
    return $.ajax({
        url: window.RazorVars.priceWfApproveUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            MasterId: row.PriceWorkflowMasterId,
            WipItemId: row.ItemId,
            Id: row.Id,
            ActionId: 1,
            Reason: "",
            WIPId: row.WIPId,
            KeyId: row.KeyId
        })
    }).done(function (res) {

        if (!res || res.success !== true) {
            Swal.fire({
                icon: "error",
                title: (res && res.message) ? res.message : (theMainLang === "en" ? "Error" : "خطأ")
            });
            return;
        }

        row.PriceWorkflowStatus = res.priceWorkflowStatus ?? PriceWorkflowStatusEnum.Pending;
        row.PriceWorkflowStatusText = res.priceWorkflowStatusText ?? (theMainLang === "en" ? "Pending" : "قيد الانتظار");

        updateRowInGrid(row).then(scheduleGridRepaint);

        Swal.fire({
            icon: "success",
            title: row.PriceWorkflowStatus === PriceWorkflowStatusEnum.Approved
                ? (theMainLang === "en" ? "Approved" : "تم الاعتماد")
                : (theMainLang === "en" ? "Step approved (Pending next)" : "تمت الموافقة (بانتظار المرحلة التالية)")
        });
    }).fail(function (xhr) {
        let msg = theMainLang === "en" ? "Request failed" : "فشل الطلب";
        const data = xhr.responseJSON || (() => { try { return JSON.parse(xhr.responseText); } catch { return null; } })();
        if (data && (data.message || data.Message)) msg = data.message || data.Message;
        Swal.fire({ icon: "error", title: msg });
    });
}

function askReason(title) {
    return Swal.fire({
        title: title,
        input: "textarea",
        inputPlaceholder: theMainLang === "en" ? "Write reason..." : "اكتب السبب...",
        showCancelButton: true,
        confirmButtonText: theMainLang === "en" ? "OK" : "موافق",
        cancelButtonText: theMainLang === "en" ? "Cancel" : "إلغاء"
    }).then(r => r.isConfirmed ? (r.value || "").trim() : null);
}

function priceWfReject(row) {
    return askReason(theMainLang === "en" ? "Reject reason" : "سبب الرفض")
        .then(function (reason) {
            if (reason === null) return; // cancel

            return $.ajax({
                url: window.RazorVars.priceWfRejectUrl,
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({
                    MasterId: row.PriceWorkflowMasterId,
                    WipItemId: row.ItemId,
                    Id: row.Id,
                    Reason: reason,
                    WIPId: row.WIPId,
                    KeyId: row.KeyId
                })
            }).done(function (res) {

                if (!res || res.success !== true) {
                    Swal.fire({
                        icon: "error",
                        title: (res && (res.message || res.Message))
                            ? (res.message || res.Message)
                            : (theMainLang === "en" ? "Error" : "خطأ")
                    });
                    return;
                }

                row.PriceWorkflowStatus = res.priceWorkflowStatus ?? PriceWorkflowStatusEnum.Rejected;
                row.PriceWorkflowStatusText = res.priceWorkflowStatusText ?? (theMainLang === "en" ? "Rejected" : "مرفوض");

                updateRowInGrid(row).then(scheduleGridRepaint);

                Swal.fire({
                    icon: "success",
                    title: theMainLang === "en" ? "Rejected" : "تم الرفض"
                });

            }).fail(function (xhr) {
                let msg = theMainLang === "en" ? "Request failed" : "فشل الطلب";
                const data = xhr.responseJSON || (() => { try { return JSON.parse(xhr.responseText); } catch { return null; } })();
                if (data && (data.message || data.Message)) msg = data.message || data.Message;

                Swal.fire({ icon: "error", title: msg });
            });
        });
}

function stackCaption(caption) {
    return String(caption || "")
        .trim()
        .split(/\s+/)           // split by spaces
        .join("<br/>");         // put each word on new line
}

async function openInventoryItemTabsModal(itemId) {
    const returnUrl = `/ItemMaster/TabsEmbed/${encodeURIComponent(itemId)}`;

    const res = await fetch(`${window.RazorVars.getEmbedUrl}?returnUrl=${encodeURIComponent(returnUrl)}`,
        {
            method: "GET",
            credentials: "same-origin"
        }
    );

    if (!res.ok) {
        const txt = await res.text();
        Swal.fire({ icon: "error", title: "Embed Error", text: txt });
        return;
    }

    const data = await res.json();

    showInvFrame(data.iframeSrc);
}


function showInvFrame(url) {
    const host = document.getElementById("invFrameHost");
    const frame = document.getElementById("invItemTabsFrame");

    if (!host || !frame) return;

    // cleanup any broken previous state first
    cleanupInvFrameArtifacts();

    frame.src = url;

    host.style.display = "flex";          // or "block" based on your layout
    host.setAttribute("aria-hidden", "false");

    // force reflow before adding show (helps with flaky transitions)
    void host.offsetWidth;

    host.classList.add("show");
    document.body.classList.add("inv-frame-open");
}

function closeInvFrameHost() {
    const host = document.getElementById("invFrameHost");
    const frame = document.getElementById("invItemTabsFrame");

    if (!host) {
        cleanupInvFrameArtifacts();
        return;
    }

    host.classList.remove("show");
    host.setAttribute("aria-hidden", "true");
    host.style.pointerEvents = "none";

    // wait a bit if you have CSS transition, then fully hide + cleanup
    setTimeout(() => {
        host.style.display = "none";
        host.style.pointerEvents = "";
        if (frame) frame.src = "about:blank";
        cleanupInvFrameArtifacts();
    }, 200);
}

function cleanupInvFrameArtifacts() {
    const host = document.getElementById("invFrameHost");

    if (host) {
        host.classList.remove("show");
        host.style.pointerEvents = "";
        host.setAttribute("aria-hidden", "true");

        // only hide if not actively shown
        if (!host.classList.contains("show")) {
            host.style.display = "none";
        }
    }

    document.body.classList.remove("inv-frame-open", "modal-open");
    document.body.style.overflow = "";
    document.body.style.paddingRight = "";

    // remove any stuck bootstrap backdrops
    document.querySelectorAll(".modal-backdrop, .dx-overlay-shader").forEach(el => {
        el.remove();
    });
}

// ESC
document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
        closeInvFrameHost();
    }
});

// click outside only
document.getElementById("invFrameHost")?.addEventListener("click", function (e) {
    if (e.target === e.currentTarget) {
        closeInvFrameHost();
    }
});

// if you have an X button, wire it explicitly
document.getElementById("btnCloseInvFrame")?.addEventListener("click", function (e) {
    e.preventDefault();
    e.stopPropagation();
    closeInvFrameHost();
});
document.getElementById("invFrameHost")?.addEventListener("click", (e) => {
    if (e.target.id === "invFrameHost") closeInvFrameHost();
});

function UndoReservation(row) {
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    const wipId = Number($("#Id").val() || 0);

    if (!(Number(row?.PartsIssueId || 0) > 0)) {
        Swal.fire({
            icon: "warning",
            title: theMainLang == "en" ? "Issue required first" : "يجب تنفيذ الحجز أولاً",
            text: theMainLang == "en"
                ? "You cannot undo before reserve item."
                : "لا يمكنك تنفيذ التراجع قبل الحجز."
        });
        return $.Deferred().reject("missing PartsIssueId").promise();
    }

    return $.ajax({
        type: 'POST',
        url: window.RazorVars.undoReservationUrl,
        dataType: 'json',
        data: {
            PartReserveId: row.PartsIssueId,
            WIPId: wipId,
            Id: row.Id
        }
    }).done(function (res) {
        if (!res || !res.success) {
            Swal.fire({
                icon: "error",
                title: (res && (res.message || res.errorMessage))
                    ? (res.message || res.errorMessage)
                    : (theMainLang == "en" ? "Error Happened" : "حصل خطأ ما")
            });
            return;
        }

        row.PartsIssueId = null;
        row.Status = 36;
        row.StatusText = statusTextById(36, true);

        updateRowInGrid(row).then(function () {
            grid.refresh();

            Swal.fire({
                icon: "success",
                title: theMainLang == "en" ? "Undo completed" : "تم إلغاء الحجز بنجاح"
            }).then(() => {
                location.reload();
            });
        });
    }).fail(function (xhr, status, error) {
        console.error("Undo failed:", status, error, xhr?.responseText);

        Swal.fire({
            icon: "error",
            title: theMainLang == "en" ? "Error Happened" : "حصل خطأ ما",
            text: xhr?.responseJSON?.message || xhr?.responseText || error || status || ""
        });
    });
}