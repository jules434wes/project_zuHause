/**
 * 後台訊息模板管理 JavaScript
 * 處理訊息模板的新增、編輯、刪除、搜尋等功能
 */

(() => {
    let currentPage = 1;
    let pageSize = 10;
    let currentFilters = {};

    // 初始化訊息模板管理器
    function initMessageTemplateManager() {
    console.log("🚀 初始化後台訊息模板管理功能");
    
    // 綁定事件
    bindEvents();
    
    // 載入模板列表
    loadMessageTemplates();
}

    // 綁定所有事件
    function bindEvents() {
    // 新增模板按鈕
    const addBtn = document.getElementById('addTemplateBtn');
    if (addBtn) {
        addBtn.addEventListener('click', showAddModal);
    }
    
    // 搜尋功能
    const searchBtn = document.getElementById('templateSearchBtn');
    const searchInput = document.getElementById('templateSearchInput');
    if (searchBtn) {
        searchBtn.addEventListener('click', performSearch);
    }
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                performSearch();
            }
        });
    }
    
    // 篩選功能
    const categoryFilter = document.getElementById('templateCategoryFilter');
    const statusFilter = document.getElementById('templateStatusFilter');
    if (categoryFilter) {
        categoryFilter.addEventListener('change', performFilter);
    }
    if (statusFilter) {
        statusFilter.addEventListener('change', performFilter);
    }
    
    // 重置按鈕
    const resetBtn = document.getElementById('templateResetFiltersBtn');
    if (resetBtn) {
        resetBtn.addEventListener('click', resetFilters);
    }
    
    // 表單提交
    const saveBtn = document.getElementById('saveTemplateBtn');
    if (saveBtn) {
        saveBtn.addEventListener('click', saveMessageTemplate);
    }
    
    // 全選功能
    const selectAllBtn = document.getElementById('selectAllTemplates');
    if (selectAllBtn) {
        selectAllBtn.addEventListener('change', toggleSelectAll);
    }
}

    // 載入訊息模板列表
    async function loadMessageTemplates(page = 1) {
    try {
        showLoading(true);
        
        const params = new URLSearchParams({
            page: page,
            pageSize: pageSize,
            ...currentFilters
        });
        
        const response = await fetch(`/Dashboard/GetMessageTemplates?${params}`);
        const result = await response.json();
        
        if (response.ok) {
            renderTemplateTable(result.data);
            renderPagination(result);
            currentPage = page;
        } else {
            showToast('載入模板列表失敗', 'error');
        }
    } catch (error) {
        console.error('載入模板失敗:', error);
        showToast('載入模板列表時發生錯誤', 'error');
    } finally {
        showLoading(false);
    }
}

    // 渲染模板表格
    function renderTemplateTable(templates) {
    const tbody = document.getElementById('templateTableBody');
    if (!tbody) return;
    
    if (!templates || templates.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-muted py-4">
                    <i class="bi bi-inbox"></i> 目前沒有模板資料
                </td>
            </tr>
        `;
        return;
    }
    
    tbody.innerHTML = templates.map(template => `
        <tr data-id="${template.templateID}">
            <td>
                <input type="checkbox" class="form-check-input template-checkbox" value="${template.templateID}">
            </td>
            <td>
                <strong title="${escapeHtml(template.title)}">${escapeHtml(template.title)}</strong>
            </td>
            <td>
                <span class="badge ${getCategoryBadgeClass(template.categoryCode)}">
                    ${getCategoryDisplayName(template.categoryCode)}
                </span>
            </td>
            <td>
                <div class="template-content-preview" title="${escapeHtml(template.templateContent)}">
                    ${highlightParameters(truncateText(escapeHtml(template.templateContent), 120))}
                </div>
            </td>
            <td>
                <span class="badge ${template.isActive ? 'bg-success' : 'bg-secondary'}">
                    ${template.isActive ? '啟用' : '停用'}
                </span>
            </td>
            <td>
                <small>${formatDate(template.updatedAt)}</small>
            </td>
            <td>
                <div class="btn-group btn-group-sm">
                    <button type="button" class="btn btn-outline-primary" onclick="editTemplate(${template.templateID})" title="編輯">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button type="button" class="btn btn-outline-info" onclick="previewTemplate(${template.templateID})" title="預覽">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button type="button" class="btn btn-outline-secondary" onclick="toggleTemplateStatus(${template.templateID})" title="${template.isActive ? '停用' : '啟用'}">
                        <i class="bi ${template.isActive ? 'bi-pause-circle' : 'bi-play-circle'}"></i>
                    </button>
                    <button type="button" class="btn btn-outline-danger" onclick="deleteTemplate(${template.templateID})" title="刪除">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

    // 渲染分頁
    function renderPagination(result) {
    const container = document.getElementById('paginationList');
    if (!container) return;
    
    const { page, totalPages } = result;
    
    if (totalPages <= 1) {
        container.innerHTML = '';
        return;
    }
    
    let pagination = '';
    
    // 上一頁
    pagination += `
        <li class="page-item ${page <= 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="loadMessageTemplates(${page - 1}); return false;">上一頁</a>
        </li>
    `;
    
    // 頁碼
    const start = Math.max(1, page - 2);
    const end = Math.min(totalPages, page + 2);
    
    for (let i = start; i <= end; i++) {
        pagination += `
            <li class="page-item ${i === page ? 'active' : ''}">
                <a class="page-link" href="#" onclick="loadMessageTemplates(${i}); return false;">${i}</a>
            </li>
        `;
    }
    
    // 下一頁
    pagination += `
        <li class="page-item ${page >= totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="loadMessageTemplates(${page + 1}); return false;">下一頁</a>
        </li>
    `;
    
    container.innerHTML = pagination;
}

    // 顯示新增模態框
    function showAddModal() {
    clearForm();
    document.getElementById('templateModalLabel').textContent = '新增模板';
    new bootstrap.Modal(document.getElementById('templateModal')).show();
}

    // 編輯模板
    async function editTemplate(id) {
    try {
        const response = await fetch(`/Dashboard/GetMessageTemplateById/${id}`);
        const template = await response.json();
        
        if (response.ok) {
            fillForm(template);
            document.getElementById('templateModalLabel').textContent = '編輯模板';
            new bootstrap.Modal(document.getElementById('templateModal')).show();
        } else {
            showToast('載入模板資料失敗', 'error');
        }
    } catch (error) {
        console.error('載入模板失敗:', error);
        showToast('載入模板資料時發生錯誤', 'error');
    }
}

// 預覽模板
async function previewTemplate(id) {
    try {
        const response = await fetch(`/Dashboard/GetMessageTemplateById/${id}`);
        const template = await response.json();
        
        if (response.ok) {
            const previewContent = document.getElementById('previewContent');
            previewContent.innerHTML = `
                <div class="card">
                    <div class="card-header">
                        <h5 class="mb-0">${escapeHtml(template.title)}</h5>
                        <div class="small text-muted">
                            分類：${getCategoryDisplayName(template.categoryCode)} | 
                            狀態：${template.isActive ? '啟用' : '停用'} |
                            更新時間：${formatDate(template.updatedAt)}
                        </div>
                    </div>
                    <div class="card-body">
                        <div style="white-space: pre-wrap; line-height: 1.6;">
                            ${highlightParameters(escapeHtml(template.templateContent))}
                        </div>
                    </div>
                    <div class="card-footer text-muted">
                        <small>
                            <strong>使用說明：</strong>使用 <code>{參數名稱}</code> 格式的內容在實際使用時會被替換為對應的動態值
                        </small>
                    </div>
                </div>
            `;
            new bootstrap.Modal(document.getElementById('previewModal')).show();
        } else {
            showToast('載入模板資料失敗', 'error');
        }
    } catch (error) {
        console.error('載入模板失敗:', error);
        showToast('載入模板資料時發生錯誤', 'error');
    }
}

// 切換模板狀態
async function toggleTemplateStatus(id) {
    try {
        const response = await fetch('/Dashboard/ToggleMessageTemplateStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(id)
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showToast(`模板已${result.isActive ? '啟用' : '停用'}`, 'success');
            loadMessageTemplates(currentPage);
        } else {
            showToast('切換模板狀態失敗', 'error');
        }
    } catch (error) {
        console.error('切換狀態失敗:', error);
        showToast('操作時發生錯誤', 'error');
    }
}

// 刪除模板
async function deleteTemplate(id) {
    if (!confirm('確定要刪除這個模板嗎？此操作無法復原。')) {
        return;
    }
    
    try {
        const response = await fetch('/Dashboard/DeleteMessageTemplate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(id)
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showToast('模板已刪除', 'success');
            loadMessageTemplates(currentPage);
        } else {
            showToast('刪除模板失敗', 'error');
        }
    } catch (error) {
        console.error('刪除失敗:', error);
        showToast('刪除時發生錯誤', 'error');
    }
}

// 儲存模板
async function saveMessageTemplate() {
    const form = document.getElementById('templateForm');
    const formData = new FormData(form);
    
    // 驗證必填欄位
    if (!formData.get('title') || !formData.get('templateContent') || !formData.get('categoryCode')) {
        showToast('請填寫所有必填欄位', 'error');
        return;
    }
    
    // 準備資料
    const data = {
        categoryCode: formData.get('categoryCode'),
        title: formData.get('title'),
        templateContent: formData.get('templateContent'),
        isActive: formData.has('isActive')
    };
    
    const isEdit = formData.get('templateID');
    if (isEdit) {
        data.templateID = parseInt(isEdit);
    }
    
    try {
        const url = isEdit ? '/Dashboard/UpdateMessageTemplate' : '/Dashboard/CreateMessageTemplate';
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showToast(`模板${isEdit ? '更新' : '新增'}成功`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('templateModal')).hide();
            loadMessageTemplates(currentPage);
        } else {
            const errorMessage = result.message || '操作失敗';
            showToast(errorMessage, 'error');
        }
    } catch (error) {
        console.error('儲存失敗:', error);
        showToast('儲存時發生錯誤', 'error');
    }
}

// 搜尋功能
function performSearch() {
    const keyword = document.getElementById('templateSearchInput').value.trim();
    
    // 更新關鍵字篩選，保留其他篩選條件
    if (keyword) {
        currentFilters.keyword = keyword;
    } else {
        delete currentFilters.keyword;
    }
    
    currentPage = 1;
    loadMessageTemplates(1);
}

// 篩選功能
function performFilter() {
    const category = document.getElementById('templateCategoryFilter').value;
    const status = document.getElementById('templateStatusFilter').value;
    
    // 更新篩選條件，保留關鍵字搜尋
    if (category) {
        currentFilters.category = category;
    } else {
        delete currentFilters.category;
    }
    
    if (status) {
        currentFilters.status = status;
    } else {
        delete currentFilters.status;
    }
    
    currentPage = 1;
    loadMessageTemplates(1);
}

// 重置篩選功能
function resetFilters() {
    // 清空所有篩選條件
    currentFilters = {};
    currentPage = 1;
    
    // 重置表單元素
    const searchInput = document.getElementById('templateSearchInput');
    const categoryFilter = document.getElementById('templateCategoryFilter');
    const statusFilter = document.getElementById('templateStatusFilter');
    
    if (searchInput) {
        searchInput.value = '';
    }
    if (categoryFilter) {
        categoryFilter.value = '';
    }
    if (statusFilter) {
        statusFilter.value = '';
    }
    
    // 重新載入全部資料
    loadMessageTemplates(1);
    
    console.log('🔄 篩選條件已重置');
}

// 全選功能
function toggleSelectAll() {
    const selectAll = document.getElementById('selectAllTemplates');
    const checkboxes = document.querySelectorAll('.template-checkbox');
    
    checkboxes.forEach(checkbox => {
        checkbox.checked = selectAll.checked;
    });
}

// 清空表單
function clearForm() {
    const form = document.getElementById('templateForm');
    form.reset();
    document.getElementById('templateId').value = '';
    document.getElementById('templateIsActive').checked = true;
}

// 填充表單
function fillForm(template) {
    document.getElementById('templateId').value = template.templateID;
    document.getElementById('templateTitle').value = template.title;
    document.getElementById('templateCategory').value = template.categoryCode;
    document.getElementById('templateContent').value = template.templateContent;
    document.getElementById('templateIsActive').checked = template.isActive;
}

// 顯示載入狀態
function showLoading(show) {
    const tbody = document.getElementById('templateTableBody');
    if (!tbody) return;
    
    if (show) {
        tbody.innerHTML = `
            <tr id="loadingRow">
                <td colspan="7" class="text-center py-4">
                    <div class="spinner-border spinner-border-sm" role="status">
                        <span class="visually-hidden">載入中...</span>
                    </div>
                    載入中，請稍候...
                </td>
            </tr>
        `;
    }
}

// 工具函數
function getCategoryBadgeClass(category) {
    const classes = {
        'SYSTEM_MESSAGE': 'bg-danger',
        'LEASE_SERVICE': 'bg-primary',
        'PROPERTY_SERVICE': 'bg-success',
        'FURNITURE_SERVICE': 'bg-warning',
        'PLATFORM_ANNOUNCE': 'bg-info'
    };
    return classes[category] || 'bg-secondary';
}

function getCategoryDisplayName(category) {
    const names = {
        'SYSTEM_MESSAGE': '系統訊息',
        'LEASE_SERVICE': '租約客服',
        'PROPERTY_SERVICE': '屋源客服',
        'FURNITURE_SERVICE': '傢俱客服',
        'PLATFORM_ANNOUNCE': '平台公告'
    };
    return names[category] || category;
}

function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleString('zh-TW', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function truncateText(text, maxLength) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}

// 高亮顯示參數
function highlightParameters(text) {
    if (!text) return '';
    return text.replace(/\{([^}]+)\}/g, '<span class="template-parameter">{$1}</span>');
}

    // 在 dashboard.js 中定義的 showToast 函數
    // 如果不存在，使用備用方案
    if (typeof showToast === 'undefined') {
        window.showToast = function(message, type = 'success') {
            console.log(`${type.toUpperCase()}: ${message}`);
            alert(message);
        };
    }

    // 暴露需要在HTML中調用的函數到全域
    window.initMessageTemplateManager = initMessageTemplateManager;
    window.editTemplate = editTemplate;
    window.previewTemplate = previewTemplate;
    window.toggleTemplateStatus = toggleTemplateStatus;
    window.deleteTemplate = deleteTemplate;
    window.loadMessageTemplates = loadMessageTemplates;

    console.log("📝 後台訊息模板管理 JS 已載入");
})();