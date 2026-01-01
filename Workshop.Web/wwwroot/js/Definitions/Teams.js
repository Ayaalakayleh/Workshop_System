$(document).ready(function () {
    $("#teamForm").validate({
        rules: {
            Code: {
                required: true
            },
            Short: {
                required: true
            },
            PrimaryName: {
                required: true
            },
            SecondaryName: {
                required: false
            },
            Color: {
                required: true
            }

        },
        messages: {
            Code: {
                required: resources.required_field
            },
            Short: {
                required: resources.required_field
            },
            PrimaryName: {
                required: resources.required_field
            },
            SecondaryName: {
                required: resources.required_field
            },
            Color: {
                required: resources.required_field
            }
   
        },
        errorClass: "text-danger",
        errorElement: "span",
        highlight: function (element) {
            $(element).addClass("is-invalid");
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid");
        },
        submitHandler: function (form) {
            $("#btnCreate").prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>&nbsp;' + RazorVars.btnSaving);
            var formData = new FormData(form);

            $.ajax({
                url: $(form).attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    $("#btnCreate").prop('disabled', false).html('<i class="fa fa-save"></i>&nbsp;' + RazorVars.btnSave);
                    if (response === -1) {
                        Swal.fire({
                            icon: 'warning',
                            title: RazorVars.duplicateCode,
                            text: RazorVars.duplicateTeam,
                            confirmButtonText: RazorVars.btnOk,
                            confirmButtonColor: '#dc3545'
                        });
                        return;
                    }
                    Swal.fire({
                        icon: 'success',
                        title: resources.success_msg,
                        confirmButtonText: RazorVars.btnOk,
                        confirmButtonColor: 'var(--primary-600)',
                        timer: 3000,
                        timerProgressBar: true
                    }).then(() => { $('#teamModal').modal('hide'); location.reload(); });
                },
                error: function (xhr) {
                    $("#btnCreate").prop('disabled', false).html('<i class="fa fa-save"></i>&nbsp;' + RazorVars.btnSave);
                    var errorMessage = "";
                    Swal.fire({
                        icon: 'error',
                        title: RazorVars.ErrorHappend,
                        confirmButtonText: RazorVars.btnTryAgain,
                        confirmButtonColor: '#dc3545'
                    });
                }
            });
            return false;
        }
    });
});
// Server-side pagination
var currentPage = 1;
var totalPages = 1;
var currentFilter = {
    name: '',
    code: '',
    pageNumber: 1,
    pageSize: 25
};

$(function () {
    // Initialize pagination from server-rendered data using simplePagination plugin
    initializePagination();

    // Update search form to reset to page 1
    $('#searchForm').on('submit', function (e) {
        currentFilter.name = $('#_Name').val() || '';
        currentFilter.code = $('#_Code').val() || '';
        currentFilter.pageNumber = 1;
        // Let form submit normally for initial search
    });
});

function initializePagination() {
    var $paginationData = $('#paginationData');
    if ($paginationData.length > 0) {
        var attrTotalPages = $paginationData.data('total-pages');
        var attrTotalRecords = $paginationData.data('total-records');
        var pageSizeValue = parseInt($paginationData.data('page-size')) || currentFilter.pageSize || 25;

        // Prefer explicit total-records; fallback to total-pages when total-records is not provided.
        var totalRecordsValue = (typeof attrTotalRecords !== 'undefined' && attrTotalRecords !== null)
            ? parseInt(attrTotalRecords, 10)
            : (typeof attrTotalPages !== 'undefined' && attrTotalPages !== null
                ? parseInt(attrTotalPages, 10)
                : 0);

        totalRecordsValue = isNaN(totalRecordsValue) ? 0 : totalRecordsValue;
        totalPages = Math.ceil((totalRecordsValue || 0) / (pageSizeValue || 1));
        currentPage = parseInt($paginationData.data('current-page')) || 1;

        updatePaginationWrapper(currentPage, totalPages, totalRecordsValue, pageSizeValue);
        setPager(currentPage, totalPages, totalRecordsValue, pageSizeValue);
    } else {
        $("#contentPager").attr("hidden", "hidden");
    }
}

function setPager(page, totalPagesValue, totalRecordsValue, pageSizeValue) {
    page = page || 1;
    var $pager = $("#contentPager");

    totalPagesValue = totalPagesValue || totalPages || 1;
    totalRecordsValue = (typeof totalRecordsValue !== 'undefined') ? totalRecordsValue : (totalPagesValue * (pageSizeValue || currentFilter.pageSize));
    pageSizeValue = pageSizeValue || currentFilter.pageSize || 25;

    if (!totalRecordsValue || totalRecordsValue <= 0) {
        $pager.attr("hidden", "hidden");
        return;
    }

    $pager.removeAttr("hidden");
    if ($pager.data('pagination')) {
        $pager.pagination('destroy');
    }

    // Pass total records and page size to plugin (plugin computes pages = items / itemsOnPage)
    $pager.pagination({
        items: totalRecordsValue,
        itemsOnPage: pageSizeValue,
        currentPage: page,
        prevText: '&laquo;',
        nextText: '&raquo;',
        onPageClick: function (clickedPage, event) {
            if (clickedPage !== currentPage) {
                event.preventDefault();
                loadPage(clickedPage);
            }
        },
        hrefTextPrefix: 'javascript:void(0)'
    });

    $pager.find('ul').addClass('pagination');
    $pager.find('li').addClass('page-item');
    $pager.find('a, span').addClass('page-link');
}

function loadPage(pageNumber) {
    if (pageNumber < 1) pageNumber = 1;

    currentFilter.pageNumber = pageNumber;
    currentFilter.name = $('#_Name').val() || '';
    currentFilter.code = $('#_Code').val() || '';

    $('#contentList').html('<div class="text-center p-4"><i class="fa fa-spinner fa-spin fa-2x"></i><br/>' + (RazorVars.loadingText || 'Loading...') + '</div>');

    $.ajax({
        url: RazorVars.getTeamsPartialUrl,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(currentFilter),
        success: function (response) {
            $('#contentList').html(response);

            var $paginationData = $('#paginationData');
            if ($paginationData.length > 0) {
                var attrTotalPages = $paginationData.data('total-pages');
                var attrTotalRecords = $paginationData.data('total-records');
                var pageSizeValue = parseInt($paginationData.data('page-size')) || currentFilter.pageSize || 25;

                var totalRecordsValue = (typeof attrTotalRecords !== 'undefined' && attrTotalRecords !== null)
                    ? parseInt(attrTotalRecords, 10)
                    : (typeof attrTotalPages !== 'undefined' && attrTotalPages !== null
                        ? parseInt(attrTotalPages, 10)
                        : 0);

                totalRecordsValue = isNaN(totalRecordsValue) ? 0 : totalRecordsValue;

                totalPages = Math.max(1, Math.ceil((totalRecordsValue || 0) / (pageSizeValue || 1)));
                currentPage = parseInt($paginationData.data('current-page')) || pageNumber || 1;

                if (totalPages > 0 && currentPage > totalPages) {
                    loadPage(totalPages);
                    return;
                }

                updatePaginationWrapper(currentPage, totalPages, totalRecordsValue, pageSizeValue);
                setPager(currentPage, totalPages, totalRecordsValue, pageSizeValue);
            } else {
                $("#contentPager").attr("hidden", "hidden");
            }

            $('html, body').animate({
                scrollTop: $('#contentList').offset().top - 100
            }, 300);
        },
        error: function (xhr) {
            $('#contentList').html('<div class="alert alert-danger">' + (RazorVars.ErrorHappend || 'An error occurred') + '</div>');
            Swal.fire({
                icon: 'error',
                title: RazorVars.ErrorHappend || RazorVars.swalErrorHappened || 'An error occurred',
                confirmButtonText: RazorVars.btnOk || 'OK',
                confirmButtonColor: '#dc3545'
            });
        }
    });
}

function updatePaginationWrapper(currentPage, totalPages, totalRecords, pageSize) {
    var $wrapper = $('#paginationWrapper');
    if ($wrapper.length === 0) return;
    $wrapper.empty();
}

function loadTeamById(teamId) {
    if (!teamId) {
        console.error("Team ID is required");
        return;
    }

    $.ajax({
        url: RazorVars.getTeamByIdUrl,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(teamId),
        success: function (data) {
            if (data && Array.isArray(data) && data.length > 0) {
                var team = data[0];

                // Fill form fields
                $("#Id").val(team.id || "");
                $("#Code").val(team.code || "");
                $("#Description").val(team.primaryName || "");
                $("#SecondDescription").val(team.secondaryName || "");
                $("#Short").val(team.short || "");
                $("#Color").val(intToHexString(team.color || 0));

                // Set technicians - convert to array of strings if needed
                var technicians = team.technicians || [];
                if (Array.isArray(technicians) && technicians.length > 0) {
                    // Convert to strings for select values
                    var technicianIds = technicians.map(function (id) {
                        return String(id);
                    });

                    // Clear any previous selections first
                    $("#Technicians option:selected").prop("selected", false);

                    // Set the values
                    $("#Technicians").val(technicianIds);

                    // For multi-select, we need to explicitly set selected property
                    $("#Technicians option").each(function () {
                        var optionValue = String($(this).val());
                        if (technicianIds.includes(optionValue)) {
                            $(this).prop("selected", true);
                        } else {
                            $(this).prop("selected", false);
                        }
                    });

                    // Trigger change event for multi-select
                    $("#Technicians").trigger('change');

                    // If select2 is initialized, trigger select2 change
                    if ($("#Technicians").data('select2')) {
                        $("#Technicians").trigger('change.select2');
                    }
                } else {
                    // Clear technicians if none
                    $("#Technicians").val([]).trigger('change');
                    if ($("#Technicians").data('select2')) {
                        $("#Technicians").trigger('change.select2');
                    }
                }
            } else {
                console.warn("No team data found or invalid format:", data);
                Swal.fire({
                    icon: 'warning',
                    title: 'No Data',
                    text: 'Team data could not be loaded.',
                    confirmButtonText: RazorVars.btnOk || 'OK'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading team:", error, xhr);
            var errorMessage = "Failed to load team data.";
            if (xhr.responseJSON && xhr.responseJSON.error) {
                errorMessage = xhr.responseJSON.error;
            }
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: errorMessage,
                confirmButtonText: RazorVars.btnOk || 'OK',
                confirmButtonColor: '#dc3545'
            });
        }
    });
}

function deleteTeam(teamId) {
    Swal.fire({
        title: RazorVars.swalDeleteTitle,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: 'var(--danger-600)',
        cancelButtonColor: 'var(--secondary-500)',
        confirmButtonText: RazorVars.swalDeleteConfirm,
        cancelButtonText: RazorVars.swalDeleteCancel
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: RazorVars.deleteTeamAjaxUrl || RazorVars.deleteUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(teamId),
                success: function (response) {
                    // Check if delete was successful (result > 0)
                    if (response && response.success && response.result > 0) {
                        Swal.fire({
                            icon: 'success',
                            title: RazorVars.swalDeleted,
                            confirmButtonText: RazorVars.btnOk,
                            confirmButtonColor: 'var(--primary-600)',
                            timer: 2000,
                            timerProgressBar: true
                        }).then(() => {
                            var targetPage = currentPage;
                            if (totalPages > 0 && currentPage > totalPages) {
                                targetPage = totalPages;
                            }
                            loadPage(targetPage > 0 ? targetPage : 1);
                        });
                    } else {
                        // Delete failed (result <= 0, typically -1)
                        var errorMessage = response && response.message 
                            ? response.message 
                            : 'Team could not be deleted. It may be in use or does not exist.';
                        Swal.fire({
                            icon: 'warning',
                            title: 'Delete Failed',
                            text: errorMessage,
                            confirmButtonText: RazorVars.btnOk || 'OK',
                            confirmButtonColor: '#dc3545'
                        });
                    }
                },
                error: function (xhr) {
                    var errorMessage = 'Failed to delete team.';
                    if (xhr.responseJSON && xhr.responseJSON.error) {
                        errorMessage = xhr.responseJSON.error;
                    }
                    Swal.fire({
                        icon: 'error',
                        title: RazorVars.ErrorHappend || 'Error',
                        text: errorMessage,
                        confirmButtonText: RazorVars.btnOk || 'OK',
                        confirmButtonColor: '#dc3545'
                    });
                }
            });
        }
    });
}

function intToHexString(colorInt) {
    const hex = (colorInt & 0xFFFFFF).toString(16);
    return "#" + hex.padStart(6, '0');
}
// Handle edit button click
$(document).on("click", "#editBtn", function (e) {
    e.preventDefault();
    var teamId = $(this).closest("tr").data("id");
    if (teamId) {
        loadTeamById(teamId);
    } else {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Could not find team ID.',
            confirmButtonText: RazorVars.btnOk || 'OK'
        });
    }
});
$(document).on("click", "#addBtn", function () {
    resetTeamForm();
});
$(document).on('hidden.bs.modal', '#teamModal', function () {
    resetTeamForm();
});
$(document).on("click", "#deleteBtn", function () {

    var teamId = $(this).data("user-id");
    deleteTeam(teamId);
});
$(document).on("click", "#btnReset", function (e) {
    e.preventDefault();
    // Clear search inputs
    $("#_Code").val('');
    $("#_Name").val('');
    
    // Reset filter and page number
    currentFilter.name = '';
    currentFilter.code = '';
    currentFilter.pageNumber = 1;
    
    // Reload data with cleared filters
    loadPage(1);
});




function resetTeamForm() {
    $("#Id").val("");
    $("#Code").val("");
    $("#Description").val("");
    $("#SecondDescription").val("");
    $("#Short").val("");
    $("#Color").val(intToHexString("000000"));
    $("#Technicians").val([]).trigger('change');
    if ($("#Technicians").data('select2')) {
        $("#Technicians").trigger('change.select2');
    }
}
