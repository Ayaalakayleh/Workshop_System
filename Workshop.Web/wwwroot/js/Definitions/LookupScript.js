"use strict";
    function headerChosen() {
  var v = $('#HeaderId').val();
    return v && v !== "0";
}
$(document).on('click', 'button[type=reset]', function () {
    //const $form = $(this).closest('form');

    //// Let the browser reset everything to its original "selected" state
    //if ($form.length) $form[0].reset();

    //// Then normalize selects (especially Select2) to a default value
    //setTimeout(function () {
    //    const $scope = $form.length ? $form : $(document);

    //    $scope.find('select').each(function () {
    //        const $sel = $(this);
    //        const isMultiple = $sel.prop('multiple');

    //        // 1) If provided, honor an explicit default
    //        //    e.g. <select data-reset-value="0"> or data-reset-value="1,3" (for multiple)
    //        const explicit = $sel.data('resetValue');

    //        let value;
    //        if (explicit !== undefined) {
    //            value = isMultiple ? String(explicit).split(',') : String(explicit);
    //        } else {
    //            // 2) Otherwise use whatever the HTML had as the original default
    //            const defaults = $sel.find('option').filter(function () { return this.defaultSelected; });

    //            if (isMultiple) {
    //                value = defaults.map(function () { return this.value; }).get();
    //            } else if (defaults.length) {
    //                value = defaults.first().val();
    //            } else if ($sel.find('option[value="0"]').length) {
    //                // 3) Fallback: common pattern where "0" = "Select..."
    //                value = "0";
    //            } else {
    //                // 4) Last resort: first option
    //                value = $sel.find('option').first().val();
    //            }
    //        }

    //        if ($sel.hasClass('select2-hidden-accessible')) {
    //            $sel.val(value).trigger('change');   // refresh Select2 UI
    //        } else {
    //            $sel.val(value).trigger('change');   // keep everything in sync
    //        }
    //    });
    //}, 0);
    window.location.reload();
});

    function syncSearchEnabled() {
  var enabled = headerChosen();
    var $btn = $('#searchCategoryFilter');
    if (!$btn.length) return;

    if ($btn.is('button, input')) {
        $btn.prop('disabled', !enabled);
  } else {
        $btn.toggleClass('disabled', !enabled)
            .attr('aria-disabled', String(!enabled))
            .css('pointer-events', enabled ? '' : 'none')
            .attr('tabindex', enabled ? '0' : '-1');
  }
}
    $(document).ready(function () {
        $(".addNewLKP").attr("hidden", true);
    syncSearchEnabled();
    $('#HeaderId').on('change input', syncSearchEnabled);
});

    /* Hard-guard the search trigger */
    $(document).on('click', '#searchCategoryFilter', function (e) {
  if (!headerChosen()) {
        e.preventDefault();
    e.stopImmediatePropagation();
    return false;
  }
  // If your button isn't otherwise wired, call search() here:
  // search();
});

    /* ===================== Search ===================== */
    function search() {
  var categoryId = $('#HeaderId').val();

    $(".addNewLKP").attr("hidden", false);

    if (!categoryId || categoryId == 0) {
        syncSearchEnabled();
    return; // guard: do nothing without a chosen header
  }

    $.ajax({
        url: window.RazorVars.getDetailsByCategory,
    type: 'GET',
    data: {id: categoryId },
    success: function (result) {
        $('#tbl_lookup_body').html(result);

      // Ensure hidden HeaderId field & spans exist on each row
      $('#tbl_lookup_body > tr').each(function () {
        if ($(this).find('input[name="HeaderId"]').length === 0) {
        // Make sure table structure remains valid: add a hidden cell with the hidden input
        $(this).prepend(
            '<td class="d-none">' +
            '<input type="hidden" name="HeaderId" value="' + categoryId + '"/>' +
            '<span class="sn"></span>' +
            '</td>'
        );
        }

        // Ensure there are <span class="field-error"> placeholders in editable cells
        var ensureSpan = function ($cell) {
          if ($cell.find('span.field-error').length === 0) {
            $cell.append('<span class="field-error"></span>');
          }
        };
        // Assuming columns: 1:Code, 2:PrimaryName, 3:SecondaryName
        ensureSpan($(this).find('td').eq(1));
        ensureSpan($(this).find('td').eq(2));
        ensureSpan($(this).find('td').eq(3));
      });
    },
        error: function (err) {
            console.error(err);
        alert('Failed to load details.');
    }
  });
}

        /* ===================== Delete Lookup ===================== */
        function DeleteLookup(ID) {
            Swal.fire({
                title: RazorVars.swalDeleteTitle,
                text: '',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: 'var(--danger-600)',
                cancelButtonColor: 'var(--secondary-500)',
                confirmButtonText: RazorVars.swalDeleteConfirm,
                cancelButtonText: RazorVars.swalDeleteCancel
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        type: 'POST',
                        url: RazorVars.deleteUrl + '?Id=' + ID,
                        contentType: 'application/json',
                        dataType: 'json',
                        data: {},
                        success: function (data) {
                            if (data === true) {
                                $("#" + ID).hide();
                            } else {
                                Swal.fire(RazorVars.swalErrorHappened, 'error');
                            }
                        },
                        error: function () {
                            Swal.fire(RazorVars.swalErrorHappened, 'error');
                        }
                    });
                }
            });
}

        /* ===================== Add New Row ===================== */
        var count = 0;
        $(document).on('click', '.add-record', function (e) {
            e.preventDefault();

        $(".editLKP").addClass("disable-click");
        var categoryId = $('#HeaderId').val();

        // Guard: don't allow adding if header not chosen
        if (!headerChosen()) {
            syncSearchEnabled();
        return;
  }

        $("#tbl_lookup_body").append(
        '<tr class="newLookup">' +
            '<td class="d-none">' +
                '<input type="hidden" name="HeaderId" value="' + categoryId + '" />' +
                '<span class="sn"></span>' +
                '</td>' +
            '<td>' +
                '<input type="text" name="Code" class="form-control" maxlength="4" />' +
                '<span class="field-error"></span>' +
                '</td>' +
            '<td>' +
                '<input type="text" name="PrimaryName" class="form-control" maxlength="250" />' +
                '<span class="field-error"></span>' +
                '</td>' +
            '<td>' +
                '<input type="text" name="SecondaryName" class="form-control" maxlength="250" />' +
                '<span class="field-error"></span>' +
                '</td>' +
            '<td>' +
                '<button type="button" class="btn btn-primary btn-sm submitLookup" hidden>' +
                    '<i class="fa fa-save"></i>&nbsp;' + RazorVars.save +
                    '</button>&nbsp;' +
                '<button type="button" onclick="search()" class="btn btn-secondary text-white btn-sm" data-id="0">' +
                    '<i class="fa fa-times"></i>&nbsp;' + RazorVars.cancel +
                    '</button>' +
                '</td>' +
            '</tr>'
        );

        count += 1;

        $(".submitLookup").removeAttr('hidden').first().focus();
        $("#hidSearchCategoryFilter").val(0);
        $(".addNewLKP").attr("hidden", false);
});

        /* ===================== Save (validate & POST) ===================== */
        $(document).on('click', '.submitLookup', function (e) {
            e.preventDefault();

        var $btn = $(this);
        var $row = $btn.closest('tr');

        // Helpers
        function tdSpan(tdIndex) {
    return $row.find('td').eq(tdIndex).find('span.field-error');
  }

        var requiredMsg = (window.resources && resources.required_field) || 'Required';

        // Collect fields (column indices: 1:Code, 2:PrimaryName, 3:SecondaryName)
        var $code      = $row.find('input[name="Code"]');
        var $primary   = $row.find('input[name="PrimaryName"]');
        var $secondary = $row.find('input[name="SecondaryName"]');
        var headerId   = $("#HeaderId").val();

        // Clear previous messages (span-only)
        $row.find('span.field-error').removeClass('invalid').text('');

        // Validate all fields (apply invalid on span only)
        var isValid = true;
        function requireFilled($input, tdIndex) {
    var val = $.trim($input.val());
        var $msg = tdSpan(tdIndex);
        if (!val) {
            $msg.addClass('invalid').text(requiredMsg);
        isValid = false;
    } else {
            $msg.removeClass('invalid').text('');
    }
  }

        requireFilled($code, 1);
        requireFilled($primary, 2);
        requireFilled($secondary, 3);

        if (!headerChosen()) {
            isValid = false;
    // You can show a global toast if desired
  }

        if (!isValid) return;

        // Build model
        var model = {
            Id: $row.attr('id') || null,
        Code: $code.val(),
        PrimaryName: $primary.val(),
        SecondaryName: $secondary.val(),
        HeaderId: headerId
  };

        // Choose save URL (configure RazorVars.saveUrl or .createUrl on server-side)
            var saveUrl = (window.RazorVars && (RazorVars.createUrl || RazorVars.saveUrl)) || window.RazorVars.createUrl;

        $btn.prop('disabled', true);

            $.ajax({
                url: saveUrl,
                type: 'POST',
                data: model,
                success: function (resp) {
                    $("#hidSearchCategoryFilter").val(1);
                    $(".editLKP").removeClass('disable-click');
                    search();
                },
                error: function (xhr) {
                    var $anyMsg = $row.find('span.field-error').first();
                    $anyMsg.addClass('invalid').text(RazorVars?.swalErrorHappened || 'Save failed.');
                },
                complete: function () { $btn.prop('disabled', false); }
            });

});

        /* ===================== Edit Lookup ===================== */
        function Editlookup(LookupId) {
  var categoryId = $('#HeaderId').val();
        $(".submitLookup").attr("hidden", true);
        $(".addNewLKP").attr("hidden", true);
        $(".editLKP").addClass('disable-click');
        $('#' + LookupId).load(lookupFindUrl + '?LookupId=' + LookupId + '&headerId=' + categoryId);
}

        /* ===================== Remove Draft Row / New Row UX ===================== */
        $(document).on('click', '.delete-record', function (e) {
            e.preventDefault();
        $(this).closest('tr').remove();

        if ($('.newLookup').length === 0) {
            $(".submitLookup").attr("hidden", true);
        $(".editLKP").removeClass("disable-click");
  }
        $(".addNewLKP").attr("hidden", false);
});

        $(document).on('click', '.addNewLKP', function () {
            $(".addNewLKP").attr("hidden", true);
});