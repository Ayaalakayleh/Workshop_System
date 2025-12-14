var pond = [];
$(document).ready(function () {
	$(".back").hide();
	var table = $('#payment-method-table').DataTable({
		responsive: true,
		orderCellsTop: false,
		fixedHeader: true,
		info: false,
		ordering: false,
		scrollY: "400px",
		scrollCollapse: true,
		paging: false,
		language: {
			searchPlaceholder: RazorVars.searchButtonMessage,
			lengthMenu: RazorVars.displayMessage,
			emptyTable: RazorVars.emptyTable
		}
	});

	if (RazorVars.modelCount > 0) {

		$("#ExternalWorkshopId").attr('readonly', true);
		$("#btnSave").attr("hidden", false);
		//$("#Month").prop("readonly", true);
	}

	$(function () {
		var arr = document.querySelectorAll('.Uploadfile');
		Array.from(arr).forEach(inputElement => {
			var id = inputElement.id;
			pond[id] = FilePond.create(inputElement, { acceptedFileTypes: ['image/png', 'image/jpeg', 'image/PNG', 'application/pdf'], });
		});

	});

	$(function () {
		$.ajax({
			type: 'GET',
			url: RazorVars.getBranchsURL,
			contentType: 'application/json',
			dataType: 'json',
			success: function (data) {
				var elements = $("[id^='BranchID_']");

				elements.each(function (index, element) {

					var $el = $(element);
					$el.empty();

					$.each(data, function (i, value) {
						$el.append(
							'<option value="' + value.id + '">' + value.text + '</option>'
						);
					});

				});
			}
		});
	});
});

$(".checkAll").change(function () {
	if ($('.checkAll').prop('checked') == true) {
		$('.Selected').prop('checked', true);
	} else {
		$('.Selected').prop('checked', false);
	}
});
function ChangeVat(tabindex) {
	var LaborCost = $('#LaborCost_' + tabindex).val();
	var PartsCost = $('#PartsCost_' + tabindex).val();
	var VatRate = $('#VatRate').val();
	var VatValue = (parseFloat(LaborCost) + parseFloat(PartsCost)) * (parseFloat(VatRate) / 100);
	//var VatValue = (parseFloat(PartsCost)) * (parseFloat(VatRate) / 100);
	var Total = (parseFloat(LaborCost) + parseFloat(PartsCost) + VatValue);
	$('#Vat_' + tabindex).val(VatValue);
	$('#TotalInvoice_' + tabindex).val(Total);
}
function Save() {
    let formdata = new FormData();
    let count = 0;
    let isValid = true;

    $('#payment-method-table').find("input.checkbox.Selected:checked").each(function () {
        const $checkbox = $(this);
        const movementId = $checkbox.data('movementid');
        const workOrderId = $checkbox.data('damegeid');
        const pondInstance = pond[movementId];

        const pondFile = pondInstance?.getFile();
        const file = pondFile?.file || null;
        const invoiceNo = $(`#InvoiceNo_${movementId}`).val();
        const laborCost = parseFloat($(`#LaborCost_${movementId}`).val()) || 0;
        const partsCost = parseFloat($(`#PartsCost_${movementId}`).val()) || 0;
        const vat = parseFloat($(`#Vat_${movementId}`).val()) || 0;
        const totalInvoice = parseFloat($(`#TotalInvoice_${movementId}`).val()) || 0;

        if (!invoiceNo || laborCost === 0 || partsCost === 0 || totalInvoice === 0 || !file) {
            Swal.fire(RazorVars.enterAllDataMessage, "", "warning");
            isValid = false;
            return false;
        }

        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].MovementId`, movementId);
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].WorkOrderId`, workOrderId);
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].InvoiceNo`, invoiceNo);
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].LaborCost`, laborCost);
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].PartsCost`, partsCost);
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].Vat`, vat);
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].TotalInvoice`, totalInvoice);
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].VehicleId`, $(`#VehicleId_${movementId}`).val());
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].ExternalWorkshopId`, $("#ExternalWorkshopId").val());
        formdata.append(`ExternalWorkshopInvoiceDetails[${count}].BranchId`, $(`#BranchID_${movementId}`).val());

        formdata.append('files', file);

        count++;
    });

    if (!isValid || count === 0) return;

    formdata.append("ExternalWorkshopId", $("#ExternalWorkshopId").val());
    formdata.append("GregorianFromDate", $("#GregorianFromDate").val());
    formdata.append("GregorianToDate", $("#GregorianToDate").val());
    formdata.append("InvoiceTypeId", $("#InvoiceTypeId").val());

    $("#btnSave").attr("disabled", true);

    $.ajax({
        url: RazorVars.saveUrl,
        type: 'POST',
        data: formdata,
        dataType: 'JSON',
        cache: false,
        processData: false,
        contentType: false,
        complete: function () {
            $("#btnSave").attr("disabled", false);
        },
        success: function (result) {
            if (result.isSuccess) {
                Swal.fire({
                    title: RazorVars.doneMessage,
                    icon: "success",
                    confirmButtonText: "OK"
                }).then(() => window.location.reload());
            } else {
                Swal.fire({
                    title: RazorVars.errorMessage,
                    icon: "error"
                });
            }
        },
        error: function () {
            Swal.fire(RazorVars.errorMessage, "", "error");
        }
    });
}

