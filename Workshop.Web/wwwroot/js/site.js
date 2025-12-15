$(document).ready(function () {
    $(".copyInput").on("blur", function () {
        var copyValue = $(this).val();
        $(".pasteInput").val(copyValue);
    });
    $('select').select2({
        theme: 'bootstrap-5',
        width: '100%',
        placeholder: function () {
            return $(this).data('placeholder') || 'Select an option';
        }
    });
    $('.main-table').DataTable({
        responsive: true,
        pageLength: 10,
        info: false,
        paging: false,
        dom: 'tp',
        language: {
            emptyTable: resources.EmptyData
        }
    });
    flatpickr(".flat-picker-today", {
        dateFormat: "Y-m-d",
        allowInput: false,
        clickOpens: true,
        minDate: "1950-01-01",
        maxDate: "2100-12-31",
        disableMobile: true,
        defaultDate: "today"
    });
    flatpickr(".flat-picker", {
        dateFormat: "Y-m-d",
        allowInput: false,
        clickOpens: true,
        minDate: "1950-01-01",
        maxDate: "2100-12-31",
        disableMobile: true
    });
    flatpickr(".flat-picker-future", {
        dateFormat: "Y-m-d",
        allowInput: false,
        clickOpens: true,
        minDate: "today",
        maxDate: "2100-12-31",
        disableMobile: true,
        defaultDate: "today"
    });


    $(document).on("input", 'input[type="number"]', function () {
        let val = $(this).val();
        if (val < 0) $(this).val('');
    });
    $(document).on("keydown", 'input[type="number"]', function (e) {
        if (e.key === '-' || e.keyCode === 189) {
            e.preventDefault();
        }
    });
    var phoneInput = document.querySelector(".phone-number");
    var iti = window.intlTelInput(phoneInput, {
        hiddenInput: "full_number",
        preferredCountries: ['sa', 'jo', 'eg'],
        autoPlaceholder: 'off',
        separateDialCode: true,
    });
    // Completely disable DataTables alert popups
    $.fn.dataTable.ext.errMode = 'none';

});
(function ($) {
    function sanitize($el) {
        let v = String($el.val());
        v = v.replace(/[^\d]/g, "");
        v = v.replace(/^0+(?=\d)/, "");

        $el.val(v);
    }
    $(document).on("input", ".make-integer", function () {
        sanitize($(this));
    });
    $(document).on("keydown", ".make-integer", function (e) {
        const k = e.key;
        const code = e.keyCode || e.which;
        const allowedCodes = [8, 9, 13, 27, 35, 36, 37, 38, 39, 40, 46];
        if (allowedCodes.includes(code) || e.ctrlKey || e.metaKey) return;
        if (k === "-" || k === "." || k === "," || k === "e" || k === "E" || k === " ") {
            e.preventDefault();
            return;
        }
        if (/^\d$/.test(k)) {
            const el = this;
            const hasSelection = el.selectionStart !== el.selectionEnd;
            if (!hasSelection && el.selectionStart === 0 && k === "0") {
                e.preventDefault();
            }
            return;
        }
        e.preventDefault();
    });
    $(document).on("blur", ".make-integer", function () {
        const $t = $(this);
        let v = String($t.val());
        v = v.replace(/^0+/, "");
        $t.val(v);
    });
    $(document).on('click', 'button[type=reset]', function () {
        const $form = $(this).closest('form');

        // Let the browser reset everything to its original "selected" state
        if ($form.length) $form[0].reset();

        // Then normalize selects (especially Select2) to a default value
        setTimeout(function () {
            const $scope = $form.length ? $form : $(document);

            $scope.find('select').each(function () {
                const $sel = $(this);
                const isMultiple = $sel.prop('multiple');

                // 1) If provided, honor an explicit default
                //    e.g. <select data-reset-value="0"> or data-reset-value="1,3" (for multiple)
                const explicit = $sel.data('resetValue');

                let value;
                if (explicit !== undefined) {
                    value = isMultiple ? String(explicit).split(',') : String(explicit);
                } else {
                    // 2) Otherwise use whatever the HTML had as the original default
                    const defaults = $sel.find('option').filter(function () { return this.defaultSelected; });

                    if (isMultiple) {
                        value = defaults.map(function () { return this.value; }).get();
                    } else if (defaults.length) {
                        value = defaults.first().val();
                    } else if ($sel.find('option[value="0"]').length) {
                        // 3) Fallback: common pattern where "0" = "Select..."
                        value = "0";
                    } else {
                        // 4) Last resort: first option
                        value = $sel.find('option').first().val();
                    }
                }

                if ($sel.hasClass('select2-hidden-accessible')) {
                    $sel.val(value).trigger('change');   // refresh Select2 UI
                } else {
                    $sel.val(value).trigger('change');   // keep everything in sync
                }
            });
        }, 0);
    });
})(jQuery);
