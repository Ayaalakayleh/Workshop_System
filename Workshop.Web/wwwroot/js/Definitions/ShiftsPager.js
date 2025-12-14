$(function () {
    var $table = $('#data');
    var $tbody = $table.find('tbody');
    var rowsShown = 25;
    var rowsTotal = $tbody.find('tr').length;
    if (!rowsTotal) return;

    var numPages = Math.ceil(rowsTotal / rowsShown);
    var currentPage = 0;

    // Wrap pagination inside a flexbox container aligned to the end
    var $wrapper = $('<div class="d-flex justify-content-end mt-2"></div>');
    var $nav = $('<ul id="nav" class="pagination pagination-sm mb-0"></ul>');
    $wrapper.append($nav);
    $table.after($wrapper);

    function renderRows(pageIdx) {
        var startItem = pageIdx * rowsShown;
        var endItem = startItem + rowsShown;

        $tbody.find('tr')
            .stop(true, true).css('opacity', 0).hide()
            .slice(startItem, endItem)
            .css('display', 'table-row')
            .animate({ opacity: 1 }, 300);
    }

    function renderPagination() {
        $nav.empty();

        // Prev
        if (currentPage === 0) {
            $nav.append(
                '<li class="page-item disabled">' +
                '<span class="page-link" aria-label="Previous">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-left"></i></span>' +
                '</span>' +
                '</li>'
            );
        } else {
            $nav.append(
                '<li class="page-item">' +
                '<a class="page-link" href="#" aria-label="Previous" data-prev="1">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-left"></i></span>' +
                '</a>' +
                '</li>'
            );
        }

        // Page numbers
        for (var i = 0; i < numPages; i++) {
            if (i === currentPage) {
                $nav.append(
                    '<li class="page-item active" aria-current="page">' +
                    '<span class="page-link">' + (i + 1) +
                    '<span class="visually-hidden">(current)</span>' +
                    '</span>' +
                    '</li>'
                );
            } else {
                $nav.append(
                    '<li class="page-item">' +
                    '<a class="page-link" href="#" data-page="' + i + '">' + (i + 1) + '</a>' +
                    '</li>'
                );
            }
        }

        // Next
        if (currentPage === numPages - 1) {
            $nav.append(
                '<li class="page-item disabled">' +
                '<span class="page-link" aria-label="Next">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-right"></i></span>' +
                '</span>' +
                '</li>'
            );
        } else {
            $nav.append(
                '<li class="page-item">' +
                '<a class="page-link" href="#" aria-label="Next" data-next="1">' +
                '<span aria-hidden="true"><i class="sa sa-chevron-right"></i></span>' +
                '</a>' +
                '</li>'
            );
        }
    }

    function goToPage(idx) {
        if (idx < 0 || idx >= numPages) return;
        currentPage = idx;
        renderRows(currentPage);
        renderPagination();
    }

    // First render
    renderRows(0);
    renderPagination();

    // Events (delegated)
    $nav.on('click', 'a.page-link[data-page]', function (e) {
        e.preventDefault();
        goToPage(parseInt($(this).attr('data-page'), 10));
    });

    $nav.on('click', 'a.page-link[data-prev]', function (e) {
        e.preventDefault();
        goToPage(currentPage - 1);
    });

    $nav.on('click', 'a.page-link[data-next]', function (e) {
        e.preventDefault();
        goToPage(currentPage + 1);
    });
});
