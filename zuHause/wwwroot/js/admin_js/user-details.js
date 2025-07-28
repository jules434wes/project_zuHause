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
            console.error('載入模板錯誤：', error);
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
    console.log(`已插入模板：${templateTitle}`);
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
            console.error('伺服器返回錯誤：', data);
            alert('發送失敗：' + data.message + (data.error ? '\n錯誤詳情：' + data.error : ''));
        }
    })
    .catch(error => {
        hideUserSendingState();
        console.error('AJAX 錯誤：', error);
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