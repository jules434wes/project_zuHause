/**
 * 房源建立表單暫存管理器
 * 提供自動暫存、載入、清除功能，支援 SessionStorage
 */
class PropertyFormCache {
    constructor(formSelector) {
        this.form = document.querySelector(formSelector);
        this.cacheKey = 'zuHause_property_creation_form';
        this.autoSaveDelay = 1000; // 1秒後自動暫存
        this.autoSaveTimer = null;
        
        if (!this.form) {
            console.error('PropertyFormCache: 找不到表單元素', formSelector);
            return;
        }
        
        this.init();
    }
    
    /**
     * 初始化暫存管理器
     */
    init() {
        this.setupEventListeners();
        this.loadCachedData();
        console.log('PropertyFormCache: 表單暫存管理器初始化完成');
    }
    
    /**
     * 設定事件監聽器
     */
    setupEventListeners() {
        // 監聽所有表單欄位變更
        const formElements = this.form.querySelectorAll('input, select, textarea');
        
        formElements.forEach(element => {
            // 根據元素類型設定合適的事件
            const eventTypes = this.getEventTypesForElement(element);
            
            eventTypes.forEach(eventType => {
                element.addEventListener(eventType, () => {
                    this.scheduleAutoSave();
                });
            });
        });
        
        // 監聽表單提交，成功後清除暫存
        this.form.addEventListener('submit', () => {
            this.scheduleFormSubmitClear();
        });
        
        // 頁面離開前暫存
        window.addEventListener('beforeunload', () => {
            this.saveFormData();
        });
    }
    
    /**
     * 根據元素類型決定監聽的事件
     */
    getEventTypesForElement(element) {
        const tagName = element.tagName.toLowerCase();
        const inputType = element.type ? element.type.toLowerCase() : '';
        
        switch (tagName) {
            case 'input':
                if (inputType === 'text' || inputType === 'number' || inputType === 'email' || inputType === 'url') {
                    return ['input', 'blur'];
                } else if (inputType === 'checkbox' || inputType === 'radio') {
                    return ['change'];
                } else if (inputType === 'file') {
                    return ['change'];
                }
                return ['input', 'change'];
                
            case 'select':
                return ['change'];
                
            case 'textarea':
                return ['input', 'blur'];
                
            default:
                return ['input', 'change'];
        }
    }
    
    /**
     * 排程自動暫存
     */
    scheduleAutoSave() {
        if (this.autoSaveTimer) {
            clearTimeout(this.autoSaveTimer);
        }
        
        this.autoSaveTimer = setTimeout(() => {
            this.saveFormData();
        }, this.autoSaveDelay);
    }
    
    /**
     * 暫存表單資料
     */
    saveFormData() {
        try {
            const formData = this.collectFormData();
            sessionStorage.setItem(this.cacheKey, JSON.stringify(formData));
            console.log('PropertyFormCache: 表單資料已暫存', formData);
        } catch (error) {
            console.error('PropertyFormCache: 暫存失敗', error);
        }
    }
    
    /**
     * 載入暫存資料
     */
    loadCachedData() {
        try {
            const cachedData = sessionStorage.getItem(this.cacheKey);
            if (cachedData) {
                const formData = JSON.parse(cachedData);
                this.restoreFormData(formData);
                console.log('PropertyFormCache: 已載入暫存資料', formData);
                
                // 顯示恢復提示
                this.showRestoredDataNotification();
            }
        } catch (error) {
            console.error('PropertyFormCache: 載入暫存資料失敗', error);
            this.clearCache();
        }
    }
    
    /**
     * 收集表單資料
     */
    collectFormData() {
        const formData = {};
        const formElements = this.form.querySelectorAll('input, select, textarea');
        
        formElements.forEach(element => {
            const name = element.name;
            if (!name) return;
            
            const tagName = element.tagName.toLowerCase();
            const inputType = element.type ? element.type.toLowerCase() : '';
            
            try {
                if (tagName === 'input') {
                    if (inputType === 'checkbox' || inputType === 'radio') {
                        // 處理多選框和單選框
                        if (!formData[name]) {
                            formData[name] = [];
                        }
                        if (element.checked) {
                            formData[name].push(element.value);
                        }
                    } else if (inputType === 'file') {
                        // 檔案上傳欄位不暫存實際檔案，只記錄有檔案
                        formData[name] = element.files.length > 0;
                    } else {
                        // 一般文字欄位
                        formData[name] = element.value;
                    }
                } else if (tagName === 'select') {
                    if (element.multiple) {
                        // 多選下拉選單
                        formData[name] = Array.from(element.selectedOptions).map(option => option.value);
                    } else {
                        // 單選下拉選單
                        formData[name] = element.value;
                    }
                } else if (tagName === 'textarea') {
                    formData[name] = element.value;
                }
            } catch (error) {
                console.warn('PropertyFormCache: 收集欄位資料失敗', name, error);
            }
        });
        
        // 添加暫存時間戳
        formData._cacheTimestamp = new Date().toISOString();
        
        return formData;
    }
    
    /**
     * 恢復表單資料
     */
    restoreFormData(formData) {
        const formElements = this.form.querySelectorAll('input, select, textarea');
        
        formElements.forEach(element => {
            const name = element.name;
            if (!name || !formData.hasOwnProperty(name)) return;
            
            const tagName = element.tagName.toLowerCase();
            const inputType = element.type ? element.type.toLowerCase() : '';
            
            try {
                if (tagName === 'input') {
                    if (inputType === 'checkbox' || inputType === 'radio') {
                        // 處理多選框和單選框
                        const values = Array.isArray(formData[name]) ? formData[name] : [formData[name]];
                        element.checked = values.includes(element.value);
                    } else if (inputType === 'file') {
                        // 檔案欄位不恢復，只顯示提示
                        // 實際檔案無法透過 JavaScript 設定
                    } else {
                        // 一般文字欄位
                        element.value = formData[name] || '';
                    }
                } else if (tagName === 'select') {
                    if (element.multiple) {
                        // 多選下拉選單
                        const values = Array.isArray(formData[name]) ? formData[name] : [formData[name]];
                        Array.from(element.options).forEach(option => {
                            option.selected = values.includes(option.value);
                        });
                    } else {
                        // 單選下拉選單
                        element.value = formData[name] || '';
                    }
                } else if (tagName === 'textarea') {
                    element.value = formData[name] || '';
                }
                
                // 觸發變更事件，讓其他功能知道資料已更新
                element.dispatchEvent(new Event('change', { bubbles: true }));
                
            } catch (error) {
                console.warn('PropertyFormCache: 恢復欄位資料失敗', name, error);
            }
        });
    }
    
    /**
     * 清除暫存資料
     */
    clearCache() {
        try {
            sessionStorage.removeItem(this.cacheKey);
            console.log('PropertyFormCache: 暫存資料已清除');
        } catch (error) {
            console.error('PropertyFormCache: 清除暫存失敗', error);
        }
    }
    
    /**
     * 手動暫存（可供外部呼叫）
     */
    manualSave() {
        this.saveFormData();
        this.showSaveNotification();
    }
    
    /**
     * 檢查是否有暫存資料
     */
    hasCachedData() {
        try {
            return sessionStorage.getItem(this.cacheKey) !== null;
        } catch (error) {
            return false;
        }
    }
    
    /**
     * 取得暫存資料的時間戳
     */
    getCacheTimestamp() {
        try {
            const cachedData = sessionStorage.getItem(this.cacheKey);
            if (cachedData) {
                const formData = JSON.parse(cachedData);
                return formData._cacheTimestamp;
            }
        } catch (error) {
            console.error('PropertyFormCache: 無法取得暫存時間戳', error);
        }
        return null;
    }
    
    /**
     * 排程表單提交後清除暫存
     */
    scheduleFormSubmitClear() {
        // 延遲清除，給表單提交一些時間
        setTimeout(() => {
            this.clearCache();
        }, 2000);
    }
    
    /**
     * 顯示恢復資料通知
     */
    showRestoredDataNotification() {
        const timestamp = this.getCacheTimestamp();
        const timeStr = timestamp ? new Date(timestamp).toLocaleString('zh-TW') : '未知時間';
        
        this.showNotification(
            `已恢復您在 ${timeStr} 的表單資料`, 
            'info', 
            5000
        );
    }
    
    /**
     * 顯示暫存通知
     */
    showSaveNotification() {
        this.showNotification('表單資料已暫存', 'success', 2000);
    }
    
    /**
     * 顯示通知訊息
     */
    showNotification(message, type = 'info', duration = 3000) {
        // 創建通知元素
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = `
            top: 20px; 
            right: 20px; 
            z-index: 9999; 
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
        
        notification.innerHTML = `
            <i class="fas fa-info-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        // 自動移除
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, duration);
    }
}

// 全域函式，供外部調用
window.PropertyFormCache = PropertyFormCache;