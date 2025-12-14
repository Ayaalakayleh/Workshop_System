$(function () {
    $("#workshopform .form-group label").each(function () {
        // prevent double-adding if re-initialized
        if (!$(this).find(".text-danger.required-star").length) {
            $(this).append(' <span class="text-danger required-star">*</span>');
        }
    });
    $(document).on("change", ".select2", function () {
        $(this).valid();
    });
    $.validator.addMethod("joPhone", function (value, element) {
        if (this.optional(element)) return true;
        var v = value.replace(/\s+/g, "");
        return /^\d{9,10}$/.test(v);
    }, "Please enter a valid phone number.");

    $("#workshopform").validate({
        ignore: ":hidden:not(.select2-hidden-accessible)",
        highlight: function (element) {
            $(element).addClass("is-invalid");
            if ($(element).hasClass("select2-hidden-accessible")) {
                $(element).next(".select2").find(".select2-selection").addClass("is-invalid");
            }
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid");
            if ($(element).hasClass("select2-hidden-accessible")) {
                $(element).next(".select2").find(".select2-selection").removeClass("is-invalid");
            }
        },
        errorPlacement: function (error, element) {
            var name = element.attr("name");
            var mvcSpan = element.closest(".form-group").find("span[data-valmsg-for='" + name + "']");
            if (mvcSpan.length) {
                mvcSpan.empty().append(error);
                return;
            }
            if (element.hasClass("select2-hidden-accessible")) {
                element.next(".select2").after(error);
                return;
            }
            error.insertAfter(element);
        },
        rules: {
            PrimaryName: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            },
            SecondaryName: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            },
            Email: {
                required: true,
                email: true,
                normalizer: function (value) { return $.trim(value); }
            },
            Phone: {
                required: true,
                joPhone: true,
                normalizer: function (value) { return $.trim(value); }
            },
            PrimaryAddress: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            },
            SecondaryAddress: {
                required: true,
                normalizer: function (value) { return $.trim(value); }
            },
            GoogleURL: {
                required: true,
                url: true,
                normalizer: function (value) { return $.trim(value); }
            },
            CityId: {
                required: true
            },
            InsuranceCompanyIds: {
                // multiselect: at least one
                required: true
            },
            ParentId: {
                required: true
            },
            VatClassificationId: {
                required: true
            },
            // AccountId section is hidden in your markup — it will be ignored by our "ignore" rule
            SupplierId: {
                required: true
            }
        },
        messages: {
            PrimaryName: { required: resources.required_field },
            SecondaryName: { required: resources.required_field },
            Email: { required: resources.required_field },
            Phone: { required: resources.required_field },
            PrimaryAddress: { required: resources.required_field },
            SecondaryAddress: { required: resources.required_field },
            GoogleURL: { required: resources.required_field },
            CityId: { required: resources.required_field },
            InsuranceCompanyIds: { required: resources.required_field },
            ParentId: { required: resources.required_field },
            VatClassificationId: { required: resources.required_field },
            SupplierId: { required: resources.required_field }
        },
        // Prevent double submit + allow your own success handler to run
        submitHandler: function (form, event) {
            $("#btnCreate").prop("disabled", true);
            form.submit();
        }
    });

    // 5) Make sure the MVC validation spans are empty at start (nice UI)
    $("#workshopform span[data-valmsg-for]").html("&nbsp;");
});