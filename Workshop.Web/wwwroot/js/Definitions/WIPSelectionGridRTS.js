
function initialize_RTSGrid() {

    $("#rtsSelection").dxDataGrid({
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
            { dataField: "primaryName", caption: "Name EN", allowEditing: false },
            { dataField: "secondaryName", caption: "Name AR", allowEditing: false },
            { dataField: "hours", caption: "Hours", allowEditing: false },
            //{ dataField: "price", caption: "Price", allowEditing: false },
            //{ dataField: "fK_SkillId", caption: "SkillId", visible: false },
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
            injectMainGrid(e.selectedRowsData.length);
        }
    });
}
let mainGridRowCounter = 0;
function injectMainGrid(count) {
    const btn = $("#selectBtn");
    btn.off("click");
    if (count > 0) {
        btn.on("click", function () {
            const sourceGrid = $("#rtsSelection").dxDataGrid("instance");
            const targetGrid = $("#mainRTSGrid").dxDataGrid("instance");

            const selectedRows = sourceGrid.getSelectedRowsData();
            if (selectedRows.length > 0) {
                const existingRows = targetGrid.option("dataSource") || [];

                let nextKeyId = 1;
                if (existingRows.length > 0) {
                    const maxKeyId = Math.max(
                        ...existingRows.map(r => parseInt(r.KeyId, 10) || 0)
                    );
                    nextKeyId = maxKeyId + 1;
                }



                const newItems = [];

                selectedRows.forEach(item => {
                    newItems.push({
                        KeyId: nextKeyId++, 
                        Id: item.id,
                        ItemId: item.id,
                        Code: item.code,
                        Description: lang == "en" ? item.primaryName : item.secondaryName,
                        LongDescription: lang == "en" ? item.primaryDescription : item.secondaryDescription,
                        StandardHours: item.hours,
                        Rate: 0,
                        BaseRate: 0,
                        Total: 0,
                        Status: LabourLineEnum[1].value,
                        StatusText: LabourLineEnum[1].text
                    });
                });

                if (newItems.length > 0) {
                    const updated = existingRows.concat(newItems);
                    targetGrid.option("dataSource", updated);
                    targetGrid.getDataSource().reload();
                    targetGrid.refresh(true);
                    targetGrid.saveEditData();
                    newItems.forEach(item => getRateAmount(item.KeyId, item.Id));

                    $("#addRtsModal").modal("hide");
                }
            }
        });
    }
}

let pendingRateCalls = 0;

function setSaveBusy(isBusy) {
    const $btn = $("#btnSave2");
    $("#saveSpinner").toggle(isBusy);
    $btn.prop("disabled", isBusy).attr("aria-disabled", isBusy);
    $btn.toggleClass("is-busy", isBusy);
}

function waitForGridIdle(grid, timeoutMs = 10000) {
    const d = $.Deferred();
    const start = Date.now();

    (function tick() {
        const ds = grid.getDataSource();
        const stillEditing = grid.hasEditData && grid.hasEditData();
        const stillLoading = ds && ds.isLoading && ds.isLoading();

        if (!stillEditing && !stillLoading) return d.resolve();
        if (Date.now() - start > timeoutMs) return d.resolve(); 

        setTimeout(tick, 50);
    })();

    return d.promise();
}

function getRateAmount(keyId, RTSId) {
    pendingRateCalls++;
    setSaveBusy(true);

    const grid = $('#mainRTSGrid').dxDataGrid('instance');
    var model = {
        CustomerId: parseInt($('#CustomerId').val()),
        RTSId: parseInt(RTSId),
        WIPId: $('#Id').val(),
        AccountType: parseInt($('#AccountType').val()),
        SalesType: parseInt($('#SalesType').val())
    };

    $.ajax({
        type: 'POST',
        url: window.RazorVars.getLabourRateUrl,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(model)
    }).then(function (result) {
        if (result == null) return;

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

function showModal(type) {
    //initialize_RTSGrid();
    const grid = $("#rtsSelection").dxDataGrid("instance");
    grid.option("dataSource", []);
    if (type == 1) {
        loadData();
    } else {
        loadMenuData();
    }
}


function loadData() {
    const grid = $("#rtsSelection").dxDataGrid("instance");

    var MakeId = $("#VehMakeModel").val();
    var ModelId = $("#VehModel").val();
    var YearId = $("#VehYear").val();
    var Class = $("#VehClass").val();

    var model = {
        'Make': MakeId,
        'Model': ModelId,
        'Year': YearId,
        'Class': Class
    }

    $.ajax({
        url: window.RazorVars.getRTSDDLUrl,
        method: 'GET',
        dataType: 'json',
        data: model,
        beforeSend: function () {
            grid.option("loadPanel.enabled", true);
        },
        success: function (result) {
            if (result) {
                grid.option("dataSource", result);
            }
        },
        error: function (xhr, status, error) {
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

function loadMenuData() {
    const grid = $("#rtsSelection").dxDataGrid("instance");

    $.ajax({
        url: window.RazorVars.getMenuDDLUrl,
        method: 'GET',
        dataType: 'json',
        beforeSend: function () {
            grid.option("loadPanel.enabled", true);
        },
        success: function (result) {
            if (result) {
                var mappedData = result.map(function (x) {
                    return {
                        ...x,
                        code: x.groupCode,
                        hours: x.totalTime
                    };
                });

                grid.option("dataSource", mappedData);
            }
        },
        error: function (xhr, status, error) {
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

//-------------------------------------------------------------------------------------------------------------------
$("#btnAddRTS, #addRts").on("click", function () {
    showModal(1)
});

$("#btnAddMenu").on("click", function () {
    showModal(2)
});
//-------------------------------------------------------------------------------------------------------------------
$('.btn-group button').on('click', function () {
    $('.btn-group button').removeClass('active');
    $(this).addClass('active');
});

$(document).ready(function () {
    initialize_RTSGrid();
});
//-------------------------------------------------------------------------------------------------------------------
//const LabourLineEnum = {
//    1: "M-Draft", //Draft
//    2: "B-Booked", //Booked
//    3: "W-WIP", //WIP technician clock-in
//    4 : "P", //Waiting For Parts
//    5: "A-Approval", //Approval
//    6 : "L", //Waiting for Labour
//    7: "T-QA", //QA
//    8: "C-Completed", //Completed
//    9: "X-Invoiced", //Invoiced
//};
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