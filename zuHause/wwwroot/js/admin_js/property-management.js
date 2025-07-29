// 房源管理頁面 JavaScript
// 全域變數
var currentPropertyId = null;

// 全域函數 - 開啟審核Modal
function openReviewModal(propertyId) {
    currentPropertyId = propertyId;
    var reviewModal = new bootstrap.Modal(document.getElementById('reviewModal'), {
        backdrop: 'static',
        keyboard: false
    });
    reviewModal.show();
}

// 全域函數 - 開啟文件查看Modal
function openDocumentModal(propertyId) {
    var documentModal = new bootstrap.Modal(document.getElementById('viewDocumentModal'), {
        backdrop: 'static',
        keyboard: false
    });
    documentModal.show();
}

document.addEventListener('DOMContentLoaded', function() {
    var triggerTabList = [].slice.call(document.querySelectorAll('#propertyTabs button'))
    triggerTabList.forEach(function (triggerEl) {
        var tabTrigger = new bootstrap.Tab(triggerEl)
        
        triggerEl.addEventListener('click', function (event) {
            event.preventDefault()
            tabTrigger.show()
        })
    })

    // 初始化各分頁的事件處理器
    initTabEvents('', 'property-checkbox', 'bulkMessageBtn', 'selectAllProperties', '全部屋源');
    initTabEvents('Pending', 'property-checkbox-pending', 'bulkMessageBtnPending', 'selectAllPropertiesPending', '待審核屋源');

    // 初始化排序功能
    initSortingEvents();

    // Double Check 確認機制
    let currentAction = null;

    // 審核操作按鈕事件
    document.getElementById('reviewApproveBtn').addEventListener('click', function() {
        currentAction = 'approve';
        document.getElementById('doubleCheckMessage').textContent = '確定要審核通過此屋源嗎？';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    document.getElementById('reviewRejectBtn').addEventListener('click', function() {
        currentAction = 'reject';
        document.getElementById('doubleCheckMessage').textContent = '確定要拒絕此屋源並要求重新審核嗎？';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 最終確認按鈕事件
    document.getElementById('finalConfirmBtn').addEventListener('click', function() {
        // 這裡執行實際的審核操作 - 後續開發
        switch(currentAction) {
            case 'approve':
                console.log('執行屋源審核通過操作 - PropertyID: ' + currentPropertyId);
                break;
            case 'reject':
                console.log('執行屋源審核拒絕操作 - PropertyID: ' + currentPropertyId);
                break;
        }
        
        // 關閉所有Modal
        var doubleCheckModal = bootstrap.Modal.getInstance(document.getElementById('doubleCheckModal'));
        var reviewModal = bootstrap.Modal.getInstance(document.getElementById('reviewModal'));
        
        doubleCheckModal.hide();
        if (reviewModal) reviewModal.hide();
        
        // 重置狀態
        currentAction = null;
        currentPropertyId = null;
        
        // 顯示操作成功訊息 - 後續可改為實際的成功回饋
        alert('操作已執行完成');
    });

    // 通用分頁事件初始化函數
    function initTabEvents(suffix, checkboxClass, bulkBtnId, selectAllId, tabName) {
        // 搜尋功能
        var searchBtn = document.getElementById('searchBtn' + suffix);
        if (searchBtn) {
            searchBtn.addEventListener('click', function() {
                performSearch(suffix);
            });
        }

        // 重置功能
        var resetBtn = document.getElementById('resetBtn' + suffix);
        if (resetBtn) {
            resetBtn.addEventListener('click', function() {
                resetFormFields(suffix);
                // 清除所有篩選參數並重新載入頁面
                window.location.href = window.location.pathname;
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
                    icon.className = 'bi bi-chevron-down';
                    this.innerHTML = '<i class="bi bi-chevron-down"></i> 進階篩選';
                } else {
                    collapse.show();
                    icon.className = 'bi bi-chevron-up';
                    this.innerHTML = '<i class="bi bi-chevron-up"></i> 收合篩選';
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
            'propertyStatus' + suffix,
            'documentUploadStatus' + suffix,
            'paymentStatus' + suffix,
            'cityID' + suffix,
            'rentMin' + suffix,
            'rentMax' + suffix,
            'publishDateStart' + suffix,
            'publishDateEnd' + suffix,
            'updateDateStart' + suffix,
            'updateDateEnd' + suffix,
            'applyDateStart' + suffix,
            'applyDateEnd' + suffix
        ];

        fields.forEach(function(fieldId) {
            var element = document.getElementById(fieldId);
            if (element) {
                if (element.type === 'select-one') {
                    element.value = element.id.includes('searchField') ? 'propertyID' : '';
                } else {
                    element.value = '';
                }
            }
        });
    }

    // 排序功能初始化
    function initSortingEvents() {
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
            
            if (direction === 'asc') {
                return aValue.localeCompare(bValue, 'zh-TW');
            } else {
                return bValue.localeCompare(aValue, 'zh-TW');
            }
        });
        
        // 重新排列表格行
        rows.forEach(function(row) {
            tbody.appendChild(row);
        });
    }

    // 獲取排序值
    function getSortValue(row, sortField) {
        var cells = row.querySelectorAll('td');
        var tableType = row.closest('table').querySelector('.sortable').getAttribute('data-table');
        
        switch(sortField) {
            case 'publishDate':
                return cells[4].textContent.trim(); // 建立時間 (all表格，第5欄)
            case 'applyTime':
                return cells[4].textContent.trim(); // 申請審核時間 (pending表格，第5欄)
            case 'expiryDate':
                return cells[6].textContent.trim(); // 上架期限 (all表格，第7欄)
            case 'updateTime':
                if (tableType === 'pending') {
                    return cells[6].textContent.trim(); // pending表格的更新時間 (第7欄)
                } else {
                    return cells[7].textContent.trim(); // all表格的更新時間 (第8欄)
                }
            default:
                return '';
        }
    }
});

// 分頁功能
let currentPageData = {
    'all': 1,
    'pending': 1
};

// 全域函數 - 分頁切換
function changePage(page, tableType) {
    if (page < 1) return;
    
    currentPageData[tableType] = page;
    
    // 重新載入頁面內容（這裡可以用 AJAX 來優化）
    // 暫時使用重新載入的方式，之後可以改成 AJAX
    const currentUrl = new URL(window.location);
    currentUrl.searchParams.set(tableType + '_page', page);
    window.location.href = currentUrl.toString();
}

// 獲取當前頁面
function getCurrentPage(tableType) {
    const urlParams = new URLSearchParams(window.location.search);
    return parseInt(urlParams.get(tableType + '_page')) || 1;
}

// 搜尋功能實現
function performSearch(suffix) {
    const url = new URL(window.location);
    
    // 清除舊的搜尋參數
    const searchParams = ['search', 'searchField', 'propertyStatus', 'paymentStatus', 'documentUploadStatus', 
                         'cityID', 'rentMin', 'rentMax', 'publishDateStart', 'publishDateEnd', 
                         'applyDateStart', 'applyDateEnd', 'updateDateStart', 'updateDateEnd'];
    searchParams.forEach(param => url.searchParams.delete(param));
    
    // 重置分頁到第一頁
    url.searchParams.delete('all_page');
    url.searchParams.delete('pending_page');
    
    // 添加新的搜尋參數
    const searchInput = document.getElementById('searchInput' + suffix);
    const searchField = document.getElementById('searchField' + suffix);
    const propertyStatus = document.getElementById('propertyStatus' + suffix);
    const paymentStatus = document.getElementById('paymentStatus' + suffix);
    const documentUploadStatus = document.getElementById('documentUploadStatus' + suffix);
    const cityID = document.getElementById('cityID' + suffix);
    const rentMin = document.getElementById('rentMin' + suffix);
    const rentMax = document.getElementById('rentMax' + suffix);
    const publishDateStart = document.getElementById('publishDateStart' + suffix);
    const publishDateEnd = document.getElementById('publishDateEnd' + suffix);
    const applyDateStart = document.getElementById('applyDateStart' + suffix);
    const applyDateEnd = document.getElementById('applyDateEnd' + suffix);
    const updateDateStart = document.getElementById('updateDateStart' + suffix);
    const updateDateEnd = document.getElementById('updateDateEnd' + suffix);
    
    if (searchInput && searchInput.value.trim()) {
        url.searchParams.set('search', searchInput.value.trim());
    }
    if (searchField && searchField.value) {
        url.searchParams.set('searchField', searchField.value);
    }
    if (propertyStatus && propertyStatus.value) {
        url.searchParams.set('propertyStatus', propertyStatus.value);
    }
    if (paymentStatus && paymentStatus.value) {
        url.searchParams.set('paymentStatus', paymentStatus.value);
    }
    if (documentUploadStatus && documentUploadStatus.value) {
        url.searchParams.set('documentUploadStatus', documentUploadStatus.value);
    }
    if (cityID && cityID.value) {
        url.searchParams.set('cityID', cityID.value);
    }
    if (rentMin && rentMin.value) {
        url.searchParams.set('rentMin', rentMin.value);
    }
    if (rentMax && rentMax.value) {
        url.searchParams.set('rentMax', rentMax.value);
    }
    if (publishDateStart && publishDateStart.value) {
        url.searchParams.set('publishDateStart', publishDateStart.value);
    }
    if (publishDateEnd && publishDateEnd.value) {
        url.searchParams.set('publishDateEnd', publishDateEnd.value);
    }
    if (applyDateStart && applyDateStart.value) {
        url.searchParams.set('applyDateStart', applyDateStart.value);
    }
    if (applyDateEnd && applyDateEnd.value) {
        url.searchParams.set('applyDateEnd', applyDateEnd.value);
    }
    if (updateDateStart && updateDateStart.value) {
        url.searchParams.set('updateDateStart', updateDateStart.value);
    }
    if (updateDateEnd && updateDateEnd.value) {
        url.searchParams.set('updateDateEnd', updateDateEnd.value);
    }
    
    // 跳轉到新的URL
    window.location.href = url.toString();
}

// 初始化分頁
document.addEventListener('DOMContentLoaded', function() {
    // 從 URL 參數獲取當前頁面
    currentPageData['all'] = getCurrentPage('all');
    currentPageData['pending'] = getCurrentPage('pending');
});