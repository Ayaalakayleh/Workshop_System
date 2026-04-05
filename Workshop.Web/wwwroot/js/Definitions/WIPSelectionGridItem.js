//var allCategories = [];
let allItemsOurs = [];
let allItemsOthers = [];
let altFilterState = { active: false, ids: [] };

function getActiveBaseItems() {
    return $("#ours-tab").hasClass("active") ? allItemsOurs : allItemsOthers;
}

function applyAlternativeFilter(ids) {
    const grid = $("#gridContainer").dxDataGrid("instance");
    if (!grid) return;

    const set = new Set((ids || []).filter(Boolean));
    const base = getActiveBaseItems();

    const filtered = base.filter(x => set.has(x.id));

    grid.option("dataSource", filtered);

    altFilterState.active = true;
    altFilterState.ids = Array.from(set);
}

function clearAlternativeFilter() {
    const grid = $("#gridContainer").dxDataGrid("instance");
    if (!grid) return;

    grid.option("dataSource", getActiveBaseItems());

    altFilterState.active = false;
    altFilterState.ids = [];
}

$("#btnShowAllItems").on("click", function () {
    clearAlternativeFilter();
});

function initializeItemSelectionGrid() {

    $("#gridContainer").dxDataGrid({
        dataSource: {
            store: {
                type: "array",
                key: "id",
                data: []
            }
        },
        remoteOperations: false,
        columns: [
            { dataField: "id", caption: theMainLang == "en" ? "ID" : "الرقم", visible: false },
            { dataField: "code", caption: theMainLang == "en" ? "Code" : "الرمز", allowEditing: false },
            {
                dataField: "Name",
                caption: theMainLang == "en" ? "Name" : "الاسم",
                allowEditing: false,
                calculateCellValue: function (rowData) {
                    return lang === "en" ? rowData?.primaryName : rowData?.secondaryName;
                }
            },
            { dataField: "primaryName", caption: theMainLang == "en" ? "Primary Name" : "الاسم الأساسي", visible: false },
            { dataField: "secondaryName", caption: theMainLang == "en" ? "Secondary Name" : "الاسم الثانوي", visible: false },

            { dataField: "availableQty", caption: theMainLang == "en" ? "Available Qty" : "الكمية المتاحة", allowEditing: false, visible: true, width: 90 },

            { dataField: "warehouseId", caption: theMainLang == "en" ? "Warehouse ID" : "رقم المستودع", allowEditing: false, visible: false },
            { dataField: "warehouse", caption: theMainLang == "en" ? "Warehouse" : "المستودع", allowEditing: false, visible: true },

            { dataField: "locatorId", caption: theMainLang == "en" ? "Locator Id" : "رقم الموقع", dataType: "number", allowEditing: false, visible: false },
            { dataField: "locatorCode", caption: theMainLang == "en" ? "Locator Code" : "رمز الموقع", allowEditing: false, visible: true },

            { dataField: "fK_CategoryId", caption: theMainLang == "en" ? "CategoryId" : "رمز الفئة", visible: false },
            {
                dataField: "CategoryName",
                caption: theMainLang == "en" ? "Category Name" : "اسم الفئة",
                allowEditing: false,
                calculateCellValue: function (rowData) {
                    return lang === "en" ? rowData?.categoryPrimaryName : rowData?.categorySecondaryName;
                }
            },
            { dataField: "categoryPrimaryName", caption: theMainLang == "en" ? "Category Primary Name" : "الاسم الأساسي للفئة", visible: false },
            { dataField: "categorySecondaryName", caption: theMainLang == "en" ? "Category Secondary Name" : "الاسم الثانوي للفئة", visible: false },
            { dataField: "fK_SubCategoryId", caption: theMainLang == "en" ? "Sub Category" : "الفئة الفرعية", visible: false },
            {
                dataField: "SubCategoryName",
                caption: theMainLang == "en" ? "Sub Category" : "الفئة الفرعية",
                allowEditing: false,
                calculateCellValue: function (rowData) {
                    return lang === "en" ? rowData?.subCategoryPrimaryName : rowData?.subCategorySecondaryName;
                }
            },
            { dataField: "subCategoryPrimaryName", caption: theMainLang == "en" ? "Sub Category Primary Name" : "الاسم الأساسي للفئة الفرعية", visible: false },
            { dataField: "subCategorySecondaryName", caption: theMainLang == "en" ? "Sub Category Secondary Name" : "الاسم الثانوي للفئة الفرعية", visible: false },

            { dataField: "fK_UnitId", caption: theMainLang == "en" ? "Unit" : "الوحدة", visible: false },
            {
                dataField: "UnitName",
                caption: theMainLang == "en" ? "Unit" : "الوحدة",
                allowEditing: false,
                calculateCellValue: function (rowData) {
                    return lang === "en" ? rowData?.unitPrimaryName : rowData?.unitSecondaryName;
                }
            },
            { dataField: "unitPrimaryName", caption: theMainLang == "en" ? "Unit Primary Name" : "الاسم الأساسي للوحدة", visible: false },
            { dataField: "unitSecondaryName", caption: theMainLang == "en" ? "Unit Secondary Name" : "الاسم الثانوي للوحدة", visible: false },

            { dataField: "avgCost", caption: theMainLang == "en" ? "Avg Cost" : "معدل السعر", dataType: "number", allowEditing: false, visible: true, width: 70, },
            { dataField: "price", caption: theMainLang == "en" ? "Price" : "السعر", dataType: "number", allowEditing: false, visible: false },
            { dataField: "costPrice", caption: theMainLang == "en" ? "Cost" : "التكلفة", dataType: "number", allowEditing: false, visible: false },
            { dataField: "salePrice", caption: theMainLang == "en" ? "Sale Price" : "سعر البيع", dataType: "number", allowEditing: false, visible: false },
            {
                type: "buttons",
                width: 70,
                alignment: "left",
                buttons: [
                    {
                        hint: "Alternatives",
                        icon: "search",
                        type: "success",
                        stylingMode: "contained",
                        visible: function (e) {
                            return (e.row.data.availableQty == 0 );
                        },
                        onClick: function (e) {

                            const itemId = e.row.data.id;

                            Swal.fire({
                                icon: "question",
                                title: theMainLang == "en"
                                    ? "Do you want to include alternative of alternatives?"
                                    : "هل تود عرض البدائل؟",
                                showDenyButton: true,
                                confirmButtonText: theMainLang == "en" ? "Yes" : "نعم",
                                denyButtonText: theMainLang == "en" ? "No" : "لا",
                                allowOutsideClick: false
                            }).then(function (r) {

                                if (!r.isConfirmed && !r.isDenied) return;

                                const includeIndirectAlternatives = r.isConfirmed;

                                const grid = $("#gridContainer").dxDataGrid("instance");

                                $.ajax({
                                    url: window.RazorVars.getAlternativeItemsUrl,
                                    method: "GET", 
                                    dataType: "json",
                                    data: {
                                        ItemId: itemId,
                                        includeIndirectAlternatives: includeIndirectAlternatives
                                    },
                                    beforeSend: function () {
                                        grid?.option("loadPanel.enabled", true);
                                    },
                                    success: function (res) {
                                        const ids = (res?.items || []).map(x => x.id);

                                        if (!ids.length) {
                                            Swal.fire({
                                                icon: "info",
                                                title: theMainLang == "en" ? "No alternatives found" : "لا يوجد بدائل",
                                                showConfirmButton: true,
                                                confirmButtonText: theMainLang == "en" ? "Ok" : "حسناً",
                                            });
                                            return;
                                        }

                                        //Filter
                                        applyAlternativeFilter(ids);
                                    },
                                    error: function () {
                                        Swal.fire({
                                            icon: "error",
                                            title: theMainLang == "en" ? "Error Happened" : "حصل خطأ ما",
                                            showConfirmButton: true,
                                            confirmButtonText: theMainLang == "en" ? "Ok" : "حسناً",
                                        });
                                    },
                                    complete: function () {
                                        grid?.option("loadPanel.enabled", false);
                                    }
                                });
                            });
                        }

                    },
                    {
                        hint: theMainLang == "en" ? "Create Material Request" : "إنشاء طلب شراء داخلي",
                        icon: "cart",
                        type: "default",
                        stylingMode: "contained",
                        visible: function (e) {
                            return Number(e.row?.data?.availableQty || 0) === 0;
                        },
                        onClick: function (e) {
                            const row = e.row.data;

                            const itemId = row.id;
                            const wipId = parseInt($("#Id").val() || "0");
                            const qty = 1;

                            const url = window.RazorVars.internalPurchaseRequestEditUrl
                                + "?FromWhere=1"
                                + "&itemId=" + encodeURIComponent(itemId)
                                + "&wipId=" + encodeURIComponent(wipId)
                                + "&qty=" + encodeURIComponent(qty);

                            window.location.href = url;
                        }
                    }
                ]
            }
        ],

        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnAutoWidth: true,
        filterRow: {
            visible: true,
            applyFilter: "auto"
        },
        headerFilter: {
            visible: true
        },
        paging: {
            pageSize: 10
        },
        pager: {
            visible: true,
            showPageSizeSelector: true,
            allowedPageSizes: [5, 10, 20, 50],
            showInfo: true,
            showNavigationButtons: true
        },
        selection: {
            mode: "multiple",
            showCheckBoxesMode: "always",
            selectAllMode: "page"
        },
        rowAlternationEnabled: true,
        hoverStateEnabled: true,
        onSelectionChanged: function (e) {
            updateSelectionCount(e.selectedRowsData.length);
        },
        onContentReady: function (e) {

        }
    });
}

function updateSelectionCount(count) {
    const btn = $("#selectItemsBtn");
    btn.off("click");
    if (count > 0) {
        btn.on("click", function () {
            const sourceGrid = $("#gridContainer").dxDataGrid("instance");
            const targetGrid = $("#mainItemsGrid").dxDataGrid("instance");

            const selectedRows = sourceGrid.getSelectedRowsData();


            const needApproval = selectedRows.some(x => x.requiresPriceApproval === true);
            if (needApproval) {
                Swal.fire({
                    icon: "warning",
                    title: theMainLang == "en" ? "Price approval required" : "مطلوب اعتماد سعر",
                    text: theMainLang == "en"
                        ? "One or more selected parts require price approval. Workflow will start after saving."
                        : "يوجد قطع مختارة تحتاج اعتماد سعر. سيتم بدء سير العمل بعد الحفظ.",
                    confirmButtonText: theMainLang == "en" ? "OK" : "حسناً"
                });
            }



            if (selectedRows.length > 0) {

                const _existingData = targetGrid.option("dataSource") || [];

                let nextKeyId = 1;
                if (_existingData.length > 0) {
                    const maxKeyId = Math.max(..._existingData.map(r => parseInt(r.KeyId, 10) || 0));
                    nextKeyId = maxKeyId + 1;
                }
                debugger
                const transformedData = selectedRows.map(item => ({
                    KeyId: nextKeyId++,
                    Id: item.id,
                    ItemId: item.id,
                    Code: item.code,
                    Name: lang == "en" ? item.primaryName : item.secondaryName,
                    PrimaryName: item.primaryName,
                    SecondaryName: item.secondaryName,
                    Quantity: 1,
                    RequestQuantity: 1,
                    Price: item.price ?? 0,
                    BaseCostPrice: item.costPrice ?? 0,
                    CostPrice: item.costPrice ?? 0,
                    SalePrice: item.salePrice ?? 0,
                    ItemUnits: [],
                    UnitFactor: 1,
                    Discount: 0,
                    fk_UnitId: item.fK_UnitId,
                    StatusText: "Estimate",
                    //UnitName: lang == "en" ? item.unitPrimaryName : item.unitSecondaryName
                    LocatorId: item.locatorId,
                    LocatorCode: item.locatorCode,
                    AvailableQty: item.availableQty,
                    MaxQty: item.availableQty,
                    WarehouseId: item.warehouseId,
                    isDecimalUnit: item.isDecimalUnit,
                    FK_SubCategoryId: item.fK_SubCategoryId,
                    AccountType: $("#optPartialInv").is(":checked") ? null : parseInt($("#AccountType").val()),
                    RequiresPriceApproval: !!item.requiresPriceApproval,
                    PriceWorkflowEnumId: item.priceWorkflowEnumId ?? null,
                    PriceWorkflowStatus: 0,       
                    PriceWorkflowReason: null,
                    PriceWorkflowMasterId: null

                }));
                const existingData = targetGrid.option("dataSource") || [];
                const newDataSource = existingData.concat(transformedData);

                $("#addPartModal").modal("hide");
                targetGrid.option("dataSource", newDataSource);
                loadUnitsForRows(transformedData);
                //saveWipItemsAuto(transformedData);           
            }
        });
    }
}

//function saveWipItemsAuto(itemsToInsert) {
//    const wipId = parseInt($("#Id").val() || "0");
//    if (!wipId || wipId <= 0) return;

//    if (!Array.isArray(itemsToInsert) || itemsToInsert.length === 0) return;

//    const baseItems = itemsToInsert.map(x => ({
//        WIPId: wipId,
//        KeyId: x.KeyId,
//        ItemId: x.ItemId,
//        fk_UnitId: x.fk_UnitId,
//        Quantity: x.Quantity ?? 1,
//        RequestQuantity: x.RequestQuantity ?? x.Quantity ?? 1,
//        CostPrice: x.CostPrice ?? 0,
//        SalePrice: x.SalePrice ?? 0,
//        Discount: x.Discount ?? 0,
//        LocatorId: x.LocatorId ?? null,
//        WarehouseId: x.WarehouseId ?? null
//    }));

    //return $.ajax({
    //    url: window.RazorVars.insertItemsUrl,
    //    method: "POST",
    //    dataType: "json",
    //    data: {
    //        WIPId: wipId,
    //        ItemsList: baseItems
    //    },
    //    error: function () {
    //        Swal.fire({
    //            icon: "error",
    //            title: theMainLang == "en" ? 'Error Happened' : "حصل خطأ ما",
    //            showConfirmButton: true,
    //            confirmButtonText: theMainLang == "en" ? 'Ok' : "حسناً",
    //        });
    //    }
    //});
//}

window.itemUnitsCache = window.itemUnitsCache || {};

function fetchItemUnitsCached(itemId) {
    if (window.itemUnitsCache[itemId]) {
        return $.Deferred().resolve(window.itemUnitsCache[itemId]).promise();
    }

    return $.getJSON(window.RazorVars.getItemUnitsUrl, { itemId: itemId })
        .then(function (units) {
            units = Array.isArray(units) ? units : [];
            window.itemUnitsCache[itemId] = units;
            return units;
        });
}
function getRowAccountType(row) {
    const type =
        (row.AccountType != null && row.AccountType !== 0 && row.AccountType !== "")
            ? String(row.AccountType)
            : String($("#AccountType").val() || "");
    return type;
}

function recalcCostPriceForUnit(row) {
    const type = getRowAccountType(row);

    if (type === "1") {
        const factor = Number(row.UnitFactor) || 1;

        const baseCost = Number(row.BaseCostPrice) || 0;

        row.CostPrice = +(baseCost / factor).toFixed(2);

        row.Price = row.CostPrice;
    }
}

function loadUnitsForRows(rows) {
    if (!rows || !rows.length) return;

    const grid = $("#mainItemsGrid").dxDataGrid("instance");
    if (!grid) return;

    const store = grid.getDataSource().store();
    debugger
    const tasks = rows.map(r => {
        return fetchItemUnitsCached(r.ItemId).then(units => {

            r.ItemUnits = Array.isArray(units) ? units : [];

            let selectedUnit = r.ItemUnits.find(x => x.unitId == r.fk_UnitId);

            if (!selectedUnit) {
                selectedUnit = r.ItemUnits.find(x => x.isBaseUnit) || r.ItemUnits[0];
                if (selectedUnit) {
                    r.fk_UnitId = selectedUnit.unitId;
                }
            }

            r.UnitFactor = Number(selectedUnit?.conversionFactor) || 1;
            r.isDecimalUnit = !!selectedUnit?.isDecimalUnit;
            
            if (!r.BaseCostPrice || Number(r.BaseCostPrice) === 0) {
                const costNow = Number(r.CostPrice) || 0;
                r.BaseCostPrice = +(costNow * r.UnitFactor).toFixed(2);
            }

            recalcCostPriceForUnit(r);

            return store.update(r.KeyId, r);
        });
    });

    $.when.apply($, tasks).then(function () {
        grid.refresh();
    });
}

function showItemSelectionModal() {
    initializeItemSelectionGrid();
    $("#ours-tab").addClass("active");
    $("#others-tab").removeClass("active");
    loadItemsData();
}

function loadItemsData() {
    const grid = $("#gridContainer").dxDataGrid("instance");
    const wipId = parseInt($("#Id").val() || "0");

    $.ajax({
        url: window.RazorVars.getAllItemsUrl,
        method: 'POST',
        dataType: 'json',
        data: { wipId: wipId },
        beforeSend: function () {
            grid.option("loadPanel.enabled", true);
        },
        success: function (res) {
            if (res) {
                allItemsOurs = res.ours || [];
                allItemsOthers = res.others || [];
                grid.option("dataSource", allItemsOurs);
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: theMainLang == "en" ? 'Error Happened' : "حصل خطأ ما",
                showConfirmButton: true,
                confirmButtonText: theMainLang == "en" ? 'Ok' : "حسناً",
            });
        },
        complete: function () {
            grid.option("loadPanel.enabled", false);
        }
    });
}



$("#ours-tab, #others-tab").on("click", function () {
    // toggle active class
    $("#ours-tab, #others-tab").removeClass("active");
    $(this).addClass("active");

    // switch datasource
    const grid = $("#gridContainer").dxDataGrid("instance");
    if (!grid) return;

    if (this.id === "ours-tab") {
        grid.option("dataSource", allItemsOurs);
    } else {
        grid.option("dataSource", allItemsOthers);
    }

    if (altFilterState.active) {
        applyAlternativeFilter(altFilterState.ids);
    }
});



//---------------------------------------------------------------------------------------------------------------------------------------
$("#btnAddPart").on("click", function () {
    showItemSelectionModal()
});


$("#btnAddPartGeneral").on("click", function () {
    var WIPID = $("#Id").val();
    var desc = $("#RequestDescription").val();
    $.ajax({
        url: window.RazorVars.generalRequestUrl,
        method: 'Post',
        dataType: 'json',
        data: { WIPId: WIPID, RequestDescription: desc },
        success: function (result) {
            Swal.fire({
                icon: "success",
                title: theMainLang == "en" ? 'Success' : "تمت العملية بنجاح",
                showConfirmButton: true,
                confirmButtonText: theMainLang == "en" ? 'Ok' : "حسناً",
            });
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: "error",
                title: theMainLang == "en" ? 'Error Happened' : "حصل خطأ ما",
                showConfirmButton: true,
                confirmButtonText: theMainLang == "en" ? 'Ok' : "حسناً",
            });
        },
        complete: function () {
            //grid.option("loadPanel.enabled", false);
        }
    });
});