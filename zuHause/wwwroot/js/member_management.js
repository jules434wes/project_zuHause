// Dashboard 會員管理模組 - 使用 IIFE 封裝避免衝突
(function() {
    'use strict';
    
    const PREFIX = 'dashboard-';
    
    // Dashboard 專用函數 - 帳戶狀態切換
    window[PREFIX + 'toggleAccountStatus'] = function(memberId, currentStatus) {
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
                    console.log('[Dashboard] 切換會員 ' + memberId + ' 帳戶狀態為: ' + newStatus);
                    // TODO: 實作後端API呼叫
                    alert('帳戶狀態已' + newStatus + ' (開發中...)');
                } else if (userInput !== null) {
                    alert('輸入的會員ID不正確，操作已取消');
                }
            }
        }, 300); // 延遲300ms避免誤點擊
    };

    // Dashboard 專用函數 - 開啟管理備註Modal
    window[PREFIX + 'openAdminNotesModal'] = function(memberId) {
        console.log('[Dashboard] 開啟會員 ' + memberId + ' 管理備註');
        // TODO: 實作管理備註Modal
        alert('管理備註功能開發中...');
    };

    // Dashboard 專用函數 - 重置驗證狀態
    window[PREFIX + 'resetVerificationStatus'] = function(memberId) {
        if (confirm('確定要重置會員 ' + memberId + ' 的驗證狀態嗎？此操作會清除現有驗證記錄。')) {
            console.log('[Dashboard] 重置會員 ' + memberId + ' 驗證狀態');
            // TODO: 實作後端API呼叫
            alert('驗證狀態已重置 (開發中...)');
        }
    };

    // Dashboard 專用函數 - 查看用戶操作記錄
    window[PREFIX + 'viewUserActivityLog'] = function(memberId) {
        console.log('[Dashboard] 查看會員 ' + memberId + ' 操作記錄');
        // TODO: 實作用戶操作記錄Modal
        alert('操作記錄功能開發中...');
    };

    // Dashboard 專用函數 - 載入證件文件
    window[PREFIX + 'loadDocuments'] = function(memberId) {
        console.log('[Dashboard] 載入會員 ' + memberId + ' 證件文件');
        // TODO: 實作證件查看功能
        var modal = document.getElementById(PREFIX + 'viewDocumentModal');
        if (modal) {
            var modalTitle = modal.querySelector('h5');
            if (modalTitle) modalTitle.textContent = '查看會員 ' + memberId + ' 證件';
        }
        alert('證件查看功能開發中...');
    };

    // Dashboard 專用函數 - Tab 切換處理
    function handleTabSwitches() {
        const tabButtons = document.querySelectorAll('#' + PREFIX + 'userTabs button[data-bs-toggle="tab"]');
        tabButtons.forEach(button => {
            button.addEventListener('click', function() {
                console.log('[Dashboard] 切換到會員管理分頁:', this.textContent.trim());
            });
        });
    }

    // Dashboard 專用函數 - 進階搜尋切換
    function handleAdvancedSearch() {
        const advancedBtns = document.querySelectorAll('[id^="' + PREFIX + 'advancedSearchBtn"]');
        advancedBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const tabId = this.id.replace(PREFIX + 'advancedSearchBtn', '');
                const collapseEl = document.getElementById(PREFIX + 'advancedSearchCollapse' + tabId);
                if (collapseEl) {
                    const bsCollapse = new bootstrap.Collapse(collapseEl, { toggle: true });
                    const icon = this.querySelector('i');
                    if (icon) {
                        icon.classList.toggle('fa-chevron-down');
                        icon.classList.toggle('fa-chevron-up');
                    }
                }
            });
        });
    }

    // 載入狀態管理
    function showLoadingState(button, text = '載入中...') {
        if (!button) return;
        
        button.disabled = true;
        const originalText = button.textContent;
        button.setAttribute('data-original-text', originalText);
        button.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status"></span>${text}`;
        
        return originalText;
    }
    
    function hideLoadingState(button, originalText = null) {
        if (!button) return;
        
        button.disabled = false;
        const text = originalText || button.getAttribute('data-original-text');
        if (text) {
            button.textContent = text;
            button.removeAttribute('data-original-text');
        }
    }
    
    // Toast 通知函數
    function showToast(message, type = 'info', duration = 3000) {
        // 創建 Toast 容器（如果不存在）
        let toastContainer = document.getElementById(PREFIX + 'toastContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = PREFIX + 'toastContainer';
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }
        
        // 創建 Toast 元素
        const toastId = PREFIX + 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'primary'}" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas fa-${type === 'error' ? 'exclamation-circle' : type === 'success' ? 'check-circle' : 'info-circle'} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;
        
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        
        // 顯示 Toast
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: duration });
        toast.show();
        
        // 自動清理
        setTimeout(() => {
            if (toastElement && toastElement.parentNode) {
                toastElement.remove();
            }
        }, duration + 500);
    }

    // 收集篩選條件函數
    function collectFilterConditions(tabId) {
        // 處理 isLandlord 欄位值轉換
        const isLandlordValue = document.getElementById(PREFIX + 'isLandlord' + tabId)?.value || '';
        let isLandlord = null;
        if (isLandlordValue === 'true') isLandlord = true;
        else if (isLandlordValue === 'false') isLandlord = false;
        
        // 處理城市ID轉換
        const residenceCityIdStr = document.getElementById(PREFIX + 'residenceCityID' + tabId)?.value || '';
        const primaryRentalCityIdStr = document.getElementById(PREFIX + 'primaryRentalCityID' + tabId)?.value || '';
        
        return {
            keyword: document.getElementById(PREFIX + 'searchInput' + tabId)?.value?.trim() || '',
            searchField: document.getElementById(PREFIX + 'searchField' + tabId)?.value || '',
            verificationStatus: document.getElementById(PREFIX + 'verificationStatus' + tabId)?.value || '',
            accountStatus: document.getElementById(PREFIX + 'accountStatus' + tabId)?.value || '',
            gender: document.getElementById(PREFIX + 'gender' + tabId)?.value || '',
            isLandlord: isLandlord,
            residenceCityId: residenceCityIdStr ? parseInt(residenceCityIdStr) : null,
            primaryRentalCityId: primaryRentalCityIdStr ? parseInt(primaryRentalCityIdStr) : null,
            registerDateStart: document.getElementById(PREFIX + 'registerDateStart' + tabId)?.value || null,
            registerDateEnd: document.getElementById(PREFIX + 'registerDateEnd' + tabId)?.value || null,
            lastLoginDateStart: document.getElementById(PREFIX + 'lastLoginDateStart' + tabId)?.value || null,
            lastLoginDateEnd: document.getElementById(PREFIX + 'lastLoginDateEnd' + tabId)?.value || null,
            applyDateStart: document.getElementById(PREFIX + 'applyDateStart' + tabId)?.value || null,
            applyDateEnd: document.getElementById(PREFIX + 'applyDateEnd' + tabId)?.value || null
        };
    }

    // 檢查是否有任何篩選條件
    function hasAnyFilterConditions(conditions) {
        return !!(
            conditions.keyword ||
            conditions.verificationStatus ||
            conditions.accountStatus ||
            conditions.gender ||
            conditions.isLandlord !== null ||
            conditions.residenceCityId ||
            conditions.primaryRentalCityId ||
            conditions.registerDateStart ||
            conditions.registerDateEnd ||
            conditions.lastLoginDateStart ||
            conditions.lastLoginDateEnd ||
            conditions.applyDateStart ||
            conditions.applyDateEnd
        );
    }

    // Dashboard 專用函數 - 搜尋功能
    function handleSearch() {
        // 執行搜尋的共用函數
        function performSearch(tabId) {
            const searchInput = document.getElementById(PREFIX + 'searchInput' + tabId);
            const searchField = document.getElementById(PREFIX + 'searchField' + tabId);
            const searchBtn = document.getElementById(PREFIX + 'searchBtn' + tabId);
            
            if (searchInput && searchField) {
                const keyword = searchInput.value.trim();
                const field = searchField.value;
                
                // 收集所有篩選條件
                const filterConditions = collectFilterConditions(tabId);
                
                // 檢查是否有任何篩選條件
                if (!hasAnyFilterConditions(filterConditions)) {
                    showToast('請輸入搜尋關鍵字或選擇篩選條件', 'error');
                    searchInput.focus();
                    return;
                }
                
                console.log('[Dashboard] 搜尋請求:', {
                    tabId: tabId,
                    filterConditions: filterConditions
                });
                
                // 顯示載入狀態
                showLoadingState(searchBtn, '搜尋中...');
                
                // 調用新的高級搜尋 API
                fetch('/Dashboard/AdvancedSearchUsers', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify(filterConditions)
                })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('[Dashboard] 搜尋結果:', data);
                    hideLoadingState(searchBtn);
                    
                    if (data && data.length !== undefined) {
                        showToast(`找到 ${data.length} 筆符合條件的會員`, 'success');
                        
                        // 實作搜尋結果顯示
                        updateSearchResultsDisplay(data, tabId);
                    } else {
                        showToast('搜尋完成，結果格式異常', 'error');
                    }
                })
                .catch(error => {
                    console.error('[Dashboard] 搜尋失敗:', {
                        error: error,
                        message: error.message,
                        stack: error.stack,
                        searchParams: { keyword, field, tabId }
                    });
                    hideLoadingState(searchBtn);
                    
                    let errorMessage = '搜尋失敗，請稍後再試';
                    if (error.message.includes('HTTP 404')) {
                        errorMessage = '搜尋功能暫時無法使用 (404)';
                        console.error('[Dashboard] API 端點不存在，請檢查路由配置');
                    } else if (error.message.includes('HTTP 500')) {
                        errorMessage = '伺服器錯誤，請聯繫系統管理員 (500)';
                        console.error('[Dashboard] 伺服器內部錯誤，請檢查後端日誌');
                    } else if (error.name === 'TypeError') {
                        errorMessage = '網路連線問題，請檢查網路狀態';
                        console.error('[Dashboard] 網路或 CORS 問題');
                    } else if (error.message.includes('Unexpected token')) {
                        errorMessage = '伺服器回應格式錯誤';
                        console.error('[Dashboard] JSON 解析失敗，伺服器可能回傳了非 JSON 格式');
                    }
                    
                    showToast(errorMessage, 'error', 5000);
                });
            }
        }
        
        // 綁定搜尋按鈕點擊事件
        const searchBtns = document.querySelectorAll('[id^="' + PREFIX + 'searchBtn"]');
        searchBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const tabId = this.id.replace(PREFIX + 'searchBtn', '');
                performSearch(tabId);
            });
        });
        
        // 綁定搜尋輸入框 Enter 鍵事件
        const searchInputs = document.querySelectorAll('[id^="' + PREFIX + 'searchInput"]');
        searchInputs.forEach(input => {
            input.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const tabId = this.id.replace(PREFIX + 'searchInput', '');
                    performSearch(tabId);
                }
            });
        });
    }

    // Dashboard 專用函數 - 重置功能
    function handleReset() {
        const resetBtns = document.querySelectorAll('[id^="' + PREFIX + 'resetBtn"]');
        resetBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const tabId = this.id.replace(PREFIX + 'resetBtn', '');
                performReset(tabId);
            });
        });
    }

    // 執行完整重置
    function performReset(tabId) {
        // 1. 重置所有表單元素
        const inputs = document.querySelectorAll(`[id^="${PREFIX}"][id*="${tabId}"]`);
        inputs.forEach(input => {
            if (input.type === 'text' || input.type === 'date') {
                input.value = '';
            } else if (input.tagName === 'SELECT') {
                input.selectedIndex = 0;
            } else if (input.type === 'checkbox') {
                input.checked = false;
            }
        });

        // 2. 清除搜尋結果並恢復原始表格內容
        window[PREFIX + 'clearSearchResults'](tabId);

        // 3. 顯示重置完成提示
        showToast('已重置所有篩選條件', 'info');
        console.log('[Dashboard] 已完整重置搜尋條件和結果');
    }


    // Dashboard 專用函數 - 全選功能
    function handleSelectAll() {
        const selectAllBtns = document.querySelectorAll('[id^="' + PREFIX + 'selectAllUsers"]');
        selectAllBtns.forEach(btn => {
            btn.addEventListener('change', function() {
                const tableType = this.id.replace(PREFIX + 'selectAllUsers', '').toLowerCase();
                const checkboxes = document.querySelectorAll('.' + PREFIX + 'user-checkbox-' + (tableType || 'all'));
                checkboxes.forEach(checkbox => {
                    checkbox.checked = this.checked;
                });
                
                // 更新批量操作按鈕狀態
                updateBulkActionButtons(tableType);
            });
        });
    }

    // 更新批量操作按鈕狀態
    function updateBulkActionButtons(tableType) {
        const checkboxes = document.querySelectorAll('.' + PREFIX + 'user-checkbox-' + (tableType || 'all') + ':checked');
        const bulkBtn = document.getElementById(PREFIX + 'bulkMessageBtn' + (tableType ? tableType.charAt(0).toUpperCase() + tableType.slice(1) : ''));
        
        if (bulkBtn) {
            bulkBtn.disabled = checkboxes.length === 0;
        }
    }

    // 搜尋結果顯示函數
    function updateSearchResultsDisplay(searchResults, tabId) {
        console.log('[Dashboard] 更新搜尋結果顯示:', searchResults);
        
        // 根據 tabId 決定要更新的表格
        let targetTabId = '';
        if (tabId === 'Pending') {
            targetTabId = 'dashboard-pending-verification';
        } else if (tabId === 'Landlord') {
            targetTabId = 'dashboard-pending-landlord';
        } else {
            targetTabId = 'dashboard-all-users';
        }
        
        // 找到目標表格的 tbody
        const targetTable = document.querySelector(`#${targetTabId} table tbody`);
        if (!targetTable) {
            console.error('[Dashboard] 找不到目標表格:', targetTabId);
            return;
        }
        
        // 備份原始表格內容
        if (!targetTable.getAttribute('data-original-content')) {
            targetTable.setAttribute('data-original-content', targetTable.innerHTML);
        }
        
        if (searchResults.length === 0) {
            // 沒有搜尋結果時顯示提示
            targetTable.innerHTML = `
                <tr>
                    <td colspan="100%" class="text-center py-4">
                        <i class="fas fa-search fa-2x text-muted mb-2"></i>
                        <div class="text-muted">沒有找到符合條件的會員</div>
                        <button class="btn btn-sm btn-outline-primary mt-2" onclick="dashboard-clearSearchResults('${tabId}')">
                            <i class="fas fa-undo"></i> 清除篩選
                        </button>
                    </td>
                </tr>
            `;
        } else {
            // 顯示搜尋結果
            let resultsHtml = '';
            searchResults.forEach((member, index) => {
                resultsHtml += `
                    <tr class="search-result-row">
                        <td class="text-center">
                            <input type="checkbox" class="${PREFIX}user-checkbox" value="${member.id}">
                        </td>
                        <td>${member.memberId}</td>
                        <td>${member.name}</td>
                        <td>${member.email}</td>
                        <td>${member.phone}</td>
                        <td>${member.identityNumber}</td>
                        <td>
                            <span class="badge bg-success">啟用</span>
                        </td>
                        <td>
                            <div class="btn-group btn-group-sm">
                                <button class="btn btn-outline-primary" onclick="${PREFIX}viewUserActivityLog('${member.id}')" title="查看詳情">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn btn-outline-warning" onclick="${PREFIX}toggleAccountStatus('${member.id}', 'active')" title="帳戶管理">
                                    <i class="fas fa-user-cog"></i>
                                </button>
                                <button class="btn btn-outline-info" onclick="${PREFIX}openAdminNotesModal('${member.id}')" title="管理備註">
                                    <i class="fas fa-sticky-note"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;
            });
            
            // 加上清除篩選按鈕
            resultsHtml += `
                <tr class="search-controls-row">
                    <td colspan="100%" class="text-center py-2 bg-light">
                        <small class="text-muted">搜尋結果 (${searchResults.length} 筆)</small>
                        <button class="btn btn-sm btn-outline-secondary ms-3" onclick="dashboard-clearSearchResults('${tabId}')">
                            <i class="fas fa-undo"></i> 顯示全部會員
                        </button>
                    </td>
                </tr>
            `;
            
            targetTable.innerHTML = resultsHtml;
        }
    }
    
    // 清除搜尋結果函數 (供全域調用)
    window[PREFIX + 'clearSearchResults'] = function(tabId) {
        console.log('[Dashboard] 清除搜尋結果:', tabId);
        
        // 清空所有輸入欄位（但不影響其他篩選條件，除非是重置按鈕調用）
        const searchInput = document.getElementById(PREFIX + 'searchInput' + tabId);
        if (searchInput) {
            searchInput.value = '';
        }
        
        // 根據 tabId 決定要恢復的表格
        let targetTabId = '';
        if (tabId === 'Pending') {
            targetTabId = 'dashboard-pending-verification';
        } else if (tabId === 'Landlord') {
            targetTabId = 'dashboard-pending-landlord';
        } else {
            targetTabId = 'dashboard-all-users';
        }
        
        // 恢復原始表格內容
        const targetTable = document.querySelector(`#${targetTabId} table tbody`);
        if (targetTable) {
            const originalContent = targetTable.getAttribute('data-original-content');
            if (originalContent) {
                targetTable.innerHTML = originalContent;
                // 移除搜尋結果標記
                targetTable.removeAttribute('data-search-active');
            }
        }
        
        showToast('已清除搜尋篩選', 'info');
    };

    // 主要初始化函數
    window.initMemberManagement = function() {
        console.log('[Dashboard] 初始化會員管理模組');
        
        // 延遲執行以確保DOM完全載入
        setTimeout(() => {
            handleTabSwitches();
            handleAdvancedSearch();
            handleSearch();
            handleReset();
            handleSelectAll();
            
            console.log('[Dashboard] 會員管理模組初始化完成');
        }, 100);
    };

})();