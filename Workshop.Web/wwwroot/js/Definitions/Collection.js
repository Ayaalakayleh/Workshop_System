
    $("#searchMaintenance").click(function () {
        getData(1);
    });
        
function getData(page) {

        var ExternalWorkshopId = $('#ExternalWorkshopId').val();
        var FromDate = $('#FromDate').val();
        var ToDate = $('#ToDate').val();
         MaintenanceFilter = {
             'ExternalWorkshopId': ExternalWorkshopId, 'Page': page, 'ToDate': ToDate, 'FromDate': FromDate
        };
       
        $.ajax({
            type: 'GET',
            url: RazorVars.indexUrl,
            contentType: 'application/json',
            dataType: 'html',
            data: MaintenanceFilter
        }).done(function (result) {
           
            $("#contentList").html(result);
           
            if ($("#total_pages").val() != undefined) {
                $("#contentPager").attr("hidden", false);
                $('#contentPager').pagination({
                    
                    items: $("#total_pages").val(),
                    itemOnPage: 25,
                    currentPage: page,
                    cssStyle: 'light-theme',
                    onInit: function () {
                    },
                    onPageClick: function (page, evt) {
                        getData(page);
                    }
                });
            }
            else {
                $("#contentPager").attr("hidden", true);
            }
        });
    }