/**
 * 圖片管理器主控制器 - 協調所有子模組的運作
 * 整合 ImagePreviewManager, ImageSortManager, ImageModalManager, ImageButtonManager, ImageUploadManager, ImageGalleryManager
 */
class ImageManagerController {
    constructor(container) {
        this.container = container;
        this.containerId = container.id;
        
        // 子模組實例
        this.previewManager = null;
        this.sortManager = null;
        this.modalManager = null;
        this.buttonManager = null;
        this.uploadManager = null;
        this.galleryManager = null;
        
        // 狀態管理
        this.isInitialized = false;
        this.config = this.extractConfig();
        
        this.init();
    }
    
    init() {
        try {
            this.validateContainer();
            this.initializeModules();
            this.setupEventListeners();
            this.setupGlobalEventHandlers();
            
            this.isInitialized = true;
            console.log(`圖片管理器 ${this.containerId} 初始化完成`);
            
            // 派發初始化完成事件
            this.dispatchEvent('imageManagerReady', {
                containerId: this.containerId,
                config: this.config
            });
            
        } catch (error) {
            console.error('圖片管理器初始化失敗:', error);
            this.handleInitializationError(error);
        }
    }
    
    validateContainer() {
        if (!this.container) {
            throw new Error('Container element is required');
        }
        
        if (!this.container.id) {
            throw new Error('Container must have an ID');
        }
        
        // 檢查必要的 data 屬性
        const requiredAttrs = ['entity-type', 'entity-id', 'max-files', 'max-file-size'];
        const missingAttrs = requiredAttrs.filter(attr => 
            !this.container.dataset[this.camelCase(attr)]
        );
        
        if (missingAttrs.length > 0) {
            console.warn('Missing data attributes:', missingAttrs);
        }
    }
    
    extractConfig() {
        const dataset = this.container.dataset;
        
        return {
            entityType: dataset.entityType || '',
            entityId: dataset.entityId || '',
            category: dataset.category || 'default',
            maxFiles: parseInt(dataset.maxFiles) || 10,
            maxFileSize: parseInt(dataset.maxFileSize) || 5242880, // 5MB
            allowedTypes: dataset.allowedTypes ? 
                dataset.allowedTypes.split(',') : 
                ['image/jpeg', 'image/png', 'image/webp', 'image/gif'],
            allowMultiple: dataset.allowMultiple !== 'false',
            enableDragSort: dataset.enableDragSort !== 'false',
            enableBatchOps: dataset.enableBatchOps !== 'false',
            theme: dataset.theme || 'default',
            apiUploadUrl: dataset.apiUpload || '/api/image/upload',
            apiDeleteUrl: dataset.apiDelete || '/api/image/delete',
            apiReorderUrl: dataset.apiReorder || '/api/image/reorder',
            apiListUrl: dataset.apiList || '/api/image/list'
        };
    }
    
    initializeModules() {
        // 初始化 Modal 管理器 (優先，其他模組可能需要用到)
        this.modalManager = new ImageModalManager(this.container);
        ImageModalManager.setGlobalInstance(this.modalManager);
        
        // 初始化預覽管理器
        this.previewManager = new ImagePreviewManager(this.container);
        
        
        // 初始化排序管理器 (如果啟用)
        if (this.config.enableDragSort) {
            this.sortManager = new ImageSortManager(this.container);
        }
        
        // 初始化按鈕管理器 (Phase 3)
        this.buttonManager = new ImageButtonManager(this.container, this.previewManager, this.modalManager);
        
        // 初始化上傳管理器 (Phase 3)
        this.uploadManager = new ImageUploadManager(this.container, this.previewManager, {
            entityType: this.config.entityType,
            entityId: this.config.entityId,
            category: this.config.category,
            uploadEndpoint: '/ImageManager/Upload',
            apiUploadEndpoint: '/api/imagemanager/upload'
        });
        
        // 初始化圖片庫管理器 (Phase 3)
        this.galleryManager = new ImageGalleryManager(this.container, {
            entityType: this.config.entityType,
            entityId: this.config.entityId,
            category: this.config.category,
            apiListUrl: this.config.apiListUrl || '/ImageManager/List',
            apiDeleteUrl: this.config.apiDeleteUrl || '/ImageManager/Delete',
            apiReorderUrl: this.config.apiReorderUrl || '/ImageManager/Reorder'
        });
        
        console.log('所有子模組初始化完成');
    }
    
    setupEventListeners() {
        // 跨模組事件協調
        this.setupPreviewEvents();
        this.setupSortEvents();
        this.setupModalEvents();
        this.setupUploadEvents();
    }
    
    setupPreviewEvents() {
        // 檔案變更事件
        this.container.addEventListener('filesChanged', (e) => {
            const { files, count } = e.detail;
            
            // 通知其他模組更新狀態
            
            if (this.sortManager) {
                // 排序管理器會自動處理 filesChanged 事件
            }
            
            // 更新上傳按鈕狀態
            this.updateUploadButtonState(count);
        });
    }
    
    
    setupSortEvents() {
        if (!this.sortManager) return;
        
        // 項目重新排序事件
        this.container.addEventListener('itemReordered', (e) => {
            const { fromOrder, toOrder, orderMapping } = e.detail;
            
            // 可以在這裡處理排序後的業務邏輯
            console.log(`項目從第 ${fromOrder} 位移動到第 ${toOrder} 位`);
            
            // 如果需要立即同步到後端，可以在這裡調用 API
            if (this.config.apiReorderUrl) {
                this.syncOrderToServer(orderMapping);
            }
        });
    }
    
    setupModalEvents() {
        // Modal 相關事件監聽
        // 上傳事件由 ImageUploadManager 處理
    }
    
    setupUploadEvents() {
        // 上傳相關事件監聽 - 由 ImageUploadManager 處理
        this.container.addEventListener('uploadCompleted', (e) => {
            // 重新載入已上傳的圖片列表
            this.refreshUploadedImages();
        });
        
        this.container.addEventListener('reloadExistingImages', (e) => {
            this.refreshUploadedImages();
        });
    }
    
    setupGlobalEventHandlers() {
        // 全域鍵盤事件
        document.addEventListener('keydown', (e) => {
            if (!this.container.contains(document.activeElement)) return;
            
        });
        
        // 視窗調整大小事件
        window.addEventListener('resize', this.debounce(() => {
            this.handleResize();
        }, 250));
    }
    
    // 檔案操作處理方法
    handleClearAllFiles() {
        if (!this.previewManager) return;
        
        this.processClearAllFiles();
    }
    
    handleDeleteSingleFile(imageId) {
        if (!imageId) return;
        
        this.processFileDeletion([imageId]);
    }
    
    processFileDeletion(fileIds) {
        if (!this.previewManager) return;
        
        // 從預覽管理器中移除檔案
        fileIds.forEach(fileId => {
            this.previewManager.removeFile(fileId);
        });
        
        // 顯示成功訊息
        if (this.modalManager) {
            const count = fileIds.length;
            this.modalManager.showSuccess(`已刪除 ${count} 張圖片`);
        }
    }
    
    processClearAllFiles() {
        if (!this.previewManager) return;
        
        this.previewManager.clearAllFiles();
        
        // 顯示成功訊息
        if (this.modalManager) {
            this.modalManager.showSuccess('已清空所有圖片');
        }
    }
    
    // 上傳處理已由 ImageUploadManager 負責
    
    async refreshUploadedImages() {
        try {
            const response = await fetch(
                `${this.config.apiListUrl}?entityType=${this.config.entityType}&entityId=${this.config.entityId}`
            );
            
            if (response.ok) {
                const imageData = await response.json();
                this.updateUploadedImagesDisplay(imageData);
            }
        } catch (error) {
            console.error('重新載入圖片清單失敗:', error);
        }
    }
    
    updateUploadedImagesDisplay(imageData) {
        // 更新已上傳圖片的顯示
        const existingSection = this.container.querySelector('.existing-images-section');
        if (existingSection && imageData.images) {
            // 這裡可以動態更新已上傳圖片的顯示
            // 具體實作取決於後端回傳的資料格式
        }
    }
    
    // 排序同步方法
    async syncOrderToServer(orderMapping) {
        try {
            const orderData = Array.from(orderMapping.entries()).map(([imageId, order]) => ({
                imageId,
                displayOrder: order
            }));
            
            const response = await fetch(this.config.apiReorderUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    entityType: this.config.entityType,
                    entityId: this.config.entityId,
                    orders: orderData
                })
            });
            
            if (!response.ok) {
                throw new Error(`排序同步失敗: ${response.statusText}`);
            }
            
            console.log('排序已同步到伺服器');
            
        } catch (error) {
            console.error('排序同步失敗:', error);
            
            if (this.modalManager) {
                this.modalManager.showError('排序同步失敗，請稍後再試');
            }
        }
    }
    
    // UI 狀態更新方法
    updateUploadButtonState(fileCount) {
        const uploadBtn = this.container.querySelector('.btn-upload-confirm');
        if (uploadBtn) {
            uploadBtn.disabled = fileCount === 0;
            
            const countSpan = uploadBtn.querySelector('.pending-count');
            if (countSpan) {
                countSpan.textContent = fileCount;
            }
        }
    }
    
    
    handleResize() {
        // 處理視窗調整大小時的重新佈局
        // 可以在這裡調整網格佈局或其他響應式元素
    }
    
    handleInitializationError(error) {
        console.error('圖片管理器初始化失敗:', error);
        
        // 顯示錯誤訊息給用戶
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger';
        errorDiv.innerHTML = `
            <h6>圖片管理器載入失敗</h6>
            <p>錯誤原因：${error.message}</p>
            <small>請重新整理頁面或聯繫系統管理員</small>
        `;
        
        this.container.innerHTML = '';
        this.container.appendChild(errorDiv);
    }
    
    // 輔助方法
    camelCase(str) {
        return str.replace(/-([a-z])/g, (g) => g[1].toUpperCase());
    }
    
    debounce(func, wait) {
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
    
    dispatchEvent(eventName, detail) {
        const event = new CustomEvent(eventName, {
            detail: detail,
            bubbles: true,
            cancelable: true
        });
        this.container.dispatchEvent(event);
    }
    
    // 公共 API
    getConfig() {
        return { ...this.config };
    }
    
    getPreviewManager() {
        return this.previewManager;
    }
    
    
    getSortManager() {
        return this.sortManager;
    }
    
    getModalManager() {
        return this.modalManager;
    }
    
    getUploadManager() {
        return this.uploadManager;
    }
    
    getGalleryManager() {
        return this.galleryManager;
    }
    
    isReady() {
        return this.isInitialized;
    }
    
    destroy() {
        // 清理事件監聽器和資源
        if (this.previewManager) {
            // 清理預覽管理器
        }
        
        
        if (this.sortManager) {
            // 清理排序管理器
        }
        
        if (this.modalManager) {
            // 清理 Modal 管理器
        }
        
        if (this.uploadManager) {
            this.uploadManager.destroy();
        }
        
        if (this.galleryManager) {
            this.galleryManager.destroy();
        }
        
        console.log(`圖片管理器 ${this.containerId} 已銷毀`);
    }
}

// 全域初始化函數
window.ImageManager = {
    instances: new Map(),
    
    init: function(container) {
        if (!container) {
            console.error('Container is required for ImageManager.init()');
            return null;
        }
        
        const containerId = container.id || `image-manager-${Date.now()}`;
        
        if (this.instances.has(containerId)) {
            console.warn(`ImageManager already initialized for container: ${containerId}`);
            return this.instances.get(containerId);
        }
        
        const instance = new ImageManagerController(container);
        this.instances.set(containerId, instance);
        
        return instance;
    },
    
    getInstance: function(containerId) {
        return this.instances.get(containerId);
    },
    
    destroy: function(containerId) {
        const instance = this.instances.get(containerId);
        if (instance) {
            instance.destroy();
            this.instances.delete(containerId);
        }
    },
    
    destroyAll: function() {
        this.instances.forEach((instance, containerId) => {
            instance.destroy();
        });
        this.instances.clear();
    }
};

// 導出主控制器類別
window.ImageManagerController = ImageManagerController;