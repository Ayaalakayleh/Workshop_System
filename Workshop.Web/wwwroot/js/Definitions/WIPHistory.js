// في ملف wiphistory.js
var historyDataTable = null;
var historyLoaded = false;

document.addEventListener('DOMContentLoaded', function () {
    const tabHistoryBtn = document.getElementById('tabHistoryBTN');
    const vehicleId = $('#_vehicleId').val();//6134; 

    if (tabHistoryBtn && !historyLoaded) {
        tabHistoryBtn.addEventListener('click', function () {
            loadServiceHistory(vehicleId);
        });
    }

    $(window).on('beforeunload', function () {
        cleanupHistoryTable();
    });
});

function initializeHistoryTable() {
    const table = $('#tblHistory');

    if (!table.length) {
        console.log('Table not found for initialization');
        return;
    }

    if ($.fn.DataTable && $.fn.DataTable.isDataTable(table)) {
        console.log('DataTable already initialized');
        return;
    }

    if ($.fn.DataTable) {
        try {
            cleanupHistoryTable();

            historyDataTable = table.DataTable({
                responsive: true,
                orderCellsTop: false,
                fixedHeader: true,
                info: false,
                ordering: true,
                order: [[0, "desc"]],
                scrollY: "400px",
                scrollCollapse: true,
                paging: false,
                autoWidth: false,
                searching: false,
                language: {
                    emptyTable: "No data available",
                    search: "Search:",
                    zeroRecords: "No matching records found"
                },
                columnDefs: [
                    {
                        orderable: true,
                        targets: [0, 1, 3, 7]
                    },
                    {
                        orderable: false,
                        targets: '_all'
                    },
                    {
                        className: "text-center",
                        targets: [8]
                    },
                    {
                        className: "text-end",
                        targets: [7]
                    }
                ],
             
                "destroy": true,
                "retrieve": true
            });

            console.log('DataTable initialized successfully');
            reattachHistoryEvents();

        } catch (error) {
            console.error('Error initializing DataTable:', error);
            reattachHistoryEvents();
        }
    } else {
        reattachHistoryEvents();
    }
}

function reattachHistoryEvents() {
    $(document).off('click', '.btn-history-details').on('click', '.btn-history-details', function () {
        const wipId = $(this).data('wip-id');
        showWIPDetails(wipId);
    });
}

function showWIPDetails(wipId) {
    showLoadingInDetailsTab();

    //$.ajax({
    //    url: '/WIP/GetWIPDetails',
    //    type: 'GET',
    //    data: { wipId: wipId },
    //    success: function (response) {
    //        $('#tabHistoryDetails').html(response);
    //        const detailsTab = $('[data-bs-target="#tabHistoryDetails"]');
    //        if (detailsTab.length) {
    //            detailsTab.tab('show');
    //        }
    //    },
    //    error: function (xhr, status, error) {
    //        console.error('Error loading WIP details:', error);
    //        showErrorInDetailsTab('Failed to load WIP details');
    //    }
    //});
}

function showLoadingInDetailsTab() {
    $('#tabHistoryDetails').html(`
        <div class="text-center p-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading WIP details...</p>
        </div>
    `);
}

function showErrorInDetailsTab(message) {
    $('#tabHistoryDetails').html(`
        <div class="alert alert-danger m-3" role="alert">
            <i class="fas fa-exclamation-triangle me-2"></i> ${message}
        </div>
    `);
}

function showLoadingIndicator() {
    $('#tabHistory').html(`
        <div class="card">
            <div class="card-body">
                <div class="text-center p-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2">Loading service history...</p>
                </div>
            </div>
        </div>
    `);
}

function hideLoadingIndicator() {
}

function showError(message) {
    $('#tabHistory').html(`
        <div class="card">
            <div class="card-body">
                <div class="alert alert-danger" role="alert">
                    <i class="fas fa-exclamation-triangle me-2"></i> ${message}
                </div>
            </div>
        </div>
    `);
}

function loadServiceHistory(vehicleId) {
    showLoadingIndicator();

    $.ajax({
        url: window.RazorVars.getServiceHistoryUrl,
        type: 'GET',
        data: { vehicleId: vehicleId },
        success: function (response) {
            console.log(response);
            $('#tabHistory').html(response);

            setTimeout(function () {
                initializeHistoryTable();
                historyLoaded = true;
            }, 50);

            hideLoadingIndicator();
        },
        error: function (xhr, status, error) {
            console.error('Error loading service history:', error);
            showError('Failed to load service history');
            hideLoadingIndicator();
        }
    });
}

function cleanupHistoryTable() {
    const table = $('#tblHistory');

    if (historyDataTable !== null) {
        try {
            historyDataTable.destroy();
            historyDataTable = null;
        } catch (error) {
            console.warn('Error during DataTable cleanup:', error);
        }
    }

    if (table.length && $.fn.DataTable && $.fn.DataTable.isDataTable(table)) {
        table.DataTable().destroy();
    }

    table.removeClass('dataTable');
    $('.dataTables_wrapper').remove();
}

/* -------------------- Service history -> details tab -------------------- */
function ensureHistoryDetailsTab() {
    // add tab header if missing
    if (!document.querySelector('button[data-bs-target="#tabHistoryDetails"]')) {
        const li = document.createElement('li');
        li.className = 'nav-item';
        li.innerHTML =
            `<button class="nav-link" data-bs-toggle="tab" data-bs-target="#tabHistoryDetails" type="button">${resources.history_details_tab}</button>`;
        document.querySelector('#wipTabs').appendChild(li);
    }

    // fill panel once
    const panel = document.getElementById('tabHistoryDetails');
    if (!panel.dataset.filled) {
        const partsHeader = resources.title_parts;
        const labourHeader = resources.title_labour;
        const serviceHeader = resources.title_service_details;

        const thProduct = resources.label_product;
        const thDesc = resources.label_description;
        const thW = resources.label_w;
        const thQty = resources.label_quantity;
        const thPrice = resources.label_price;
        const thDiscPct = resources.label_discount_pct;
        const thContribPct = resources.label_contrib_pct;
        const thCost = resources.label_cost;

        const thRtsCode = resources.label_rts_code;
        const thTime = resources.label_time;
        const thRate = resources.label_rate;
        const thExtValue = resources.label_ext_value;
        const thTech = resources.label_technician;

        const posAccount = resources.label_point_of_sale_account;
        const afterBranch = resources.label_aftersales_branch;
        const afterExec = resources.label_aftersales_executive;
        const dispOdoAs = resources.label_display_odometer_as;
        const kmLabel = resources.label_kilometres;
        const lastService = resources.label_last_service_date;
        const nextService = resources.label_next_service_due;
        const lastWork = resources.label_date_of_last_work;
        const lastOdo = resources.label_last_known_odometer;
        const timingBelt = resources.label_timing_belt;
        const svcInterval = resources.label_service_interval;
        const months = resources.label_months;
        const orTxt = resources.label_or;
        const msCompany = resources.label_ms_company;

        const flags = (resources.service_flags || []).map((label, i) => `
          <div class="col-lg-3 col-md-6 col-sm-12">
            <div class="form-check form-switch">
              <input type="checkbox" class="form-check-input" id="svc${i + 1}">
              <label class="form-check-label" for="svc${i + 1}">${label}</label>
            </div>
          </div>`).join('');

        panel.innerHTML = `
          <div class="card mb-3">
            <div class="card-header"><h5 class="mb-0">${partsHeader}</h5></div>
            <div class="card-body p-0">
              <div class="table-responsive">
                <table class="table mb-0">
                  <thead>
                    <tr class="small text-muted">
                      <th>${thProduct}</th><th>${thDesc}</th><th>${thW}</th><th>${thQty}</th>
                      <th>${thPrice}</th><th>${thDiscPct}</th><th>${thContribPct}</th><th>${thCost}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>FDBBSZ 17K707 W</td><td>GLASS ASY - REAR VIEW OUTE</td><td>W</td><td>1.000</td>
                      <td>390.40</td><td>20</td><td>100.00</td><td>191.12</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <div class="card mb-3">
            <div class="card-header"><h5 class="mb-0">${labourHeader}</h5></div>
            <div class="card-body p-0">
              <div class="table-responsive">
                <table class="table mb-0">
                  <thead>
                    <tr class="small text-muted">
                      <th>${thRtsCode}</th><th>${thDesc}</th><th>${thW}</th><th>${thTime}</th><th>${thRate}</th>
                      <th>${thDiscPct}</th><th>${thContribPct}</th><th>${thExtValue}</th><th>${thCost}</th><th>${thTech}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr><td>OPLBODY</td><td>Repair RF door assy</td><td>W</td><td>15.50</td><td>50.00</td><td>0.00</td><td>100.00</td><td>775.00</td><td>0.00</td><td>BS-Jacinto Ne</td></tr>
                    <tr><td>NOTES</td><td>Cash OPL</td><td>0</td><td>0.00</td><td>50.00</td><td>0.00</td><td>100.00</td><td>0.00</td><td>0.00</td><td>-</td></tr>
                    <tr><td>OPLPAINT</td><td>Prep painting</td><td>W</td><td>14.40</td><td>50.00</td><td>0.00</td><td>100.00</td><td>720.00</td><td>0.00</td><td>BS-Bijaya Kur</td></tr>
                    <tr><td>OPLBODY</td><td>Mech & elec</td><td>W</td><td>5.00</td><td>50.00</td><td>0.00</td><td>100.00</td><td>250.00</td><td>0.00</td><td>BS-Abdullah S</td></tr>
                    <tr><td>P&M</td><td>Paint used on repair</td><td>1.00</td><td>1.00</td><td>715.00</td><td>0.00</td><td>100.00</td><td>715.00</td><td>0.00</td><td>-</td></tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <div class="card">
            <div class="card-header"><h5 class="mb-0">${serviceHeader}</h5></div>
            <div class="card-body">
              <div class="mb-3">
                <div><strong>${posAccount}:</strong> A9022085 SAUDI EGIS ENGINEERING CONSU</div>
                <div><strong>${afterBranch}:</strong> 1047 A1 - OPL Riyadh Main Branch</div>
                <div><strong>${afterExec}:</strong> kccsv VSB Default</div>
              </div>
              <div class="mb-3">
                <div><strong>${dispOdoAs}:</strong> ${kmLabel}</div>
                <div>${lastService}: 26-05-2025 @ 161045 ${kmLabel}</div>
                <div>${nextService}: 08-06-2025 @ 162045 ${kmLabel}</div>
                <div>${lastWork}: 18-08-2025 @ 168129 ${kmLabel}</div>
                <div>${lastOdo}: 18-08-2025 @ 168129 ${kmLabel}</div>
                <div>${timingBelt}: 0 ${months} ${orTxt} 0 ${kmLabel}</div>
                <div>${svcInterval}: 12 ${months} ${orTxt} 10000 ${kmLabel}</div>
                <div>${msCompany}: 00 Global Data</div>
              </div>
              <div class="row g-3">
                ${flags}
              </div>
            </div>
          </div>
        `;
        panel.dataset.filled = "1";
    }
    const trigger = document.querySelector('button[data-bs-target="#tabHistoryDetails"]');
    new bootstrap.Tab(trigger).show();
}


function ShowHistoryDetails(element) {
    let $el = $(element);
    let id = $el.data('wip-id'); // get the data-wip-id
    let $tr = $("#TR-" + id);    // select the related row

    if ($tr.length === 0) {
        console.warn("Related row not found for WIP ID:", id);
        return; // stop if no matching element
    }

    if ($el.hasClass('collapsed')) {
        $el.removeClass('collapsed').addClass('expanded');
        $tr.show();
    } else if ($el.hasClass('expanded')) {
        $el.removeClass('expanded').addClass('collapsed');
        $tr.hide();
    }
}



//$(document).on('click', '.btn-history-details', ensureHistoryDetailsTab($(this)));

