$(document).ready(function () {
    


});
// End Ready =============================================
function getData(page) {
    page = page || 1;
    var FilterModel = {
        'Name': $('#_Name').val(),
        'PageNumber': page
    };
    debugger;
    $.ajax({
        type: 'GET',
        url: RazorVars.indexUrl,
        dataType: 'html',
        data: FilterModel
    }).done(function (result) {
        debugger;
        $('#tbl_labourRate_body').html(result);
    });

}
$("#btnSearch").click(function () {
    getData();
});
//========================================================
$("#cancelEditing").click(function () {
    window.location = RazorVars.indexUrl + '?Id=' + Model.Id;
});
$("#SubmitEditLookup").click(function () {
    var Id = $('#Id').val();
    var isValid = true;
    $('#' + Id + '').each(function () {

        //if ($(this).find('input[name ="LabourCode"]').val() == "") {
        //    $(this).find("td:eq(1) > input").addClass("invalid");
        //    isValid = false;
        //}
        //else if ($(this).find('input[name ="LabourCode"]').val().length > 3) {
        //    $(this).find("td:eq(1) > input").addClass("invalid");
        //    $(this).find("td:eq(1) > span").addClass("invalid").html("<b>" + "InvalidLength" + " 3 </b>");
        //    isValid = false;
        //}
        //else {
        //    $(this).find("td:eq(1) > input").removeClass("invalid");
        //    $(this).find("td:eq(1) > span").removeClass("invalid").html("");
        //}

        //if ($(this).find('input[name ="DescriptionEn"]').val() == "") {
        //    $(this).find("td:eq(2) > input").addClass("invalid");
        //    isValid = false;
        //}
        //else if ($(this).find('input[name ="DescriptionEn"]').val().length > 250) {
        //    $(this).find("td:eq(2) > input").addClass("invalid");
        //    $(this).find("td:eq(2) > span").addClass("invalid").html("<b>" + "InvalidLength" + " 250 </b>");
        //    isValid = false;
        //}
        //else {

        //    $(this).find("td:eq(2) > input").removeClass("invalid");
        //    $(this).find("td:eq(2) > span").removeClass("invalid").html("");
        
        //}
        //if ($(this).find('input[name ="DescriptionAr"]').val().length >= 250) {
        //    $(this).find("td:eq(3) > input").removeClass("invalid");
        //    $(this).find("td:eq(3) > span").addClass("invalid").html("<b>" + "InvalidLength" + " 250 </b>");
        //    isValid = false;
        //}
        
    });

    if (isValid) {
        debugger;
        var LabourCode = $('#LabourCode').val();
        var DescriptionEn = $('#DescriptionEn').val();
        var DescriptionAr = $('#DescriptionAr').val();
        debugger;
        var model = {
            'Id': Id,
            'LabourCode': LabourCode,
            'DescriptionEn': DescriptionEn,
            'DescriptionAr': DescriptionAr
        };

        $.ajax({
            type: 'POST',
            url: RazorVars.editUrl,
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify(model),
            success: function (result) {
                debugger
                window.location = RazorVars.indexUrl;
            }
        });
    }
});

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
                        Swal.fire(
                            RazorVars.swalDeleted,
                            'success'
                        ).then(() => {
                            window.location.href = RazorVars.indexUrl;
                        });
                    } else {
                        Swal.fire(
                            RazorVars.swalErrorHappened,
                            'error'
                        );
                    }
                },
                error: function () {
                    Swal.fire(
                        RazorVars.swalErrorHappened,
                        'error'
                    );
                }
            });
        } else if (result.dismiss === Swal.DismissReason.cancel) {
            Swal.fire(RazorVars.swalCancelled, '', 'info');
        }
    });
}

var count = 0;
$(".add-record").on('click', function (e) {
    e.preventDefault();
    debugger;
    $(".editLKP").addClass("disable-click");
    var content = $('#sample_table tr'),
        size = $('#tbl_posts >tbody >tr').length,
        element = null,
        element = content.clone();
    element.attr('id', 'rec-' + size);
    element.find('.delete-record').attr('data-id', size);
    element.find('.sn').html(size);
    $("#tbl_labourRate_body").append('<tr id="" class="newLookup"> \
<td class="d-none"><span class="sn"></span></td>\
<td><input type="text" name="LabourCode" class="form-control" maxlength="3"/><span></span></td>\
<td><input type="text" name="DescriptionEn" class="form-control" maxlength="250"/></td>\
<td><input type="text" name="DescriptionAr" class="form-control" maxlength="250"/></td>\
<td> \
  <button type="submit" class="btn btn-primary btn-sm submitLookup" onclick="return validateLookupForm();" hidden> \
    <i class="fa fa-save"></i>&nbsp; Save \
  </button> \
  <button type="button" class="btn btn-secondary text-white btn-sm delete-record" data-id="0">\
    <i class="fa fa-times"></i>&nbsp;Cancel\
  </button> \
</td></tr>');

    count += 1;

    $(".submitLookup").removeAttr('hidden');
    $(".submitLookup").focus();
    $("#hidSearchCategoryFilter").val(0);
    $(".addNewLKP").attr("hidden", false);
});

function validateLookupForm() {
    var isValid = true;
    //$('#tbl_posts > tbody > tr.newLookup ').each(function () {

    //    var LabourCode = $(this).find('input[name="LabourCode"]');
    //    var DescriptionEn = $(this).find('input[name="DescriptionEn"]');
    //    var DescriptionAr = $(this).find('input[name="DescriptionAr"]');

    //    // PrimaryName validation
    //    if (LabourCode.val() === "") {
    //        LabourCode.addClass("invalid");
    //        isValid = false;
    //    }
    //    else if (LabourCode.val().length > 3) {
    //        LabourCode.addClass("invalid");
    //        $(this).find("td:eq(1) > span").addClass("invalid").html("<b>InvalidLength 3</b>");
    //        isValid = false;
    //    }
    //    else {
    //        LabourCode.removeClass("invalid");
    //        $(this).find("td:eq(1) > span").removeClass("invalid").html("");
    //    }

    //    // SecondaryName validation
    //    if (DescriptionEn.val() === "") {
    //        DescriptionEn.addClass("invalid");
    //        isValid = false;
    //    }
    //    else if (DescriptionEn.val().length > 250) {
    //        DescriptionEn.addClass("invalid");
    //        $(this).find("td:eq(2) > span").addClass("invalid").html("<b>InvalidLength 250</b>");
    //        isValid = false;
    //    }
    //    else {
    //        DescriptionEn.removeClass("invalid");
    //        $(this).find("td:eq(2) > span").removeClass("invalid").html("");
    //    }

    //    // StatusValue validation
    //    if (DescriptionAr.val().length > 10) {
    //        $(this).find("td:eq(3) > span").addClass("invalid").html("<b>InvalidLength 250</b>");
    //        isValid = false;
    //    }
    //    else {
    //        $(this).find("td:eq(3) > span").removeClass("invalid").html("");
    //    }
    //});

    //if (isValid) {
    //    $("#hidSearchCategoryFilter").val(1);
    //    return true;
    //}
    //else {
    //    return false;
    //}
}

function Editlookup(Id) {
    $(".submitLookup").attr("hidden", true);
    $(".addNewLKP").attr("hidden", true);
    $(".editLKP").addClass('disable-click');
    $('#' + Id).load(lookupFindUrl + '?Id=' + Id);
}
$(document).delegate('a.delete-record', 'click', function (e) {
    e.preventDefault();
    $(this).parent().parent().remove();
    var newLKP = $('.newLookup').length;
    if (newLKP == 0) {
        $(".submitLookup").attr("hidden", true);
        $(".editLKP").removeClass("disable-click");
    }
    $(".addNewLKP").attr("hidden", false);

    return true;
});
$(document).on('click', '.addNewLKP', function () {
    $(".addNewLKP").attr("hidden", true);
});
$(document).on('click', '.delete-record', function () {
    window.location.reload();
});