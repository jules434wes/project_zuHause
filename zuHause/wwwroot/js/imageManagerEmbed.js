/**
 * zuHause 統一圖片管理器 JavaScript 組件
 * 用於支援 zh-image-manager Tag Helper
 */

class ZhImageManager {
    constructor(element) {
        this.container = element;
        this.entityType = element.dataset.entityType;
        this.entityId = parseInt(element.dataset.entityId);
        this.category = element.dataset.category;
        this.maxCount = parseInt(element.dataset.maxCount) || 15;
        
        // 已上傳的圖片（來自伺服器）
        this.uploadedImages = [];
        // 預覽中的圖片（尚未上傳，保存在瀏覽器）
        this.previewImages = [];
        // 選中的圖片ID集合（為了避免錯誤而保留，但不使用）
        this.selectedImages = new Set();
        // 載入狀態
        this.isLoading = false;
        this.dragCounter = 0;
        // 排序變更追蹤
        this.hasUnsavedChanges = false;
        this.originalOrder = null;
        
        this.init();
    }
    
    async init() {
        this.renderContainer();
        this.bindEvents();
        await this.loadImages();
        this.initSortable();
    }
    
    renderContainer() {
        this.container.innerHTML = `
            <div class="zh-image-manager-wrapper">
                <div class="image-manager-header">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <h5 class="mb-0">
                            <i class="fas fa-images me-2"></i>圖片管理
                            <span class="badge bg-secondary ms-2" id="imageCount">0/${this.maxCount}</span>
                        </h5>
                        <div class="btn-group">
                            <button class="btn btn-success btn-sm" data-action="confirm-upload" id="confirmUploadBtn" disabled>
                                <i class="fas fa-cloud-upload-alt me-1"></i>確認上傳
                            </button>
                            <button class="btn btn-danger btn-sm" data-action="delete-all" id="deleteAllBtn" disabled>
                                <i class="fas fa-trash me-1"></i>全部刪除
                            </button>
                            <button class="btn btn-warning btn-sm" data-action="delete-selected" id="deleteSelectedBtn" style="display: none;">
                                <i class="fas fa-trash me-1"></i>刪除選中
                            </button>
                            <button class="btn btn-primary btn-sm" data-action="save-order" id="saveOrderBtn" disabled>
                                <i class="fas fa-save me-1"></i>保存排序
                            </button>
                            <button class="btn btn-outline-secondary btn-sm" data-action="refresh">
                                <i class="fas fa-sync me-1"></i>重新載入
                            </button>
                        </div>
                    </div>
                </div>
                
                <!-- 拖拽上傳區域 -->
                <div class="drop-zone" id="dropZone">
                    <div class="drop-zone-content">
                        <i class="fas fa-cloud-upload-alt fa-2x text-muted mb-2"></i>
                        <p class="text-muted mb-1">拖拽圖片檔案到此處或點擊此區域選擇檔案</p>
                        <small class="text-info d-block mt-1">支援 JPG、PNG、WebP 格式</small>
                    </div>
                </div>
                
                <!-- 隱藏的檔案輸入 -->
                <input type="file" id="fileInput-${this.entityType}-${this.entityId}" 
                       multiple accept="image/*" style="display: none;">
                
                <!-- 圖片網格 -->
                <div class="image-grid" id="imageGrid">
                    <div class="loading-placeholder text-center py-4">
                        <i class="fas fa-spinner fa-spin fa-2x text-muted"></i>
                        <p class="text-muted mt-2">載入中...</p>
                    </div>
                </div>
                
                <!-- 狀態列 -->
                <div class="status-bar mt-3">
                    <div class="row text-center">
                        <div class="col-4">
                            <small class="text-muted">已上傳</small>
                            <div class="h6 mb-0" id="uploadedCount">0</div>
                        </div>
                        <div class="col-4">
                            <small class="text-muted">已選擇</small>
                            <div class="h6 mb-0" id="selectedCount">0</div>
                        </div>
                        <div class="col-4">
                            <small class="text-muted">狀態</small>
                            <div class="h6 mb-0" id="statusText">就緒</div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }
    
    bindEvents() {
        const dropZone = this.container.querySelector('#dropZone');
        const fileInput = this.container.querySelector(`#fileInput-${this.entityType}-${this.entityId}`);
        
        // 拖拽事件
        dropZone.addEventListener('dragenter', (e) => this.handleDragEnter(e));
        dropZone.addEventListener('dragover', (e) => this.handleDragOver(e));
        dropZone.addEventListener('dragleave', (e) => this.handleDragLeave(e));
        dropZone.addEventListener('drop', (e) => this.handleDrop(e));
        
        // 拖拽區域點擊事件（總是觸發檔案選擇）
        dropZone.addEventListener('click', (e) => {
            e.preventDefault();
            this.selectFiles();
        });
        
        // 檔案選擇事件
        fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        
        // 按鈕事件委託
        this.container.addEventListener('click', (e) => {
            const actionElement = e.target.closest('[data-action]');
            if (!actionElement) {
                // 檢查是否點擊圖片項目本身（用於選擇）
                const imageItem = e.target.closest('.image-item');
                if (imageItem && e.target.tagName === 'IMG') {
                    // 圖片選擇功能已禁用
                    // const imageId = imageItem.dataset.imageId;
                    // this.toggleImageSelection(imageId);
                }
                return;
            }
            
            const action = actionElement.dataset.action;
            e.preventDefault();
            e.stopPropagation();
            
            if (action === 'confirm-upload') {
                this.confirmUpload();
            } else if (action === 'delete-all') {
                this.deleteAll();
            } else if (action === 'delete-selected') {
                this.deleteSelected();
            } else if (action === 'refresh') {
                this.refreshImages();
            } else if (action === 'save-order') {
                this.saveOrder();
            } else if (action === 'toggle-select') {
                // 圖片選擇功能已禁用
                // const imageId = actionElement.dataset.imageId;
                // this.toggleImageSelection(imageId);
            } else if (action === 'set-main') {
                const imageId = parseInt(actionElement.dataset.imageId);
                this.setMainImage(imageId);
            } else if (action === 'set-main-preview') {
                const imageId = actionElement.dataset.imageId;
                this.setMainImagePreview(imageId);
            } else if (action === 'delete-uploaded') {
                const imageId = parseInt(actionElement.dataset.imageId);
                this.deleteUploadedImage(imageId);
            } else if (action === 'delete-preview') {
                const imageId = actionElement.dataset.imageId;
                this.deletePreviewImage(imageId);
            } else if (action === 'preview-image') {
                const imageUrl = actionElement.dataset.imageUrl;
                this.previewImage(imageUrl);
            }
        });
    }
    
    async loadImages() {
        if (this.isLoading) return;
        
        this.setLoading(true);
        
        try {
            const response = await fetch(
                `/api/ImageManagerApi/list?entityType=${this.getEntityTypeValue()}&entityId=${this.entityId}&category=${this.getCategoryValue()}`
            );
            
            if (response.ok) {
                const data = await response.json();
                this.uploadedImages = data.Data?.Images || [];
                this.uploadedImages.sort((a, b) => (a.DisplayOrder || 0) - (b.DisplayOrder || 0));
                
                // 調試資訊：檢查主圖狀態
                console.log('載入的圖片資料:', this.uploadedImages.map(img => ({
                    ImageId: img.ImageId,
                    FileName: img.FileName,
                    IsMainImage: img.IsMainImage,
                    isMainImage: img.isMainImage,
                    DisplayOrder: img.DisplayOrder
                })));
                
                this.saveOriginalOrder(); // 保存原始順序
                this.renderAllImages();
            } else {
                this.showError('載入圖片失敗');
            }
        } catch (error) {
            console.error('載入圖片錯誤:', error);
            this.showError('網路錯誤');
        } finally {
            this.setLoading(false);
        }
    }
    
    renderAllImages() {
        const grid = this.container.querySelector('#imageGrid');
        const imageCount = this.container.querySelector('#imageCount');
        const wrapper = this.container.querySelector('.zh-image-manager-wrapper');
        
        // 合併已上傳和預覽圖片，按順序排列
        const allImages = [...this.uploadedImages, ...this.previewImages];
        const totalCount = allImages.length;
        
        imageCount.textContent = `${totalCount}/${this.maxCount}`;
        
        // 根據是否有圖片來控制樣式
        if (totalCount === 0) {
            wrapper.classList.remove('has-images');
        } else {
            wrapper.classList.add('has-images');
        }
        
        if (totalCount === 0) {
            grid.innerHTML = `
                <div class="empty-state text-center py-4">
                    <i class="fas fa-images fa-3x text-muted mb-3"></i>
                    <h6 class="text-muted">尚未選擇任何圖片</h6>
                    <p class="text-muted small">拖拽檔案或點擊「選擇圖片」開始添加</p>
                </div>
            `;
            return;
        }
        
        const self = this;
        
        // 分別渲染圖片Grid和勾選框覆蓋層
        grid.innerHTML = allImages.map((image, index) => {
            const imageId = image.ImageId || image.id; // 已上傳圖片用ImageId，預覽圖片用臨時id
            const isUploaded = image.isUploaded !== false && image.ImageId; // 判斷是否已上傳
            const displayUrl = isUploaded ? this.getImageDisplayUrl(image) : image.previewUrl;
            
            return `
            <div class="image-item ${isUploaded ? 'uploaded' : 'preview'}" 
                 data-image-id="${imageId}"
                 data-is-uploaded="${isUploaded}"
                 data-index="${index}">
                
                <!-- 圖片順序標記 -->
                <div class="image-order-badge">${index + 1}</div>
                
                <!-- 主圖標記（如果是主圖） -->
                ${image.IsMainImage || image.isMainImage ? '<div class="main-image-badge"><i class="fas fa-star"></i></div>' : ''}
                
                <!-- 狀態標記 -->
                <div class="image-status-badge">
                    ${isUploaded ? 
                        '<span class="badge bg-success">已上傳</span>' : 
                        '<span class="badge bg-info">待上傳</span>'
                    }
                </div>
                
                <!-- 圖片主體 -->
                <div class="image-wrapper">
                    <img src="${displayUrl}" 
                         alt="${image.FileName || image.fileName}" 
                         loading="lazy"
                         ${isUploaded ? `onerror="this.src='${image.OriginalUrl || image.ThumbnailUrl}'"` : ''}>
                    
                    <!-- 刪除按鈕 - 右上角內側 -->
                    <button class="image-delete-btn" 
                            data-action="${isUploaded ? 'delete-uploaded' : 'delete-preview'}" 
                            data-image-id="${imageId}"
                            title="${isUploaded ? '刪除' : '移除'}">
                        <i class="fas fa-trash"></i>
                    </button>
                    
                    <!-- 主圖按鈕 - 右下角內側 -->
                    <button class="image-main-btn ${image.IsMainImage || image.isMainImage ? 'is-main' : ''}" 
                            data-action="${isUploaded ? 'set-main' : 'set-main-preview'}" 
                            data-image-id="${imageId}"
                            title="設為主圖">
                        <i class="fas fa-star"></i>
                    </button>
                    
                    <!-- 預覽按鈕（hover時顯示） -->
                    <button class="image-preview-btn" 
                            data-action="preview-image"
                            data-image-url="${displayUrl}"
                            title="預覽圖片">
                        <i class="fas fa-eye"></i>
                    </button>
                </div>
                
                <!-- 圖片資訊 -->
                <div class="image-info">
                    <small class="text-muted text-truncate">${image.FileName || image.fileName}</small>
                    ${!isUploaded ? `<small class="text-info d-block">${(image.size / 1024).toFixed(1)} KB</small>` : ''}
                </div>
            </div>
        `;}).join('');
        
        this.updateButtonStates();
        this.updateStatusBar();
        this.initSortable();
    }
    
    updateButtonStates() {
        const confirmUploadBtn = this.container.querySelector('#confirmUploadBtn');
        const deleteAllBtn = this.container.querySelector('#deleteAllBtn');
        const saveOrderBtn = this.container.querySelector('#saveOrderBtn');
        
        const hasPreviewImages = this.previewImages.length > 0;
        const hasAnyImages = this.uploadedImages.length > 0 || this.previewImages.length > 0;
        
        // 確認上傳按鈕：只有預覽圖片時才可點選（需求第7點）
        if (confirmUploadBtn) {
            confirmUploadBtn.disabled = !hasPreviewImages || this.isLoading;
            confirmUploadBtn.innerHTML = this.isLoading ? 
                '<i class="fas fa-spinner fa-spin me-1"></i>上傳中...' :
                `<i class="fas fa-cloud-upload-alt me-1"></i>確認上傳 (${this.previewImages.length})`;
        }
        
        // 全部刪除按鈕：圖片數為0時不可點選（需求第7點）
        if (deleteAllBtn) {
            deleteAllBtn.disabled = !hasAnyImages || this.isLoading;
        }
        
        
        // 保存排序按鈕：只有在有未保存變更時才可點選
        if (saveOrderBtn) {
            saveOrderBtn.disabled = !this.hasUnsavedChanges || this.isLoading;
        }
    }
    
    initSortable() {
        const grid = this.container.querySelector('#imageGrid');
        const allImages = [...this.uploadedImages, ...this.previewImages];
        
        if (!grid || allImages.length === 0) return;
        
        if (this.sortableInstance) {
            this.sortableInstance.destroy();
        }
        
        // 需求第5、6點：拖拽排序功能
        this.sortableInstance = Sortable.create(grid, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            chosenClass: 'sortable-chosen',
            filter: '.empty-state',
            onEnd: async (evt) => {
                if (evt.oldIndex !== evt.newIndex) {
                    const allImages = [...this.uploadedImages, ...this.previewImages];
                    const movedImage = allImages[evt.oldIndex];
                    
                    // 檢查是否為預覽圖片嘗試移動到已上傳圖片前方
                    const isPreviewImage = !movedImage.ImageId || movedImage.isUploaded === false;
                    const uploadedImagesCount = this.uploadedImages.length;
                    
                    if (isPreviewImage && evt.newIndex < uploadedImagesCount) {
                        // 預覽圖片不能移動到已上傳圖片前方
                        this.showError('請完成上傳後再排序圖片');
                        this.renderAllImages(); // 恢復原始排序
                        return;
                    }
                    
                    // 執行正常的排序邏輯
                    const reorderedImages = [...allImages];
                    const imageToMove = reorderedImages.splice(evt.oldIndex, 1)[0];
                    reorderedImages.splice(evt.newIndex, 0, imageToMove);
                    
                    // 重新分配到各自陣列
                    this.uploadedImages = reorderedImages.filter(img => img.isUploaded !== false && img.ImageId);
                    this.previewImages = reorderedImages.filter(img => img.isUploaded === false || !img.ImageId);
                    
                    // 更新顯示順序（需求第9、10點）
                    this.updateDisplayOrder();
                    this.renderAllImages();
                    
                    // 如果有已上傳圖片被移動，靜默標記有未保存的變更
                    if (this.uploadedImages.length > 0 && !isPreviewImage) {
                        this.markUnsavedChanges();
                    }
                }
            }
        });
    }
    
    selectFiles() {
        const fileInput = this.container.querySelector(`#fileInput-${this.entityType}-${this.entityId}`);
        fileInput.click();
    }
    
    refreshImages() {
        this.loadImages();
    }
    
    // 確認上傳功能（需求第16點）
    async confirmUpload() {
        if (this.previewImages.length === 0) {
            this.showError('沒有圖片可以上傳');
            return;
        }
        
        // 確認對話框（需求第11點 - 使用Modal）
        const confirmed = await this.showConfirm(
            `確定要上傳 ${this.previewImages.length} 張圖片嗎？`,
            '確認上傳'
        );
        
        if (!confirmed) return;
        
        this.setLoading(true);
        
        try {
            const formData = new FormData();
            formData.append('EntityType', this.getEntityTypeValue());
            formData.append('EntityId', this.entityId);
            formData.append('Category', this.getCategoryValue());
            formData.append('SkipEntityValidation', 'true');
            
            // 按照預覽順序添加檔案
            this.previewImages.forEach(previewImage => {
                formData.append('Files', previewImage.file);
            });
            
            const response = await fetch('/api/ImageManagerApi/upload', {
                method: 'POST',
                body: formData
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.Success) {
                    // 記錄上傳的圖片數量（在清空前記錄）
                    const uploadedCount = this.previewImages.length;
                    
                    // 清空預覽圖片
                    this.previewImages = [];
                    // 選擇狀態已移除，不需要清空
                    
                    // 重新載入已上傳的圖片
                    await this.loadImages();
                    
                    // 成功提示（需求第13點）
                    this.showSuccess(`成功上傳 ${uploadedCount} 張圖片！`);
                } else {
                    this.showError(data.Message || '上傳失敗');
                }
            } else {
                this.showError('上傳失敗');
            }
        } catch (error) {
            console.error('上傳錯誤:', error);
            this.showError('網路錯誤');
        } finally {
            this.setLoading(false);
        }
    }
    
    // 切換圖片選擇狀態（需求第3點）
    toggleImageSelection(imageId) {
        if (this.selectedImages.has(imageId)) {
            this.selectedImages.delete(imageId);
        } else {
            this.selectedImages.add(imageId);
        }
        this.renderAllImages();
    }
    
    // 刪除所有圖片（需求第4點）
    async deleteAll() {
        const allCount = this.uploadedImages.length + this.previewImages.length;
        if (allCount === 0) return;
        
        const confirmed = await this.showConfirm(
            `確定要刪除所有 ${allCount} 張圖片嗎？`,
            '刪除所有圖片'
        );
        
        if (!confirmed) return;
        
        try {
            // 刪除已上傳的圖片
            for (const image of this.uploadedImages) {
                await fetch(`/api/ImageManagerApi/delete/${image.ImageId}`, { method: 'DELETE' });
            }
            
            // 清空預覽圖片
            this.previewImages = [];
            this.selectedImages.clear();
            
            // 重新載入
            await this.loadImages();
            this.showSuccess('已刪除所有圖片');
        } catch (error) {
            console.error('刪除失敗:', error);
            this.showError('刪除失敗');
        }
    }
    
    // 刪除選中的圖片（需求第4點）
    async deleteSelected() {
        if (this.selectedImages.size === 0) return;
        
        const confirmed = await this.showConfirm(
            `確定要刪除選中的 ${this.selectedImages.size} 張圖片嗎？`,
            '刪除選中圖片'
        );
        
        if (!confirmed) return;
        
        if (this.isLoading) return;
        this.setLoading(true);
        
        try {
            // 分別處理已上傳和預覽圖片
            for (const imageId of this.selectedImages) {
                if (typeof imageId === 'string' && imageId.startsWith('temp_')) {
                    // 預覽圖片，直接從陣列移除
                    this.previewImages = this.previewImages.filter(img => img.id !== imageId);
                } else {
                    // 已上傳圖片，呼叫API刪除
                    await fetch(`/api/ImageManagerApi/delete/${imageId}`, { method: 'DELETE' });
                }
            }
            
            this.selectedImages.clear();
            await this.loadImages();
            this.showSuccess('已刪除選中的圖片');
        } catch (error) {
            console.error('刪除失敗:', error);
            this.showError('刪除失敗');
        } finally {
            this.setLoading(false);
        }
    }
    
    // 刪除單個預覽圖片
    deletePreviewImage(imageId) {
        this.previewImages = this.previewImages.filter(img => img.id !== imageId);
        this.selectedImages.delete(imageId);
        this.renderAllImages();
    }
    
    // 刪除單個已上傳圖片
    async deleteUploadedImage(imageId) {
        const confirmed = await this.showConfirm('確定要刪除這張圖片嗎？', '刪除確認');
        if (!confirmed) return;
        
        if (this.isLoading) return;
        this.setLoading(true);
        
        try {
            const response = await fetch(`/api/ImageManagerApi/delete/${imageId}`, { method: 'DELETE' });
            if (response.ok) {
                this.selectedImages.delete(imageId);
                await this.loadImages();
                this.showSuccess('圖片已刪除');
            } else {
                this.showError('刪除失敗');
            }
        } catch (error) {
            console.error('刪除錯誤:', error);
            this.showError('網路錯誤');
        } finally {
            this.setLoading(false);
        }
    }
    
    // 設定預覽圖片為主圖
    setMainImagePreview(imageId) {
        // 顯示警告訊息
        this.showError('請完成上傳後，再重新設定主圖');
        return;
        
        // 以下代碼暫時保留，但不會執行
        // 清除所有主圖標記
        this.previewImages.forEach(img => img.isMainImage = false);
        // 設定新的主圖
        const targetImage = this.previewImages.find(img => img.id === imageId);
        if (targetImage) {
            targetImage.isMainImage = true;
        }
        this.renderAllImages();
    }
    
    handleDragEnter(e) {
        e.preventDefault();
        this.dragCounter++;
        e.currentTarget.classList.add('drag-over');
    }
    
    handleDragOver(e) {
        e.preventDefault();
    }
    
    handleDragLeave(e) {
        e.preventDefault();
        this.dragCounter--;
        if (this.dragCounter === 0) {
            e.currentTarget.classList.remove('drag-over');
        }
    }
    
    handleDrop(e) {
        e.preventDefault();
        this.dragCounter = 0;
        e.currentTarget.classList.remove('drag-over');
        
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            this.addFilesToPreview(files);
        }
    }
    
    handleFileSelect(e) {
        const files = e.target.files;
        if (files.length > 0) {
            this.addFilesToPreview(files);
        }
    }
    
    addFilesToPreview(files) {
        // 檢查圖片數量限制
        const totalCount = this.uploadedImages.length + this.previewImages.length + files.length;
        if (totalCount > this.maxCount) {
            this.showError(`圖片總數不能超過 ${this.maxCount} 張`);
            return;
        }
        
        let processedCount = 0;
        const newPreviewImages = [];
        
        Array.from(files).forEach((file, index) => {
            if (file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    // 為預覽圖片生成臨時ID
                    const tempId = 'temp_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
                    
                    newPreviewImages.push({
                        id: tempId,
                        file: file,
                        previewUrl: e.target.result,
                        fileName: file.name,
                        size: file.size,
                        displayOrder: this.uploadedImages.length + this.previewImages.length + newPreviewImages.length,
                        isMainImage: false, // 預設不是主圖
                        isUploaded: false,
                        isSelected: false
                    });
                    
                    processedCount++;
                    
                    // 當所有檔案都處理完成時
                    if (processedCount === Array.from(files).filter(f => f.type.startsWith('image/')).length) {
                        // 新圖片自動在最後插入排序（需求第8點）
                        this.previewImages.push(...newPreviewImages);
                        this.renderAllImages();
                        this.updateButtonStates();
                        
                        // 清空檔案輸入
                        const fileInput = this.container.querySelector(`#fileInput-${this.entityType}-${this.entityId}`);
                        if (fileInput) fileInput.value = '';
                    }
                };
                reader.readAsDataURL(file);
            }
        });
        
        const imageFiles = Array.from(files).filter(f => f.type.startsWith('image/'));
        if (imageFiles.length === 0) {
            this.showError('請選擇有效的圖片檔案（JPG、PNG、WebP）');
        }
    }
    
    async uploadFiles(files) {
        if (this.isLoading) return;
        
        // 檢查圖片數量限制
        if (this.currentImages.length + files.length > this.maxCount) {
            this.showError(`圖片總數不能超過 ${this.maxCount} 張`);
            return;
        }
        
        this.setLoading(true);
        
        try {
            const formData = new FormData();
            formData.append('EntityType', this.getEntityTypeValue());
            formData.append('EntityId', this.entityId);
            formData.append('Category', this.getCategoryValue());
            formData.append('SkipEntityValidation', 'true');
            
            Array.from(files).forEach(file => {
                if (file.type.startsWith('image/')) {
                    formData.append('Files', file);
                }
            });
            
            const response = await fetch('/api/ImageManagerApi/upload', {
                method: 'POST',
                body: formData
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.Success) {
                    // 清空檔案輸入
                    const fileInput = this.container.querySelector(`#fileInput-${this.entityType}-${this.entityId}`);
                    fileInput.value = '';
                    
                    // 重新載入圖片
                    await this.loadImages();
                    this.showSuccess(`成功上傳 ${files.length} 張圖片`);
                } else {
                    this.showError(data.Message || '上傳失敗');
                }
            } else {
                this.showError('上傳失敗');
            }
        } catch (error) {
            console.error('上傳錯誤:', error);
            this.showError('網路錯誤');
        } finally {
            this.setLoading(false);
        }
    }
    
    async deleteImage(imageId) {
        if (!confirm('確定要刪除這張圖片嗎？')) return;
        
        try {
            const response = await fetch(`/api/ImageManagerApi/delete/${imageId}`, {
                method: 'DELETE'
            });
            
            if (response.ok) {
                this.selectedImages.delete(imageId);
                await this.loadImages();
                this.showSuccess('圖片已刪除');
            } else {
                this.showError('刪除失敗');
            }
        } catch (error) {
            console.error('刪除錯誤:', error);
            this.showError('網路錯誤');
        }
    }
    
    async setMainImage(imageId) {
        // 檢查是否有預覽圖片未上傳
        if (this.previewImages.length > 0) {
            this.showError('請完成上傳後，再重新設定主圖');
            return;
        }

        if (this.isLoading) return;
        this.setLoading(true);

        try {
            // 準備請求資料
            const requestData = {
                ImageId: imageId,
                EntityType: this.getEntityTypeValue(),
                EntityId: this.entityId,
                Category: this.getCategoryValue()
            };
            
            // 記錄請求資料以便除錯
            console.log('設定主圖請求資料:', requestData);
            
            const response = await fetch('/api/ImageManagerApi/setMain', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });
            
            // 記錄響應狀態
            console.log('設定主圖響應狀態:', response.status, response.statusText);
            
            if (response.ok) {
                const data = await response.json();
                console.log('設定主圖響應資料:', data);
                
                if (data.Success) {
                    // 實現設定主圖的排序邏輯
                    await this.applyMainImageSorting(imageId);
                    
                    // 立即更新本地狀態，然後重新載入確認
                    this.updateLocalMainImageState(imageId);
                    
                    // 短暫延遲確保伺服器狀態已更新
                    setTimeout(async () => {
                        await this.loadImages();
                    }, 100);
                    
                    this.markUnsavedChanges(); // 標記有未保存的變更
                    this.showSuccess('主圖設定成功');
                } else {
                    console.error('設定主圖失敗:', data);
                    this.showError(data.Message || '設定主圖失敗');
                }
            } else {
                // 嘗試讀取錯誤響應內容
                let errorMessage = '設定主圖失敗';
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.Message || errorData.message || `HTTP ${response.status}: ${response.statusText}`;
                } catch (parseError) {
                    errorMessage = `HTTP ${response.status}: ${response.statusText}`;
                }
                console.error('設定主圖 HTTP 錯誤:', response.status, errorMessage);
                this.showError(errorMessage);
            }
        } catch (error) {
            console.error('設定主圖網路錯誤:', error);
            this.showError(`網路錯誤: ${error.message}`);
        } finally {
            this.setLoading(false);
        }
    }
    
    // 實現設定主圖的排序邏輯（用戶需求）
    async applyMainImageSorting(mainImageId) {
        try {
            // 找到被設定為主圖的圖片在當前排序中的位置
            const currentMainImageIndex = this.uploadedImages.findIndex(img => img.ImageId === mainImageId);
            if (currentMainImageIndex === -1) return;

            // 創建新的排序陣列
            const reorderedImages = [...this.uploadedImages];
            const mainImage = reorderedImages[currentMainImageIndex];
            
            // 移除主圖
            reorderedImages.splice(currentMainImageIndex, 1);
            
            // 將主圖插入到第一位
            reorderedImages.unshift(mainImage);
            
            // 計算新的DisplayOrder
            const orderData = reorderedImages.map((image, index) => ({
                imageId: image.ImageId,
                displayOrder: index + 1
            }));
            
            // 調用重新排序API
            const response = await fetch('/api/ImageManagerApi/reorder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    EntityType: this.getEntityTypeValue(),
                    EntityId: this.entityId,
                    ImageDisplayOrders: Object.fromEntries(
                        orderData.map(item => [item.imageId, item.displayOrder])
                    )
                })
            });
            
            if (!response.ok) {
                console.error('更新主圖排序失敗');
            }
        } catch (error) {
            console.error('更新主圖排序錯誤:', error);
        }
    }
    
    // 立即更新本地主圖狀態，不等待伺服器響應
    updateLocalMainImageState(newMainImageId) {
        // 清除所有圖片的主圖狀態
        this.uploadedImages.forEach(image => {
            image.IsMainImage = false;
            image.isMainImage = false;
        });
        
        // 設定新的主圖
        const newMainImage = this.uploadedImages.find(img => img.ImageId === newMainImageId);
        if (newMainImage) {
            newMainImage.IsMainImage = true;
            newMainImage.isMainImage = true;
        }
        
        // 立即重新渲染 UI
        this.renderAllImages();
        
        console.log('本地主圖狀態已更新:', newMainImageId);
    }
    
    async updateImageOrder() {
        try {
            const orderData = this.currentImages.map((image, index) => ({
                imageId: image.ImageId,
                displayOrder: index + 1
            }));
            
            const response = await fetch('/api/ImageManagerApi/reorder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(orderData)
            });
            
            if (!response.ok) {
                console.error('更新排序失敗');
            }
        } catch (error) {
            console.error('更新排序錯誤:', error);
        }
    }
    
    toggleSelection(imageId) {
        if (this.selectedImages.has(imageId)) {
            this.selectedImages.delete(imageId);
        } else {
            this.selectedImages.add(imageId);
        }
        this.renderAllImages();
    }
    
    previewImage(imageUrl) {
        // 簡單的圖片預覽
        const modal = document.createElement('div');
        modal.style.cssText = `
            position: fixed; top: 0; left: 0; width: 100%; height: 100%;
            background: rgba(0,0,0,0.8); z-index: 9999; display: flex;
            align-items: center; justify-content: center; cursor: pointer;
        `;
        
        const img = document.createElement('img');
        img.src = imageUrl;
        img.style.cssText = 'max-width: 90%; max-height: 90%; object-fit: contain;';
        
        modal.appendChild(img);
        modal.onclick = () => document.body.removeChild(modal);
        document.body.appendChild(modal);
    }
    
    setLoading(loading) {
        this.isLoading = loading;
        const statusText = this.container.querySelector('#statusText');
        if (statusText) {
            statusText.textContent = loading ? '載入中...' : '就緒';
        }
    }
    
    updateStatusBar() {
        const uploadedCount = this.container.querySelector('#uploadedCount');
        
        if (uploadedCount) {
            uploadedCount.textContent = this.uploadedImages.length;
        }
        // 選中數量功能已移除，不再顯示
    }
    
    // 更新顯示順序（需求第9、10點）
    updateDisplayOrder() {
        const allImages = [...this.uploadedImages, ...this.previewImages];
        allImages.forEach((image, index) => {
            image.displayOrder = index + 1;
        });
    }
    
    showSuccess(message) {
        this.showNotification(message, 'success');
    }
    
    showError(message) {
        this.showNotification(message, 'error');
    }
    
    // Modal 對話框系統（需求第11點 - 使用Modal而非Toast）
    showConfirm(message, title = '確認') {
        return new Promise((resolve) => {
            const modalId = 'zh-confirm-modal-' + Date.now();
            const modal = document.createElement('div');
            modal.innerHTML = `
                <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${title}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <p>${message}</p>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                                <button type="button" class="btn btn-primary" id="confirm-btn-${modalId}">確認</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
            const modalElement = document.getElementById(modalId);
            const bootstrapModal = new bootstrap.Modal(modalElement);
            
            // 確認按鈕事件
            document.getElementById(`confirm-btn-${modalId}`).addEventListener('click', () => {
                bootstrapModal.hide();
                resolve(true);
            });
            
            // 取消或關閉事件
            modalElement.addEventListener('hidden.bs.modal', () => {
                document.body.removeChild(modal);
                resolve(false);
            });
            
            bootstrapModal.show();
        });
    }
    
    showNotification(message, type = 'info') {
        // Modal 通知系統（需求第11、13點）
        const modalId = 'zh-notification-modal-' + Date.now();
        const modal = document.createElement('div');
        const isError = type === 'error';
        
        modal.innerHTML = `
            <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-sm">
                    <div class="modal-content">
                        <div class="modal-header bg-${isError ? 'danger' : 'success'} text-white">
                            <h6 class="modal-title">
                                <i class="fas ${isError ? 'fa-exclamation-triangle' : 'fa-check-circle'} me-2"></i>
                                ${isError ? '錯誤' : '成功'}
                            </h6>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body text-center">
                            <p class="mb-0">${message}</p>
                        </div>
                        <div class="modal-footer justify-content-center">
                            <button type="button" class="btn btn-${isError ? 'danger' : 'success'}" data-bs-dismiss="modal">確定</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        const modalElement = document.getElementById(modalId);
        const bootstrapModal = new bootstrap.Modal(modalElement);
        
        modalElement.addEventListener('hidden.bs.modal', () => {
            document.body.removeChild(modal);
        });
        
        bootstrapModal.show();
        
        // 自動關閉（錯誤訊息保持較久）
        setTimeout(() => {
            if (document.getElementById(modalId)) {
                bootstrapModal.hide();
            }
        }, isError ? 8000 : 3000);
    }
    
    getEntityTypeValue() {
        const typeMap = {
            'Property': 1,
            'Member': 0,
            'Furniture': 2,
            'Announcement': 3
        };
        return typeMap[this.entityType] || 1;
    }
    
    getCategoryValue() {
        const categoryMap = {
            'BedRoom': 0,
            'Living': 1,
            'Kitchen': 2,
            'Balcony': 3,
            'Gallery': 4,
            'Avatar': 5,
            'Product': 6
        };
        return categoryMap[this.category] || 4; // 預設為 Gallery
    }
    
    getImageDisplayUrl(image) {
        // 優先使用縮圖，如果沒有則使用原圖
        return image.ThumbnailUrl || image.MediumUrl || image.OriginalUrl || '';
    }
    
    // 儲存原始順序
    saveOriginalOrder() {
        this.originalOrder = this.uploadedImages.map(img => ({
            imageId: img.ImageId,
            displayOrder: img.DisplayOrder
        }));
    }
    
    // 檢查是否有未保存的變更
    checkForUnsavedChanges() {
        if (!this.originalOrder) return false;
        
        const currentOrder = this.uploadedImages.map(img => ({
            imageId: img.ImageId,
            displayOrder: img.DisplayOrder
        }));
        
        // 比較原始順序和目前順序
        if (currentOrder.length !== this.originalOrder.length) return true;
        
        for (let i = 0; i < currentOrder.length; i++) {
            if (currentOrder[i].imageId !== this.originalOrder[i].imageId ||
                currentOrder[i].displayOrder !== this.originalOrder[i].displayOrder) {
                return true;
            }
        }
        
        return false;
    }
    
    // 標記有未保存的變更
    markUnsavedChanges() {
        this.hasUnsavedChanges = true;
        this.updateButtonStates();
    }
    
    // 保存排序
    async saveOrder() {
        if (!this.hasUnsavedChanges) return;
        
        if (this.isLoading) return;
        this.setLoading(true);
        
        try {
            // 準備排序資料
            const orderData = {};
            this.uploadedImages.forEach((image, index) => {
                orderData[image.ImageId] = index + 1;
            });
            
            const requestData = {
                EntityType: this.getEntityTypeValue(),
                EntityId: this.entityId,
                ImageDisplayOrders: orderData
            };
            
            console.log('保存排序請求資料:', requestData);
            
            const response = await fetch('/api/ImageManagerApi/reorder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.Success) {
                    this.hasUnsavedChanges = false;
                    this.saveOriginalOrder();
                    await this.loadImages();
                    this.showSuccess('圖片順序已保存');
                } else {
                    this.showError(data.Message || '保存排序失敗');
                }
            } else {
                this.showError('保存排序失敗');
            }
        } catch (error) {
            console.error('保存排序錯誤:', error);
            this.showError(`保存排序失敗: ${error.message}`);
        } finally {
            this.setLoading(false);
        }
    }
}

// 自動初始化所有 zh-image-manager 元素
document.addEventListener('DOMContentLoaded', function() {
    const elements = document.querySelectorAll('.zh-image-manager');
    elements.forEach(element => {
        if (!element.zhImageManagerInstance) {
            element.zhImageManagerInstance = new ZhImageManager(element);
        }
    });
});

// 全域暴露類別以供手動初始化
window.ZhImageManager = ZhImageManager;