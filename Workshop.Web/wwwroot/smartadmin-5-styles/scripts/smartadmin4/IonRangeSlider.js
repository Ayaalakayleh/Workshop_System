$(document).ready(function () {
    // Basic Single Slider
    $("#basic-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        onChange: function (data) {
            $("#basic-info").text("Value: " + data.from);
        }
    });

    // Range Slider
    $("#range-slider").ionRangeSlider({
        type: "double",
        min: 0,
        max: 100,
        from: 20,
        to: 80,
        onChange: function (data) {
            $("#range-info").text("Range: " + data.from + " - " + data.to);
        }
    });

    // Step Slider
    $("#step-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        step: 10,
        onChange: function (data) {
            $("#step-info").text("Value: " + data.from + " (Step: 10)");
        }
    });

    // Grid Slider
    $("#grid-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        grid: true,
        onChange: function (data) {
            $("#grid-info").text("Value: " + data.from);
        }
    });

    // Min/Max Slider
    $("#minmax-slider").ionRangeSlider({
        type: "double",
        min: 0,
        max: 100,
        from: 30,
        to: 70,
        min_interval: 10,
        max_interval: 60,
        onChange: function (data) {
            $("#minmax-info").text("Range: " + data.from + " - " + data.to + " (Min interval: 10, Max interval: 60)");
        }
    });

    // Prefix/Postfix Slider
    $("#prefix-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        prefix: "$",
        onChange: function (data) {
            $("#prefix-info").text("Price: $" + data.from);
        }
    });

    // Custom Values Slider
    $("#custom-slider").ionRangeSlider({
        values: ["Small", "Medium", "Large", "Extra Large"],
        from: 1,
        onChange: function (data) {
            $("#custom-info").text("Size: " + data.from_value);
        }
    });

    // Disabled Slider
    $("#disabled-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        disable: true
    });

    // Decimal Values
    $("#decimal-slider").ionRangeSlider({
        min: 0,
        max: 10,
        from: 5,
        step: 0.1,
        onChange: function (data) {
            $("#decimal-info").text("Value: " + data.from.toFixed(1));
        }
    });

    // Negative Values
    $("#negative-slider").ionRangeSlider({
        min: -50,
        max: 50,
        from: 0,
        onChange: function (data) {
            $("#negative-info").text("Value: " + data.from);
        }
    });

    // Prettify Numbers
    $("#prettify-slider").ionRangeSlider({
        min: 1000,
        max: 100000,
        from: 50000,
        prettify_enabled: true,
        prettify_separator: ",",
        onChange: function (data) {
            $("#prettify-info").text("Value: " + data.from.toLocaleString());
        }
    });

    // Hide Labels
    $("#hidden-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        hide_min_max: true,
        hide_from_to: true,
        onChange: function (data) {
            $("#hidden-info").text("Value: " + data.from + " (Labels hidden)");
        }
    });

    // Keyboard Support
    $("#keyboard-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        keyboard: true,
        onChange: function (data) {
            $("#keyboard-info").text("Value: " + data.from + " (Use arrow keys after clicking)");
        }
    });

    // No Animation
    $("#no-animate-slider").ionRangeSlider({
        min: 0,
        max: 100,
        from: 50,
        animate: false,
        onChange: function (data) {
            $("#no-animate-info").text("Value: " + data.from + " (No animation)");
        }
    });

    // Panel collapse functionality
    $(document).on('click', '[data-action="panel-collapse"]', function () {
        var panel = $(this).closest('.panel');
        var container = panel.find('.panel-container');

        if (container.hasClass('show')) {
            container.removeClass('show').addClass('collapsed');
        } else {
            container.removeClass('collapsed').addClass('show');
        }
    });

    $("#input-control-slider").ionRangeSlider({
        type: "double",
        min: 0,
        max: 100,
        from: 20,
        to: 80,
        onStart: function (data) {
            updateInputs(data.from, data.to);
        },
        onChange: function (data) {
            updateInputs(data.from, data.to);
        }
    });

    const slider = $("#input-control-slider").data("ionRangeSlider");

    // Update slider when inputs change
    $("#min-input, #max-input").on("input", function () {
        const minVal = parseInt($("#min-input").val()) || 0;
        const maxVal = parseInt($("#max-input").val()) || 100;

        slider.update({
            from: minVal,
            to: maxVal
        });

        $("#input-control-info").text(`Current Range: ${minVal} - ${maxVal}`);
    });

    function updateInputs(from, to) {
        $("#min-input").val(from);
        $("#max-input").val(to);
        $("#input-control-info").text(`Current Range: ${from} - ${to}`);
    }
});