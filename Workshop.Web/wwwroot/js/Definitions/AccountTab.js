$(document).ready(function () {

    // ===== Clear VAT/Currency/Terms when AccountType goes 2 -> 1 =====
    let prevAccType = String($("#AccountType").val() || "");
    let prevPAccType = String($("#PartialAccountType").val() || "");
    let isInitialLoad = true;

    function clearMainVatCurrencyTerms() {
        $("#CurrencyId").val("").trigger("change.select2");
        $("#Vat").val("").trigger("change.select2");
        $("#TermsId").val("").trigger("change.select2");

    }

    function clearPartialVatCurrencyTerms() {
        $("#PartialCurrencyId").val("").trigger("change.select2");
        $("#PartialVat").val("").trigger("change.select2");
        $("#PartialTermsId").val("").trigger("change.select2");
    }

    $("#AccountType")
        .off("change.clearfields")
        .on("change.clearfields", function () {
            const now = String($(this).val() || "");
            if (!isInitialLoad && prevAccType === "2" && now === "1") {
                clearMainVatCurrencyTerms();
            }
            prevAccType = now;
        });

    $("#PartialAccountType")
        .off("change.clearfields")
        .on("change.clearfields", function () {
            const now = String($(this).val() || "");
            if (!isInitialLoad && prevPAccType === "2" && now === "1") {
                clearPartialVatCurrencyTerms();
            }
            prevPAccType = now;
        });


    $(function () {
        var $cb = $("#optPartialInv");
        var $acc = $("#AccountType");
        var $pacc = $("#PartialAccountType");

        function inverse(val) {
            return (String(val) === "1") ? "2" : "1";
        }

        function applyByCheckbox() {
            var base = $("#AccountType").val();
            var partial = $("#optPartialInv").is(":checked") ? inverse(base) : base;

            if ($("#PartialAccountType").val() !== partial) {
                $("#PartialAccountType").val(partial).trigger("change");
            } else {
                syncCustomerDisable();
            }
        }

        $("#optPartialInv").on("change", applyByCheckbox);
        $("#AccountType").on("change", applyByCheckbox);

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

            syncCustomerDisable();
        });

        applyByCheckbox();
    });

 

    $("#CustomerId").change(function () {

        if (String($("#AccountType").val()) === "1") return;

        var Id = $("#CustomerId").val();
        if (!Id) return;

        $.ajax({
            type: 'GET',
            url: window.appUrls.getCustomerById,
            dataType: 'json',
            data: { Id: Id }
        }).done(function (result) {
            if (result) {
                debugger
                $("#CurrencyId").val(result.currencyId).trigger("change.select2");
                $("#Vat").val(result.salesTaxGroupId).trigger("change.select2");
                $("#TermsId").val(result.oLDBPaymentType).trigger("change.select2");
                $("#_DiscountPercentageLabor").val(result.discountPercentageLabor || 0);
                $("#_DiscountPercentagePart").val(result.discountPercentagePart || 0);
                $("#_DiscountPercentageLaborText").text("%" + (result.discountPercentageLabor || 0));
                $("#_DiscountPercentagePartText").text("%" + (result.discountPercentagePart || 0));
                debugger
            }
        }).fail(function (xhr, status, error) {
            console.error("Error:", error);
        });
    });

    $("#PartialCustomerId").on("change", function () {

        if (String($("#PartialAccountType").val()) === "1") return;

        var id = $("#PartialCustomerId").val();
        if (!id) return;

        $.ajax({
            type: 'GET',
            url: window.appUrls.getCustomerById,
            dataType: 'json',
            data: { Id: id }
        }).done(function (result) {
            if (!result) return;

            $("#PartialCurrencyId").val(result.currencyId).trigger("change.select2");
            $("#PartialVat").val(result.salesTaxGroupId).trigger("change.select2");
            $("#PartialTermsId").val(result.oLDBPaymentType).trigger("change.select2");
            $("#_DiscountPercentageLabor").val(result.discountPercentageLabor || 0);
            $("#_DiscountPercentagePart").val(result.discountPercentagePart || 0);
            $("#_DiscountPercentageLaborText").text("%" + (result.discountPercentageLabor || 0));
            $("#_DiscountPercentagePartText").text("%" + (result.discountPercentagePart || 0));
        }).fail(function (xhr, status, error) {
            console.error("Error:", error);
        });
    });




    var Id = $("#CustomerId").val();
    var vat = $("#Vat").val();

    if (!Id) {
        Id = $("#PartialCustomerId").val();
        vat = $("#PartialVat").val();
    }

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


    function loadSalesTypes(accountId, $ddl) {
        const prev = $ddl.val();  

        return $.ajax({
            type: 'GET',
            url: window.appUrls.getSalesType,
            dataType: 'json',
            data: { accountId: accountId }
        }).done(function (result) {
            $ddl.empty().append($('<option>', { value: "", text: "Select" }));
            $.each(result || [], function (i, item) {
                $ddl.append($('<option>', { value: item.value, text: item.text }));
            });

            if (prev) $ddl.val(prev);
        });
    }


    $("#AccountType").on("change", function () {
        const t = $(this).val();
        loadSalesTypes(t, $("#SalesType"));
    });

    $("#PartialAccountType").on("change", function () {
        const t = $(this).val();
        loadSalesTypes(t, $("#PartialSalesType"));
        syncCustomerDisable();
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

    let firstLoad = true;

    function syncCustomerDisable() {
        const acc = String($("#AccountType").val());
        const pacc = String($("#PartialAccountType").val());

        $("#CustomerId").prop("disabled", acc === "1");
        $("#PartialCustomerId").prop("disabled", pacc === "1");

        if (!firstLoad) {
            if (acc === "1") $("#CustomerId").val("").trigger("change.select2");
            if (pacc === "1") $("#PartialCustomerId").val("").trigger("change");
        }
    }

    $(function () {
        syncCustomerDisable();
        firstLoad = false;
        isInitialLoad = false;
    });


});