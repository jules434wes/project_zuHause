// 會員詳情頁面 JavaScript
// 全域變數
var currentMemberId = null;

// 全域函數 - 開啟身分驗證Modal
function openVerifyIdModal() {
    var verifyModal = new bootstrap.Modal(document.getElementById('verifyIdModal'), {
        backdrop: 'static',
        keyboard: false
    });
    verifyModal.show();
}

// 全域函數 - 開啟停用帳號Modal
function openDeactivateModal() {
    var deactivateModal = new bootstrap.Modal(document.getElementById('deactivateModal'), {
        backdrop: 'static',
        keyboard: false
    });
    deactivateModal.show();
}

// 全域函數 - 開啟啟用帳號Modal (預留功能)
function openActivateModal() {
    // 啟用帳號功能 - 後續開發
    console.log('啟用帳號功能 - MemberID: ' + currentMemberId);
}

// 全域函數 - 開啟身分證檔案查看Modal
function openIdDocumentModal() {
    var idDocumentModal = new bootstrap.Modal(document.getElementById('idDocumentModal'), {
        backdrop: 'static',
        keyboard: false
    });
    idDocumentModal.show();
}

document.addEventListener('DOMContentLoaded', function() {
    // 初始化頁籤功能
    var triggerTabList = [].slice.call(document.querySelectorAll('#memberDetailsTabs button'))
    triggerTabList.forEach(function (triggerEl) {
        var tabTrigger = new bootstrap.Tab(triggerEl)
        
        triggerEl.addEventListener('click', function (event) {
            event.preventDefault()
            tabTrigger.show()
        })
    })

    // Double Check 確認機制
    let currentAction = null;

    // 身分驗證確認按鈕事件
    document.getElementById('confirmVerifyBtn').addEventListener('click', function() {
        var idNumber = document.getElementById('idNumberInput').value;
        if (!idNumber) {
            alert('請輸入身分證字號！');
            return;
        }
        
        currentAction = 'verify_identity';
        document.getElementById('doubleCheckMessage').textContent = '確定要為此會員進行身分驗證嗎？此操作將授予房東資格且不可逆。';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 停用帳號確認按鈕事件
    document.getElementById('confirmDeactivateBtn').addEventListener('click', function() {
        currentAction = 'deactivate_account';
        document.getElementById('doubleCheckMessage').textContent = '確定要停用此會員帳號嗎？停用後該會員將無法登入系統。';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 最終確認按鈕事件
    document.getElementById('finalConfirmBtn').addEventListener('click', function() {
        // 這裡執行實際的操作 - 後續開發
        switch(currentAction) {
            case 'verify_identity':
                console.log('執行身分驗證操作 - MemberID: ' + currentMemberId);
                break;
            case 'deactivate_account':
                console.log('執行帳號停用操作 - MemberID: ' + currentMemberId);
                break;
        }
        
        // 關閉所有Modal
        var doubleCheckModal = bootstrap.Modal.getInstance(document.getElementById('doubleCheckModal'));
        var verifyModal = bootstrap.Modal.getInstance(document.getElementById('verifyIdModal'));
        var deactivateModal = bootstrap.Modal.getInstance(document.getElementById('deactivateModal'));
        
        doubleCheckModal.hide();
        if (verifyModal) verifyModal.hide();
        if (deactivateModal) deactivateModal.hide();
        
        // 重置狀態
        currentAction = null;
        
        // 顯示操作成功訊息 - 後續可改為實際的成功回饋
        alert('操作已執行完成');
    });

    // Double Check 確認框的checkbox控制
    document.getElementById('confirmCheckbox').addEventListener('change', function() {
        document.getElementById('finalConfirmBtn').disabled = !this.checked;
    });
    
    // 當 doubleCheckModal 關閉時重置 checkbox
    document.getElementById('doubleCheckModal').addEventListener('hidden.bs.modal', function() {
        document.getElementById('confirmCheckbox').checked = false;
        document.getElementById('finalConfirmBtn').disabled = true;
    });

    // 排序功能初始化
    initSortingEvents();
});

// 排序功能初始化
function initSortingEvents() {
    document.querySelectorAll('.sortable').forEach(function(header) {
        header.addEventListener('click', function() {
            var sortField = this.getAttribute('data-sort');
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
    
    switch(sortField) {
        case 'rentPeriod':
            return cells[3].textContent.trim(); // 租期
        case 'expiryDate':
            return cells[4].textContent.trim(); // 刊登到期日
        case 'complaintDate':
            return cells[2].textContent.trim(); // 投訴時間
        case 'orderDate':
            return cells[4].textContent.trim(); // 下單時間
        case 'lastReplyTime':
            return cells[4].textContent.trim(); // 最後回覆時間
        case 'complaintSubmitTime':
            return cells[4].textContent.trim(); // 投訴時間
        default:
            return '';
    }
}

// 設定當前會員ID的函數（供從外部呼叫）
function setCurrentMemberId(memberId) {
    currentMemberId = memberId;
}