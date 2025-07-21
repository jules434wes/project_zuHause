/**
 * 公告管理 JavaScript
 * 處理公告的新增、編輯、刪除、搜尋等功能
 */

// 初始化公告管理器
function initAnnouncementManager() {
    console.log("🚀 初始化公告管理功能");
    
    // 綁定事件
    bindSearchEvents();
    bindFilterEvents();
    bindActionButtons();
    loadAnnouncements();
}

// 綁定搜尋功能
function bindSearchEvents() {
    const searchInput = document.querySelector('input[placeholder="搜尋公告標題或內容..."]');
    const searchButton = document.querySelector('.input-group .btn-outline-secondary');
    
    if (searchButton) {
        searchButton.addEventListener('click', performSearch);
    }
    
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                performSearch();
            }
        });
    }
}

// 綁定篩選功能
function bindFilterEvents() {
    const statusFilter = document.querySelector('select[class="form-select"]:first-of-type');
    const typeFilter = document.querySelector('select[class="form-select"]:last-of-type');
    
    if (statusFilter) {
        statusFilter.addEventListener('change', performFilter);
    }
    
    if (typeFilter) {
        typeFilter.addEventListener('change', performFilter);
    }
}

// 綁定操作按鈕
function bindActionButtons() {
    // 新增公告按鈕
    const addButton = document.querySelector('.btn-primary');
    if (addButton) {
        addButton.addEventListener('click', showAddAnnouncementModal);
    }
    
    // 全選按鈕
    const selectAllCheckbox = document.querySelector('thead input[type="checkbox"]');
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', toggleSelectAll);
    }
    
    // 個別操作按鈕
    bindRowActionButtons();
}

// 綁定表格行操作按鈕
function bindRowActionButtons() {
    const editButtons = document.querySelectorAll('.btn-outline-primary');
    const viewButtons = document.querySelectorAll('.btn-outline-secondary');
    const deleteButtons = document.querySelectorAll('.btn-outline-danger');
    
    editButtons.forEach(btn => {
        if (btn.querySelector('.bi-pencil')) {
            btn.addEventListener('click', function() {
                const row = this.closest('tr');
                editAnnouncement(row);
            });
        }
    });
    
    viewButtons.forEach(btn => {
        if (btn.querySelector('.bi-eye')) {
            btn.addEventListener('click', function() {
                const row = this.closest('tr');
                previewAnnouncement(row);
            });
        }
    });
    
    deleteButtons.forEach(btn => {
        if (btn.querySelector('.bi-trash')) {
            btn.addEventListener('click', function() {
                const row = this.closest('tr');
                deleteAnnouncement(row);
            });
        }
    });
}

// 執行搜尋
function performSearch() {
    const keyword = document.querySelector('input[placeholder="搜尋公告標題或內容..."]').value;
    console.log(`🔍 搜尋公告: ${keyword}`);
    
    // TODO: 實作搜尋邏輯
    // 這裡應該發送 AJAX 請求到後端搜尋 API
    loadAnnouncements({ keyword });
}

// 執行篩選
function performFilter() {
    const statusFilter = document.querySelector('select[class="form-select"]:first-of-type').value;
    const typeFilter = document.querySelector('select[class="form-select"]:last-of-type').value;
    
    console.log(`🎛️ 篩選公告 - 狀態: ${statusFilter}, 類型: ${typeFilter}`);
    
    // TODO: 實作篩選邏輯
    loadAnnouncements({ status: statusFilter, type: typeFilter });
}

// 載入公告列表
function loadAnnouncements(filters = {}) {
    console.log("📋 載入公告列表", filters);
    
    // TODO: 發送 AJAX 請求載入公告數據
    // 這裡暫時使用現有的示例數據
    
    // 示例：模擬載入完成後重新綁定按鈕事件
    setTimeout(() => {
        bindRowActionButtons();
    }, 100);
}

// 顯示新增公告模態框
function showAddAnnouncementModal() {
    console.log("➕ 顯示新增公告模態框");
    
    // TODO: 實作新增公告模態框
    alert('新增公告功能開發中...');
}

// 編輯公告
function editAnnouncement(row) {
    const title = row.querySelector('strong').textContent;
    console.log(`✏️ 編輯公告: ${title}`);
    
    // TODO: 實作編輯公告功能
    alert(`編輯公告 "${title}" 功能開發中...`);
}

// 預覽公告
function previewAnnouncement(row) {
    const title = row.querySelector('strong').textContent;
    console.log(`👁️ 預覽公告: ${title}`);
    
    // TODO: 實作預覽公告功能
    alert(`預覽公告 "${title}" 功能開發中...`);
}

// 刪除公告
function deleteAnnouncement(row) {
    const title = row.querySelector('strong').textContent;
    
    if (confirm(`確定要刪除公告 "${title}" 嗎？`)) {
        console.log(`🗑️ 刪除公告: ${title}`);
        
        // TODO: 實作刪除公告功能
        // 暫時移除該行
        row.remove();
        
        // 顯示成功提示
        showToast(`公告 "${title}" 已成功刪除`, 'success');
    }
}

// 全選/取消全選
function toggleSelectAll() {
    const isChecked = this.checked;
    const checkboxes = document.querySelectorAll('tbody input[type="checkbox"]');
    
    checkboxes.forEach(checkbox => {
        checkbox.checked = isChecked;
    });
    
    console.log(`📋 ${isChecked ? '全選' : '取消全選'}所有公告`);
}

// 顯示提示訊息
function showToast(message, type = 'success') {
    // 使用 dashboard.js 中的 showToast 函數
    if (typeof window.showToast === 'function') {
        window.showToast(message, type);
    } else {
        // 備用方案
        console.log(`${type.toUpperCase()}: ${message}`);
        alert(message);
    }
}

// 初始化函數在頁面載入時自動執行
// 注意：實際初始化由 dashboard.js 中的 openTab 函數調用
console.log("📢 公告管理 JS 已載入");