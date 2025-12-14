$(function () {
    // Validate Select2 on change
    $(document).on("change", ".select2", function () {
        $(this).valid();
    });

    // Custom rule: GregorianToDate >= GregorianFromDate (use visible FromDate b/c you have two)
    $.validator.addMethod("dateGteFrom", function (value, element) {
        if (!value) return false;
        var fromVal = $("input[name='GregorianFromDate']:visible").first().val();
        if (!fromVal) return true; // required rule will handle empty from
        var to = new Date(value);
        var from = new Date(fromVal);
        if (isNaN(to.getTime()) || isNaN(from.getTime())) return true; // let 'date' rule handle parsing errors
        return to.getTime() >= from.getTime();
    }, "To Date must be on or after From Date.");

    // Init jQuery Validate
    $("#form").validate({
        ignore: ":hidden:not(.select2-hidden-accessible)",
        rules: {
            GregorianFromDate: { required: true, date: true },
            GregorianToDate: { required: true, date: true, dateGteFrom: true },
            ExternalWorkshopId: { required: true },
            InvoiceTypeId: { required: true }
        },
        errorClass: "is-invalid",
        validClass: "is-valid",
        highlight: function (el) {
            $(el).addClass("is-invalid");
            if ($(el).hasClass("select2-hidden-accessible")) {
                $(el).next(".select2").find(".select2-selection").addClass("is-invalid");
            }
        },
        unhighlight: function (el) {
            $(el).removeClass("is-invalid");
            if ($(el).hasClass("select2-hidden-accessible")) {
                $(el).next(".select2").find(".select2-selection").removeClass("is-invalid");
            }
        },
        errorPlacement: function (error, element) {
            error.addClass("text-danger mt-1");

            // MVC span support (if present)
            var name = element.attr("name");
            var mvcSpan = element.closest(".form-group").find("span[data-valmsg-for='" + name + "']");
            if (mvcSpan.length) {
                mvcSpan.empty().append(error);
                return;
            }

            // Input-group handling
            if (element.closest(".input-group").length) {
                error.insertAfter(element.closest(".input-group"));
                return;
            }

            // Select2 handling
            if (element.hasClass("select2-hidden-accessible")) {
                error.insertAfter(element.next(".select2"));
                return;
            }

            error.insertAfter(element);
        },
        // AJAX submit only when valid
        submitHandler: function (form) {
            var $form = $(form);
            $.ajax({
                type: 'POST',
                url: $form.attr('action') || RazorVars.EditUrl,
                data: $form.serialize(),
                success: function (result) {
                    $('#contentList').html(result);
                }
            });
        }
    });
});

    function GetDate() {
        if (parseInt($("#Month").val()) <= 12) {
            $.ajax({
                type: 'GET',
                url: RazorVars.GetDateUrl,
                contentType: 'application/json',
                dataType: 'json',
                data: { Month: $("#Month").val() },
                success: function (data) {

                    $("input[name='GregorianFromDate']:visible").first().val(data.startDate).trigger("change");
                    $("input[name='GregorianToDate']").val(data.endDate).trigger("change");
                    $("#ExternalWorkshopId").empty();
/*                    $("#ExternalWorkshopId").html('<option value="">@Common["Select"]</option>');*/
                    $.each(data.olworkshopDefinition, function (index, value) {
                        $("#ExternalWorkshopId").append(
                            '<option value="' + value.id + '">' + value.name + '</option>'
                        );
                    });
                },
                error: function () {
                    swal(RazorVars.commonError, RazorVars.ErrorHappended, 'error');
                }
            });
        }
    }

    function ChangeMonth() {

        GetDate();
    }

    // Optional: bind ChangeMonth automatically when Month changes
    $("#Month").on("change", ChangeMonth);

