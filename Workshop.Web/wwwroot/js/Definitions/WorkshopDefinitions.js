$(function () {

    $.validator.addMethod("joPhone", function (value, element) {
        if (this.optional(element)) return true;
        var v = value.replace(/\s+/g, "");
        return /^\d{9,10}$/.test(v);
    }, "Please enter a valid phone number.");

    $("#workshopform").validate({
        highlight: function (element) {
            $(element).addClass("is-invalid");
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid");
        },
        errorPlacement: function (error, element) {
            var $el = $(element);
            var name = $el.attr("name");
            var $group = $el.closest(".form-group, .mb-3, .col-md-6, .col-12");

            // Prefer MVC span if it exists
            var $mvcSpan = $group.find("span[data-valmsg-for='" + name + "']");
            error.addClass("mt-1");

            if ($mvcSpan.length) {
                $mvcSpan.empty().append(error);
                return;
            }

            // Select2 (single & multiple): place after the visible container
            if ($el.hasClass("select2-hidden-accessible")) {
                error.insertAfter($el.next(".select2")); // or .select2-container
                return;
            }

            // intl-tel-input: place after the .iti wrapper
            var $iti = $el.closest(".iti");
            if ($iti.length) {
                error.insertAfter($iti);
                return;
            }

            // Fallback
            error.insertAfter($el);
        },
        rules: {
            PrimaryName: {
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
            GoogleURL: {
                required: true,
                url: true,
                normalizer: function (value) { return $.trim(value); }
            },
            CityId: { required: true },
            InsuranceCompanyIds: { required: true },
            VatClassificationId: { required: true },
            SupplierId: { required: true }
        },
        messages: {
            PrimaryName: { required: resources.required_field },
            Email: { required: resources.required_field, email: resources.valid_email },
            Phone: { required: resources.required_field },
            PrimaryAddress: { required: resources.required_field },
            GoogleURL: { required: resources.required_field },
            CityId: { required: resources.required_field },
            InsuranceCompanyIds: { required: resources.required_field },
            VatClassificationId: { required: resources.required_field },
            SupplierId: { required: resources.required_field }
        },
        submitHandler: function (form) {

            $("#btnCreate").prop("disabled", true);
            var formData = new FormData(form);
            formData.append('IsActive', $("#IsActive").prop('checked'));

            $.ajax({
                url: $(form).attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.isSuccess) {
                        Swal.fire({
                            icon: 'success',
                            title: resources.success_msg,
                            confirmButtonText: RazorVars.btnOk,
                            confirmButtonColor: 'var(--primary-600)',
                            timer: 3000,
                            timerProgressBar: true
                        }).then(() => {
                            window.location.href = RazorVars.indexUrl;
                        });
                    } else {
                        Swal.fire(RazorVars.swalErrorHappened, 'error');
                        $("#btnCreate").prop("disabled", false);
                    }
                },
                error: function () {
                    Swal.fire(RazorVars.swalErrorHappened, 'error');
                    $("#btnCreate").prop("disabled", false);
                }
            });

            return false;
        }
    });

    $("#workshopform span[data-valmsg-for]").html("&nbsp;");
});

// Search and filter
$(document).ready(function () {

    setPager();

    $("#btnClearWorkshop").click(function () {
        $('#Id').val("");
        $('#CityId').val('');
        $('#ParentId').val('');
        $('#PrimaryName').val("");
        $('#SecondaryName').val("");
        $('#Email').val("");
        $('#Phone').val("");
        $('#PrimaryAddress').val("");
        $('#SecondaryAddress').val("");

        $("#btnSearchWorkshop").click();
    });
});

$("#btnSearchWorkshop").click(function () {
    getWorkshopData();
});

$(document).keypress(function (event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == '13') {
        getWorkshopData();
    }
});

function getWorkshopData(page) {

    page = page || 1;

    var Id = $('#Id').val();
    var CityId = $('#CityId').val();
    var ParentId = $('#ParentId').val();
    var PrimaryName = $('#PrimaryName').val();
    var SecondaryName = $('#SecondaryName').val();
    var Email = $('#Email').val();
    var Phone = $('#Phone').val();
    var PrimaryAddress = $('#PrimaryAddress').val();
    var SecondaryAddress = $('#SecondaryAddress').val();

    var workshopForm = {
        Id: Id || null,
        CityId: CityId || null,
        ParentId: ParentId || null,
        PrimaryName: PrimaryName || null,
        SecondaryName: SecondaryName || null,
        Email: Email || null,
        Phone: Phone || null,
        PrimaryAddress: PrimaryAddress || null,
        SecondaryAddress: SecondaryAddress || null,
        Page: page
    }

    $.ajax({
        type: 'GET',
        url: RazorVars.indexUrl,
        contentType: 'application/json',
        dataType: 'html',
        data: workshopForm,
        cache: false,
    }).done(function (result) {
        $("#contentList").html(result);
        setPager(page);
    });
}

function setPager(page) {
    page = page || 1;
    if ($("#total_pages").val() != undefined) {
        $("#contentPager").attr("hidden", false);
        $('#contentPager').pagination('destroy');
        $('#contentPager').pagination({
            items: $("#total_pages").val(),
            itemOnPage: 25,
            currentPage: page,
            prevText: '&laquo;',
            nextText: '&raquo;',
            onPageClick: function (page, evt) {
                getWorkshopData(page);
            }
        });

        $('#contentPager ul').addClass('pagination');
        $('#contentPager li').addClass('page-item');
        $('#contentPager a, #contentPager span').addClass('page-link');
    }
    else {
        $("#contentPager").attr("hidden", true);
    }
}
