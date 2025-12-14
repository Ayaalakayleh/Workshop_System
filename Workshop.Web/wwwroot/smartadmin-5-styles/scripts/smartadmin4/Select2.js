$(document).ready(function () {
    // Initialize panel controls
    $('[data-action="panel-collapse"]').click(function () {
        const panelContainer = $(this).closest('.panel').find('.panel-container');
        panelContainer.toggleClass('show collapsed');
    });

    $('[data-action="panel-fullscreen"]').click(function () {
        const panel = $(this).closest('.panel');
        panel.toggleClass('fullscreen');
        if (panel.hasClass('fullscreen')) {
            panel.css({
                position: 'fixed',
                top: '0',
                left: '0',
                width: '100%',
                height: '100%',
                zIndex: '1050'
            });
        } else {
            panel.css({
                position: '',
                top: '',
                left: '',
                width: '',
                height: '',
                zIndex: ''
            });
        }
    });

    $('[data-action="panel-close"]').click(function () {
        $(this).closest('.panel').fadeOut(300, function () {
            $(this).remove();
        });
    });

    // Initialize Select2 examples

    // Example 1: Basic Select
    $('#basic-select').select2({
        placeholder: "Select a language",
        allowClear: true
    });

    // Example 2: Multi-Select
    $('#multi-select').select2({
        placeholder: "Select frameworks",
        closeOnSelect: false
    });

    // Example 3: AJAX Data Loading
    $('#ajax-select').select2({
        ajax: {
            url: 'https://api.github.com/search/repositories',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    q: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.items.map(item => ({
                        id: item.id,
                        text: item.full_name
                    }))
                };
            }
        },
        placeholder: 'Search GitHub repositories',
        minimumInputLength: 3
    });

    // Example 4: Templating
    function formatCountry(country) {
        if (!country.id) return country.text;
        const flagClass = 'fi fi-' + country.id.toLowerCase();
        return $('<span><span class="' + flagClass + '"></span> ' + country.text + '</span>');
    }

    $('#template-select').select2({
        templateResult: formatCountry,
        templateSelection: formatCountry
    });

    // Example 5: Icons Multiselect
    function formatIconOption(option) {
        if (!option.id) return option.text;

        const $option = $(option.element);
        const iconClass = $option.data('icon');

        if (!iconClass) return option.text;

        const $result = $(
            '<span><i class="' + iconClass + ' option-icon"></i>' + option.text + '</span>'
        );
        return $result;
    }

    function formatIconSelection(option) {
        if (!option.id) return option.text;

        const $option = $(option.element);
        const iconClass = $option.data('icon');

        if (!iconClass) return option.text;

        const $selection = $(
            '<span><i class="' + iconClass + ' selection-icon"></i>' + option.text + '</span>'
        );
        return $selection;
    }

    $('#icons-select').select2({
        placeholder: "Select menu items",
        templateResult: formatIconOption,
        templateSelection: formatIconSelection,
        closeOnSelect: false
    });

    // Example 6: Placeholder with Clear
    $('#placeholder-select').select2({
        placeholder: "Select your skill level",
        allowClear: true
    });
});