$(document).ready(function () {
    // Common table data
    const sampleData = [
        ['Tiger Nixon', 'System Architect', 'Edinburgh', 61, '2011/04/25', '$320,800'],
        ['Garrett Winters', 'Accountant', 'Tokyo', 63, '2011/07/25', '$170,750'],
        ['Ashton Cox', 'Junior Technical Author', 'San Francisco', 66, '2009/01/12', '$86,000'],
        ['Cedric Kelly', 'Senior Javascript Developer', 'Edinburgh', 22, '2012/03/29', '$433,060'],
        ['Airi Satou', 'Accountant', 'Tokyo', 33, '2008/11/28', '$162,700'],
        ['Brielle Williamson', 'Integration Specialist', 'New York', 61, '2012/12/02', '$372,000'],
        ['Herrod Chandler', 'Sales Assistant', 'San Francisco', 59, '2012/08/06', '$137,500'],
        ['Rhona Davidson', 'Integration Specialist', 'Tokyo', 55, '2010/10/14', '$327,900'],
        ['Colleen Hurst', 'Javascript Developer', 'San Francisco', 39, '2009/09/15', '$205,500'],
        ['Sonya Frost', 'Software Engineer', 'Edinburgh', 23, '2008/12/13', '$103,600'],
        ['Jena Gaines', 'Office Manager', 'London', 30, '2008/12/19', '$90,560'],
        ['Quinn Flynn', 'Support Lead', 'Edinburgh', 22, '2013/03/03', '$342,000'],
        ['Charde Marshall', 'Regional Director', 'San Francisco', 36, '2008/10/16', '$470,600'],
        ['Haley Kennedy', 'Senior Marketing Designer', 'London', 43, '2012/12/18', '$313,500'],
        ['Tatyana Fitzpatrick', 'Regional Director', 'London', 19, '2010/03/17', '$385,750'],
        ['Michael Silva', 'Marketing Designer', 'London', 66, '2012/11/27', '$198,500'],
        ['Paul Byrd', 'Chief Financial Officer (CFO)', 'New York', 64, '2010/06/09', '$725,000'],
        ['Gloria Little', 'Systems Administrator', 'New York', 59, '2009/04/10', '$237,500'],
        ['Bradley Greer', 'Software Engineer', 'London', 41, '2012/10/13', '$132,000'],
        ['Dai Rios', 'Personnel Lead', 'Edinburgh', 35, '2012/09/26', '$217,500']
    ];

    // Extended data for fixed columns table
    const extendedData = sampleData.map(row => [
        ...row,
        '+1 (555) ' + Math.floor(Math.random() * 9000000 + 1000000),
        row[0].toLowerCase().replace(' ', '.') + 'company.com',
        ['IT', 'Sales', 'Marketing', 'HR', 'Finance'][Math.floor(Math.random() * 5)],
        sampleData[Math.floor(Math.random() * sampleData.length)][0]
    ]);

    // 1. Basic DataTable
    $('#basicTable').DataTable({
        responsive: true,
        pageLength: 10,
        info: false,
        paging:false,
        dom: 'tp' // Removes lengthMenu, search box, and info
    });


    // 2. Advanced Search & Filtering
    const searchTable = $('#searchTable').DataTable({
        data: sampleData,
        columns: [
            { title: "Name" },
            { title: "Position" },
            { title: "Office" },
            { title: "Age" },
            { title: "Start date" },
            { title: "Salary" }
        ],
        responsive: true,
        pageLength: 10
    });

    // Custom search functions
    $('#searchName').on('keyup', function () {
        searchTable.column(0).search(this.value).draw();
    });

    $('#searchPosition').on('keyup', function () {
        searchTable.column(1).search(this.value).draw();
    });

    // Age range filter
    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        if (settings.nTable.id !== 'searchTable') {
            return true;
        }

        const min = parseInt($('#minAge').val(), 10);
        const max = parseInt($('#maxAge').val(), 10);
        const age = parseFloat(data[3]) || 0;

        if ((isNaN(min) && isNaN(max)) ||
            (isNaN(min) && age <= max) ||
            (min <= age && isNaN(max)) ||
            (min <= age && age <= max)) {
            return true;
        }
        return false;
    });

    $('#minAge, #maxAge').on('keyup', function () {
        searchTable.draw();
    });

    // 3. Server-Side Processing (simulated)
    $('#serverSideTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: function (data, callback) {
            // Simulate server processing
            setTimeout(() => {
                const result = {
                    draw: data.draw,
                    recordsTotal: 1000,
                    recordsFiltered: 1000,
                    data: []
                };

                // Generate sample data
                for (let i = 0; i < data.length; i++) {
                    const names = ['John', 'Jane', 'Bob', 'Alice', 'Charlie', 'Diana', 'Eve', 'Frank'];
                    const surnames = ['Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis'];
                    const positions = ['Developer', 'Designer', 'Manager', 'Analyst', 'Coordinator'];
                    const offices = ['New York', 'London', 'Tokyo', 'Sydney', 'Paris'];

                    result.data.push([
                        names[Math.floor(Math.random() * names.length)],
                        surnames[Math.floor(Math.random() * surnames.length)],
                        positions[Math.floor(Math.random() * positions.length)],
                        offices[Math.floor(Math.random() * offices.length)],
                        '2020/01/01',
                        ' + (Math.floor(Math.random() * 100000) + 50000).toLocaleString()'
                    ]);
                }

                callback(result);
            }, 500);
        },
        columns: [
            { title: "First name" },
            { title: "Last name" },
            { title: "Position" },
            { title: "Office" },
            { title: "Start date" },
            { title: "Salary" }
        ],
        responsive: true
    });

    // 4. Column Visibility & Export
    $('#exportTable').DataTable({
        data: sampleData.map(row => [...row, '<button class="btn btn-sm btn-primary">View</button> <button class="btn btn-sm btn-warning">Edit</button> <button class="btn btn-sm btn-danger">Delete</button>']),
        columns: [
            { title: "Name" },
            { title: "Position" },
            { title: "Office" },
            { title: "Age" },
            { title: "Start date" },
            { title: "Salary" },
            { title: "Actions", orderable: false }
        ],
        dom: 'Bfrtip',
        buttons: [
            {
                extend: 'copy',
                text: '<i class="fas fa-copy"></i> Copy',
                className: 'btn-primary'
            },
            {
                extend: 'csv',
                text: '<i class="fas fa-file-csv"></i> CSV',
                className: 'btn-success'
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i> Excel',
                className: 'btn-success'
            },
            {
                extend: 'pdf',
                text: '<i class="fas fa-file-pdf"></i> PDF',
                className: 'btn-danger'
            },
            {
                extend: 'print',
                text: '<i class="fas fa-print"></i> Print',
                className: 'btn-info'
            },
            {
                extend: 'colvis',
                text: '<i class="fas fa-columns"></i> Columns',
                className: 'btn-secondary'
            }
        ],
        responsive: true,
        pageLength: 10
    });

    // 5. Row Selection & Bulk Actions - FIXED VERSION
    const selectionTable = $('#selectionTable').DataTable({
        data: sampleData,
        columns: [
            {
                title: '',
                data: null,
                defaultContent: '',
                orderable: false,
                className: 'select-checkbox'
            },
            { title: "Name", data: 0 },
            { title: "Position", data: 1 },
            { title: "Office", data: 2 },
            { title: "Age", data: 3 },
            { title: "Start date", data: 4 },
            { title: "Salary", data: 5 }
        ],
        select: {
            style: 'multi',
            selector: 'td:first-child'
        },
        responsive: true
    });

    // Selection event handlers
    selectionTable.on('select deselect', function () {
        const selectedRows = selectionTable.rows({ selected: true }).count();
        $('#selectedCount').text(selectedRows + ' selected');
        $('#deleteSelected').prop('disabled', selectedRows === 0);
    });

    $('#selectAll').on('click', function () {
        selectionTable.rows().select();
    });

    $('#deselectAll').on('click', function () {
        selectionTable.rows().deselect();
    });

    $('#deleteSelected').on('click', function () {
        if (confirm('Are you sure you want to delete the selected rows?')) {
            selectionTable.rows({ selected: true }).remove().draw();
            $('#selectedCount').text('0 selected');
            $(this).prop('disabled', true);
        }
    });

    // 6. Child Rows & Detailed View
    function formatChildRow(data) {
        return `<div class="child-row-details">
                    <div class="row">
                        <div class="col-md-6">
                            <h6><i class="fas fa-user"></i> Personal Information</h6>
                            <p><strong>Full Name:</strong> ${data[0]}</p>
                            <p><strong>Age:</strong> ${data[3]} years old</p>
                            <p><strong>Start Date:</strong> ${data[4]}</p>
                        </div>
                        <div class="col-md-6">
                            <h6><i class="fas fa-briefcase"></i> Professional Details</h6>
                            <p><strong>Position:</strong> ${data[1]}</p>
                            <p><strong>Office Location:</strong> ${data[2]}</p>
                            <p><strong>Annual Salary:</strong> ${data[5]}</p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-12">
                            <h6><i class="fas fa-chart-line"></i> Performance Metrics</h6>
                            <div class="progress mb-2">
                                <div class="progress-bar bg-success" style="width: ${Math.floor(Math.random() * 40) + 60}%">Performance</div>
                            </div>
                            <p><strong>Projects Completed:</strong> ${Math.floor(Math.random() * 20) + 5}</p>
                            <p><strong>Team Rating:</strong> ${(Math.random() * 2 + 3).toFixed(1)}/5.0</p>
                        </div>
                    </div>
                </div>`;
    }

    const childRowsTable = $('#childRowsTable').DataTable({
        data: sampleData,
        columns: [
            {
                className: 'dtr-control',
                orderable: false,
                data: null,
                defaultContent: ''
            },
            { title: "Name" },
            { title: "Position" },
            { title: "Office" },
            { title: "Salary" }
        ],
        responsive: false
    });

    $('#childRowsTable tbody').on('click', 'td.dtr-control', function () {
        const tr = $(this).closest('tr');
        const row = childRowsTable.row(tr);

        if (row.child.isShown()) {
            row.child.hide();
            tr.removeClass('shown');
        } else {
            row.child(formatChildRow(row.data())).show();
            tr.addClass('shown');
        }
    });

    // 7. Fixed Columns
    $('#fixedColumnsTable').DataTable({
        data: extendedData,
        columns: [
            { title: "Name" },
            { title: "Position" },
            { title: "Office" },
            { title: "Age" },
            { title: "Start date" },
            { title: "Salary" },
            { title: "Phone" },
            { title: "Email" },
            { title: "Department" },
            { title: "Manager" }
        ],
        scrollX: true,
        fixedColumns: {
            leftColumns: 2
        },
        responsive: false
    });

    // 8. Inline Editing
    let editingTable = $('#editingTable').DataTable({
        data: sampleData.map(row => [...row, '<button class="btn btn-sm btn-primary edit-btn">Edit</button> <button class="btn btn-sm btn-danger delete-btn">Delete</button>']),
        columns: [
            { title: "Name", className: "editable-cell" },
            { title: "Position", className: "editable-cell" },
            { title: "Office", className: "editable-cell" },
            { title: "Age", className: "editable-cell" },
            { title: "Start date", className: "editable-cell" },
            { title: "Salary", className: "editable-cell" },
            { title: "Actions", orderable: false }
        ],
        responsive: true
    });

    // Edit functionality
    let currentEditingRow = null;
    let originalData = {};

    $('#editingTable tbody').on('dblclick', 'td.editable-cell', function () {
        const cell = editingTable.cell(this);
        const cellData = cell.data();
        const input = $('<input type="text" class="form-control form-control-sm">').val(cellData);

        $(this).html(input);
        input.focus().select();

        currentEditingRow = editingTable.row(this);
        if (!currentEditingRow.node().classList.contains('editing-row')) {
            originalData = [...currentEditingRow.data()];
            $(currentEditingRow.node()).addClass('editing-row');
        }

        input.on('blur keypress', function (e) {
            if (e.type === 'blur' || e.which === 13) {
                cell.data(this.value).draw(false);
            }
        });
    });

    // Edit button functionality
    $('#editingTable tbody').on('click', '.edit-btn', function () {
        const row = editingTable.row($(this).parents('tr'));
        const data = row.data();

        if ($(this).text() === 'Edit') {
            originalData = [...data];
            $(this).text('Save').removeClass('btn-primary').addClass('btn-success');
            $(this).siblings('.delete-btn').text('Cancel').removeClass('btn-danger').addClass('btn-secondary');
            $(row.node()).addClass('editing-row');
        } else {
            // Save
            $(this).text('Edit').removeClass('btn-success').addClass('btn-primary');
            $(this).siblings('.delete-btn').text('Delete').removeClass('btn-secondary').addClass('btn-danger');
            $(row.node()).removeClass('editing-row');
            // Here you would normally send data to server
            alert('Changes saved successfully!');
        }
    });

    // Delete/Cancel button functionality
    $('#editingTable tbody').on('click', '.delete-btn', function () {
        const row = editingTable.row($(this).parents('tr'));

        if ($(this).text() === 'Delete') {
            if (confirm('Are you sure you want to delete this row?')) {
                row.remove().draw();
            }
        } else {
            // Cancel
            row.data(originalData).draw(false);
            $(this).text('Delete').removeClass('btn-secondary').addClass('btn-danger');
            $(this).siblings('.edit-btn').text('Edit').removeClass('btn-success').addClass('btn-primary');
            $(row.node()).removeClass('editing-row');
        }
    });

    // Add new row functionality
    $('#addNew').on('click', function () {
        const newRow = ['New Employee', 'New Position', 'New Office', '25', '2024/01/01', '$50,000', '<button class="btn btn-sm btn-primary edit-btn">Edit</button> <button class="btn btn-sm btn-danger delete-btn">Delete</button>'];
        editingTable.row.add(newRow).draw();
    });

    // Custom styling for DataTables
    $('.dataTables_wrapper').each(function () {
        $(this).find('.dataTables_length select').addClass('form-select form-select-sm');
        $(this).find('.dataTables_filter input').addClass('form-control form-control-sm');
    });
});