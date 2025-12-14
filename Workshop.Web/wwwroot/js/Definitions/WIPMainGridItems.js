window.transferLocatorsCache = [];
 
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
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid) return $.Deferred().resolve().promise();

    const store = grid.getDataSource().store();
    return store.update(row.Id, row).then(() => {
        const idx = grid.getRowIndexByKey(row.Id);
        if (idx >= 0) grid.repaintRows([idx]);
        else scheduleGridRepaint();

        if (typeof updateFieldsFromGrid === "function") updateFieldsFromGrid();
    });
}


$(function () {
    let locatorsLoadedOnInit = false;
    $("#mainItemsGrid").dxDataGrid({
        dataSource: ItemsData,
        keyExpr: "Id",
        noDataText: resources.NoDataInTable,
        showBorders: true,
        remoteOperations: {
            filtering: true,
            sorting: true,
            paging: true
        },
        columns: [
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
            {
                dataField: "WarehouseId",
                caption: "Warehouse",
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
                    $("<div>").dxSelectBox({
                        dataSource: UnitsData,
                        valueExpr: "Value",
                        displayExpr: "Text",
                        value: cellInfo.value,
                        onValueChanged: function (e) {
                            const row = cellInfo.data;

                            row.fk_UnitId = e.value;
                            row.LocatorId = null;
                            row.LocatorCode = null;
                            row.AvailableQty = null;
                            row.AvailableLocators = [];

                            cellInfo.setValue(e.value);

                            fillLocators([row]);

                            const grid = $("#mainItemsGrid").dxDataGrid("instance");
                            grid.getDataSource().store().update(row.Id, row).then(() => {
                                grid.refresh();
                            });
                        }
                    }).appendTo(cellElement);
                }
            },
            {
                dataField: "RequestQuantity",
                caption: window.RazorVars.DXRequestQuantity,
                dataType: "number",
                allowEditing: generalRequest == false ? true : false,
                alignment: "left"
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
                dataType: "number",
                allowEditing: true,
                alignment: "left",
                editCellTemplate: function (cellElement, cellInfo) {
                    const row = cellInfo.data;
                    let maxQty = row.MaxQty != null ? Number(row.MaxQty) : null;

                    const initialVal = cellInfo.value != null ? cellInfo.value : 0;

                    $("<div>").dxNumberBox({
                        value: initialVal,
                        min: 0,
                        max: maxQty,
                        showSpinButtons: true,
                        inputAttr: { "autocomplete": "off" },
                        onValueChanged: function (e) {
                            let v = Number(e.value) || 0;

                            if (maxQty != null && v > maxQty) {
                                v = maxQty;
                                e.component.option("value", maxQty);
                            }

                            cellInfo.setValue(v);
                            row.Quantity = v;
                        }
                    }).appendTo(cellElement);
                }
            },
            {
                dataField: "UsedQuantity",
                caption: window.RazorVars.DXUsedQuantity,
                dataType: "number",
                allowEditing: true,
                alignment: "left",
                editCellTemplate: function (cellElement, cellInfo) {
                    $("<div>").dxNumberBox({
                        value: cellInfo.value,
                        min: 0,
                        max: cellInfo.data.Quantity,
                        showSpinButtons: true,
                        onValueChanged: function (e) {
                            cellInfo.setValue(e.value);

                            var isLess = e.value < cellInfo.data.Quantity;
                            $("#optReturnParts").prop("checked", isLess);

                            updateFieldsFromGrid();
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
                    var type = $("#AccountType").val();

                    var discount = parseFloat($("#_DiscountPercentagePart").val());
                    if (isNaN(discount)) discount = 0;

                    var basePrice = 0;

                    if (type === '1') {
                        basePrice = parseFloat(rowData.CostPrice);
                        if (isNaN(basePrice)) basePrice = 0;
                    } else if (type === '2') {
                        basePrice = parseFloat(rowData.SalePrice);
                        if (isNaN(basePrice)) basePrice = 0;

                        basePrice = basePrice - (basePrice * (discount / 100));
                    }

                    var discountedPrice = Number(basePrice) || 0;

                    rowData.Price = discountedPrice;
                    return Number(discountedPrice.toFixed(2));
                }
            },
            {
                dataField: "Discount",
                caption: window.RazorVars.DXDiscount,
                dataType: "number",
                allowEditing: true,
                defaultValue: 0,
                alignment: "left",
                editorOptions: {
                    min: 0,
                    max: 100
                }
            },
            {
                dataField: "Tax",
                caption: window.RazorVars.DXTax,
                dataType: "number",
                allowEditing: true,
                alignment: "left",
                calculateCellValue: function (rowData) {
                    var vatId = $("#Vat").val();
                    var vatPercent = parseFloat(GetVatValueById(vatId) / 100) || 0;

                    var qty = parseFloat(rowData.Quantity) || 0;

                    var price = parseFloat(rowData.Price) || 0;

                    var taxAmount = qty * price * vatPercent;

                    rowData.Tax = taxAmount.toFixed(2);

                    return parseFloat(taxAmount.toFixed(2));
                }
            },
            {
                dataField: "Total",
                caption: window.RazorVars.DXTotal,
                dataType: "number",
                allowEditing: false,
                alignment: "left",
                calculateCellValue: function (rowData) {
                    var requestQuantity = parseFloat(rowData.RequestQuantity) || 0;
                    var quantity = parseFloat(rowData.Quantity) || 0;
                    var price = parseFloat(rowData.Price) || 0;
                    var tax = parseFloat(rowData.Vat) || 0;
                    var _rowDiscount = parseFloat(rowData.Discount) || 0;

                    var Qty = quantity > 0 ? quantity : requestQuantity;

                    var totalValue = (Qty * price) + tax;

                    if (_rowDiscount > 0) {
                        totalValue -= totalValue * (_rowDiscount / 100);
                    }

                    rowData.Total = totalValue;
                    return parseFloat(totalValue.toFixed(2));
                }
            },
            {
                dataField: "PartsIssueId", allowEditing: false,
                caption: "Issue No.",
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
                width: 170,
                alignment: "left",
                buttons: [
                    {
                        hint: "Approve",
                        icon: "check",
                        type: "success",
                        stylingMode: "contained",
                        visible: function (e) {
                            return AllowActions && parseInt(e.row.data.Status) !== 42;
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
                            return AllowActions && parseInt(e.row.data.Status) !== 42;
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
                            return AllowActions &&
                                parseInt(e.row.data.Status) !== 41 &&
                                OurWarehouses.includes(parseInt(e.row.data.WarehouseId));
                        },
                        disabled: function (e) {
                            return parseInt(e.row.data.Status) === 42;
                        },
                        onClick: function (e) {
                            if (parseInt(e.row.data.Status) !== 36) {
                                Swal.fire({
                                    icon: "warning",
                                    title: "Please request approval first",
                                    text: "You cannot issue this item until it is approved."
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
                            debugger;
                            return AllowActions &&
                                parseInt(e.row.data.Status) !== 41 &&
                                OurWarehouses.includes(parseInt(e.row.data.WarehouseId));
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
                            return AllowActions &&
                                parseInt(e.row.data.Status) !== 42 &&
                                parseInt(e.row.data.Status) !== 41 &&
                                !OurWarehouses.includes(parseInt(e.row.data.WarehouseId));
                        },
                        onClick: function (e) {
                            TransferItem(e.row.data);
                        }
                    },
                    {
                        hint: "Delete",
                        icon: "fad fa-trash",
                        visible: function (e) {
                            return parseInt(e.row.data.Status) !== 42 &&
                                !(wipStatus === Gone || wipStatus === Invoiced);
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
        columnAutoWidth: false,
        columnMinWidth: 60,
        wordWrapEnabled: false,
        hoverStateEnabled: true,
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
            if (["Quantity", "Price", "Discount", "UsedQuantity"].includes(e.column.dataField)) {
                e.component.updateDimensions();
            }

            updateFieldsFromGrid();
        },
        onRowRemoved: function () {
            updateFieldsFromGrid();
        },
        onContentReady: function (e) {
            updateFieldsFromGrid();

            if (!locatorsLoadedOnInit) {
                const grid = e.component;
                const rows = grid.getVisibleRows().map(r => r.data);

                fillLocators(rows);

                locatorsLoadedOnInit = true;
            }
        },
        onInitNewRow: function (e) {
            const accountTypeVal = parseInt($("#AccountType").val());
            const partialInvoicing = $("#optPartialInv").is(":checked");

            if (!partialInvoicing) {
                e.data.AccountType = accountTypeVal;
            }
        },
        onRowInserted: function (e) {
            if (typeof updateFieldsFromGrid === "function") {
                updateFieldsFromGrid();
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

            if (e.parentType === "dataRow" && e.dataField === "Discount") {

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
            Fk_UnitId: Number(row.fk_UnitId),
            Fk_WarehouseId: row.WarehouseId
        };

        const p = getLocatorsCached(dto).then(function (locators) {
            row.AvailableLocators = locators || [];

            if (!row.LocatorId && row.AvailableLocators.length > 0) {
                row.LocatorId = row.AvailableLocators[0].locatorId;
                row.LocatorCode = row.AvailableLocators[0].locatorCode;
                row.AvailableQty = row.AvailableLocators[0].onHandQtyInUnit;
            }

            const match = row.AvailableLocators.find(x => x.locatorId == row.LocatorId);
            if (match) {
                row.LocatorCode = match.locatorCode;
                row.AvailableQty = match.onHandQtyInUnit;
            }

            return updateRowInGrid(row);
        });

        updates.push(p);
    });

    $.when.apply($, updates).always(function () {
        scheduleGridRepaint(); 
    });
}


$('#AccountType, #Vat').on('change', function () {
    $('#mainItemsGrid').dxDataGrid('instance').refresh();
});

function updateFieldsFromGrid() {
    var grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid) return;

    var rows = grid.getVisibleRows() || [];
    var totalSum = 0;
    var totalDiscountsPart = 0;

    rows.forEach(function (r) {
        var d = r.data || {};
        var qty = parseFloat(d.Quantity) || parseFloat(d.RequestQuantity) || 0;
        var price = parseFloat(d.Price) || 0;
        var disc = parseFloat(d.Discount) || 0;
        var tax = parseFloat(d.Tax) || 0;

        var base = qty * price;
        var discAmt = base * (disc / 100);
        var lineTot = base - discAmt;

        totalDiscountsPart += discAmt;
        totalSum += lineTot;
    });

    $("#totParts").text("SAR " + totalSum.toFixed(2));
    setAmount("#totParts", totalSum);
    $("#TotalDiscountsPart").text("SAR " + totalDiscountsPart.toFixed(2));
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
    const v = $.trim($("#Vat").val());
    return v === "" ? 0 : (Number(GetVatValueById(v)) || 0);
}

function setVatPercentageText(pct) {
    $("#VatPercentage").text("% " + Number(pct).toFixed(0));
}

function updateSubtotal() {
    const labour = getAmount("#totLabour");
    const parts = getAmount("#totParts");
    setAmount("#totSubtotal", labour + parts);
    updateVatAndTotal();
}

function updateVatAndTotal() {
    const labour = getAmount("#totLabour");
    const parts = getAmount("#totParts");

    const subtotal = getAmount("#totSubtotal");
    const vatPct = getVatPercent();

    const vatAmt = (labour * (vatPct / 100)) + (parts * (vatPct / 100));
    const total = subtotal + vatAmt;

    setVatPercentageText(vatPct);
    setAmount("#totVAT", vatAmt);
    setAmount("#totTotal", total);
}

$(document).on("input change", "#Vat", function () {
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
                title: "Return parts send successfuly!"
            });
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: "error",
                title: "Error loading items data",
                showConfirmButton: true,
                confirmButtonText: "ok"
            });
        }
    });
}

$("#ApproveRequestPartsBTN").on("click", function () {
    UpdatePartStatus(36, "Approved");
});

$("#FK_WarehouseId").on("change", function () {
    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    const data = grid.option("dataSource");

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
        return statusId == 36 ? "Approved" :
            statusId == 37 ? "Rejected" :
                statusId == 41 ? "Waiting Part" :
                    statusId == 42 ? "Part Received" :
                        "Updated";
    } else {
        return statusId == 36 ? window.RazorVars.DXPartApproved :
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

    return $.ajax({
        url: window.RazorVars.updatePartStatusWithSingleItem,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(dto)
    }).done(function (res) {
        if (res && res.success) {
           
            row.Status = statusId;
            row.StatusText = statusTextById(statusId, true);

            updateRowInGrid(row).then(() => {
                Swal.fire({
                    icon: "success",
                    title: statusTextById(statusId, false)
                });
            });
        }
    }).fail(function (xhr, st, err) {
        console.error("Error updating status:", err);
        Swal.fire({ icon: "error", title: "Status update failed" });
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
            debugger;
            if (res.success) {
                UpdatePartStatus(42, "Part Received");
            } else {
                Swal.fire("Error", "Transaction failed.", "error");
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
        UnitQuantity: row.Quantity,
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
                            title: "Transfer request prepared",
                            text: "You can now proceed with stock transfer"
                        });
                    });
                });
            } else {
                Swal.fire("Error", "Transaction failed", "error");
            }
        }).fail(function (err) {
            console.error("Error updating status:", err);
            Swal.fire("Error", "Transaction failed", "error");
        }).always(function () {
            btn.prop("disabled", false).html(oldTxt);
        });
    });
}
function saveData() {
    // RTS - Services
    var Services_grid = $('#mainRTSGrid').dxDataGrid('instance');
    var Services_Items = Services_grid.getDataSource().items();

    var ServicesJson = JSON.stringify(Services_Items);
    $("#Services").val(ServicesJson);

    // Items
    var grid = $('#mainItemsGrid').dxDataGrid('instance');
    var gridItems = grid.getDataSource().items();

    var itemsJson = JSON.stringify(gridItems);
    $("#Items").val(itemsJson);

    var WIPId = $('#Id').val();
    var WsId = 10;//$("#FK_WarehouseId").val();
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
    var inv_AccountType = $("#invAccountType").val();
    var inv_InvoiceNo = $("#InvoiceNo").val();
    var inv_Date = $("#invDate").val();
    var inv_Total = $("#invTotal").val();
    var inv_Tax = $("#invTax").val();
    var inv_Net = $("#invNet").val();

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

    //var invoiceDetails = {
    //    WIPId: WIPId ?? 0,
    //    AccountType: inv_AccountType,
    //    InvoiceNo: inv_InvoiceNo,
    //    InvoiceDate: inv_Date,
    //    Total: inv_Total,
    //    Tax: inv_Tax,
    //    Net: inv_Net
    //};

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

    //$.ajax({
    //    type: 'POST',
    //    url: window.URLs.editPostUrl,
    //    dataType: 'json',
    //    data: model
    //}).done(function (result) {
    //    if (result) {
    //        Swal.fire(
    //            "Success",
    //            'WIP Saved Successfully!'
    //        ).then(() => {
    //            window.location.href = window.URLs.editGetUrl + '?id=' + result.wipId + '&movementId=' + MovId;
    //        });
    //    }
    //}).fail(function (xhr, status, error) {
    //    console.error("Error:", error);
    //});
    $.ajax({
        type: 'POST',
        url: window.URLs.editPostUrl,
        dataType: 'json',
        data: model
    }).done(function (result) {
        //console.log("Edit_Post result:", result);

        //if (result && result.success && result.wipId) {
        //    Swal.fire("Success", "WIP " + WIPId + " Saved Successfully!").then(() => {
        //        window.location.href = window.URLs.editGetUrl + '?id=' + result.wipId + '&movementId=' + MovId;
        //    });
        //} else {
        //    Swal.fire({
        //        icon: "error",
        //        title: "Save Failed",
        //        text: result && result.errorMessage ? result.errorMessage : "Unknown error occurred"
        //    });
        //}

    });

}
function CreateIssueVoucher(row) {
    var valselect = "";
    let isValid = true;
    let message = "";
    var wipid = $("#Id").val();

    let formData = new FormData();

    debugger;
    const detailsList = [{
        FK_ItemId: row.ItemId,
        FK_UnitId: row.fk_UnitId,
        KeyId: row.Id,
        Quantity: row.RequestQuantity,
        UnitQuantity: row.Quantity,
        Price: row.Price || 0,
        Total: row.Total,
        FK_LocatorId: row.LocatorId,
        FK_WarehouseId: row.WarehouseId
    }];
    debugger;
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
    formData.append("FK_TransactionTypeId", 1);
    formData.append("FK_TransactionStatusId", 2);
    formData.append("AttachmentPath", "1");
    formData.append("FK_FromWarehouseId", "");
    formData.append("FK_ToWarehouseId", "");
    formData.append("Fk_FinancialTransactionMasterId", "1");
    formData.append("FinancialTransactionNo", "1");
    formData.append("FinancialTransactionTypeNo", "1");
    formData.append("Fk_InvoiceType", 0);
    formData.append("StockType", -1);
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
        success: function (data) {
            submitBtn.html(originalText).prop('disabled', false);

            if (data == -2) {
                Swal.fire({
                    icon: 'error',
                    title: "T.codeAlreadyExists" || 'Code already exists',
                    confirmButtonText: resources.ok
                });
            } else if (data == -3) {
                Swal.fire({
                    icon: 'error',
                    title: "T.nameAlreadyExists" || 'Name already exists',
                    confirmButtonText: resources.ok
                });
            } else if (data == -4) {
                Swal.fire({
                    icon: 'error',
                    title: "T.secondaryAlreadyExists" || 'Secondary already exists',
                    confirmButtonText: resources.ok
                });
            } else {
                if (data && (data.success === true || data.Success === true || !data.hasOwnProperty('success'))) {
                    row.PartsIssueId = data.partsIssueId;
                    debugger;
                    const grid = $("#mainItemsGrid").dxDataGrid("instance");
                    grid.getDataSource().store().update(row.Id, row).then(() => {
                        grid.refresh();
                    });

                    updateStatusItem(row, 42);
                } else {
                    const errorMessage =
                        data.message ||
                        data.Message ||
                        "T.validationErrorDefault" ||
                        'Failed to create the GRN due to validation errors.';
                    Swal.fire({
                        icon: 'error',
                        title: "T.validationErrorTitle " || 'Validation Error',
                        text: errorMessage,
                        confirmButtonText: 'OK'
                    });
                }
            }
        },
        error: function (xhr) {
            submitBtn.html(originalText).prop('disabled', false);

            let errorMessage = resources.error_msg;

            if (xhr.responseJSON && (xhr.responseJSON.message || xhr.responseJSON.Message)) {
                errorMessage = xhr.responseJSON.message || xhr.responseJSON.Message;
            } else if (xhr.responseText) {
                try {
                    const errorData = JSON.parse(xhr.responseText);
                    errorMessage = errorData.message || errorData.Message || errorMessage;
                } catch (e) { }
            }

            Swal.fire({
                icon: 'error',
                title: resources.error,
                text: errorMessage,
                confirmButtonText: 'OK'
            });
        }
    });
}

function UndoIssueVoucher(row) {
    $.ajax({
        type: 'GET',
        url: window.RazorVars.undoIssueVoucherUrl,
        dataType: 'json',
        data: {
            PartsIssueId: row.PartsIssueId,
            WIPId: row.WIPId
        },
        success: function (res) {
            if (res.success) {
                Swal.fire({
                    icon: "success",
                    title: "Undo completed"
                }).then(() => $("#mainItemsGrid").dxDataGrid("instance")?.repaint());
            } else {
                Swal.fire("Error", "Undo failed.", "error");
            }
        },
        error: function (err) {
            console.error("Error:", err);
        }
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

    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid) return;

    grid.getDataSource().store().update(window.transferRow.Id, window.transferRow)
        .then(() => grid.refresh());
});


$("#btnConfirmTransfer").on("click", function () {
    const fromWh = $("#transferFromWarehouse").val();
    const toWh = $("#transferToWarehouse").val();
    const fromLocator = $("#transferFromLocator").val();

    if (!fromWh || !toWh) {
        Swal.fire({ icon: "warning", title: "Missing data", text: "Please select both warehouses" });
        return;
    }

    //if (!fromLocator) {
    //    Swal.fire({ icon: "warning", title: "Missing data", text: "Please select locator" });
    //    return;
    //}

    if (fromWh === toWh) {
        Swal.fire({ icon: "warning", title: "Invalid selection", text: "From and To warehouses must be different" });
        return;
    }

    $("#transferPartModal").modal("hide");
    TransferParts();
});
