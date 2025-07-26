// 客服管理系統 JavaScript
// 全域變數
let currentPage = 1;
let currentTab = 'all';
let searchParams = {};

// 全域函數 - 查看客服案件詳情
function viewTicketDetails(ticketId) {
    window.location.href = '/Admin/admin_customerServiceDetails?ticketId=' + ticketId;
}

// 客服管理主物件
const CustomerServiceManager = {
    // 初始化
    init() {
        console.log('客服管理系統初始化');
        this.initEventHandlers();
        this.loadInitialData();
    },

    // 初始化事件處理器
    initEventHandlers() {
        // 搜尋表單事件
        const searchForm = document.getElementById('searchForm');
        if (searchForm) {
            searchForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.performSearch();
            });
        }

        // 快速搜尋按鈕
        const quickSearchBtn = document.getElementById('quickSearchBtn');
        if (quickSearchBtn) {
            quickSearchBtn.addEventListener('click', () => this.performSearch());
        }

        // 重設按鈕
        const resetBtn = document.getElementById('resetBtn');
        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.resetFilters());
        }

        // 進階篩選按鈕
        const applyAdvancedBtn = document.getElementById('applyAdvancedBtn');
        if (applyAdvancedBtn) {
            applyAdvancedBtn.addEventListener('click', () => this.performSearch());
        }

        const clearAdvancedBtn = document.getElementById('clearAdvancedBtn');
        if (clearAdvancedBtn) {
            clearAdvancedBtn.addEventListener('click', () => this.clearAdvancedFilters());
        }

        // 頁籤切換事件
        const tabButtons = document.querySelectorAll('#ticketTabs button[data-bs-toggle="tab"]');
        tabButtons.forEach(button => {
            button.addEventListener('shown.bs.tab', (e) => {
                currentTab = e.target.id === 'all-tab' ? 'all' : 'unresolved';
                this.loadTickets();
            });
        });

        // Enter鍵搜尋
        const searchKeyword = document.getElementById('searchKeyword');
        if (searchKeyword) {
            searchKeyword.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.performSearch();
                }
            });
        }
    },

    // 載入初始資料
    loadInitialData() {
        this.loadTickets();
        this.loadStatistics();
    },

    // 執行搜尋
    performSearch(page = 1) {
        const formData = this.getSearchParams();
        formData.page = page;
        this.loadTickets(formData);
    },

    // 取得搜尋參數
    getSearchParams() {
        return {
            keyword: document.getElementById('searchKeyword')?.value || '',
            searchField: document.getElementById('searchField')?.value || 'all',
            status: document.getElementById('statusFilter')?.value || '',
            category: document.getElementById('categoryFilter')?.value || '',
            dateStart: document.getElementById('dateStart')?.value || '',
            dateEnd: document.getElementById('dateEnd')?.value || '',
            page: currentPage,
            pageSize: 10
        };
    },

    // 載入案件列表
    async loadTickets(params = null) {
        try {
            const searchParams = params || this.getSearchParams();
            
            // 根據當前頁籤調整篩選條件
            if (currentTab === 'unresolved' && !searchParams.status) {
                searchParams.status = 'PENDING,PROGRESS';
            }

            const formData = new FormData();
            Object.keys(searchParams).forEach(key => formData.append(key, searchParams[key]));

            const response = await fetch('/Admin/FilterCustomerService', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                this.renderTicketsTable(result.data);
                this.renderPagination(result);
                this.updateCounts(result);
            } else {
                console.error('載入案件失敗:', result.message);
                this.showError('載入案件資料失敗');
            }
        } catch (error) {
            console.error('載入案件時發生錯誤:', error);
            this.showError('載入案件資料時發生錯誤');
        }
    },

    // 渲染案件表格
    renderTicketsTable(tickets) {
        const tableBody = currentTab === 'all' 
            ? document.getElementById('ticketsTableBody')
            : document.getElementById('unresolvedTableBody');

        if (!tableBody) return;

        if (tickets.length === 0) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center py-4">
                        <i class="bi bi-inbox text-muted" style="font-size: 2rem;"></i>
                        <div class="mt-2">暫無符合條件的客服案件</div>
                    </td>
                </tr>
            `;
            return;
        }

        const rows = tickets.map(ticket => `
            <tr class="ticket-row" data-ticket-id="${ticket.ticketId}" style="cursor: pointer;">
                <td>
                    <a href="/Admin/admin_customerServiceDetails?ticketId=${ticket.ticketId}" 
                       class="text-primary text-decoration-none">
                        ${ticket.ticketIdDisplay}
                    </a>
                </td>
                <td>
                    <a href="/Admin/admin_userDetails?id=${ticket.memberId}" 
                       class="text-decoration-none">
                        ${ticket.memberName}
                    </a>
                </td>
                <td title="${ticket.subject}">
                    ${ticket.subject.length > 30 ? ticket.subject.substring(0, 30) + '...' : ticket.subject}
                </td>
                <td>
                    <span class="badge ${this.getCategoryBadgeClass(ticket.categoryCode)}">
                        ${ticket.categoryDisplay}
                    </span>
                </td>
                <td>
                    <span class="badge ${this.getStatusBadgeClass(ticket.statusCode)}">
                        ${ticket.statusDisplay}
                    </span>
                </td>
                <td>${this.formatDateTime(ticket.createdAt)}</td>
                <td>${ticket.handledByName}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-outline-primary" 
                            onclick="viewTicketDetails(${ticket.ticketId})" title="檢視詳情">
                        <i class="bi bi-eye"></i> 前往處理
                    </button>
                </td>
            </tr>
        `).join('');

        tableBody.innerHTML = rows;
    },

    // 渲染分頁
    renderPagination(result) {
        const pagination = currentTab === 'all' 
            ? document.getElementById('pagination')
            : document.getElementById('unresolvedPagination');

        if (!pagination || result.totalPages <= 1) return;

        let paginationHTML = '<ul class="pagination justify-content-center">';

        // 上一頁
        paginationHTML += `
            <li class="page-item ${result.currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="CustomerServiceManager.performSearch(${result.currentPage - 1})">
                    <i class="bi bi-chevron-left"></i>
                </a>
            </li>
        `;

        // 頁碼
        for (let i = 1; i <= result.totalPages; i++) {
            paginationHTML += `
                <li class="page-item ${i === result.currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="CustomerServiceManager.performSearch(${i})">${i}</a>
                </li>
            `;
        }

        // 下一頁
        paginationHTML += `
            <li class="page-item ${result.currentPage === result.totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="CustomerServiceManager.performSearch(${result.currentPage + 1})">
                    <i class="bi bi-chevron-right"></i>
                </a>
            </li>
        `;

        paginationHTML += '</ul>';
        pagination.innerHTML = paginationHTML;
    },

    // 更新統計數據
    updateCounts(result) {
        // 更新對應頁籤的總數顯示
        if (currentTab === 'all') {
            const totalCountElement = document.getElementById('totalCount');
            if (totalCountElement) {
                totalCountElement.textContent = result.totalCount;
            }
            const allTabCountElement = document.getElementById('allTicketsCount');
            if (allTabCountElement) {
                allTabCountElement.textContent = result.totalCount;
            }
        } else if (currentTab === 'unresolved') {
            const unresolvedTotalCountElement = document.getElementById('unresolvedTotalCount');
            if (unresolvedTotalCountElement) {
                unresolvedTotalCountElement.textContent = result.totalCount;
            }
            const unresolvedTabCountElement = document.getElementById('unresolvedCount');
            if (unresolvedTabCountElement) {
                unresolvedTabCountElement.textContent = result.totalCount;
            }
        }
    },

    // 載入統計資料
    async loadStatistics() {
        try {
            const response = await fetch('/Admin/GetCustomerServiceStatistics', {
                method: 'GET'
            });

            const result = await response.json();

            if (result.success) {
                this.updateStatisticsBadges(result.data);
            } else {
                console.error('載入統計數據失敗:', result.message);
            }
        } catch (error) {
            console.error('載入統計數據時發生錯誤:', error);
        }
    },

    // 更新統計數據徽章
    updateStatisticsBadges(statistics) {
        // 更新狀態統計徽章
        const pendingElement = document.getElementById('pendingCount');
        if (pendingElement) {
            pendingElement.textContent = statistics.pendingCount;
        }

        const progressElement = document.getElementById('progressCount');
        if (progressElement) {
            progressElement.textContent = statistics.progressCount;
        }

        const resolvedElement = document.getElementById('resolvedCount');
        if (resolvedElement) {
            resolvedElement.textContent = statistics.resolvedCount;
        }

        // 更新頁籤徽章
        const allTicketsCountElement = document.getElementById('allTicketsCount');
        if (allTicketsCountElement) {
            allTicketsCountElement.textContent = statistics.totalCount;
        }

        const unresolvedCountElement = document.getElementById('unresolvedCount');
        if (unresolvedCountElement) {
            unresolvedCountElement.textContent = statistics.unresolvedCount;
        }
    },

    // 重設篩選條件
    resetFilters() {
        document.getElementById('searchKeyword').value = '';
        document.getElementById('searchField').value = 'all';
        document.getElementById('statusFilter').value = '';
        document.getElementById('categoryFilter').value = '';
        this.clearAdvancedFilters();
        this.performSearch();
    },

    // 清除進階篩選
    clearAdvancedFilters() {
        document.getElementById('dateStart').value = '';
        document.getElementById('dateEnd').value = '';
    },

    // 取得類別徽章樣式
    getCategoryBadgeClass(categoryCode) {
        switch (categoryCode) {
            case 'PROPERTY': return 'badge-subtle-info';
            case 'CONTRACT': return 'badge-subtle-secondary';
            case 'FURNITURE': return 'badge-subtle-primary';
            default: return 'badge-subtle-secondary';
        }
    },

    // 取得狀態徽章樣式
    getStatusBadgeClass(statusCode) {
        switch (statusCode) {
            case 'PENDING': return 'badge-subtle-warning';
            case 'PROGRESS': return 'badge-subtle-primary';
            case 'RESOLVED': return 'badge-subtle-secondary';
            default: return 'badge-subtle-secondary';
        }
    },

    // 格式化日期時間
    formatDateTime(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('zh-TW') + ' ' + 
               date.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' });
    },

    // 顯示錯誤訊息
    showError(message) {
        console.error(message);
        // 這裡可以顯示toast通知或其他錯誤提示
        alert(message);
    }
};

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', function() {
    CustomerServiceManager.init();
});