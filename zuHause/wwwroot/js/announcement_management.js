/**
 * 公告管理 JavaScript
 * 處理公告的新增、編輯、刪除、搜尋等功能
 */

(() => {
    let currentPage = 1;
    let pageSize = 10;
    let currentFilters = {};

    // 初始化公告管理器
    function initAnnouncementManager() {
        console.log("🚀 初始化公告管理功能");
        
        // 綁定事件
        bindEvents();
        
        // 載入公告列表
        loadAnnouncements();
    }

    // 綁定所有事件
    function bindEvents() {
        // 新增公告按鈕
        const addBtn = document.getElementById('addAnnouncementBtn');
        if (addBtn) {
            addBtn.addEventListener('click', showAddModal);
        }
        
        // 搜尋功能
        const searchBtn = document.getElementById('announcementSearchBtn');
        const searchInput = document.getElementById('announcementSearchInput');
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
        const statusFilter = document.getElementById('announcementStatusFilter');
        const scopeFilter = document.getElementById('announcementScopeFilter');
        if (statusFilter) {
            statusFilter.addEventListener('change', performFilter);
        }
        if (scopeFilter) {
            scopeFilter.addEventListener('change', performFilter);
        }
        
        // 重置按鈕
        const resetBtn = document.getElementById('announcementResetFiltersBtn');
        if (resetBtn) {
            resetBtn.addEventListener('click', resetFilters);
        }
        
        // 表單提交
        const saveBtn = document.getElementById('saveAnnouncementBtn');
        if (saveBtn) {
            saveBtn.addEventListener('click', saveAnnouncement);
        }
        
        // 模板選擇功能
        const templateSelect = document.getElementById('announcementTemplateSelect');
        if (templateSelect) {
            templateSelect.addEventListener('change', insertTemplate);
        }
        
        // 重置表單按鈕
        const resetFormBtn = document.getElementById('announcementResetFormBtn');
        if (resetFormBtn) {
            resetFormBtn.addEventListener('click', resetAnnouncementForm);
        }
    }

    // 載入公告列表
    async function loadAnnouncements(page = 1) {
    try {
        showLoading(true);
        
        const params = new URLSearchParams({
            page: page,
            pageSize: pageSize,
            ...currentFilters
        });
        
        const response = await fetch(`/Dashboard/GetAnnouncements?${params}`);
        const result = await response.json();
        
        if (response.ok) {
            renderAnnouncementTable(result.data);
            renderPagination(result);
            currentPage = page;
        } else {
            showToast('載入公告列表失敗', 'error');
        }
    } catch (error) {
        console.error('載入公告失敗:', error);
        showToast('載入公告列表時發生錯誤', 'error');
    } finally {
        showLoading(false);
    }
}

    // 渲染公告表格
    function renderAnnouncementTable(announcements) {
    const tbody = document.getElementById('announcementTableBody');
    if (!tbody) return;
    
    if (!announcements || announcements.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    <i class="bi bi-inbox"></i> 目前沒有公告資料
                </td>
            </tr>
        `;
        return;
    }
    
    tbody.innerHTML = announcements.map(announcement => `
        <tr data-id="${announcement.siteMessagesId}">
            <td>
                <input type="checkbox" class="form-check-input" value="${announcement.siteMessagesId}">
            </td>
            <td>
                <strong class="d-block" title="${escapeHtml(announcement.title)}">${truncateText(escapeHtml(announcement.title), 50)}</strong>
                <small class="text-muted announcement-content-preview" title="${escapeHtml(announcement.siteMessageContent)}">${truncateText(escapeHtml(announcement.siteMessageContent), 80)}</small>
            </td>
            <td>
                <span class="badge ${getScopeBadgeClass(announcement.moduleScope)}">
                    ${getScopeDisplayName(announcement.moduleScope)}
                </span>
            </td>
            <td>
                <span class="badge ${announcement.isActive ? 'bg-success' : 'bg-secondary'}">
                    ${announcement.isActive ? '已發布' : '未發布'}
                </span>
            </td>
            <td>
                <small>${formatDate(announcement.createdAt)}</small>
            </td>
            <td>
                <small>
                    ${formatDate(announcement.startAt)}<br>
                    ${announcement.endAt ? `至 ${formatDate(announcement.endAt)}` : '無期限'}
                </small>
            </td>
            <td>
                <div class="btn-group btn-group-sm">
                    <button type="button" class="btn btn-outline-primary" onclick="editAnnouncement(${announcement.siteMessagesId})" title="編輯">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button type="button" class="btn btn-outline-info" onclick="previewAnnouncement(${announcement.siteMessagesId})" title="預覽">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button type="button" class="btn btn-outline-secondary" onclick="toggleAnnouncementStatus(${announcement.siteMessagesId})" title="${announcement.isActive ? '停用' : '啟用'}">
                        <i class="bi ${announcement.isActive ? 'bi-pause-circle' : 'bi-play-circle'}"></i>
                    </button>
                    <button type="button" class="btn btn-outline-danger" onclick="deleteAnnouncement(${announcement.siteMessagesId})" title="刪除">
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
            <a class="page-link" href="#" onclick="loadAnnouncements(${page - 1}); return false;">上一頁</a>
        </li>
    `;
    
    // 頁碼
    const start = Math.max(1, page - 2);
    const end = Math.min(totalPages, page + 2);
    
    for (let i = start; i <= end; i++) {
        pagination += `
            <li class="page-item ${i === page ? 'active' : ''}">
                <a class="page-link" href="#" onclick="loadAnnouncements(${i}); return false;">${i}</a>
            </li>
        `;
    }
    
    // 下一頁
    pagination += `
        <li class="page-item ${page >= totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="loadAnnouncements(${page + 1}); return false;">下一頁</a>
        </li>
    `;
    
    container.innerHTML = pagination;
}

    // 顯示新增模態框
    function showAddModal() {
    clearForm();
    document.getElementById('announcementModalLabel').textContent = '新增公告';
    
    // 顯示模板選擇區塊並載入模板選項
    const templateSection = document.getElementById('announcementTemplateSection');
    if (templateSection) {
        templateSection.style.display = 'block';
        loadTemplateOptions();
    }
    
    new bootstrap.Modal(document.getElementById('announcementModal')).show();
}

    // 載入平台公告模板選項
    async function loadTemplateOptions() {
        try {
            const response = await fetch('/Dashboard/GetPlatformAnnounceTemplates');
            const result = await response.json();
            
            if (response.ok && result.success) {
                const templateSelect = document.getElementById('announcementTemplateSelect');
                
                // 清空現有選項，保留預設選項
                templateSelect.innerHTML = '<option value="">請選擇模板...</option>';
                
                // 添加模板選項
                result.data.forEach(template => {
                    const option = document.createElement('option');
                    option.value = template.templateId;
                    option.textContent = template.title;
                    templateSelect.appendChild(option);
                });
            } else {
                console.error('載入模板選項失敗:', result.message);
            }
        } catch (error) {
            console.error('載入模板選項時發生錯誤:', error);
        }
    }
    
    // 插入選中的模板
    async function insertTemplate() {
        const templateSelect = document.getElementById('announcementTemplateSelect');
        const templateId = templateSelect.value;
        
        if (!templateId) {
            return;
        }
        
        try {
            const response = await fetch('/Dashboard/GetPlatformAnnounceTemplates');
            const result = await response.json();
            
            if (response.ok && result.success) {
                const selectedTemplate = result.data.find(t => t.templateId == templateId);
                
                if (selectedTemplate) {
                    // 填入標題和內容
                    document.getElementById('announcementTitle').value = selectedTemplate.title;
                    document.getElementById('announcementContent').value = selectedTemplate.templateContent;
                    
                    console.log('📝 已插入模板:', selectedTemplate.title);
                }
            }
        } catch (error) {
            console.error('插入模板時發生錯誤:', error);
            showToast('插入模板時發生錯誤', 'error');
        }
    }
    
    // 重置公告表單
    function resetAnnouncementForm() {
        const form = document.getElementById('announcementForm');
        form.reset();
        
        // 重置隱藏欄位
        document.getElementById('announcementId').value = '';
        
        // 重置模板選擇
        const templateSelect = document.getElementById('announcementTemplateSelect');
        if (templateSelect) {
            templateSelect.value = '';
        }
        
        // 重置checkbox預設狀態
        document.getElementById('announcementIsActive').checked = true;
        
        console.log('🔄 公告表單已重置');
    }

    // 編輯公告
    async function editAnnouncement(id) {
    try {
        const response = await fetch(`/Dashboard/GetAnnouncementById/${id}`);
        const announcement = await response.json();
        
        if (response.ok) {
            fillForm(announcement);
            document.getElementById('announcementModalLabel').textContent = '編輯公告';
            
            // 隱藏模板選擇區塊（編輯模式不需要）
            const templateSection = document.getElementById('announcementTemplateSection');
            if (templateSection) {
                templateSection.style.display = 'none';
            }
            
            new bootstrap.Modal(document.getElementById('announcementModal')).show();
        } else {
            showToast('載入公告資料失敗', 'error');
        }
    } catch (error) {
        console.error('載入公告失敗:', error);
        showToast('載入公告資料時發生錯誤', 'error');
    }
}

    // 預覽公告
    async function previewAnnouncement(id) {
    try {
        const response = await fetch(`/Dashboard/GetAnnouncementById/${id}`);
        const announcement = await response.json();
        
        if (response.ok) {
            const previewContent = document.getElementById('previewContent');
            previewContent.innerHTML = `
                <div class="card">
                    <div class="card-header">
                        <h5 class="mb-0">${escapeHtml(announcement.title)}</h5>
                        <div class="small text-muted">
                            範圍：${getScopeDisplayName(announcement.moduleScope)} | 
                            狀態：${announcement.isActive ? '已發布' : '未發布'} |
                            建立時間：${formatDate(announcement.createdAt)}
                        </div>
                    </div>
                    <div class="card-body">
                        <div style="white-space: pre-wrap;">${escapeHtml(announcement.siteMessageContent)}</div>
                        ${announcement.attachmentUrl ? `<hr><div><strong>附件：</strong><a href="${announcement.attachmentUrl}" target="_blank">${announcement.attachmentUrl}</a></div>` : ''}
                    </div>
                    <div class="card-footer text-muted">
                        <small>
                            生效時間：${formatDate(announcement.startAt)}${announcement.endAt ? ` 至 ${formatDate(announcement.endAt)}` : ' (無期限)'}
                        </small>
                    </div>
                </div>
            `;
            new bootstrap.Modal(document.getElementById('previewModal')).show();
        } else {
            showToast('載入公告資料失敗', 'error');
        }
    } catch (error) {
        console.error('載入公告失敗:', error);
        showToast('載入公告資料時發生錯誤', 'error');
    }
}

    // 切換公告狀態
    async function toggleAnnouncementStatus(id) {
    try {
        const response = await fetch('/Dashboard/ToggleAnnouncementStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(id)
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showToast(`公告已${result.isActive ? '啟用' : '停用'}`, 'success');
            loadAnnouncements(currentPage);
        } else {
            showToast('切換公告狀態失敗', 'error');
        }
    } catch (error) {
        console.error('切換狀態失敗:', error);
        showToast('操作時發生錯誤', 'error');
    }
}

    // 刪除公告
    async function deleteAnnouncement(id) {
    if (!confirm('確定要刪除這筆公告嗎？此操作無法復原。')) {
        return;
    }
    
    try {
        const response = await fetch('/Dashboard/DeleteAnnouncement', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(id)
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showToast('公告已刪除', 'success');
            loadAnnouncements(currentPage);
        } else {
            showToast('刪除公告失敗', 'error');
        }
    } catch (error) {
        console.error('刪除失敗:', error);
        showToast('刪除時發生錯誤', 'error');
    }
}

    // 儲存公告
    async function saveAnnouncement() {
    const form = document.getElementById('announcementForm');
    const formData = new FormData(form);
    
    // 驗證必填欄位
    if (!formData.get('title') || !formData.get('siteMessageContent') || !formData.get('moduleScope')) {
        showToast('請填寫所有必填欄位', 'error');
        return;
    }
    
    // 準備資料
    const data = {
        title: formData.get('title'),
        siteMessageContent: formData.get('siteMessageContent'),
        moduleScope: formData.get('moduleScope'),
        displayOrder: parseInt(formData.get('displayOrder')) || 1,
        startAt: formData.get('startAt') || null,
        endAt: formData.get('endAt') || null,
        isActive: formData.has('isActive'),
        attachmentUrl: formData.get('attachmentUrl') || null
    };
    
    const isEdit = formData.get('siteMessagesId');
    if (isEdit) {
        data.siteMessagesId = parseInt(isEdit);
    }
    
    try {
        const url = isEdit ? '/Dashboard/UpdateAnnouncement' : '/Dashboard/CreateAnnouncement';
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showToast(`公告${isEdit ? '更新' : '新增'}成功`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('announcementModal')).hide();
            loadAnnouncements(currentPage);
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
    const keyword = document.getElementById('announcementSearchInput').value.trim();
    
    // 更新關鍵字篩選，保留其他篩選條件
    if (keyword) {
        currentFilters.keyword = keyword;
    } else {
        delete currentFilters.keyword;
    }
    
    currentPage = 1;
    loadAnnouncements(1);
}

    // 篩選功能
    function performFilter() {
    const status = document.getElementById('announcementStatusFilter').value;
    const scope = document.getElementById('announcementScopeFilter').value;
    
    // 更新篩選條件，保留關鍵字搜尋
    if (status) {
        currentFilters.status = status;
    } else {
        delete currentFilters.status;
    }
    
    if (scope) {
        currentFilters.scope = scope;
    } else {
        delete currentFilters.scope;
    }
    
    currentPage = 1;
    loadAnnouncements(1);
}

    // 重置篩選功能
    function resetFilters() {
    // 清空所有篩選條件
    currentFilters = {};
    currentPage = 1;
    
    // 重置表單元素
    const searchInput = document.getElementById('announcementSearchInput');
    const statusFilter = document.getElementById('announcementStatusFilter');
    const scopeFilter = document.getElementById('announcementScopeFilter');
    
    if (searchInput) {
        searchInput.value = '';
    }
    if (statusFilter) {
        statusFilter.value = '';
    }
    if (scopeFilter) {
        scopeFilter.value = '';
    }
    
    // 重新載入全部資料
    loadAnnouncements(1);
    
    console.log('🔄 篩選條件已重置');
}

    // 清空表單
    function clearForm() {
    const form = document.getElementById('announcementForm');
    form.reset();
    document.getElementById('announcementId').value = '';
    document.getElementById('announcementIsActive').checked = true;
}

    // 填充表單
    function fillForm(announcement) {
    document.getElementById('announcementId').value = announcement.siteMessagesId;
    document.getElementById('announcementTitle').value = announcement.title;
    document.getElementById('announcementScope').value = announcement.moduleScope;
    document.getElementById('announcementDisplayOrder').value = announcement.displayOrder;
    document.getElementById('announcementContent').value = announcement.siteMessageContent;
    document.getElementById('announcementStartAt').value = announcement.startAt ? formatDateTimeLocal(announcement.startAt) : '';
    document.getElementById('announcementEndAt').value = announcement.endAt ? formatDateTimeLocal(announcement.endAt) : '';
    document.getElementById('announcementIsActive').checked = announcement.isActive;
    document.getElementById('announcementAttachmentUrl').value = announcement.attachmentUrl || '';
}

    // 顯示載入狀態
    function showLoading(show) {
    const tbody = document.getElementById('announcementTableBody');
    if (!tbody) return;
    
    if (show) {
        tbody.innerHTML = `
            <tr id="loadingRow">
                <td colspan="6" class="text-center py-4">
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
    function getScopeBadgeClass(scope) {
    const classes = {
        'TENANT': 'bg-primary',
        'LANDLORD': 'bg-success',
        'FURNITURE': 'bg-warning',
        'COMMON': 'bg-info'
    };
    return classes[scope] || 'bg-secondary';
}

    function getScopeDisplayName(scope) {
    const names = {
        'TENANT': '租戶端',
        'LANDLORD': '房東端',
        'FURNITURE': '家具端',
        'COMMON': '通用'
    };
    return names[scope] || scope;
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

    function formatDateTimeLocal(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toISOString().slice(0, 16);
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

    // 在 dashboard.js 中定義的 showToast 函數
    // 如果不存在，使用備用方案
    if (typeof showToast === 'undefined') {
        window.showToast = function(message, type = 'success') {
            console.log(`${type.toUpperCase()}: ${message}`);
            alert(message);
        };
    }

    // 暴露需要在HTML中調用的函數到全域
    window.initAnnouncementManager = initAnnouncementManager;
    window.editAnnouncement = editAnnouncement;
    window.previewAnnouncement = previewAnnouncement;
    window.toggleAnnouncementStatus = toggleAnnouncementStatus;
    window.deleteAnnouncement = deleteAnnouncement;
    window.loadAnnouncements = loadAnnouncements;
    window.resetAnnouncementForm = resetAnnouncementForm;

    console.log("📢 公告管理 JS 已載入");
})();