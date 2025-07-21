/**
 * 圖片 Modal 管理器 - 處理確認對話框、載入狀態和提示訊息
 * 實作需求 11, 12, 13
 */
class ImageModalManager {
    constructor(container) {
        this.container = container;
        this.loadingOverlay = container.querySelector('.loading-overlay');
        this.toastContainer = document.querySelector('.toast-container') || this.createToastContainer();
        
        // Modal 模板
        this.confirmTemplate = container.querySelector('#confirm-dialog-template');
        this.successTemplate = container.querySelector('#success-toast-template');
        this.errorTemplate = container.querySelector('#error-toast-template');
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        console.log('圖片 Modal 管理器初始化完成');
    }
    
    setupEventListeners() {
        // 監聽按鈕點擊事件
        this.container.addEventListener('click', this.handleButtonClick.bind(this));
        
        // 監聽檔案上傳事件
        this.container.addEventListener('uploadStarted', this.showLoading.bind(this));
        this.container.addEventListener('uploadCompleted', this.hideLoading.bind(this));
        this.container.addEventListener('uploadFailed', this.handleUploadError.bind(this));
        
        // 監聽檔案操作事件
        this.container.addEventListener('filesChanged', this.handleFilesChanged.bind(this));
        this.container.addEventListener('itemReordered', this.handleItemReordered.bind(this));
    }
    
    handleButtonClick(e) {
        const button = e.target.closest('button');
        if (!button) return;
        
        // 確認上傳按鈕 (需求 12, 13)
        if (button.classList.contains('btn-upload-confirm')) {
            this.handleUploadConfirm(button);
        }
        
        // 刪除選中按鈕 (需求 11)
        else if (button.classList.contains('btn-delete-selected')) {
            this.handleDeleteSelected(button);
        }
        
        // 全部清除按鈕 (需求 11)
        else if (button.classList.contains('btn-clear-all')) {
            this.handleClearAll(button);
        }
        
        // 單個刪除按鈕 (需求 11)
        else if (button.classList.contains('btn-delete-single')) {
            this.handleDeleteSingle(button);
        }
    }
    
    async handleUploadConfirm(button) {
        const fileCount = this.getFileCount();
        
        if (fileCount === 0) {
            this.showError('請先選擇要上傳的圖片');
            return;
        }
        
        // 顯示確認對話框 (需求 11)
        const confirmed = await this.showConfirmation(
            `確定要上傳 ${fileCount} 張圖片嗎？`,
            '上傳確認',
            '確定上傳',
            'btn-success'
        );
        
        if (!confirmed) return;
        
        try {
            // 顯示載入狀態 (需求 12)
            this.showLoadingWithText('正在上傳圖片，請稍候...');
            
            // 派發上傳事件
            const uploadEvent = new CustomEvent('startUpload', {
                detail: { confirmed: true }
            });
            this.container.dispatchEvent(uploadEvent);
            
        } catch (error) {
            this.hideLoading();
            this.showError('上傳失敗：' + error.message);
        }
    }
    
    async handleDeleteSelected(button) {
        const selectedCount = this.getSelectedCount();
        
        if (selectedCount === 0) {
            this.showError('請先選擇要刪除的圖片');
            return;
        }
        
        // 顯示確認對話框 (需求 11)
        const confirmed = await this.showConfirmation(
            `確定要刪除選中的 ${selectedCount} 張圖片嗎？此操作無法復原。`,
            '刪除確認',
            '確定刪除',
            'btn-danger'
        );
        
        if (confirmed) {
            const deleteEvent = new CustomEvent('confirmDeleteSelected', {
                detail: { confirmed: true }
            });
            this.container.dispatchEvent(deleteEvent);
        }
    }
    
    async handleClearAll(button) {
        const totalCount = this.getTotalCount();
        
        if (totalCount === 0) {
            this.showError('沒有圖片可以清除');
            return;
        }
        
        // 顯示確認對話框 (需求 11)
        const confirmed = await this.showConfirmation(
            `確定要清除所有 ${totalCount} 張圖片嗎？此操作無法復原。`,
            '清除確認',
            '確定清除',
            'btn-danger'
        );
        
        if (confirmed) {
            const clearEvent = new CustomEvent('confirmClearAll', {
                detail: { confirmed: true }
            });
            this.container.dispatchEvent(clearEvent);
        }
    }
    
    async handleDeleteSingle(button) {
        const imageId = button.dataset.imageId;
        const imageItem = button.closest('.image-item');
        const fileName = imageItem ? 
            imageItem.querySelector('.image-filename')?.textContent || '此圖片' : 
            '此圖片';
        
        // 顯示確認對話框 (需求 11)
        const confirmed = await this.showConfirmation(
            `確定要刪除圖片「${fileName}」嗎？此操作無法復原。`,
            '刪除確認',
            '確定刪除',
            'btn-danger'
        );
        
        if (confirmed) {
            const deleteEvent = new CustomEvent('confirmDeleteSingle', {
                detail: { 
                    imageId: imageId,
                    confirmed: true 
                }
            });
            this.container.dispatchEvent(deleteEvent);
        }
    }
    
    // Modal 對話框相關方法
    async showConfirmation(message, title = '確認操作', confirmText = '確定', confirmClass = 'btn-primary') {
        return new Promise((resolve) => {
            const modal = this.createConfirmModal(message, title, confirmText, confirmClass);
            
            const confirmBtn = modal.querySelector('.btn-confirm-action');
            const cancelBtn = modal.querySelector('[data-bs-dismiss="modal"]');
            
            // 確定按鈕事件
            confirmBtn.addEventListener('click', () => {
                this.destroyModal(modal);
                resolve(true);
            });
            
            // 取消按鈕事件
            const handleCancel = () => {
                this.destroyModal(modal);
                resolve(false);
            };
            
            cancelBtn.addEventListener('click', handleCancel);
            
            // ESC 鍵關閉
            modal.addEventListener('keydown', (e) => {
                if (e.key === 'Escape') {
                    handleCancel();
                }
            });
            
            // 顯示 Modal
            this.showModal(modal);
        });
    }
    
    createConfirmModal(message, title, confirmText, confirmClass) {
        const template = this.confirmTemplate.content.cloneNode(true);
        const modal = template.querySelector('.modal');
        
        // 設定內容
        const titleElement = modal.querySelector('.modal-title');
        const messageElement = modal.querySelector('.confirm-message');
        const confirmBtn = modal.querySelector('.btn-confirm-action');
        
        if (titleElement) titleElement.textContent = title;
        if (messageElement) messageElement.textContent = message;
        if (confirmBtn) {
            confirmBtn.textContent = confirmText;
            confirmBtn.className = `btn ${confirmClass} btn-confirm-action`;
        }
        
        // 添加到文檔
        document.body.appendChild(modal);
        
        return modal;
    }
    
    showModal(modal) {
        // 使用 Bootstrap Modal 或自定義顯示邏輯
        if (window.bootstrap && window.bootstrap.Modal) {
            const bsModal = new window.bootstrap.Modal(modal);
            bsModal.show();
            
            // 儲存 Bootstrap Modal 實例以便後續銷毀
            modal._bsModal = bsModal;
        } else {
            // 簡單的顯示邏輯
            modal.style.display = 'block';
            modal.classList.add('show');
            document.body.classList.add('modal-open');
            
            // 添加背景覆蓋
            const backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop fade show';
            document.body.appendChild(backdrop);
            modal._backdrop = backdrop;
        }
        
        // 聚焦到 Modal
        setTimeout(() => {
            const firstButton = modal.querySelector('button');
            if (firstButton) {
                firstButton.focus();
            }
        }, 100);
    }
    
    destroyModal(modal) {
        if (modal._bsModal) {
            modal._bsModal.dispose();
        } else {
            modal.style.display = 'none';
            modal.classList.remove('show');
            document.body.classList.remove('modal-open');
            
            if (modal._backdrop) {
                modal._backdrop.remove();
            }
        }
        
        modal.remove();
    }
    
    // 載入狀態相關方法 (需求 12)
    showLoading(customText = null) {
        if (this.loadingOverlay) {
            const loadingText = this.loadingOverlay.querySelector('.loading-text');
            if (loadingText && customText) {
                loadingText.textContent = customText;
            }
            
            this.loadingOverlay.classList.remove('d-none');
            this.loadingOverlay.setAttribute('aria-hidden', 'false');
            
            // 無障礙支援
            this.loadingOverlay.setAttribute('aria-live', 'polite');
            this.loadingOverlay.setAttribute('aria-label', customText || '載入中，請稍候');
        }
    }
    
    showLoadingWithText(text) {
        this.showLoading(text);
    }
    
    hideLoading() {
        if (this.loadingOverlay) {
            this.loadingOverlay.classList.add('d-none');
            this.loadingOverlay.setAttribute('aria-hidden', 'true');
        }
    }
    
    updateLoadingText(text) {
        if (this.loadingOverlay) {
            const loadingText = this.loadingOverlay.querySelector('.loading-text');
            if (loadingText) {
                loadingText.textContent = text;
            }
        }
    }
    
    // Toast 提示相關方法 (需求 13)
    showSuccess(message) {
        this.showToast(message, 'success');
    }
    
    showError(message) {
        this.showToast(message, 'error');
    }
    
    showInfo(message) {
        this.showToast(message, 'info');
    }
    
    showToast(message, type = 'success', duration = 4000) {
        const toast = this.createToast(message, type);
        this.toastContainer.appendChild(toast);
        
        // 顯示 Toast
        setTimeout(() => {
            if (window.bootstrap && window.bootstrap.Toast) {
                const bsToast = new window.bootstrap.Toast(toast, {
                    delay: duration
                });
                bsToast.show();
                
                // 自動移除
                toast.addEventListener('hidden.bs.toast', () => {
                    toast.remove();
                });
            } else {
                // 簡單的顯示邏輯
                toast.classList.add('showing');
                
                setTimeout(() => {
                    toast.classList.add('show');
                }, 100);
                
                setTimeout(() => {
                    toast.classList.remove('show');
                    setTimeout(() => {
                        toast.remove();
                    }, 300);
                }, duration);
            }
        }, 100);
    }
    
    createToast(message, type) {
        let template;
        
        switch (type) {
            case 'success':
                template = this.successTemplate;
                break;
            case 'error':
                template = this.errorTemplate;
                break;
            default:
                template = this.successTemplate;
        }
        
        if (!template) {
            // 創建簡單的 Toast
            return this.createSimpleToast(message, type);
        }
        
        const toastElement = template.content.cloneNode(true);
        const toast = toastElement.querySelector('.toast');
        const messageSpan = toast.querySelector('.message-text');
        
        if (messageSpan) {
            messageSpan.textContent = message;
        }
        
        return toast;
    }
    
    createSimpleToast(message, type) {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-bg-${type === 'error' ? 'danger' : 'success'} border-0`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        
        const icon = type === 'error' ? 'exclamation-circle' : 'check-circle';
        
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-${icon} me-2" aria-hidden="true"></i>
                    <span class="message-text">${message}</span>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                        data-bs-dismiss="toast" aria-label="關閉"></button>
            </div>
        `;
        
        return toast;
    }
    
    createToastContainer() {
        const container = document.createElement('div');
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
        return container;
    }
    
    // 事件處理方法
    handleFilesChanged(event) {
        const { count } = event.detail;
        
        if (count > 0) {
            // 可以在這裡顯示檔案變更的提示
            console.log(`檔案數量變更為 ${count}`);
        }
    }
    
    handleItemReordered(event) {
        const { fromOrder, toOrder } = event.detail;
        
        // 顯示排序成功提示 (需求 13)
        this.showSuccess(`圖片順序已從第 ${fromOrder} 位調整為第 ${toOrder} 位`);
    }
    
    handleUploadError(event) {
        const { error } = event.detail;
        this.hideLoading();
        this.showError('上傳失敗：' + (error.message || '未知錯誤'));
    }
    
    // 輔助方法
    getFileCount() {
        const fileInputs = this.container.querySelectorAll('.image-checkbox[type="checkbox"]');
        return fileInputs.length;
    }
    
    getSelectedCount() {
        const selectedInputs = this.container.querySelectorAll('.image-checkbox[type="checkbox"]:checked');
        return selectedInputs.length;
    }
    
    getTotalCount() {
        const allItems = this.container.querySelectorAll('.image-item');
        return allItems.length;
    }
    
    // 公共 API
    showConfirmDialog(message, title, confirmText, confirmClass) {
        return this.showConfirmation(message, title, confirmText, confirmClass);
    }
    
    showLoadingDialog(text) {
        this.showLoadingWithText(text);
    }
    
    hideLoadingDialog() {
        this.hideLoading();
    }
    
    showSuccessToast(message, duration) {
        this.showToast(message, 'success', duration);
    }
    
    showErrorToast(message, duration) {
        this.showToast(message, 'error', duration);
    }
    
    // 全域設定
    static setGlobalInstance(instance) {
        window.ImageModalManager = instance;
    }
}

// 導出供其他模組使用
window.ImageModalManager = ImageModalManager;