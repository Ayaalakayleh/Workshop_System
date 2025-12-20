
function initialize_RTSGrid() {

    $("#rtsSelection").dxDataGrid({
        dataSource: {
            store: {
                type: "array",
                key: "Id",
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

function injectMainGrid(count) {
    const btn = $("#selectBtn");
    btn.off("click");
    if (count > 0) {
        //btn.on("click", function () {
        //    const sourceGrid = $("#rtsSelection").dxDataGrid("instance");
        //    const targetGrid = $("#mainRTSGrid").dxDataGrid("instance");

        //    const selectedRows = sourceGrid.getSelectedRowsData();
        //    if (selectedRows.length > 0) {
        //        const transformedData = selectedRows.map(item => ({
        //            Id: item.id,
        //            ItemId: item.id,
        //            Code: item.code,
        //            Description: lang == "en" ? item.primaryDescription : item.secondaryDescription,
        //            StandardHours: item.hours,
        //            //Allowed: item.Hours,
        //            Rate: 0,
        //            TimeTaken: 0,
        //            Status: LabourLineEnum[1].value,
        //            StatusText: LabourLineEnum[1].text,
        //            Total:0

        //        }));
        //        const existing = targetGrid.option("dataSource") || [];
        //        const newData = existing.concat(transformedData);
        //        $("#addRtsModal").modal("hide");
        //        targetGrid.option("dataSource", newData);
        //    }
        //});
        btn.on("click", function () {
            const sourceGrid = $("#rtsSelection").dxDataGrid("instance");
            const targetGrid = $("#mainRTSGrid").dxDataGrid("instance");

            const selectedRows = sourceGrid.getSelectedRowsData();
            if (selectedRows.length > 0) {

                const existingRows = targetGrid.option("dataSource") || [];

                const newItems = [];

                selectedRows.forEach(item => {
                    const exists = existingRows.some(x => x.Id === item.id);

                    if (!exists) {
                        newItems.push({
                            Id: item.id,
                            ItemId: item.id,
                            Code: item.code,
                            Description: lang == "en" ? item.primaryName : item.secondaryName,
                            LongDescription: lang == "en" ? item.primaryDescription : item.secondaryDescription,
                            StandardHours: item.hours,
                            Rate: getRateAmount(item.id),
                            TimeTaken: 0,
                            Status: LabourLineEnum[1].value,
                            StatusText: LabourLineEnum[1].text,
                            Total: 0
                        });
                    }
                });

                if (newItems.length > 0) {
                    const updated = existingRows.concat(newItems);
                    targetGrid.option("dataSource", updated);
                }

                $("#addRtsModal").modal("hide");
            }
        });

    }
}

function getRateAmount(RTSId) {
    var id = $('#Id').val();
    var CustomerId = $('#CustomerId').val();
    var AccountType = $('#AccountType').val();
    var SalesType = $('#SalesType').val();
    debugger
    var model = {
        //TechnicianId: parseInt(TechnicianId),
        CustomerId: parseInt(CustomerId),
        RTSId: parseInt(RTSId),
        WIPId: id,
        AccountType: parseInt(AccountType),
        SalesType: parseInt(SalesType)
    };

    $.ajax({
        type: 'POST',
        url: window.RazorVars.getLabourRateUrl,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(model)
    }).done(function (result) {
        if (result) {
            const grid = $('#mainRTSGrid').dxDataGrid('instance');
            const rowIndex = grid.getRowIndexByKey(model.RTSId);
            if (rowIndex >= 0) {
                grid.cellValue(rowIndex, "BaseRate", result);
                grid.cellValue(rowIndex, "Rate", result);

                const rowData = grid.getVisibleRows()[rowIndex].data;
                const total = (parseFloat(result) || 0) * (parseFloat(rowData.StandardHours) || 1);
                grid.cellValue(rowIndex, "Total", total);

                var data = grid.option("dataSource");
                if (Array.isArray(data)) {
                    const target = data.find(x => x.Id === model.RTSId);
                    if (target) {
                        target.Rate = result;
                        target.Total = total;
                    }
                }

                grid.saveEditData();
                updateTotalLabourFieldsFromGrid();

            }

        }
    }).fail(function (xhr, status, error) {
        console.error("Error:", error);
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