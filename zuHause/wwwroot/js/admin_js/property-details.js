// 房源詳情頁面 JavaScript
// 全域變數
var currentPropertyId = null;

// 全域函數 - 開啟通過審核Modal
function openApproveModal() {
    var approveModal = new bootstrap.Modal(document.getElementById('approveModal'), {
        backdrop: 'static',
        keyboard: false
    });
    approveModal.show();
}

// 全域函數 - 開啟駁回申請Modal
function openRejectModal() {
    var rejectModal = new bootstrap.Modal(document.getElementById('rejectModal'), {
        backdrop: 'static',
        keyboard: false
    });
    rejectModal.show();
}

// 全域函數 - 開啟手動標記已付款Modal
function openMarkAsPaidModal() {
    var markAsPaidModal = new bootstrap.Modal(document.getElementById('markAsPaidModal'), {
        backdrop: 'static',
        keyboard: false
    });
    markAsPaidModal.show();
}

// 全域函數 - 開啟強制下架Modal
function openForceRemoveModal() {
    var forceRemoveModal = new bootstrap.Modal(document.getElementById('forceRemoveModal'), {
        backdrop: 'static',
        keyboard: false
    });
    forceRemoveModal.show();
}

// 全域函數 - 開啟產權證明檢視Modal
window.openPropertyProofModal = function() {
    if (!currentPropertyId) {
        alert('無法獲取房源ID');
        return;
    }
    
    // 檢查 modal 元素是否存在
    const modalElement = document.getElementById('propertyProofModal');
    if (!modalElement) {
        console.error('找不到 propertyProofModal 元素');
        alert('Modal 元素不存在');
        return;
    }
    
    // 顯示 Modal
    const proofModal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: false
    });
    proofModal.show();
    
    // Modal 開啟後載入 PDF
    setTimeout(() => {
        loadPropertyProofPDF();
    }, 300);
};

// 載入產權證明 PDF
function loadPropertyProofPDF() {
    if (!currentPropertyId) {
        showProofError('無法取得房源ID');
        return;
    }

    const proofViewArea = document.getElementById('propertyProofViewArea');
    
    // 顯示載入狀態
    proofViewArea.innerHTML = `
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">載入中...</span>
        </div>
        <p class="mt-2 text-muted">載入產權證明文件中...</p>
    `;

    // 獲取PDF URL (這裡假設從頁面的某個地方取得，或者通過 API)
    // 實際實作時需要根據具體情況調整
    const pdfUrl = getPDFUrl();
    
    if (!pdfUrl) {
        showProofError('無法取得產權證明文件路徑');
        return;
    }

    // 顯示 PDF
    displayPDF(pdfUrl);
}

// 獲取 PDF URL
function getPDFUrl() {
    // 從頁面中尋找 PDF URL，可能從 data attribute 或其他方式取得
    const proofLink = document.querySelector('a[href*=".pdf"]');
    if (proofLink) {
        return proofLink.href;
    }
    
    // 或者從全域變數取得（需要在頁面載入時設定）
    if (window.currentPropertyProofUrl) {
        return window.currentPropertyProofUrl;
    }
    
    return null;
}

// 顯示 PDF
function displayPDF(pdfUrl) {
    const proofViewArea = document.getElementById('propertyProofViewArea');
    
    // 使用 iframe 顯示 PDF
    proofViewArea.innerHTML = `
        <div class="border rounded" style="height: 600px;">
            <iframe src="${pdfUrl}" 
                    width="100%" 
                    height="100%" 
                    style="border: none;"
                    title="產權證明文件">
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    您的瀏覽器不支援 PDF 預覽，請
                    <a href="${pdfUrl}" target="_blank" class="alert-link">點擊此處下載查看</a>
                </div>
            </iframe>
        </div>
        <div class="mt-3 text-center">
            <a href="${pdfUrl}" class="btn btn-outline-primary" target="_blank">
                <i class="bi bi-box-arrow-up-right me-1"></i>在新分頁開啟
            </a>
        </div>
    `;
}

// 顯示產權證明載入錯誤
function showProofError(message) {
    const proofViewArea = document.getElementById('propertyProofViewArea');
    proofViewArea.innerHTML = `
        <div class="alert alert-danger">
            <i class="bi bi-exclamation-triangle me-2"></i>
            ${escapeHtml(message)}
        </div>
    `;
}

// HTML 轉義函數
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, function(m) { return map[m]; });
}

document.addEventListener('DOMContentLoaded', function() {
    // 初始化頁籤功能
    var triggerTabList = [].slice.call(document.querySelectorAll('#propertyDetailsTabs button'))
    triggerTabList.forEach(function (triggerEl) {
        var tabTrigger = new bootstrap.Tab(triggerEl)
        
        triggerEl.addEventListener('click', function (event) {
            event.preventDefault()
            tabTrigger.show()
        })
    })

    // Double Check 確認機制
    let currentAction = null;

    // 通過審核確認按鈕事件
    document.getElementById('confirmApproveBtn').addEventListener('click', function() {
        currentAction = 'approve_property';
        document.getElementById('doubleCheckMessage').textContent = '確定要通過此屋源的審核嗎？通過後將通知房東進行付款。';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 駁回申請確認按鈕事件
    document.getElementById('confirmRejectBtn').addEventListener('click', function() {
        var rejectionReason = document.getElementById('rejectionReason').value;
        if (!rejectionReason.trim()) {
            alert('請填寫駁回原因！');
            return;
        }
        
        currentAction = 'reject_property';
        document.getElementById('doubleCheckMessage').textContent = '確定要駁回此屋源的申請嗎？系統將會通知房東駁回原因。';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 手動標記已付款確認按鈕事件
    document.getElementById('confirmMarkAsPaidBtn').addEventListener('click', function() {
        currentAction = 'mark_as_paid';
        document.getElementById('doubleCheckMessage').textContent = '確定要將此屋源標記為已付款嗎？請確認已收到款項。';
        var doubleCheckModal = new bootstrap.Modal(document.getElementById('doubleCheckModal'), {
            backdrop: 'static',
            keyboard: false
        });
        doubleCheckModal.show();
    });

    // 強制下架確認按鈕事件
    document.getElementById('confirmForceRemoveBtn').addEventListener('click', function() {
        var forceRemoveReason = document.getElementById('forceRemoveReason').value;
        if (!forceRemoveReason.trim()) {
            alert('請填寫下架原因！');
            return;
        }
        
        currentAction = 'force_remove';
        document.getElementById('doubleCheckMessage').textContent = '確定要強制下架此屋源嗎？下架後房東需要重新申請上架。';
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
            case 'approve_property':
                console.log('執行審核通過操作 - PropertyID: ' + currentPropertyId);
                break;
            case 'reject_property':
                console.log('執行駁回申請操作 - PropertyID: ' + currentPropertyId);
                break;
            case 'mark_as_paid':
                console.log('執行標記已付款操作 - PropertyID: ' + currentPropertyId);
                break;
            case 'force_remove':
                console.log('執行強制下架操作 - PropertyID: ' + currentPropertyId);
                break;
        }
        
        // 關閉所有Modal
        var doubleCheckModal = bootstrap.Modal.getInstance(document.getElementById('doubleCheckModal'));
        var approveModal = bootstrap.Modal.getInstance(document.getElementById('approveModal'));
        var rejectModal = bootstrap.Modal.getInstance(document.getElementById('rejectModal'));
        var markAsPaidModal = bootstrap.Modal.getInstance(document.getElementById('markAsPaidModal'));
        var forceRemoveModal = bootstrap.Modal.getInstance(document.getElementById('forceRemoveModal'));
        
        doubleCheckModal.hide();
        if (approveModal) approveModal.hide();
        if (rejectModal) rejectModal.hide();
        if (markAsPaidModal) markAsPaidModal.hide();
        if (forceRemoveModal) forceRemoveModal.hide();
        
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
            return cells[2].textContent.trim(); // 租期
        case 'complaintDate':
            return cells[3].textContent.trim(); // 投訴時間
        case 'operationTime':
            return cells[0].textContent.trim(); // 操作時間
        default:
            return '';
    }
}

// 設定當前房源ID的函數（供從外部呼叫）
function setCurrentPropertyId(propertyId) {
    currentPropertyId = propertyId;
}