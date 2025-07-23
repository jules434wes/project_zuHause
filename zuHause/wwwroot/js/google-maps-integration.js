/**
 * Google Maps 整合管理器
 * 負責房源詳情頁面的地圖功能整合
 */
class PropertyMapManager {
    constructor() {
        this.map = null;
        this.propertyMarker = null;
        this.nearbyMarkers = [];
        this.isMapLoaded = false;
        this.mapContainer = null;
        this.apiKey = 'AIzaSyCIMrRHlR6LL2jayjfau4w9UH5V61WT9zM';
    }

    /**
     * 初始化地圖功能
     * @param {number} propertyId 房源ID
     */
    async initMap(propertyId) {
        console.log('開始初始化房源地圖，房源ID:', propertyId);
        
        try {
            // 1. 建立地圖容器
            this.createMapContainer();
            
            // 2. 呼叫 API 獲取房源地圖資料
            const mapData = await this.fetchPropertyMapData(propertyId);
            
            if (!mapData) {
                this.showFallbackContent();
                return;
            }

            // 3. 檢查 API 限制狀態
            if (mapData.isLimited) {
                this.showLimitedMessage(mapData.limitMessage);
                this.updateTransportInfo([]);
                return;
            }

            // 4. 載入 Google Maps JavaScript API
            await this.loadGoogleMapsAPI();
            
            // 5. 初始化地圖並顯示房源位置
            await this.initializeMap(mapData);
            
            // 6. 載入附近設施標記
            this.loadNearbyMarkers(mapData.nearbyPlaces);
            
            // 7. 更新現有的交通資訊文字區塊
            this.updateTransportInfo(mapData.nearbyPlaces);
            
            console.log('地圖初始化完成');
            
        } catch (error) {
            console.warn('地圖載入失敗，顯示靜態資訊:', error);
            this.showFallbackContent();
        }
    }

    /**
     * 建立地圖容器
     */
    createMapContainer() {
        const transportSection = document.getElementById('transportation');
        if (!transportSection) return;

        // 建立地圖容器
        const mapContainer = document.createElement('div');
        mapContainer.id = 'property-map';
        mapContainer.className = 'property-map-container mb-3';
        mapContainer.style.cssText = 'height: 300px; width: 100%; border-radius: 8px; overflow: hidden;';
        
        // 將地圖容器插入到交通資訊區塊的內容開始處
        const sectionContent = transportSection.querySelector('.section-content');
        if (sectionContent) {
            sectionContent.insertBefore(mapContainer, sectionContent.firstChild);
            this.mapContainer = mapContainer;
        }
    }

    /**
     * 呼叫 PropertyMap API 獲取房源地圖資料
     * @param {number} propertyId 房源ID
     * @returns {Promise<object|null>} 地圖資料或null
     */
    async fetchPropertyMapData(propertyId) {
        try {
            const response = await fetch(`/api/property/${propertyId}/map`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`API 請求失敗: ${response.status} ${response.statusText}`);
            }

            const data = await response.json();
            console.log('取得房源地圖資料:', data);
            return data;
            
        } catch (error) {
            console.error('獲取房源地圖資料失敗:', error);
            return null;
        }
    }

    /**
     * 載入 Google Maps JavaScript API
     * @returns {Promise<void>}
     */
    async loadGoogleMapsAPI() {
        if (window.google && window.google.maps) {
            return Promise.resolve();
        }

        return new Promise((resolve, reject) => {
            // 設定全域回調函數
            window.initGoogleMaps = () => {
                console.log('Google Maps API 載入完成');
                this.isMapLoaded = true;
                resolve();
            };

            // 動態載入 Google Maps API
            const script = document.createElement('script');
            script.src = `https://maps.googleapis.com/maps/api/js?key=${this.apiKey}&callback=initGoogleMaps&libraries=places`;
            script.defer = true;
            script.async = true;
            script.onerror = () => reject(new Error('Google Maps API 載入失敗'));
            
            document.head.appendChild(script);
            
            // 30秒超時
            setTimeout(() => {
                if (!this.isMapLoaded) {
                    reject(new Error('Google Maps API 載入超時'));
                }
            }, 30000);
        });
    }

    /**
     * 初始化地圖並顯示房源位置
     * @param {object} mapData 地圖資料
     */
    async initializeMap(mapData) {
        if (!this.mapContainer || !window.google) return;

        const propertyLocation = {
            lat: mapData.latitude,
            lng: mapData.longitude
        };

        // 建立地圖實例
        this.map = new google.maps.Map(this.mapContainer, {
            zoom: 15,
            center: propertyLocation,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            gestureHandling: 'cooperative',
            mapTypeControl: false,
            fullscreenControl: false,
            streetViewControl: false,
            styles: [
                {
                    featureType: 'poi',
                    elementType: 'labels',
                    stylers: [{ visibility: 'off' }]
                }
            ]
        });

        // 建立房源標記
        this.propertyMarker = new google.maps.Marker({
            position: propertyLocation,
            map: this.map,
            title: '房源位置',
            icon: {
                url: 'data:image/svg+xml;base64,' + btoa(`
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="32" height="32">
                        <path fill="#e74c3c" d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z"/>
                        <circle fill="#fff" cx="12" cy="9" r="3"/>
                    </svg>
                `),
                scaledSize: new google.maps.Size(32, 32),
                anchor: new google.maps.Point(16, 32)
            }
        });
    }

    /**
     * 載入附近設施標記
     * @param {Array} nearbyPlaces 附近設施列表
     */
    loadNearbyMarkers(nearbyPlaces) {
        if (!this.map || !nearbyPlaces || nearbyPlaces.length === 0) return;

        // 清除現有標記
        this.nearbyMarkers.forEach(marker => marker.setMap(null));
        this.nearbyMarkers = [];

        // 建立設施類型圖標映射
        const typeIcons = this.getPlaceTypeIcons();

        nearbyPlaces.forEach((place, index) => {
            if (place.latitude && place.longitude) {
                const marker = new google.maps.Marker({
                    position: {
                        lat: place.latitude,
                        lng: place.longitude
                    },
                    map: this.map,
                    title: `${place.name} (${place.typeDisplayName})`,
                    icon: {
                        url: typeIcons[place.type] || typeIcons.default,
                        scaledSize: new google.maps.Size(24, 24),
                        anchor: new google.maps.Point(12, 12)
                    }
                });

                // 建立資訊視窗
                const infoWindow = new google.maps.InfoWindow({
                    content: `
                        <div class="place-info">
                            <h6>${place.name}</h6>
                            <p class="mb-1">${place.typeDisplayName}</p>
                            <p class="mb-1">距離: ${Math.round(place.distance)}公尺</p>
                            <p class="mb-0">步行約: ${place.walkingTimeMinutes}分鐘</p>
                            ${place.rating ? `<p class="mb-0">評分: ${place.rating}⭐</p>` : ''}
                        </div>
                    `
                });

                marker.addListener('click', () => {
                    infoWindow.open(this.map, marker);
                });

                this.nearbyMarkers.push(marker);
            }
        });
    }

    /**
     * 取得設施類型圖標映射
     * @returns {object} 圖標映射
     */
    getPlaceTypeIcons() {
        const createIcon = (color) => `data:image/svg+xml;base64,${btoa(`
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
                <circle fill="${color}" cx="12" cy="12" r="8" stroke="#fff" stroke-width="2"/>
            </svg>
        `)}`;

        return {
            'subway_station': createIcon('#3498db'),    // 藍色 - 捷運
            'bus_station': createIcon('#2ecc71'),       // 綠色 - 公車
            'train_station': createIcon('#9b59b6'),     // 紫色 - 火車
            'supermarket': createIcon('#f39c12'),       // 橙色 - 超市
            'convenience_store': createIcon('#e67e22'), // 深橙 - 便利商店
            'post_office': createIcon('#34495e'),       // 深灰 - 郵局
            'bank': createIcon('#27ae60'),              // 深綠 - 銀行
            'pharmacy': createIcon('#e74c3c'),          // 紅色 - 藥局
            'police': createIcon('#2c3e50'),            // 黑色 - 警局
            'fire_station': createIcon('#c0392b'),      // 深紅 - 消防
            'hospital': createIcon('#e74c3c'),          // 紅色 - 醫院
            'park': createIcon('#27ae60'),              // 綠色 - 公園
            'gym': createIcon('#8e44ad'),               // 紫色 - 健身房
            'library': createIcon('#2980b9'),           // 藍色 - 圖書館
            'school': createIcon('#f1c40f'),            // 黃色 - 學校
            'university': createIcon('#d35400'),        // 深橙 - 大學
            'default': createIcon('#95a5a6')            // 灰色 - 預設
        };
    }

    /**
     * 更新現有的交通資訊文字區塊
     * @param {Array} nearbyPlaces 附近設施列表
     */
    updateTransportInfo(nearbyPlaces) {
        const transportInfo = document.querySelector('.transport-info');
        if (!transportInfo) return;

        // 清除現有內容
        transportInfo.innerHTML = '';

        if (!nearbyPlaces || nearbyPlaces.length === 0) {
            transportInfo.innerHTML = `
                <div class="transport-item">
                    <span class="transport-label">附近設施：</span>
                    <span class="transport-value">暫無資料</span>
                </div>
            `;
            return;
        }

        // 按類型分組並排序
        const groupedPlaces = this.groupPlacesByType(nearbyPlaces);
        
        // 建立交通資訊項目
        Object.entries(groupedPlaces).forEach(([typeDisplayName, places]) => {
            const closest = places[0]; // 取最近的設施
            const transportItem = document.createElement('div');
            transportItem.className = 'transport-item';
            transportItem.innerHTML = `
                <span class="transport-label">${closest.name}：</span>
                <span class="transport-value">步行${closest.walkingTimeMinutes}分鐘 (${Math.round(closest.distance)}公尺)</span>
            `;
            transportInfo.appendChild(transportItem);
        });
    }

    /**
     * 按類型分組設施
     * @param {Array} places 設施列表
     * @returns {object} 分組後的設施
     */
    groupPlacesByType(places) {
        const grouped = {};
        const priorityTypes = ['subway_station', 'bus_station', 'convenience_store', 'supermarket', 'park'];
        
        places.forEach(place => {
            const typeKey = place.typeDisplayName;
            if (!grouped[typeKey]) {
                grouped[typeKey] = [];
            }
            grouped[typeKey].push(place);
        });

        // 每個類型只保留最近的設施，並按距離排序
        Object.keys(grouped).forEach(key => {
            grouped[key].sort((a, b) => a.distance - b.distance);
            grouped[key] = grouped[key].slice(0, 1); // 只取最近的一個
        });

        // 按優先級排序並限制顯示數量
        const result = {};
        let count = 0;
        
        // 先加入優先類型
        priorityTypes.forEach(type => {
            const typeDisplayName = Object.keys(grouped).find(key => 
                grouped[key][0] && grouped[key][0].type === type
            );
            if (typeDisplayName && count < 6) {
                result[typeDisplayName] = grouped[typeDisplayName];
                delete grouped[typeDisplayName];
                count++;
            }
        });
        
        // 再加入其他類型
        Object.entries(grouped).forEach(([key, value]) => {
            if (count < 6) {
                result[key] = value;
                count++;
            }
        });
        
        return result;
    }

    /**
     * 顯示 API 限制訊息
     * @param {string} message 限制訊息
     */
    showLimitedMessage(message) {
        if (this.mapContainer) {
            this.mapContainer.innerHTML = `
                <div class="map-message limited-message">
                    <div class="text-center p-4">
                        <i class="fas fa-exclamation-triangle fa-2x mb-3 text-warning"></i>
                        <h6>地圖服務暫時受限</h6>
                        <p class="mb-0">${message}</p>
                    </div>
                </div>
            `;
            this.mapContainer.style.cssText += 'display: flex; align-items: center; justify-content: center; background-color: #f8f9fa;';
        }
    }

    /**
     * 顯示降級內容
     */
    showFallbackContent() {
        if (this.mapContainer) {
            this.mapContainer.innerHTML = `
                <div class="map-message fallback-message">
                    <div class="text-center p-4">
                        <i class="fas fa-map-marker-alt fa-2x mb-3 text-muted"></i>
                        <h6>地圖載入失敗</h6>
                        <p class="mb-0">請查看下方的交通資訊</p>
                    </div>
                </div>
            `;
            this.mapContainer.style.cssText += 'display: flex; align-items: center; justify-content: center; background-color: #f8f9fa;';
        }

        // 恢復原始的靜態交通資訊
        this.updateTransportInfo([]);
    }
}

// 當頁面載入完成後自動初始化
document.addEventListener('DOMContentLoaded', function() {
    // 從頁面 URL 或資料屬性取得房源ID
    const propertyId = getPropertyIdFromPage();
    
    if (propertyId) {
        const mapManager = new PropertyMapManager();
        
        // 延遲載入以避免影響頁面載入效能
        setTimeout(() => {
            mapManager.initMap(propertyId);
        }, 1000);
    }
});

/**
 * 從頁面取得房源ID
 * @returns {number|null} 房源ID
 */
function getPropertyIdFromPage() {
    // 方法1: 從URL路徑解析 (/property/123 或 /property/detail/123)
    const pathMatch = window.location.pathname.match(/\/property\/(?:detail\/)?(\d+)/);
    if (pathMatch) {
        return parseInt(pathMatch[1]);
    }
    
    // 方法2: 從頁面資料屬性取得
    const propertyElement = document.querySelector('[data-property-id]');
    if (propertyElement) {
        return parseInt(propertyElement.dataset.propertyId);
    }
    
    console.warn('無法取得房源ID');
    return null;
}