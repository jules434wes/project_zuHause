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
    // 搜尋表單提交事件
    const searchForm = document.getElementById('searchForm');
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            e.preventDefault();
            performSearch(1);
        });
    }
    
    // 快速搜尋按鈕事件
    const quickSearchBtn = document.getElementById('quickSearchBtn');
    if (quickSearchBtn) {
        quickSearchBtn.addEventListener('click', function(e) {
            e.preventDefault();
            performSearch(1);
        });
    }
    
    // 重設按鈕事件
    const resetBtn = document.getElementById('resetBtn');
    if (resetBtn) {
        resetBtn.addEventListener('click', function(e) {
            e.preventDefault();
            resetSearch();
        });
    }
    
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
    
    // 搜尋欄位選擇變更事件
    const searchField = document.getElementById('searchField');
    if (searchField) {
        searchField.addEventListener('change', function() {
            // 只有在有關鍵字時才觸發搜尋
            const keyword = document.getElementById('searchKeyword')?.value?.trim();
            if (keyword) {
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
    
    // 日期篩選變更事件
    const dateStart = document.getElementById('dateStart');
    const dateEnd = document.getElementById('dateEnd');
    
    if (dateStart) {
        dateStart.addEventListener('change', function() {
            // 延遲執行，讓用戶有時間設定結束日期
            setTimeout(() => performSearch(1), 300);
        });
    }
    
    if (dateEnd) {
        dateEnd.addEventListener('change', function() {
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
    
    // 收集搜尋參數並進行驗證
    const keyword = document.getElementById('searchKeyword')?.value?.trim() || '';
    const searchField = document.getElementById('searchField')?.value || 'all';
    const status = document.getElementById('statusFilter')?.value || '';
    const dateStart = document.getElementById('dateStart')?.value || '';
    const dateEnd = document.getElementById('dateEnd')?.value || '';
    
    // 日期驗證
    if (dateStart && dateEnd && new Date(dateStart) > new Date(dateEnd)) {
        showErrorMessage('開始日期不能晚於結束日期');
        isLoading = false;
        hideLoadingState();
        return;
    }
    
    const formData = new FormData();
    formData.append('keyword', keyword);
    formData.append('searchField', searchField);
    formData.append('status', status);
    formData.append('dateStart', dateStart);
    formData.append('dateEnd', dateEnd);
    formData.append('page', page.toString());
    formData.append('pageSize', '10');
    
    console.log('Performing search with params:', {
        keyword: keyword,
        searchField: searchField,
        status: status,
        dateStart: dateStart,
        dateEnd: dateEnd,
        page: page,
        pageSize: 10
    });
    
    fetch('/Admin/FilterComplaints', {
        method: 'POST',
        body: formData,
        headers: {
            'RequestVerificationToken': document.querySelector('input[name=\"__RequestVerificationToken\"]')?.value || ''
        }
    })
    .then(response => {
        console.log('Response status:', response.status, response.statusText);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Search response received:', data);
        
        if (data && data.success) {
            // 確保 data.data 是陣列
            const complaintsData = Array.isArray(data.data) ? data.data : [];
            
            renderComplaintsTable(complaintsData);
            renderPagination(data.currentPage || 1, data.totalPages || 0, data.totalCount || 0);
            updateTotalCount(data.totalCount || 0);
            updateStatusSummaryFromTable();
            
            // 顯示服務器消息（如果有）
            if (data.message) {
                console.log('Server message:', data.message);
                // 如果是空結果消息，可以選擇顯示給用戶
                if (data.totalCount === 0 && data.message.includes('沒有符合條件')) {
                    showInfoMessage(data.message);
                }
            }
        } else {
            const errorMsg = data?.message || '搜尋失敗，請稍後再試';
            console.error('Search failed:', errorMsg);
            showErrorMessage(errorMsg);
            renderComplaintsTable([]);
            renderPagination(1, 0, 0);
            updateTotalCount(0);
        }
    })
    .catch(error => {
        console.error('Search error:', error);
        let errorMsg = '搜尋時發生錯誤';
        
        if (error.message.includes('HTTP 404')) {
            errorMsg = '搜尋功能暫時無法使用，請聯絡系統管理員';
        } else if (error.message.includes('HTTP 500')) {
            errorMsg = '服務器內部錯誤，請稍後再試';
        } else if (error.message.includes('NetworkError') || error.message.includes('fetch')) {
            errorMsg = '網路連接錯誤，請檢查網路狀態';
        } else {
            errorMsg += '：' + error.message;
        }
        
        showErrorMessage(errorMsg);
        renderComplaintsTable([]);
        renderPagination(1, 0, 0);
        updateTotalCount(0);
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
    console.error('Error:', message);
    alert('錯誤：' + message);
}

/**
 * 顯示信息訊息
 */
function showInfoMessage(message) {
    console.info('Info:', message);
    // 可以改為使用 Bootstrap Toast 顯示信息
    // 目前暫時不顯示彈窗，避免干擾用戶
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
    
    // 確保 complaints 是陣列
    if (!Array.isArray(complaints)) {
        console.error('complaints is not an array:', typeof complaints, complaints);
        complaints = [];
    }
    
    console.log('Rendering complaints table with', complaints.length, 'items');
    
    if (complaints.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="empty-state">
                    <div class="empty-state-icon">
                        <i class="bi bi-inbox"></i>
                    </div>
                    <div class="empty-state-title">查無投訴記錄</div>
                    <div class="empty-state-description">沒有符合條件的投訴案件</div>
                </td>
            </tr>
        `;
        return;
    }
    
    tbody.innerHTML = complaints.map(complaint => {
        // 安全地處理所有可能為 null/undefined 的值
        const complaintId = complaint.complaintId || 0;
        const complaintIdDisplay = complaint.complaintIdDisplay || (complaint.complaintId ? `CMPL-${String(complaint.complaintId).padStart(4, '0')}` : 'N/A');
        const complainantName = complaint.complainantName || '未知用戶';
        const complainantId = complaint.complainantId || 0;
        const propertyTitle = complaint.propertyTitle || '未知房源';
        const propertyId = complaint.propertyId || 0;
        const landlordName = complaint.landlordName || '未知房東';
        const landlordId = complaint.landlordId || 0;
        const complaintContent = complaint.complaintContent || '';
        const summary = complaint.summary || complaintContent || '無內容';
        const status = complaint.status || 'UNKNOWN';
        const statusDisplay = complaint.statusDisplay || getStatusDisplay(status);
        
        // 狀態徽章樣式
        const badgeClass = status === 'PENDING' ? 'bg-warning' : 
                          status === 'PROGRESS' ? 'bg-info' : 
                          status === 'RESOLVED' ? 'bg-success' : 'bg-dark';
        
        // 安全地處理日期格式
        let formattedDate = '無資料';
        try {
            if (complaint.createdAt) {
                const createdDate = new Date(complaint.createdAt);
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
                    <button type="button" class="btn btn-sm btn-outline-primary text-nowrap" onclick="showComplaintDetails(${complaintId})" title="編輯">
                        編輯
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
        case 'PENDING': return '待處理';
        case 'PROGRESS': return '處理中';
        case 'RESOLVED': return '已處理';
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
    if (isLoading) {
        console.log('Reset search blocked: search already in progress');
        return;
    }
    
    console.log('Resetting search form and filters');
    
    // 重設表單欄位
    const searchKeyword = document.getElementById('searchKeyword');
    const searchField = document.getElementById('searchField');
    const statusFilter = document.getElementById('statusFilter');
    const dateStart = document.getElementById('dateStart');
    const dateEnd = document.getElementById('dateEnd');
    
    if (searchKeyword) searchKeyword.value = '';
    if (searchField) searchField.selectedIndex = 0; // 選擇第一個選項（全部欄位）
    if (statusFilter) statusFilter.selectedIndex = 0; // 選擇第一個選項（全部）
    if (dateStart) dateStart.value = '';
    if (dateEnd) dateEnd.value = '';
    
    // 執行不帶任何搜尋條件的查詢，顯示所有記錄
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
            const pendingCount = complaints.filter(c => c.Status === 'PENDING').length;
            const progressCount = complaints.filter(c => c.Status === 'PROGRESS').length;
            const resolvedCount = complaints.filter(c => c.Status === 'RESOLVED').length;
            
            const pendingCountElement = document.getElementById('pendingCount');
            const progressCountElement = document.getElementById('progressCount');
            const resolvedCountElement = document.getElementById('resolvedCount');
            
            if (pendingCountElement) pendingCountElement.textContent = pendingCount;
            if (progressCountElement) progressCountElement.textContent = progressCount;
            if (resolvedCountElement) resolvedCountElement.textContent = resolvedCount;
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
    let pendingCount = 0, progressCount = 0, resolvedCount = 0;
    
    rows.forEach(row => {
        const statusBadge = row.querySelector('.badge');
        if (statusBadge) {
            const statusText = statusBadge.textContent.trim();
            if (statusText === '待處理') pendingCount++;
            else if (statusText === '處理中') progressCount++;
            else if (statusText === '已處理') resolvedCount++;
        }
    });
    
    // 更新顯示
    const pendingCountElement = document.getElementById('pendingCount');
    const progressCountElement = document.getElementById('progressCount');
    const resolvedCountElement = document.getElementById('resolvedCount');
    
    if (pendingCountElement) pendingCountElement.textContent = pendingCount;
    if (progressCountElement) progressCountElement.textContent = progressCount;
    if (resolvedCountElement) resolvedCountElement.textContent = resolvedCount;
}

/**
 * 檢查是否有任何搜尋條件或篩選
 */
function checkIfHasSearchConditions() {
    const searchKeyword = document.getElementById('searchKeyword')?.value || '';
    const statusFilter = document.getElementById('statusFilter')?.value || '';
    const dateStart = document.getElementById('dateStart')?.value || '';
    const dateEnd = document.getElementById('dateEnd')?.value || '';
    
    return searchKeyword.trim() !== '' || 
           statusFilter !== '' || 
           dateStart !== '' || 
           dateEnd !== '';
}

/**
 * 更新表格中特定投訴行的狀態顯示
 */
function updateComplaintRowInTable(complaintId, newStatus, updatedData) {
    // 找到對應的表格行
    const rows = document.querySelectorAll('#complaintsTableBody tr');
    
    rows.forEach(row => {
        const viewButton = row.querySelector('button[onclick*="showComplaintDetails"]');
        if (viewButton) {
            const onclickAttr = viewButton.getAttribute('onclick');
            const match = onclickAttr.match(/showComplaintDetails\((\d+)\)/);
            if (match && parseInt(match[1]) === parseInt(complaintId)) {
                // 找到匹配的行，更新狀態徽章
                const statusCell = row.cells[5]; // 狀態列是第6列（索引5）
                const statusBadge = statusCell.querySelector('.badge');
                
                if (statusBadge) {
                    // 更新狀態文字和樣式
                    const statusDisplay = getStatusDisplay(newStatus);
                    const badgeClass = newStatus === 'PENDING' ? 'bg-warning' : 
                                     newStatus === 'PROGRESS' ? 'bg-info' : 
                                     newStatus === 'RESOLVED' ? 'bg-success' : 'bg-dark';
                    
                    // 移除舊的樣式類別
                    statusBadge.className = `badge ${badgeClass}`;
                    statusBadge.textContent = statusDisplay;
                }
                
                return; // 找到目標行後退出迴圈
            }
        }
    });
}

/**
 * 顯示投訴詳情 Modal
 */
function showComplaintDetails(complaintId) {
    console.log('Loading complaint details for ID:', complaintId);
    
    // 先顯示 Modal
    const modal = new bootstrap.Modal(document.getElementById('complaintDetailsModal'), {
        backdrop: 'static',
        keyboard: false
    });
    modal.show();
    
    // 顯示載入狀態
    const content = document.getElementById('complaintDetailsContent');
    if (!content) {
        console.error('complaintDetailsContent element not found');
        return;
    }
    
    content.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">載入中...</span>
            </div>
            <p class="mt-2">載入投訴詳情中...</p>
        </div>
    `;
    
    // 載入投訴詳情
    fetch(`/Admin/GetComplaintDetails?complaintId=${complaintId}`)
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Received complaint data:', data);
            if (data.success && data.data) {
                renderComplaintDetails(data.data);
            } else {
                console.error('Server error:', data.message || 'No data received');
                const errorMessage = data.message || '無法載入投訴詳情資料';
                content.innerHTML = `
                    <div class="alert alert-danger">
                        <strong>錯誤：</strong>${errorMessage}
                        <br><small>投訴ID: ${complaintId}</small>
                    </div>
                `;
            }
        })
        .catch(error => {
            console.error('Fetch error:', error);
            content.innerHTML = `
                <div class="alert alert-danger">
                    <strong>網路錯誤：</strong>${error.message}
                    <br><small>請檢查網路連接或聯絡系統管理員</small>
                    <br><small>投訴ID: ${complaintId}</small>
                </div>
            `;
        });
}

/**
 * 取得投訴狀態顯示文字 - 處理所有可能的狀態值
 */
function getComplaintStatusDisplay(status) {
    switch (status) {
        case 'PENDING': return '待處理';
        case 'PROGRESS': return '處理中';
        case 'RESOLVED': return '已處理';
        default: return '未知';
    }
}

/**
 * 渲染投訴詳情
 */
function renderComplaintDetails(complaint) {
    console.log('Rendering complaint details:', complaint);
    
    const content = document.getElementById('complaintDetailsContent');
    
    if (!complaint) {
        content.innerHTML = `
            <div class="alert alert-warning">
                <strong>警告：</strong>無法載入投訴詳情資料
            </div>
        `;
        return;
    }
    
    // 驗證必要資料欄位 - 處理後端欄位名稱不一致問題
    const complaintId = complaint.ComplaintId || complaint.complaintId;
    if (!complaintId) {
        content.innerHTML = `
            <div class="alert alert-danger">
                <strong>錯誤：</strong>投訴資料不完整，缺少投訴ID
                <br><small>收到的資料: ${JSON.stringify(complaint, null, 2)}</small>
            </div>
        `;
        return;
    }
    
    // 處理狀態樣式 - 兼容多種狀態值
    const currentStatus = complaint.Status || complaint.status || 'UNKNOWN';
    const statusBadgeClass = currentStatus === 'PENDING' ? 'bg-warning' : 
                           currentStatus === 'PROGRESS' ? 'bg-info' : 
                           currentStatus === 'RESOLVED' ? 'bg-success' : 'bg-dark';
    
    // 安全地處理日期格式 - 後端已經格式化過，直接使用
    const formatDate = (dateStr) => {
        if (!dateStr) return '無資料';
        // 後端已經以 "yyyy-MM-dd HH:mm:ss" 格式返回，直接使用
        if (typeof dateStr === 'string' && dateStr.includes('-')) {
            // 移除秒數部分，只顯示到分鐘
            return dateStr.substring(0, 16).replace(' ', ' ');
        }
        // 如果是其他格式，嘗試解析
        try {
            const date = new Date(dateStr);
            if (isNaN(date.getTime())) return '無效日期';
            return date.toLocaleString('zh-TW', {
                year: 'numeric', 
                month: '2-digit', 
                day: '2-digit', 
                hour: '2-digit', 
                minute: '2-digit'
            });
        } catch (error) {
            console.warn('Date parsing error:', error);
            return dateStr; // 如果解析失敗，直接返回原始字串
        }
    };
    
    // 安全地處理所有數據欄位 - 處理後端欄位名稱不一致問題
    const safeComplaint = {
        ComplaintId: complaint.ComplaintId || complaint.complaintId || 0,
        ComplaintIdDisplay: complaint.ComplaintIdDisplay || complaint.complaintIdDisplay || `CMPL-${(complaint.ComplaintId || complaint.complaintId || 0).toString().padStart(4, '0')}`,
        ComplainantName: complaint.ComplainantName || complaint.complainantName || '未知用戶',
        ComplainantId: complaint.ComplainantId || complaint.complainantId || 0,
        PropertyTitle: complaint.PropertyTitle || complaint.propertyTitle || '未知房源',
        PropertyId: complaint.PropertyId || complaint.propertyId || 0,
        LandlordName: complaint.LandlordName || complaint.landlordName || '未知房東',
        LandlordId: complaint.LandlordId || complaint.landlordId || 0,
        ComplaintContent: complaint.ComplaintContent || complaint.complaintContent || '無投訴內容',
        Status: complaint.Status || complaint.status || 'UNKNOWN',
        StatusDisplay: complaint.StatusDisplay || complaint.statusDisplay || getComplaintStatusDisplay(complaint.Status || complaint.status || 'UNKNOWN'),
        CreatedAt: complaint.CreatedAt || complaint.createdAt,
        UpdatedAt: complaint.UpdatedAt || complaint.updatedAt,
        ResolvedAt: complaint.ResolvedAt || complaint.resolvedAt,
        InternalNote: complaint.InternalNote || complaint.internalNote || ''
    };

    content.innerHTML = `
        <div class="row">
            <div class="col-md-6">
                <div class="mb-3">
                    <label class="form-label fw-bold">投訴編號</label>
                    <p class="text-primary">${safeComplaint.ComplaintIdDisplay}</p>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold">投訴人</label>
                    <p><a href="/Admin/admin_userDetails/${safeComplaint.ComplainantId}" class="text-decoration-none">${safeComplaint.ComplainantName}</a></p>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold">房源標題</label>
                    <p><a href="/Admin/admin_propertyDetails/${safeComplaint.PropertyId}" class="text-decoration-none">${safeComplaint.PropertyTitle}</a></p>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold">房東</label>
                    <p><a href="/Admin/admin_userDetails/${safeComplaint.LandlordId}" class="text-decoration-none">${safeComplaint.LandlordName}</a></p>
                </div>
            </div>
            <div class="col-md-6">
                <div class="mb-3">
                    <label class="form-label fw-bold">處理狀態</label>
                    <div>
                        <span class="badge ${statusBadgeClass} me-2">${safeComplaint.StatusDisplay}</span>
                        <select id="statusSelect" class="form-select form-select-sm d-inline-block" style="width: auto;" onchange="updateComplaintStatus(${safeComplaint.ComplaintId}, this.value)">
                            <option value="PENDING" ${safeComplaint.Status === 'PENDING' ? 'selected' : ''}>待處理</option>
                            <option value="PROGRESS" ${safeComplaint.Status === 'PROGRESS' ? 'selected' : ''}>處理中</option>
                            <option value="RESOLVED" ${safeComplaint.Status === 'RESOLVED' ? 'selected' : ''}>已處理</option>
                        </select>
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold">投訴時間</label>
                    <p>${formatDate(safeComplaint.CreatedAt)}</p>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold">最後更新</label>
                    <p id="lastUpdated">${formatDate(safeComplaint.UpdatedAt)}</p>
                </div>
                ${safeComplaint.ResolvedAt ? `
                <div class="mb-3">
                    <label class="form-label fw-bold">結案時間</label>
                    <p>${formatDate(safeComplaint.ResolvedAt)}</p>
                </div>
                ` : ''}
            </div>
        </div>
        <div class="row">
            <div class="col-12">
                <div class="mb-3">
                    <label class="form-label fw-bold">投訴內容</label>
                    <div class="p-3 bg-light rounded">
                        ${safeComplaint.ComplaintContent.replace(/\n/g, '<br>')}
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold">內部註記</label>
                    <textarea id="internalNote" class="form-control" rows="4" placeholder="請輸入內部處理註記...">${safeComplaint.InternalNote}</textarea>
                    <input type="hidden" id="currentComplaintId" value="${safeComplaint.ComplaintId}">
                </div>
            </div>
        </div>
    `;
}

/**
 * 儲存內部註記
 */
function saveInternalNote() {
    const complaintId = document.getElementById('currentComplaintId')?.value;
    const internalNote = document.getElementById('internalNote')?.value || '';
    
    if (!complaintId) {
        alert('無法取得投訴ID');
        return;
    }
    
    const formData = new FormData();
    formData.append('complaintId', complaintId);
    formData.append('internalNote', internalNote);
    
    fetch('/Admin/UpdateInternalNote', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert('內部註記已更新');
            const lastUpdatedElement = document.getElementById('lastUpdated');
            if (lastUpdatedElement && data.data && data.data.UpdatedAt) {
                // 後端返回已格式化的日期字串，直接使用並移除秒數
                const formattedDate = data.data.UpdatedAt.substring(0, 16);
                lastUpdatedElement.textContent = formattedDate;
            }
            // 只有在當前有搜尋條件或篩選時才重新載入表格
            const hasSearchConditions = checkIfHasSearchConditions();
            if (hasSearchConditions) {
                performSearch(currentPage);
            } else {
                // 如果沒有搜尋條件，只更新狀態統計
                updateStatusSummaryFromTable();
            }
        } else {
            alert('更新失敗：' + (data.message || '未知錯誤'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('更新時發生錯誤：' + error.message);
    });
}

/**
 * 更新投訴狀態
 */
function updateComplaintStatus(complaintId, status) {
    const formData = new FormData();
    formData.append('complaintId', complaintId);
    formData.append('status', status);
    
    fetch('/Admin/UpdateComplaintStatus', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert('投訴狀態已更新');
            const lastUpdatedElement = document.getElementById('lastUpdated');
            if (lastUpdatedElement && data.data && data.data.UpdatedAt) {
                // 後端返回已格式化的日期字串，直接使用並移除秒數
                const formattedDate = data.data.UpdatedAt.substring(0, 16);
                lastUpdatedElement.textContent = formattedDate;
            }
            
            // 更新表格中對應行的狀態顯示
            updateComplaintRowInTable(complaintId, status, data.data);
            
            // 只有在當前有搜尋條件或篩選時才重新載入表格
            const hasSearchConditions = checkIfHasSearchConditions();
            if (hasSearchConditions) {
                performSearch(currentPage);
            } else {
                // 更新狀態統計
                updateStatusSummaryFromTable();
            }
        } else {
            alert('更新失敗：' + (data.message || '未知錯誤'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('更新時發生錯誤：' + error.message);
    });
}

// 使函數在全域可用，供 HTML onclick 屬性使用
window.performSearch = performSearch;
window.resetSearch = resetSearch;
window.renderComplaintsTable = renderComplaintsTable;
window.renderPagination = renderPagination;
window.updateStatusSummary = updateStatusSummary;
window.updateStatusSummaryFromTable = updateStatusSummaryFromTable;
window.showComplaintDetails = showComplaintDetails;
window.renderComplaintDetails = renderComplaintDetails;
window.saveInternalNote = saveInternalNote;
window.updateComplaintStatus = updateComplaintStatus;
window.getComplaintStatusDisplay = getComplaintStatusDisplay;
window.checkIfHasSearchConditions = checkIfHasSearchConditions;
window.updateComplaintRowInTable = updateComplaintRowInTable;