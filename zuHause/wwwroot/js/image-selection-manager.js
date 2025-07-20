/**
 * 圖片勾選管理器 - 處理圖片勾選狀態和批量操作邏輯
 * 實作需求 3, 4, 7
 */
class ImageSelectionManager {
    constructor(container) {
        this.container = container;
        this.selectedItems = new Set(); // 已選中的圖片 ID
        this.enableBatchOps = container.dataset.enableBatchOps === 'true';
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        this.updateUI();
        console.log('圖片勾選管理器初始化完成');
    }
    
    setupEventListeners() {
        // 監聽圖片網格內的勾選框變更 (需求 3)
        this.container.addEventListener('change', (e) => {
            if (e.target.classList.contains('image-checkbox') && e.target.type === 'checkbox') {
                this.handleItemSelection(e.target);
            }
        });
        
        // 全選按鈕 (可選功能)
        const selectAllBtn = this.container.querySelector('.btn-select-all');
        if (selectAllBtn) {
            selectAllBtn.addEventListener('click', this.selectAll.bind(this));
        }
        
        // 刪除選中項目按鈕 (需求 4, 7)
        const deleteSelectedBtn = this.container.querySelector('.btn-delete-selected');
        if (deleteSelectedBtn) {
            deleteSelectedBtn.addEventListener('click', this.deleteSelected.bind(this));
        }
        
        // 全部清除按鈕
        const clearAllBtn = this.container.querySelector('.btn-clear-all');
        if (clearAllBtn) {
            clearAllBtn.addEventListener('click', this.clearAll.bind(this));
        }
        
        // 監聽檔案變更事件，更新選中狀態
        this.container.addEventListener('filesChanged', this.handleFilesChanged.bind(this));
        
        // 鍵盤支援
        this.container.addEventListener('keydown', this.handleKeyboardNavigation.bind(this));
    }
    
    handleItemSelection(checkbox) {
        const imageId = checkbox.value;
        const imageItem = checkbox.closest('.image-item');
        
        if (checkbox.checked) {
            this.selectedItems.add(imageId);
            this.markItemAsSelected(imageItem);
        } else {
            this.selectedItems.delete(imageId);
            this.markItemAsUnselected(imageItem);
        }
        
        this.updateUI();
        this.notifySelectionChanged();
    }
    
    markItemAsSelected(imageItem) {
        if (imageItem) {
            imageItem.classList.add('selected');
            imageItem.setAttribute('aria-selected', 'true');
        }
    }
    
    markItemAsUnselected(imageItem) {
        if (imageItem) {
            imageItem.classList.remove('selected');
            imageItem.setAttribute('aria-selected', 'false');
        }
    }
    
    selectAll() {
        const checkboxes = this.container.querySelectorAll('.image-checkbox[type="checkbox"]');
        
        checkboxes.forEach(checkbox => {
            if (!checkbox.checked) {
                checkbox.checked = true;
                this.handleItemSelection(checkbox);
            }
        });
    }
    
    clearSelection() {
        const checkboxes = this.container.querySelectorAll('.image-checkbox[type="checkbox"]');
        
        checkboxes.forEach(checkbox => {
            if (checkbox.checked) {
                checkbox.checked = false;
                this.handleItemSelection(checkbox);
            }
        });
    }
    
    async deleteSelected() {
        if (this.selectedItems.size === 0) {
            return;
        }
        
        // 顯示確認對話框 (需求 4)
        const confirmed = await this.showDeleteConfirmation(this.selectedItems.size);
        if (!confirmed) {
            return;
        }
        
        try {
            // 收集要刪除的項目資訊
            const itemsToDelete = Array.from(this.selectedItems);
            
            // 派發刪除事件讓其他管理器處理
            const deleteEvent = new CustomEvent('deleteSelectedFiles', {
                detail: {
                    fileIds: itemsToDelete,
                    count: itemsToDelete.length
                }
            });
            this.container.dispatchEvent(deleteEvent);
            
            // 清空選中狀態
            this.selectedItems.clear();
            this.updateUI();
            
            // 顯示成功訊息
            this.showSuccessMessage(`已刪除 ${itemsToDelete.length} 張圖片`);
            
        } catch (error) {
            console.error('刪除選中圖片失敗:', error);
            this.showErrorMessage('刪除圖片時發生錯誤，請稍後再試');
        }
    }
    
    async clearAll() {
        // 先檢查是否有圖片
        const allCheckboxes = this.container.querySelectorAll('.image-checkbox[type="checkbox"]');
        if (allCheckboxes.length === 0) {
            return;
        }
        
        // 顯示確認對話框
        const confirmed = await this.showDeleteConfirmation(allCheckboxes.length, true);
        if (!confirmed) {
            return;
        }
        
        try {
            // 派發清空事件
            const clearEvent = new CustomEvent('clearAllFiles', {
                detail: {
                    count: allCheckboxes.length
                }
            });
            this.container.dispatchEvent(clearEvent);
            
            // 清空選中狀態
            this.selectedItems.clear();
            this.updateUI();
            
            // 顯示成功訊息
            this.showSuccessMessage('已清空所有圖片');
            
        } catch (error) {
            console.error('清空圖片失敗:', error);
            this.showErrorMessage('清空圖片時發生錯誤，請稍後再試');
        }
    }
    
    updateUI() {
        this.updateSelectionCount();
        this.updateButtonStates();
        this.updateAriaLabels();
    }
    
    updateSelectionCount() {
        // 更新已選中數量顯示
        const selectedCountSpans = this.container.querySelectorAll('.selected-count');
        selectedCountSpans.forEach(span => {
            span.textContent = this.selectedItems.size;
        });
    }
    
    updateButtonStates() {
        // 更新刪除選中按鈕狀態 (需求 7)
        const deleteSelectedBtn = this.container.querySelector('.btn-delete-selected');
        if (deleteSelectedBtn) {
            if (this.selectedItems.size > 0) {
                deleteSelectedBtn.classList.remove('d-none');
                deleteSelectedBtn.disabled = false;
            } else {
                deleteSelectedBtn.classList.add('d-none');
                deleteSelectedBtn.disabled = true;
            }
        }
        
        // 更新全選按鈕狀態
        const selectAllBtn = this.container.querySelector('.btn-select-all');
        if (selectAllBtn) {
            const totalCheckboxes = this.container.querySelectorAll('.image-checkbox[type=\"checkbox\"]').length;
            selectAllBtn.disabled = totalCheckboxes === 0;
            
            // 更新按鈕文字（全選/取消全選）
            if (this.selectedItems.size === totalCheckboxes && totalCheckboxes > 0) {
                selectAllBtn.innerHTML = '<i class=\"fas fa-square me-2\" aria-hidden=\"true\"></i>取消全選';
                selectAllBtn.onclick = this.clearSelection.bind(this);
            } else {
                selectAllBtn.innerHTML = '<i class=\"fas fa-check-square me-2\" aria-hidden=\"true\"></i>全選';
                selectAllBtn.onclick = this.selectAll.bind(this);
            }
        }
    }
    
    updateAriaLabels() {
        // 更新 ARIA 標籤以提升無障礙性
        const statusElement = this.container.querySelector('[aria-live=\"polite\"]');
        if (statusElement) {
            const totalImages = this.container.querySelectorAll('.image-item').length;
            const selectedCount = this.selectedItems.size;
            statusElement.setAttribute('aria-label', 
                `共 ${totalImages} 張圖片，已選中 ${selectedCount} 張`);
        }
    }
    
    async showDeleteConfirmation(count, isAll = false) {
        const action = isAll ? '清空所有' : '刪除選中的';
        const message = `確定要${action} ${count} 張圖片嗎？此操作無法復原。`;
        
        // 如果有 Modal 管理器，使用它來顯示確認對話框
        if (window.ImageModalManager) {
            return await window.ImageModalManager.showConfirmation(message);
        } else {
            // 暫時使用瀏覽器內建確認對話框
            return confirm(message);
        }
    }
    
    showSuccessMessage(message) {
        if (window.ImageModalManager) {
            window.ImageModalManager.showSuccess(message);
        } else {
            console.log('成功:', message);
        }
    }
    
    showErrorMessage(message) {
        if (window.ImageModalManager) {
            window.ImageModalManager.showError(message);
        } else {
            console.error('錯誤:', message);
            alert(message);
        }
    }
    
    handleFilesChanged(event) {
        // 當檔案清單變更時，清理不存在的選中項目
        const currentFileIds = new Set();
        
        // 收集當前存在的檔案 ID
        const checkboxes = this.container.querySelectorAll('.image-checkbox[type=\"checkbox\"]');
        checkboxes.forEach(checkbox => {
            currentFileIds.add(checkbox.value);
        });
        
        // 移除不存在的選中項目
        const itemsToRemove = [];
        this.selectedItems.forEach(itemId => {
            if (!currentFileIds.has(itemId)) {
                itemsToRemove.push(itemId);
            }
        });
        
        itemsToRemove.forEach(itemId => {
            this.selectedItems.delete(itemId);
        });
        
        this.updateUI();
    }
    
    handleKeyboardNavigation(event) {
        // 鍵盤導航支援
        if (event.target.classList.contains('image-item')) {
            switch (event.key) {
                case ' ':
                case 'Enter':
                    // 空格鍵或 Enter 鍵切換勾選狀態
                    event.preventDefault();
                    const checkbox = event.target.querySelector('.image-checkbox[type=\"checkbox\"]');
                    if (checkbox) {
                        checkbox.checked = !checkbox.checked;
                        this.handleItemSelection(checkbox);
                    }
                    break;
                    
                case 'a':
                case 'A':
                    // Ctrl+A 全選
                    if (event.ctrlKey || event.metaKey) {
                        event.preventDefault();
                        this.selectAll();
                    }
                    break;
                    
                case 'Delete':
                case 'Backspace':
                    // Delete 鍵刪除選中項目
                    if (this.selectedItems.size > 0) {
                        event.preventDefault();
                        this.deleteSelected();
                    }
                    break;
            }
        }
    }
    
    notifySelectionChanged() {
        // 派發選中狀態變更事件
        const event = new CustomEvent('selectionChanged', {
            detail: {
                selectedItems: this.selectedItems,
                count: this.selectedItems.size
            }
        });
        this.container.dispatchEvent(event);
    }
    
    // 公共 API
    getSelectedItems() {
        return new Set(this.selectedItems);
    }
    
    getSelectedCount() {
        return this.selectedItems.size;
    }
    
    selectItem(itemId) {
        const checkbox = this.container.querySelector(`input[value="${itemId}"]`);
        if (checkbox && !checkbox.checked) {
            checkbox.checked = true;
            this.handleItemSelection(checkbox);
        }
    }
    
    unselectItem(itemId) {
        const checkbox = this.container.querySelector(`input[value="${itemId}"]`);
        if (checkbox && checkbox.checked) {
            checkbox.checked = false;
            this.handleItemSelection(checkbox);
        }
    }
    
    hasSelection() {
        return this.selectedItems.size > 0;
    }
    
    clearAllSelections() {
        this.clearSelection();
    }
}

// 導出供其他模組使用
window.ImageSelectionManager = ImageSelectionManager;