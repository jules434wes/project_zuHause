// 客服管理系統 JavaScript
// 全域函數 - 查看客服案件詳情
function viewTicketDetails(ticketId) {
    // 跳轉至客服案件詳情頁
    window.location.href = '/Admin/admin_customerServiceDetails?ticketId=' + ticketId;
    console.log('查看客服案件詳情: ' + ticketId);
}

document.addEventListener('DOMContentLoaded', function() {
    console.log('DOMContentLoaded: 客服頁面初始化開始');
    
    // 進階篩選摺疊功能
    document.getElementById('advancedSearchBtn').addEventListener('click', function() {
        var collapseElement = document.getElementById('advancedSearchCollapse');
        var collapse = new bootstrap.Collapse(collapseElement, {
            toggle: true
        });
        
        // 切換按鈕圖示 - 使用 Bootstrap Icons
        var icon = this.querySelector('i');
        if (collapseElement.classList.contains('show')) {
            icon.className = 'bi bi-chevron-down';
        } else {
            icon.className = 'bi bi-chevron-up';
        }
    });

    // 搜尋功能
    document.getElementById('searchBtn').addEventListener('click', function() {
        performSearch();
    });

    // Enter鍵搜尋
    document.getElementById('searchInput').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            performSearch();
        }
    });

    // 狀態篩選變更
    document.getElementById('statusFilter').addEventListener('change', function() {
        filterByStatus(this.value);
    });

    // 重置篩選
    document.getElementById('resetBtn').addEventListener('click', function() {
        resetAllFilters();
    });

    // 重新整理按鈕
    document.getElementById('refreshBtn').addEventListener('click', function() {
        location.reload();
    });

    // 匯出按鈕
    document.getElementById('exportBtn').addEventListener('click', function() {
        exportTicketData();
    });

    // 表格行點擊跳轉
    document.querySelectorAll('.ticket-row').forEach(function(row) {
        row.addEventListener('click', function(e) {
            // 避免點擊按鈕時觸發
            if (!e.target.closest('button') && !e.target.closest('a')) {
                var ticketId = this.getAttribute('data-ticket-id');
                viewTicketDetails(ticketId);
            }
        });
        
        // 添加滑鼠游標樣式
        row.style.cursor = 'pointer';
    });
});

// 執行搜尋
function performSearch() {
    var searchTerm = document.getElementById('searchInput').value;
    var searchField = document.getElementById('searchField').value;
    var status = document.getElementById('statusFilter').value;
    var source = document.getElementById('sourceFilter').value;
    var category = document.getElementById('categoryFilter').value;
    
    console.log('搜尋條件:', {
        searchTerm: searchTerm,
        searchField: searchField,
        status: status,
        source: source,
        category: category
    });
    
    // 這裡實際應該發送AJAX請求到後端
    // 目前只顯示搜尋參數
    var conditions = [];
    if (searchTerm) conditions.push(searchField + ': ' + searchTerm);
    if (status) conditions.push('狀態: ' + status);
    if (source) conditions.push('來源: ' + source);
    if (category) conditions.push('類別: ' + category);
    
    if (conditions.length > 0) {
        alert('搜尋功能 - ' + conditions.join(', '));
    }
}

// 依狀態篩選
function filterByStatus(status) {
    var rows = document.querySelectorAll('.ticket-row');
    
    rows.forEach(function(row) {
        var statusButton = row.querySelector('td:nth-child(5) button');
        var rowStatus = '';
        
        if (statusButton.classList.contains('btn-warning')) rowStatus = 'pending';
        else if (statusButton.classList.contains('btn-info')) rowStatus = 'in_progress';
        else if (statusButton.classList.contains('btn-success')) rowStatus = 'resolved';
        
        if (status === '' || rowStatus === status) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
    
    updateVisibleCount();
}

// 重置所有篩選
function resetAllFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('searchField').value = 'ticketId';
    document.getElementById('statusFilter').value = '';
    document.getElementById('sourceFilter').value = '';
    document.getElementById('categoryFilter').value = '';
    document.getElementById('handlerFilter').value = '';
    document.getElementById('createDateStart').value = '';
    document.getElementById('createDateEnd').value = '';
    document.getElementById('updateDateStart').value = '';
    document.getElementById('updateDateEnd').value = '';
    
    // 顯示所有行
    document.querySelectorAll('.ticket-row').forEach(function(row) {
        row.style.display = '';
    });
    
    updateVisibleCount();
}

// 匯出資料
function exportTicketData() {
    console.log('匯出客服案件資料');
    alert('匯出功能 - 將下載客服案件列表');
}

// 更新可見行數統計
function updateVisibleCount() {
    var visibleRows = document.querySelectorAll('.ticket-row[style=""], .ticket-row:not([style])').length;
    var badge = document.querySelector('.card-header .badge');
    badge.textContent = '共 ' + visibleRows + ' 筆案件';
}