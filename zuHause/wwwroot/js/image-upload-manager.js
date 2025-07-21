/**
 * 圖片上傳流程管理器 - 處理檔案上傳、進度追蹤和錯誤處理
 * 實作需求 15, 16 和分階段上傳流程
 */
class ImageUploadManager {
    constructor(container, previewManager, config = {}) {
        this.container = container;
        this.previewManager = previewManager;
        this.config = {
            apiMode: 'mvc',
            entityType: '',
            entityId: 0,
            category: 'Gallery',
            uploadEndpoint: '/ImageManager/Upload',
            apiUploadEndpoint: '/api/imagemanager/upload',
            maxConcurrentUploads: 3,
            retryAttempts: 3,
            retryDelay: 1000,
            ...config
        };
        
        // 整合錯誤處理系統
        this.errorHandler = new ImageErrorHandler({
            maxRetries: this.config.retryAttempts,
            baseDelay: this.config.retryDelay
        });
        
        this.recoveryManager = new ImageRecoveryManager(
            container, 
            this.errorHandler, 
            window.ImageModalManager?.getInstance()
        );
        
        // 上傳狀態管理
        this.uploadQueue = new Map();
        this.activeUploads = new Set();
        this.uploadResults = new Map();
        this.totalFiles = 0;
        this.completedFiles = 0;
        this.failedFiles = 0;
        
        // UI 元素
        this.progressContainer = null;
        this.progressBar = null;
        this.statusText = null;
        this.cancelButton = null;
        
        // 狀態控制
        this.isUploading = false;
        this.isCancelled = false;
        this.abortController = null;
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        this.createProgressUI();
        console.log('圖片上傳管理器初始化完成');
    }
    
    setupEventListeners() {
        // 監聽確認上傳事件
        this.container.addEventListener('confirmUpload', this.handleUploadConfirmation.bind(this));
        
        // 監聽檔案變更事件
        this.container.addEventListener('filesChanged', this.handleFilesChanged.bind(this));
        
        // 監聽視窗關閉事件（防止意外關閉）
        window.addEventListener('beforeunload', this.handleBeforeUnload.bind(this));
    }
    
    handleUploadConfirmation(event) {
        if (this.isUploading) {
            this.showMessage('正在上傳中，請稍候...', 'warning');
            return;
        }
        
        const selectedFiles = this.previewManager.getSelectedFiles();
        if (selectedFiles.size === 0) {
            this.showMessage('沒有選擇要上傳的檔案', 'warning');
            return;
        }
        
        this.startUpload(selectedFiles);
    }
    
    handleFilesChanged(event) {
        // 如果正在上傳，警告用戶
        if (this.isUploading) {
            this.showMessage('正在上傳中，檔案變更將不會影響當前上傳', 'info');
        }
    }
    
    async startUpload(selectedFiles) {
        try {
            this.isUploading = true;
            this.isCancelled = false;
            this.abortController = new AbortController();
            
            this.resetUploadState();
            this.prepareUploadQueue(selectedFiles);
            this.showProgressUI();
            
            // 派發上傳開始事件
            this.dispatchUploadEvent('uploadStarted', {
                totalFiles: this.totalFiles
            });
            
            // 需求 16: 點確認上傳才啟動轉換 WebP、存到 Blob Storage、轉存到 Azure SQL 的行為
            const uploadResult = await this.processUploadQueue();
            
            if (!this.isCancelled) {
                await this.handleUploadSuccess(uploadResult);
            }
            
        } catch (error) {
            if (!this.isCancelled) {
                await this.handleUploadError(error);
            }
        } finally {
            this.isUploading = false;
            this.hideProgressUI();
            this.abortController = null;
        }
    }
    
    resetUploadState() {
        this.uploadQueue.clear();
        this.activeUploads.clear();
        this.uploadResults.clear();
        this.totalFiles = 0;
        this.completedFiles = 0;
        this.failedFiles = 0;
    }
    
    prepareUploadQueue(selectedFiles) {
        this.totalFiles = selectedFiles.size;
        let uploadOrder = 1;
        
        selectedFiles.forEach((fileData, fileId) => {
            this.uploadQueue.set(fileId, {
                fileId,
                fileData,
                order: uploadOrder++,
                attempts: 0,
                status: 'pending' // pending, uploading, completed, failed
            });
        });
    }
    
    async processUploadQueue() {
        return new Promise(async (resolve, reject) => {
            try {
                // 建立 FormData
                const formData = this.createFormData();
                
                // 執行批次上傳
                const uploadResult = await this.performBatchUpload(formData);
                
                resolve(uploadResult);
            } catch (error) {
                reject(error);
            }
        });
    }
    
    createFormData() {
        const formData = new FormData();
        
        // 添加基本參數
        formData.append('EntityType', this.config.entityType);
        formData.append('EntityId', this.config.entityId);
        formData.append('Category', this.config.category);
        
        // 添加檔案（按順序）
        const sortedFiles = Array.from(this.uploadQueue.values())
            .sort((a, b) => a.order - b.order);
        
        sortedFiles.forEach((uploadItem, index) => {
            const { fileData } = uploadItem;
            formData.append('Files', fileData.file);
            
            // 添加檔案順序和元數據
            formData.append(`FileOrders[${index}]`, fileData.displayOrder || index + 1);
            formData.append(`FileNames[${index}]`, fileData.fileName);
        });
        
        return formData;
    }
    
    async performBatchUpload(formData) {
        const uploadEndpoint = this.getUploadEndpoint();
        const operationId = `upload_${Date.now()}`;
        
        // 使用錯誤處理器執行帶重試的上傳
        return await this.errorHandler.executeWithRetry(async () => {
            try {
                // 更新進度
                this.updateProgress(0, '準備上傳...');
                
                const response = await fetch(uploadEndpoint, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    signal: this.abortController.signal
                });
                
                this.updateProgress(50, '處理上傳結果...');
                
                if (!response.ok) {
                    const error = new Error(`HTTP ${response.status}: ${response.statusText}`);
                    error.status = response.status;
                    error.url = uploadEndpoint;
                    throw error;
                }
                
                const result = await response.json();
                
                this.updateProgress(100, '上傳完成');
                
                return this.handleUploadResponse(result);
                
            } catch (error) {
                if (error.name === 'AbortError') {
                    const cancelError = new Error('上傳已取消');
                    cancelError.name = 'AbortError';
                    throw cancelError;
                }
                
                // 增強錯誤信息
                error.operationId = operationId;
                error.context = {
                    endpoint: uploadEndpoint,
                    fileCount: this.totalFiles
                };
                throw error;
            }
        }, operationId, {
            operationId,
            endpoint: uploadEndpoint,
            fileCount: this.totalFiles,
            retryOperation: () => this.performBatchUpload(formData)
        });
    }
    
    getUploadEndpoint() {
        return this.config.apiMode === 'mvc' 
            ? this.config.uploadEndpoint
            : this.config.apiUploadEndpoint;
    }
    
    handleUploadResponse(result) {
        // 根據 API 模式處理回應
        if (this.config.apiMode === 'mvc') {
            if (!result.isSuccess) {
                throw new Error(result.message || '上傳失敗');
            }
            return result.data;
        } else {
            if (!result.isSuccess) {
                throw new Error(result.message || '上傳失敗');
            }
            return result.data;
        }
    }
    
    async handleUploadSuccess(uploadResult) {
        this.completedFiles = this.totalFiles;
        this.updateProgress(100, `成功上傳 ${this.totalFiles} 張圖片`);
        
        // 需求 15: 縮圖也一併保存在 Blob Storage
        this.showMessage(
            `成功上傳 ${this.totalFiles} 張圖片，包含縮圖已保存到雲端`, 
            'success'
        );
        
        // 清空預覽管理器中的檔案
        this.previewManager.clearAllFiles();
        
        // 派發上傳成功事件
        this.dispatchUploadEvent('uploadCompleted', {
            totalFiles: this.totalFiles,
            uploadResults: uploadResult
        });
        
        // 可選：重新載入已上傳圖片
        this.dispatchUploadEvent('reloadExistingImages');
    }
    
    async handleUploadError(error) {
        this.failedFiles = this.totalFiles;
        
        // 使用錯誤處理器分析錯誤
        const errorAnalysis = this.errorHandler.analyzeError(error, {
            operationId: error.operationId || 'upload_error',
            totalFiles: this.totalFiles,
            uploadEndpoint: this.getUploadEndpoint()
        });
        
        // 嘗試錯誤恢復
        const recovered = await this.recoveryManager.recoverFromError(errorAnalysis, {
            operationId: error.operationId,
            retryOperation: () => this.startUpload(this.previewManager.getSelectedFiles())
        });
        
        if (!recovered) {
            // 恢復失敗，顯示錯誤信息
            const errorMessage = errorAnalysis.userMessage || '上傳過程中發生未知錯誤';
            this.updateProgress(0, `上傳失敗: ${errorMessage}`);
            this.showMessage(`上傳失敗: ${errorMessage}`, 'error');
            
            // 派發上傳失敗事件
            this.dispatchUploadEvent('uploadFailed', {
                error: errorMessage,
                errorAnalysis: errorAnalysis,
                totalFiles: this.totalFiles
            });
        }
    }
    
    createProgressUI() {
        // 檢查是否已存在進度 UI
        if (this.container.querySelector('.upload-progress')) {
            return;
        }
        
        const progressHTML = `
            <div class="upload-progress d-none" role="dialog" aria-labelledby="upload-title" aria-describedby="upload-status">
                <div class="upload-overlay"></div>
                <div class="upload-modal">
                    <div class="upload-header">
                        <h5 id="upload-title">圖片上傳中</h5>
                        <button type="button" class="btn-close upload-cancel" aria-label="取消上傳"></button>
                    </div>
                    <div class="upload-body">
                        <div class="progress mb-3">
                            <div class="progress-bar progress-bar-striped progress-bar-animated" 
                                 role="progressbar" 
                                 style="width: 0%" 
                                 aria-valuenow="0" 
                                 aria-valuemin="0" 
                                 aria-valuemax="100"></div>
                        </div>
                        <div id="upload-status" class="upload-status text-center" aria-live="polite">
                            準備上傳...
                        </div>
                        <div class="upload-details mt-3">
                            <small class="text-muted">
                                <span class="upload-completed">0</span> / <span class="upload-total">0</span> 檔案已完成
                            </small>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        this.container.insertAdjacentHTML('beforeend', progressHTML);
        
        // 取得元素引用
        this.progressContainer = this.container.querySelector('.upload-progress');
        this.progressBar = this.progressContainer.querySelector('.progress-bar');
        this.statusText = this.progressContainer.querySelector('.upload-status');
        this.cancelButton = this.progressContainer.querySelector('.upload-cancel');
        
        // 綁定取消按鈕事件
        this.cancelButton.addEventListener('click', this.cancelUpload.bind(this));
        
        // 添加 CSS 樣式
        this.addProgressStyles();
    }
    
    addProgressStyles() {
        if (document.getElementById('upload-progress-styles')) {
            return;
        }
        
        const styles = document.createElement('style');
        styles.id = 'upload-progress-styles';
        styles.textContent = `
            .upload-progress {
                position: fixed;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                z-index: 9999;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            
            .upload-overlay {
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: rgba(0, 0, 0, 0.5);
                backdrop-filter: blur(2px);
            }
            
            .upload-modal {
                position: relative;
                background: white;
                border-radius: 8px;
                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                min-width: 400px;
                max-width: 500px;
                z-index: 1;
            }
            
            .upload-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 1rem;
                border-bottom: 1px solid var(--bs-border-color);
            }
            
            .upload-header h5 {
                margin: 0;
                color: var(--bs-primary);
            }
            
            .upload-body {
                padding: 1.5rem;
            }
            
            .upload-cancel {
                background: none;
                border: none;
                font-size: 1.2rem;
                cursor: pointer;
                padding: 0.25rem;
                display: flex;
                align-items: center;
                justify-content: center;
                width: 1.5rem;
                height: 1.5rem;
            }
            
            .upload-cancel:hover {
                background: var(--bs-light);
                border-radius: 50%;
            }
            
            .upload-cancel::before {
                content: "×";
                font-weight: bold;
            }
            
            .upload-status {
                font-weight: 500;
                color: var(--bs-dark);
            }
            
            .upload-details {
                text-align: center;
            }
            
            @media (max-width: 576px) {
                .upload-modal {
                    min-width: 90vw;
                    margin: 1rem;
                }
                
                .upload-body {
                    padding: 1rem;
                }
            }
        `;
        
        document.head.appendChild(styles);
    }
    
    showProgressUI() {
        if (this.progressContainer) {
            this.progressContainer.classList.remove('d-none');
            this.progressContainer.querySelector('.upload-total').textContent = this.totalFiles;
            
            // 設定焦點到對話框
            this.progressContainer.setAttribute('tabindex', '-1');
            this.progressContainer.focus();
        }
    }
    
    hideProgressUI() {
        if (this.progressContainer) {
            setTimeout(() => {
                this.progressContainer.classList.add('d-none');
            }, 2000); // 2秒後隱藏
        }
    }
    
    updateProgress(percentage, status) {
        if (this.progressBar) {
            this.progressBar.style.width = `${percentage}%`;
            this.progressBar.setAttribute('aria-valuenow', percentage);
        }
        
        if (this.statusText) {
            this.statusText.textContent = status;
        }
        
        if (this.progressContainer) {
            this.progressContainer.querySelector('.upload-completed').textContent = this.completedFiles;
        }
    }
    
    cancelUpload() {
        if (this.isUploading && this.abortController) {
            this.isCancelled = true;
            this.abortController.abort();
            this.updateProgress(0, '上傳已取消');
            this.showMessage('上傳已取消', 'info');
        }
    }
    
    handleBeforeUnload(event) {
        if (this.isUploading) {
            event.preventDefault();
            event.returnValue = '正在上傳檔案，離開頁面將中斷上傳過程。確定要離開嗎？';
            return event.returnValue;
        }
    }
    
    showMessage(message, type = 'info') {
        // 整合現有的 Modal 管理器或使用簡單通知
        if (window.ImageModalManager) {
            const modalManager = window.ImageModalManager.getInstance();
            if (modalManager) {
                switch (type) {
                    case 'success':
                        modalManager.showSuccess(message);
                        break;
                    case 'error':
                        modalManager.showError(message);
                        break;
                    case 'warning':
                        modalManager.showWarning(message);
                        break;
                    default:
                        modalManager.showInfo(message);
                }
                return;
            }
        }
        
        // 暫時的通知方式
        console.log(`[${type.toUpperCase()}] ${message}`);
        
        // 建立簡單的通知元素
        this.showSimpleNotification(message, type);
    }
    
    showSimpleNotification(message, type) {
        const notification = document.createElement('div');
        notification.className = `alert alert-${this.getBootstrapAlertClass(type)} alert-dismissible position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        // 自動移除
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 5000);
    }
    
    getBootstrapAlertClass(type) {
        const classMap = {
            'success': 'success',
            'error': 'danger',
            'warning': 'warning',
            'info': 'info'
        };
        return classMap[type] || 'info';
    }
    
    dispatchUploadEvent(eventName, detail = {}) {
        const event = new CustomEvent(eventName, {
            detail: {
                ...detail,
                uploadManager: this
            }
        });
        this.container.dispatchEvent(event);
    }
    
    // 公共 API
    getUploadStatus() {
        return {
            isUploading: this.isUploading,
            totalFiles: this.totalFiles,
            completedFiles: this.completedFiles,
            failedFiles: this.failedFiles,
            progress: this.totalFiles > 0 ? (this.completedFiles / this.totalFiles) * 100 : 0
        };
    }
    
    setConfig(newConfig) {
        this.config = { ...this.config, ...newConfig };
    }
    
    isUploadInProgress() {
        return this.isUploading;
    }
    
    destroy() {
        // 取消正在進行的上傳
        if (this.isUploading && this.abortController) {
            this.abortController.abort();
        }
        
        // 移除事件監聽器
        window.removeEventListener('beforeunload', this.handleBeforeUnload.bind(this));
        
        // 移除 UI 元素
        if (this.progressContainer) {
            this.progressContainer.remove();
        }
        
        // 銷毀錯誤處理系統
        if (this.errorHandler) {
            this.errorHandler.destroy();
        }
        
        if (this.recoveryManager) {
            this.recoveryManager.destroy();
        }
        
        console.log('圖片上傳管理器已銷毀');
    }
}

// 導出供其他模組使用
window.ImageUploadManager = ImageUploadManager;