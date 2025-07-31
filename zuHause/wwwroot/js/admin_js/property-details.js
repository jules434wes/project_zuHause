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
    // 優先從全域變數取得
    if (window.currentPropertyProofUrl && window.currentPropertyProofUrl.trim() !== '') {
        const proofUrl = window.currentPropertyProofUrl.trim();
        
        // 如果是相對路徑（Azure Blob Storage格式），需要轉換成完整URL
        if (!proofUrl.startsWith('http')) {
            // Azure Blob Storage 基礎URL
            const blobBaseUrl = 'https://zuhauseimg.blob.core.windows.net/zuhaus-images';
            return `${blobBaseUrl}/${proofUrl}`;
        }
        
        return proofUrl;
    }
    
    // 備用：從頁面中尋找 PDF URL
    const proofLink = document.querySelector('a[href*=".pdf"]');
    if (proofLink) {
        return proofLink.href;
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
    document.getElementById('finalConfirmBtn').addEventListener('click', async function() {
        // 顯示載入狀態
        showFinalConfirmLoading();
        
        try {
            let response;
            
            switch(currentAction) {
                case 'approve_property':
                    response = await executeApproveProperty();
                    break;
                case 'reject_property':
                    response = await executeRejectProperty();
                    break;
                case 'mark_as_paid':
                    response = await executeMarkAsPaid();
                    break;
                case 'force_remove':
                    response = await executeForceRemove();
                    break;
                default:
                    throw new Error('未知的操作類型');
            }
            
            if (response.success) {
                // 關閉所有Modal
                closeAllPropertyModals();
                
                // 顯示成功訊息
                showSuccessMessage(response.message);
                
                // 重新載入頁面以更新狀態
                setTimeout(() => {
                    location.reload();
                }, 1500);
            } else {
                console.error('API 返回失敗:', response);
                throw new Error(response.message || '操作失敗');
            }
        } catch (error) {
            console.error('執行操作時發生錯誤:', error);
            console.error('錯誤詳情:', {
                action: currentAction,
                propertyId: currentPropertyId,
                error: error.message,
                stack: error.stack
            });
            showErrorMessage('操作失敗：' + error.message);
        } finally {
            hideFinalConfirmLoading();
        }
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

// ========== 房源審核 API 調用函數 ==========

// 執行審核通過
async function executeApproveProperty() {
    const formData = new FormData();
    formData.append('propertyId', currentPropertyId);
    
    const response = await fetch('/Admin/ApproveProperty', {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    });
    
    return await response.json();
}

// 執行駁回申請
async function executeRejectProperty() {
    const rejectionReason = document.getElementById('rejectionReason').value.trim();
    
    // 驗證必要參數
    if (!currentPropertyId) {
        throw new Error('房源ID未設定');
    }
    
    if (!rejectionReason) {
        throw new Error('請填寫駁回原因');
    }
    
    console.log('執行駁回申請 - propertyId:', currentPropertyId, 'reason:', rejectionReason);
    
    const formData = new FormData();
    formData.append('propertyId', currentPropertyId);
    formData.append('rejectionReason', rejectionReason);
    
    const response = await fetch('/Admin/RejectProperty', {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    });
    
    if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    
    const result = await response.json();
    console.log('駁回申請 API 回應:', result);
    
    return result;
}

// 執行標記已付款
async function executeMarkAsPaid() {
    const paymentNote = document.getElementById('paymentNote').value.trim();
    
    const formData = new FormData();
    formData.append('propertyId', currentPropertyId);
    formData.append('paymentNote', paymentNote);
    
    const response = await fetch('/Admin/MarkPropertyAsPaid', {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    });
    
    return await response.json();
}

// 執行強制下架
async function executeForceRemove() {
    const removeReason = document.getElementById('forceRemoveReason').value.trim();
    
    const formData = new FormData();
    formData.append('propertyId', currentPropertyId);
    formData.append('removeReason', removeReason);
    
    const response = await fetch('/Admin/ForceRemoveProperty', {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    });
    
    return await response.json();
}

// 關閉所有房源相關Modal
function closeAllPropertyModals() {
    const modalIds = ['doubleCheckModal', 'approveModal', 'rejectModal', 'markAsPaidModal', 'forceRemoveModal'];
    
    modalIds.forEach(modalId => {
        const modalElement = document.getElementById(modalId);
        if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            if (modalInstance) {
                modalInstance.hide();
            }
        }
    });
    
    // 重置狀態
    currentAction = null;
}

// 顯示最終確認載入狀態
function showFinalConfirmLoading() {
    const btn = document.getElementById('finalConfirmBtn');
    btn.disabled = true;
    btn.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        處理中...
    `;
}

// 隱藏最終確認載入狀態
function hideFinalConfirmLoading() {
    const btn = document.getElementById('finalConfirmBtn');
    btn.disabled = false;
    btn.innerHTML = `
        <i class="bi bi-check me-1"></i>確定執行
    `;
}

// 顯示成功訊息
function showSuccessMessage(message) {
    showToast(message, 'success');
}

// 顯示錯誤訊息
function showErrorMessage(message) {
    showToast(message, 'error');
}

// Toast 訊息顯示功能
function showToast(message, type = 'info') {
    // 創建或取得toast容器
    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }
    
    // 設定顏色和圖示
    let bgClass, iconClass;
    switch (type) {
        case 'success':
            bgClass = 'bg-success';
            iconClass = 'bi-check-circle-fill';
            break;
        case 'error':
            bgClass = 'bg-danger';
            iconClass = 'bi-exclamation-circle-fill';
            break;
        case 'warning':
            bgClass = 'bg-warning';
            iconClass = 'bi-exclamation-triangle-fill';
            break;
        default:
            bgClass = 'bg-info';
            iconClass = 'bi-info-circle-fill';
    }
    
    // 創建toast HTML
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast ${bgClass} text-white" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header ${bgClass} text-white border-0">
                <i class="bi ${iconClass} me-2"></i>
                <strong class="me-auto">系統訊息</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${escapeHtml(message)}
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    // 顯示toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: type === 'error' ? 5000 : 3000
    });
    
    toast.show();
    
    // 清理：toast隱藏後移除元素
    toastElement.addEventListener('hidden.bs.toast', function() {
        toastElement.remove();
    });
}

// 照片模態視窗相關功能
let currentPhotoIndex = 0;
let photoUrls = [];

// 全域函數 - 開啟照片放大Modal
function openPhotoModal(photoIndex) {
    // 收集所有照片URL
    const photoImages = document.querySelectorAll('[data-image-url]');
    photoUrls = Array.from(photoImages).map(img => img.getAttribute('data-image-url'));
    
    if (photoUrls.length === 0) {
        showErrorMessage('未找到照片');
        return;
    }
    
    currentPhotoIndex = photoIndex || 0;
    showPhoto(currentPhotoIndex);
    
    const photoModal = new bootstrap.Modal(document.getElementById('photoEnlargeModal'));
    photoModal.show();
}

// 顯示指定索引的照片
function showPhoto(index) {
    if (index < 0 || index >= photoUrls.length) {
        return;
    }
    
    const enlargedPhoto = document.getElementById('enlargedPhoto');
    const photoCounter = document.getElementById('photoCounter');
    const prevBtn = document.getElementById('prevPhotoBtn');
    const nextBtn = document.getElementById('nextPhotoBtn');
    
    // 更新照片
    enlargedPhoto.src = photoUrls[index];
    
    // 更新計數器
    photoCounter.textContent = `${index + 1} / ${photoUrls.length}`;
    
    // 更新按鈕狀態
    prevBtn.style.display = index === 0 ? 'none' : 'block';
    nextBtn.style.display = index === photoUrls.length - 1 ? 'none' : 'block';
    
    currentPhotoIndex = index;
}

// 全域函數 - 照片導航
function navigatePhoto(direction) {
    const newIndex = currentPhotoIndex + direction;
    if (newIndex >= 0 && newIndex < photoUrls.length) {
        showPhoto(newIndex);
    }
}

// 鍵盤快捷鍵支援
document.addEventListener('keydown', function(e) {
    const photoModal = document.getElementById('photoEnlargeModal');
    const modalInstance = bootstrap.Modal.getInstance(photoModal);
    
    if (modalInstance && modalInstance._isShown) {
        switch(e.key) {
            case 'ArrowLeft':
                e.preventDefault();
                navigatePhoto(-1);
                break;
            case 'ArrowRight':
                e.preventDefault();
                navigatePhoto(1);
                break;
            case 'Escape':
                e.preventDefault();
                modalInstance.hide();
                break;
        }
    }
});