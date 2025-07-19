/**
 * 圖片預覽管理器 - 處理拖曳上傳、檔案選擇和前端預覽功能
 * 實作需求 1, 2, 6, 8, 10, 17, 18
 */
class ImagePreviewManager {
    constructor(container) {
        this.container = container;
        this.uploadZone = container.querySelector('.image-upload-zone');
        this.fileInput = container.querySelector('.file-input');
        this.previewGrid = container.querySelector('.image-preview-grid');
        this.pendingSection = container.querySelector('.pending-images-section');
        this.pendingContainer = container.querySelector('.pending-container');
        this.emptyState = container.querySelector('.empty-state');
        this.currentCountSpan = container.querySelector('.count-number');
        this.pendingCountSpan = container.querySelector('.pending-count');
        
        // 取得配置參數
        this.maxFiles = parseInt(container.dataset.maxFiles) || 10;
        this.maxFileSize = parseInt(container.dataset.maxFileSize) || 5242880; // 5MB
        this.allowedTypes = container.dataset.allowedTypes ? 
            container.dataset.allowedTypes.split(',') : ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
        
        // 狀態管理
        this.selectedFiles = new Map(); // 維護檔案順序和資訊
        this.displayOrder = 1;
        this.dragCounter = 0;
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        this.updateUI();
        console.log('圖片預覽管理器初始化完成');
    }
    
    setupEventListeners() {
        // 拖曳事件 (需求 1)
        this.uploadZone.addEventListener('dragenter', this.handleDragEnter.bind(this));
        this.uploadZone.addEventListener('dragover', this.handleDragOver.bind(this));
        this.uploadZone.addEventListener('dragleave', this.handleDragLeave.bind(this));
        this.uploadZone.addEventListener('drop', this.handleDrop.bind(this));
        
        // 點擊選擇檔案 (需求 1)
        const selectButton = this.container.querySelector('.btn-select-files');
        if (selectButton) {
            selectButton.addEventListener('click', () => this.fileInput.click());
        }
        
        // 鍵盤存取支援
        this.uploadZone.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.fileInput.click();
            }
        });
        
        this.fileInput.addEventListener('change', this.handleFileSelect.bind(this));
    }
    
    handleDragEnter(e) {
        e.preventDefault();
        this.dragCounter++;
        this.uploadZone.classList.add('dragover');
        this.showOverlay();
    }
    
    handleDragOver(e) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
    }
    
    handleDragLeave(e) {
        e.preventDefault();
        this.dragCounter--;
        if (this.dragCounter === 0) {
            this.uploadZone.classList.remove('dragover');
            this.hideOverlay();
        }
    }
    
    handleDrop(e) {
        e.preventDefault();
        this.dragCounter = 0;
        this.uploadZone.classList.remove('dragover');
        this.hideOverlay();
        
        const files = Array.from(e.dataTransfer.files);
        this.addFiles(files);
    }
    
    handleFileSelect(e) {
        const files = Array.from(e.target.files);
        this.addFiles(files);
        // 清空 input 以允許重複選擇相同檔案
        e.target.value = '';
    }
    
    async addFiles(files) {
        const validFiles = [];
        const errors = [];
        
        for (const file of files) {
            const validation = this.validateFile(file);
            if (validation.valid) {
                validFiles.push(file);
            } else {
                errors.push(validation.error);
            }
        }
        
        // 顯示錯誤訊息
        if (errors.length > 0) {
            this.showErrors(errors);
        }
        
        // 處理有效檔案
        for (const file of validFiles) {
            // 檢查是否已達到最大檔案數限制
            if (this.selectedFiles.size >= this.maxFiles) {
                this.showError(`最多只能選擇 ${this.maxFiles} 張圖片`);
                break;
            }
            
            try {
                const fileId = this.generateFileId();
                const previewUrl = await this.createImagePreview(file);
                
                // 自動在最後插入排序 (需求 8)
                this.selectedFiles.set(fileId, {
                    file: file,
                    previewUrl: previewUrl,
                    displayOrder: this.displayOrder++,
                    fileName: file.name,
                    fileSize: this.formatFileSize(file.size),
                    timestamp: Date.now()
                });
            } catch (error) {
                console.error('建立圖片預覽失敗:', error);
                this.showError(`檔案 ${file.name} 處理失敗`);
            }
        }
        
        this.updateUI();
        this.notifyFilesChanged();
    }
    
    validateFile(file) {
        // 檢查檔案類型
        if (!this.allowedTypes.includes(file.type)) {
            return {
                valid: false,
                error: `檔案 ${file.name} 格式不支援。支援格式：${this.getAllowedTypesString()}`
            };
        }
        
        // 檢查檔案大小 (需求 17)
        if (file.size > this.maxFileSize) {
            return {
                valid: false,
                error: `檔案 ${file.name} 大小超過限制 (${this.formatFileSize(this.maxFileSize)})`
            };
        }
        
        // 檢查是否為圖片檔案
        if (!file.type.startsWith('image/')) {
            return {
                valid: false,
                error: `檔案 ${file.name} 不是有效的圖片檔案`
            };
        }
        
        return { valid: true };
    }
    
    createImagePreview(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (e) => resolve(e.target.result);
            reader.onerror = (e) => reject(e);
            reader.readAsDataURL(file);
        });
    }
    
    updateUI() {
        this.updateImageCount(); // 需求 18
        this.updatePreviewDisplay(); // 需求 2
        this.updateEmptyState();
        this.updateButtonStates();
    }
    
    updateImageCount() {
        const count = this.selectedFiles.size;
        if (this.currentCountSpan) {
            this.currentCountSpan.textContent = count;
        }
        if (this.pendingCountSpan) {
            this.pendingCountSpan.textContent = count;
        }
        
        // 更新 aria-live 區域
        const statusInfo = this.container.querySelector('[aria-live="polite"]');
        if (statusInfo) {
            statusInfo.textContent = `已選擇 ${count} 張圖片，最多可選擇 ${this.maxFiles} 張`;
        }
    }
    
    updatePreviewDisplay() {
        if (this.selectedFiles.size === 0) {
            this.hidePendingSection();
            return;
        }
        
        this.showPendingSection();
        this.renderPreviewItems();
    }
    
    renderPreviewItems() {
        if (!this.pendingContainer) return;
        
        this.pendingContainer.innerHTML = '';
        
        // 按顯示順序排序 (需求 10: 順序顯示)
        const sortedFiles = Array.from(this.selectedFiles.entries())
            .sort((a, b) => a[1].displayOrder - b[1].displayOrder);
        
        sortedFiles.forEach(([fileId, fileData]) => {
            const itemElement = this.createPreviewItem(fileId, fileData);
            this.pendingContainer.appendChild(itemElement);
        });
    }
    
    createPreviewItem(fileId, fileData) {
        const item = document.createElement('div');
        item.className = 'image-item pending-image';
        item.dataset.imageId = fileId;
        item.dataset.displayOrder = fileData.displayOrder;
        item.setAttribute('role', 'gridcell');
        item.setAttribute('tabindex', '0');
        item.setAttribute('aria-label', `待上傳圖片 ${fileData.fileName}，順序 ${fileData.displayOrder}`);
        
        item.innerHTML = `
            <!-- 圖片縮圖 -->
            <div class="image-thumbnail">
                <img src="${fileData.previewUrl}" 
                     alt="${fileData.fileName}"
                     loading="lazy"
                     draggable="false">
            </div>

            <!-- 圖片資訊 (需求 2) -->
            <div class="image-info">
                <div class="image-filename" title="${fileData.fileName}">
                    ${fileData.fileName}
                </div>
                <div class="image-meta text-muted">
                    <span class="file-size">${fileData.fileSize}</span>
                    <span class="display-order-badge">
                        #${fileData.displayOrder}
                    </span>
                </div>
            </div>

            <!-- 操作按鈕 -->
            <div class="image-actions">
                <label class="image-checkbox">
                    <input type="checkbox" 
                           value="${fileId}"
                           aria-label="選擇圖片 ${fileData.fileName}">
                    <span class="checkmark">
                        <i class="fas fa-check" aria-hidden="true"></i>
                    </span>
                </label>
                <button type="button" 
                        class="btn btn-sm btn-outline-danger btn-delete-single"
                        data-image-id="${fileId}"
                        aria-label="刪除圖片 ${fileData.fileName}">
                    <i class="fas fa-trash" aria-hidden="true"></i>
                </button>
                <div class="drag-handle" 
                     aria-label="拖拽調整順序"
                     role="button"
                     tabindex="0">
                    <i class="fas fa-grip-vertical" aria-hidden="true"></i>
                </div>
            </div>
        `;
        
        // 加入刪除事件監聽器
        const deleteBtn = item.querySelector('.btn-delete-single');
        deleteBtn.addEventListener('click', () => this.removeFile(fileId));
        
        return item;
    }
    
    removeFile(fileId) {
        if (this.selectedFiles.has(fileId)) {
            this.selectedFiles.delete(fileId);
            this.reorderAfterDeletion(); // 需求 9: 刪除後自動遞補排序缺口
            this.updateUI();
            this.notifyFilesChanged();
        }
    }
    
    reorderAfterDeletion() {
        // 重新排序以填補缺口 (需求 9, 10)
        const sortedFiles = Array.from(this.selectedFiles.entries())
            .sort((a, b) => a[1].displayOrder - b[1].displayOrder);
        
        let newOrder = 1;
        sortedFiles.forEach(([fileId, fileData]) => {
            fileData.displayOrder = newOrder++;
        });
        
        this.displayOrder = newOrder;
    }
    
    updateEmptyState() {
        if (this.emptyState) {
            if (this.selectedFiles.size === 0) {
                this.emptyState.classList.remove('d-none');
            } else {
                this.emptyState.classList.add('d-none');
            }
        }
    }
    
    updateButtonStates() {
        // 更新上傳確認按鈕狀態 (需求 7)
        const uploadBtn = this.container.querySelector('.btn-upload-confirm');
        if (uploadBtn) {
            uploadBtn.disabled = this.selectedFiles.size === 0;
        }
        
        // 更新全部清除按鈕狀態 (需求 7)
        const clearAllBtn = this.container.querySelector('.btn-clear-all');
        if (clearAllBtn) {
            clearAllBtn.disabled = this.selectedFiles.size === 0;
        }
        
        // 更新全選按鈕狀態
        const selectAllBtn = this.container.querySelector('.btn-select-all');
        if (selectAllBtn) {
            selectAllBtn.disabled = this.selectedFiles.size === 0;
        }
    }
    
    showPendingSection() {
        if (this.pendingSection) {
            this.pendingSection.classList.remove('d-none');
        }
    }
    
    hidePendingSection() {
        if (this.pendingSection) {
            this.pendingSection.classList.add('d-none');
        }
    }
    
    showOverlay() {
        const overlay = this.uploadZone.querySelector('.upload-zone-overlay');
        if (overlay) {
            overlay.classList.add('active');
        }
    }
    
    hideOverlay() {
        const overlay = this.uploadZone.querySelector('.upload-zone-overlay');
        if (overlay) {
            overlay.classList.remove('active');
        }
    }
    
    // 輔助方法
    generateFileId() {
        return `file_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }
    
    formatFileSize(bytes) {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    }
    
    getAllowedTypesString() {
        return this.allowedTypes
            .map(type => type.replace('image/', '').toUpperCase())
            .join(', ');
    }
    
    showError(message) {
        console.error(message);
        // 這裡可以整合 Modal 管理器來顯示錯誤
        if (window.ImageModalManager) {
            window.ImageModalManager.showError(message);
        } else {
            alert(message); // 暫時的錯誤顯示方式
        }
    }
    
    showErrors(errors) {
        const message = errors.join('\n');
        this.showError(message);
    }
    
    notifyFilesChanged() {
        // 派發自訂事件，讓其他組件可以監聽檔案變更
        const event = new CustomEvent('filesChanged', {
            detail: {
                files: this.selectedFiles,
                count: this.selectedFiles.size
            }
        });
        this.container.dispatchEvent(event);
    }
    
    // 公共 API
    getSelectedFiles() {
        return this.selectedFiles;
    }
    
    clearAllFiles() {
        this.selectedFiles.clear();
        this.displayOrder = 1;
        this.updateUI();
        this.notifyFilesChanged();
    }
    
    getFileCount() {
        return this.selectedFiles.size;
    }
    
    canAddMoreFiles() {
        return this.selectedFiles.size < this.maxFiles;
    }
}

// 導出供其他模組使用
window.ImagePreviewManager = ImagePreviewManager;