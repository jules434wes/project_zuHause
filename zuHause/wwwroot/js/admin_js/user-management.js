// 會員管理頁面 JavaScript
// 全域函數：帳戶狀態切換 (供HTML onclick呼叫)
function toggleAccountStatus(memberId, currentStatus) {
    var newStatus = currentStatus === 'active' ? '停用' : '啟用';
    var actionText = currentStatus === 'active' ? '停用此帳戶' : '啟用此帳戶';
    
    // 危險操作需要延遲確認，防止誤點擊
    setTimeout(function() {
        var confirmMessage = '危險操作確認\n\n' +
                           '會員ID: ' + memberId + '\n' +
                           '操作: ' + actionText + '\n\n' +
                           '此操作將會影響會員的登入權限。\n' +
                           '確定要繼續嗎？';
        
        if (confirm(confirmMessage)) {
            // 二次確認
            var secondConfirm = '最後確認:\n\n確定要' + actionText + '嗎？\n\n' +
                              '請輸入會員ID "' + memberId + '" 以確認操作:';
            var userInput = prompt(secondConfirm);
            
            if (userInput === memberId) {
                console.log('切換會員 ' + memberId + ' 帳戶狀態為: ' + newStatus);
                // TODO: 實作後端API呼叫
                alert('帳戶狀態已' + newStatus + ' (開發中...)');
            } else if (userInput !== null) {
                alert('輸入的會員ID不正確，操作已取消');
            }
        }
    }, 300); // 延遲300ms避免誤點擊
}

// 全域函數：開啟管理備註Modal (供HTML onclick呼叫)
function openAdminNotesModal(memberId) {
    console.log('開啟會員 ' + memberId + ' 管理備註');
    // TODO: 實作管理備註Modal
    alert('管理備註功能開發中...');
}

// 全域函數：重置驗證狀態 (供HTML onclick呼叫)
function resetVerificationStatus(memberId) {
    if (confirm('確定要重置會員 ' + memberId + ' 的驗證狀態嗎？此操作會清除現有驗證記錄。')) {
        console.log('重置會員 ' + memberId + ' 驗證狀態');
        // TODO: 實作後端API呼叫
        alert('驗證狀態已重置 (開發中...)');
    }
}

// 全域函數：查看用戶操作記錄 (供HTML onclick呼叫)
function viewUserActivityLog(memberId) {
    console.log('查看會員 ' + memberId + ' 操作記錄');
    // TODO: 實作操作記錄查看功能
    alert('操作記錄查看功能開發中...');
}

// 全域函數：載入會員證件 (供HTML onclick呼叫)
function loadDocuments(memberId) {
    console.log('載入會員ID ' + memberId + ' 的證件');
    
    // 更新Modal標題
    document.getElementById('viewDocumentModalLabel').textContent = '查看身分證件 - 會員ID: ' + memberId;
    
    // TODO: 後續實作從伺服器載入證件圖片的功能
    // 暫時顯示會員ID資訊
    var modalBody = document.querySelector('#viewDocumentModal .modal-body');
    modalBody.innerHTML = `
        <div class="text-center">
            <h6>會員ID: ${memberId}</h6>
            <p class="text-muted">身分證件載入功能開發中...</p>
            <div class="row">
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-header">
                            <small class="text-muted">身分證正面</small>
                        </div>
                        <div class="card-body text-center" style="height: 200px; background-color: #f8f9fa;">
                            <i class="fas fa-image fa-3x text-muted"></i>
                            <p class="text-muted mt-2">圖片載入中...</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-header">
                            <small class="text-muted">身分證反面</small>
                        </div>
                        <div class="card-body text-center" style="height: 200px; background-color: #f8f9fa;">
                            <i class="fas fa-image fa-3x text-muted"></i>
                            <p class="text-muted mt-2">圖片載入中...</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

document.addEventListener('DOMContentLoaded', function() {
    // 初始化分頁切換事件
    var triggerTabList = [].slice.call(document.querySelectorAll('#userTabs button'))
    triggerTabList.forEach(function (triggerEl) {
        var tabTrigger = new bootstrap.Tab(triggerEl)
        
        triggerEl.addEventListener('click', function (event) {
            event.preventDefault()
            tabTrigger.show()
            
            // 切換分頁後重新初始化排序功能
            setTimeout(function() {
                initSortingEvents();
            }, 100);
        })
    })

    // 初始化各分頁的事件處理器
    initTabEvents('', 'user-checkbox', 'bulkMessageBtn', 'selectAllUsers', '全部會員');
    initTabEvents('Pending', 'user-checkbox-pending', 'bulkMessageBtnPending', 'selectAllUsersPending', '等待驗證會員');
    initTabEvents('Landlord', 'user-checkbox-landlord', 'bulkMessageBtnLandlord', 'selectAllUsersLandlord', '申請房東會員');

    // 動態載入城市資料
    loadCityData();

    // 初始化排序功能
    initSortingEvents();
    
    // 初始化載入筆數功能
    initPageSizeEvents();
    
    // 初始頁面載入時套用預設載入筆數 (10筆)
    setTimeout(function() {
        document.querySelectorAll('.page-size-selector').forEach(function(selector) {
            var tableType = selector.getAttribute('data-table-type');
            updateTableDisplay(tableType, 10);
        });
    }, 100);

    // Double Check 確認機制
    let currentAction = null;
    let currentUserId = null;

    // 身分驗證操作按鈕事件
    document.getElementById('verificationApproveBtn').addEventListener('click', function() {
        currentAction = 'approve';
        document.getElementById('doubleCheckMessage').textContent = '確定要審核通過此會員的身分驗證嗎？';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    document.getElementById('verificationRejectBtn').addEventListener('click', function() {
        currentAction = 'reject';
        document.getElementById('doubleCheckMessage').textContent = '確定要拒絕此會員的身分驗證嗎？';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 房東驗證操作按鈕事件
    document.getElementById('landlordApproveBtn').addEventListener('click', function() {
        currentAction = 'landlord_approve';
        document.getElementById('doubleCheckMessage').textContent = '確定要審核通過此會員申請並設為房東嗎？';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    document.getElementById('landlordRejectBtn').addEventListener('click', function() {
        currentAction = 'landlord_reject';
        document.getElementById('doubleCheckMessage').textContent = '確定要拒絕此會員的房東申請嗎？';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 最終確認按鈕事件
    document.getElementById('finalConfirmBtn').addEventListener('click', function() {
        // 這裡執行實際的驗證操作 - 後續開發
        switch(currentAction) {
            case 'approve':
                console.log('執行身分驗證通過操作');
                break;
            case 'reject':
                console.log('執行身分驗證拒絕操作');
                break;
            case 'landlord_approve':
                console.log('執行房東申請通過操作');
                break;
            case 'landlord_reject':
                console.log('執行房東申請拒絕操作');
                break;
        }
        
        // 關閉所有Modal
        var doubleCheckModal = bootstrap.Modal.getInstance(document.getElementById('doubleCheckModal'));
        var verificationModal = bootstrap.Modal.getInstance(document.getElementById('verificationModal'));
        var landlordModal = bootstrap.Modal.getInstance(document.getElementById('landlordVerificationModal'));
        
        doubleCheckModal.hide();
        if (verificationModal) verificationModal.hide();
        if (landlordModal) landlordModal.hide();
        
        // 重置狀態
        currentAction = null;
        currentUserId = null;
        
        // 顯示操作成功訊息 - 後續可改為實際的成功回饋
        alert('操作已執行完成');
    });

    // 通用分頁事件初始化函數
    function initTabEvents(suffix, checkboxClass, bulkBtnId, selectAllId, tabName) {
        // 搜尋功能
        var searchBtn = document.getElementById('searchBtn' + suffix);
        if (searchBtn) {
            searchBtn.addEventListener('click', function() {
                performSearch(suffix, tabName);
            });
        }

        // 重置功能
        var resetBtn = document.getElementById('resetBtn' + suffix);
        if (resetBtn) {
            resetBtn.addEventListener('click', function() {
                resetFormFields(suffix);
                performSearch(suffix, tabName);
                console.log('重置' + tabName + '搜尋條件');
            });
        }

        // 進階搜尋展開/收合功能
        var advancedBtn = document.getElementById('advancedSearchBtn' + suffix);
        if (advancedBtn) {
            advancedBtn.addEventListener('click', function() {
                var collapseElement = document.getElementById('advancedSearchCollapse' + suffix);
                var icon = this.querySelector('i');
                var collapse = new bootstrap.Collapse(collapseElement, {
                    toggle: false
                });
                
                if (collapseElement.classList.contains('show')) {
                    collapse.hide();
                    icon.className = 'fas fa-chevron-down';
                    this.innerHTML = '<i class="fas fa-chevron-down"></i> 進階篩選';
                } else {
                    collapse.show();
                    icon.className = 'fas fa-chevron-up';
                    this.innerHTML = '<i class="fas fa-chevron-up"></i> 收合篩選';
                }
            });
        }

        // 全選功能
        var selectAllBox = document.getElementById(selectAllId);
        if (selectAllBox) {
            selectAllBox.addEventListener('change', function() {
                var self = this;
                var checkboxes = document.querySelectorAll('.' + checkboxClass);
                var bulkMessageBtn = document.getElementById(bulkBtnId);
                
                checkboxes.forEach(function(checkbox) {
                    checkbox.checked = self.checked;
                });
                
                if (bulkMessageBtn) {
                    bulkMessageBtn.disabled = !this.checked;
                }
            });
        }

        // 個別勾選功能
        document.addEventListener('change', function(e) {
            if (e.target.classList.contains(checkboxClass)) {
                var allCheckboxes = document.querySelectorAll('.' + checkboxClass);
                var checkedBoxes = document.querySelectorAll('.' + checkboxClass + ':checked');
                var selectAllBox = document.getElementById(selectAllId);
                var bulkMessageBtn = document.getElementById(bulkBtnId);
                
                if (selectAllBox) {
                    selectAllBox.checked = allCheckboxes.length === checkedBoxes.length;
                }
                if (bulkMessageBtn) {
                    bulkMessageBtn.disabled = checkedBoxes.length === 0;
                }
            }
        });
    }

    // 重置表單欄位函數
    function resetFormFields(suffix) {
        var fields = [
            'searchInput' + suffix,
            'searchField' + suffix,
            'verificationStatus' + suffix,
            'idUploadStatus' + suffix,
            'accountStatus' + suffix,
            'gender' + suffix,
            'isLandlord' + suffix,
            'primaryRentalCityID' + suffix,
            'residenceCityID' + suffix,
            'registerDateStart' + suffix,
            'registerDateEnd' + suffix,
            'lastLoginDateStart' + suffix,
            'lastLoginDateEnd' + suffix,
            'applyDateStart' + suffix,
            'applyDateEnd' + suffix,
            'identityVerifiedDateStart',
            'identityVerifiedDateEnd'
        ];

        fields.forEach(function(fieldId) {
            var element = document.getElementById(fieldId);
            if (element) {
                if (element.type === 'select-one') {
                    element.value = element.id.includes('searchField') ? 'memberID' : '';
                } else {
                    element.value = '';
                }
            }
        });
    }

    // 排序功能初始化
    function initSortingEvents() {
        // 移除現有的排序事件監聽器
        document.querySelectorAll('.sortable').forEach(function(header) {
            var newHeader = header.cloneNode(true);
            header.parentNode.replaceChild(newHeader, header);
        });

        // 重新添加排序事件監聽器
        document.querySelectorAll('.sortable').forEach(function(header) {
            header.addEventListener('click', function() {
                var sortField = this.getAttribute('data-sort');
                var tableType = this.getAttribute('data-table');
                var icon = this.querySelector('.sort-icon');
                var currentDirection = icon.getAttribute('data-direction');
                
                // 重置其他排序圖示
                var currentTable = this.closest('table');
                currentTable.querySelectorAll('.sort-icon').forEach(function(otherIcon) {
                    if (otherIcon !== icon) {
                        otherIcon.className = 'bi bi-sort-up-alt sort-icon';
                        otherIcon.setAttribute('data-direction', 'none');
                        otherIcon.classList.remove('active');
                    }
                });
                
                // 切換排序方向
                var newDirection, newIconClass;
                if (currentDirection === 'none' || currentDirection === 'desc') {
                    newDirection = 'asc';
                    newIconClass = 'bi bi-sort-up-alt sort-icon active';
                } else {
                    newDirection = 'desc';
                    newIconClass = 'bi bi-sort-down sort-icon active';
                }
                
                icon.className = newIconClass;
                icon.setAttribute('data-direction', newDirection);
                
                // 執行排序
                sortTable(currentTable, sortField, newDirection);
                
                console.log('排序: ' + sortField + ' (' + newDirection + ')');
            });
        });
    }

    // 表格排序功能
    function sortTable(table, sortField, direction) {
        var tbody = table.querySelector('tbody');
        var rows = Array.from(tbody.querySelectorAll('tr'));
        
        rows.sort(function(a, b) {
            var aValue = getSortValue(a, sortField);
            var bValue = getSortValue(b, sortField);
            
            // 如果是日期欄位，使用日期比較
            if (sortField === 'registerDate' || sortField === 'lastLogin' || sortField === 'verificationTime') {
                // 處理日期格式 YYYY-MM-DD
                var aDate = new Date(aValue);
                var bDate = new Date(bValue);
                
                if (direction === 'asc') {
                    return aDate - bDate;
                } else {
                    return bDate - aDate;
                }
            } else {
                // 其他欄位使用字串比較
                if (direction === 'asc') {
                    return aValue.localeCompare(bValue, 'zh-TW');
                } else {
                    return bValue.localeCompare(aValue, 'zh-TW');
                }
            }
        });
        
        // 重新排列表格行
        rows.forEach(function(row) {
            tbody.appendChild(row);
        });
        
        // 排序後重新應用分頁
        var tableTypeAttr = table.getAttribute('data-table-type');
        var pageSizeSelector = document.querySelector('#pageSize' + tableTypeAttr);
        var currentPageSize = pageSizeSelector ? parseInt(pageSizeSelector.value) : 10;
        
        updateTableDisplay(tableTypeAttr, currentPageSize, 1); // 排序後回到第一頁
    }

    // 獲取排序值
    function getSortValue(row, sortField) {
        var cells = row.querySelectorAll('td');
        
        switch(sortField) {
            case 'registerDate':
                // 註冊日期在第4個欄位 (index 3)
                var dateValue = cells[3].textContent.trim();
                return dateValue === '--' ? '0000-00-00' : dateValue;
            case 'lastLogin':
                // 上次登入時間在第5個欄位 (index 4)
                var loginValue = cells[4].textContent.trim();
                return loginValue === '--' ? '0000-00-00' : loginValue;
            case 'verificationTime':
                // 申請驗證時間在第6個欄位 (index 5)
                var verificationValue = cells[5].textContent.trim();
                return verificationValue === '--' ? '0000-00-00' : verificationValue;
            default:
                return '';
        }
    }

    // 載入筆數功能初始化
    function initPageSizeEvents() {
        document.querySelectorAll('.page-size-selector').forEach(function(selector) {
            selector.addEventListener('change', function() {
                var pageSize = parseInt(this.value);
                var tableType = this.getAttribute('data-table-type');
                updateTableDisplay(tableType, pageSize);
            });
        });
    }

    // 更新表格顯示
    function updateTableDisplay(tableType, pageSize, currentPage) {
        if (!currentPage) currentPage = 1;
        
        var tableId = getTableId(tableType);
        var table = document.querySelector(tableId + ' table tbody');
        var paginationContainer = document.querySelector(tableId + ' nav[aria-label*="分頁"]');
        
        if (table) {
            var allRows = Array.from(table.querySelectorAll('tr'));
            var totalRows = allRows.length;
            var totalPages = Math.ceil(totalRows / pageSize);
            
            // 隱藏所有行
            allRows.forEach(function(row) {
                row.style.display = 'none';
            });
            
            // 顯示指定頁面的行
            showPageRows(allRows, currentPage, pageSize);
            
            // 更新分頁按鈕
            updatePagination(paginationContainer, currentPage, totalPages, pageSize, allRows, tableType);
        }
    }
    
    // 顯示指定頁面的行
    function showPageRows(allRows, currentPage, pageSize) {
        var startIndex = (currentPage - 1) * pageSize;
        var endIndex = Math.min(startIndex + pageSize, allRows.length);
        
        for (var i = startIndex; i < endIndex; i++) {
            if (allRows[i]) {
                allRows[i].style.display = '';
            }
        }
    }

    // 獲取對應的表格ID
    function getTableId(tableType) {
        switch(tableType) {
            case 'all':
                return '#all-users';
            case 'pending':
                return '#pending-verification';
            case 'landlord':
                return '#pending-landlord';
            default:
                return '#all-users';
        }
    }

    // 更新分頁
    function updatePagination(paginationContainer, currentPage, totalPages, pageSize, allRows, tableType) {
        if (!paginationContainer) return;
        
        var paginationList = paginationContainer.querySelector('.pagination');
        if (!paginationList) return;
        
        // 如果只有一頁或沒有資料，隱藏分頁
        if (totalPages <= 1) {
            paginationContainer.style.display = 'none';
            return;
        } else {
            paginationContainer.style.display = 'block';
        }
        
        // 清空現有分頁
        paginationList.innerHTML = '';
        
        // 上一頁按鈕
        var prevItem = document.createElement('li');
        prevItem.className = currentPage === 1 ? 'page-item disabled' : 'page-item';
        prevItem.innerHTML = '<a class="page-link" href="#" tabindex="-1">上一頁</a>';
        if (currentPage > 1) {
            prevItem.querySelector('a').addEventListener('click', function(e) {
                e.preventDefault();
                showPage(currentPage - 1, totalPages, pageSize, allRows, paginationContainer, tableType);
            });
        }
        paginationList.appendChild(prevItem);
        
        // 頁碼按鈕
        for (var i = 1; i <= totalPages; i++) {
            var pageItem = document.createElement('li');
            pageItem.className = i === currentPage ? 'page-item active' : 'page-item';
            pageItem.innerHTML = '<a class="page-link" href="#">' + i + '</a>';
            
            if (i !== currentPage) {
                (function(pageNum) {
                    pageItem.querySelector('a').addEventListener('click', function(e) {
                        e.preventDefault();
                        showPage(pageNum, totalPages, pageSize, allRows, paginationContainer, tableType);
                    });
                })(i);
            }
            paginationList.appendChild(pageItem);
        }
        
        // 下一頁按鈕
        var nextItem = document.createElement('li');
        nextItem.className = currentPage === totalPages ? 'page-item disabled' : 'page-item';
        nextItem.innerHTML = '<a class="page-link" href="#">下一頁</a>';
        if (currentPage < totalPages) {
            nextItem.querySelector('a').addEventListener('click', function(e) {
                e.preventDefault();
                showPage(currentPage + 1, totalPages, pageSize, allRows, paginationContainer, tableType);
            });
        }
        paginationList.appendChild(nextItem);
    }

    // 顯示指定頁面
    function showPage(pageNum, totalPages, pageSize, allRows, paginationContainer, tableType) {
        // 隱藏所有行
        allRows.forEach(function(row) {
            row.style.display = 'none';
        });
        
        // 顯示當前頁的行
        showPageRows(allRows, pageNum, pageSize);
        
        // 更新分頁按鈕狀態
        updatePagination(paginationContainer, pageNum, totalPages, pageSize, allRows, tableType);
    }

    // 執行搜尋篩選
    function performSearch(suffix, tabName) {
        var searchInput = document.getElementById('searchInput' + suffix);
        var searchField = document.getElementById('searchField' + suffix);
        var verificationStatus = document.getElementById('verificationStatus' + suffix);
        var accountStatus = document.getElementById('accountStatus' + suffix);
        var gender = document.getElementById('gender' + suffix);
        var isLandlord = document.getElementById('isLandlord' + suffix);
        var primaryRentalCityID = document.getElementById('primaryRentalCityID' + suffix);
        var residenceCityID = document.getElementById('residenceCityID' + suffix);
        var registerDateStart = document.getElementById('registerDateStart' + suffix);
        var registerDateEnd = document.getElementById('registerDateEnd' + suffix);
        var lastLoginDateStart = document.getElementById('lastLoginDateStart' + suffix);
        var lastLoginDateEnd = document.getElementById('lastLoginDateEnd' + suffix);
        var applyDateStart = document.getElementById('applyDateStart' + suffix);
        var applyDateEnd = document.getElementById('applyDateEnd' + suffix);

        // 獲取篩選條件
        var filters = {
            searchText: searchInput ? searchInput.value.trim() : '',
            searchField: searchField ? searchField.value : 'memberID',
            verificationStatus: verificationStatus ? verificationStatus.value : '',
            accountStatus: accountStatus ? accountStatus.value : '',
            gender: gender ? gender.value : '',
            isLandlord: isLandlord ? isLandlord.value : '',
            primaryRentalCityID: primaryRentalCityID ? primaryRentalCityID.value : '',
            residenceCityID: residenceCityID ? residenceCityID.value : '',
            registerDateStart: registerDateStart ? registerDateStart.value : '',
            registerDateEnd: registerDateEnd ? registerDateEnd.value : '',
            lastLoginDateStart: lastLoginDateStart ? lastLoginDateStart.value : '',
            lastLoginDateEnd: lastLoginDateEnd ? lastLoginDateEnd.value : '',
            applyDateStart: applyDateStart ? applyDateStart.value : '',
            applyDateEnd: applyDateEnd ? applyDateEnd.value : ''
        };

        // 獲取對應的表格
        var tableId = getTableId(getTableTypeFromSuffix(suffix));
        var table = document.querySelector(tableId + ' table tbody');
        
        if (!table) return;

        var allRows = Array.from(table.querySelectorAll('tr'));
        var filteredRows = allRows.filter(function(row) {
            return matchesFilters(row, filters);
        });

        // 隱藏所有行
        allRows.forEach(function(row) {
            row.style.display = 'none';
        });

        // 顯示符合條件的行
        filteredRows.forEach(function(row) {
            row.style.display = '';
        });

        // 更新分頁顯示
        var tableType = getTableTypeFromSuffix(suffix);
        var paginationContainer = document.querySelector(tableId + ' nav[aria-label*="分頁"]');
        var pageSizeSelector = document.querySelector('#pageSize' + tableType);
        var currentPageSize = pageSizeSelector ? parseInt(pageSizeSelector.value) : 10;
        
        updatePaginationForFiltered(paginationContainer, filteredRows, currentPageSize, tableType);

        console.log('搜尋' + tabName + '，找到 ' + filteredRows.length + ' 筆符合條件的資料');
    }

    // 檢查行是否符合篩選條件
    function matchesFilters(row, filters) {
        var cells = row.querySelectorAll('td');
        if (cells.length === 0) return false;

        // 關鍵字搜尋
        if (filters.searchText) {
            var searchValue = getSearchValue(row, filters.searchField);
            if (!searchValue.toLowerCase().includes(filters.searchText.toLowerCase())) {
                return false;
            }
        }

        // 身分證驗證狀態
        if (filters.verificationStatus) {
            var verificationCell = cells[5]; // 身分證驗證狀態欄位
            if (verificationCell) {
                var verificationText = verificationCell.textContent.trim();
                var matches = false;
                
                switch(filters.verificationStatus) {
                    case 'verified':
                        matches = verificationText.includes('已驗證') || verificationText.includes('通過');
                        break;
                    case 'pending':
                        matches = verificationText.includes('等待驗證');
                        break;
                    case 'unverified':
                        matches = verificationText.includes('尚未驗證');
                        break;
                }
                
                if (!matches) return false;
            }
        }

        // 帳戶狀態
        if (filters.accountStatus) {
            var statusCell = cells[7]; // 帳戶狀態欄位 (修正為第7個欄位)
            if (statusCell) {
                var statusText = statusCell.textContent.trim();
                var matches = false;
                
                switch(filters.accountStatus) {
                    case 'active':
                        matches = statusText.includes('啟用');
                        break;
                    case 'inactive':
                        matches = statusText.includes('停用');
                        break;
                }
                
                if (!matches) return false;
            }
        }

        // 進階篩選條件
        // 性別篩選
        if (filters.gender) {
            var gender = row.getAttribute('data-gender');
            if (filters.gender === 'other') {
                if (gender === '1' || gender === '2') return false;
            } else {
                if (gender !== filters.gender) return false;
            }
        }

        // 房東身分篩選
        if (filters.isLandlord !== '') {
            var isLandlord = row.getAttribute('data-is-landlord');
            if (isLandlord !== filters.isLandlord) return false;
        }

        // 居住縣市篩選
        if (filters.residenceCityID) {
            var residenceCity = row.getAttribute('data-residence-city');
            if (residenceCity !== filters.residenceCityID) return false;
        }

        // 偏好租賃縣市篩選
        if (filters.primaryRentalCityID) {
            var primaryRentalCity = row.getAttribute('data-primary-rental-city');
            if (primaryRentalCity !== filters.primaryRentalCityID) return false;
        }

        // 註冊日期範圍篩選
        if (filters.registerDateStart || filters.registerDateEnd) {
            var registrationDate = row.getAttribute('data-registration-date');
            if (registrationDate && registrationDate !== '--') {
                var regDate = new Date(registrationDate);
                if (filters.registerDateStart) {
                    var startDate = new Date(filters.registerDateStart);
                    if (regDate < startDate) return false;
                }
                if (filters.registerDateEnd) {
                    var endDate = new Date(filters.registerDateEnd);
                    if (regDate > endDate) return false;
                }
            }
        }

        // 最後登入日期範圍篩選
        if (filters.lastLoginDateStart || filters.lastLoginDateEnd) {
            var lastLoginDate = row.getAttribute('data-last-login');
            if (lastLoginDate && lastLoginDate !== '--') {
                var loginDate = new Date(lastLoginDate);
                if (filters.lastLoginDateStart) {
                    var startDate = new Date(filters.lastLoginDateStart);
                    if (loginDate < startDate) return false;
                }
                if (filters.lastLoginDateEnd) {
                    var endDate = new Date(filters.lastLoginDateEnd);
                    if (loginDate > endDate) return false;
                }
            }
        }

        return true;
    }

    // 獲取搜尋值
    function getSearchValue(row, searchField) {
        var cells = row.querySelectorAll('td');
        
        switch(searchField) {
            case 'memberID':
                return cells[1] ? cells[1].textContent.trim() : '';
            case 'memberName':
                return cells[2] ? cells[2].textContent.trim() : '';
            case 'email':
                return cells[3] ? cells[3].textContent.trim() : '';
            case 'phoneNumber':
                return cells[4] ? cells[4].textContent.trim() : '';
            case 'nationalIdNo':
                // 身分證字號可能在不同欄位，需要根據實際表格結構調整
                return cells[7] ? cells[7].textContent.trim() : '';
            default:
                return '';
        }
    }

    // 從後綴獲取表格類型
    function getTableTypeFromSuffix(suffix) {
        switch(suffix) {
            case '':
                return 'all';
            case 'Pending':
                return 'pending';
            case 'Landlord':
                return 'landlord';
            default:
                return 'all';
        }
    }

    // 為篩選結果更新分頁
    function updatePaginationForFiltered(paginationContainer, filteredRows, pageSize, tableType) {
        if (!paginationContainer) return;
        
        var totalRows = filteredRows.length;
        var totalPages = Math.ceil(totalRows / pageSize);
        
        // 如果只有一頁或沒有資料，隱藏分頁
        if (totalPages <= 1) {
            paginationContainer.style.display = 'none';
            return;
        } else {
            paginationContainer.style.display = 'block';
        }
        
        // 顯示第一頁的篩選結果
        showFilteredPageRows(filteredRows, 1, pageSize);
        
        // 更新分頁按鈕
        updatePagination(paginationContainer, 1, totalPages, pageSize, filteredRows, tableType);
    }

    // 顯示篩選結果的指定頁面
    function showFilteredPageRows(filteredRows, currentPage, pageSize) {
        // 先隱藏所有篩選結果
        filteredRows.forEach(function(row) {
            row.style.display = 'none';
        });
        
        // 顯示當前頁的行
        var startIndex = (currentPage - 1) * pageSize;
        var endIndex = Math.min(startIndex + pageSize, filteredRows.length);
        
        for (var i = startIndex; i < endIndex; i++) {
            if (filteredRows[i]) {
                filteredRows[i].style.display = '';
            }
        }
    }

    // 動態載入城市資料
    function loadCityData() {
        fetch('/Admin/GetCities')
            .then(response => response.json())
            .then(cities => {
                // 更新所有城市下拉選單
                document.querySelectorAll('[id^="residenceCityID"], [id^="primaryRentalCityID"]').forEach(function(select) {
                    // 保留「全部」選項
                    var defaultOption = select.querySelector('option[value=""]');
                    select.innerHTML = '';
                    if (defaultOption) {
                        select.appendChild(defaultOption.cloneNode(true));
                    }
                    
                    // 添加城市選項
                    cities.forEach(function(city) {
                        var option = document.createElement('option');
                        option.value = city.id;
                        option.textContent = city.name;
                        select.appendChild(option);
                    });
                });
            })
            .catch(error => {
                console.error('載入城市資料失敗:', error);
            });
    }
});