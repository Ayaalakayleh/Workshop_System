import { SmartTables } from '../optional/smartTables/smartTables.bundle.js';

function getModelData() {
    if (typeof window.itemMasterData !== 'undefined') {
        return window.itemMasterData.map(item => ({
            id: String(item.id),
            itemCode: item.itemCode || '',
            itemNameEn: item.itemNameEn || '',
            itemNameAr: item.itemNameAr || '',
            uom: item.uom || '',
            serialized: Boolean(item.serialized),
            batchControlled: Boolean(item.batchControlled),
            warrantyEligible: Boolean(item.warrantyEligible),
            active: Boolean(item.active)
        }));
    }

    console.warn('itemMasterData not found. Make sure to include the data script in your Razor view.');
    return [];
}

document.addEventListener('DOMContentLoaded', () => {
    const tableData = getModelData();

    if (tableData.length === 0) {
        console.warn('No data available from model. Table will be empty.');
    }

    const rowStates = {
        editing: null,
        saved: new Set()
    };

    const clientTable = new SmartTables('clientTable', {
        data: {
            type: 'json',
            source: tableData,
            idField: 'id',
            columns: [
                {
                    data: 'id',
                    title: 'ID',
                    editable: false
                },
                {
                    data: 'itemCode',
                    title: 'Item Code',
                    required: true
                },
                {
                    data: 'itemNameEn',
                    title: 'Item Name (EN)',
                    required: true
                },
                {
                    data: 'itemNameAr',
                    title: 'Item Name (AR)',
                    required: false
                },
                {
                    data: 'uom',
                    title: 'UOM',
                    type: 'select',
                    options: [
                        'PCS', 'KG', 'LITER', 'METER', 'BOX', 'CARTON', 'PACK', 'SET'
                    ],
                    required: true
                },
                {
                    data: 'serialized',
                    title: 'Serialized?',
                    type: 'boolean',
                    render: data => data === true
                        ? '<span class="badge bg-success">Yes</span>'
                        : '<span class="badge bg-danger">No</span>'
                },
                {
                    data: 'batchControlled',
                    title: 'Batch Controlled?',
                    type: 'boolean',
                    render: data => data === true
                        ? '<span class="badge bg-success">Yes</span>'
                        : '<span class="badge bg-danger">No</span>'
                },
                {
                    data: 'warrantyEligible',
                    title: 'Warranty Eligible?',
                    type: 'boolean',
                    render: data => data === true
                        ? '<span class="badge bg-success">Yes</span>'
                        : '<span class="badge bg-danger">No</span>'
                },
                {
                    data: 'active',
                    title: 'Active',
                    type: 'boolean',
                    render: data => data === true
                        ? '<span class="badge bg-success">Yes</span>'
                        : '<span class="badge bg-danger">No</span>'
                },
                {
                    data: 'actions',
                    title: 'Actions',
                    sortable: false,
                    editable: false,
                    render: (data, row) => `
                        <a class="btn btn-xs btn-info" href="/ItemMaster/Edit/${row.id}">
                            <i class="fa fa-edit"></i> Edit
                        </a>
                                                <button type="button" class="btn btn-xs btn-danger">
                            <i class="fa fa-trash"></i> Delete
                        </a>
                    `
                }
            ]
        },
        debug: true,
        responsive: true,

        hooks: {
            beforeEdit(rowId) {
                rowStates.editing = rowId;
                const rowElement = this.table.querySelector(`tbody tr[data-id="${rowId}"]`);
                if (rowElement) {
                    rowElement.classList.add('editing');
                }
                return true;
            },

            afterEdit(...args) {
                let rowId, rowData, success;
                if (args.length === 1 && typeof args[0] === 'object') {
                    ({ rowId, rowData, success } = args[0]);
                } else {
                    [rowId, rowData, success] = args;
                }

                rowStates.editing = null;

                if (success === true) {
                    rowStates.saved.add(rowId);
                    setTimeout(() => {
                        rowStates.saved.delete(rowId);
                        const rowElement = this.table.querySelector(`tbody tr[data-id="${rowId}"]`);
                        if (rowElement) {
                            rowElement.classList.remove('saved');
                        }
                    }, 3000);
                }

                this.draw();

                if (this.searchQuery && this.searchQuery.trim() !== '') {
                    this.handleSearch(this.searchQuery);
                }

                if (this.currentSortColumn !== undefined && this.currentSortDirection) {
                    this.sortBy(this.currentSortColumn, this.currentSortDirection);
                }

                if (rowStates.editing) {
                    const editingRow = this.table.querySelector(`tbody tr[data-id="${rowStates.editing}"]`);
                    if (editingRow) {
                        editingRow.classList.add('editing');
                    }
                }
                rowStates.saved.forEach(savedRowId => {
                    const savedRow = this.table.querySelector(`tbody tr[data-id="${savedRowId}"]`);
                    if (savedRow) {
                        savedRow.classList.add('saved');
                    }
                });
            },

            onEditModalCreated(modalHTML, rowId, rowData) {
                return modalHTML;
            },

            onEditModalBeforeShow(modalElement, rowId, rowData) {
                console.log('Edit modal is about to be shown for row:', rowId);
                modalElement.classList.add('custom-edit-modal');

                const form = modalElement.querySelector('form');
                if (form) {
                    form.addEventListener('keydown', (e) => {
                        if (e.key === 'Enter') {
                            e.preventDefault();
                        }
                    });
                }
            },

            onEditDataCollected(updatedData, rowId, originalData) {
                console.log('Data collected from edit form:', updatedData);

                const processedData = { ...updatedData };

                const columnTypes = {};
                this.options.data.columns.forEach(column => {
                    if (column.data && column.type) {
                        columnTypes[column.data] = column.type;
                    }
                });

                for (const field in columnTypes) {
                    const type = columnTypes[field];
                    const value = processedData[field];

                    switch (type) {
                        case 'number':
                            processedData[field] = value !== undefined && value !== ''
                                ? parseFloat(value)
                                : 0;
                            break;
                        case 'boolean':
                            processedData[field] = field in processedData && value === 'on';
                            break;
                        case 'date':
                            processedData[field] = value ? new Date(value).toISOString().split('T')[0] : '';
                            break;
                        default:
                            break;
                    }
                }

                processedData.lastModified = new Date().toISOString();

                const recordIndex = tableData.findIndex(item => item.id === rowId);
                if (recordIndex !== -1) {
                    tableData[recordIndex] = { ...tableData[recordIndex], ...processedData };
                }

                return processedData;
            },

            onEditSuccess(rowId, updatedRecord, submittedData) {
                console.log('Record updated successfully:', rowId);

                const notification = document.createElement('div');
                notification.className = 'floating-notification success';
                notification.textContent = `Item ${updatedRecord.itemCode} updated successfully`;
                document.body.appendChild(notification);

                setTimeout(() => {
                    notification.classList.add('show');
                    setTimeout(() => {
                        notification.classList.remove('show');
                        setTimeout(() => notification.remove(), 300);
                    }, 2000);
                }, 10);
            },

            onEditError(rowId, error, attemptedData) {
                console.error('Error updating record:', rowId, error);
            }
        }
    });

    document.addEventListener('hidden.bs.modal', (event) => {
        const modal = event.target;

        if (rowStates.editing) {
            const editingRow = clientTable.table.querySelector(`tbody tr[data-id="${rowStates.editing}"]`);
            if (editingRow) {
                editingRow.classList.remove('editing');
            }
            rowStates.editing = null;
        }

        rowStates.saved.forEach(savedRowId => {
            const savedRow = clientTable.table.querySelector(`tbody tr[data-id="${savedRowId}"]`);
            if (savedRow) {
                savedRow.classList.add('saved');
            }
        });
    });
});