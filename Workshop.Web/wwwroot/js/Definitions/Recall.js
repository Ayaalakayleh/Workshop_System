const LABELS = (typeof window !== "undefined" && window.LABELS) ? window.LABELS : {};
const VALIDATION = (typeof window !== "undefined" && window.VALIDATION) ? window.VALIDATION : {};

var counter = -1;

$(function () {

    // 5) FilePond: Excel only + custom idle text
    if (window.FilePond) {
        FilePond.registerPlugin(
            // eslint-disable-next-line no-undef
            FilePondPluginFileValidateType
        );

        const pond = FilePond.create(document.querySelector('#ExcelVehicle'), {
            allowMultiple: false,
            required: false,
            credits: false,
            labelIdle: LABELS.FilepondIdle || "Drag & drop Excel or <span class='filepond--label-action'>Browse</span>",
            acceptedFileTypes: [
                'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                'application/vnd.ms-excel',
                '.xlsx',
                '.xls'
            ],
            fileValidateTypeLabelExpectedTypes: LABELS.FileTypeExcelOnly || 'Excel files only'
        });
        window.__pondExcel = pond;
    }

    // 3) DevExtreme DataGrid setup
    let grid = $("#gridContainer").dxDataGrid({
        dataSource: typeof VehcilesRecall !== "undefined" && VehcilesRecall ? VehcilesRecall : [],
        keyExpr: "Id",
        showBorders: true,
        noDataText: LABELS.NoData || "No data",
        export: { enabled: true },
        editing: {
            mode: "cell",
            allowAdding: true,
            allowUpdating: true,
            allowDeleting: true,
            useIcons: true,
            texts: { confirmDeleteMessage: null }
        },
        columns: [
            { dataField: "Id", caption: LABELS.Id || "ID", visible: false },
            {
                dataField: "MakeID",
                caption: LABELS.Make || "Make",
                allowEditing: true,
                validationRules: [{ type: "required", message: VALIDATION.Required || "Required" }],
                setCellValue(rowData, value) { rowData.MakeID = value; },
                lookup: {
                    dataSource: (typeof MakeID !== "undefined" && MakeID) ? MakeID : [],
                    valueExpr: 'Value',
                    displayExpr: 'Text'
                }
            },
            {
                dataField: "ModelID",
                caption: LABELS.Model || "Model",
                allowEditing: true,
                validationRules: [{ type: "required", message: VALIDATION.Required || "Required" }],
                lookup: {
                    dataSource(options) {
                        const store = (typeof Vehicle !== "undefined" && Vehicle) ? Vehicle : [];
                        return {
                            store,
                            filter: options?.data ? ['ManufacturerId', '=', options.data.MakeID] : null
                        };
                    },
                    valueExpr: 'Id',
                    displayExpr: 'Name'
                }
            },
            {
                dataField: "Chassis",
                caption: LABELS.Chassis || "Chassis",
                allowEditing: true,
                validationRules: [{ type: "required", message: VALIDATION.Required || "Required" }],
                editorType: "dxTextBox",
                editorOptions: {
                    mode: "text",
                    placeholder: LABELS.ChassisPlaceholder || ""
                },
                setCellValue(rowData, value) { rowData.Chassis = value; }
            },
            {
                caption: LABELS.Action || "Action",
                type: "buttons",
                width: 80,
                buttons: ["delete"]
            }
        ],
        onInitNewRow: (e) => {
            e.data.Id = parseInt(e.data.Id) || counter--;
        },
        onRowRemoving: function (e) {
            if (!window.Swal) return;
            e.cancel = true;
            Swal.fire({
                title: LABELS.DeleteConfirmTitle || 'Confirm',
                text: LABELS.DeleteConfirmText || 'Are you sure you want to delete?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: LABELS.Yes || 'Yes',
                cancelButtonText: LABELS.No || 'No'
            }).then((result) => {
                if (result.isConfirmed) {
                    e.component.getDataSource().store().remove(e.key)
                        .then(() => e.component.refresh());
                }
            });
        },
        onExporting: function (e) {
            if (!window.ExcelJS || !DevExpress?.excelExporter) return;

            var workbook = new ExcelJS.Workbook();
            var worksheet = workbook.addWorksheet("Recall");
            var lookupSheet = workbook.addWorksheet("List");

            var dataLookups = {
                MakeID: ((typeof MakeID !== "undefined" && MakeID) ? MakeID : []).map(v => v.Text),
                ModelID: ((typeof Vehicle !== "undefined" && Vehicle) ? Vehicle : []).map(v => v.Name),
                Chassis: ((typeof Chasses !== "undefined" && Chasses) ? Chasses : []).map(v => v.ChassisNo)
            };

            let colStart = 1;
            let mapping = {};

            Object.keys(dataLookups).forEach((key) => {
                let list = dataLookups[key] || [];
                list.forEach((value, rowIndex) => (lookupSheet.getCell(rowIndex + 1, colStart).value = value));
                mapping[key] = `List!$${String.fromCharCode(65 + colStart - 1)}$1:$${String.fromCharCode(65 + colStart - 1)}$${list.length}`;
                colStart++;
            });

            DevExpress.excelExporter.exportDataGrid({
                worksheet: worksheet,
                component: e.component,
                customizeCell: function (options) {
                    if (mapping[options.gridCell.column.dataField]) {
                        options.excelCell.dataValidation = {
                            type: "list",
                            allowBlank: true,
                            formulae: [mapping[options.gridCell.column.dataField]]
                        };
                    }
                }
            }).then(function () {
                workbook.xlsx.writeBuffer().then(function (buffer) {
                    saveAs(new Blob([buffer], { type: "application/octet-stream" }), "Vehicles.xlsx");
                });
            });
        }
    }).dxDataGrid("instance");

    // jQuery Validate on Create/Edit form + simple grid validation
    if ($.fn.validate) {
        $("#recallForm").validate({
            ignore: [],
            rules: {
                Code: { required: true, maxlength: 50 },
                Title: { required: true, maxlength: 200 },
                Description: { required: true, maxlength: 2000 },
                StartDate: { required: true, date: true },
                EndDate: { required: true, date: true }
            },
            messages: {
                Code: { required: VALIDATION.Required || "Required" },
                Title: { required: VALIDATION.Required || "Required" },
                Description: { required: VALIDATION.Required || "Required" },
                StartDate: { required: VALIDATION.Required || "Required" },
                EndDate: { required: VALIDATION.Required || "Required" }
            },
            errorClass: "is-invalid",
            errorPlacement: function (error, element) {
                error.addClass("text-danger small");
                if (element.parent(".input-group").length) error.insertAfter(element.parent());
                else error.insertAfter(element);
            }
        });
    }

    // SUBMIT: success if (a) backend gives result > 0 OR (b) backend returns HTML page
    $('#recallForm').on('submit', function (evt) {
        evt.preventDefault();

        if ($.fn.validate && !$("#recallForm").valid()) {
            return false;
        }

        var s = $('#StartDate').val();
        var e = $('#EndDate').val();
        if (s && e && new Date(e) < new Date(s)) {
            if (window.Swal) {
                Swal.fire({
                    icon: 'error',
                    text: VALIDATION.DateRange || 'End date must be after start date'
                });
            }
            return false;
        }

        grid.saveEditData();
        const gridData = grid.getDataSource().items() || [];

        if (!gridData.length) {
            if (window.Swal) {
                Swal.fire({
                    icon: 'error',
                    text: VALIDATION.GridEmpty || 'Grid is empty'
                });
            }
            return false;
        }

        const badRow = gridData.find(r =>
            !r || r.MakeID == null || r.ModelID == null || (r.Chassis == null || r.Chassis === '')
        );
        if (badRow) {
            if (window.Swal) {
                Swal.fire({
                    icon: 'error',
                    text: VALIDATION.GridRequired || 'Please complete required grid fields'
                });
            }
            return false;
        }

        $('#recallJson').val(JSON.stringify(gridData));

        const $form = $(this);

        $.ajax({
            url: $form.attr('action'),
            type: $form.attr('method') || 'POST',
            data: $form.serialize(),
            // if your backend starts returning JSON, you can also set: dataType: 'json',
            success: function (res) {

                // 1) If backend returned HTML (what your screenshot showed): treat as SUCCESS
                if (typeof res === 'string' && res.trim().startsWith('<')) {
                    // Optional: console.log for sanity
                    console.log('Received HTML response from server, treating as success.');
                    if (window.Swal) {
                        Swal.fire({
                            icon: 'success',
                            title: LABELS.RecallSaveTitle || 'Success',
                            text: LABELS.RecallSaveText || 'Recall has been saved successfully.'
                        }).then(() => {
                            window.location.href = window.RazorVars.indexUrl;
                        });
                    } else {
                        alert('Recall has been saved successfully.');
                        window.location.href =window.RazorVars.indexUrl;
                    }
                    return;
                }

                // 2) Otherwise, try to follow your original requirement: "if result value > 0"
                let resultValue = 0;
                let message = null;

                if (typeof res === 'number') {
                    resultValue = res;
                } else if (typeof res === 'string') {
                    try {
                        const parsed = JSON.parse(res);
                        if (typeof parsed === 'number') {
                            resultValue = parsed;
                        } else if (parsed && typeof parsed === 'object') {
                            if (typeof parsed.result !== 'undefined') {
                                resultValue = parseInt(parsed.result, 10);
                            } else if (typeof parsed.success !== 'undefined') {
                                resultValue = parsed.success ? 1 : 0;
                            }
                            message = parsed.message || null;
                        }
                    } catch {
                        const num = parseInt(res, 10);
                        if (!isNaN(num)) {
                            resultValue = num;
                        }
                    }
                } else if (res && typeof res === 'object') {
                    if (typeof res.result !== 'undefined') {
                        resultValue = parseInt(res.result, 10);
                    } else if (typeof res.success !== 'undefined') {
                        resultValue = res.success ? 1 : 0;
                    }
                    message = res.message || null;
                }

                const isSuccess = !isNaN(resultValue) && resultValue > 0;

                if (isSuccess) {
                    if (window.Swal) {
                        Swal.fire({
                            icon: 'success',
                            title: LABELS.RecallSaveTitle || 'Success',
                            text: message || LABELS.RecallSaveText || 'Recall has been saved successfully.'
                        }).then(() => {
                            window.location.href = window.RazorVars.indexUrl;
                        });
                    } else {
                        alert(message || 'Recall has been saved successfully.');
                        window.location.href = window.RazorVars.indexUrl;
                    }
                } else {
                    if (window.Swal) {
                        Swal.fire({
                            icon: 'error',
                            title: LABELS.Error || 'Error',
                            text: message || 'Unable to save recall.'
                        });
                    } else {
                        alert(message || 'Unable to save recall.');
                    }
                }
            },
            error: function (xhr) {
                if (window.Swal) {
                    Swal.fire({
                        icon: 'error',
                        title: LABELS.Error || 'Error',
                        text: xhr.responseText || 'An error occurred while saving.'
                    });
                } else {
                    alert(xhr.responseText || 'An error occurred while saving.');
                }
            }
        });
    });

    // Excel import
    $('#RecallUpload').on('click', function () {
        var formData = new FormData();
        let fileBlob = null;

        if (window.__pondExcel) {
            const file = window.__pondExcel.getFile();
            fileBlob = file && file.file ? file.file : null;
        } else {
            fileBlob = $('#ExcelVehicle')[0]?.files?.[0] || null;
        }

        if (!fileBlob) {
            if (window.Swal) Swal.fire({ icon: 'warning', text: LABELS.FileRequired || 'Please select a file' });
            return;
        }

        const okTypes = [
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            'application/vnd.ms-excel'
        ];
        const name = fileBlob.name || '';
        const ext = name.substring(name.lastIndexOf('.')).toLowerCase();
        if (!okTypes.includes(fileBlob.type) && !['.xlsx', '.xls'].includes(ext)) {
            if (window.Swal) Swal.fire({ icon: 'error', text: LABELS.FileTypeExcelOnly || 'Excel files only' });
            return;
        }

        formData.append("ExcelVehicle", fileBlob);

        $.ajax({
            url: window.RazorVars.importTemplateUrl,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                (res.importedRows || []).forEach(r => {
                    grid.getDataSource().store().insert({
                        Id: parseInt(r.Id) || (counter--),
                        MakeID: r.makeID,
                        ModelID: r.modelID,
                        Chassis: r.chassis
                    }).then(() => grid.refresh());
                });
                grid.refresh();

                if (window.Swal) {
                    Swal.fire({
                        icon: 'success',
                        title: LABELS.ImportSuccessTitle || 'Imported',
                        text: LABELS.ImportSuccessText || 'File imported successfully.'
                    });
                }
            },
            error: function (error) {
                console.log(error);
                if (window.Swal) {
                    Swal.fire({
                        icon: 'error',
                        title: LABELS.Error || 'Error',
                        text: LABELS.ImportErrorText || 'An error occurred while importing the file.'
                    });
                }
            }
        });
    });

    // Download template
    $('#RecallDownload').on('click', function () {
        var formData = new FormData();
        var gridData = grid.getDataSource().items();
        formData.append("GridData", JSON.stringify(gridData));

        $.ajax({
            url: window.RazorVars.downloadTemplateUrl,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function () { console.log("Success"); },
            error: function (error) { console.log(error); }
        });
    });
});

/* INDEX PAGE helpers */
$(document).ready(function () {
    $('#ResetBTN').on('click', function () {
        $('#RecallFilter_Tittle').val('').trigger('change');
        $('#RecallFilter_Code').val('').trigger('change');
        $('#btnSearch').click();
    });
});

$(document).ready(function () {
    function deleteRecall(id) {
        $.ajax({
            type: 'POST',
            url: window.RazorVars.deleteRecallUrl,
            data: { Id: id },
            dataType: 'json',
            success: function (data) {
                if (data > 0) {
                    location.reload();
                } else {
                    console.error(data.message || 'Unable to delete the record.');
                }
            },
            error: function (xhr, status, error) {
                console.error('An error occurred while deleting:', error);
            }
        });
    }

    $(document).on('click', '.DeleteBTN', function (e) {
        e.preventDefault();
        const id = $(this).data('id');

        if (!window.Swal) {
            if (confirm('Are you sure?')) deleteRecall(id);
            return;
        }

        Swal.fire({
            title: LABELS.DeleteConfirmTitle || 'Confirm',
            text: LABELS.DeleteConfirmText || 'Are you sure you want to delete?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: LABELS.Yes || 'Yes',
            cancelButtonText: LABELS.No || 'No'
        }).then((result) => {
            if (result.isConfirmed) deleteRecall(id);
        });
    });
});

/* Simple client-side pager */
$(function () {
    var $table = $('#data');
    var $tbody = $table.find('tbody');
    var rowsShown = 25;
    var rowsTotal = $tbody.find('tr').length;
    if (!rowsTotal) return;

    var numPages = Math.ceil(rowsTotal / rowsShown);
    var currentPage = 0;

    var $wrapper = $('<div class="d-flex justify-content-end mt-2"></div>');
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
            $nav.append(
                '<li class="page-item disabled">' +
                '<span class="page-link" aria-label="Previous">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-right"></i></span>' +
                '</span>' +
                '</li>'
            );
        } else {
            $nav.append(
                '<li class="page-item">' +
                '<a class="page-link" href="#" aria-label="Previous" data-prev="1">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-right"></i></span>' +
                '</a>' +
                '</li>'
            );
        }

        for (var i = 0; i < numPages; i++) {
            if (i === currentPage) {
                $nav.append(
                    '<li class="page-item active" aria-current="page">' +
                    '<span class="page-link">' + (i + 1) +
                    '<span class="visually-hidden">(current)</span>' +
                    '</span>' +
                    '</li>'
                );
            } else {
                $nav.append(
                    '<li class="page-item">' +
                    '<a class="page-link" href="#" data-page="' + i + '">' + (i + 1) + '</a>' +
                    '</li>'
                );
            }
        }

        if (currentPage === numPages - 1) {
            $nav.append(
                '<li class="page-item disabled">' +
                '<span class="page-link" aria-label="Next">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-left"></i></span>' +
                '</span>' +
                '</li>'
            );
        } else {
            $nav.append(
                '<li class="page-item">' +
                '<a class="page-link" href="#" aria-label="Next" data-next="1">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-left"></i></span>' +
                '</a>' +
                '</li>'
            );
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
        e.preventDefault();
        goToPage(parseInt($(this).attr('data-page'), 10));
    });

    $nav.on('click', 'a.page-link[data-prev]', function (e) {
        e.preventDefault();
        goToPage(currentPage - 1);
    });

    $nav.on('click', 'a.page-link[data-next]', function (e) {
        e.preventDefault();
        goToPage(currentPage + 1);
    });
});
