// Form validation and submission handling removed - now handled in Edit page



// Dependent dropdowns (search form only)
$(document).ready(function () {
    $("#_Manufacturer").on("change", function () {
        $.get(RazorVars.getVehicleByManufacturerIdURL, { manufacturerId: $(this).val() }, function (data) {
            let $_modelSelect = $('#_Model');
            $_modelSelect.empty().append(
                $('<option>', { value: '', text: RazorVars.placeholderSelect || 'Select' })
            );
            $.each(data, function (_, item) {
                $_modelSelect.append(
                    $('<option>', { value: item.id, text: item.name })
                );
            });
        });
    });
});

// Delete functionality (with SweetAlert2)
$(document).ready(function () {
    $(document).on("click", ".deleteBtn", function () {

        var $btn = $(this);
        var reminderId = $btn.data("id");
        var $row = $btn.closest("tr");

        Swal.fire({
            icon: "warning",
            title: theMainLang == "en" ? 'Are you sure?' : "هل أنت متأكد؟",
            showCancelButton: true,
            confirmButtonColor: "var(--danger-800)",
            confirmButtonText: theMainLang == "en" ? 'Yes' : "نعم",
            cancelButtonText: theMainLang == "en" ? 'No' : "لا",
            reverseButtons: true
        }).then(function (result) {

            // If user clicked Cancel -> just close (SweetAlert2 closes automatically)
            if (!result.isConfirmed) return;

            $.get("/ServiceReminder/Delete", { id: reminderId })
                .done(function () {
                    // Option A (best UX): remove row without reload
                    $row.fadeOut(250, function () {
                        $(this).remove();

                        Swal.fire({
                            icon: "success",
                            title: theMainLang == "en" ? 'Deleted Successfully' : "تمت عملية الحذف بنجاح تام",
                            timer: 900,
                            showConfirmButton: false
                        });
                    });
                })
        });
    });
});



$(document).ready(function () {

    $(document).on("click", "#btnReset", function () {
        $("#_Manufacturer").val('').trigger('change');
        $("#_Services").val('').trigger('change');
        $("#Model").val('').trigger('change');
        $("#_Vehicle").val('').trigger('change');
        $("#_Year").val('').trigger('change');
        $("#btnSearch").trigger('click');
    });

    // Normalize chart data to ensure it has the required fields
    function normalizeChartData(data) {
        if (!data) {
            console.warn("normalizeChartData: No data provided", data);
            return [];
        }

        console.log("normalizeChartData: Raw data received:", JSON.stringify(data, null, 2));

        var normalized = [];

        // Handle case where data is an object with status names as keys
        // e.g., { "Scheduled": 60, "Due Soon": 30, "Overdue": 10 }
        if (!Array.isArray(data) && typeof data === 'object') {
            console.log("normalizeChartData: Data is an object, converting to array format");
            for (var key in data) {
                if (data.hasOwnProperty(key)) {
                    var value = data[key];
                    var numValue = Number(value);
                    if (!isNaN(numValue) && numValue >= 0) {
                        normalized.push({
                            StatusName: key,
                            StatusValue: numValue
                        });
                        console.log("normalizeChartData: Added from object -", key + ":", numValue);
                    }
                }
            }
        }
        // Handle case where data is an array of objects
        else if (Array.isArray(data)) {
            console.log("normalizeChartData: Data is an array, processing items");
            normalized = data.map(function(item) {
                if (!item || typeof item !== 'object') {
                    console.warn("normalizeChartData: Invalid item:", item);
                    return null;
                }

                // Try to find StatusName field (check common variations)
                var statusName = item.StatusName || item.statusName || item.Status || item.status || item.name || item.label || item.key || '';
                
                // Try to find StatusValue field (check common variations)
                // Use !== undefined to handle 0 values correctly
                var statusValue = 0;
                if (item.StatusValue !== undefined) {
                    statusValue = item.StatusValue;
                } else if (item.statusValue !== undefined) {
                    statusValue = item.statusValue;
                } else if (item.Value !== undefined) {
                    statusValue = item.Value;
                } else if (item.value !== undefined) {
                    statusValue = item.value;
                } else if (item.count !== undefined) {
                    statusValue = item.count;
                } else if (item.Count !== undefined) {
                    statusValue = item.Count;
                } else if (item.quantity !== undefined) {
                    statusValue = item.quantity;
                } else if (item.Quantity !== undefined) {
                    statusValue = item.Quantity;
                }

                // Convert to number, ensuring we get the actual value
                var numValue = Number(statusValue);
                if (isNaN(numValue)) {
                    console.warn("normalizeChartData: Invalid StatusValue for item:", item, "converted value:", numValue);
                    numValue = 0;
                }

                var result = {
                    StatusName: statusName,
                    StatusValue: numValue
                };

                console.log("normalizeChartData: Normalized item:", result, "from original:", item);
                return result;
            }).filter(function(item) {
                // Filter out null items and items with missing StatusName
                if (!item) {
                    console.warn("normalizeChartData: Filtered out null item");
                    return false;
                }
                var hasName = item.StatusName && item.StatusName !== '';
                var hasValue = item.StatusValue !== undefined && item.StatusValue !== null;
                if (!hasName) {
                    console.warn("normalizeChartData: Filtered out item with missing StatusName:", item);
                }
                if (!hasValue) {
                    console.warn("normalizeChartData: Filtered out item with missing StatusValue:", item);
                }
                return hasName && hasValue;
            });
        } else {
            console.warn("normalizeChartData: Unknown data format:", typeof data, data);
            return [];
        }

        console.log("normalizeChartData: Final normalized data:", JSON.stringify(normalized, null, 2));
        console.log("normalizeChartData: Total items:", normalized.length);
        normalized.forEach(function(item) {
            console.log("  - " + item.StatusName + ": " + item.StatusValue);
        });
        
        return normalized;
    }

    // Chart initialization function
    function initChart() {
        var $chartElement = $('#StatusPieChart');
        
        // Check if chart element exists
        if (!$chartElement.length) {
            console.warn("Chart element #StatusPieChart not found");
            return;
        }

        // Check if dataSource exists and is valid
        if (!window.dataSource || !Array.isArray(window.dataSource)) {
            console.warn("No valid data source for chart");
            // Initialize with empty data to show empty state
            window.dataSource = [];
            window.Total = 0;
        }

        // Validate that data has required fields
        var hasValidData = window.dataSource.length > 0 && 
            window.dataSource.every(function(item) {
                return item && item.hasOwnProperty('StatusName') && item.hasOwnProperty('StatusValue');
            });

        if (!hasValidData && window.dataSource.length > 0) {
            console.warn("Data source missing required fields (StatusName, StatusValue). Data:", window.dataSource);
            // Try to normalize the data
            window.dataSource = normalizeChartData(window.dataSource);
        }

        // Calculate Total if not already calculated
        if (typeof window.Total === 'undefined' || window.Total === null) {
            window.Total = (window.dataSource || []).reduce(
                (sum, item) => sum + (Number(item.StatusValue) || 0),
                0
            );
        }

        // Check if chart is already initialized and dispose it
        // Use a safer method to check for existing instance
        var existingChart = null;
        try {
            // Check if element has dxInstance data attribute (DevExtreme stores instance here)
            if ($chartElement.data('dxInstance')) {
                existingChart = $chartElement.dxPieChart('instance');
            }
        } catch (e) {
            // Chart not initialized yet, which is fine
        }

        // Dispose existing chart if found
        if (existingChart) {
            try {
                existingChart.dispose();
            } catch (e) {
                console.warn("Error disposing existing chart:", e);
            }
        }

        // Initialize the chart
        try {
            $chartElement.dxPieChart({
                palette: ["var(--success-800)", "var(--warning-800)", "var(--danger-800)"],
                dataSource: window.dataSource || [],
                series: [{ argumentField: 'StatusName', valueField: 'StatusValue' }],
                title: (RazorVars.totalText || 'Total') + ': ' + (window.Total || 0),
                export: { enabled: true },
                legend: {
                    verticalAlignment: 'bottom',
                    horizontalAlignment: 'center',
                    itemTextPosition: 'bottom',
                },
                onPointClick: function(e) { 
                    toggleVisibility(e.target); 
                },
                onLegendClick: function(e) {
                    const arg = e.target;
                    toggleVisibility(this.getAllSeries()[0].getPointsByArg(arg)[0]);
                },
            });

            function toggleVisibility(item) {
                if (item.isVisible()) item.hide(); else item.show();
            }
        } catch (e) {
            console.error("Error initializing chart:", e);
        }
    }

    // Load chart data and initialize
    (function () {
        if (!window.dataSource) {
            $.get(RazorVars.getDueServiceRemindersURL, function (data) {
                console.log("=== Service Reminder Chart Data ===");
                console.log("Raw data from server:", JSON.stringify(data, null, 2));
                console.log("Data type:", typeof data);
                console.log("Is array:", Array.isArray(data));
                console.log("Is object:", typeof data === 'object' && data !== null && !Array.isArray(data));

                // Check if data is nested in a property (e.g., data.data, data.result, etc.)
                var actualData = data;
                if (data && typeof data === 'object' && !Array.isArray(data)) {
                    // Check common nested property names
                    if (data.data && (Array.isArray(data.data) || typeof data.data === 'object')) {
                        console.log("Data found in 'data' property");
                        actualData = data.data;
                    } else if (data.result && (Array.isArray(data.result) || typeof data.result === 'object')) {
                        console.log("Data found in 'result' property");
                        actualData = data.result;
                    } else if (data.items && (Array.isArray(data.items) || typeof data.items === 'object')) {
                        console.log("Data found in 'items' property");
                        actualData = data.items;
                    }
                }

                console.log("Actual data to process:", JSON.stringify(actualData, null, 2));

                // Validate and normalize data - handle both array and object formats
                if (actualData && (Array.isArray(actualData) || (typeof actualData === 'object' && actualData !== null))) {
                    // Log data structure before normalization
                    if (Array.isArray(actualData)) {
                        console.log("Items before normalization (array format):");
                        actualData.forEach(function(item, index) {
                            console.log("  Item " + index + ":", JSON.stringify(item, null, 2));
                        });
                    } else {
                        console.log("Items before normalization (object format):");
                        for (var key in actualData) {
                            if (actualData.hasOwnProperty(key)) {
                                console.log("  " + key + ":", actualData[key]);
                            }
                        }
                    }

                    // Normalize the data (handles both array and object formats)
                    window.dataSource = normalizeChartData(actualData);

                    // Calculate Total AFTER data loads
                    window.Total = window.dataSource.reduce(
                        function(sum, item) {
                            var value = Number(item.StatusValue) || 0;
                            console.log("Adding to total:", item.StatusName, "=", value);
                            return sum + value;
                        },
                        0
                    );

                    console.log("=== Normalized Data Summary ===");
                    console.log("Normalized Data:", JSON.stringify(window.dataSource, null, 2));
                    console.log("Number of categories:", window.dataSource.length);
                    window.dataSource.forEach(function(item) {
                        console.log("  - " + item.StatusName + ": " + item.StatusValue);
                    });
                    console.log("Total:", window.Total);

                    // Verify we have the expected categories
                    var expectedCategories = ["Scheduled", "Due Soon", "Overdue"];
                    var foundCategories = window.dataSource.map(function(item) { return item.StatusName; });
                    console.log("Expected categories:", expectedCategories);
                    console.log("Found categories:", foundCategories);
                    
                    // Check for missing categories
                    expectedCategories.forEach(function(cat) {
                        if (foundCategories.indexOf(cat) === -1) {
                            console.warn("Missing expected category:", cat);
                        }
                    });

                    // Now build the chart
                    initChart();
                } else {
                    console.warn("Invalid data format received:", data);
                    window.dataSource = [];
                    window.Total = 0;
                    initChart();
                }
            }).fail(function (xhr, status, error) {
                console.error("Error loading reminder data:", error);
                console.error("Response:", xhr.responseText);
                // Initialize with empty data on error
                window.dataSource = [];
                window.Total = 0;
                initChart();
            });
        } else {
            // If dataSource already exists (cached), normalize it and initialize chart
            console.log("Using cached dataSource:", window.dataSource);
            window.dataSource = normalizeChartData(window.dataSource);
            initChart();
        }
    })();

});

// Simple client-side pagination
$(function () {
    var $table = $('#data');
    var $tbody = $table.find('tbody');
    var rowsShown = 25;
    var rowsTotal = $tbody.find('tr').length;
    if (!rowsTotal) return;

    var numPages = Math.ceil(rowsTotal / rowsShown);
    var currentPage = 0;

    $('#data').next('.pagination-wrapper').remove();

    var $wrapper = $('<div class="pagination-wrapper d-flex justify-content-end mt-2"></div>');
    var $nav = $('<ul id="nav" class="pagination pagination-sm mb-0"></ul>');
    $wrapper.append($nav);
    $table.after($wrapper);

    function renderRows(pageIdx) {
        var startItem = pageIdx * rowsShown;
        var endItem = startItem + rowsShown;

        $tbody.find('tr')
            .stop(true, true).css('opacity', 0).hide()
            .slice(startItem, endItem)
            .css('display', 'table-row')
            .animate({ opacity: 1 }, 300);
    }

    function renderPagination() {
        $nav.empty();

        if (currentPage === 0) {
            $nav.append('<li class="page-item disabled"><span class="page-link" aria-label="Previous"><span aria-hidden="true"><i class="sa sa-chevron-left"></i></span></span></li>');
        } else {
            $nav.append('<li class="page-item"><a class="page-link" href="#" aria-label="Previous" data-prev="1"><span aria-hidden="true"><i class="sa sa-chevron-left"></i></span></a></li>');
        }

        for (var i = 0; i < numPages; i++) {
            if (i === currentPage) {
                $nav.append('<li class="page-item active" aria-current="page"><span class="page-link">' + (i + 1) + '<span class="visually-hidden">(current)</span></span></li>');
            } else {
                $nav.append('<li class="page-item"><a class="page-link" href="#" data-page="' + i + '">' + (i + 1) + '</a></li>');
            }
        }

        if (currentPage === numPages - 1) {
            $nav.append('<li class="page-item disabled"><span class="page-link" aria-label="Next"><span aria-hidden="true"><i class="sa sa-chevron-right"></i></span></span></li>');
        } else {
            $nav.append('<li class="page-item"><a class="page-link" href="#" aria-label="Next" data-next="1"><span aria-hidden="true"><i class="sa sa-chevron-right"></i></span></a></li>');
        }
    }

    function goToPage(idx) {
        if (idx < 0 || idx >= numPages) return;
        currentPage = idx;
        renderRows(currentPage);
        renderPagination();
    }

    renderRows(0);
    renderPagination();

    $nav.on('click', 'a.page-link[data-page]', function (e) {
        e.preventDefault(); goToPage(parseInt($(this).attr('data-page'), 10));
    });
    $nav.on('click', 'a.page-link[data-prev]', function (e) {
        e.preventDefault(); goToPage(currentPage - 1);
    });
    $nav.on('click', 'a.page-link[data-next]', function (e) {
        e.preventDefault(); goToPage(currentPage + 1);
    });
});
