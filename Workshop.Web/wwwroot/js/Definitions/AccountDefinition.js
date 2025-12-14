$('#AccountDefinitionForm').on('submit', function (e) {
    e.preventDefault();

    $.ajax({
        url: this.action,
        type: this.method,
        data: $(this).serialize(),
        success: function (response) {
            Swal.fire({
                icon: 'success',
                title: 'Saved successfully!',
                showConfirmButton: false,
                timer: 2000
            });
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'An error occurred while saving.',
                showConfirmButton: true
            });
        }
    });
});

//$(document).ready(function () {

//    $("#AccountDefinitionForm select, #AccountDefinitionForm input").prop("disabled", true);

//    $("#btnEdit").on("click", function () {
//        $("#AccountDefinitionForm select, #AccountDefinitionForm input").prop("disabled", false);

//        $("#btnEdit").addClass("d-none");
//        $("#btnCreate").removeClass("d-none");
//    });

//    $("#AccountDefinitionForm").on("submit", function () {
//        $("#AccountDefinitionForm select, #AccountDefinitionForm input").prop("disabled", true);
//    });

//});

$(document).ready(function () {

    $("#AccountDefinitionForm select, #AccountDefinitionForm input").prop("disabled", true);

    $("#btnEdit").on("click", function () {
        $("#AccountDefinitionForm select, #AccountDefinitionForm input").prop("disabled", false);

        $("#btnEdit").addClass("d-none");
        $("#btnCreate").removeClass("d-none");
    });

    $("#AccountDefinitionForm").on("submit", function () {

        $("#AccountDefinitionForm select, #AccountDefinitionForm input").prop("disabled", true);

        setTimeout(function () {
            location.reload();
        }, 500);
    });

});


