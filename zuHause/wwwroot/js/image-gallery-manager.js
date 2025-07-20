/**
 * 已上傳圖片管理組件 - 顯示、管理和操作已確認上傳的圖片
 * 實作需求 19：支援指定 entityType、entityId，重新操作添加或刪除圖片
 */
class ImageGalleryManager {
    constructor(container, config = {}) {
        this.container = container;
        this.config = {
            entityType: '',
            entityId: 0,
            category: 'Gallery',
            apiListUrl: '/ImageManager/List',
            apiDeleteUrl: '/ImageManager/Delete',
            apiReorderUrl: '/ImageManager/Reorder',
            enableSorting: true,
            enableDelete: true,
            enableMainImage: true,
            ...config
        };
        
        // 狀態管理
        this.existingImages = new Map();
        this.mainImageId = null;
        this.isLoading = false;
        this.sortableInstance = null;
        
        // UI 元素
        this.galleryContainer = null;
        this.galleryGrid = null;
        this.loadingIndicator = null;
        this.emptyState = null;
        
        // 整合錯誤處理系統
        this.errorHandler = new ImageErrorHandler();
        this.recoveryManager = new ImageRecoveryManager(
            container,
            this.errorHandler,
            window.ImageModalManager?.getInstance()
        );
        
        this.init();
    }
    
    init() {
        this.createGalleryUI();
        this.setupEventListeners();
        this.loadExistingImages();
        console.log('已上傳圖片管理器初始化完成');
    }
    
    // ===== UI 建立 =====
    
    createGalleryUI() {
        // 檢查是否已存在圖片庫容器
        this.galleryContainer = this.container.querySelector('.existing-images-gallery');
        
        if (!this.galleryContainer) {
            const galleryHTML = `
                <div class="existing-images-gallery">
                    <div class="gallery-header">
                        <h6 class="gallery-title">
                            <i class="fas fa-images"></i>
                            已上傳圖片
                            <span class="gallery-count">(0)</span>
                        </h6>
                        <div class="gallery-actions">
                            <button type="button" class="btn btn-sm btn-outline-secondary btn-refresh-gallery" 
                                    title="重新載入圖片">
                                <i class="fas fa-sync-alt"></i>
                            </button>
                        </div>
                    </div>
                    
                    <div class="gallery-content">
                        <!-- 載入指示器 -->
                        <div class="gallery-loading d-none">
                            <div class="d-flex align-items-center justify-content-center p-4">
                                <div class="spinner-border spinner-border-sm me-2" role="status">
                                    <span class="visually-hidden">載入中...</span>
                                </div>
                                <span>載入圖片中...</span>
                            </div>
                        </div>
                        
                        <!-- 空狀態 -->
                        <div class="gallery-empty d-none">
                            <div class="text-center p-4 text-muted">
                                <i class="fas fa-image fa-3x mb-3 opacity-50"></i>
                                <p class="mb-0">尚未上傳任何圖片</p>
                                <small>點擊上方「添加更多圖片」開始上傳</small>
                            </div>
                        </div>
                        
                        <!-- 圖片網格 -->
                        <div class="gallery-grid" role="grid">
                            <!-- 動態生成的圖片項目會插入此處 -->
                        </div>
                    </div>
                </div>
            `;
            
            this.container.insertAdjacentHTML('beforeend', galleryHTML);
            this.galleryContainer = this.container.querySelector('.existing-images-gallery');
        }
        
        // 取得 UI 元素引用
        this.galleryGrid = this.galleryContainer.querySelector('.gallery-grid');
        this.loadingIndicator = this.galleryContainer.querySelector('.gallery-loading');
        this.emptyState = this.galleryContainer.querySelector('.gallery-empty');
        
        // 添加 CSS 樣式
        this.addGalleryStyles();
        
        // 初始化拖拽排序
        if (this.config.enableSorting) {
            this.initSortable();
        }
    }
    
    addGalleryStyles() {
        if (document.getElementById('gallery-manager-styles')) {
            return;
        }
        
        const styles = document.createElement('style');
        styles.id = 'gallery-manager-styles';
        styles.textContent = `
            .existing-images-gallery {
                margin-top: 1.5rem;
                border: 1px solid var(--bs-border-color);
                border-radius: 0.375rem;
                background: var(--bs-body-bg);
            }
            
            .gallery-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 1rem;
                border-bottom: 1px solid var(--bs-border-color);
                background: var(--bs-light);
                border-radius: 0.375rem 0.375rem 0 0;
            }
            
            .gallery-title {
                margin: 0;
                display: flex;
                align-items: center;
                gap: 0.5rem;
                color: var(--bs-primary);
                font-weight: 600;
            }
            
            .gallery-count {
                color: var(--bs-secondary);
                font-weight: normal;
                font-size: 0.875rem;
            }
            
            .gallery-actions {
                display: flex;
                gap: 0.5rem;
            }
            
            .gallery-content {
                padding: 1rem;
            }
            
            .gallery-grid {
                display: grid;
                grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
                gap: 1rem;
                min-height: 100px;
            }
            
            .gallery-item {
                position: relative;
                border: 2px solid var(--bs-border-color);
                border-radius: 0.375rem;
                overflow: hidden;
                transition: all 0.2s ease;
                cursor: pointer;
                background: white;
            }
            
            .gallery-item:hover {
                border-color: var(--bs-primary);
                transform: translateY(-2px);
                box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            }
            
            .gallery-item.main-image {
                border-color: var(--bs-success);
                border-width: 3px;
            }
            
            .gallery-item.main-image::before {
                content: "主圖";
                position: absolute;
                top: 0;
                right: 0;
                background: var(--bs-success);
                color: white;
                padding: 0.25rem 0.5rem;
                font-size: 0.75rem;
                font-weight: 600;
                z-index: 2;
                border-radius: 0 0 0 0.25rem;
            }
            
            .gallery-item-image {
                width: 100%;
                height: 120px;
                object-fit: cover;
                transition: transform 0.2s ease;
            }
            
            .gallery-item:hover .gallery-item-image {
                transform: scale(1.05);
            }
            
            .gallery-item-overlay {
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: rgba(0,0,0,0.5);
                display: flex;
                align-items: center;
                justify-content: center;
                gap: 0.5rem;
                opacity: 0;
                transition: opacity 0.2s ease;
            }
            
            .gallery-item:hover .gallery-item-overlay {
                opacity: 1;
            }
            
            .gallery-item-overlay .btn {
                border: none;
                background: rgba(255,255,255,0.9);
                color: var(--bs-dark);
                padding: 0.375rem;
                border-radius: 50%;
                transition: all 0.2s ease;
            }
            
            .gallery-item-overlay .btn:hover {
                background: white;
                transform: scale(1.1);
            }
            
            .gallery-item-overlay .btn-set-main {
                background: var(--bs-success);
                color: white;
            }
            
            .gallery-item-overlay .btn-delete {
                background: var(--bs-danger);
                color: white;
            }
            
            .gallery-item-info {
                position: absolute;
                bottom: 0;
                left: 0;
                right: 0;
                background: linear-gradient(transparent, rgba(0,0,0,0.7));
                color: white;
                padding: 1rem 0.5rem 0.5rem;
                font-size: 0.75rem;
                opacity: 0;
                transition: opacity 0.2s ease;
            }
            
            .gallery-item:hover .gallery-item-info {
                opacity: 1;
            }
            
            .gallery-item-name {
                font-weight: 600;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
                margin-bottom: 0.25rem;
            }
            
            .gallery-item-meta {
                display: flex;
                justify-content: space-between;
                font-size: 0.6875rem;
                opacity: 0.8;
            }
            
            /* 拖拽狀態 */
            .gallery-item.sortable-chosen {
                transform: rotate(5deg);
                opacity: 0.8;
            }
            
            .gallery-item.sortable-ghost {
                opacity: 0.3;
                background: var(--bs-primary);
            }
            
            /* 載入狀態 */
            .gallery-item-loading {
                position: relative;
                background: var(--bs-light);
                display: flex;
                align-items: center;
                justify-content: center;
                height: 120px;
                color: var(--bs-secondary);
            }
            
            /* 響應式設計 */
            @media (max-width: 576px) {
                .gallery-grid {
                    grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
                    gap: 0.5rem;
                }
                
                .gallery-item-image {
                    height: 100px;
                }
                
                .gallery-header {
                    padding: 0.75rem;
                }
                
                .gallery-content {
                    padding: 0.75rem;
                }
            }
            
            /* 無障礙功能 */
            .gallery-item:focus {
                outline: 2px solid var(--bs-primary);
                outline-offset: 2px;
            }
            
            @media (prefers-reduced-motion: reduce) {
                .gallery-item,
                .gallery-item-image,
                .gallery-item-overlay,
                .gallery-item-info {
                    transition: none;
                }
                
                .gallery-item:hover {
                    transform: none;
                }
                
                .gallery-item:hover .gallery-item-image {
                    transform: none;
                }
            }
        `;
        
        document.head.appendChild(styles);
    }
    
    // ===== 事件監聽 =====
    
    setupEventListeners() {
        // 重新整理按鈕
        const refreshBtn = this.galleryContainer.querySelector('.btn-refresh-gallery');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', this.handleRefreshGallery.bind(this));
        }
        
        // 監聽重新載入事件
        this.container.addEventListener('reloadExistingImages', this.handleReloadRequest.bind(this));
        
        // 監聽上傳完成事件
        this.container.addEventListener('uploadCompleted', this.handleUploadCompleted.bind(this));
        
        // 圖片網格事件委派
        this.galleryGrid.addEventListener('click', this.handleImageAction.bind(this));
        this.galleryGrid.addEventListener('keydown', this.handleKeyboardNavigation.bind(this));
    }
    
    // ===== 載入和顯示 =====
    
    async loadExistingImages() {
        if (this.isLoading) return;
        
        this.isLoading = true;
        this.showLoading(true);
        
        try {
            const images = await this.fetchImagesFromServer();
            this.updateImageDisplay(images);
            
        } catch (error) {
            console.error('載入已上傳圖片失敗:', error);
            await this.handleLoadError(error);
            
        } finally {
            this.isLoading = false;
            this.showLoading(false);
        }
    }
    
    async fetchImagesFromServer() {
        const url = `${this.config.apiListUrl}?entityType=${this.config.entityType}&entityId=${this.config.entityId}&category=${this.config.category}`;
        
        return await this.errorHandler.executeWithRetry(async () => {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (!response.ok) {
                const error = new Error(`HTTP ${response.status}: ${response.statusText}`);
                error.status = response.status;
                error.url = url;
                throw error;
            }
            
            const result = await response.json();
            
            if (!result.isSuccess) {
                throw new Error(result.message || '載入圖片失敗');
            }
            
            return result.data;
            
        }, 'load_existing_images', {
            entityType: this.config.entityType,
            entityId: this.config.entityId,
            category: this.config.category
        });
    }
    
    updateImageDisplay(imageData) {
        // 清空現有顯示
        this.galleryGrid.innerHTML = '';
        this.existingImages.clear();
        
        if (!imageData || !imageData.images || imageData.images.length === 0) {
            this.showEmptyState(true);
            this.updateGalleryCount(0);
            return;
        }
        
        this.showEmptyState(false);
        
        // 更新主圖 ID
        this.mainImageId = imageData.mainImage?.imageId || null;
        
        // 按 displayOrder 排序並顯示圖片
        const sortedImages = imageData.images.sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0));
        
        sortedImages.forEach(image => {
            this.existingImages.set(image.imageId, image);
            this.renderImageItem(image);
        });
        
        this.updateGalleryCount(sortedImages.length);
        
        // 更新拖拽排序
        if (this.sortableInstance) {
            this.sortableInstance.option('disabled', sortedImages.length <= 1);
        }
    }
    
    renderImageItem(image) {
        const isMainImage = image.imageId === this.mainImageId;
        
        const imageHTML = `
            <div class="gallery-item ${isMainImage ? 'main-image' : ''}" 
                 data-image-id="${image.imageId}"
                 data-display-order="${image.displayOrder || 0}"
                 role="gridcell"
                 tabindex="0"
                 aria-label="${image.fileName}${isMainImage ? ' (主圖)' : ''}">
                
                <img class="gallery-item-image" 
                     src="${image.thumbnailUrl || image.mediumUrl}" 
                     alt="${image.fileName}"
                     loading="lazy"
                     onerror="this.src='data:image/svg+xml,<svg xmlns=%22http://www.w3.org/2000/svg%22 width=%22150%22 height=%22120%22><rect width=%22100%%22 height=%22100%%22 fill=%22%23e9ecef%22/><text x=%2250%%22 y=%2250%%22 text-anchor=%22middle%22 dy=%22.3em%22 fill=%22%236c757d%22>無法載入</text></svg>'">
                
                <div class="gallery-item-overlay">
                    ${!isMainImage && this.config.enableMainImage ? `
                        <button type="button" class="btn btn-set-main" 
                                data-action="set-main" 
                                title="設為主圖"
                                aria-label="設 ${image.fileName} 為主圖">
                            <i class="fas fa-star"></i>
                        </button>
                    ` : ''}
                    
                    <button type="button" class="btn btn-view" 
                            data-action="view" 
                            title="檢視大圖"
                            aria-label="檢視 ${image.fileName} 大圖">
                        <i class="fas fa-eye"></i>
                    </button>
                    
                    ${this.config.enableDelete ? `
                        <button type="button" class="btn btn-delete" 
                                data-action="delete" 
                                title="刪除圖片"
                                aria-label="刪除 ${image.fileName}">
                            <i class="fas fa-trash"></i>
                        </button>
                    ` : ''}
                </div>
                
                <div class="gallery-item-info">
                    <div class="gallery-item-name" title="${image.fileName}">
                        ${image.fileName}
                    </div>
                    <div class="gallery-item-meta">
                        <span>${this.formatFileSize(image.fileSize)}</span>
                        <span>${this.formatDate(image.createdAt)}</span>
                    </div>
                </div>
            </div>
        `;
        
        this.galleryGrid.insertAdjacentHTML('beforeend', imageHTML);
    }
    
    // ===== 操作處理 =====
    
    async handleImageAction(event) {
        const actionBtn = event.target.closest('[data-action]');
        if (!actionBtn) return;
        
        const action = actionBtn.dataset.action;
        const imageItem = actionBtn.closest('.gallery-item');
        const imageId = parseInt(imageItem.dataset.imageId);
        const image = this.existingImages.get(imageId);
        
        if (!image) return;
        
        event.preventDefault();
        event.stopPropagation();
        
        switch (action) {
            case 'set-main':
                await this.handleSetMainImage(imageId, image);
                break;
            case 'view':
                this.handleViewImage(image);
                break;
            case 'delete':
                await this.handleDeleteImage(imageId, image);
                break;
        }
    }
    
    async handleSetMainImage(imageId, image) {
        // 設為主圖的邏輯（此處簡化，實際需要後端 API 支援）
        const previousMainId = this.mainImageId;
        this.mainImageId = imageId;
        
        // 更新 UI
        const previousMainItem = this.galleryGrid.querySelector(`[data-image-id="${previousMainId}"]`);
        if (previousMainItem) {
            previousMainItem.classList.remove('main-image');
        }
        
        const newMainItem = this.galleryGrid.querySelector(`[data-image-id="${imageId}"]`);
        if (newMainItem) {
            newMainItem.classList.add('main-image');
        }
        
        // 顯示成功訊息
        if (window.ImageModalManager) {
            window.ImageModalManager.getInstance()?.showSuccess(`已將「${image.fileName}」設為主圖`);
        }
    }
    
    handleViewImage(image) {
        // 顯示大圖檢視（可整合現有的 Modal 系統）
        if (window.ImageModalManager) {
            const modalManager = window.ImageModalManager.getInstance();
            modalManager?.showImageViewer({
                title: image.fileName,
                imageSrc: image.originalUrl || image.largeUrl,
                metadata: {
                    fileName: image.fileName,
                    fileSize: this.formatFileSize(image.fileSize),
                    uploadDate: this.formatDate(image.createdAt),
                    isMainImage: image.imageId === this.mainImageId
                }
            });
        }
    }
    
    async handleDeleteImage(imageId, image) {
        // 確認刪除
        if (window.ImageModalManager) {
            const confirmed = await window.ImageModalManager.getInstance()?.showConfirm({
                title: '確認刪除圖片',
                message: `確定要刪除「${image.fileName}」嗎？此操作無法復原。`,
                confirmText: '刪除',
                confirmClass: 'btn-danger'
            });
            
            if (!confirmed) return;
        } else {
            if (!confirm(`確定要刪除「${image.fileName}」嗎？`)) return;
        }
        
        try {
            await this.deleteImageFromServer(imageId);
            this.removeImageFromDisplay(imageId);
            
            if (window.ImageModalManager) {
                window.ImageModalManager.getInstance()?.showSuccess('圖片刪除成功');
            }
            
        } catch (error) {
            console.error('刪除圖片失敗:', error);
            
            const errorAnalysis = this.errorHandler.analyzeError(error, {
                operationId: 'delete_image',
                imageId: imageId,
                fileName: image.fileName
            });
            
            await this.recoveryManager.recoverFromError(errorAnalysis, {
                operationId: 'delete_image',
                retryOperation: () => this.handleDeleteImage(imageId, image)
            });
        }
    }
    
    async deleteImageFromServer(imageId) {
        const url = `${this.config.apiDeleteUrl}/${imageId}`;
        
        return await this.errorHandler.executeWithRetry(async () => {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (!response.ok) {
                const error = new Error(`HTTP ${response.status}: ${response.statusText}`);
                error.status = response.status;
                error.url = url;
                throw error;
            }
            
            const result = await response.json();
            
            if (!result.isSuccess) {
                throw new Error(result.message || '刪除圖片失敗');
            }
            
            return result.data;
            
        }, 'delete_image', {
            imageId: imageId
        });
    }
    
    removeImageFromDisplay(imageId) {
        const imageItem = this.galleryGrid.querySelector(`[data-image-id="${imageId}"]`);
        if (imageItem) {
            imageItem.remove();
        }
        
        this.existingImages.delete(imageId);
        
        // 如果刪除的是主圖，清除主圖狀態
        if (this.mainImageId === imageId) {
            this.mainImageId = null;
        }
        
        this.updateGalleryCount(this.existingImages.size);
        
        // 顯示空狀態
        if (this.existingImages.size === 0) {
            this.showEmptyState(true);
        }
    }
    
    // ===== 拖拽排序 =====
    
    initSortable() {
        if (typeof Sortable === 'undefined') {
            console.warn('SortableJS 未載入，跳過拖拽排序功能');
            return;
        }
        
        this.sortableInstance = Sortable.create(this.galleryGrid, {
            animation: 200,
            ghostClass: 'sortable-ghost',
            chosenClass: 'sortable-chosen',
            dragClass: 'sortable-drag',
            onEnd: this.handleSortEnd.bind(this)
        });
    }
    
    async handleSortEnd(evt) {
        if (evt.oldIndex === evt.newIndex) return;
        
        try {
            // 重新計算所有圖片的顯示順序
            const newOrder = this.calculateNewDisplayOrder();
            
            // 同步到伺服器
            await this.syncOrderToServer(newOrder);
            
            // 更新本地狀態
            this.updateLocalDisplayOrder(newOrder);
            
        } catch (error) {
            console.error('更新圖片順序失敗:', error);
            
            // 復原排序
            this.loadExistingImages();
            
            if (window.ImageModalManager) {
                window.ImageModalManager.getInstance()?.showError('更新圖片順序失敗，已復原');
            }
        }
    }
    
    calculateNewDisplayOrder() {
        const imageItems = Array.from(this.galleryGrid.querySelectorAll('.gallery-item'));
        const newOrder = {};
        
        imageItems.forEach((item, index) => {
            const imageId = parseInt(item.dataset.imageId);
            newOrder[imageId] = index + 1;
        });
        
        return newOrder;
    }
    
    async syncOrderToServer(newOrder) {
        const url = this.config.apiReorderUrl;
        
        return await this.errorHandler.executeWithRetry(async () => {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    entityType: this.config.entityType,
                    entityId: this.config.entityId,
                    imageDisplayOrders: newOrder
                })
            });
            
            if (!response.ok) {
                const error = new Error(`HTTP ${response.status}: ${response.statusText}`);
                error.status = response.status;
                error.url = url;
                throw error;
            }
            
            const result = await response.json();
            
            if (!result.isSuccess) {
                throw new Error(result.message || '更新圖片順序失敗');
            }
            
            return result.data;
            
        }, 'reorder_images', {
            entityType: this.config.entityType,
            entityId: this.config.entityId,
            orderCount: Object.keys(newOrder).length
        });
    }
    
    updateLocalDisplayOrder(newOrder) {
        Object.entries(newOrder).forEach(([imageId, order]) => {
            const image = this.existingImages.get(parseInt(imageId));
            if (image) {
                image.displayOrder = order;
            }
        });
    }
    
    // ===== 輔助方法 =====
    
    showLoading(show) {
        if (this.loadingIndicator) {
            this.loadingIndicator.classList.toggle('d-none', !show);
        }
    }
    
    showEmptyState(show) {
        if (this.emptyState) {
            this.emptyState.classList.toggle('d-none', !show);
        }
    }
    
    updateGalleryCount(count) {
        const countElement = this.galleryContainer.querySelector('.gallery-count');
        if (countElement) {
            countElement.textContent = `(${count})`;
        }
    }
    
    formatFileSize(bytes) {
        if (!bytes) return '0 B';
        
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        
        return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    }
    
    formatDate(dateString) {
        if (!dateString) return '';
        
        const date = new Date(dateString);
        return date.toLocaleDateString('zh-TW', {
            month: 'numeric',
            day: 'numeric'
        });
    }
    
    // ===== 事件處理 =====
    
    handleRefreshGallery() {
        this.loadExistingImages();
    }
    
    handleReloadRequest() {
        this.loadExistingImages();
    }
    
    handleUploadCompleted() {
        // 上傳完成後重新載入圖片
        setTimeout(() => {
            this.loadExistingImages();
        }, 1000);
    }
    
    async handleLoadError(error) {
        const errorAnalysis = this.errorHandler.analyzeError(error, {
            operationId: 'load_existing_images',
            entityType: this.config.entityType,
            entityId: this.config.entityId
        });
        
        await this.recoveryManager.recoverFromError(errorAnalysis, {
            operationId: 'load_existing_images',
            retryOperation: () => this.loadExistingImages()
        });
    }
    
    handleKeyboardNavigation(event) {
        // 處理鍵盤導航（方向鍵等）
        const focused = event.target.closest('.gallery-item');
        if (!focused) return;
        
        let nextItem = null;
        
        switch (event.key) {
            case 'ArrowRight':
                nextItem = focused.nextElementSibling;
                break;
            case 'ArrowLeft':
                nextItem = focused.previousElementSibling;
                break;
            case 'Enter':
            case ' ':
                event.preventDefault();
                this.handleViewImage(this.existingImages.get(parseInt(focused.dataset.imageId)));
                return;
        }
        
        if (nextItem) {
            event.preventDefault();
            nextItem.focus();
        }
    }
    
    // ===== 公共 API =====
    
    getExistingImages() {
        return Array.from(this.existingImages.values());
    }
    
    getMainImage() {
        return this.mainImageId ? this.existingImages.get(this.mainImageId) : null;
    }
    
    refreshGallery() {
        return this.loadExistingImages();
    }
    
    setConfig(newConfig) {
        this.config = { ...this.config, ...newConfig };
    }
    
    destroy() {
        if (this.sortableInstance) {
            this.sortableInstance.destroy();
        }
        
        if (this.errorHandler) {
            this.errorHandler.destroy();
        }
        
        if (this.recoveryManager) {
            this.recoveryManager.destroy();
        }
        
        console.log('已上傳圖片管理器已銷毀');
    }
}

// 導出供其他模組使用
window.ImageGalleryManager = ImageGalleryManager;