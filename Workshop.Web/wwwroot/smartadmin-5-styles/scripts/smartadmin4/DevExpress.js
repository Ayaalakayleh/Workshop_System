class EditableDataGrid {
    constructor() {
        this.gridBody = document.getElementById('gridBody');
        this.addRowBtn = document.getElementById('addRowBtn');
        this.rowIdCounter = 1;
        this.data = [];

        this.init();
    }

    init() {
        // Add initial sample data
        this.addSampleData();

        // Event listeners
        this.addRowBtn.addEventListener('click', () => this.addNewRow());

        // Event delegation for grid interactions
        this.gridBody.addEventListener('click', this.handleCellClick.bind(this));
        this.gridBody.addEventListener('keydown', this.handleKeyDown.bind(this));
        this.gridBody.addEventListener('input', this.handleInputChange.bind(this));
        this.gridBody.addEventListener('blur', this.handleInputBlur.bind(this), true);
    }

    addSampleData() {
        const sampleData = [
            { id: 1, firstName: 'John', lastName: 'Doe', email: 'john.doe@example.com', department: 'Engineering', salary: '75000' },
            { id: 2, firstName: 'Jane', lastName: 'Smith', email: 'jane.smith@example.com', department: 'Marketing', salary: '65000' },
            { id: 3, firstName: 'Bob', lastName: 'Johnson', email: 'bob.johnson@example.com', department: 'Sales', salary: '55000' }
        ];

        sampleData.forEach(data => {
            this.data.push(data);
            this.renderRow(data);
        });
    }

    addNewRow(data = null) {
        const newData = data || {
            id: this.getNextId(),
            firstName: '',
            lastName: '',
            email: '',
            department: '',
            salary: ''
        };

        this.data.push(newData);
        this.renderRow(newData, true);
    }

    renderRow(data, isNew = false) {
        const row = document.createElement('tr');
        row.className = `grid-row ${isNew ? 'new-row' : ''}`;
        row.dataset.rowId = data.id;

        row.innerHTML = `
            <td>
                <input type="text" class="grid-input readonly" value="${data.id}" readonly data-field="id" tabindex="-1">
            </td>
            <td><input type="text" class="grid-input" value="${data.firstName}" data-field="firstName" placeholder="First Name"></td>
            <td><input type="text" class="grid-input" value="${data.lastName}" data-field="lastName" placeholder="Last Name"></td>
            <td><input type="email" class="grid-input" value="${data.email}" data-field="email" placeholder="Email Address"></td>
            <td><input type="text" class="grid-input" value="${data.department}" data-field="department" placeholder="Department"></td>
            <td><input type="number" class="grid-input" value="${data.salary}" data-field="salary" placeholder="Salary"></td>
            <td style="text-align: center;">
                <button type="button" class="btn btn-sm btn-primary delete-btn" onclick="dataGrid.deleteRow(${data.id})">
                    <i class="fal fa-trash"></i>
                </button>
            </td>
        `;

        this.gridBody.appendChild(row);
    }

    handleCellClick(event) {
        const input = event.target.closest('.grid-input');
        if (input && !input.classList.contains('readonly')) {
            this.focusInput(input);
        }
    }

    handleKeyDown(event) {
        const input = event.target.closest('.grid-input');
        if (!input) return;

        const row = input.closest('tr');

        switch (event.key) {
            case 'Tab':
            case 'Enter':
                event.preventDefault();
                this.moveToNextInput(input, event.shiftKey);
                break;
            case 'ArrowDown':
                event.preventDefault();
                this.moveToRowBelow(input);
                break;
            case 'ArrowUp':
                event.preventDefault();
                this.moveToRowAbove(input);
                break;
            case 'Escape':
                input.blur();
                break;
        }
    }

    handleInputChange(event) {
        const input = event.target.closest('.grid-input');
        if (!input) return;

        const row = input.closest('tr');
        const rowId = parseInt(row.dataset.rowId);
        const field = input.dataset.field;
        const value = input.value;

        // Update data
        const dataIndex = this.data.findIndex(item => item.id === rowId);
        if (dataIndex !== -1) {
            this.data[dataIndex][field] = value;
        }
    }

    handleInputBlur(event) {
        const input = event.target.closest('.grid-input');
        if (!input) return;

        const row = input.closest('tr');
        row.classList.remove('editing');
    }

    focusInput(input) {
        input.focus();
        input.select();

        const row = input.closest('tr');
        row.classList.add('editing');
    }

    moveToNextInput(currentInput, reverse = false) {
        const row = currentInput.closest('tr');
        const inputs = Array.from(row.querySelectorAll('.grid-input:not(.readonly)'));
        const currentIndex = inputs.indexOf(currentInput);

        let nextInput;

        if (reverse) {
            if (currentIndex > 0) {
                nextInput = inputs[currentIndex - 1];
            } else {
                // Move to previous row, last input
                const prevRow = row.previousElementSibling;
                if (prevRow) {
                    const prevInputs = Array.from(prevRow.querySelectorAll('.grid-input:not(.readonly)'));
                    nextInput = prevInputs[prevInputs.length - 1];
                }
            }
        } else {
            if (currentIndex < inputs.length - 1) {
                nextInput = inputs[currentIndex + 1];
            } else {
                // Last input in row - check if we need to add a new row
                if (this.isLastRow(row) && this.hasRowData(row)) {
                    this.addNewRow();
                    // Focus on first input of new row after a short delay
                    setTimeout(() => {
                        const newRow = this.gridBody.lastElementChild;
                        const firstInput = newRow.querySelector('.grid-input:not(.readonly)');
                        if (firstInput) {
                            this.focusInput(firstInput);
                        }
                    }, 50);
                    return;
                } else {
                    // Move to next row, first input
                    const nextRow = row.nextElementSibling;
                    if (nextRow) {
                        nextInput = nextRow.querySelector('.grid-input:not(.readonly)');
                    }
                }
            }
        }

        if (nextInput) {
            this.focusInput(nextInput);
        }
    }

    moveToRowBelow(currentInput) {
        const row = currentInput.closest('tr');
        const nextRow = row.nextElementSibling;
        if (nextRow) {
            const field = currentInput.dataset.field;
            const targetInput = nextRow.querySelector(`[data-field="${field}"]`);
            if (targetInput && !targetInput.classList.contains('readonly')) {
                this.focusInput(targetInput);
            }
        }
    }

    moveToRowAbove(currentInput) {
        const row = currentInput.closest('tr');
        const prevRow = row.previousElementSibling;
        if (prevRow) {
            const field = currentInput.dataset.field;
            const targetInput = prevRow.querySelector(`[data-field="${field}"]`);
            if (targetInput && !targetInput.classList.contains('readonly')) {
                this.focusInput(targetInput);
            }
        }
    }

    isLastRow(row) {
        return row === this.gridBody.lastElementChild;
    }

    hasRowData(row) {
        const inputs = row.querySelectorAll('.grid-input:not(.readonly)');
        return Array.from(inputs).some(input => input.value.trim() !== '');
    }

    deleteRow(rowId) {
        if (confirm('Are you sure you want to delete this row?')) {
            // Remove from data
            this.data = this.data.filter(item => item.id !== rowId);

            // Remove from DOM
            const row = document.querySelector(`tr[data-row-id="${rowId}"]`);
            if (row) {
                row.style.animation = 'slideOut 0.3s ease-out';
                setTimeout(() => {
                    row.remove();
                }, 300);
            }
        }
    }

    getNextId() {
        return Math.max(...this.data.map(item => item.id), 0) + 1;
    }

    exportData() {
        return this.data;
    }
}

// Add slideOut animation
const style = document.createElement('style');
style.textContent = `
    @keyframes slideOut {
        from {
            opacity: 1;
            transform: translateX(0);
        }
        to {
            opacity: 0;
            transform: translateX(-100%);
        }
    }
`;
document.head.appendChild(style);

// Initialize the grid when DOM is loaded
let dataGrid;
document.addEventListener('DOMContentLoaded', () => {
    dataGrid = new EditableDataGrid();
});

// === Step 1: Helper Functions ===
function generateEmployees(count = 50) {
    const positions = ['Manager', 'Developer', 'Designer', 'Analyst', 'Tester', 'Admin'];
    const departments = ['IT', 'HR', 'Finance', 'Marketing', 'Sales'];
    const cities = ['New York', 'London', 'Tokyo', 'Paris', 'Berlin', 'Sydney'];
    const names = ['John Smith', 'Jane Doe', 'Mike Johnson', 'Sarah Wilson', 'Chris Brown', 'Lisa Davis', 'Tom Miller', 'Amy Taylor', 'David Lee', 'Emma White'];
    const data = [];
    for (let i = 1; i <= count; i++) {
        data.push({
            ID: i,
            Name: names[Math.floor(Math.random() * names.length)] + ' ' + i,
            Position: positions[Math.floor(Math.random() * positions.length)],
            Department: departments[Math.floor(Math.random() * departments.length)],
            Salary: Math.floor(Math.random() * 80000) + 30000,
            HireDate: new Date(
                2020 + Math.floor(Math.random() * 4),
                Math.floor(Math.random() * 12),
                Math.floor(Math.random() * 28) + 1
            ),
            City: cities[Math.floor(Math.random() * cities.length)],
            Email: `employee${i}@company.com`,
            Phone: `+1-${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 9000) + 1000}`,
            Active: Math.random() > 0.2
        });
    }
    return data;
}

function generateLargeDataset(count = 10000) {
    return generateEmployees(count);
}

// === Step 2: Panel Controls (no jQuery) ===
document.querySelectorAll('[data-action="panel-collapse"]').forEach(btn => {
    btn.addEventListener('click', function () {
        const panelContainer = this.closest('.panel').querySelector('.panel-container');
        panelContainer.classList.toggle('show');
        panelContainer.classList.toggle('collapsed');
        const icon = this.querySelector('i');
        icon.className = panelContainer.classList.contains('collapsed')
            ? 'fas fa-plus'
            : 'fas fa-minus';
    });
});
document.querySelectorAll('[data-action="panel-fullscreen"]').forEach(btn => {
    btn.addEventListener('click', function () {
        const panel = this.closest('.panel');
        panel.classList.toggle('fullscreen');
        const icon = this.querySelector('i');
        if (panel.classList.contains('fullscreen')) {
            panel.style.cssText = 'position: fixed; top: 0; left: 0; width:100%; height:100%; z-index:1050; margin:0;';
            icon.className = 'fas fa-compress';
        } else {
            panel.style.cssText = '';
            icon.className = 'fas fa-expand';
        }
    });
});
document.querySelectorAll('[data-action="panel-close"]').forEach(btn => {
    btn.addEventListener('click', function () {
        const panel = this.closest('.panel');
        panel.style.transition = 'all 0.3s ease';
        panel.style.opacity = '0';
        panel.style.transform = 'scale(0.8)';
        setTimeout(() => panel.remove(), 300);
    });
});

// === Step 3: Grid Instances Container ===
let gridInstances = {};

// === Step 4: DevExpress Grids Initialization ===

// Example 1: Basic Grid
gridInstances.basic = new DevExpress.ui.dxDataGrid(document.getElementById('basic-grid'), {
    dataSource: generateEmployees(20),
    columns: [
        { dataField: 'ID', width: 70 },
        'Name', 'Position', 'Department',
        { dataField: 'Salary', format: 'currency' }
    ],
    showBorders: true,
    columnAutoWidth: true
});

// Example 2: Sorting & Filtering Grid
gridInstances.filtering = new DevExpress.ui.dxDataGrid(document.getElementById('filtering-grid'), {
    dataSource: generateEmployees(50),
    columns: [
        { dataField: 'ID', width: 70 },
        'Name', 'Position', 'Department',
        { dataField: 'Salary', format: 'currency' },
        'City'
    ],
    showBorders: true,
    columnAutoWidth: true,
    sorting: { mode: 'multiple' },
    filterRow: { visible: true },
    headerFilter: { visible: true },
    searchPanel: { visible: true, width: 240, placeholder: 'Search...' }
});

// Example 3: Paging
gridInstances.paging = new DevExpress.ui.dxDataGrid(document.getElementById('paging-grid'), {
    dataSource: generateEmployees(100),
    columns: [
        { dataField: 'ID', width: 70 },
        'Name', 'Position', 'Department',
        { dataField: 'Salary', format: 'currency' }
    ],
    showBorders: true,
    columnAutoWidth: true,
    paging: { enabled: true, pageSize: 10 },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [5, 10, 20, 50],
        showInfo: true
    }
});

// Example 4: Selection
gridInstances.selection = new DevExpress.ui.dxDataGrid(document.getElementById('selection-grid'), {
    dataSource: generateEmployees(30),
    columns: [
        { dataField: 'ID', width: 70 },
        'Name', 'Position', 'Department',
        { dataField: 'Salary', format: 'currency' }
    ],
    showBorders: true,
    columnAutoWidth: true,
    selection: { mode: 'multiple', showCheckBoxesMode: 'always' },
    onSelectionChanged(e) {
        document.getElementById('selection-info').textContent =
            `Selected rows: ${e.selectedRowsData.length}`;
    }
});

// Example 5: Editing
gridInstances.editing = new DevExpress.ui.dxDataGrid(document.getElementById('editing-grid'), {
    dataSource: generateEmployees(20),
    columns: [
        { dataField: 'ID', width: 70, allowEditing: false },
        { dataField: 'Name', validationRules: [{ type: 'required' }] },
        {
            dataField: 'Position',
            lookup: {
                dataSource: ['Manager', 'Developer', 'Designer', 'Analyst', 'Tester'],
                valueExpr: 'this',
                displayExpr: 'this'
            }
        },
        'Department',
        {
            dataField: 'Salary',
            format: 'currency',
            validationRules: [{ type: 'range', min: 1000, max: 200000 }]
        }
    ],
    showBorders: true,
    columnAutoWidth: true,
    editing: {
        mode: 'row',
        allowUpdating: true,
        allowDeleting: true,
        allowAdding: true
    }
});

// Example 6: Grouping
gridInstances.grouping = new DevExpress.ui.dxDataGrid(document.getElementById('grouping-grid'), {
    dataSource: generateEmployees(50),
    columns: [
        { dataField: 'ID', width: 70 },
        'Name', 'Position', 'Department',
        { dataField: 'Salary', format: 'currency' },
        'City'
    ],
    showBorders: true,
    columnAutoWidth: true,
    groupPanel: { visible: true },
    grouping: { autoExpandAll: false },
    summary: {
        groupItems: [
            { column: 'Salary', summaryType: 'avg', valueFormat: 'currency', displayFormat: 'Avg Salary: {0}' },
            { column: 'ID', summaryType: 'count', displayFormat: 'Count: {0}' }
        ]
    }
});

// Example 7: Master‑Detail
const masterData = [
    { ID: 1, CompanyName: 'Tech Corp', City: 'New York' },
    { ID: 2, CompanyName: 'Design Studio', City: 'London' },
    { ID: 3, CompanyName: 'Data Systems', City: 'Tokyo' }
];
gridInstances.masterDetail = new DevExpress.ui.dxDataGrid(document.getElementById('master-detail-grid'), {
    dataSource: masterData,
    columns: [
        { dataField: 'ID', width: 70 },
        'CompanyName', 'City'
    ],
    showBorders: true,
    columnAutoWidth: true,
    masterDetail: {
        enabled: true,
        template(container, options) {
            const detailData = generateEmployees(10).map(emp => ({
                ...emp,
                CompanyID: options.key
            }));
            const div = document.createElement('div');
            container.appendChild(div);
            new DevExpress.ui.dxDataGrid(div, {
                dataSource: detailData,
                columns: ['Name', 'Position', 'Department', { dataField: 'Salary', format: 'currency' }, 'Email'],
                showBorders: true,
                columnAutoWidth: true
            });
        }
    }
});
