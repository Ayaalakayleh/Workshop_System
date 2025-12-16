// ~/js/Definitions/wip.js
(function ($, window, document) {
    "use strict";
    const resources = window.resources || {};

    $(document).ready(function () {

        $("#optPartialInv").change(function () {
            if ($(this).is(":checked")) {
                $("#tabPartialAccountBTN").show();
            } else {
                $("#tabPartialAccountBTN").hide();
            }
        });

        /* ------- WIP Filter + Pagination (same concept as Technicians) ------- */
        function FilterWIP(page) {
            page = page || 1;

            var WIPNo = $("#WipNo").val();
            var status = $("#Status").val();
            var CustomerId = $("#CustomerId").val();

            var model = {
                WIPNo: WIPNo,
                Status: status,
                CustomerId: CustomerId,
                PageNumber: page
            };

            $.ajax({
                type: 'POST',                      // matches your old code
                url: window.URLs.indexUrl,
                dataType: 'html',
                data: model
            }).done(function (result) {
                if (!result) return;

                $("#WIPListContainer").html(result);

                if ($("#total_pages").val() != undefined) {
                    $("#contentPager").attr("hidden", false);

                    $('#contentPager').pagination('destroy');
                    $('#contentPager').pagination({
                        items: $("#total_pages").val(), // same usage as Technicians
                        itemOnPage: 25,
                        currentPage: page,
                        prevText: '&laquo;',
                        nextText: '&raquo;',
                        onPageClick: function (pageNumber) { FilterWIP(pageNumber); }
                    });

                    $('#contentPager ul').addClass('pagination');
                    $('#contentPager li').addClass('page-item');
                    $('#contentPager a, #contentPager span').addClass('page-link');
                } else {
                    $("#contentPager").attr("hidden", true);
                }
            }).fail(function (xhr, status, error) {
                console.error("Error:", error);
            });
        }

        // Initial load to wire pager correctly
        FilterWIP(1);

        $("#btnWIPSearch").on("click", function () {
            FilterWIP(1);
        });

        $("#btnResetSearch").on("click", function () {
            $("#WipNo").val("");
            $("#Status").val("").trigger('change');
            $("#CustomerId").val("").trigger('change');

            FilterWIP(1);
        });

        $("#VehMakeModel").on("change", function () {
            console.log("ChangeD")
            $.get(RazorVars.getVehicleByManufacturerIdURL, { manufacturerId: $(this).val() }, function (data) {
                let $modelSelect = $('#VehModel');
                $modelSelect.empty().append(
                    $('<option>', { value: '', text: RazorVars.placeholderSelect || 'Select' })
                );
                $.each(data, function (_, item) {
                    $modelSelect.append(
                        $('<option>', { value: item.id, text: item.vehicleModelPrimaryName ?? item.vehicleModelSecondaryName })
                    );
                });

                if ($modelSelect) {
                    $modelSelect.trigger('change');
                }
            });
        });

        var _VehicleId = 0;
        function validateAccountTab() {
            let isValid = true;

            const fields = [
                { id: "#AccountType", name: "Account Type" },
                { id: "#SalesType", name: "Sales Type" },
                { id: "#statusId", name: "status" },
                { id: "#WipDate", name: "date" },
            ];

            fields.forEach(f => {
                const val = $(f.id).val();
                const errorSpan = $(f.id).siblings(".text-danger");
                if (!val || val === "Select" || val === "") {
                    errorSpan.text("This field is required");
                    $(f.id).addClass("is-invalid");
                    isValid = false;
                } else {
                    errorSpan.text("");
                    $(f.id).removeClass("is-invalid");
                }
            });

            return isValid;
        }

        /* ------- Save ------- */
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
                console.log("Edit_Post result:", result);

                if (result && result.success && result.wipId) {
                    Swal.fire("Success", "WIP " + WIPId + " Saved Successfully!").then(() => {
                        window.location.href = window.URLs.editGetUrl + '?id=' + result.wipId + '&movementId=' + MovId;
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Save Failed",
                        text: result && result.errorMessage ? result.errorMessage : "Unknown error occurred"
                    });
                }

            });

        }

        /* ------- Submit ------- */
        $("#btnSave2").on("click", function () {
            //if (validateAccountTab()) {
            //    saveData();
            //}
            if (!validateAccountTab()) {
                return;
            }

            if (!validateGridsAccountTypeForPartialInv()) {
                return;
            }

            saveData();
        });

        /* ------- Close WIP ------- */
        $("#closeBTN").on("click", function () {

            var Services_Items = $("#mainRTSGrid").dxDataGrid('instance')._controllers.data._dataSource._items;
            var ServicesJson = JSON.stringify(Services_Items);
            $("#Services").val(ServicesJson);

            var gridItems = $("#mainItemsGrid").dxDataGrid('instance')._controllers.data._dataSource._items;

            if (Services_Items.length == 0) {
                Swal.fire({
                    icon: "warning",
                    title: "Warning",
                    text: "You dont have services"
                });
                return;
            }

            
            var notCompletedItems = gridItems.filter(function (row) {

                return row.StatusId !== 42;
            });

            if (notCompletedItems.length > 0) {
                Swal.fire({
                    icon: "warning",
                    title: "Warning",
                    text: "You have uncompleted Items"
                });
                return;
            }

            var invalidItems = gridItems.filter(function (row) {
                debugger
                return row.UsedQuantity === null ||
                    row.UsedQuantity === undefined ||
                    row.UsedQuantity === "" ||
                    row.UsedQuantity === 0;
            });

            if (invalidItems.length > 0) {
                Swal.fire({
                    icon: "warning",
                    title: "Warning",
                    text: "Used  Quantity Is Requierd"
                });
                return;
            }

            var itemsJson = JSON.stringify(gridItems);
            $("#Items").val(itemsJson);

            var WIPId = $('#Id').val();
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

            var close = {
                Id: WIPId,
                VehicleId: VehId,
                MovementId: MovId,
                Items: itemsJson,
                Services: ServicesJson,
                AccountDetails: accountDetails,
                VehicleTab: vehicleTab,
                Status: status,
                WipDate: wipDate,
                Note: note,
                Options: optionsTab
            };

            $.ajax({
                type: 'POST',
                url: window.URLs.closeWipUrl,
                dataType: 'json',
                data: close
            }).done(function (result) {
                if (result.success) {
                    Swal.fire(
                        "Success",
                        "WIP has been closed successfully."
                    ).then(() => {
                        window.location.href = window.URLs.indexUrl;
                    });
                } else {
                    Swal.fire(
                        "Error",
                        result.message || "An unknown error occurred."
                    );
                }
            }).fail(function (xhr, status, error) {
                console.error("Error:", error);
            });
        });


        /* ------- View swapping (Search cars <-> WIP form) ------- */
        const $wipFormWrapper = $('#wipFormWrapper');
        const $vehicleSearchWrapper = $('#vehicleSearchWrapper');
        const $btnFindVehicle = $('#btnFindVehicle');
        const $btnBackToWip = $('#btnBackToWip');
        $("#VehicleId").val("");

        function showSearchView() {
            $wipFormWrapper.addClass('d-none');
            $vehicleSearchWrapper.removeClass('d-none');
            $btnBackToWip.removeClass('d-none');
        }
        function showWipFormView() {
            $vehicleSearchWrapper.addClass('d-none');
            $wipFormWrapper.removeClass('d-none');
            $btnBackToWip.addClass('d-none');
        }

        $btnFindVehicle.on('click', showSearchView);
        $btnBackToWip.on('click', showWipFormView);
        $('#wipModal').on('hidden.bs.modal', showWipFormView);

        $('#btnDoVehicleSearch').on('click', function () {
            // Implement vehicle search if needed
        });

        $('#btnResetVehicleSearch').on('click', function () {
            $('#searchVin, #searchPlate, #searchPhone, #searchDoor').val('');
        });

        // Selecting a vehicle pushes it back to the main form field
        $(document).on('click', '.select-vehicle', function () {
            const id = $(this).data('id');
            const display = $(this).data('display') || '';

            $("#VehicleId").val(display).attr("data-id", id);
            showWipFormView();
        });

        /* -------------------- Top actions -------------------- */
        $('#btnBack').on('click', () => window.history.back());

        /* -------------------- RTS filter -------------------- */
        function filterRts() {
            const qd = ($('#rtsFilterDesc').val() || '').toLowerCase().trim();
            const qc = ($('#rtsFilterCode').val() || '').toLowerCase().trim();
            const qcat = ($('#rtsFilterCategory').val() || '').toLowerCase().trim();
            $('#rtsRows tr').each(function () {
                const $tr = $(this);
                const code = ($tr.data('code') || '').toString().toLowerCase();
                const cat = ($tr.data('cat') || '').toString().toLowerCase();
                const desc = $tr.children().eq(1).text().toLowerCase();
                $tr.toggle((!qd || desc.includes(qd)) && (!qc || code.includes(qc)) && (!qcat || cat === qcat));
            });
        }
        $('#btnRtsSearch').on('click', filterRts);
        $('#rtsFilterDesc,#rtsFilterCode,#rtsFilterCategory')
            .on('keyup change', function (e) { if (e.type === 'keyup' && e.key !== 'Enter') return; filterRts(); });

        /* -------------------- Add Part: validate -------------------- */
        $("#addPartForm").validate({
            errorClass: "is-invalid",
            validClass: "is-valid",
            rules: {
                partSku: { required: true, minlength: 2 },
                partDesc: { required: true, minlength: 2 },
                partQty: { required: true, number: true, min: 1 },
                partCost: { required: true, number: true, min: 0 },
                partSell: { required: true, number: true, min: 0 },
                partDisc: { required: true, number: true, min: 0, max: 100 }
            },
            highlight: (el) => $(el).addClass("is-invalid"),
            unhighlight: (el) => $(el).removeClass("is-invalid")
        });

        $('#btnSavePart').on('click', function () {
            if (!$("#addPartForm").valid()) return;
            const cost = +$('#partCost').val() || 0;
            const sell = +$('#partSell').val() || 0;
            if (sell === 0) $('#partSell').val((cost * 1.25).toFixed(2));
            $('#addPartModal').modal('hide');
        });

        /* -------------------- Delete via Swal2 -------------------- */
        $(document).on('click', '.act-delete', function (e) {
            e.preventDefault();
            const $row = $(this).closest('tr');
            Swal.fire({
                title: resources.confirm_title,
                text: resources.are_you_sure,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: resources.yes,
                cancelButtonText: resources.no,
                confirmButtonColor: "var(--danger-700)",
                cancelButtonColor: "var(--secondary-600)",
            }).then(res => {
                if (res.isConfirmed) {
                    $row.remove();
                    Swal.fire({ icon: 'success', title: resources.done_title, timer: 1200, showConfirmButton: false });
                }
            });
        });

        $('#tabVehicleBTN').on("click", function () {
            var Id = $('#_vehicleId').val();
            var WIPId = $('#Id').val();

            $.ajax({
                type: 'Get',
                url: window.URLs.getVehicleDetailsByIdUrl,
                contentType: 'application/json',
                dataType: 'json',
                data: { Id: Id, WIPId: WIPId },
                success: function (result) {
                    if (result) {
                        //$('#VehPlate').val(result.plateNumber);
                        //$('#VehMakeModel').val(result.manufacturerPrimaryName);
                        //$('#VehModel').val(result.vehicleModelPrimaryName);
                        //$('#VehClass').val(result.vehicleClassPrimaryName);
                        //$('#VehYear').val(result.manufacturingYear);
                        //$('#Color').val(result.color);
                        $('#VehVIN').val(result.chassisNo);

                        $('#VehServiceDesc').val(result.vehServiceDesc);
                        $('#VehConcerns').val(result.vehConcerns);
                        $('#VehAdvisorNotes').val(result.vehAdvisorNotes);

                        $('#OdoPrev').val(result.odometerPrevious);
                        $('#OdoCurrentIn').val(result.odometerCurrentIN);
                        $('#OdoCurrentOut').val(result.odometerCurrentOUT);
                    }
                }
            });
        });

        $('#tabOptionsBTN').on("click", function () {
            var Id = $('#Id').val();

            $.ajax({
                type: 'Get',
                url: window.URLs.getWIPOptionsByIdUrl,
                contentType: 'application/json',
                dataType: 'json',
                data: { Id: Id },
                success: function (result) {
                    if (result) {
                        $("#optPartialInv").prop("checked", result.partialInvoicing == 1);
                        $("#optReturnParts").prop("checked", result.returnParts == 1);
                        $("#optRepeatRepair").prop("checked", result.repeatRepair == 1);
                        $("#optUpdateDemand").prop("checked", result.updateDemand == 1);
                    }
                }
            });
        });

        const WIP_Id = $('#Id').val();
        $.ajax({
            type: 'GET',
            url: window.URLs.transferMoveInUrl + '?movementId=' + movementId + '&WIP_Id=' + WIP_Id,
            contentType: 'application/json',
            dataType: 'html',
            success: function (result) {
                $("#TransferMovement").html(result);
            }
        });

        /* -------------------- Save WIP (basic) -------------------- */
        $('#btnSaveWip').on('click', function () {
            var VehicleId = $('#VehicleId').data("id");
            var Id = $('#Id').val();
            var type = $("#VehicleTypeId").val();

            var model = {
                'Id': Id,
                'VehicleId': VehicleId,
                'IsExternal': type === '2' ? true : false
            };

            $.ajax({
                type: 'POST',
                url: window.URLs.addUrl,
                contentType: 'application/json',
                dataType: 'json',
                data: JSON.stringify(model),
                success: function (result) {
                    $("#VehicleId").val("");
                    window.location = window.URLs.indexUrl;
                }
            });
        });
        /* -------------------- Credit Note -------------------- */
   
      
       
    });

})(jQuery, window, document);


$("#SalesType").on("change", function () {

    const grid = $("#mainRTSGrid").dxDataGrid("instance");
    if (!grid) return;

    grid.saveEditData();
    const items = grid.getDataSource().items() || [];

    items.forEach(function (row) {

        const rtsId = row.RTSId || row.Id;

        if (rtsId) {
            getRateAmount(rtsId);
        }
    });
});


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
        if (result !== undefined && result !== null) {
            const grid = $('#mainRTSGrid').dxDataGrid('instance');
            const rowIndex = grid.getRowIndexByKey(model.RTSId);
            if (rowIndex >= 0) {
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

                //grid.saveEditData();
                //updateTotalLabourFieldsFromGrid();
                grid.saveEditData();
                grid.refresh().done(function () {
                    updateTotalLabourFieldsFromGrid();
                });
            }

        }
    }).fail(function (xhr, status, error) {
        console.error("Error:", error);
    });
}

$(document).ready(function () {

    function getGrids() {
        const g1 = $("#mainItemsGrid").dxDataGrid("instance");
        const g2 = $("#mainRTSGrid").dxDataGrid("instance");
        return [g1, g2].filter(g => g); 
    }

    function updateAccountTypeValues() {
        const accountTypeVal = parseInt($("#AccountType").val()) || 0;
        const partialInvoicing = $("#optPartialInv").is(":checked");

        getGrids().forEach(grid => {
            const store = grid.getDataSource().store();
            const rows = grid.getVisibleRows();

            rows.forEach(r => {
                const row = r.data;
                if (!partialInvoicing) {
                    row.AccountType = accountTypeVal;
                    store.update(row.Id, row);
                }
            });

            grid.refresh();
        });
    }

    function toggleEditingByPartialInv() {
        const partialInvoicing = $("#optPartialInv").is(":checked");
        const accountTypeVal = parseInt($("#AccountType").val()) || 0;

        getGrids().forEach(grid => {

            const cols = grid.option("columns");
            cols.forEach(col => {
                if (col.dataField === "AccountType") {
                    col.allowEditing = partialInvoicing;

                    if (partialInvoicing) {
                        col.validationRules = [
                            { type: "required", message: "required" }
                        ];
                    } else {
                        col.validationRules = [];
                    }
                }
            });
            grid.option("columns", cols);

            const store = grid.getDataSource().store();
            const rows = grid.getVisibleRows();

            rows.forEach(r => {
                const row = r.data;

                if (partialInvoicing) {
                    row.AccountType = null;
                } else {
                    row.AccountType = accountTypeVal;
                }

                store.update(row.Id, row);
            });

            grid.refresh();
        });
    }

    $("#AccountType").on("change", updateAccountTypeValues);

    $("#optPartialInv").on("change", toggleEditingByPartialInv);

    updateAccountTypeValues();
    toggleEditingByPartialInv();

    function wireOnRowInserted() {
        getGrids().forEach(grid => {
            const prevHandler = grid.option("onRowInserted");

            grid.option("onRowInserted", function (e) {
                const accountTypeVal = parseInt($("#AccountType").val()) || 0;
                const partialInvoicing = $("#optPartialInv").is(":checked");
                const store = grid.getDataSource().store();

                if (!partialInvoicing) {
                    e.data.AccountType = accountTypeVal;
                } else {
                    e.data.AccountType = null;
                }

                store.update(e.key, e.data).then(() => grid.refresh());

                if (typeof prevHandler === "function") {
                    prevHandler(e);
                }
            });
        });
    }

    wireOnRowInserted();
});
function validateGridsAccountTypeForPartialInv() {
    const partialInvoicing = $("#optPartialInv").is(":checked");
    if (!partialInvoicing) return true; 

    let hasError = false;
    let msgs = [];

    const gridsInfo = [
        { id: "#mainItemsGrid", name: "(Parts)" },
        { id: "#mainRTSGrid", name: "(Labour)" }
    ];

    gridsInfo.forEach(g => {
        const grid = $(g.id).dxDataGrid("instance");
        if (!grid) return;

        const rows = grid.getVisibleRows() || [];

        rows.forEach((r, idx) => {
            const d = r.data || {};


            const hasMeaning =
                (d.Quantity && d.Quantity > 0) ||
                (d.RequestQuantity && d.RequestQuantity > 0) ||
                (d.StandardHours && d.StandardHours > 0) ||
                (d.Total && d.Total > 0);

            const accountTypeEmpty = (d.AccountType == null || d.AccountType === 0 || d.AccountType === "");

            if (hasMeaning && accountTypeEmpty) {
                hasError = true;
                msgs.push("required");
            }
        });
    });

    if (hasError) {
        Swal.fire({
            icon: "error",
            title: "Please Add Account Type",
           
        });

        return false;
    }

    return true;
}

