/**
 * 房源座標自動補全管理器
 * 負責檢查房源是否有座標，若無則自動調用 API 進行地址轉換
 */
class CoordinateCompletionManager {
    constructor() {
        this.isProcessing = false;
        this.retryCount = 0;
        this.maxRetries = 3;
        this.notificationShown = false;
    }

    /**
     * 初始化座標補全功能
     * @param {number} propertyId 房源ID
     * @param {string} address 房源地址
     * @param {number|null} latitude 現有緯度
     * @param {number|null} longitude 現有經度
     */
    async initialize(propertyId, address, latitude, longitude) {
        console.log('🗺️ 初始化座標補全檢查', {
            propertyId,
            address,
            hasLatitude: latitude !== null && latitude !== undefined,
            hasLongitude: longitude !== null && longitude !== undefined
        });

        // 檢查是否需要補全座標
        if (this.needsCoordinateCompletion(latitude, longitude, address)) {
            await this.startCoordinateCompletion(propertyId, address);
        } else {
            console.log('✅ 房源座標已存在，無需補全');
        }
    }

    /**
     * 檢查是否需要座標補全
     * @param {number|null} latitude 緯度
     * @param {number|null} longitude 經度
     * @param {string} address 地址
     * @returns {boolean}
     */
    needsCoordinateCompletion(latitude, longitude, address) {
        const hasCoordinates = latitude !== null && 
                              latitude !== undefined && 
                              longitude !== null && 
                              longitude !== undefined &&
                              latitude !== 0 && 
                              longitude !== 0;
        
        const hasAddress = address && address.trim().length > 0;
        
        return !hasCoordinates && hasAddress;
    }

    /**
     * 開始座標補全流程
     * @param {number} propertyId 房源ID
     * @param {string} address 房源地址
     */
    async startCoordinateCompletion(propertyId, address) {
        if (this.isProcessing) {
            console.log('⚠️ 座標補全正在進行中，跳過重複請求');
            return;
        }

        this.isProcessing = true;
        console.log('🚀 開始自動座標補全', { propertyId, address });

        try {
            // 顯示用戶提示
            this.showProcessingNotification();

            // 調用座標轉換 API
            const result = await this.callGeocodingAPI(propertyId, address);

            if (result.success) {
                console.log('✅ 座標補全成功', result);
                this.showSuccessNotification();
                
                // 重新載入地圖（如果存在）
                this.refreshMapIfExists();
            } else {
                console.warn('⚠️ 座標補全失敗', result);
                this.showErrorNotification(result.message);
            }

        } catch (error) {
            console.error('❌ 座標補全發生異常', error);
            this.handleError(error, propertyId, address);
        } finally {
            this.isProcessing = false;
        }
    }

    /**
     * 調用地理編碼 API
     * @param {number} propertyId 房源ID
     * @param {string} address 地址
     * @returns {Promise<Object>}
     */
    async callGeocodingAPI(propertyId, address) {
        const response = await fetch('/api/property/coordinate-completion', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify({
                propertyId: propertyId,
                address: address
            })
        });

        if (!response.ok) {
            throw new Error(`API 請求失敗: ${response.status} ${response.statusText}`);
        }

        return await response.json();
    }

    /**
     * 獲取防偽標記
     * @returns {string}
     */
    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    /**
     * 顯示處理中通知
     */
    showProcessingNotification() {
        if (this.notificationShown) return;

        const notification = this.createNotification(
            '🗺️ 正在補全房源位置資訊...',
            'info',
            'coordinate-processing'
        );
        
        document.body.appendChild(notification);
        this.notificationShown = true;

        // 3秒後自動移除處理中通知
        setTimeout(() => {
            this.removeNotification('coordinate-processing');
        }, 3000);
    }

    /**
     * 顯示成功通知
     */
    showSuccessNotification() {
        this.removeNotification('coordinate-processing');
        
        const notification = this.createNotification(
            '✅ 房源位置資訊已自動補全',
            'success',
            'coordinate-success'
        );
        
        document.body.appendChild(notification);

        // 5秒後自動移除
        setTimeout(() => {
            this.removeNotification('coordinate-success');
        }, 5000);
    }

    /**
     * 顯示錯誤通知
     * @param {string} message 錯誤訊息
     */
    showErrorNotification(message) {
        this.removeNotification('coordinate-processing');
        
        const notification = this.createNotification(
            `⚠️ 位置資訊補全失敗: ${message}`,
            'warning',
            'coordinate-error'
        );
        
        document.body.appendChild(notification);

        // 8秒後自動移除
        setTimeout(() => {
            this.removeNotification('coordinate-error');
        }, 8000);
    }

    /**
     * 創建通知元素
     * @param {string} message 訊息
     * @param {string} type 類型 (info, success, warning, error)
     * @param {string} id 唯一ID
     * @returns {HTMLElement}
     */
    createNotification(message, type, id) {
        const notification = document.createElement('div');
        notification.id = id;
        notification.className = `coordinate-notification coordinate-notification-${type}`;
        notification.innerHTML = `
            <div class="coordinate-notification-content">
                <span class="coordinate-notification-message">${message}</span>
                <button class="coordinate-notification-close" onclick="document.getElementById('${id}').remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;

        return notification;
    }

    /**
     * 移除通知
     * @param {string} id 通知ID
     */
    removeNotification(id) {
        const notification = document.getElementById(id);
        if (notification) {
            notification.remove();
        }
    }

    /**
     * 處理錯誤（包含重試邏輯）
     * @param {Error} error 錯誤物件
     * @param {number} propertyId 房源ID
     * @param {string} address 地址
     */
    async handleError(error, propertyId, address) {
        this.retryCount++;
        
        if (this.retryCount <= this.maxRetries) {
            console.log(`🔄 座標補全重試 ${this.retryCount}/${this.maxRetries}`, error.message);
            
            // 延遲重試
            await new Promise(resolve => setTimeout(resolve, this.retryCount * 2000));
            
            // 重新開始補全流程
            this.isProcessing = false;
            await this.startCoordinateCompletion(propertyId, address);
        } else {
            console.error('❌ 座標補全重試次數已達上限', error);
            this.showErrorNotification('無法自動補全位置資訊，請聯繫管理員');
        }
    }

    /**
     * 重新載入地圖（如果存在）
     */
    refreshMapIfExists() {
        try {
            // 檢查是否存在地圖管理器
            if (window.PropertyMapManager || window.mapManager) {
                console.log('🔄 重新載入地圖以顯示更新的座標');
                
                // 延遲重新載入地圖，確保後端資料已更新
                setTimeout(() => {
                    if (window.mapManager && typeof window.mapManager.initMap === 'function') {
                        const propertyId = this.getPropertyIdFromPage();
                        if (propertyId) {
                            window.mapManager.initMap(propertyId);
                        }
                    }
                }, 2000);
            }
        } catch (error) {
            console.warn('⚠️ 重新載入地圖時發生錯誤', error);
        }
    }

    /**
     * 從頁面取得房源ID
     * @returns {number|null}
     */
    getPropertyIdFromPage() {
        // 方法1: 從URL路徑解析
        const pathMatch = window.location.pathname.match(/\/property\/(?:detail\/)?(\d+)/);
        if (pathMatch) {
            return parseInt(pathMatch[1]);
        }
        
        // 方法2: 從頁面資料屬性取得
        const propertyElement = document.querySelector('[data-property-id]');
        if (propertyElement) {
            return parseInt(propertyElement.dataset.propertyId);
        }
        
        return null;
    }
}

// CSS 樣式注入
const styles = `
.coordinate-notification {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 9999;
    max-width: 400px;
    min-width: 300px;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    animation: slideInRight 0.3s ease-out;
}

.coordinate-notification-info {
    background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
    color: white;
}

.coordinate-notification-success {
    background: linear-gradient(135deg, #10b981 0%, #047857 100%);
    color: white;
}

.coordinate-notification-warning {
    background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
    color: white;
}

.coordinate-notification-error {
    background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
    color: white;
}

.coordinate-notification-content {
    padding: 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.coordinate-notification-message {
    flex: 1;
    font-size: 14px;
    line-height: 1.4;
    font-weight: 500;
}

.coordinate-notification-close {
    background: none;
    border: none;
    color: inherit;
    font-size: 16px;
    cursor: pointer;
    padding: 4px;
    border-radius: 4px;
    transition: all 0.2s ease;
    margin-left: 12px;
}

.coordinate-notification-close:hover {
    background: rgba(255, 255, 255, 0.2);
}

@keyframes slideInRight {
    from {
        transform: translateX(100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

@media (max-width: 768px) {
    .coordinate-notification {
        top: 10px;
        right: 10px;
        left: 10px;
        max-width: none;
        min-width: auto;
    }
}
`;

// 動態注入 CSS
const coordinateStyleSheet = document.createElement('style');
coordinateStyleSheet.textContent = styles;
document.head.appendChild(coordinateStyleSheet);

// 全域變數，供其他腳本使用
window.CoordinateCompletionManager = CoordinateCompletionManager;