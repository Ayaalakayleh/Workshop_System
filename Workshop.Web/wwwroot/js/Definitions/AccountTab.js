$(document).ready(function () {
    $("#AccountType").change(function () {
        const selectedType = $(this).val();
        const match = $("#SalesType");

        if (selectedType == 1) {
            $("#CustomerId").val("").trigger("change");
            $("#CustomerId").attr("disabled", true);
        } else {
            $("#CustomerId").attr("disabled", false);
            $("#CurrencyId").attr("disabled", true);
            $("#Vat").attr("disabled", true);
            $("#TermsId").attr("disabled", true);
        }

        $.ajax({
            type: 'GET',
            url: window.appUrls.getSalesType,
            dataType: 'json',
            data: { accountId: selectedType }
        }).done(function (result) {
            if (result) {
                match.empty();
                match.append($('<option>', { value: "", text: "Select" }));
                $.each(result, function (i, item) {
                    match.append($('<option>', {
                        value: item.value,
                        text: item.text
                    }));
                });
            }
        }).fail(function (xhr, status, error) {
            console.error("Error:", error);
        });
    });


    $("#optPartialInv").change(function () {
        var isPartial = $("#optPartialInv").is(":checked");
        if (isPartial === true || isPartial === "true") {
            const _selectedType = $("#PartialAccountType").val();
            const realType = _selectedType == "1" ? "2" : "1";
            const _match = $("#PartialSalesType");
            $.ajax({
                type: 'GET',
                url: window.appUrls.getSalesType,
                dataType: 'json',
                data: { accountId: realType }
            }).done(function (result) {
                if (result) {
                    _match.empty();
                    _match.append($('<option>', { value: "", text: "Select" }));
                    $.each(result, function (i, item) {
                        _match.append($('<option>', {
                            value: item.value,
                            text: item.text
                        }));
                    });
                }
            }).fail(function (xhr, status, error) {
                console.error("Error:", error);
            });
        }
    });

    $(function () {
        var $cb = $("#optPartialInv");
        var $acc = $("#AccountType");
        var $pacc = $("#PartialAccountType");

        function inverse(val) {
            return (String(val) === "1") ? "2" : "1";
        }

        function applyByCheckbox() {
            if ($cb.is(":checked")) {
                var inv = inverse($acc.val()); 
                $pacc.val(inv).trigger("change");
            } else {
                $pacc.val($acc.val()).trigger("change");
            }
        }

        $cb.on("change", function () {
            applyByCheckbox();
        });

        $acc.on("change", function () {
            if ($cb.is(":checked")) {
                var inv = inverse($acc.val());
                if ($pacc.val() !== inv) {
                    //$pacc.val(inv).trigger("change");
                    $pacc.val(inv).change();
                }
            }
        });

        $pacc.on("change", function () {
            if ($cb.is(":checked")) {
                var inv = inverse($pacc.val());
                if ($acc.val() !== inv) {
                    $acc.val(inv).trigger("change");
                }
            }

            if ($(this).val() == 1) {
                $("#CustomerId").attr("disabled", true);
            }
        });

        applyByCheckbox();
    });

    var accountType_base = $("#AccountType").val();
    if (accountType_base == 1) {
        $("#CustomerId").attr("disabled", true);
    } else {
        $("#CustomerId").attr("disabled", false);
    }


    var accountType_Partial = $("#PartialAccountType").val();
    if (accountType_Partial == 1) {
        $("#PartialCustomerId").attr("disabled", true);
    } else {
        $("#PartialCustomerId").attr("disabled", false);
    }

    $("#CustomerId").change(function () {
        var Id = $("#CustomerId").val();

        $.ajax({
            type: 'GET',
            url: window.appUrls.getCustomerById,
            dataType: 'json',
            data: {Id: Id}
        }).done(function (result) {
            if (result) {
                debugger

                $("#CurrencyId").val(result.currencyId).trigger("change.select2");
                $("#Vat").val(result.salesTaxGroupId).trigger("change.select2");
                $("#TermsId").val(result.oLDBPaymentType).trigger("change.select2");
                

            }
        }).fail(function (xhr, status, error) {
            console.error("Error:", error);
        });
    });



    var Id = $("#CustomerId").val();
    var vat = $("#Vat").val();

    $.ajax({
        type: 'GET',
        url: window.appUrls.getCustomerById,
        dataType: 'json',
        data: { Id: Id }
    }).done(function (result) {
        if (result) {
            var labour = result.discountPercentageLabor;
            var parts = result.discountPercentagePart;
            $("#_DiscountPercentageLabor").val(labour);
            $("#_DiscountPercentagePart").val(parts);
            $("#_DiscountPercentageLaborText").text("%" + labour);
            $("#_DiscountPercentagePartText").text("%" + parts);
      
            $("#VatPercentage").text("%" + parseInt(GetVatValueById(vat)));
            $("#mainItemsGrid").dxDataGrid("instance").refresh();
            $("#mainRTSGrid").dxDataGrid("instance").refresh();
        }
    }).fail(function (xhr, status, error) {
        console.error("Error:", error);
    });


    function GetVatValueById(vatId) {
        var vatValue = 0;
        $.ajax({
            url: window.appUrls.getVatValueById,
            method: 'Get',
            dataType: 'json',
            data: { VatId: vatId },
            async: false,
            success: function (result) {
                vatValue = result;
            },
            error: function () {
                vatValue = 0;
            }
        });
        return vatValue;
    }

    $("#Vat").change(function () {
        var vat = $("#Vat").val();
        var VatAmount = GetVatValueById(vat);
        $("#VatPercentage").text("%" + parseInt(VatAmount));
    })
});