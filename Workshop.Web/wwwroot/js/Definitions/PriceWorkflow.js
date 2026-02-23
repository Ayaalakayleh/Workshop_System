function newGuid() {
    if (window.crypto && typeof crypto.randomUUID === "function") return crypto.randomUUID();
    // fallback
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

let isEditMode = false;

function setGridEditMode(enabled) {
    const grid = $("#PriceWorkflowGrid").dxDataGrid("instance");
    if (!grid) return;

    isEditMode = enabled;

    grid.cancelEditData();

    grid.option("editing", {
        mode: "cell",
        allowUpdating: enabled,
        allowAdding: enabled,
        allowDeleting: enabled
    });

    grid.repaint();
}

let gridInstance = null;

function getItemsWithPendingChanges(grid) {
    const base = (grid.getDataSource()?.items() || []).map(x => ({ ...x }));
    const changes = grid.option("editing.changes") || [];

    changes.forEach(ch => {
        if (ch.type === "insert") {
            base.push({ ...ch.data });
        } else if (ch.type === "update") {
            const idx = base.findIndex(r => r.KeyId === ch.key);
            if (idx > -1) base[idx] = { ...base[idx], ...ch.data };
        } else if (ch.type === "remove") {
            const idx = base.findIndex(r => r.KeyId === ch.key);
            if (idx > -1) base.splice(idx, 1);
        }
    });

    return base;
}

function isDuplicateValue(grid, field, value, currentKeyId) {

    if (value === null || value === undefined || value === "") return false;

    const items = getItemsWithPendingChanges(grid);
    const v = Number(value);

    return items.some(r =>
        r.KeyId !== currentKeyId &&
        Number(r[field]) === v
    );
}



$(function () {
    let locatorsLoadedOnInit = false;
    $("#PriceWorkflowGrid").dxDataGrid({
        dataSource: Data,
        keyExpr: "KeyId",
        noDataText: resources.NoDataInTable,
        showBorders: true,
        remoteOperations: {
            filtering: true,
            sorting: true,
            paging: true
        },
        columns: [
            { dataField: "KeyId", visible: false },
            { dataField: "Id", caption: "ID", visible: false, alignment: "left" },
            {
                dataField: "Price",
                caption: window.RazorVars.DXPrice,
                visible: true,
                dataType: "number",
                alignment: "left",
                allowEditing: false,
                validationRules: [{
                    type: "custom",
                    message: "The price already exist",
                    validationCallback: function (e) {
                        const grid = gridInstance;
                        if (!grid) return true;

                        const currentKeyId = e.data?.KeyId; 
                        return !isDuplicateValue(grid, "Price", e.value, currentKeyId);
                    }
                }]
            },
            {
                dataField: "WorkflowID",
                caption: window.RazorVars.DXWorkflow,
                dataType: "number",
                allowEditing: false,
                alignment: "left",
                lookup: {
                    dataSource: Workflows.map(x => ({
                        Value: Number(x.Value),
                        Text: x.Text
                    })),
                    valueExpr: "Value",
                    displayExpr: "Text"
                },
                validationRules: [{
                    type: "custom",
                    message: "This Workflow already exist",
                    validationCallback: function (e) {
                        const grid = gridInstance;
                        if (!grid) return true;

                        const currentKeyId = e.data?.KeyId;
                        return !isDuplicateValue(grid, "WorkflowID", e.value, currentKeyId);
                    }
                }],
                calculateDisplayValue: function (rowData) {
                    return Workflows.find(x => Number(x.Value) === rowData.WorkflowID)?.Text || "";
                }
            },
            {
                type: "buttons",
                width: 170,
                alignment: "left",
                buttons: [
                   
                    {
                        hint: "Delete",
                        icon: "fad fa-trash",
                        visible: function () { return isEditMode; }, 
                        onClick: function (e) {
                            var grid = e.component;
                            grid.getDataSource().store().remove(e.row.key);
                            grid.refresh();
                            DeletePrice(e);
                        }
                    }
                ]
            }
        ],
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnAutoWidth: false,
        columnMinWidth: 50,
        wordWrapEnabled: false,
        hoverStateEnabled: true,
        paging: {
            pageSize: 5
        },
        rowAlternationEnabled: true,
        pager: {
            visible: true,
            showPageSizeSelector: false,
            //allowedPageSizes: [5, 10, 20, 50],
            showInfo: true,
            showNavigationButtons: false
        },
        editing: {
            mode: "cell",
            allowAdding: false,   
            allowDeleting: false, 
            allowUpdating: false
        },
        onCellValueChanged: function (e) {
            
        },
        onRowRemoved: function () {
        },
        onContentReady: function (e) {
            const grid = e.component;
            const count = grid.getDataSource().items().length;

            const toolbar = grid.getToolbar && grid.getToolbar();
            if (toolbar) {
                const items = toolbar.option("items");
                items.forEach(it => {
                    if (it.widget === "dxButton" && it.options && it.options.icon === "add") {
                        it.options.disabled = count >= 5;
                    }
                });
                toolbar.option("items", items);
            }
        },
        onInitNewRow: function (e) {
            e.data.KeyId = newGuid();
        },
        onRowInserting: function (e) {
            const grid = e.component;
            const ds = grid.getDataSource();

            const count = (ds && ds.items) ? ds.items().length : 0;

            if (count >= 5) {
                e.cancel = true;
                DevExpress.ui.notify("Allow 5 rows only", "warning", 2500);
                return;
            }

            if (!e.data.KeyId) {
                e.data.KeyId = newGuid();
            }
        },
        onEditorPreparing: function (e) {
            if (e.parentType !== "dataRow") return;

            if (e.dataField === "WorkflowID") {
                const grid = e.component;
                const currentKeyId = e.row?.data?.KeyId;

                const usedWorkflowIds = getItemsWithPendingChanges(grid)
                    .filter(r => r.KeyId !== currentKeyId)
                    .map(r => Number(r.WorkflowID))
                    .filter(v => !Number.isNaN(v));

                e.editorOptions.dataSource = Workflows
                    .map(x => ({ Value: Number(x.Value), Text: x.Text }))
                    .filter(w => !usedWorkflowIds.includes(w.Value));

                e.editorOptions.valueExpr = "Value";
                e.editorOptions.displayExpr = "Text";
            }
        },
        onToolbarPreparing: function (e) {
            const grid = e.component;

            e.toolbarOptions.items.unshift({
                location: "after",
                widget: "dxButton",
                name: "addButton",
                options: {
                    icon: "add",
                    text: "Add",
                    visible: false,
                    onClick: function () {
                        //const count = grid.getDataSource().items().length;
                        const count = grid.totalCount ? grid.totalCount() : grid.getDataSource().items().length;

                        if (count >= 4) {  
                            return; 
                        }
                        grid.pageIndex(0);
                        grid.addRow();
                    }
                }
            });
        },
        onInitialized: function (e) {
            gridInstance = e.component;
        },



    });
});

// Buttons  =============================================================================
$(document).ready(function () {

    $(".dx-toolbar-button").hide();
    setGridEditMode(false);

    $("#btnEdit").on("click", function () {
        setGridEditMode(true);

        const grid = $("#PriceWorkflowGrid").dxDataGrid("instance");
        grid.columnOption("Price", "allowEditing", true);
        grid.columnOption("WorkflowID", "allowEditing", true);
        grid.columnOption(4, "visible", true);

        const tb = grid.getToolbar && grid.getToolbar();
        if (tb) {
            const items = tb.option("items");
            items.forEach(it => {
                if (it.name === "addButton") it.options.visible = true;
            });
            tb.option("items", items);
        }

        $("#PriceWorkflowForm select, #PriceWorkflowForm input").prop("disabled", false);

        $("#btnEdit").addClass("d-none");
        $("#btnCreate").removeClass("d-none");

        $(".dx-toolbar-button").show();
    });

    
    $("#btnCreate").on("click", function (e) {
        e.preventDefault();
        $(".dx-toolbar-button").hide();
        SaveData();

    });

})

// Save  ================================================================================
function SaveData() {
    var grid = $('#PriceWorkflowGrid').dxDataGrid('instance');

    var p = grid.saveEditData(); // Promise

    $.when(p).done(function () {
        grid.getDataSource().store().load().done(function (allItems) {
            var payload = allItems.map(x => ({
                Id: x.Id || 0,
                Price: x.Price,
                KeyId: x.KeyId,
                WorkflowID: x.WorkflowID
            }));

            $.ajax({
                type: 'POST',
                url: window.URLs.editPostUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify(payload)
            }).done(function (result) {
                if (result && result.success) {
                    Swal.fire("Success", "Saved Successfully!", "success");
                    location.reload();
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Save Failed",
                        text: result?.errorMessage || "Unknown error occurred"
                    });
                }
            });
        });
    });
}

// Delete  ==============================================================================
function DeletePrice(e) {
    const id = e.row.data.Id;
    console.log(id);

    $.ajax({
        type: 'POST',
        url: window.URLs.deleteUrl,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(id)
    }).done(function (result) {
        debugger
        if (result && result.success) {
            Swal.fire("Success", "Deleted Successfully!", "success");
        } else {
            Swal.fire({
                icon: "error",
                title: "Delete Failed",
                text: result?.errorMessage || "Unknown error occurred"
            });
        }
    });
}

