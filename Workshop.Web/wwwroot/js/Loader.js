$(document).ajaxStart(function () {
    StartLoading();
});

$(document).ajaxComplete(function () {
    EndLoading();
});


function StartLoading() {
    $(".the-accouting-loader").css("display", "flex");
}

function EndLoading() {
    $(".the-accouting-loader").css("display", "none");
}
