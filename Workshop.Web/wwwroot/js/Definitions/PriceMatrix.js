$(document).ready(function () {

    // jQuery Validate rules
    $("#matrixForm").validate({
        rules: {
            AppliesTo: { required: true },
            //AccountType: { required: true },
            //AccountId: { required: true },
            Basis: { required: true },
            MatchValue: { required: true },
            RatePerHour: { required: true, number: true, min: 0 },
            Markup: { required: true, number: true, min: 0 },
            Name: { required: true }
        },
        messages: {
            AppliesTo: resources.required_field,
            //AccountType: resources.required_field,
            //AccountId: resources.required_field,
            Basis: resources.required_field,
            MatchValue: resources.required_field,
            RatePerHour: resources.required_field,
            Markup: resources.required_field,
            Name: resources.required_field
        },
        errorClass: "is-invalid",
        validClass: "is-valid",
        errorPlacement: function (error, element) {
            error.addClass("text-danger mt-1");

            // ✅ Handle Select2 fields separately
            if (element.hasClass("select2-hidden-accessible")) {
                error.insertAfter(element.next('.select2'));
            } else {
                error.insertAfter(element);
            }
        },
        highlight: function (element) {
            if ($(element).hasClass("select2-hidden-accessible")) {
                $(element).next(".select2").find(".select2-selection").addClass("is-invalid");
            } else {
                $(element).addClass("is-invalid");
            }
        },
        unhighlight: function (element) {
            if ($(element).hasClass("select2-hidden-accessible")) {
                $(element).next(".select2").find(".select2-selection").removeClass("is-invalid");
            } else {
                $(element).removeClass("is-invalid");
            }
        },
        submitHandler: function (form) {
            $("#btnCreate").prop('disabled', true)
                .html('<i class="fa fa-spinner fa-spin"></i>&nbsp;' + RazorVars.btnSaving);

            var formData = new FormData(form);

            $.ajax({
                url: $(form).attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    $("#btnCreate").prop('disabled', false)
                        .html('<i class="fa fa-save"></i>&nbsp;' + RazorVars.btnSave);

                    Swal.fire({
                        icon: 'success',
                        title: resources.success_msg,
                        confirmButtonText: RazorVars.btnOk,
                        confirmButtonColor: 'var(--primary-600)',
                        timer: 3000,
                        timerProgressBar: true
                    }).then(() => {
                        $('#matrixModal').modal('hide');
                        location.reload();
                    });
                },
                error: function () {
                    $("#btnCreate").prop('disabled', false)
                        .html('<i class="fa fa-save"></i>&nbsp;' + RazorVars.btnSave);
                    Swal.fire({
                        icon: 'error',
                        title: RazorVars.ErrorHappend,
                        confirmButtonText: RazorVars.btnTryAgain,
                        confirmButtonColor: '#dc3545'
                    });
                }
            });

            return false;
        }
    });

});
$(document).ready(function () {
    // Cache the fields
    var $rateField = $("#RatePerHour").closest(".col-md-6");
    var $markupField = $("#Markup").closest(".col-md-6");
    var $rateInput = $("#RatePerHour");
    var $markupInput = $("#Markup");

    // Hide initially
    $rateField.hide();
    $markupField.hide();

    // On dropdown change
    $("select[name='Applies']").on("change", function () {
        var val = $(this).val();

        // Hide all first
        $rateField.hide();
        $markupField.hide();

        if (val === "Labour") {
            $rateField.show();
            $markupInput.val(0);
        } else if (val === "Parts") {
            $markupField.show();
            $rateInput.val(0);
        }
    });
});

$(document).ready(function () {

    $('#addButton').on('click', function () {
        var recordId = $(this).data('id');

        // This is a simple example. A more robust solution would use AJAX to load a partial view.
        // For now, let's just update the form action.
        var form = $('#matrixForm');
        form.attr('action', RazorVars.editUrl + '/' + recordId);

        // Fill form fields

        $("#AccountNumber").val();
        $("#Name").val("");
        $("#Markup").val("");
        $("#RatePerHour").val("");
        $('#MatchValue').val("");
        $('#Basis').val("");
        $("#Id").val("");
        $('#MatchValue').trigger('change');
        $('#Basis').trigger('change');
        $('#Applies').trigger('change');
        $('#AccountType').trigger('change');
        // Reset customers when adding a new price matrix (id == 0/default)
        $("#Customers").val([]).trigger('change');

    });

    $('#edtButton').on('click', function () {
        var recordId = $(this).data('id');

        $.get(RazorVars.editUrl + '?id=' + recordId, function (data) {
            // Populate form fields with the received 'data'

            // ... and so on for all your form fields
        });
        // This is a simple example. A more robust solution would use AJAX to load a partial view.
        // For now, let's just update the form action.
        var form = $('#matrixForm');
        form.attr('action', RazorVars.editUrl + '/' + recordId);

        // Optionally, make an AJAX call to get the data and populate the form
        // Optionally, make an AJAX call to get the data and populate the form
        $.get(RazorVars.editUrl + '?id=' + recordId, function (data) {
            // Populate form fields with the received 'data'
            $('#MatchValue').val(data.matchValue);
            // ... and so on for all your form fields
        });

    });
    $('#addButton').on('click', function () {



    });
    $('#btnReset').on('click', function () {

        $('#_Name').val("");
        $('#AppliesTo').val("");
        $('#BasisId').val("");
        $('input[name="PriceMatrixFilter.PageNumber"]').val(1);
        $("#btnSearch").trigger('click');

    });
    
    // Reset page number to 1 when searching
    $('#btnSearch').on('click', function (e) {
        $('input[name="PriceMatrixFilter.PageNumber"]').val(1);
    });

});
function editButtonClick(button) {
    var recordId = $(button).data('id');

    /*$.get('/PriceMatrix/Edit?id=' + recordId, function (data) {
        // Populate form fields with the received 'data'
        $("#AppliesTo").val(data[0].AppliesTo);
        $("#AccountType").val(data[0].AccountType);
        // ... and so on for all your form fields
    });*/
    //var form = $('#matrixForm');
    //form.attr('action', '/PriceMatrix/Edit/' + recordId);

    loadPriceById(recordId);

}

// keep a handle to abort in-flight requests when Basis changes quickly
let basisRequest = null;

function onBasisChange(select) {
    const selectedValue = select.value;
    const $matchValue = $('#MatchValue');

    // Abort any previous pending request to avoid interleaved results
    if (basisRequest && basisRequest.readyState !== 4) {
        basisRequest.abort();
    }

    // Helper: keep placeholder (option with empty value) if you have one
    const hasPlaceholder = $matchValue.find('option[value=""]').length > 0;

    // Reset & (re)disable if "ALL"
    if (selectedValue === "0" || selectedValue === 0) {
        // clear all but placeholder
        $matchValue.find('option').not('[value=""]').remove();
        $matchValue.prop('disabled', true).val('');
        // fire once (use .trigger('change.select2') if you use Select2)
        $matchValue.trigger('change');
        return;
    }

    // Enable field and clear previous items (but keep placeholder if it exists)
    if (hasPlaceholder) {
        $matchValue.find('option').not('[value=""]').remove();
    } else {
        $matchValue.empty();
    }
    $matchValue.prop('disabled', false).val('').trigger('change');

    // Optional: show a loading state
    $matchValue.addClass('is-loading');

    // Fetch new options
    basisRequest = $.get(RazorVars.setBasisUrl, { id: selectedValue })
        .done(function (data) {
            if (!Array.isArray(data)) return;

            // Build options efficiently without stray text nodes
            const frag = document.createDocumentFragment();
            data.forEach(function (item) {
                const text = item.name ?? item.text ?? item.Code ?? '';
                const value = item.id ?? item.value ?? '';
                frag.appendChild(new Option(text, value, false, false));
            });
            $matchValue.append(frag);
        })
        .fail(function (xhr, status) {
            if (status !== 'abort') {
                console.error('Failed to load Match options', xhr);
            }
        })
        .always(function () {
            $matchValue.removeClass('is-loading');
            // Ensure value remains empty so user must pick after basis change
            $matchValue.val('').trigger('change'); // use 'change.select2' if Select2
        });
}


//function onAccountTypeChange(select) {

//    var id = $(select).find('option:selected').val();
//    $.get('/PriceMatrix/SetAccountType?id=' + id, function (data) {


//    }).done(function (data) {
//        var $matchValue = $('#AccountId');
//        $.each(data, function (i, item) {
//            $matchValue.append($('<option>', {
//                value: item.ID,
//                text: item.name
//            }));
//            $matchValue.append('</option>');
//        });

//    });

//}

function onAccountTypeChange(select) {
    var id = $(select).val();

    $.get(RazorVars.setAccountTypeUrl, { id: id }, function (data) {
        var $matchValue = $('#AccountId');
        $matchValue.empty();

        $.each(data, function (i, item) {
            $matchValue.append($('<option>', {
                value: item.value,
                text: item.text
            }));
        });
    });
}

function loadPriceById(teamId) {
    $.ajax({

        url: RazorVars.getPriceUrl,
        type: 'GET',
        contentType: 'application/json',
        data: { id: teamId },
        success: function (pricedata) {
            if (pricedata) {

                // Fill form fields
                // $("#AppliesTo").val(data.appliesTo);
                $("#AccountNumber").val(pricedata.accountNumber);
                $("#AccountId option").filter(function () {
                    return $(this).val() === pricedata.accountId;
                }).prop('selected', true);
                $("#Name").val(pricedata.name);
                $("#Markup").val(pricedata.markup);
                $("#RatePerHour").val(pricedata.ratePerHour);
                $("#MatchValue").val(pricedata.matchValue[0]);
                $("#Id").val(pricedata.id);
                $("#Basis").val(pricedata.basisId);

                // Applies to (Labour/Parts)
                $("#Applies").val(pricedata.appliesTo).trigger('change');

                // Customers multiselect
                const customerIds = pricedata.customers || pricedata.customersIds || [];
                $("#Customers").val(customerIds).trigger('change');

                // Account type & sales type
                $("#AccountType").val(pricedata.accountType).trigger('change');
                $.get(RazorVars.setAccountTypeUrl, { id: pricedata.accountType }, function (data) {
                    var $account = $('#AccountId');
                    $account.empty();
                    $.each(data, function (i, item) {
                        $account.append($('<option>', {
                            value: item.value ?? item.id ?? item.Id,
                            text: item.text ?? item.name ?? item.Name
                        }));
                    });
                    $account.val(pricedata.accountId).trigger('change');
                });

                // Basis + match values
                $("#Basis option").filter(function () {
                    return $(this).val() === String(pricedata.basisId);
                }).prop('selected', true);
                $('#Basis').trigger('change');

                $.get(RazorVars.setBasisUrl + '?id=' + pricedata.basisId, function (data) {
                }).done(function (data) {
                    var $matchValue = $('#MatchValue');
                    $matchValue.empty(); // optional: clear old options first
                    $.each(data, function (i, item) {
                        $matchValue.append($('<option>', {
                            value: item.id,
                            text: item.name ? item.name : item.code
                        }));
                    });

                    $.ajax({
                        url: RazorVars.getMatchedValuesUrl,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({
                            matchValue: pricedata.matchValue, // array of int
                            basisId: pricedata.basisId
                        }),
                    }).done(function (data) {
                        var $matchValue = $('#MatchValue');

                        $.each(data, function (i, item) {
                            $matchValue.append($('<option>', {
                                value: item.id,
                                text: item.name,
                                selected: true
                            }));
                        });

                        $('#MatchValue').trigger('change');
                    }).fail(function (xhr, status, error) {
                        console.error("Error fetching matched values:", error);
                    });

                });

            }

        },
        error: function () {
            alert("Failed to load team data.");
        }
    });
}

function deletePrice(teamId) {
    Swal.fire({
        title: RazorVars.swalDeleteTitle,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: 'var(--danger-600)',
        cancelButtonColor: 'var(--secondary-500)',
        confirmButtonText: RazorVars.swalDeleteConfirm,
        cancelButtonText: RazorVars.swalDeleteCancel
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: RazorVars.deletePriceUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(teamId),
                success: function (data) {
                    location.reload();
                },
                error: function () {
                    alert("Failed to delete price.");
                }
            });

        }
    });
}



$(document).ready(function () {
    var $totalPages = $("#total_pages");
    var $table = $('#data');
    var $tbody = $table.find('tbody');
    var rowsShown = 25;
    var rowsTotal = $tbody.find('tr').length;
    if (!rowsTotal) return;

    var numPages = $totalPages.val();
    var currentPage  = parseInt($('input[name="PriceMatrixFilter.PageNumber"]').val() || 1, 10);;


    // Wrap pagination inside a flexbox container aligned to the end
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

        // Prev
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

        // Page numbers
        for (var i = 0; i < numPages; i++) {
            if (i+1 === currentPage) {
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

        // Next
        if (currentPage === numPages) {

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

        if (idx < 0 || idx > numPages) return;
        currentPage = idx;

        //Added by Nader
                        var $pageInput = $('input[name="PriceMatrixFilter.PageNumber"]');
                        if ($pageInput.length === 0) {
                            $('#searchForm').append('<input type="hidden" name="PriceMatrixFilter.PageNumber" value="' + page + '">');
                        } else {
                            $pageInput.val(idx);
                        }
                        $('#searchForm').submit();
        //------------------
        renderRows(currentPage);
        renderPagination();
    }

    // First render
    renderRows(0);
    renderPagination();

    // Events (delegated)
    $nav.on('click', 'a.page-link[data-page]', function (e) {
        e.preventDefault();
        goToPage(parseInt($(this).text(), 10));
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
