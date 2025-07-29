// 會員詳情頁面 JavaScript
// user-details.js 檔案

// 全域變數
var currentMemberId = null;
var currentIdNumber = null;
var currentAction = null;

// 通用函數：開啟確認 Modal 並處理 z-index
function showDoubleCheckModal(needHigherZIndex = false) {
    var doubleCheckModalElement = document.getElementById('doubleCheckModal');
    var doubleCheckModal = new bootstrap.Modal(doubleCheckModalElement, {
        backdrop: 'static',
        keyboard: false
    });
    
    if (needHigherZIndex) {
        // 監聽 Modal 顯示事件，設置更高的 z-index (用於疊加在其他 Modal 上)
        doubleCheckModalElement.addEventListener('shown.bs.modal', function() {
            // 設置第二個 Modal 的 z-index 比第一個更高
            const firstModal = document.getElementById('verifyIdModal');
            const firstModalZIndex = window.getComputedStyle(firstModal).zIndex;
            const newZIndex = parseInt(firstModalZIndex) + 10;
            
            doubleCheckModalElement.style.zIndex = newZIndex;
            
            // 同時設置 backdrop 的 z-index
            const backdrop = document.querySelector('.modal-backdrop:last-child');
            if (backdrop) {
                backdrop.style.zIndex = newZIndex - 1;
            }
        }, { once: true }); // 只執行一次
    }
    
    doubleCheckModal.show();
}

// 測試函數已移除

// 測試函數是否可用
window.openVerifyIdModal = function() {
    // 開啟身分驗證 Modal
    
    // 檢查 modal 元素是否存在
    const modalElement = document.getElementById('verifyIdModal');
    if (!modalElement) {
        console.error('找不到 verifyIdModal 元素');
        alert('Modal 元素不存在');
        return;
    }
    // 先顯示簡單訊息
    const documentsArea = document.getElementById('identityDocumentsArea');
    if (documentsArea) {
        documentsArea.innerHTML = `
            <div class="col-12 text-center">
                <div class="alert alert-info">
                    <i class="bi bi-info-circle me-2"></i>
                    會員ID: ${currentMemberId || '未設定'}
                </div>
            </div>
        `;
    } else {
        console.error('找不到 identityDocumentsArea 元素');
    }
    
    try {
        var verifyModal = new bootstrap.Modal(modalElement, {
            backdrop: 'static',
            keyboard: false
        });
        verifyModal.show();
        
        // Modal 開啟後再載入檔案
        setTimeout(() => {
            if (typeof loadIdentityDocuments === 'function') {
                loadIdentityDocuments();
            } else {
                console.error('loadIdentityDocuments 函數不存在');
            }
        }, 500);
    } catch (error) {
        console.error('開啟 Modal 時發生錯誤:', error);
        alert('Modal 開啟失敗: ' + error.message);
    }
};

// 載入身分證檔案
function loadIdentityDocuments() {
    if (!currentMemberId) {
        console.error('會員ID 為空');
        showDocumentError('無法取得會員ID');
        return;
    }
    
    // 檢查會員ID是否為數字
    const memberIdNum = parseInt(currentMemberId, 10);
    if (isNaN(memberIdNum) || memberIdNum <= 0) {
        console.error('會員ID 格式不正確:', currentMemberId);
        showDocumentError('會員ID 格式不正確');
        return;
    }

    const documentsArea = document.getElementById('identityDocumentsArea');
    
    // 顯示載入狀態
    documentsArea.innerHTML = `
        <div class="col-12 text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">載入中...</span>
            </div>
            <p class="mt-2 text-muted">載入身分證檔案中...</p>
        </div>
    `;

    // 調用API載入檔案
    const apiUrl = `/Admin/GetMemberIdentityDocuments?memberId=${memberIdNum}`;
    // 調用 API 載入身分證檔案
    
    fetch(apiUrl)
        .then(response => {
            // API 回應處理
            
            if (!response.ok) {
                console.error('網路錯誤:', response.status, response.statusText);
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success && data.data && data.data.documents && Array.isArray(data.data.documents) && data.data.documents.length > 0) {
                displayIdentityDocuments(data.data.documents);
            } else {
                showApiErrorMessage(data.message, memberIdNum);
            }
        })
        .catch(error => {
            console.error('載入檔案錯誤：', error);
            // 不停止 modal，只顯示錯誤訊息
            const documentsArea = document.getElementById('identityDocumentsArea');
            documentsArea.innerHTML = `
                <div class="col-12 text-center">
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle me-2"></i>
                        網路錯誤，無法載入檔案
                        <br><small>錯誤詳情: ${error.message}</small>
                        <br><small>會員ID: ${memberIdNum}</small>
                    </div>
                    <div class="mt-3">
                        <button class="btn btn-primary btn-sm me-2" onclick="loadIdentityDocuments()">
                            <i class="bi bi-arrow-clockwise me-1"></i>重新載入
                        </button>
                        <button class="btn btn-outline-info btn-sm" onclick="window.open('/Admin/GetMemberIdentityDocuments?memberId=' + ${memberIdNum}, '_blank')">
                            <i class="bi bi-box-arrow-up-right me-1"></i>直接檢查 API
                        </button>
                    </div>
                </div>
            `;
        });
}

// 顯示身分證檔案 - 使用外部連結方式
function displayIdentityDocuments(documents) {
    
    const documentsArea = document.getElementById('identityDocumentsArea');
    if (!documentsArea) {
        console.error('找不到 identityDocumentsArea 元素');
        return;
    }
    
    if (!documents || documents.length === 0) {
        documentsArea.innerHTML = `
            <div class="col-12 text-center">
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    尚未找到身分證上傳檔案
                </div>
            </div>
        `;
        return;
    }

    let html = '';
    documents.forEach(doc => {
        // 處理每個檔案物件
        
        // 確保 fileUrl 是完整的 URL，用於外部連結
        // 數據庫中的路徑格式: /uploads/userPhoto/filename.jpg
        // 注意: API 返回的是 fileUrl（小寫 f）
        const fileUrl = doc.fileUrl && doc.fileUrl.startsWith('http') 
            ? doc.fileUrl 
            : `${window.location.origin}${doc.fileUrl || ''}`;
        
        // 處理檔案 URL
        
        html += `
            <div class="col-md-6">
                <div class="card border-info">
                    <div class="card-header bg-info bg-opacity-10">
                        <h6 class="card-title mb-0">
                            <i class="bi bi-file-image me-2"></i>${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}
                        </h6>
                    </div>
                    <div class="card-body text-center">
                        <!-- 內嵌圖片顯示區域 -->
                        <div class="mb-3 p-2 bg-light border rounded" style="min-height: 300px; display: flex; align-items: center; justify-content: center;">
                            <img src="${fileUrl}" 
                                 alt="${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}" 
                                 class="img-fluid" 
                                 style="max-height: 280px; max-width: 100%; object-fit: contain; cursor: pointer;"
                                 onclick="openImageModal('${fileUrl}', '${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}')"
                                 onerror="this.style.display='none'; this.nextElementSibling.style.display='block';"
                                 loading="lazy">
                            <!-- 圖片載入失敗時的備用內容 -->
                            <div style="display: none; color: #6c757d;" class="text-center p-4">
                                <i class="bi bi-image-alt fs-1 mb-2"></i>
                                <p>圖片載入失敗</p>
                                <a href="${fileUrl}" target="_blank" class="btn btn-outline-primary btn-sm">
                                    <i class="bi bi-box-arrow-up-right me-1"></i>在新分頁開啟
                                </a>
                            </div>
                        </div>
                        <!-- 檔案資訊 -->
                        <div class="document-info mb-3">
                            <p class="mb-1"><small class="text-muted">檔案名稱：${escapeHtml(doc.fileName || doc.FileName || '')}</small></p>
                            <p class="mb-1"><small class="text-muted">上傳時間：${doc.uploadedAt || doc.UploadedAt || ''}</small></p>
                            <p class="mb-0"><small class="text-muted">檔案大小：${doc.fileSize || doc.FileSize || ''}</small></p>
                        </div>
                        <!-- 操作按鈕 -->
                        <div class="d-flex justify-content-center gap-2">
                            <button class="btn btn-outline-primary btn-sm" onclick="openImageModal('${fileUrl}', '${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}')">
                                <i class="bi bi-zoom-in me-1"></i>放大檢視
                            </button>
                            <a href="${fileUrl}" target="_blank" class="btn btn-outline-secondary btn-sm">
                                <i class="bi bi-box-arrow-up-right me-1"></i>新分頁開啟
                            </a>
                            <a href="${fileUrl}" download="${escapeHtml(doc.fileName || doc.FileName || '')}" class="btn btn-outline-success btn-sm">
                                <i class="bi bi-download me-1"></i>下載
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });
    
    documentsArea.innerHTML = html;
}

// 顯示檔案載入錯誤
function showDocumentError(message) {
    const documentsArea = document.getElementById('identityDocumentsArea');
    documentsArea.innerHTML = `
        <div class="col-12 text-center">
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-triangle me-2"></i>
                ${escapeHtml(message)}
            </div>
        </div>
    `;
}

// 圖片放大檢視 Modal
window.openImageModal = function(imageUrl, title) {
    // 開啟圖片放大檢視
    
    // 檢查是否已經存在 Modal
    let imageModal = document.getElementById('imageZoomModal');
    
    if (!imageModal) {
        // 創建 Modal HTML
        const modalHtml = `
            <div class="modal fade" id="imageZoomModal" tabindex="-1" aria-labelledby="imageZoomModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-xl modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="imageZoomModalLabel">
                                <i class="bi bi-zoom-in me-2"></i>圖片放大檢視
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body text-center p-2">
                            <img id="zoomImage" src="" alt="" class="img-fluid" style="max-height: 80vh; max-width: 100%;">
                        </div>
                        <div class="modal-footer">
                            <a id="downloadLink" href="" download="" class="btn btn-success">
                                <i class="bi bi-download me-1"></i>下載原始檔案
                            </a>
                            <a id="openNewTabLink" href="" target="_blank" class="btn btn-primary">
                                <i class="bi bi-box-arrow-up-right me-1"></i>在新分頁開啟
                            </a>
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                                <i class="bi bi-x me-1"></i>關閉
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        imageModal = document.getElementById('imageZoomModal');
    }
    
    // 更新 Modal 內容
    document.getElementById('imageZoomModalLabel').innerHTML = `<i class="bi bi-zoom-in me-2"></i>${escapeHtml(title)}`;
    document.getElementById('zoomImage').src = imageUrl;
    document.getElementById('zoomImage').alt = escapeHtml(title);
    document.getElementById('downloadLink').href = imageUrl;
    document.getElementById('openNewTabLink').href = imageUrl;
    
    // 顯示 Modal
    const modal = new bootstrap.Modal(imageModal);
    modal.show();
};

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
    // 啟用帳號功能 - 後續開發
}

// 開啟身分證檔案檢視 Modal (純檢視功能)
window.openIdDocumentModal = function() {
    if (!currentMemberId) {
        alert('無法獲取會員ID');
        return;
    }
    
    // 檢查 modal 元素是否存在
    const modalElement = document.getElementById('idDocumentViewModal');
    if (!modalElement) {
        console.error('找不到 idDocumentViewModal 元素');
        alert('Modal 元素不存在');
        return;
    }
    
    // 顯示 Modal
    const viewModal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: false
    });
    viewModal.show();
    
    // Modal 開啟後載入檔案
    setTimeout(() => {
        loadIdDocumentsForView();
    }, 300);
};

// 載入身分證檔案用於純檢視
function loadIdDocumentsForView() {
    if (!currentMemberId) {
        showIdDocumentViewError('無法取得會員ID');
        return;
    }
    
    const memberIdNum = parseInt(currentMemberId, 10);
    if (isNaN(memberIdNum) || memberIdNum <= 0) {
        showIdDocumentViewError('會員ID 格式不正確');
        return;
    }

    const documentsViewArea = document.getElementById('idDocumentsViewArea');
    
    // 顯示載入狀態
    documentsViewArea.innerHTML = `
        <div class="col-12 text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">載入中...</span>
            </div>
            <p class="mt-2 text-muted">載入身分證檔案中...</p>
        </div>
    `;

    // 調用API載入檔案
    const apiUrl = `/Admin/GetMemberIdentityDocuments?memberId=${memberIdNum}`;
    
    fetch(apiUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success && data.data && data.data.documents && Array.isArray(data.data.documents) && data.data.documents.length > 0) {
                displayIdDocumentsForView(data.data.documents);
            } else {
                showIdDocumentViewError(data.message || '無法載入身分證檔案');
            }
        })
        .catch(error => {
            console.error('載入檔案錯誤：', error);
            showIdDocumentViewError('網路錯誤，無法載入檔案：' + error.message);
        });
}

// 顯示身分證檔案用於純檢視
function displayIdDocumentsForView(documents) {
    const documentsViewArea = document.getElementById('idDocumentsViewArea');
    
    if (!documents || documents.length === 0) {
        documentsViewArea.innerHTML = `
            <div class="col-12 text-center">
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    尚未找到身分證上傳檔案
                </div>
            </div>
        `;
        return;
    }

    let html = '';
    documents.forEach(doc => {
        const fileUrl = doc.fileUrl && doc.fileUrl.startsWith('http') 
            ? doc.fileUrl 
            : `${window.location.origin}${doc.fileUrl || ''}`;
        
        html += `
            <div class="col-md-6 mb-3">
                <div class="card border-secondary">
                    <div class="card-header bg-light">
                        <h6 class="card-title mb-0">
                            <i class="bi bi-file-image me-2"></i>${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}
                        </h6>
                    </div>
                    <div class="card-body text-center p-2">
                        <div class="mb-2 p-2 bg-light border rounded" style="min-height: 200px; display: flex; align-items: center; justify-content: center;">
                            <img src="${fileUrl}" 
                                 alt="${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}" 
                                 class="img-fluid" 
                                 style="max-height: 180px; max-width: 100%; object-fit: contain; cursor: pointer;"
                                 onclick="openImageModal('${fileUrl}', '${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}')"
                                 onerror="this.style.display='none'; this.nextElementSibling.style.display='block';"
                                 loading="lazy">
                            <div style="display: none; color: #6c757d;" class="text-center p-3">
                                <i class="bi bi-image-alt fs-3 mb-2"></i>
                                <p class="small">圖片載入失敗</p>
                                <a href="${fileUrl}" target="_blank" class="btn btn-outline-primary btn-sm">
                                    <i class="bi bi-box-arrow-up-right me-1"></i>在新分頁開啟
                                </a>
                            </div>
                        </div>
                        <div class="d-flex justify-content-center gap-1">
                            <button class="btn btn-outline-primary btn-sm" onclick="openImageModal('${fileUrl}', '${doc.typeDisplay || doc.TypeDisplay || '身分證檔案'}')">
                                <i class="bi bi-zoom-in me-1"></i>放大
                            </button>
                            <a href="${fileUrl}" target="_blank" class="btn btn-outline-secondary btn-sm">
                                <i class="bi bi-box-arrow-up-right me-1"></i>新分頁
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });
    
    documentsViewArea.innerHTML = html;
}

// 顯示身分證檔案檢視錯誤
function showIdDocumentViewError(message) {
    const documentsViewArea = document.getElementById('idDocumentsViewArea');
    documentsViewArea.innerHTML = `
        <div class="col-12 text-center">
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-triangle me-2"></i>
                ${escapeHtml(message)}
            </div>
        </div>
    `;
}

// 顯示無檔案訊息
function showNoDocumentsMessage(memberId) {
    const documentsArea = document.getElementById('identityDocumentsArea');
    documentsArea.innerHTML = `
        <div class="col-12 text-center">
            <div class="alert alert-warning">
                <i class="bi bi-exclamation-triangle me-2"></i>
                尚未找到身分證上傳檔案
                <br><small>會員ID: ${memberId}</small>
            </div>
            <button class="btn btn-secondary btn-sm" onclick="loadIdentityDocuments()">
                <i class="bi bi-arrow-clockwise me-1"></i>重新載入
            </button>
        </div>
    `;
}

// 顯示 API 錯誤訊息
function showApiErrorMessage(message, memberId) {
    const documentsArea = document.getElementById('identityDocumentsArea');
    documentsArea.innerHTML = `
        <div class="col-12 text-center">
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-triangle me-2"></i>
                ${message || 'API 呼叫失敗'}
                <br><small>會員ID: ${memberId}</small>
            </div>
            <button class="btn btn-secondary btn-sm" onclick="loadIdentityDocuments()">
                <i class="bi bi-arrow-clockwise me-1"></i>重新載入
            </button>
        </div>
    `;
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

    // Double Check 確認機制已移至全域變數

    // 身分驗證確認按鈕事件
    document.getElementById('confirmVerifyBtn').addEventListener('click', function() {
        var idNumber = document.getElementById('idNumberInput').value.trim();
        
        // 驗證身分證字號格式
        if (!idNumber) {
            showToast('請輸入身分證字號！', 'warning');
            return;
        }
        
        if (idNumber.length !== 10) {
            showToast('身分證字號格式不正確，應為10碼！', 'warning');
            return;
        }
        
        // 簡單的身分證字號格式驗證
        var idPattern = /^[A-Z][12][0-9]{8}$/;
        if (!idPattern.test(idNumber)) {
            showToast('身分證字號格式不正確！', 'warning');
            return;
        }
        
        // 儲存當前輸入的身分證字號
        currentIdNumber = idNumber;
        currentAction = 'verify_identity';
        
        // 更新確認訊息
        document.getElementById('doubleCheckMessage').textContent = '確定要為此會員進行身分驗證嗎？此操作不可逆。';
        
        // 顯示身分證號確認區域
        document.getElementById('idNumberConfirmArea').style.display = 'block';
        document.getElementById('confirmIdNumber').textContent = idNumber;
        
        // 直接開啟第二個 Modal (疊加在第一個 Modal 上面)
        showDoubleCheckModal(true);
    });

    // 停用帳號確認按鈕事件
    document.getElementById('confirmDeactivateBtn').addEventListener('click', function() {
        currentAction = 'deactivate_account';
        document.getElementById('doubleCheckMessage').textContent = '確定要停用此會員帳號嗎？停用後該會員將無法登入系統。';
        showDoubleCheckModal(false); // 不需要更高的 z-index
    });

    // 最終確認按鈕事件
    document.getElementById('finalConfirmBtn').addEventListener('click', function() {
        switch(currentAction) {
            case 'verify_identity':
                executeIdentityVerification();
                break;
            case 'deactivate_account':
                // 執行帳號停用操作
                // 後續實作帳號停用邏輯
                break;
        }
    });

    // Double Check 確認框的checkbox控制
    document.getElementById('confirmCheckbox').addEventListener('change', function() {
        document.getElementById('finalConfirmBtn').disabled = !this.checked;
    });
    
    // 當 doubleCheckModal 關閉時重置 checkbox 和隱藏身分證號確認區域
    document.getElementById('doubleCheckModal').addEventListener('hidden.bs.modal', function() {
        document.getElementById('confirmCheckbox').checked = false;
        document.getElementById('finalConfirmBtn').disabled = true;
        document.getElementById('idNumberConfirmArea').style.display = 'none';
        
        // 重置 z-index
        const doubleCheckModal = document.getElementById('doubleCheckModal');
        doubleCheckModal.style.zIndex = '';
        
        // 如果操作未完成，重置狀態但保持第一個 Modal 開啟
        if (currentAction !== null) {
            // 只有在操作成功完成時才重置狀態，取消操作時保持狀態
            // currentAction 和 currentIdNumber 保持不變，讓使用者可以重新嘗試
        }
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
            
            // 執行表格排序
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
window.setCurrentMemberId = function(memberId) {
    currentMemberId = memberId;
    // 設定當前會員ID
};

// ============ 發送系統訊息功能 ============

// 全域函數 - 開啟發送系統訊息Modal
function openSendMessageModal() {
    if (!currentMemberId) {
        alert('無法獲取會員ID');
        return;
    }
    
    // 重置表單
    resetUserMessageForm();
    
    // 開啟Modal
    const sendMessageModal = new bootstrap.Modal(document.getElementById('sendMessageModal'), {
        backdrop: 'static',
        keyboard: false
    });
    sendMessageModal.show();
    
    // 初始化字數統計
    updateUserCharacterCount();
}

// 重置用戶訊息表單
function resetUserMessageForm() {
    document.getElementById('userMessageTitle').value = '';
    document.getElementById('userMessageContent').value = '';
    document.getElementById('userConfirmCheckbox').checked = false;
    document.getElementById('userFinalSendBtn').disabled = true;
    updateUserCharacterCount();
}

// 更新字數統計
function updateUserCharacterCount() {
    const content = document.getElementById('userMessageContent').value;
    const count = content.length;
    const maxLength = 2000;
    
    const charCountElement = document.getElementById('userContentCharCount');
    charCountElement.textContent = `${count}/${maxLength}`;
    
    // 字數接近上限時改變顏色
    if (count > maxLength * 0.9) {
        charCountElement.className = 'float-end text-warning';
    } else if (count >= maxLength) {
        charCountElement.className = 'float-end text-danger';
    } else {
        charCountElement.className = 'float-end';
    }
}

// 開啟模板選擇Modal
function openUserTemplateModal() {
    loadUserMessageTemplates();
    const userTemplateModal = new bootstrap.Modal(document.getElementById('userTemplateModal'));
    userTemplateModal.show();
}

// 載入系統訊息模板
function loadUserMessageTemplates() {
    const templateList = document.getElementById('userTemplateList');
    
    // 顯示載入狀態
    templateList.innerHTML = `
        <div class="text-center text-muted py-4">
            <i class="bi bi-hourglass-split fs-1 mb-2 d-block"></i>
            <div class="fw-semibold">載入模板中...</div>
        </div>
    `;
    
    // 調用 API 載入模板
    fetch('/Admin/GetSystemMessageTemplates')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                displayUserTemplates(data.data);
            } else {
                showUserTemplateError('載入模板失敗：' + data.message);
            }
        })
        .catch(error => {
            showUserTemplateError('載入模板時發生錯誤');
        });
}

// 顯示模板列表
function displayUserTemplates(templates) {
    const templateList = document.getElementById('userTemplateList');
    
    if (!templates || templates.length === 0) {
        templateList.innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="bi bi-inbox fs-1 mb-2 d-block"></i>
                <div class="fw-semibold">尚無可用模板</div>
                <div class="small">目前沒有系統訊息模板</div>
            </div>
        `;
        return;
    }
    
    let html = '';
    templates.forEach(template => {
        html += `
            <a href="#" class="list-group-item list-group-item-action user-template-item" 
               data-template-id="${template.templateID}"
               data-template-title="${escapeHtml(template.title)}" 
               data-template-content="${escapeHtml(template.templateContent)}"
               onclick="selectUserTemplate(this); return false;">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <h6 class="mb-1">
                            <i class="bi bi-file-text text-primary me-1"></i>
                            ${escapeHtml(template.title)}
                        </h6>
                        <p class="mb-1 small text-muted">${escapeHtml(template.contentPreview)}</p>
                        <small class="text-info">建立日期：${template.createdAt}</small>
                    </div>
                    <i class="bi bi-chevron-right text-muted"></i>
                </div>
            </a>
        `;
    });
    
    templateList.innerHTML = html;
}

// 顯示模板載入錯誤
function showUserTemplateError(message) {
    document.getElementById('userTemplateList').innerHTML = `
        <div class="text-center text-danger py-4">
            <i class="bi bi-exclamation-triangle fs-1 mb-2 d-block"></i>
            <div class="fw-semibold">載入失敗</div>
            <div class="small">${escapeHtml(message)}</div>
        </div>
    `;
}

// 選擇模板
function selectUserTemplate(element) {
    const templateTitle = element.getAttribute('data-template-title');
    const templateContent = element.getAttribute('data-template-content');
    
    // 插入模板內容到表單
    document.getElementById('userMessageTitle').value = templateTitle;
    document.getElementById('userMessageContent').value = templateContent;
    
    // 更新字數統計
    updateUserCharacterCount();
    
    // 關閉模板選擇Modal
    const userTemplateModal = bootstrap.Modal.getInstance(document.getElementById('userTemplateModal'));
    userTemplateModal.hide();
    
    // 顯示成功提示
    // 已插入模板內容
}

// 顯示確認發送Modal
function showUserMessageConfirmModal() {
    // 驗證表單
    if (!validateUserMessageForm()) {
        return;
    }
    
    // 重置確認複選框
    document.getElementById('userConfirmCheckbox').checked = false;
    document.getElementById('userFinalSendBtn').disabled = true;
    
    // 顯示確認Modal
    const userMessageConfirmModal = new bootstrap.Modal(document.getElementById('userMessageConfirmModal'), {
        backdrop: 'static',
        keyboard: false
    });
    userMessageConfirmModal.show();
}

// 驗證用戶訊息表單
function validateUserMessageForm() {
    let isValid = true;
    const errors = [];
    
    // 檢查標題
    const title = document.getElementById('userMessageTitle').value.trim();
    if (!title) {
        errors.push('請輸入訊息標題');
        isValid = false;
    } else if (title.length > 100) {
        errors.push('標題長度不能超過100個字元');
        isValid = false;
    }
    
    // 檢查內容
    const content = document.getElementById('userMessageContent').value.trim();
    if (!content) {
        errors.push('請輸入訊息內容');
        isValid = false;
    } else if (content.length > 2000) {
        errors.push('內容長度不能超過2000個字元');
        isValid = false;
    }
    
    // 顯示錯誤訊息
    if (!isValid) {
        alert('表單驗證失敗：\n' + errors.join('\n'));
    }
    
    return isValid;
}

// 發送用戶訊息
function sendUserMessage() {
    if (!currentMemberId) {
        alert('無法獲取會員ID');
        return;
    }
    
    // 顯示載入狀態
    showUserSendingState();
    
    // 準備表單資料
    const formData = {
        messageTitle: document.getElementById('userMessageTitle').value,
        messageContent: document.getElementById('userMessageContent').value,
        messageCategory: 'SYSTEM',  // 固定為 SYSTEM
        audienceType: 'INDIVIDUAL', // 固定為個別用戶
        selectedUserId: currentMemberId
    };
    
    // 發送請求
    fetch('/Admin/SendSystemMessage', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams(formData)
    })
    .then(response => response.json())
    .then(data => {
        hideUserSendingState();
        
        if (data.success) {
            // 關閉所有Modal
            const confirmModal = bootstrap.Modal.getInstance(document.getElementById('userMessageConfirmModal'));
            const sendModal = bootstrap.Modal.getInstance(document.getElementById('sendMessageModal'));
            
            confirmModal.hide();
            sendModal.hide();
            
            // 顯示成功訊息
            alert('成功：' + data.message);
        } else {
            alert('發送失敗：' + data.message + (data.error ? '\n錯誤詳情：' + data.error : ''));
        }
    })
    .catch(error => {
        hideUserSendingState();
        alert('發送訊息時發生錯誤：' + error.message);
    });
}

// 顯示發送中狀態
function showUserSendingState() {
    const btn = document.getElementById('userFinalSendBtn');
    btn.disabled = true;
    btn.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        發送中...
    `;
}

// 隱藏發送中狀態
function hideUserSendingState() {
    const btn = document.getElementById('userFinalSendBtn');
    btn.disabled = false;
    btn.innerHTML = `
        <i class="bi bi-send-fill"></i> 確認發送
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

// 處理模板搜尋
function handleUserTemplateSearch() {
    const keyword = document.getElementById('userTemplateSearch').value.toLowerCase().trim();
    const templateItems = document.querySelectorAll('.user-template-item');
    
    if (!keyword) {
        templateItems.forEach(item => item.style.display = 'block');
        return;
    }
    
    templateItems.forEach(item => {
        const title = item.getAttribute('data-template-title').toLowerCase();
        const content = item.getAttribute('data-template-content').toLowerCase();
        
        if (title.includes(keyword) || content.includes(keyword)) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// 在 DOMContentLoaded 事件中添加新的事件監聽器
document.addEventListener('DOMContentLoaded', function() {
    // 原有的事件監聽器...
    
    // 用戶訊息內容字數統計
    const userMessageContent = document.getElementById('userMessageContent');
    if (userMessageContent) {
        userMessageContent.addEventListener('input', updateUserCharacterCount);
    }
    
    // 用戶確認發送複選框
    const userConfirmCheckbox = document.getElementById('userConfirmCheckbox');
    if (userConfirmCheckbox) {
        userConfirmCheckbox.addEventListener('change', function() {
            document.getElementById('userFinalSendBtn').disabled = !this.checked;
        });
    }
    
    // 模板搜尋
    const userTemplateSearch = document.getElementById('userTemplateSearch');
    if (userTemplateSearch) {
        userTemplateSearch.addEventListener('input', debounce(handleUserTemplateSearch, 200));
    }
});

// 防抖函數
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// ============ 身分驗證功能 ============

// 執行身分驗證
function executeIdentityVerification() {
    if (!currentMemberId || !currentIdNumber) {
        showToast('無法取得會員ID或身分證字號', 'error');
        return;
    }
    
    // 顯示載入狀態
    showFinalConfirmLoading();
    
    // 準備表單資料
    const formData = new FormData();
    formData.append('memberId', currentMemberId);
    formData.append('nationalIdNo', currentIdNumber);
    
    // 發送請求
    fetch('/Admin/ApproveIdentityVerification', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        hideFinalConfirmLoading();
        
        if (data.success) {
            // 關閉所有Modal
            closeAllVerificationModals();
            
            // 顯示成功訊息
            showToast('身分驗證審核通過！', 'success');
            
            // 重新整理頁面以更新會員狀態
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            showToast('審核失敗：' + data.message, 'error');
        }
    })
    .catch(error => {
        hideFinalConfirmLoading();
        showToast('執行身分驗證時發生錯誤：' + error.message, 'error');
    });
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
        <i class="fas fa-check me-1"></i>確定執行
    `;
}

// 關閉所有驗證相關Modal
function closeAllVerificationModals() {
    const doubleCheckModal = bootstrap.Modal.getInstance(document.getElementById('doubleCheckModal'));
    const verifyModal = bootstrap.Modal.getInstance(document.getElementById('verifyIdModal'));
    
    if (doubleCheckModal) doubleCheckModal.hide();
    if (verifyModal) verifyModal.hide();
    
    // 重置狀態
    currentAction = null;
    currentIdNumber = null;
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