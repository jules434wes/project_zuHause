/**
 * 房源投訴管理 JavaScript 文件
 * 處理投訴列表的 AJAX 載入、搜尋、分頁和詳情顯示
 */

// 全域變數
let currentPage = 1;
let isLoading = false;

/**
 * 頁面初始化
 */
document.addEventListener('DOMContentLoaded', function() {
    console.log('Property complaints page initialized');
    
    // 初始化搜尋表單事件
    initializeSearchForm();
    
    // 不自動載入資料，使用 Razor 渲染的初始資料
    // performSearch(1); // 註解掉，避免覆蓋 Server-side 渲染的資料
    
    // 更新狀態統計（基於現有資料）
    updateStatusSummaryFromTable();
});

/**
 * 初始化搜尋表單事件
 */
function initializeSearchForm() {
    // 搜尋按鈕事件
    const searchButtons = document.querySelectorAll('button[onclick="performSearch()"]');
    searchButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            performSearch(1);
        });
    });
    
    // Enter 鍵搜尋
    const searchInput = document.getElementById('searchKeyword');
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                performSearch(1);
            }
        });
    }
    
    // 狀態篩選變更事件
    const statusFilter = document.getElementById('statusFilter');
    if (statusFilter) {
        statusFilter.addEventListener('change', function() {
            performSearch(1);
        });
    }
}

/**
 * 執行搜尋 - 全域函數，供 HTML onclick 使用
 */
function performSearch(page = 1) {
    if (isLoading) {
        console.log('Search already in progress, skipping...');
        return;
    }
    
    isLoading = true;
    currentPage = page;
    
    // 顯示載入狀態
    showLoadingState();
    
    const formData = new FormData();
    formData.append('keyword', document.getElementById('searchKeyword')?.value || '');
    formData.append('searchField', document.getElementById('searchField')?.value || 'all');
    formData.append('status', document.getElementById('statusFilter')?.value || '');
    formData.append('dateStart', document.getElementById('dateStart')?.value || '');
    formData.append('dateEnd', document.getElementById('dateEnd')?.value || '');
    formData.append('page', page);
    formData.append('pageSize', 10);
    
    console.log('Performing search with params:', {
        keyword: formData.get('keyword'),
        searchField: formData.get('searchField'),
        status: formData.get('status'),
        dateStart: formData.get('dateStart'),
        dateEnd: formData.get('dateEnd'),
        page: formData.get('page'),
        pageSize: formData.get('pageSize')
    });
    
    fetch('/Admin/FilterComplaints', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        console.log('Response status:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Search response:', data);
        
        if (data.success) {
            renderComplaintsTable(data.data || []);
            renderPagination(data.currentPage || 1, data.totalPages || 0, data.totalCount || 0);
            updateTotalCount(data.totalCount || 0);
            
            // 如果有 message，顯示在控制台
            if (data.message) {
                console.log('Server message:', data.message);
            }
        } else {
            console.error('Search failed:', data.message);
            showErrorMessage('搜尋失敗：' + (data.message || '未知錯誤'));
            renderComplaintsTable([]);
        }
    })
    .catch(error => {
        console.error('Search error:', error);
        showErrorMessage('搜尋時發生錯誤：' + error.message);
        renderComplaintsTable([]);
    })
    .finally(() => {
        isLoading = false;
        hideLoadingState();
    });
}

/**
 * 顯示載入狀態
 */
function showLoadingState() {
    const tbody = document.getElementById('complaintsTableBody');
    if (tbody) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center py-4">
                    <div class="spinner-border spinner-border-sm text-primary me-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    載入中...
                </td>
            </tr>
        `;
    }
}

/**
 * 隱藏載入狀態
 */
function hideLoadingState() {
    // 載入狀態由 renderComplaintsTable 處理
}

/**
 * 顯示錯誤訊息
 */
function showErrorMessage(message) {
    // 可以改為使用 Bootstrap Toast 或其他通知機制
    alert(message);
}

/**
 * 渲染投訴表格 - 全域函數，供其他腳本使用
 */
function renderComplaintsTable(complaints) {
    const tbody = document.getElementById('complaintsTableBody');
    
    if (!tbody) {
        console.error('complaintsTableBody element not found');
        return;
    }
    
    console.log('Rendering complaints table with', complaints.length, 'items');
    
    if (!complaints || complaints.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    <i class="bi bi-inbox me-2"></i>查無投訴記錄
                </td>
            </tr>
        `;
        return;
    }
    
    tbody.innerHTML = complaints.map(complaint => {
        // 安全地處理所有可能為 null/undefined 的值
        const complaintId = complaint.ComplaintId || 0;
        const complaintIdDisplay = complaint.ComplaintIdDisplay || complaint.ComplaintId ? `CMPL-${String(complaint.ComplaintId).padStart(4, '0')}` : 'N/A';
        const complainantName = complaint.ComplainantName || '未知用戶';
        const complainantId = complaint.ComplainantId || '';
        const propertyTitle = complaint.PropertyTitle || '未知房源';
        const propertyId = complaint.PropertyId || '';
        const landlordName = complaint.LandlordName || '未知房東';
        const landlordId = complaint.LandlordId || '';
        const complaintContent = complaint.ComplaintContent || '';
        const summary = complaint.Summary || complaintContent || '無內容';
        const status = complaint.Status || 'UNKNOWN';
        const statusDisplay = complaint.StatusDisplay || getStatusDisplay(status);
        
        // 狀態徽章樣式
        const badgeClass = status === 'OPEN' ? 'bg-danger' : 
                          status === 'RESOLVED' ? 'bg-success' : 
                          status === 'CLOSED' ? 'bg-secondary' : 'bg-dark';
        
        // 安全地處理日期格式
        let formattedDate = '無資料';
        try {
            if (complaint.CreatedAt) {
                const createdDate = new Date(complaint.CreatedAt);
                if (!isNaN(createdDate.getTime())) {
                    formattedDate = createdDate.toLocaleString('zh-TW', {
                        year: 'numeric', 
                        month: '2-digit', 
                        day: '2-digit', 
                        hour: '2-digit', 
                        minute: '2-digit'
                    });
                }
            }
        } catch (error) {
            console.warn('Date formatting error:', error);
        }
        
        return `
            <tr>
                <td><strong class="text-primary">${complaintIdDisplay}</strong></td>
                <td><a href="/Admin/admin_userDetails/${complainantId}" class="text-decoration-none">${complainantName}</a></td>
                <td><a href="/Admin/admin_propertyDetails/${propertyId}" class="text-decoration-none">${propertyTitle}</a></td>
                <td><a href="/Admin/admin_userDetails/${landlordId}" class="text-decoration-none">${landlordName}</a></td>
                <td><span title="${complaintContent}">${summary}</span></td>
                <td><span class="badge ${badgeClass}">${statusDisplay}</span></td>
                <td>${formattedDate}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="showComplaintDetails(${complaintId})" title="檢視詳情">
                        <i class="bi bi-eye"></i>
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

/**
 * 取得狀態顯示文字
 */
function getStatusDisplay(status) {
    switch (status) {
        case 'OPEN': return '處理中';
        case 'RESOLVED': return '已處理';
        case 'CLOSED': return '已關閉';
        default: return '未知';
    }
}

/**
 * 渲染分頁 - 全域函數，供其他腳本使用
 */
function renderPagination(currentPage, totalPages, totalCount) {
    const paginationDiv = document.getElementById('pagination');
    
    if (!paginationDiv) {
        console.error('pagination element not found');
        return;
    }
    
    if (totalPages <= 1) {
        paginationDiv.innerHTML = '';
        return;
    }
    
    const pageSize = 10;
    const startItem = (currentPage - 1) * pageSize + 1;
    const endItem = Math.min(currentPage * pageSize, totalCount);
    
    let paginationHTML = `
        <div class="d-flex justify-content-between align-items-center">
            <div class="text-muted">
                顯示第 ${startItem}-${endItem} 筆，共 ${totalCount} 筆投訴記錄
            </div>
            <ul class="pagination mb-0">
    `;
    
    // 上一頁
    if (currentPage > 1) {
        paginationHTML += `<li class="page-item"><a class="page-link" href="#" onclick="event.preventDefault(); performSearch(${currentPage - 1})">上一頁</a></li>`;
    } else {
        paginationHTML += `<li class="page-item disabled"><span class="page-link">上一頁</span></li>`;
    }
    
    // 頁碼
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);
    
    for (let i = startPage; i <= endPage; i++) {
        if (i === currentPage) {
            paginationHTML += `<li class="page-item active"><span class="page-link">${i}</span></li>`;
        } else {
            paginationHTML += `<li class="page-item"><a class="page-link" href="#" onclick="event.preventDefault(); performSearch(${i})">${i}</a></li>`;
        }
    }
    
    // 下一頁
    if (currentPage < totalPages) {
        paginationHTML += `<li class="page-item"><a class="page-link" href="#" onclick="event.preventDefault(); performSearch(${currentPage + 1})">下一頁</a></li>`;
    } else {
        paginationHTML += `<li class="page-item disabled"><span class="page-link">下一頁</span></li>`;
    }
    
    paginationHTML += `
            </ul>
        </div>
    `;
    
    paginationDiv.innerHTML = paginationHTML;
}

/**
 * 更新總數顯示
 */
function updateTotalCount(count) {
    const totalCountElement = document.getElementById('totalCount');
    if (totalCountElement) {
        totalCountElement.textContent = count;
    }
}

/**
 * 重設搜尋 - 全域函數，供 HTML onclick 使用
 */
function resetSearch() {
    const searchKeyword = document.getElementById('searchKeyword');
    const searchField = document.getElementById('searchField');
    const statusFilter = document.getElementById('statusFilter');
    const dateStart = document.getElementById('dateStart');
    const dateEnd = document.getElementById('dateEnd');
    
    if (searchKeyword) searchKeyword.value = '';
    if (searchField) searchField.selectedIndex = 0;
    if (statusFilter) statusFilter.selectedIndex = 0;
    if (dateStart) dateStart.value = '';
    if (dateEnd) dateEnd.value = '';
    
    performSearch(1);
}

/**
 * 更新狀態統計 - 全域函數，供其他腳本使用
 */
function updateStatusSummary() {
    // 透過 AJAX 取得實際的統計資料
    fetch('/Admin/FilterComplaints', {
        method: 'POST',
        body: new FormData() // 空的 FormData 以取得所有資料
    })
    .then(response => response.json())
    .then(data => {
        if (data.success && data.data) {
            const complaints = data.data;
            const openCount = complaints.filter(c => c.Status === 'OPEN').length;
            const resolvedCount = complaints.filter(c => c.Status === 'RESOLVED').length;
            const closedCount = complaints.filter(c => c.Status === 'CLOSED').length;
            
            const openCountElement = document.getElementById('openCount');
            const resolvedCountElement = document.getElementById('resolvedCount');
            const closedCountElement = document.getElementById('closedCount');
            
            if (openCountElement) openCountElement.textContent = openCount;
            if (resolvedCountElement) resolvedCountElement.textContent = resolvedCount;
            if (closedCountElement) closedCountElement.textContent = closedCount;
        }
    })
    .catch(error => {
        console.error('Error updating status summary:', error);
    });
}

/**
 * 更新狀態統計（基於現有表格資料）- 供初始化使用
 */
function updateStatusSummaryFromTable() {
    // 計算當前表格中顯示的各種狀態數量
    const rows = document.querySelectorAll('#complaintsTableBody tr');
    let openCount = 0, resolvedCount = 0, closedCount = 0;
    
    rows.forEach(row => {
        const statusBadge = row.querySelector('.badge');
        if (statusBadge) {
            const statusText = statusBadge.textContent.trim();
            if (statusText === '處理中') openCount++;
            else if (statusText === '已處理') resolvedCount++;
            else if (statusText === '已關閉') closedCount++;
        }
    });
    
    // 更新顯示
    const openCountElement = document.getElementById('openCount');
    const resolvedCountElement = document.getElementById('resolvedCount');
    const closedCountElement = document.getElementById('closedCount');
    
    if (openCountElement) openCountElement.textContent = openCount;
    if (resolvedCountElement) resolvedCountElement.textContent = resolvedCount;
    if (closedCountElement) closedCountElement.textContent = closedCount;
}

// 使函數在全域可用，供 HTML onclick 屬性使用
window.performSearch = performSearch;
window.resetSearch = resetSearch;
window.renderComplaintsTable = renderComplaintsTable;
window.renderPagination = renderPagination;
window.updateStatusSummary = updateStatusSummary;
window.updateStatusSummaryFromTable = updateStatusSummaryFromTable;