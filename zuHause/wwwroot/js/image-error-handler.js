/**
 * 圖片管理器進階錯誤處理和監控系統
 * 提供錯誤分類、自動重試、恢復策略和監控功能
 */
class ImageErrorHandler {
    constructor(config = {}) {
        this.config = {
            maxRetries: 3,
            baseDelay: 1000, // 1秒
            maxDelay: 30000, // 30秒
            enableLogging: true,
            enableRecovery: true,
            ...config
        };
        
        // 錯誤統計
        this.errorStats = {
            networkErrors: 0,
            fileErrors: 0,
            serverErrors: 0,
            validationErrors: 0,
            recoveredErrors: 0,
            totalErrors: 0
        };
        
        // 重試記錄
        this.retryHistory = new Map();
        
        // 性能監控
        this.performanceMetrics = {
            operationTimes: [],
            uploadSpeeds: [],
            lastOperationTime: null
        };
        
        this.init();
    }
    
    init() {
        this.setupPerformanceMonitoring();
        console.log('圖片錯誤處理器初始化完成');
    }
    
    // ===== 錯誤分類系統 =====
    
    /**
     * 分析錯誤類型和嚴重程度
     * @param {Error|Object} error - 錯誤對象
     * @param {Object} context - 錯誤上下文
     * @returns {Object} 錯誤分析結果
     */
    analyzeError(error, context = {}) {
        const analysis = {
            type: this.classifyErrorType(error),
            severity: this.determineSeverity(error),
            recoverable: this.isRecoverable(error),
            retryable: this.isRetryable(error),
            userMessage: this.generateUserMessage(error),
            technicalDetails: this.extractTechnicalDetails(error),
            recoveryActions: this.suggestRecoveryActions(error),
            timestamp: new Date().toISOString(),
            context: context
        };
        
        this.updateErrorStats(analysis.type);
        this.logError(analysis);
        
        return analysis;
    }
    
    /**
     * 錯誤類型分類
     */
    classifyErrorType(error) {
        // 網路錯誤
        if (error.name === 'NetworkError' || 
            error.code === 'NETWORK_ERROR' ||
            error.message?.includes('fetch') ||
            error.message?.includes('network')) {
            return 'NetworkError';
        }
        
        // HTTP 錯誤
        if (error.status) {
            if (error.status >= 400 && error.status < 500) {
                return 'ValidationError';
            }
            if (error.status >= 500) {
                return 'ServerError';
            }
        }
        
        // 檔案相關錯誤
        if (error.name === 'FileError' ||
            error.code === 'FILE_TOO_LARGE' ||
            error.code === 'INVALID_FILE_TYPE' ||
            error.message?.includes('file')) {
            return 'FileError';
        }
        
        // AbortError (取消操作)
        if (error.name === 'AbortError') {
            return 'OperationCancelled';
        }
        
        // 預設為系統錯誤
        return 'SystemError';
    }
    
    /**
     * 判定錯誤嚴重程度
     */
    determineSeverity(error) {
        const type = this.classifyErrorType(error);
        
        switch (type) {
            case 'NetworkError':
                return 'medium';
            case 'FileError':
                return 'low';
            case 'ValidationError':
                return 'medium';
            case 'ServerError':
                return 'high';
            case 'OperationCancelled':
                return 'low';
            default:
                return 'high';
        }
    }
    
    /**
     * 判定錯誤是否可恢復
     */
    isRecoverable(error) {
        const type = this.classifyErrorType(error);
        
        switch (type) {
            case 'NetworkError':
                return true; // 網路錯誤通常可恢復
            case 'FileError':
                return true; // 可重新選擇檔案
            case 'ValidationError':
                return error.status !== 401; // 除了認證錯誤
            case 'ServerError':
                return error.status === 503; // 服務暫時不可用
            case 'OperationCancelled':
                return true;
            default:
                return false;
        }
    }
    
    /**
     * 判定錯誤是否適合自動重試
     */
    isRetryable(error) {
        const type = this.classifyErrorType(error);
        
        switch (type) {
            case 'NetworkError':
                return true;
            case 'ServerError':
                return error.status === 503 || error.status === 502;
            case 'ValidationError':
                return false; // 驗證錯誤不應重試
            case 'FileError':
                return false; // 檔案錯誤需要用戶干預
            default:
                return false;
        }
    }
    
    // ===== 用戶友好訊息生成 =====
    
    generateUserMessage(error) {
        const type = this.classifyErrorType(error);
        
        const messages = {
            'NetworkError': '網路連線中斷，請檢查您的網路連線',
            'FileError': '檔案格式或大小不符合要求',
            'ValidationError': '資料驗證失敗，請檢查輸入內容',
            'ServerError': '伺服器暫時無法處理請求，請稍後再試',
            'OperationCancelled': '操作已取消',
            'SystemError': '系統發生未預期的錯誤'
        };
        
        return messages[type] || messages['SystemError'];
    }
    
    extractTechnicalDetails(error) {
        return {
            name: error.name,
            message: error.message,
            code: error.code,
            status: error.status,
            stack: error.stack?.substring(0, 500), // 限制堆疊長度
            url: error.url,
            timestamp: Date.now()
        };
    }
    
    suggestRecoveryActions(error) {
        const type = this.classifyErrorType(error);
        
        const actions = {
            'NetworkError': [
                '檢查網路連線',
                '重新整理頁面',
                '稍後再試'
            ],
            'FileError': [
                '選擇其他檔案',
                '檢查檔案大小和格式',
                '壓縮圖片後重試'
            ],
            'ValidationError': [
                '檢查輸入資料',
                '確認檔案完整性',
                '聯繫系統管理員'
            ],
            'ServerError': [
                '稍後再試',
                '重新整理頁面',
                '聯繫技術支援'
            ],
            'OperationCancelled': [
                '重新開始操作'
            ],
            'SystemError': [
                '重新整理頁面',
                '清除瀏覽器快取',
                '聯繫技術支援'
            ]
        };
        
        return actions[type] || actions['SystemError'];
    }
    
    // ===== 自動重試機制 =====
    
    /**
     * 執行帶重試的操作
     * @param {Function} operation - 要執行的操作
     * @param {string} operationId - 操作識別符
     * @param {Object} context - 操作上下文
     * @returns {Promise} 操作結果
     */
    async executeWithRetry(operation, operationId, context = {}) {
        const startTime = Date.now();
        let lastError = null;
        
        for (let attempt = 0; attempt <= this.config.maxRetries; attempt++) {
            try {
                // 記錄重試歷史
                this.recordRetryAttempt(operationId, attempt);
                
                // 執行操作
                const result = await operation();
                
                // 成功，記錄性能指標
                this.recordOperationTime(Date.now() - startTime);
                
                if (attempt > 0) {
                    this.errorStats.recoveredErrors++;
                    console.log(`操作 ${operationId} 在第 ${attempt + 1} 次嘗試後成功`);
                }
                
                return result;
                
            } catch (error) {
                lastError = error;
                const analysis = this.analyzeError(error, { 
                    ...context, 
                    attempt: attempt + 1,
                    operationId 
                });
                
                // 不可重試的錯誤直接拋出
                if (!analysis.retryable || attempt === this.config.maxRetries) {
                    throw this.enhanceError(error, analysis);
                }
                
                // 計算延遲時間（指數退避）
                const delay = this.calculateBackoffDelay(attempt);
                console.log(`操作 ${operationId} 第 ${attempt + 1} 次失敗，${delay}ms 後重試:`, error.message);
                
                await this.delay(delay);
            }
        }
        
        throw this.enhanceError(lastError, this.analyzeError(lastError, context));
    }
    
    /**
     * 計算指數退避延遲
     */
    calculateBackoffDelay(attempt) {
        const exponentialDelay = this.config.baseDelay * Math.pow(2, attempt);
        const jitter = Math.random() * 0.1 * exponentialDelay; // 加入隨機性
        return Math.min(exponentialDelay + jitter, this.config.maxDelay);
    }
    
    /**
     * 延遲執行
     */
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    
    /**
     * 記錄重試嘗試
     */
    recordRetryAttempt(operationId, attempt) {
        if (!this.retryHistory.has(operationId)) {
            this.retryHistory.set(operationId, []);
        }
        
        this.retryHistory.get(operationId).push({
            attempt,
            timestamp: Date.now()
        });
        
        // 清理舊記錄（保留最近 100 個操作）
        if (this.retryHistory.size > 100) {
            const oldestKey = this.retryHistory.keys().next().value;
            this.retryHistory.delete(oldestKey);
        }
    }
    
    /**
     * 增強錯誤對象
     */
    enhanceError(originalError, analysis) {
        const enhancedError = new Error(analysis.userMessage);
        enhancedError.originalError = originalError;
        enhancedError.analysis = analysis;
        enhancedError.isEnhanced = true;
        
        return enhancedError;
    }
    
    // ===== 性能監控 =====
    
    setupPerformanceMonitoring() {
        // 記錄操作開始時間
        this.startOperationTime = (operationId) => {
            this.performanceMetrics.lastOperationTime = {
                id: operationId,
                startTime: Date.now()
            };
        };
        
        // 記錄操作結束時間
        this.endOperationTime = (operationId) => {
            if (this.performanceMetrics.lastOperationTime?.id === operationId) {
                const duration = Date.now() - this.performanceMetrics.lastOperationTime.startTime;
                this.recordOperationTime(duration);
                this.performanceMetrics.lastOperationTime = null;
            }
        };
    }
    
    recordOperationTime(duration) {
        this.performanceMetrics.operationTimes.push({
            duration,
            timestamp: Date.now()
        });
        
        // 保留最近 50 個記錄
        if (this.performanceMetrics.operationTimes.length > 50) {
            this.performanceMetrics.operationTimes.shift();
        }
    }
    
    // ===== 統計和日誌 =====
    
    updateErrorStats(errorType) {
        this.errorStats[errorType.toLowerCase() + 's'] = 
            (this.errorStats[errorType.toLowerCase() + 's'] || 0) + 1;
        this.errorStats.totalErrors++;
    }
    
    logError(analysis) {
        if (!this.config.enableLogging) return;
        
        console.group(`🚨 錯誤記錄 [${analysis.type}]`);
        console.log('用戶訊息:', analysis.userMessage);
        console.log('技術詳情:', analysis.technicalDetails);
        console.log('可恢復:', analysis.recoverable);
        console.log('可重試:', analysis.retryable);
        console.log('建議操作:', analysis.recoveryActions);
        console.groupEnd();
    }
    
    // ===== 公共 API =====
    
    getErrorStats() {
        return { ...this.errorStats };
    }
    
    getPerformanceMetrics() {
        const times = this.performanceMetrics.operationTimes;
        if (times.length === 0) return null;
        
        const durations = times.map(t => t.duration);
        return {
            averageTime: durations.reduce((a, b) => a + b, 0) / durations.length,
            minTime: Math.min(...durations),
            maxTime: Math.max(...durations),
            recentOperations: times.length,
            lastUpdate: times[times.length - 1]?.timestamp
        };
    }
    
    clearHistory() {
        this.retryHistory.clear();
        this.performanceMetrics.operationTimes = [];
        console.log('錯誤處理歷史已清除');
    }
    
    destroy() {
        this.clearHistory();
        console.log('圖片錯誤處理器已銷毀');
    }
}

// 導出供其他模組使用
window.ImageErrorHandler = ImageErrorHandler;