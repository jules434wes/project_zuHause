/**
 * 系統訊息管理頁面 JavaScript
 * 支援訊息檢視、搜尋篩選、表格排序等功能
 */

$(document).ready(function() {
    initializeSystemMessageList();
});

/**
 * 初始化系統訊息列表頁面
 */
function initializeSystemMessageList() {
    // 初始化事件監聽器
    bindEventListeners();
    
    // 初始化表格排序
    initializeTableSorting();
    
    // 初始化進階搜尋
    initializeAdvancedSearch();
}

/**
 * 綁定事件監聽器
 */
function bindEventListeners() {
    // 搜尋按鈕
    $('#searchBtn').on('click', handleSearch);
    
    // 重置按鈕
    $('#resetBtn').on('click', handleReset);
    
    // Enter 鍵搜尋
    $('#searchInput').on('keypress', function(e) {
        if (e.which === 13) {
            handleSearch();
        }
    });
    
    // 進階搜尋切換
    $('#advancedSearchBtn').on('click', toggleAdvancedSearch);
    
    // 檢視訊息按鈕 (使用事件委派)
    $(document).on('click', '.view-message-btn', function() {
        const messageId = $(this).data('message-id');
        const title = $(this).data('message-title');
        const content = $(this).data('message-content');
        viewMessage(messageId, title, content);
    });
}

/**
 * 初始化表格排序功能
 */
function initializeTableSorting() {
    $('.sortable').on('click', function() {
        const $this = $(this);
        const sortField = $this.data('sort');
        const $icon = $this.find('.sort-icon');
        
        // 重置其他欄位的排序圖示
        $('.sortable').not($this).find('.sort-icon')
            .removeClass('bi-sort-up bi-sort-down')
            .addClass('bi-sort-down');
        
        // 切換當前欄位的排序
        if ($icon.hasClass('bi-sort-down')) {
            $icon.removeClass('bi-sort-down').addClass('bi-sort-up');
            sortTable(sortField, 'asc');
        } else {
            $icon.removeClass('bi-sort-up').addClass('bi-sort-down');
            sortTable(sortField, 'desc');
        }
    });
}

/**
 * 表格排序（前端實現）
 */
function sortTable(field, direction) {
    const $tbody = $('.table tbody');
    const $rows = $tbody.find('tr').not(':has(.empty-state)');
    
    if ($rows.length === 0) return;
    
    const sortedRows = $rows.sort(function(a, b) {
        let aVal, bVal;
        
        switch (field) {
            case 'messageID':
                aVal = parseInt($(a).find('td:first').text()) || 0;
                bVal = parseInt($(b).find('td:first').text()) || 0;
                break;
            case 'title':
                aVal = $(a).find('td:nth-child(2) .fw-semibold').text().toLowerCase();
                bVal = $(b).find('td:nth-child(2) .fw-semibold').text().toLowerCase();
                break;
            case 'sentAt':
                // 假設日期格式為 YYYY-MM-DD HH:mm:ss
                aVal = new Date($(a).find('td:nth-child(7)').text());
                bVal = new Date($(b).find('td:nth-child(7)').text());
                break;
            default:
                return 0;
        }
        
        if (direction === 'asc') {
            return aVal > bVal ? 1 : aVal < bVal ? -1 : 0;
        } else {
            return aVal < bVal ? 1 : aVal > bVal ? -1 : 0;
        }
    });
    
    $tbody.append(sortedRows);
}

/**
 * 初始化進階搜尋
 */
function initializeAdvancedSearch() {
    const $advancedCollapse = $('#advancedSearchCollapse');
    
    $advancedCollapse.on('shown.bs.collapse', function() {
        $('#advancedSearchBtn i').removeClass('bi-chevron-down').addClass('bi-chevron-up');
    });
    
    $advancedCollapse.on('hidden.bs.collapse', function() {
        $('#advancedSearchBtn i').removeClass('bi-chevron-up').addClass('bi-chevron-down');
    });
}

/**
 * 切換進階搜尋
 */
function toggleAdvancedSearch() {
    const $collapse = $('#advancedSearchCollapse');
    $collapse.collapse('toggle');
}

/**
 * 處理搜尋
 */
function handleSearch() {
    const searchParams = getSearchParameters();
    
    // 顯示載入狀態
    showSearchLoading();
    
    // 這裡可以調用後端API進行搜尋
    // 目前使用前端模擬搜尋
    performClientSideSearch(searchParams);
}

/**
 * 取得搜尋參數
 */
function getSearchParameters() {
    return {
        keyword: $('#searchInput').val().trim(),
        searchField: $('#searchField').val(),
        audienceType: $('#audienceFilter').val(),
        category: $('#categoryFilter').val(),
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        admin: $('#adminFilter').val()
    };
}

/**
 * 顯示搜尋載入狀態
 */
function showSearchLoading() {
    $('#searchBtn').prop('disabled', true).html(`
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        搜尋中...
    `);
}

/**
 * 隱藏搜尋載入狀態
 */
function hideSearchLoading() {
    $('#searchBtn').prop('disabled', false).html(`
        <i class="bi bi-search"></i> 搜尋
    `);
}

/**
 * 執行前端搜尋（模擬）
 */
function performClientSideSearch(params) {
    const $rows = $('.table tbody tr').not(':has(.empty-state)');
    let visibleCount = 0;
    
    $rows.each(function() {
        const $row = $(this);
        let shouldShow = true;
        
        // 關鍵字搜尋
        if (params.keyword) {
            const keyword = params.keyword.toLowerCase();
            let searchText = '';
            
            switch (params.searchField) {
                case 'title':
                    searchText = $row.find('td:nth-child(2) .fw-semibold').text().toLowerCase();
                    break;
                case 'content':
                    // 內容需要從 data attribute 或其他地方獲取
                    searchText = $row.find('td:nth-child(2)').text().toLowerCase();
                    break;
                case 'messageID':
                    searchText = $row.find('td:first').text().toLowerCase();
                    break;
                case 'adminName':
                    searchText = $row.find('td:nth-child(6)').text().toLowerCase();
                    break;
                default:
                    // 全欄位搜尋
                    searchText = $row.text().toLowerCase();
                    break;
            }
            
            if (!searchText.includes(keyword)) {
                shouldShow = false;
            }
        }
        
        // 發送對象篩選
        if (params.audienceType && shouldShow) {
            const audienceText = $row.find('td:nth-child(4)').text().trim();
            const audienceMap = {
                'ALL_MEMBERS': '全體會員',
                'ALL_LANDLORDS': '全體房東',
                'INDIVIDUAL': '個別用戶'
            };
            
            if (audienceText !== audienceMap[params.audienceType]) {
                shouldShow = false;
            }
        }
        
        // 分類篩選
        if (params.category && shouldShow) {
            const categoryText = $row.find('td:nth-child(3) .badge').text().trim();
            const categoryMap = {
                'ANNOUNCEMENT': '一般公告',
                'UPDATE': '功能更新',
                'SECURITY': '安全提醒',
                'MAINTENANCE': '系統維護',
                'POLICY': '政策更新',
                'ORDER': '訂單通知',
                'PROPERTY': '房源通知',
                'ACCOUNT': '帳戶通知'
            };
            
            if (categoryText !== categoryMap[params.category]) {
                shouldShow = false;
            }
        }
        
        // 日期範圍篩選
        if ((params.startDate || params.endDate) && shouldShow) {
            const dateText = $row.find('td:nth-child(7)').text().trim();
            const messageDate = new Date(dateText);
            
            if (params.startDate && messageDate < new Date(params.startDate)) {
                shouldShow = false;
            }
            
            if (params.endDate && messageDate > new Date(params.endDate + ' 23:59:59')) {
                shouldShow = false;
            }
        }
        
        // 發送者篩選
        if (params.admin && shouldShow) {
            const adminText = $row.find('td:nth-child(6)').text().trim();
            if (adminText !== params.admin) {
                shouldShow = false;
            }
        }
        
        // 顯示或隱藏行
        if (shouldShow) {
            $row.show();
            visibleCount++;
        } else {
            $row.hide();
        }
    });
    
    // 如果沒有找到結果，顯示空狀態
    updateEmptyState(visibleCount);
    
    // 隱藏載入狀態
    setTimeout(() => {
        hideSearchLoading();
        
        // 顯示搜尋結果統計
        showSearchResults(visibleCount, params);
    }, 500);
}

/**
 * 更新空狀態顯示
 */
function updateEmptyState(visibleCount) {
    const $tbody = $('.table tbody');
    const $emptyRow = $tbody.find('.empty-state').closest('tr');
    
    if (visibleCount === 0) {
        if ($emptyRow.length === 0) {
            $tbody.append(`
                <tr class="search-empty-state">
                    <td colspan="8" class="text-center text-muted py-4">
                        <div class="empty-state">
                            <i class="bi bi-search empty-state-icon"></i>
                            <div class="empty-state-title">找不到符合條件的訊息</div>
                            <div class="empty-state-description">請調整搜尋條件後重試</div>
                        </div>
                    </td>
                </tr>
            `);
        }
    } else {
        $('.search-empty-state').remove();
    }
}

/**
 * 顯示搜尋結果
 */
function showSearchResults(count, params) {
    const hasActiveFilters = params.keyword || params.audienceType || params.category || 
                           params.startDate || params.endDate || params.admin;
    
    if (hasActiveFilters) {
        showSuccess(`找到 ${count} 筆符合條件的訊息`);
    }
}

/**
 * 處理重置
 */
function handleReset() {
    // 重置表單
    $('#searchInput').val('');
    $('#searchField').val('title');
    $('#audienceFilter').val('');
    $('#categoryFilter').val('');
    $('#startDate').val('');
    $('#endDate').val('');
    $('#adminFilter').val('');
    
    // 顯示所有行
    $('.table tbody tr').show();
    $('.search-empty-state').remove();
    
    // 重置排序
    $('.sortable .sort-icon')
        .removeClass('bi-sort-up bi-sort-down')
        .addClass('bi-sort-down');
    
    showSuccess('已重置所有搜尋條件');
}

/**
 * 檢視訊息詳情
 */
function viewMessage(messageId, title, content) {
    // 獲取現有的Modal
    const modalElement = document.getElementById('viewMessageModal');
    if (!modalElement) {
        console.error('viewMessageModal not found');
        return;
    }
    
    // 設定Modal內容
    const modalTitle = modalElement.querySelector('.modal-title');
    const titleElement = modalElement.querySelector('#messageModalTitle');
    const contentElement = modalElement.querySelector('#messageModalContent');
    
    if (modalTitle) modalTitle.textContent = `訊息詳情 - ID: ${messageId}`;
    if (titleElement) titleElement.textContent = title;
    if (contentElement) contentElement.textContent = content;
    
    // 使用 Bootstrap 5 Modal API
    const modal = new bootstrap.Modal(modalElement);
    modal.show();
}


/**
 * 工具函數 - 顯示成功訊息
 */
function showSuccess(message) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'success',
            title: '成功',
            text: message,
            timer: 2000,
            showConfirmButton: false,
            toast: true,
            position: 'top-end'
        });
    } else {
        // 使用Bootstrap Toast或簡單alert
        console.log('成功：' + message);
    }
}

/**
 * 工具函數 - 顯示錯誤訊息
 */
function showError(message) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'error',
            title: '錯誤',
            text: message
        });
    } else {
        alert('錯誤：' + message);
    }
}

// 全域函數定義（供HTML onclick使用）
window.viewMessage = viewMessage;