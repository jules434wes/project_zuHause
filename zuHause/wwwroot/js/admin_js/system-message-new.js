/**
 * 系統訊息新增頁面 JavaScript
 * 支援模板插入、用戶搜尋、表單驗證和訊息發送功能
 */

// 頁面載入後初始化
$(document).ready(function() {
    initializeSystemMessageNew();
});

/**
 * 初始化系統訊息新增頁面
 */
function initializeSystemMessageNew() {
    // 初始化事件監聽器
    bindEventListeners();
    
    // 初始化字數統計
    updateCharacterCount();
    
    // 初始化表單驗證
    initializeFormValidation();
    
    // 設定初始狀態
    toggleUserSelector();
}

/**
 * 綁定事件監聽器
 */
function bindEventListeners() {
    // 發送對象切換
    $('input[name="audienceType"]').on('change', toggleUserSelector);
    
    // 內容字數統計
    $('#messageContent').on('input', updateCharacterCount);
    
    // 用戶搜尋
    $('#userSearch').on('input', debounce(handleUserSearch, 300));
    
    // 確認發送複選框
    $('#confirmCheckbox').on('change', function() {
        $('#finalSendBtn').prop('disabled', !$(this).is(':checked'));
    });
    
    // 模板搜尋
    $('#templateSearch').on('input', debounce(handleTemplateSearch, 200));
    
    // 點擊外部關閉搜尋結果
    $(document).on('click', function(e) {
        if (!$(e.target).closest('#userSearch, #userSearchResults, #userSearchField').length) {
            hideUserSearchResults();
        }
    });
}

/**
 * 切換用戶選擇器顯示/隱藏
 */
function toggleUserSelector() {
    const isIndividual = $('#audienceIndividual').is(':checked');
    const userSelector = $('#individualUserSelector');
    
    if (isIndividual) {
        userSelector.show();
        $('#userSearch').prop('required', true);
    } else {
        userSelector.hide();
        $('#userSearch').prop('required', false);
        clearUserSelection();
    }
}

/**
 * 清除用戶選擇
 */
function clearUserSelection() {
    $('#userSearch').val('');
    $('#selectedUserId').val('');
    $('#userSearchResults').removeClass('show').empty();
}

/**
 * 更新字數統計
 */
function updateCharacterCount() {
    const content = $('#messageContent').val();
    const count = content.length;
    const maxLength = 2000;
    
    $('#contentCharCount').text(`${count}/${maxLength}`);
    
    // 字數接近上限時改變顏色
    if (count > maxLength * 0.9) {
        $('#contentCharCount').addClass('text-warning');
    } else if (count >= maxLength) {
        $('#contentCharCount').addClass('text-danger');
    } else {
        $('#contentCharCount').removeClass('text-warning text-danger');
    }
}

/**
 * 處理用戶搜尋
 */
function handleUserSearch() {
    const keyword = $('#userSearch').val().trim();
    const searchField = $('#userSearchField').val();
    
    if (keyword.length < 2) {
        hideUserSearchResults();
        return;
    }
    
    // 顯示載入狀態
    showUserSearchLoading();
    
    $.ajax({
        url: '/Admin/SearchMembersForMessage',
        method: 'POST',
        data: {
            keyword: keyword,
            searchField: searchField
        },
        success: function(response) {
            if (response.success) {
                displayUserSearchResults(response.data);
            } else {
                showError('搜尋用戶失敗：' + response.message);
                hideUserSearchResults();
            }
        },
        error: function(xhr, status, error) {
            console.error('用戶搜尋錯誤：', error);
            showError('搜尋用戶時發生錯誤');
            hideUserSearchResults();
        }
    });
}

/**
 * 顯示用戶搜尋載入狀態
 */
function showUserSearchLoading() {
    const resultsDiv = $('#userSearchResults');
    resultsDiv.html(`
        <div class="dropdown-item text-center">
            <i class="bi bi-hourglass-split me-2"></i>搜尋中...
        </div>
    `).addClass('show');
}

/**
 * 顯示用戶搜尋結果
 */
function displayUserSearchResults(users) {
    const resultsDiv = $('#userSearchResults');
    
    if (!users || users.length === 0) {
        resultsDiv.html(`
            <div class="dropdown-item text-muted text-center">
                <i class="bi bi-search me-2"></i>找不到符合條件的用戶
            </div>
        `).addClass('show');
        return;
    }
    
    let html = '';
    users.forEach(user => {
        html += `
            <a href="#" class="dropdown-item user-search-item" 
               data-user-id="${user.memberId}" 
               data-user-name="${escapeHtml(user.memberName)}"
               data-user-email="${escapeHtml(user.email)}">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <div class="fw-semibold">${escapeHtml(user.memberName)}</div>
                        <small class="text-muted">${escapeHtml(user.email)}</small>
                    </div>
                    <div class="text-end">
                        <small class="badge ${user.isLandlord ? 'bg-info' : 'bg-primary'}">${user.accountType}</small>
                        <br><small class="text-muted">ID: ${user.memberId}</small>
                    </div>
                </div>
            </a>
        `;
    });
    
    resultsDiv.html(html).addClass('show');
    
    // 綁定點擊事件
    $('.user-search-item').on('click', function(e) {
        e.preventDefault();
        selectUser($(this));
    });
}

/**
 * 隱藏用戶搜尋結果
 */
function hideUserSearchResults() {
    $('#userSearchResults').removeClass('show').empty();
}

/**
 * 選擇用戶
 */
function selectUser($item) {
    const userId = $item.data('user-id');
    const userName = $item.data('user-name');
    const userEmail = $item.data('user-email');
    
    $('#userSearch').val(`${userName} (${userEmail})`);
    $('#selectedUserId').val(userId);
    hideUserSearchResults();
}

/**
 * 開啟模板選擇 Modal
 */
function openTemplateModal() {
    loadSystemMessageTemplates();
    $('#templateModal').modal('show');
}

/**
 * 載入系統訊息模板
 */
function loadSystemMessageTemplates() {
    const templateList = $('#templateList');
    
    // 顯示載入狀態
    templateList.html(`
        <div class="text-center text-muted py-4">
            <i class="bi bi-hourglass-split fs-1 mb-2 d-block"></i>
            <div class="fw-semibold">載入模板中...</div>
        </div>
    `);
    
    $.ajax({
        url: '/Admin/GetSystemMessageTemplates',
        method: 'GET',
        success: function(response) {
            if (response.success) {
                displayTemplates(response.data);
            } else {
                showTemplateError('載入模板失敗：' + response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('載入模板錯誤：', error);
            showTemplateError('載入模板時發生錯誤');
        }
    });
}

/**
 * 顯示模板列表
 */
function displayTemplates(templates) {
    const templateList = $('#templateList');
    
    if (!templates || templates.length === 0) {
        templateList.html(`
            <div class="text-center text-muted py-4">
                <i class="bi bi-inbox fs-1 mb-2 d-block"></i>
                <div class="fw-semibold">尚無可用模板</div>
                <div class="small">目前沒有系統訊息模板</div>
            </div>
        `);
        return;
    }
    
    let html = '';
    templates.forEach(template => {
        html += `
            <a href="#" class="list-group-item list-group-item-action template-item" 
               data-template-id="${template.templateID}"
               data-template-title="${escapeHtml(template.title)}" 
               data-template-content="${escapeHtml(template.templateContent)}"
               onclick="selectTemplate(this); return false;">
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
    
    templateList.html(html);
}

/**
 * 顯示模板載入錯誤
 */
function showTemplateError(message) {
    $('#templateList').html(`
        <div class="text-center text-danger py-4">
            <i class="bi bi-exclamation-triangle fs-1 mb-2 d-block"></i>
            <div class="fw-semibold">載入失敗</div>
            <div class="small">${escapeHtml(message)}</div>
        </div>
    `);
}

/**
 * 選擇模板
 */
function selectTemplate(element) {
    const $element = $(element);
    const templateTitle = $element.data('template-title');
    const templateContent = $element.data('template-content');
    
    // 插入模板內容到表單
    $('#messageTitle').val(templateTitle);
    $('#messageContent').val(templateContent);
    
    // 更新字數統計
    updateCharacterCount();
    
    // 關閉 Modal
    $('#templateModal').modal('hide');
    
    // 顯示成功提示
    showSuccess(`已插入模板：${templateTitle}`);
}

/**
 * 處理模板搜尋
 */
function handleTemplateSearch() {
    const keyword = $('#templateSearch').val().toLowerCase().trim();
    const templateItems = $('.template-item');
    
    if (!keyword) {
        templateItems.show();
        return;
    }
    
    templateItems.each(function() {
        const $item = $(this);
        const title = $item.data('template-title').toLowerCase();
        const content = $item.data('template-content').toLowerCase();
        
        if (title.includes(keyword) || content.includes(keyword)) {
            $item.show();
        } else {
            $item.hide();
        }
    });
}

/**
 * 顯示確認發送 Modal
 */
function showConfirmSendModal() {
    // 驗證表單
    if (!validateForm()) {
        return;
    }
    
    // 設定確認訊息
    const audienceType = $('input[name="audienceType"]:checked').val();
    const audienceText = getAudienceText(audienceType);
    
    $('#confirmAudienceText').text(audienceText);
    
    // 如果是個別用戶，顯示用戶詳細資訊
    if (audienceType === 'INDIVIDUAL') {
        const userName = $('#userSearch').val();
        const userId = $('#selectedUserId').val();
        $('#confirmUserName').text(userName);
        $('#confirmUserId').text(`ID: ${userId}`);
        $('#confirmUserDetails').show();
    } else {
        $('#confirmUserDetails').hide();
    }
    
    // 重置確認複選框
    $('#confirmCheckbox').prop('checked', false);
    $('#finalSendBtn').prop('disabled', true);
    
    // 顯示 Modal
    $('#confirmSendModal').modal('show');
}

/**
 * 取得發送對象文字
 */
function getAudienceText(audienceType) {
    switch (audienceType) {
        case 'ALL_MEMBERS':
            return '全體會員';
        case 'ALL_LANDLORDS':
            return '全體房東';
        case 'INDIVIDUAL':
            return '指定用戶';
        default:
            return '未知對象';
    }
}


/**
 * 發送訊息
 */
function sendMessage() {
    // 顯示載入狀態
    showSendingState();
    
    // 準備表單資料
    const formData = {
        messageTitle: $('#messageTitle').val(),
        messageContent: $('#messageContent').val(),
        messageCategory: $('#messageCategory').val(),
        audienceType: $('input[name="audienceType"]:checked').val(),
        selectedUserId: $('#selectedUserId').val() || null
    };
    
    // 偵錯：記錄發送的資料
    console.log('Sending form data:', formData);
    
    $.ajax({
        url: '/Admin/SendSystemMessage',
        method: 'POST',
        data: formData,
        success: function(response) {
            hideSendingState();
            
            // 偵錯：記錄伺服器回應
            console.log('Server response:', response);
            
            if (response.success) {
                // 關閉確認 Modal
                $('#confirmSendModal').modal('hide');
                
                // 顯示成功訊息
                showSuccess(response.message);
                
                // 延遲後跳轉到列表頁面
                setTimeout(function() {
                    window.location.href = '/Admin/admin_systemMessageList';
                }, 2000);
            } else {
                console.error('Server returned error:', response);
                showError('發送失敗：' + response.message + (response.error ? '\n錯誤詳情：' + response.error : ''));
            }
        },
        error: function(xhr, status, error) {
            hideSendingState();
            console.error('AJAX error:', { xhr, status, error });
            console.error('Response text:', xhr.responseText);
            
            let errorMessage = '發送訊息時發生錯誤';
            if (xhr.responseText) {
                try {
                    const errorResponse = JSON.parse(xhr.responseText);
                    if (errorResponse.message) {
                        errorMessage += '：' + errorResponse.message;
                    }
                    if (errorResponse.error) {
                        errorMessage += '\n詳情：' + errorResponse.error;
                    }
                } catch (e) {
                    errorMessage += '：' + xhr.responseText;
                }
            }
            
            showError(errorMessage);
        }
    });
}

/**
 * 顯示發送中狀態
 */
function showSendingState() {
    $('#finalSendBtn').prop('disabled', true).html(`
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        發送中...
    `);
}

/**
 * 隱藏發送中狀態
 */
function hideSendingState() {
    $('#finalSendBtn').prop('disabled', false).html(`
        <i class="bi bi-send-fill"></i> 確認發送
    `);
}

/**
 * 表單驗證
 */
function validateForm() {
    let isValid = true;
    const errors = [];
    
    // 檢查標題
    const title = $('#messageTitle').val().trim();
    if (!title) {
        errors.push('請輸入訊息標題');
        isValid = false;
    } else if (title.length > 100) {
        errors.push('標題長度不能超過100個字元');
        isValid = false;
    }
    
    // 檢查內容
    const content = $('#messageContent').val().trim();
    if (!content) {
        errors.push('請輸入訊息內容');
        isValid = false;
    } else if (content.length > 2000) {
        errors.push('內容長度不能超過2000個字元');
        isValid = false;
    }
    
    // 檢查分類
    const category = $('#messageCategory').val();
    if (!category) {
        errors.push('請選擇訊息分類');
        isValid = false;
    }
    
    // 檢查個別用戶選擇
    const audienceType = $('input[name="audienceType"]:checked').val();
    if (audienceType === 'INDIVIDUAL') {
        const selectedUserId = $('#selectedUserId').val();
        if (!selectedUserId) {
            errors.push('請選擇個別用戶');
            isValid = false;
        }
    }
    
    
    // 顯示錯誤訊息
    if (!isValid) {
        showError('表單驗證失敗：\n' + errors.join('\n'));
    }
    
    return isValid;
}

/**
 * 初始化表單驗證
 */
function initializeFormValidation() {
    // 即時驗證標題長度
    $('#messageTitle').on('input', function() {
        const length = $(this).val().length;
        const maxLength = 100;
        
        if (length > maxLength) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    // 即時驗證內容長度
    $('#messageContent').on('input', function() {
        const length = $(this).val().length;
        const maxLength = 2000;
        
        if (length > maxLength) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
}

/**
 * 工具函數 - 防抖
 */
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

/**
 * 工具函數 - HTML轉義
 */
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

/**
 * 工具函數 - URL驗證
 */
function isValidUrl(string) {
    try {
        new URL(string);
        return true;
    } catch (_) {
        return false;
    }
}

/**
 * 工具函數 - 顯示成功訊息
 */
function showSuccess(message) {
    // 可以使用 Bootstrap Toast 或其他通知元件
    // 這裡使用簡單的 alert，實際使用時可替換為更美觀的通知
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'success',
            title: '成功',
            text: message,
            timer: 3000,
            showConfirmButton: false
        });
    } else {
        alert('成功：' + message);
    }
}

/**
 * 工具函數 - 顯示錯誤訊息
 */
function showError(message) {
    // 可以使用 Bootstrap Toast 或其他通知元件
    // 這裡使用簡單的 alert，實際使用時可替換為更美觀的通知
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
window.openTemplateModal = openTemplateModal;
window.selectTemplate = selectTemplate;
window.showConfirmSendModal = showConfirmSendModal;
window.sendMessage = sendMessage;