function CreditNote(WIPId) {
    debugger
    var model = {
        'WIPId': WIPId
    };

    Swal.fire({
        title: resources.areYouSure,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: 'var(--danger-600)',
        cancelButtonColor: 'var(--secondary-500)',
        confirmButtonText: resources.yes,
        cancelButtonText: resources.no
    }).then((result) => {
        if (result.isConfirmed) {
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
    });

}
