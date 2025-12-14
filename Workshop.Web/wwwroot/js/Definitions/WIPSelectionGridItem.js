//var allCategories = [];
let allItemsOurs = [];
let allItemsOthers = [];
function initializeItemSelectionGrid() {

    $("#gridContainer").dxDataGrid({
        dataSource: {
            store: {
                type: "array",
                key: "id",
                data: []
            }
        },
        remoteOperations: {
            filtering: true,
            sorting: true,
            paging: true
        },
        columns: [
            { dataField: "id", caption: "ID", visible: false },
            { dataField: "code", caption: "Code", allowEditing: false },
            {
                dataField: "Name", caption: "Name", allowEditing: false,
                calculateDisplayValue: function (rowData) {
                    return lang === "en" ? rowData?.primaryName : rowData?.secondaryName;
                }
            },
            { dataField: "primaryName", caption: "PrimaryName", visible: false },
            { dataField: "secondaryName", caption: "SecondaryName", visible: false },
            { dataField: "fK_CategoryId", caption: "CategoryId", visible: false },
            {
                dataField: "CategoryName",
                caption: "CategoryName",
                allowEditing: false,
                calculateDisplayValue: function (rowData) {
                    return lang === "en" ? rowData?.categoryPrimaryName : rowData?.categorySecondaryName;
                }
            },
            { dataField: "categoryPrimaryName", caption: "CategoryPrimaryName", visible: false },
            { dataField: "categorySecondaryName", caption: "CategorySecondaryName", visible: false },
            { dataField: "fK_SubCategoryId", caption: "SubCategory", visible: false },
            {
                dataField: "SubCategoryName",
                caption: "Sub Category",
                allowEditing: false,
                calculateDisplayValue: function (rowData) {
                    return lang === "en" ? rowData?.subCategoryPrimaryName : rowData?.subCategorySecondaryName;
                }
            },
            { dataField: "subCategoryPrimaryName", caption: "subCategoryPrimaryName", visible: false },
            { dataField: "subCategorySecondaryName", caption: "subCategorySecondaryName", visible: false },
            { dataField: "fK_UnitId", caption: "Unit", visible: false },
            {
                dataField: "UnitName", caption: "Unit", allowEditing: false,
                calculateDisplayValue: function (rowData) {
                    return lang === "en" ? rowData?.unitPrimaryName : rowData?.unitSecondaryName;
                }
            },
            { dataField: "unitPrimaryName", caption: "unitPrimaryName", visible: false },
            { dataField: "unitSecondaryName", caption: "unitSecondaryName", visible: false },
            { dataField: "avgCost", caption: "Price", dataType: "number", allowEditing: false, visible: true },
            { dataField: "price", caption: "Price", dataType: "number", allowEditing: false, visible: false },
            { dataField: "costPrice", caption: "Cost", dataType: "number", allowEditing: false, visible: false },
            { dataField: "salePrice", caption: "salePrice", dataType: "number", allowEditing: false, visible: false },

            { dataField: "warehouseId", caption: "Warehouse", allowEditing: false, visible: false },
            { dataField: "warehouse", caption: "Warehouse", allowEditing: false, visible: true },
            { dataField: "locatorId", caption: "locatorId", dataType: "number", allowEditing: false, visible: false },
            { dataField: "locatorCode", caption: "locatorCode", allowEditing: false, visible: true },
            { dataField: "availableQty", caption: "availableQty", allowEditing: false, visible: true },
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

            if (selectedRows.length > 0) {
                const transformedData = selectedRows.map(item => ({
                    Id: item.id,
                    ItemId: item.id,
                    Code: item.code,
                    Name: lang == "en" ? item.primaryName : item.secondaryName,
                    PrimaryName: item.primaryName,
                    SecondaryName: item.secondaryName,
                    Quantity: 1,
                    RequestQuantity: 1,
                    Price: item.price ?? 0,
                    CostPrice: item.costPrice ?? 0,
                    SalePrice: item.salePrice ?? 0,
                    Discount: 0,
                    fk_UnitId: item.fK_UnitId,
                    StatusText: "Estimate",
                    //UnitName: lang == "en" ? item.unitPrimaryName : item.unitSecondaryName
                    LocatorId: item.locatorId,
                    LocatorCode: item.locatorCode,
                    AvailableQty: item.availableQty,
                    WarehouseId: item.warehouseId,

                }));
                const existingData = targetGrid.option("dataSource") || [];
                const newDataSource = existingData.concat(transformedData);

                $("#addPartModal").modal("hide");
                targetGrid.option("dataSource", newDataSource);
            }
        });
    }
}

function showItemSelectionModal() {
    initializeItemSelectionGrid();
    $("#ours-tab").addClass("active");
    $("#others-tab").removeClass("active");
    loadItemsData();
}

function loadItemsData() {
    const grid = $("#gridContainer").dxDataGrid("instance");

    $.ajax({
        url: window.RazorVars.getAllItemsUrl,
        method: 'POST',
        dataType: 'json',
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
                title: "Error loading items data",
                showConfirmButton: true,
                confirmButtonText: "ok"
            });
        },
        complete: function () {
            grid.option("loadPanel.enabled", false);
        }
    });
}

//$("#ours-tab").on("click", function () {
//    const grid = $("#gridContainer").dxDataGrid("instance");
//    grid.option("dataSource", allItemsOurs);
//});

//$("#others-tab").on("click", function () {
//    const grid = $("#gridContainer").dxDataGrid("instance");
//    grid.option("dataSource", allItemsOthers);
//});

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
                title: "Successfuly request",
                showConfirmButton: true,
                confirmButtonText: "ok"
            });
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: "error",
                title: "Fail the request",
                showConfirmButton: true,
                confirmButtonText: "ok"
            });
        },
        complete: function () {
            //grid.option("loadPanel.enabled", false);
        }
    });
});