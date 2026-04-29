// ~/js/Definitions/reports.js
(function ($, window, document) {
    "use strict";

    $(document).ready(function () {

        function initMainTable() {
            $('.main-table').each(function () {
                if ($.fn.DataTable.isDataTable(this)) {
                    $(this).DataTable().destroy();
                }

                $(this).DataTable({
                    responsive: true,
                    pageLength: 10,
                    info: false,
                    paging: false,
                    ordering: false,
                    dom: 'tp',
                    language: {
                        emptyTable: resources.EmptyData
                    }
                });
            });
        }

        initMainTable();

        function getReportModel() {
            var WIP = $("#WIP").val();
            var InvoiceDateStart = $("#InvoiceDateStart").val();
            var InvoiceDateEnd = $("#InvoiceDateEnd").val();
            var CustomerId = $("#CustomerId").val();

            return {
                WIP: WIP ? parseInt(WIP) : null,
                TypeIds: $("#TypeIds").val(),
                StatusId: $("#StatusId").val() ? parseInt($("#StatusId").val()) : null,
                InvoiceDateStart: InvoiceDateStart || null,
                InvoiceDateEnd: InvoiceDateEnd || null,
                CustomerId: CustomerId ? parseInt(CustomerId) : null
            };
        }

        function getPrintExportModel() {
            var WIP = $("#WIP").val();
            var InvoiceDateStart = $("#InvoiceDateStart").val();
            var InvoiceDateEnd = $("#InvoiceDateEnd").val();
            var CustomerId = $("#CustomerId").val();

            return {
                WIP: WIP ? parseInt(WIP) : null,
                TypeId: $("#TypeId").val() ? parseInt($("#TypeId").val()) : null,
                StatusId: $("#StatusId").val() ? parseInt($("#StatusId").val()) : null,
                InvoiceDateStart: InvoiceDateStart || null,
                InvoiceDateEnd: InvoiceDateEnd || null,
                CustomerId: CustomerId ? parseInt(CustomerId) : null
            };
        }

        function searchReport() {
            $.ajax({
                type: 'POST',
                url: window.URLs.getReportUrl,
                dataType: 'html',
                contentType: 'application/json',
                data: JSON.stringify(getReportModel())
            }).done(function (result) {
                if (!result) return;

                $("#ReportListContainer").html(result);
                initMainTable();

            }).fail(function (xhr, status, error) {
                console.error("Error:", error);
                $("#ReportListContainer").html('<div class="alert alert-danger">Error loading report data.</div>');
            });
        }

        $("#btnSearchReport").on("click", function () {
            searchReport();
        });

        $("#btnResetSearch").on("click", function () {
            $("#WIP").val("");
            $("#InvoiceDateStart").val("");
            $("#InvoiceDateEnd").val("");
            $("#CustomerId").val("").trigger("change");
            $("#TypeId").val("").trigger("change");
            $("#TypeIds").val(null).trigger("change");
            $("#StatusId").val("").trigger("change");

            $("#ReportListContainer").html(
                '<table class="table table-bordered table-hover text-center w-100 main-table">' +
                '<thead><tr>' +
                '<th>WIP</th><th>Invoice Date</th><th>Invoice Number</th><th>Company Code</th>' +
                '<th>Account</th><th>Department</th><th>Customer Name</th><th>Total Amount</th>' +
                '<th>Total Labours</th><th>Total Parts</th><th>VIN</th><th>Plate Number</th>' +
                '<th>Manufacture Year</th><th>OP Number</th><th>OP Name</th>' +
                '<th>Service Code</th><th>Service Description</th>' +
                '</tr></thead>' +
                '<tbody><tr><td colspan="17">No data available. Please adjust your filters and search.</td></tr></tbody>' +
                '</table>'
            );

            initMainTable();
        });

        $("#btnPrintReport").on("click", function () {
            $.ajax({
                type: 'POST',
                url: window.URLs.printReportUrl,
                contentType: 'application/json',
                data: JSON.stringify(getPrintExportModel()),
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (data, status, xhr) {
                    var contentType = xhr.getResponseHeader('Content-Type');

                    if (contentType && contentType.indexOf('application/pdf') !== -1) {
                        var blob = new Blob([data], { type: 'application/pdf' });
                        var url = window.URL.createObjectURL(blob);
                        window.open(url, '_blank');
                    } else {
                        var reader = new FileReader();
                        reader.onload = function () {
                            alert("Error generating PDF report: " + reader.result);
                        };
                        reader.readAsText(data);
                    }
                },
                error: function () {
                    alert("Error generating PDF report. Please try again.");
                }
            });
        });

        $("#btnExportExcel").on("click", function () {
            $.ajax({
                type: 'POST',
                url: window.URLs.exportExcelUrl,
                contentType: 'application/json',
                data: JSON.stringify(getPrintExportModel()),
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (data, status, xhr) {
                    var contentType = xhr.getResponseHeader('Content-Type');

                    if (contentType && contentType.indexOf('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') !== -1) {
                        var blob = new Blob([data], {
                            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
                        });

                        var url = window.URL.createObjectURL(blob);
                        var link = document.createElement('a');

                        link.href = url;
                        link.download = 'Report_' + new Date().toISOString().slice(0, 19).replace(/:/g, '') + '.xlsx';

                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);

                        window.URL.revokeObjectURL(url);
                    } else {
                        var reader = new FileReader();
                        reader.onload = function () {
                            alert("Error generating Excel report: " + reader.result);
                        };
                        reader.readAsText(data);
                    }
                },
                error: function () {
                    alert("Error generating Excel report. Please try again.");
                }
            });
        });

        let searchTimeout;
        $("#WIP, #InvoiceDateStart, #InvoiceDateEnd, #CustomerId, #TypeId, #TypeIds, #StatusId").on("input change", function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                // searchReport();
            }, 500);
        });

    });

})(jQuery, window, document);