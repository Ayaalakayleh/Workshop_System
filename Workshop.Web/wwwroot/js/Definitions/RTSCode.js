$(document).ready(function () {

    (function ($, window, document) {
        "use strict";

        // ===== i18n fallback =====
        var requiredText =
            (window.RazorVars && RazorVars.required_field) ||
            (window.resources && window.resources.required_field) ||
            "This field is required";

        window.resources = window.resources || {};
        window.resources.required_field = window.resources.required_field || requiredText;

        var successMsg = (window.resources && window.resources.success_msg) || "Saved successfully";
        var okText = (window.resources && window.resources.ok) || "OK";

        // ===== soft assertions =====
        function assertLibs() {
            if (!$) {
                console.error("[RTSCode] jQuery not found.");
                return false;
            }
            if (!$.fn || !$.fn.validate) {
                console.error("[RTSCode] jquery.validate not found. Make sure it’s included BEFORE RTSCode.js.");
                return false;
            }
            return true;
        }

        function assertForm() {
            var $form = $("#rtsform");
            if ($form.length === 0) {
                console.error("[RTSCode] #rtsform not found in DOM.");
                return null;
            }
            return $form;
        }

        // ===== custom rules =====
        function registerCustomRules() {
            if (!$.validator || !$.validator.addMethod) return;

            $.validator.addMethod(
                "atLeastOne",
                function (value, element) {
                    var $el = $(element);
                    if ($el.prop("multiple")) {
                        var v = $el.val();
                        return v && v.length > 0;
                    }
                    return $.trim(value || "").length > 0;
                },
                window.resources.required_field
            );
        }

        // ===== attach validator =====
        function attachValidation($form, rules, messages) {
            var baseSettings = {
                ignore: [],
                highlight: function (element) {
                    $(element).addClass("is-invalid");
                    var $s2 = $(element).next(".select2-container");
                    if ($s2.length) $s2.addClass("is-invalid");
                },
                unhighlight: function (element) {
                    $(element).removeClass("is-invalid");
                    var $s2 = $(element).next(".select2-container");
                    if ($s2.length) $s2.removeClass("is-invalid");
                },
                errorPlacement: function (error, element) {
                    if (element.hasClass("form-check-input")) {
                        error.insertAfter(element.closest(".form-check"));
                    } else if (element.next(".select2-container").length) {
                        error.insertAfter(element.next(".select2-container"));
                    } else {
                        error.insertAfter(element);
                    }
                },
                invalidHandler: function (e, validator) {
                    if (validator.errorList.length) {
                        var first = validator.errorList[0].element;
                        $("html, body").animate({ scrollTop: $(first).offset().top - 120 }, 300);
                        $(first).focus();
                    }
                }
            };

            var v = $form.data("validator");
            if (v) {
                $.extend(true, v.settings, baseSettings);

                Object.keys(rules).forEach(function (name) {
                    var $el = $form.find("[name='" + name + "']");
                    if ($el.length) $el.rules("add", rules[name]);
                });
                $.extend(true, v.settings.messages, messages);
            } else {
                $form.validate($.extend(true, {}, baseSettings, { rules: rules, messages: messages }));
            }
        }

        // ===== validation init =====
        function initValidation() {
            if (!assertLibs()) return;

            var $form = assertForm();
            if (!$form) return;

            $form.attr("novalidate", "novalidate");

            var rules = {
                "RTSCode.Code": { required: true },
                "RTSCode.PrimaryDescription": { required: true },
                "RTSCode.SecondaryDescription": { required: true },
                "RTSCode.PrimaryName": { required: true, maxlength: 200 },
                "RTSCode.SecondaryName": { required: true, maxlength: 200 },
                "RTSCode.FK_CategoryId": { required: true },
                "RTSCode.FK_SkillId": { required: true },
                "RTSCode.StandardHours": { required: true, number: true },
                "RTSCode.FranchiseIds": { atLeastOne: false },
                "RTSCode.VehicleClassIds": { atLeastOne: false },
                "RTSCode.EffectiveDate": { required: true, date: true }
            };

            var messages = {
                "RTSCode.Code": { required: window.resources.required_field },
                "RTSCode.PrimaryDescription": { required: window.resources.required_field },
                "RTSCode.SecondaryDescription": { required: window.resources.required_field },
                "RTSCode.PrimaryName": { required: window.resources.required_field, maxlength: "Maximum 200 characters" },
                "RTSCode.SecondaryName": { required: window.resources.required_field, maxlength: "Maximum 200 characters" },
                "RTSCode.FK_CategoryId": { required: window.resources.required_field },
                "RTSCode.FK_SkillId": { required: window.resources.required_field },
                "RTSCode.StandardHours": { required: window.resources.required_field, number: "Must be a number" },
                "RTSCode.EffectiveDate": { required: window.resources.required_field, date: "Invalid date" }
            };

            attachValidation($form, rules, messages);

            $form.off("submit.rts-hardblock").on("submit.rts-hardblock", function (e) {
                var $f = $(this);

                if ($f.data("allowSubmit")) {
                    $f.data("allowSubmit", false);
                    return;
                }

                var isValid = $f.valid();
                if (!isValid) {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    return false;
                }
                e.preventDefault();
                e.stopImmediatePropagation();

                if (window.Swal && typeof Swal.fire === "function") {
                    Swal.fire({
                        icon: 'success',
                        title: "Success",
                        confirmButtonText: "Ok",
                        confirmButtonColor: 'var(--primary-600)',
                        timer: 3000,
                        timerProgressBar: true
                    }).then(function () {
                        $f.data("allowSubmit", true);
                        $f.trigger("submit");
                    });
                } else {
                    $f.data("allowSubmit", true);
                    $f.trigger("submit");
                }
            });

            $(document).on("change", "select", function () {
                $(this).valid();
            });
        }

        // ===== Events =====
        function wireEvents() {
            $("#ddlAllowedUnits").on("change", function () {
                var $sel = $("#ddlAllowedUnits");
                var selectedText = $sel.find("option:selected").text();
                var selectedValue = $sel.val();

                $("#txtUnitOfLabour").val(selectedText);
                $("#hdnUnitOfLabour").val(selectedText);
                $("#txtMinutesPerUnit").val(selectedValue);
                $("#hdnMinutesPerUnit").val(selectedValue);

                $("[name='RTSCode.StandardHours']")
                    .val((parseFloat(selectedValue || 0) || 0) / 60)
                    .trigger("change")
                    .trigger("keyup");
            });

            $("#btnSearch").on("click", function () {
                getData();
            });
        }

        function getData(page) {
            page = page || 1;

            var FilterModel = {
                Name: $("#_Name").val(),
                Code: $("#_Code").val(),
                PageNumber: page
            };

            $.ajax({
                type: "GET",
                url: window.RazorVars.indexUrl,
                dataType: "html",
                data: FilterModel
            })
                .done(function (result) {
                    $("#contentList").html(result);

                    var $total = $("#total_pages");
                    if ($total.length && typeof $total.val() !== "undefined") {
                        $("#contentPager").attr("hidden", false);

                        try { $("#contentPager").pagination("destroy"); } catch (e) { }

                        $("#contentPager").pagination({
                            items: $total.val(),
                            itemOnPage: 25,
                            currentPage: page,
                            prevText: "&laquo;",
                            nextText: "&raquo;",
                            onPageClick: function (p) {
                                getData(p);
                            }
                        });

                        $("#contentPager ul").addClass("pagination");
                        $("#contentPager li").addClass("page-item");
                        $("#contentPager a, #contentPager span").addClass("page-link");
                    } else {
                        $("#contentPager").attr("hidden", true);
                    }
                });
        }
        window.getData = getData;

        // ===== Bootstrapping =====
        $(function () {
            if (!assertLibs()) return;

            registerCustomRules();
            initValidation();
            wireEvents();
            getData(1);

            var $form = $("#rtsform");
            console.info("[RTSCode] Ready.", {
                formFound: $form.length,
                validatorAttached: !!($form.data("validator")),
                requiredText: window.resources.required_field
            });
        });

    })(jQuery, window, document);


    // --------------------------------------------------------
    // Vehicle Make/Model
    // --------------------------------------------------------
    function onMakeChange(makeId, modelId, lang) {
        var id = $(makeId).val();
        $(modelId).val('').trigger('change');

        $(modelId).empty().append('<option value="">Select</option>');

        if (!id) {
            $(modelId).prop('disabled', true);
            return;
        }

        $(modelId).prop('disabled', false);

        $.ajax({
            type: 'GET',
            url: window.RazorVars.fillVehicleModelUrl,
            data: { id: id },
            dataType: 'json',
            success: function (data) {
                $.each(data, function (key, value) {
                    var modelName = lang === "en" ? value.vehicleModelPrimaryName : value.vehicleModelSecondaryName;
                    $(modelId).append('<option value="' + value.id + '">' + modelName + '</option>');
                });
            }
        });
    }


    // --------------------------------------------------------
    // Delete
    // --------------------------------------------------------
    $(document).ready(function () {

        function deleteCode(id) {
            $.ajax({
                type: 'POST',
                url: window.RazorVars.deleteCodeUrl,
                data: { id: id },
                dataType: 'json',
                success: function (data) {
                    if (data > 0) {
                        location.reload();
                    } else {
                        console.error('Unable to delete the code.');
                    }
                },
                error: function () {
                    console.error('An error occurred while deleting the code.');
                }
            });
        }

        $(document).on('click', '.DeleteBTN', function (e) {
            e.preventDefault();
            const id = $(this).data('id');
            deleteCode(id);
        });

    });


    // --------------------------------------------------------
    // DevExpress Grid
    // --------------------------------------------------------
    var counter = -1;

    $(() => {
        function getSelectedFranchises() {
            const selected = $('#RTSCode_FranchiseIds option:selected').map(function () {
                return { Value: $(this).val(), Text: $(this).text() };
            }).get();

            if (selected.length > 0) return selected;

            return $('#RTSCode_FranchiseIds option:not(:selected)').map(function () {
                return { Value: $(this).val(), Text: $(this).text() };
            }).get();
        }

        function getSelectedRTS() {
            const selected = $('#RTSCodeVehicleClassIds option:selected').map(function () {
                return { Value: $(this).val(), Text: $(this).text() };
            }).get();

            if (selected.length > 0) return selected;

            return $('#RTSCodeVehicleClassIds option:not(:selected)').map(function () {
                return { Value: $(this).val(), Text: $(this).text() };
            }).get();
        }

        // Initial lists
        let franchiseList = getSelectedFranchises();
        let RTSList = getSelectedRTS();

        const grid = $('#gridContainer').dxDataGrid({
            dataSource: AllowedTimes ?? [],
            keyExpr: 'Id',
            showBorders: true,
            noDataText: window.RazorVars.DXNoData,
            columns: [
                { caption: 'ID', dataField: 'Id', visible: false },
                {
                    caption: window.RazorVars.DXManufacturer,
                    dataField: 'Make',
                    setCellValue(rowData, value) {
                        rowData.Make = value;
                        rowData.ModelId = null;
                    },
                    lookup: {
                        dataSource: franchiseList,
                        valueExpr: 'Value',
                        displayExpr: 'Text'
                    },
                    placeholder: "Select"
                },
                {
                    caption: window.RazorVars.DXVehicleModel,
                    dataField: 'Model',
                    lookup: {
                        dataSource(options) {
                            return {
                                store: Vehicle,
                                filter: options.data ? ['ManufacturerId', '=', options.data.Make] : null,
                            };
                        },
                        valueExpr: 'Id',
                        displayExpr: 'Name'
                    }
                },
                {
                    caption: window.RazorVars.DXVehicleClass,
                    dataField: 'VehicleClass',
                    setCellValue(rowData, value) {
                        rowData.VehicleClass = value;
                        rowData.ModelId = null;
                    },
                    lookup: {
                        dataSource: RTSList,
                        valueExpr: 'Value',
                        displayExpr: 'Text'
                    }
                },
                {
                    caption: window.RazorVars.DXYear,
                    dataField: 'Year',
                    dataType: 'number',
                    editorOptions: { format: '', max: 9999 }
                },
                {
                    caption: RazorVars.DXAllowedHour,
                    dataField: 'AllowedHours',
                    dataType: 'number',
                    editorOptions: { format: '', min: 0, step: 0.01 }
                },
                {
                    type: 'buttons',
                    width: 110,
                    buttons: [
                        {
                            hint: 'Delete',
                            icon: 'trash',
                            onClick(e) {
                                var grid = e.component;
                                grid.getDataSource().store().remove(e.row.key);
                                grid.cancelEditData();
                                grid.refresh();
                            }
                        }
                    ]
                }
            ],
            allowColumnReordering: true,
            allowColumnResizing: true,
            columnAutoWidth: true,
            rowAlternationEnabled: true,
            hoverStateEnabled: true,
            paging: { pageSize: 10 },
            pager: {
                visible: true,
                showPageSizeSelector: true,
                allowedPageSizes: [5, 10, 20, 50],
                showInfo: true,
                showNavigationButtons: true
            },
            editing: {
                mode: 'cell',
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            onInitNewRow: function (e) {
                e.data.Id = parseInt(e.data.Id) || counter--;
            }

        }).dxDataGrid('instance');

        $('#RTSCode_FranchiseIds').on('change', function () {
            franchiseList = getSelectedFranchises();
            grid.columnOption('Make', 'lookup.dataSource', franchiseList);
        });

        $('#RTSCodeVehicleClassIds').on('change', function () {
            RTSList = getSelectedRTS();
            grid.columnOption('VehicleClass', 'lookup.dataSource', RTSList);
        });

        $('#rtsform').on('submit', function () {
            const gridData = grid.getDataSource().items();
            $('#AllowedTimesJson').val(JSON.stringify(gridData));
        });
    });

    $(".copyInput2").on("blur", function () {
        var copyValue = $(this).val();
        $(".pasteInput2").val(copyValue);
    });

});
