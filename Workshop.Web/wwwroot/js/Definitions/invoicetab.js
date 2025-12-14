function CreditNote(WIPId) {
    debugger
    var model = {
        'WIPId': WIPId
    };

    $.ajax({
        type: 'POST',
        url: window.URLs.addCreditNote,
        dataType: 'json',
        data: model,
        success: function (result) {
            window.location = window.URLs.indexUrl;
        }
    });
}
