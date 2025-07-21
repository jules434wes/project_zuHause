/**
 * 圖片管理器錯誤恢復和用戶引導系統
 * 提供自動恢復、手動恢復引導和降級模式
 */
class ImageRecoveryManager {
    constructor(container, errorHandler, modalManager) {
        this.container = container;
        this.errorHandler = errorHandler;
        this.modalManager = modalManager;
        
        // 恢復策略配置
        this.recoveryStrategies = new Map([
            ['NetworkError', this.handleNetworkErrorRecovery.bind(this)],
            ['FileError', this.handleFileErrorRecovery.bind(this)],
            ['ValidationError', this.handleValidationErrorRecovery.bind(this)],
            ['ServerError', this.handleServerErrorRecovery.bind(this)],
            ['SystemError', this.handleSystemErrorRecovery.bind(this)]
        ]);
        
        // 恢復狀態
        this.recoveryState = {
            isRecovering: false,
            lastRecoveryAttempt: null,
            failedOperations: new Map(),
            degradedMode: false
        };
        
        // 網路狀態監控
        this.networkMonitor = {
            isOnline: navigator.onLine,
            lastOnlineCheck: Date.now(),
            reconnectAttempts: 0
        };
        
        this.init();
    }
    
    init() {
        this.setupNetworkMonitoring();
        this.setupAutoRecovery();
        console.log('圖片恢復管理器初始化完成');
    }
    
    // ===== 主要恢復入口 =====
    
    /**
     * 執行錯誤恢復
     * @param {Object} errorAnalysis - 錯誤分析結果
     * @param {Object} context - 操作上下文
     * @returns {Promise<boolean>} 是否成功恢復
     */
    async recoverFromError(errorAnalysis, context = {}) {
        if (this.recoveryState.isRecovering) {
            console.log('恢復進行中，跳過重複恢復請求');
            return false;
        }
        
        this.recoveryState.isRecovering = true;
        this.recoveryState.lastRecoveryAttempt = Date.now();
        
        try {
            // 記錄失敗操作
            this.recordFailedOperation(errorAnalysis, context);
            
            // 選擇恢復策略
            const strategy = this.recoveryStrategies.get(errorAnalysis.type);
            if (!strategy) {
                console.warn(`未找到 ${errorAnalysis.type} 的恢復策略`);
                return false;
            }
            
            // 執行恢復
            const recovered = await strategy(errorAnalysis, context);
            
            if (recovered) {
                this.clearFailedOperation(context.operationId);
                this.showRecoverySuccess(errorAnalysis.type);
            } else {
                await this.showRecoveryGuidance(errorAnalysis, context);
            }
            
            return recovered;
            
        } catch (recoveryError) {
            console.error('恢復過程中發生錯誤:', recoveryError);
            await this.showRecoveryGuidance(errorAnalysis, context);
            return false;
        } finally {
            this.recoveryState.isRecovering = false;
        }
    }
    
    // ===== 具體恢復策略 =====
    
    /**
     * 網路錯誤恢復
     */
    async handleNetworkErrorRecovery(errorAnalysis, context) {
        console.log('開始網路錯誤恢復');
        
        // 檢查網路連線
        const isOnline = await this.checkNetworkConnection();
        if (!isOnline) {
            console.log('網路仍然不可用，等待網路恢復');
            return await this.waitForNetworkRecovery();
        }
        
        // 網路已恢復，嘗試重新執行原操作
        if (context.retryOperation) {
            try {
                await context.retryOperation();
                return true;
            } catch (retryError) {
                console.log('重新執行操作失敗:', retryError);
                return false;
            }
        }
        
        return true;
    }
    
    /**
     * 檔案錯誤恢復
     */
    async handleFileErrorRecovery(errorAnalysis, context) {
        console.log('開始檔案錯誤恢復');
        
        // 檔案錯誤通常需要用戶干預
        const recoveryAction = await this.showFileErrorDialog(errorAnalysis, context);
        
        switch (recoveryAction) {
            case 'reselect':
                this.triggerFileReselection();
                return true;
            case 'skip':
                this.skipCurrentFile(context);
                return true;
            case 'retry':
                if (context.retryOperation) {
                    try {
                        await context.retryOperation();
                        return true;
                    } catch (error) {
                        return false;
                    }
                }
                return false;
            default:
                return false;
        }
    }
    
    /**
     * 驗證錯誤恢復
     */
    async handleValidationErrorRecovery(errorAnalysis, context) {
        console.log('開始驗證錯誤恢復');
        
        // 對於驗證錯誤，主要是引導用戶修正輸入
        await this.showValidationErrorGuidance(errorAnalysis, context);
        return false; // 需要用戶手動修正
    }
    
    /**
     * 服務器錯誤恢復
     */
    async handleServerErrorRecovery(errorAnalysis, context) {
        console.log('開始服務器錯誤恢復');
        
        // 503 服務不可用，等待服務恢復
        if (errorAnalysis.technicalDetails.status === 503) {
            return await this.waitForServiceRecovery(context);
        }
        
        // 其他服務器錯誤，進入降級模式
        this.enterDegradedMode();
        return false;
    }
    
    /**
     * 系統錯誤恢復
     */
    async handleSystemErrorRecovery(errorAnalysis, context) {
        console.log('開始系統錯誤恢復');
        
        // 嘗試清除暫存資料和重置狀態
        this.clearCachedData();
        
        // 如果是關鍵系統錯誤，進入降級模式
        if (errorAnalysis.severity === 'high') {
            this.enterDegradedMode();
        }
        
        return false;
    }
    
    // ===== 網路狀態監控 =====
    
    setupNetworkMonitoring() {
        // 監聽網路狀態變化
        window.addEventListener('online', this.handleNetworkOnline.bind(this));
        window.addEventListener('offline', this.handleNetworkOffline.bind(this));
        
        // 定期檢查網路狀態
        setInterval(this.checkNetworkStatus.bind(this), 30000); // 30秒檢查一次
    }
    
    handleNetworkOnline() {
        console.log('網路連線恢復');
        this.networkMonitor.isOnline = true;
        this.networkMonitor.reconnectAttempts = 0;
        
        // 嘗試恢復失敗的網路操作
        this.retryNetworkOperations();
    }
    
    handleNetworkOffline() {
        console.log('網路連線中斷');
        this.networkMonitor.isOnline = false;
        this.showNetworkOfflineNotice();
    }
    
    async checkNetworkConnection() {
        try {
            // 嘗試發送小的請求檢查連線
            const response = await fetch('/ping', { 
                method: 'HEAD',
                timeout: 5000 
            });
            this.networkMonitor.isOnline = response.ok;
            return this.networkMonitor.isOnline;
        } catch (error) {
            this.networkMonitor.isOnline = false;
            return false;
        }
    }
    
    async waitForNetworkRecovery(maxWaitTime = 30000) {
        const startTime = Date.now();
        
        while (Date.now() - startTime < maxWaitTime) {
            if (await this.checkNetworkConnection()) {
                return true;
            }
            await new Promise(resolve => setTimeout(resolve, 2000));
        }
        
        return false;
    }
    
    // ===== 服務恢復監控 =====
    
    async waitForServiceRecovery(context, maxWaitTime = 60000) {
        const startTime = Date.now();
        const checkInterval = 5000; // 5秒檢查一次
        
        while (Date.now() - startTime < maxWaitTime) {
            try {
                // 嘗試發送健康檢查請求
                const response = await fetch('/health', { 
                    method: 'HEAD',
                    timeout: 3000 
                });
                
                if (response.ok) {
                    console.log('服務已恢復');
                    return true;
                }
            } catch (error) {
                // 服務仍不可用
            }
            
            await new Promise(resolve => setTimeout(resolve, checkInterval));
        }
        
        console.log('服務恢復超時');
        return false;
    }
    
    // ===== 用戶引導對話框 =====
    
    async showRecoveryGuidance(errorAnalysis, context) {
        if (!this.modalManager) {
            console.warn('無法顯示恢復引導：Modal 管理器未可用');
            return;
        }
        
        const modalContent = this.buildRecoveryGuidanceModal(errorAnalysis, context);
        
        return new Promise((resolve) => {
            this.modalManager.showCustomModal({
                title: '操作失敗 - 恢復建議',
                content: modalContent,
                size: 'medium',
                backdrop: 'static',
                onAction: (action) => {
                    this.handleRecoveryAction(action, context);
                    resolve(action);
                }
            });
        });
    }
    
    buildRecoveryGuidanceModal(errorAnalysis, context) {
        return `
            <div class="recovery-guidance">
                <div class="error-summary">
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle"></i>
                        <strong>${errorAnalysis.userMessage}</strong>
                    </div>
                </div>
                
                <div class="recovery-actions">
                    <h6>建議的解決方案：</h6>
                    <ul class="recovery-action-list">
                        ${errorAnalysis.recoveryActions.map((action, index) => 
                            `<li>
                                <button class="btn btn-outline-primary btn-sm recovery-action-btn" 
                                        data-action="${index}">
                                    ${action}
                                </button>
                            </li>`
                        ).join('')}
                    </ul>
                </div>
                
                ${errorAnalysis.retryable ? `
                    <div class="retry-option">
                        <button class="btn btn-primary retry-btn" data-action="retry">
                            <i class="fas fa-redo"></i> 重試操作
                        </button>
                    </div>
                ` : ''}
                
                <div class="modal-actions">
                    <button class="btn btn-secondary" data-action="cancel">取消</button>
                    <button class="btn btn-outline-danger" data-action="skip">跳過此操作</button>
                </div>
            </div>
        `;
    }
    
    async showFileErrorDialog(errorAnalysis, context) {
        if (!this.modalManager) return 'cancel';
        
        return new Promise((resolve) => {
            const modalContent = `
                <div class="file-error-dialog">
                    <div class="alert alert-warning">
                        <i class="fas fa-file-exclamation"></i>
                        檔案處理失敗：${errorAnalysis.userMessage}
                    </div>
                    
                    <div class="file-error-actions">
                        <button class="btn btn-primary" data-action="reselect">
                            <i class="fas fa-folder-open"></i> 重新選擇檔案
                        </button>
                        <button class="btn btn-warning" data-action="skip">
                            <i class="fas fa-skip-forward"></i> 跳過此檔案
                        </button>
                        <button class="btn btn-outline-secondary" data-action="retry">
                            <i class="fas fa-redo"></i> 重試此檔案
                        </button>
                    </div>
                </div>
            `;
            
            this.modalManager.showCustomModal({
                title: '檔案錯誤處理',
                content: modalContent,
                onAction: resolve
            });
        });
    }
    
    // ===== 降級模式 =====
    
    enterDegradedMode() {
        if (this.recoveryState.degradedMode) return;
        
        console.log('進入降級模式');
        this.recoveryState.degradedMode = true;
        
        // 停用進階功能
        this.disableAdvancedFeatures();
        
        // 顯示降級模式通知
        this.showDegradedModeNotice();
    }
    
    exitDegradedMode() {
        if (!this.recoveryState.degradedMode) return;
        
        console.log('退出降級模式');
        this.recoveryState.degradedMode = false;
        
        // 重新啟用功能
        this.enableAdvancedFeatures();
        
        this.modalManager?.showSuccess('系統功能已恢復正常');
    }
    
    disableAdvancedFeatures() {
        // 停用拖拽排序
        const sortableElements = this.container.querySelectorAll('.sortable-enabled');
        sortableElements.forEach(el => el.classList.add('sortable-disabled'));
        
        // 停用批量操作
        const batchButtons = this.container.querySelectorAll('.batch-operation-btn');
        batchButtons.forEach(btn => {
            btn.disabled = true;
            btn.title = '降級模式下暫時不可用';
        });
    }
    
    enableAdvancedFeatures() {
        // 重新啟用拖拽排序
        const sortableElements = this.container.querySelectorAll('.sortable-disabled');
        sortableElements.forEach(el => el.classList.remove('sortable-disabled'));
        
        // 重新啟用批量操作
        const batchButtons = this.container.querySelectorAll('.batch-operation-btn');
        batchButtons.forEach(btn => {
            btn.disabled = false;
            btn.title = '';
        });
    }
    
    // ===== 輔助方法 =====
    
    recordFailedOperation(errorAnalysis, context) {
        const operationId = context.operationId || 'unknown';
        this.recoveryState.failedOperations.set(operationId, {
            errorAnalysis,
            context,
            timestamp: Date.now()
        });
    }
    
    clearFailedOperation(operationId) {
        this.recoveryState.failedOperations.delete(operationId);
    }
    
    triggerFileReselection() {
        const fileInput = this.container.querySelector('.file-input');
        if (fileInput) {
            fileInput.click();
        }
    }
    
    clearCachedData() {
        // 清除本地暫存
        if (window.localStorage) {
            const keysToRemove = [];
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key?.startsWith('imageManager_')) {
                    keysToRemove.push(key);
                }
            }
            keysToRemove.forEach(key => localStorage.removeItem(key));
        }
        
        console.log('暫存資料已清除');
    }
    
    showRecoverySuccess(errorType) {
        this.modalManager?.showSuccess(`${errorType} 已自動恢復`);
    }
    
    showNetworkOfflineNotice() {
        this.modalManager?.showWarning('網路連線中斷，部分功能可能不可用');
    }
    
    showDegradedModeNotice() {
        this.modalManager?.showWarning('系統進入降級模式，部分進階功能暫時停用');
    }
    
    // ===== 公共 API =====
    
    getRecoveryState() {
        return { ...this.recoveryState };
    }
    
    getFailedOperations() {
        return Array.from(this.recoveryState.failedOperations.entries());
    }
    
    isDegradedMode() {
        return this.recoveryState.degradedMode;
    }
    
    destroy() {
        window.removeEventListener('online', this.handleNetworkOnline.bind(this));
        window.removeEventListener('offline', this.handleNetworkOffline.bind(this));
        console.log('圖片恢復管理器已銷毀');
    }
}

// 導出供其他模組使用
window.ImageRecoveryManager = ImageRecoveryManager;