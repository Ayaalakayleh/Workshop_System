$(document).ready(function () {
    getData(1);

    // ------------------ DataTables (unchanged) ------------------
    $('#partsTable').DataTable({
        scrollY: '500px',
        scrollCollapse: true,
        paging: false,
        info: false,
        searching: false,
        dom: 't',
        autoWidth: true,
    });
    $('#partModal').on('shown.bs.modal', function () {
        $('#partsTable').DataTable().columns.adjust();
    });
    $("#PartInput").on("click", function () { $("#partModal").modal("show"); });




    // ------------------ Responsive helpers ------------------
    function isTabletOrSmaller() {
        return window.innerWidth <= 992; // ~Bootstrap md/lg breakpoint
    }
    function isPhone() {
        return window.innerWidth <= 576;
    }

    // ------------------ Utilities ------------------
    function getGridInstance() {
        var $el = $("#gridContainer");
        // Try to get instance; if not available, initialize safely
        try {
            var inst = $el.dxDataGrid("instance");
            if (inst) return inst;
        } catch (e) { /* ignore E0009 during probe */ }

        var initialData = Array.isArray(window.data) ? window.data : []; // tolerate missing window.data
        $el.dxDataGrid({
            dataSource: initialData,
            keyExpr: "Id",
            noDataText: resources.NoDataInTable,

            // ---------- RESPONSIVE: core grid layout ----------
            width: "100%",
            columnAutoWidth: true,           // size columns by content
            wordWrapEnabled: true,           // wrap text instead of forcing horizontal scroll
            showBorders: true,
            showColumnLines: false,          // cleaner on small screens
            rowAlternationEnabled: true,

            // Enable adaptive column hiding + detail row (tap "..." to view hidden fields)
            columnHidingEnabled: true,
            // Allow resizing without disabling hiding priorities
            allowColumnResizing: true,
            columnResizingMode: "nextColumn",

            // Optional: let users pick columns on tablets/desktop
            columnChooser: { enabled: true, mode: "select" },

            // ---------- Columns ----------
            columns: [
                // hidden technical fields
                { dataField: "Id", caption: RazorVars.ID, visible: false },
                { dataField: "IsPart", caption: RazorVars.IsPart, visible: false },

                // Keep these visible longest on small screens -> highest hidingPriority numbers
                { dataField: "Code", caption: RazorVars.Code, allowEditing: false, minWidth: 110, hidingPriority: 5 },
                { dataField: "Description", caption: RazorVars.Description, allowEditing: false, minWidth: 160, hidingPriority: 6 },

                // Numeric/editable fields
                { dataField: "Quantity", caption: RazorVars.Quantity, dataType: "number", allowEditing: true, minWidth: 90, alignment: "right", hidingPriority: 4 },
                { dataField: "StandardHours", caption: RazorVars.StandardHours, dataType: "number", allowEditing: true, minWidth: 100, alignment: "right", hidingPriority: 1 },
                { dataField: "Price", caption: RazorVars.Price, dataType: "number", allowEditing: true, minWidth: 100, alignment: "right", hidingPriority: 2 },
                { dataField: "Discount", caption: RazorVars.Discount, dataType: "number", allowEditing: true, minWidth: 100, alignment: "right", hidingPriority: 0 },

                {
                    dataField: "Total",
                    caption: RazorVars.Total,
                    dataType: "number",
                    allowEditing: false,
                    minWidth: 110,
                    alignment: "right",
                    hidingPriority: 3,
                    calculateCellValue: function (rowData) {
                        var quantity = parseFloat(rowData.Quantity) || 1;
                        var price = parseFloat(rowData.Price) || 0;
                        var discount = parseFloat(rowData.Discount) || 0;
                        var totalValue = (quantity * price) - discount;
                        rowData.Total = totalValue;
                        return totalValue;
                    }
                },

                // Command column (keep compact for phones)
                {
                    type: "buttons",
                    width: isPhone() ? 72 : 110,
                    buttons: [{
                        hint: "Delete",
                        icon: "trash",
                        onClick: function (e) {
                            var grid = e.component;
                            var store = grid.getDataSource().store();
                            store.remove(e.row.key).then(function () {
                                grid.refresh();
                                if ($("#PricingMethod").val() === "Sum") {
                                    setTimeout(updateFieldsFromGrid, 50);
                                }
                            });
                        }
                    }]
                }
            ],

            // Editing mode stays "cell" but adapts its editor form inside the adaptive detail row
            editing: {
                mode: "cell",
                allowDeleting: true,
                allowUpdating: true,
                useIcons: true
            },

            // ---------- RESPONSIVE: tune the adaptive detail form ----------
            onAdaptiveDetailRowPreparing: function (e) {
                // When columns are hidden, their values show in a Form; set sensible layout per screen size
                e.formOptions = e.formOptions || {};
                e.formOptions.colCount = isPhone() ? 1 : 2;
                e.formOptions.labelLocation = isPhone() ? "top" : "left";
            },

            // ---------- Behaviors (unchanged) ----------
            onCellValueChanged: function (e) {
                if (["Quantity", "Price", "Discount", "StandardHours"].includes(e.column.dataField)) {
                    e.component.refresh();
                }
                if ($("#PricingMethod").val() === "Sum") {
                    updateFieldsFromGrid();
                }
            },
            onRowInserted: function () { if ($("#PricingMethod").val() === "Sum") updateFieldsFromGrid(); },
            onRowRemoved: function () { if ($("#PricingMethod").val() === "Sum") updateFieldsFromGrid(); },
            onContentReady: function (ev) {
                // Ensure correct sizing after first paint
                ev.component.updateDimensions();
                if ($("#PricingMethod").val() === "Sum") updateFieldsFromGrid();
            }
        });

        return $el.dxDataGrid("instance");
    }

    function gridItemsArray(grid) {
        // Always read via the DataSource to support both array/DS
        var ds = grid.getDataSource();
        return (ds && ds.items()) || [];
    }

    function setGridData(grid, arr) {
        // Rebind array to trigger change detection
        grid.option("dataSource", arr.slice());
    }

    function findItemInGrid(grid, itemId, isPart) {
        var items = gridItemsArray(grid);
        for (var i = 0; i < items.length; i++) {
            if (items[i].Id == itemId && items[i].IsPart == isPart) {
                return { index: i, item: items[i] };
            }
        }
        return null;
    }

    // ------------------ Totals / Pricing ------------------
    function updateFieldsFromGrid() {
        var grid;
        try { grid = getGridInstance(); } catch (_) { return; }
        if (!grid) return;

        var data = gridItemsArray(grid);
        var totalSum = 0;
        var totalTime = 0;

        for (var i = 0; i < data.length; i++) {
            var row = data[i];
            var quantity = parseFloat(row.Quantity) || 1;
            var price = parseFloat(row.Price) || 0;
            var discount = parseFloat(row.Discount) || 0;
            var standardHours = parseFloat(row.StandardHours) || 0;

            totalSum += (quantity * price) - discount;
            totalTime += quantity * standardHours;
        }

        if ($("#PricingMethod").val() === "Sum") {
            $("#priceField").val(totalSum.toFixed(2));
            $("#TotalTime").val(totalTime.toFixed(2));
        }
    }

    function togglePriceFieldReadonly() {
        var method = $("#PricingMethod").val();
        if (method === "Sum") {
            $("#priceField, #TotalTime").prop("readonly", true).css("background-color", "#f8f9fa");
            updateFieldsFromGrid();
        } else {
            $("#priceField, #TotalTime").prop("readonly", false).css("background-color", "");
        }
    }

    // ------------------ Part search (unchanged except minor safety) ------------------
    $("#partSearch").on("click", function () {
        var fK_GroupId = $("#GroupId").val();
        var fK_CategoryId = $("#CategoryId").val();
        var fK_SubCategoryId = $("#SubCategoryId").val();

        $.ajax({
            type: 'POST',
            url: window.RazorVars.getItemFilteration,
            dataType: 'JSON',
            data: {
                fK_GroupId, fK_CategoryId, fK_SubCategoryId
            }
        }).done(function (result) {
            var tbody = $("#partsTable tbody");
            tbody.empty();
            $.each(result || [], function (i, part) {
                var Name = Lang == "en" ? part.primaryName : part.secondaryName;
                tbody.append(
                    `<tr>
                        <td>${part.id}</td>
                        <td>${Name}</td>
                        <td>
                            <button type="button" class="btn btn-sm btn-success selectPart"
                                data-id="${part.id}" data-name="${Name}">
                                select
                            </button>
                        </td>
                    </tr>`
                );
            });
        });
    });

    $(document).on("click", ".selectPart", function () {
        $("#PartId").val("");
        var id = $(this).data("id");
        var name = $(this).data("name");
        $("#PartId").val(id);
        $("#PartInput").val(name);
        $("#partModal").modal("hide");

        var Id = $("#PartId").val();
        if (Id) { loadPartToGrid(Id); }
    });

    // ------------------ Grid init (ensure it exists once) ------------------
    getGridInstance();

    // ------------------ RESPONSIVE: keep grid sized on viewport changes ------------------
    (function wireGridResize() {
        var resizeTimer;
        $(window).on("resize orientationchange", function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                var grid = getGridInstance();
                if (!grid) return;

                // Make command column width a bit tighter on phones
                try {
                    grid.columnOption("buttons", "width", isPhone() ? 72 : 110);
                } catch (_) { /* ignore if not supported */ }

                grid.updateDimensions();
            }, 120);
        });
    })();

    // ------------------ Pricing method wiring ------------------
    $("#PricingMethod").on("change", function () {
        togglePriceFieldReadonly();
        if ($(this).val() === "Sum") updateFieldsFromGrid();
    });
    togglePriceFieldReadonly();

    // ------------------ RTS: add service to grid (FIXED) ------------------
    $("#rtsDropdown").on("change", function () {
        var Id = $(this).val();
        var $dropdown = $(this);
        if (!Id) return;

        $.ajax({
            type: 'GET',
            url: window.RazorVars.getRTSCodeById,
            dataType: 'json',
            data: { Id: Id }
        }).done(function (result) {
            if (!result) return;

            var grid = getGridInstance();
            var items = gridItemsArray(grid);
            var existing = findItemInGrid(grid, result.id, false);

            if (existing) {
                var newQty = (parseFloat(existing.item.Quantity) || 0) + 1;
                items[existing.index].Quantity = newQty;
                setGridData(grid, items);
            } else {
                items.push({
                    Id: result.id,
                    Code: result.code,
                    Description: Lang == "en" ? result.primaryDescription : result.secondaryDescription,
                    Quantity: 1,
                    StandardHours: result.standardHours,
                    Price: 0,
                    Discount: 0,
                    Total: 0,
                    IsPart: false
                });
                setGridData(grid, items);
            }

            if ($("#PricingMethod").val() === "Sum") updateFieldsFromGrid();

            // Reset dropdown (works with or without Select2)
            $dropdown.val('');
            try {
                if ($dropdown.hasClass("select2-hidden-accessible")) {
                    $dropdown.trigger("change"); // Select2 will redraw
                }
            } catch (_) { /* ignore */ }
        }).fail(function (xhr, status, error) {
            console.error("Error:", error);
        });
    });

    // ------------------ Modal: add part to grid (FIXED) ------------------
    function loadPartToGrid(Id) {
        $.ajax({
            type: 'GET',
            url: window.RazorVars.getItemById,
            dataType: 'json',
            data: { Id: Id }
        }).done(function (result) {
            if (!result) return;

            var grid = getGridInstance();
            var items = gridItemsArray(grid);
            var existing = findItemInGrid(grid, result.id, true);

            if (existing) {
                var newQty = (parseFloat(existing.item.Quantity) || 0) + 1;
                items[existing.index].Quantity = newQty;
                setGridData(grid, items);
            } else {
                items.push({
                    Id: result.id,
                    Code: result.code,
                    Description: Lang == "en" ? result.primaryName : result.secondaryName,
                    Quantity: 1,
                    StandardHours: 1,
                    Price: result.price,
                    Discount: 0,
                    Total: 0,
                    IsPart: true
                });
                setGridData(grid, items);
            }

            if ($("#PricingMethod").val() === "Sum") updateFieldsFromGrid();
        }).fail(function (xhr, status, error) {
            console.error("Error:", error);
        });
    }

    // ------------------ Index pager (unchanged) ------------------
    function getData(page) {
        page = page || 1;
        var FilterModel = {
            'Name': $('#_Name').val(),
            'GroupCode': $('#_Code').val(),
            'PageNumber': page
        };

        $.ajax({
            type: 'GET',
            url: window.RazorVars.indexUrl,
            dataType: 'html',
            data: FilterModel
        }).done(function (result) {
            $("#contentList").html(result);
            if ($("#total_pages").val() != undefined) {
                $("#contentPager").attr("hidden", false);
                $('#contentPager').pagination('destroy');
                $('#contentPager').pagination({
                    items: $("#total_pages").val(),
                    itemOnPage: 25,
                    currentPage: page,
                    prevText: '&laquo;',
                    nextText: '&raquo;',
                    onPageClick: function (page, evt) {
                        getData(page);
                    }
                });

                $('#contentPager ul').addClass('pagination');
                $('#contentPager li').addClass('page-item');
                $('#contentPager a, #contentPager span').addClass('page-link');
            } else {
                $("#contentPager").attr("hidden", true);
            }
        });
    }

    $("#btnSearch").click(function () { getData(); });

    // ------------------ Form validation (minor hardening) ------------------
    $("#menuForm").validate({
        rules: {
            GroupCode: { required: true },
            GroupName: { required: true },
            PrimaryDescription: { required: true },
            SecondaryDescription: { required: false },
            PricingMethod: { required: true },
            Price: { required: true },
            TotalTime: { required: true },
            EffectiveDate: { required: true }
        },
        messages: {
            GroupCode: { required: RazorVars.required_field },
            GroupName: { required: RazorVars.required_field },
            PrimaryDescription: { required: RazorVars.required_field },
            SecondaryDescription: { required: RazorVars.required_field },
            PricingMethod: { required: RazorVars.required_field },
            Price: { required: RazorVars.required_field },
            TotalTime: { required: RazorVars.required_field },
            EffectiveDate: { required: RazorVars.required_field }
        },
        errorClass: "text-danger text-nowrap",
        errorElement: "span",
        highlight: function (el) { $(el).addClass("is-invalid"); },
        unhighlight: function (el) { $(el).removeClass("is-invalid"); },
        submitHandler: function (form) {
            var grid = getGridInstance();
            var items = gridItemsArray(grid);

            if (!items.length) {
                $("#ServicesToSave").val("");
                Swal.fire({
                    icon: "error",
                    title: RazorVars.TableData,
                    showConfirmButton: true,
                    confirmButtonText: resources.ok
                });
                return false;
            }

            $("#ServicesToSave").val(JSON.stringify(items));
            $(form).find('[type="submit"]').prop('disabled', true);
            form.submit();
        }
    });
});
function deleteMenu(itemID) {
    $.ajax({
        type: 'POST',
        url: window.RazorVars.deleteURL,
        dataType: 'json',
        data: { Id: itemID }
    })
        .done(function (result) {
            Swal.fire({
                icon: 'success',
                title: window.RazorVars.swalDeleted || 'Deleted Successfully',
                text: result?.message || '',
                confirmButtonText: 'OK'
            });

            // Wait 2 seconds before reload
            setTimeout(function () {
                window.location.reload();
            }, 2000);
        })
        .fail(function (xhr) {
            Swal.fire({
                icon: 'error',
                title: window.RazorVars.swalErrorHappened || 'Delete Failed',
                text: xhr.responseText || '',
                confirmButtonText: 'OK'
            });
        });
}

