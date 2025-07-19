// 新增系統訊息頁面 JavaScript
// 全域變數
let selectedUser = null;
let availableUsers = [];

// 頁面載入後初始化
document.addEventListener('DOMContentLoaded', function() {
    // 嘗試從 ViewData 獲取使用者資料
    try {
        if (typeof window.availableUsersData !== 'undefined') {
            availableUsers = window.availableUsersData;
        }
    } catch (e) {
        console.warn('無法載入使用者資料');
        availableUsers = [];
    }
    
    initializeAudienceSelector();
    initializeUserSearch();
    initializeConfirmCheckbox();
    initializeTemplateModal();
});

// 發送對象選擇器邏輯
function initializeAudienceSelector() {
    const audienceRadios = document.querySelectorAll('input[name="audienceType"]');
    const individualUserSelector = document.getElementById('individualUserSelector');
    const userSearchInput = document.getElementById('userSearch');

    audienceRadios.forEach(radio => {
        radio.addEventListener('change', function() {
            if (this.value === 'individual') {
                individualUserSelector.style.display = 'block';
                userSearchInput.required = true;
            } else {
                individualUserSelector.style.display = 'none';
                userSearchInput.required = false;
                userSearchInput.value = '';
                selectedUser = null;
                hideUserSearchResults();
            }
        });
    });
}

// 用戶搜尋功能
function initializeUserSearch() {
    const userSearchInput = document.getElementById('userSearch');
    const userSearchField = document.getElementById('userSearchField');
    const userSearchResults = document.getElementById('userSearchResults');

    if (!userSearchInput || !userSearchField || !userSearchResults) {
        return;
    }

    userSearchInput.addEventListener('input', function() {
        const query = this.value.toLowerCase().trim();
        const searchField = userSearchField.value;
        
        if (query.length < 2) {
            hideUserSearchResults();
            return;
        }

        const filteredUsers = availableUsers.filter(user => {
            switch(searchField) {
                case 'memberName':
                    return user.memberName && user.memberName.toLowerCase().includes(query);
                case 'memberID':
                    return user.memberId && user.memberId.toLowerCase().includes(query);
                case 'email':
                    return user.email && user.email.toLowerCase().includes(query);
                case 'nationalNo':
                    return user.nationalNo && user.nationalNo.toLowerCase().includes(query);
                default:
                    return false;
            }
        });

        showUserSearchResults(filteredUsers);
    });

    // 搜尋欄位變更時重新搜尋
    userSearchField.addEventListener('change', function() {
        const query = userSearchInput.value.toLowerCase().trim();
        if (query.length >= 2) {
            userSearchInput.dispatchEvent(new Event('input'));
        }
    });

    // 點擊外部關閉搜尋結果
    document.addEventListener('click', function(e) {
        if (!userSearchInput.contains(e.target) && !userSearchResults.contains(e.target) && !userSearchField.contains(e.target)) {
            hideUserSearchResults();
        }
    });
}

// 顯示用戶搜尋結果
function showUserSearchResults(users) {
    const userSearchResults = document.getElementById('userSearchResults');
    
    if (!userSearchResults) return;
    
    if (users.length === 0) {
        userSearchResults.innerHTML = '<div class="dropdown-item text-muted">找不到符合條件的用戶</div>';
    } else {
        userSearchResults.innerHTML = users.map(user => `
            <div class="dropdown-item user-result-item" onclick="selectUser('${user.memberId || ''}', '${user.memberName || ''}', '${user.email || ''}', '${user.nationalNo || ''}')">
                <div><strong>${user.memberName || ''}</strong> (${user.memberId || ''})</div>
                <div class="small text-muted">${user.email || ''}</div>
                ${user.nationalNo ? `<div class="small text-muted">身分證：${user.nationalNo}</div>` : ''}
            </div>
        `).join('');
    }
    
    userSearchResults.style.display = 'block';
}

// 隱藏用戶搜尋結果
function hideUserSearchResults() {
    const userSearchResults = document.getElementById('userSearchResults');
    if (userSearchResults) {
        userSearchResults.style.display = 'none';
    }
}

// 選擇用戶
function selectUser(memberId, memberName, email, nationalNo = '') {
    selectedUser = { memberId, memberName, email, nationalNo };
    const userSearchInput = document.getElementById('userSearch');
    const selectedUserIdInput = document.getElementById('selectedUserId');
    
    if (userSearchInput) {
        userSearchInput.value = `${memberName} (${memberId})`;
    }
    if (selectedUserIdInput) {
        selectedUserIdInput.value = memberId;
    }
    hideUserSearchResults();
}

// 確認checkbox邏輯
function initializeConfirmCheckbox() {
    const confirmCheckbox = document.getElementById('confirmCheckbox');
    const finalSendBtn = document.getElementById('finalSendBtn');

    if (confirmCheckbox && finalSendBtn) {
        confirmCheckbox.addEventListener('change', function() {
            finalSendBtn.disabled = !this.checked;
        });
    }
}

// 顯示確認發送Modal
function showConfirmSendModal() {
    // 表單驗證
    if (!validateForm()) {
        return;
    }

    const selectedAudienceRadio = document.querySelector('input[name="audienceType"]:checked');
    const audienceLabel = document.querySelector(`label[for="${selectedAudienceRadio.id}"]`);
    const confirmAudienceText = document.getElementById('confirmAudienceText');
    const confirmUserDetails = document.getElementById('confirmUserDetails');
    
    if (confirmAudienceText && audienceLabel) {
        confirmAudienceText.textContent = audienceLabel.textContent;
    }

    // 如果是個別用戶，顯示用戶詳細資訊
    if (selectedAudienceRadio && selectedAudienceRadio.value === 'individual' && selectedUser) {
        const confirmUserName = document.getElementById('confirmUserName');
        const confirmUserId = document.getElementById('confirmUserId');
        
        if (confirmUserName) confirmUserName.textContent = selectedUser.memberName;
        if (confirmUserId) confirmUserId.textContent = selectedUser.memberId;
        if (confirmUserDetails) confirmUserDetails.style.display = 'block';
    } else {
        if (confirmUserDetails) confirmUserDetails.style.display = 'none';
    }

    // 重置確認checkbox
    const confirmCheckbox = document.getElementById('confirmCheckbox');
    const finalSendBtn = document.getElementById('finalSendBtn');
    
    if (confirmCheckbox) confirmCheckbox.checked = false;
    if (finalSendBtn) finalSendBtn.disabled = true;

    const modal = new bootstrap.Modal(document.getElementById('confirmSendModal'));
    modal.show();
}

// 表單驗證
function validateForm() {
    const messageTitle = document.getElementById('messageTitle');
    const messageContent = document.getElementById('messageContent');
    const messageCategory = document.getElementById('messageCategory');
    const selectedAudience = document.querySelector('input[name="audienceType"]:checked');

    if (!messageTitle || !messageTitle.value.trim()) {
        alert('請輸入訊息標題');
        return false;
    }

    if (!messageContent || !messageContent.value.trim()) {
        alert('請輸入訊息內容');
        return false;
    }

    if (!messageCategory || !messageCategory.value) {
        alert('請選擇訊息分類');
        return false;
    }

    if (selectedAudience && selectedAudience.value === 'individual' && !selectedUser) {
        alert('請選擇個別用戶');
        return false;
    }

    return true;
}

// 發送訊息
function sendMessage() {
    // 收集表單資料
    const selectedAudience = document.querySelector('input[name="audienceType"]:checked');
    const messageCategory = document.getElementById('messageCategory');
    const messageTitle = document.getElementById('messageTitle');
    const messageContent = document.getElementById('messageContent');
    
    const formData = {
        audienceType: selectedAudience ? selectedAudience.value : '',
        receiverId: selectedUser ? selectedUser.memberId : null,
        messageCategory: messageCategory ? messageCategory.value : '',
        messageTitle: messageTitle ? messageTitle.value : '',
        messageContent: messageContent ? messageContent.value : ''
    };

    // 這裡將來會調用後端API
    console.log('發送訊息資料:', formData);
    
    // 模擬發送成功
    alert('訊息發送成功！');
    
    // 關閉Modal並導向列表頁
    const modal = bootstrap.Modal.getInstance(document.getElementById('confirmSendModal'));
    if (modal) modal.hide();
    window.location.href = '/Admin/admin_systemMessageList';
}

// 模板Modal相關功能
function initializeTemplateModal() {
    const templateCategoryFilter = document.getElementById('templateCategoryFilter');
    
    if (templateCategoryFilter) {
        templateCategoryFilter.addEventListener('change', function() {
            filterTemplates(this.value);
        });
    }
}

// 開啟模板Modal
function openTemplateModal() {
    const modal = new bootstrap.Modal(document.getElementById('templateModal'));
    modal.show();
}

// 篩選模板
function filterTemplates(category) {
    const templateItems = document.querySelectorAll('.template-item');
    
    templateItems.forEach(item => {
        const itemCategory = item.getAttribute('data-template-category');
        
        if (category === 'all' || itemCategory === category) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// 選擇模板
function selectTemplate(templateElement) {
    const content = templateElement.getAttribute('data-template-content');
    const messageContent = document.getElementById('messageContent');
    
    if (messageContent) {
        messageContent.value = content;
    }
    
    // 關閉Modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('templateModal'));
    if (modal) modal.hide();
}

// 設定可用使用者資料的函數（供從外部呼叫）
function setAvailableUsers(users) {
    availableUsers = users;
}