// ~/js/Definitions/reports.js
(function ($, window, document) {
    "use strict";

    $(document).ready(function () {

        // Search report function
        function searchReport() {
            var WIP = $("#WIP").val();
            var InvoiceDateStart = $("#InvoiceDateStart").val();
            var InvoiceDateEnd = $("#InvoiceDateEnd").val();
            var CustomerId = $("#CustomerId").val();

            var model = {
                WIP: WIP ? parseInt(WIP) : null,
                TypeId: $("#TypeId").val() ? parseInt($("#TypeId").val()) : null,
                InvoiceDateStart: InvoiceDateStart || null,
                InvoiceDateEnd: InvoiceDateEnd || null,
                CustomerId: CustomerId ? parseInt(CustomerId) : null
            };

            $.ajax({
                type: 'POST',
                url: window.URLs.getReportUrl,
                dataType: 'html',
                contentType: 'application/json',
                data: JSON.stringify(model)
            }).done(function (result) {
                if (!result) return;

                $("#ReportListContainer").html(result);
            }).fail(function (xhr, status, error) {
                console.error("Error:", error);
                // Show error message to user
                $("#ReportListContainer").html('<div class="alert alert-danger">Error loading report data.</div>');
            });
        }

        // Bind search button
        $("#btnSearchReport").on("click", function () {
            searchReport();
        });

        // Bind reset button
        $("#btnResetSearch").on("click", function () {
            $("#WIP").val("");
            $("#InvoiceDateStart").val("");
            $("#InvoiceDateEnd").val("");
            $("#CustomerId").val("");
            $("#TypeId").val("");

            // Optionally reload with no filters (empty report)
            $("#ReportListContainer").html('<table class="table table-bordered table-hover text-center w-100 main-table"><thead><tr><th>WIP</th><th>Invoice Date</th><th>Invoice Number</th><th>Company Code</th><th>Account</th><th>Department</th><th>Customer Name</th><th>Total Amount</th><th>Total Labours</th><th>Total Parts</th><th>VIN</th><th>Plate Number</th><th>Manufacture Year</th><th>OP Number</th><th>OP Name</th><th>Service Code</th><th>Service Description</th></tr></thead><tbody><tr><td colspan="17">No data available. Please adjust your filters and search.</td></tr></tbody></table>');
        });

        // Bind print button
        $("#btnPrintReport").on("click", function () {
            var WIP = $("#WIP").val();
            var InvoiceDateStart = $("#InvoiceDateStart").val();
            var InvoiceDateEnd = $("#InvoiceDateEnd").val();
            var CustomerId = $("#CustomerId").val();

            var model = {
                WIP: WIP ? parseInt(WIP) : null,
                TypeId: $("#TypeId").val() ? parseInt($("#TypeId").val()) : null,
                InvoiceDateStart: InvoiceDateStart || null,
                InvoiceDateEnd: InvoiceDateEnd || null,
                CustomerId: CustomerId ? parseInt(CustomerId) : null
            };

            // Open PDF in new window/tab
            $.ajax({
                type: 'POST',
                url: window.URLs.printReportUrl,
                contentType: 'application/json',
                data: JSON.stringify(model),
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (data, status, xhr) {
                    // Check if response is actually a PDF
                    var contentType = xhr.getResponseHeader('Content-Type');
                    if (contentType && contentType.indexOf('application/pdf') !== -1) {
                        var blob = new Blob([data], { type: 'application/pdf' });
                        var url = window.URL.createObjectURL(blob);
                        window.open(url, '_blank');
                    } else {
                        // If not PDF, try to read as text to show error
                        var reader = new FileReader();
                        reader.onload = function () {
                            try {
                                var errorText = reader.result;
                                console.error("Server Error:", errorText);
                                alert("Error generating PDF report: " + errorText);
                            } catch (e) {
                                console.error("Error reading response:", e);
                                alert("Error generating PDF report. Please try again.");
                            }
                        };
                        reader.readAsText(data);
                    }
                },
                error: function (xhr, status, error) {
                    console.error("AJAX Error:", xhr.status, error);
                    var errorMsg = "Error generating PDF report. Please try again.";
                    if (xhr.response && xhr.response instanceof Blob) {
                        var reader = new FileReader();
                        reader.onload = function () {
                            try {
                                var errorText = reader.result;
                                var errorData = JSON.parse(errorText);
                                if (errorData.message) {
                                    errorMsg = "Error: " + errorData.message;
                                }
                            } catch (e) {
                                errorMsg = "Error: " + errorText;
                            }
                            alert(errorMsg);
                        };
                        reader.readAsText(xhr.response);
                    } else {
                        alert(errorMsg);
                    }
                }
            });
        });

        // Bind export to excel button
        $("#btnExportExcel").on("click", function () {
            var WIP = $("#WIP").val();
            var InvoiceDateStart = $("#InvoiceDateStart").val();
            var InvoiceDateEnd = $("#InvoiceDateEnd").val();
            var CustomerId = $("#CustomerId").val();

            var model = {
                WIP: WIP ? parseInt(WIP) : null,
                TypeId: $("#TypeId").val() ? parseInt($("#TypeId").val()) : null,
                InvoiceDateStart: InvoiceDateStart || null,
                InvoiceDateEnd: InvoiceDateEnd || null,
                CustomerId: CustomerId ? parseInt(CustomerId) : null
            };

            // Download Excel file
            $.ajax({
                type: 'POST',
                url: window.URLs.exportExcelUrl,
                contentType: 'application/json',
                data: JSON.stringify(model),
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (data, status, xhr) {
                    // Check if response is actually an Excel file
                    var contentType = xhr.getResponseHeader('Content-Type');
                    if (contentType && contentType.indexOf('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') !== -1) {
                        var blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                        var url = window.URL.createObjectURL(blob);
                        var link = document.createElement('a');
                        link.href = url;
                        link.download = 'Report_' + new Date().toISOString().slice(0, 19).replace(/:/g, '') + '.xlsx';
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                        window.URL.revokeObjectURL(url);
                    } else {
                        // If not Excel, try to read as text to show error
                        var reader = new FileReader();
                        reader.onload = function () {
                            try {
                                var errorText = reader.result;
                                console.error("Server Error:", errorText);
                                alert("Error generating Excel report: " + errorText);
                            } catch (e) {
                                console.error("Error reading response:", e);
                                alert("Error generating Excel report. Please try again.");
                            }
                        };
                        reader.readAsText(data);
                    }
                },
                error: function (xhr, status, error) {
                    console.error("AJAX Error:", xhr.status, error);
                    var errorMsg = "Error generating Excel report. Please try again.";
                    if (xhr.response && xhr.response instanceof Blob) {
                        var reader = new FileReader();
                        reader.onload = function () {
                            try {
                                var errorText = reader.result;
                                var errorData = JSON.parse(errorText);
                                if (errorData.message) {
                                    errorMsg = "Error: " + errorData.message;
                                }
                            } catch (e) {
                                errorMsg = "Error: " + errorText;
                            }
                            alert(errorMsg);
                        };
                        reader.readAsText(xhr.response);
                    } else {
                        alert(errorMsg);
                    }
                }
            });
        });

        // Optional: Auto-search on date/customer changes (debounced)
        let searchTimeout;
        $("#WIP, #InvoiceDateStart, #InvoiceDateEnd, #CustomerId, #TypeId").on("input change", function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                // Uncomment below if you want auto-search
                // searchReport();
            }, 500);
        });

    });

})(jQuery, window, document);
