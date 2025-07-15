/**
 * zuHause Admin 共用 JavaScript 模組
 * 專注於真正通用的技術性邏輯，避免過度抽象化
 */

/**
 * 通用 Tab 管理器 - 處理 Bootstrap Tab 切換
 * @param {string} tabContainerId - Tab 容器 ID
 */
function initializeAdminTabs(tabContainerId) {
    const tabContainer = document.querySelector(`#${tabContainerId}`);
    if (!tabContainer) return;
    
    const triggerTabList = [].slice.call(tabContainer.querySelectorAll('button[data-bs-toggle="tab"]'));
    triggerTabList.forEach(function (triggerEl) {
        const tabTrigger = new bootstrap.Tab(triggerEl);
        
        triggerEl.addEventListener('click', function (event) {
            event.preventDefault();
            tabTrigger.show();
        });
    });
}

/**
 * 安全 Modal 管理器 - 統一 Modal 安全設定
 * @param {string} modalId - Modal 元素 ID
 * @param {Object} options - 額外選項
 * @returns {bootstrap.Modal} Bootstrap Modal 實例
 */
function createSecureModal(modalId, options = {}) {
    const modalElement = document.getElementById(modalId);
    if (!modalElement) {
        console.warn(`Modal element with ID '${modalId}' not found`);
        return null;
    }
    
    const defaultConfig = {
        backdrop: 'static',
        keyboard: false
    };
    
    const config = { ...defaultConfig, ...options };
    return new bootstrap.Modal(modalElement, config);
}

/**
 * 批量選取管理器 - 處理全選/取消全選邏輯
 * @param {string} selectAllId - 全選checkbox ID
 * @param {string} checkboxClass - 項目checkbox class
 * @param {string} bulkBtnId - 批量操作按鈕 ID
 */
function initializeBulkSelection(selectAllId, checkboxClass, bulkBtnId) {
    const selectAllCheckbox = document.getElementById(selectAllId);
    const bulkBtn = document.getElementById(bulkBtnId);
    
    if (!selectAllCheckbox || !bulkBtn) return;
    
    // 全選/取消全選
    selectAllCheckbox.addEventListener('change', function() {
        const checkboxes = document.querySelectorAll(`.${checkboxClass}`);
        const isChecked = this.checked;
        
        checkboxes.forEach(checkbox => {
            checkbox.checked = isChecked;
        });
        
        updateBulkButtonState(checkboxClass, bulkBtnId);
    });
    
    // 監聽個別 checkbox 變化
    document.addEventListener('change', function(e) {
        if (e.target.classList.contains(checkboxClass)) {
            updateBulkButtonState(checkboxClass, bulkBtnId);
            updateSelectAllState(selectAllId, checkboxClass);
        }
    });
}

/**
 * 更新批量操作按鈕狀態
 */
function updateBulkButtonState(checkboxClass, bulkBtnId) {
    const checkedBoxes = document.querySelectorAll(`.${checkboxClass}:checked`);
    const bulkBtn = document.getElementById(bulkBtnId);
    
    if (bulkBtn) {
        bulkBtn.disabled = checkedBoxes.length === 0;
    }
}

/**
 * 更新全選按鈕狀態
 */
function updateSelectAllState(selectAllId, checkboxClass) {
    const selectAllCheckbox = document.getElementById(selectAllId);
    const allCheckboxes = document.querySelectorAll(`.${checkboxClass}`);
    const checkedBoxes = document.querySelectorAll(`.${checkboxClass}:checked`);
    
    if (selectAllCheckbox && allCheckboxes.length > 0) {
        selectAllCheckbox.checked = allCheckboxes.length === checkedBoxes.length;
        selectAllCheckbox.indeterminate = checkedBoxes.length > 0 && checkedBoxes.length < allCheckboxes.length;
    }
}

/**
 * 表單重置工具
 * @param {string} formSelector - 表單選擇器
 */
function resetAdminForm(formSelector) {
    const form = document.querySelector(formSelector);
    if (!form) return;
    
    // 重置所有 input 和 select
    const inputs = form.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        if (input.type === 'checkbox' || input.type === 'radio') {
            input.checked = false;
        } else {
            input.value = '';
            if (input.tagName.toLowerCase() === 'select') {
                input.selectedIndex = 0;
            }
        }
    });
}

/**
 * 雙重確認對話框
 * @param {string} message - 確認訊息
 * @param {Function} onConfirm - 確認後執行的函數
 * @param {Object} options - 選項
 */
function showDoubleCheckConfirm(message, onConfirm, options = {}) {
    const config = {
        title: '請確認操作',
        confirmText: '確認執行',
        cancelText: '取消',
        requireCheckbox: true,
        checkboxText: '我確認要執行此操作',
        ...options
    };
    
    // 建立動態確認對話框
    const modalHtml = `
        <div class="modal fade" id="dynamicConfirmModal" tabindex="-1" data-bs-backdrop="static" data-bs-keyboard="false">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${config.title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p>${message}</p>
                        ${config.requireCheckbox ? `
                            <div class="form-check mt-3">
                                <input class="form-check-input" type="checkbox" id="confirmCheckbox">
                                <label class="form-check-label" for="confirmCheckbox">
                                    ${config.checkboxText}
                                </label>
                            </div>
                        ` : ''}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${config.cancelText}</button>
                        <button type="button" class="btn btn-danger" id="confirmActionBtn" ${config.requireCheckbox ? 'disabled' : ''}>${config.confirmText}</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // 移除舊的 modal
    const existingModal = document.getElementById('dynamicConfirmModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // 添加新的 modal
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    const modal = createSecureModal('dynamicConfirmModal');
    const confirmBtn = document.getElementById('confirmActionBtn');
    
    if (config.requireCheckbox) {
        const checkbox = document.getElementById('confirmCheckbox');
        checkbox.addEventListener('change', function() {
            confirmBtn.disabled = !this.checked;
        });
    }
    
    confirmBtn.addEventListener('click', function() {
        modal.hide();
        onConfirm();
        // 清理 modal
        setTimeout(() => {
            document.getElementById('dynamicConfirmModal').remove();
        }, 300);
    });
    
    modal.show();
}

/**
 * Admin 頁面通用初始化
 * @param {Object} config - 配置選項
 */
function initializeAdminPage(config = {}) {
    const defaultConfig = {
        tabContainerId: null,
        bulkSelections: [], // [{ selectAllId, checkboxClass, bulkBtnId }]
        sortableColumns: true
    };
    
    const pageConfig = { ...defaultConfig, ...config };
    
    // 初始化 Tabs
    if (pageConfig.tabContainerId) {
        initializeAdminTabs(pageConfig.tabContainerId);
    }
    
    // 初始化批量選取
    pageConfig.bulkSelections.forEach(selection => {
        initializeBulkSelection(selection.selectAllId, selection.checkboxClass, selection.bulkBtnId);
    });
    
    // 初始化排序 (如果需要)
    if (pageConfig.sortableColumns) {
        initializeSortableColumns();
    }
}

/**
 * 可排序欄位初始化 (保留現有排序邏輯但統一管理)
 */
function initializeSortableColumns() {
    const sortableHeaders = document.querySelectorAll('.sortable');
    
    sortableHeaders.forEach(header => {
        header.addEventListener('click', function() {
            const column = this.dataset.sort;
            const tableType = this.dataset.table;
            const icon = this.querySelector('.sort-icon');
            
            // 移除其他列的 active class
            sortableHeaders.forEach(h => {
                if (h !== this) {
                    h.classList.remove('active');
                    const otherIcon = h.querySelector('.sort-icon');
                    if (otherIcon) otherIcon.classList.remove('active');
                }
            });
            
            // 切換當前列的排序狀態
            this.classList.toggle('active');
            if (icon) icon.classList.toggle('active');
            
            // 觸發自定義排序事件，讓各頁面自行處理排序邏輯
            const sortEvent = new CustomEvent('adminTableSort', {
                detail: { column, tableType, direction: this.classList.contains('active') ? 'desc' : 'asc' }
            });
            document.dispatchEvent(sortEvent);
        });
    });
}