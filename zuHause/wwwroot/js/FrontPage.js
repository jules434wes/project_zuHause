document.addEventListener('DOMContentLoaded', function () {



    // 確保 jQuery 已經載入
    if (typeof jQuery === 'undefined') {
        console.error('jQuery is not loaded. Some functionalities may not work.');
        return;
    }


    // 1. Initialize tooltips (使用 Bootstrap 官方的 JS 方式)
    [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]')).map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // 2. Back to top button logic
    const $mybutton = $('#backToTopBtn'); // 使用 jQuery 選擇器

    if ($mybutton.length) { // 檢查按鈕是否存在
        $mybutton.on('click', function () {
            document.body.scrollTop = 0; // For Safari
            document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
        });
        $(window).on('scroll', function () {
            if ($(window).scrollTop() > 20) {
                $mybutton.fadeIn(); // 使用 jQuery 的 fadeIn/fadeOut
            } else {
                $mybutton.fadeOut();
            }
        });
    }

    // 3. Search input, button, and dropdown logic
    const $cityDropdownBtn = $('#cityDropdownBtn');
    const $searchInput = $('#searchInput');
    const $searchBtn = $('#searchBtn');
    const $dropdownItems = $('.dropdown-menu .dropdown-item'); // 使用 jQuery 選擇器
    let selectedCity = $cityDropdownBtn.text().trim(); // 初始化為按鈕上的預設城市
    let selectedCityCode = $cityDropdownBtn.data('citycode'); // 初始化為按鈕上的預設城市代碼

    // 處理城市下拉選單點擊事件
    $dropdownItems.on('click', function (event) {
        $dropdownItems.removeClass('active');
        $(this).addClass('active');
        selectedCity = $(this).data('city');
        selectedCityCode = $(this).data('citycode'); 
        $cityDropdownBtn.text(selectedCity);
        $cityDropdownBtn.data('citycode', selectedCityCode); 
        console.log('賦值後 $cityDropdownBtn 的 citycode (來自 jQuery 內部):', $cityDropdownBtn.data('citycode')); // 賦值後從 jQuery 內部讀取
    });

    // 搜尋歷史紀錄邏輯
    const searchHistoryKey = 'searchHistory';
    const maxHistoryItems = 5; // 限制最多顯示 5 個歷史紀錄

    function loadSearchHistory() {
        const history = JSON.parse(localStorage.getItem(searchHistoryKey) || '[]');
        const $searchHistoryDiv = $('#searchHistory'); // 使用 jQuery
        $searchHistoryDiv.empty();
        history.forEach(item => {
            $searchHistoryDiv.append(`
                <span class="badge bg-secondary p-2 d-flex align-items-center search-history-item " data-value="${item}">
                    ${item}
                    <button type="button" class="btn-close btn-close-white ms-2" aria-label="Close" data-value="${item}"></button>
                </span>
            `);
        });

        // Add click listener for history items (delegated for dynamically added elements)
        $searchHistoryDiv.off('click', '.search-history-item').on('click', '.search-history-item', function (e) {
            if (!$(e.target).hasClass('btn-close')) {
                const value = $(this).data('value');
                $searchInput.val(value); // 使用 jQuery
                // Optionally trigger search automatically
                // $searchBtn.click();
            }
        });

        // Add click listener for close buttons (delegated)
        $searchHistoryDiv.off('click', '.search-history-item .btn-close').on('click', '.search-history-item .btn-close', function (e) {
            e.stopPropagation(); // Prevent parent span click
            const valueToRemove = $(this).data('value');
            removeSearchHistory(valueToRemove);
        });
    }

    function addSearchHistory(value) {
        let history = JSON.parse(localStorage.getItem(searchHistoryKey) || '[]');
        history = history.filter(item => item !== value);
        history.unshift(value);
        history = history.slice(0, maxHistoryItems);
        localStorage.setItem(searchHistoryKey, JSON.stringify(history));
        loadSearchHistory();
    }

    function removeSearchHistory(value) {
        let history = JSON.parse(localStorage.getItem(searchHistoryKey) || '[]');
        history = history.filter(item => item !== value);
        localStorage.setItem(searchHistoryKey, JSON.stringify(history));
        loadSearchHistory();
    }

    // Handle search button click
    $searchBtn.on('click', function () {
        const keyword = $searchInput.val().trim();
        const citycode = $cityDropdownBtn.data('citycode'); // Get the selected city code from button data
        let searchUrl = `/Search/Search`;

        let queryParams = [];
        if (keyword) {
            queryParams.push(`keyword=${encodeURIComponent(keyword)}`);
        }
        if (citycode) {
            queryParams.push(`cityCode=${encodeURIComponent(citycode)}`);
        }

        if (queryParams.length > 0) {
            searchUrl += `?${queryParams.join('&')}`;
            // 只有當關鍵字有內容時才加入歷史紀錄
            if (keyword) { // modified: only add keyword to history
                addSearchHistory(keyword); // modified: add keyword directly
            }
           

            window.location.href = searchUrl;
        } else {
            alert("請輸入搜尋關鍵字或選擇城市！");
        }
    });


    // Toggle heart icon (for favoriting)
    $('.love-button').on('click', function () {
        const $button = $(this);
        const $icon = $button.find('.love-icon');
        let isFavorited = $button.data('favorited');

        if (isFavorited) {
            $icon.removeClass('bi-heart-fill').addClass('bi-heart'); // Bootstrap Icons: Solid to empty
            $button.data('favorited', false);
            // Send AJAX request to remove from favorites
            console.log('Removed from favorites');
        } else {
            $icon.removeClass('bi-heart').addClass('bi-heart-fill'); // Bootstrap Icons: Empty to solid
            $button.data('favorited', true);
            // Send AJAX request to add to favorites
            console.log('Added to favorites');
        }
    });

    const carouselImagesContainer = document.getElementById('carouselImagesContainer'); // 您的圖片父容器
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const indicatorsContainer = document.getElementById('carouselIndicators');
    const myCustomCarousel = document.getElementById('myCustomCarousel'); // 輪播圖外層容器

    let carouselImageUrls = window.clientCarouselImageUrls || [];

    //console.log("從 window.clientCarouselImageUrls 獲取到的圖片 URL:", carouselImageUrls);

    // 如果沒有圖片，提供一個預設圖片作為 fallback
    if (carouselImageUrls.length === 0) {
        carouselImageUrls = ["/images/carousel_8c2f33.png"]; // 這裡使用您之前有的預設圖
        console.warn("carouselImageUrls 為空，使用預設佔位圖片。請檢查後端數據傳遞。");
    }

    let currentIndex = 0;
    const totalImages = carouselImageUrls.length;
    let autoPlayInterval; // 用於儲存自動播放的計時器

    // 檢查是否有圖片可以顯示
    if (totalImages === 0) {
        console.error("錯誤：沒有圖片可供輪播。");
        // 可以添加一個佔位符圖片
        carouselImagesContainer.innerHTML = `<img src="/images/placeholder_no_image.png" alt="No Image" style="width: 100%; display: block;">`;
        if (prevBtn) prevBtn.style.display = 'none';
        if (nextBtn) nextBtn.style.display = 'none';
        if (indicatorsContainer) indicatorsContainer.style.display = 'none';
        return; // 沒有圖片就不執行輪播邏輯
    }

    // 清空現有內容並動態插入圖片
    carouselImagesContainer.innerHTML = '';
    indicatorsContainer.innerHTML = ''; // 清空指示器容器
    carouselImageUrls.forEach((url, index) => {
        // 創建圖片元素
        const img = document.createElement('img');
        img.src = url;
        img.alt = `Property Image ${index + 1}`;
        img.style.width = '100%';
        img.style.flexShrink = '0'; // 確保圖片不縮小
        img.style.display = 'block'; // 確保圖片佔用整行
        carouselImagesContainer.appendChild(img);
        //console.log(`添加圖片到輪播圖：${url}`);

        // 同步創建指示器
        const indicator = document.createElement('span');
        indicator.classList.add('custom-indicator'); // 自定義的指示器類別
        indicator.style.display = 'inline-block';
        indicator.style.width = '10px';
        indicator.style.height = '10px';
        indicator.style.backgroundColor = 'rgba(255,255,255,0.7)';
        indicator.style.borderRadius = '50%';
        indicator.style.margin = '0 5px';
        indicator.style.cursor = 'pointer';
        indicator.dataset.index = index; // 儲存索引

        indicator.addEventListener('click', function () {
            currentIndex = parseInt(this.dataset.index);
            updateCarouselDisplay();
            resetAutoPlay(); // 點擊指示器後重設自動播放
        });
        indicatorsContainer.appendChild(indicator);
    });

    const indicators = indicatorsContainer.querySelectorAll('.custom-indicator'); // 重新獲取指示器 NodeList

    function updateCarouselDisplay() {
        const offset = -currentIndex * 100; // 假設每張圖片佔 100% 寬度
        carouselImagesContainer.style.transform = `translateX(${offset}%)`;
        //console.log(`更新輪播圖顯示：滑動到索引 ${currentIndex}`);

        // 更新指示器 active 狀態
        indicators.forEach((indicator, index) => {
            if (index === currentIndex) {
                indicator.classList.add('active');
                indicator.style.backgroundColor = 'rgba(255,255,255,1)'; // Active 顏色
            } else {
                indicator.classList.remove('active');
                indicator.style.backgroundColor = 'rgba(255,255,255,0.7)'; // 非 Active 顏色
            }
        });
    }

    function showNextImage() {
        currentIndex = (currentIndex === totalImages - 1) ? 0 : currentIndex + 1;
        updateCarouselDisplay();
    }

    function showPrevImage() {
        currentIndex = (currentIndex === 0) ? totalImages - 1 : currentIndex - 1;
        updateCarouselDisplay();
    }

    // 為導航按鈕添加事件監聽器
    if (prevBtn) {
        prevBtn.addEventListener('click', function () {
            showPrevImage();
            resetAutoPlay(); // 點擊按鈕後重設自動播放
        });
    }
    if (nextBtn) {
        nextBtn.addEventListener('click', function () {
            showNextImage();
            resetAutoPlay(); // 點擊按鈕後重設自動播放
        });
    }

    // 自動播放邏輯
    function startAutoPlay() {
        clearInterval(autoPlayInterval); // 先清除現有的，避免重複
        autoPlayInterval = setInterval(showNextImage, 5000); // 每 5 秒切換一次
        //console.log("自動播放已啟動。");
    }

    function stopAutoPlay() {
        clearInterval(autoPlayInterval);
        //console.log("自動播放已停止。");
    }

    function resetAutoPlay() {
        stopAutoPlay();
        startAutoPlay();
    }

    // 鼠標懸停時暫停自動播放
    myCustomCarousel.addEventListener('mouseenter', stopAutoPlay);
    myCustomCarousel.addEventListener('mouseleave', startAutoPlay);

    // 初始啟動輪播
    updateCarouselDisplay(); // 確保初次顯示正確的圖片和指示器
    startAutoPlay(); // 啟動自動播放


    // 4. 公告跑馬燈內容輪播邏輯 (改進版：逐條顯示並等待上一條完成)
    const $marqueeContainer = $('.announcement-marquee-container');
    const $marqueeContentElement = $('#dynamicMarqueeContent');

    let marqueeData = window.clientMarqueeMessages || [];

    // 如果沒有從後端獲取到數據，提供一個預設的 MarqueeMessageViewModel 陣列
    if (marqueeData.length === 0) {
        marqueeData = [{ MessageText: "📢 目前沒有最新公告。", AttachmentUrl: null }];
    }

    let currentMessageIndex = 0;
    const animationDurationPerPixel = 0.007;
    const buffer = 10; // 增加一個小緩衝，確保完全消失

    function startMarqueeAnimation() {
        if (marqueeData.length === 0) {
            return;
        }

        const currentMarqueeItem = marqueeData[currentMessageIndex]; // 获取當前跑馬燈的完整物件
        const messageText = "📢 " + currentMarqueeItem.messageText; // 使用物件的 MessageText 屬性
        const attachmentUrl = currentMarqueeItem.attachmentUrl; // 使用物件的 AttachmentUrl 屬性
        $marqueeContentElement.text(messageText);

        $marqueeContainer.data('attachment-url', attachmentUrl || '');

        if (attachmentUrl) {
            $marqueeContainer.css('cursor', 'pointer');
        } else {
            $marqueeContainer.css('cursor', 'default');
        }

        setTimeout(() => {
            const containerWidth = $marqueeContainer.outerWidth();
            const contentWidth = $marqueeContentElement.outerWidth();

            const travelDistance = containerWidth + contentWidth + buffer;
            const animationTimeMs = travelDistance * animationDurationPerPixel * 1000;

            $marqueeContentElement.css({
                'left': containerWidth + 'px', // 設定初始位置在容器右側
                'transition': 'none' // 確保沒有舊的 CSS 過渡干擾
            });

            setTimeout(() => {
                $marqueeContentElement.animate(
                    { 'left': -(contentWidth + buffer) + 'px' }, // 動畫結束位置：完全離開左側
                    animationTimeMs,
                    'linear',
                    function () { // 動畫完成後的回調函式
                        $marqueeContentElement.css({
                            'left': containerWidth + 'px',
                            'transition': 'none' // 再次確保沒有動畫效果，快速重置
                        });

                        currentMessageIndex = (currentMessageIndex + 1) % marqueeData.length;

                        const delayBetweenMessages = 100; // 例如，延遲 100 毫秒
                        setTimeout(startMarqueeAnimation, delayBetweenMessages);
                    }
                );
            }, 50); // 確保 CSS 初始位置設置完成

        }, 100); // 確保文字渲染完成並獲取正確寬度
    }

    // 監聽跑馬燈容器的點擊事件 (使用 jQuery)
    $marqueeContainer.on('click', function () {
        const attachmentUrl = $(this).data('attachment-url');

        if (attachmentUrl) {
            window.location.href = attachmentUrl;
        }
    });

    // 初始啟動跑馬燈動畫序列
    startMarqueeAnimation();


    // Load search history on page load
    loadSearchHistory();

    // ==========================================================
    // ==== 以下是「猜你喜歡」功能的整合程式碼 ====
    // ==========================================================

    // 輔助函數：判斷使用者是否登入
    function isLoggedIn() {
        // **[變更標註]** 修正 clientIsAuthenticated 的類型檢查，因為它可能是字串 "true" 或 "false"
        return window.clientIsAuthenticated === true || window.clientIsAuthenticated === "true";
    }

    // 輔助函數：如果租客已登入，從後端獲取其 PrimaryRentalDistrictID 對應的 CityCode
    async function getTenantPreferredCityCode() {
        if (!isLoggedIn()) {
            console.log("用戶未登入，不獲取租客偏好城市代碼。");
            return null;
        }
        try {
            const token = localStorage.getItem('authToken'); // 假設 token 儲存在 localStorage
            if (!token) {
                console.warn("用戶已登入但無 authToken，無法獲取偏好城市代碼。");
                return null;
            }

            const response = await fetch('/api/Member/GetPreferredCityCode', { // 請確保這個 API 返回的是 CityCode
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            if (response.ok) {
                const data = await response.json();
                console.log("從後端獲取的租客偏好城市代碼:", data);
                // 假設 data 物件中包含 cityCode 屬性
                return data.cityCode;
            }
            console.error('Failed to fetch tenant preferred city code:', response.status, response.statusText);
            return null;
        } catch (error) {
            console.error('Error fetching tenant preferred city code:', error);
            return null;
        }
    }

    // 輔助函數：封裝 API 請求，獲取房源數據 (供「猜你喜歡」區塊使用)
    async function fetchPropertiesFromApi(params, limit) {
        let queryString = new URLSearchParams(params).toString();
        // 確保有設定 pageSize 和 pageNumber，以免 API 返回過多或過少數據
        if (!params.pageSize) queryString += `&pageSize=${limit}`;
        if (!params.pageNumber) queryString += `&pageNumber=1`;

        try {
            const response = await fetch(`/api/Tenant/Search/list?${queryString}`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            return data.properties || []; // 假設 API 返回的數據中有 properties 陣列
        } catch (error) {
            console.error('Error fetching properties from API:', error);
            return [];
        }
    }

    // 輔助函數：渲染房源到「猜你喜歡」區塊
    function renderYouMayLikeSection(properties) {
    
        const $youMayLikeContainer = $('#youMayLikeProperties');
        $youMayLikeContainer.empty(); // 清空預設的「載入中」提示

        if (properties.length === 0) {
            $youMayLikeContainer.html('<div class="col-12"><p class="text-center text-muted">目前沒有推薦的房源。</p></div>');
            return;
        }

        let htmlContent = '';
        properties.forEach(property => {
            // 請根據你的 PropertySearchViewModel 屬性調整這裡的屬性名
            const propertyId = property.propertyId;
            const title = property.title || '無標題';
            const addressLine = property.addressLine || '地址不詳';
            const roomCount = property.roomCount !== undefined && property.roomCount !== null ? property.roomCount : 0;
            const livingRoomCount = property.livingRoomCount;
            const bathroomCount = property.bathroomCount;
            const area = property.area !== undefined && property.area !== null ? property.area : 0;
            const monthlyRent = property.monthlyRent;
            const features = Array.isArray(property.features) ? property.features : [];
            const imageUrl = property.imagePath || '/images/apartment.jpg'; // 使用 property.ImagePath 並添加預設圖片
            const isFavorited = property.isFavorited;

            const layoutInfo = `${roomCount}房${livingRoomCount}廳${bathroomCount}衛`;
            const rentText = monthlyRent !== undefined && monthlyRent !== null ? `NT$ ${monthlyRent.toLocaleString()}/月` : '價格面議';
            const propertyUrl = `/Property/Details/${propertyId}`;

            htmlContent += `
                <div class="col">
                    <div class="card card-listing">
                        <img src="${imageUrl}" class="card-img-top" alt="${title}">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <h5 class="card-title mb-0">
                                    <a href="${propertyUrl}" class="text-truncate" title="${title}">${title}</a>
                                </h5>
                                <button type="button" class="btn btn-link p-0 love-button" data-favorited="${isFavorited}" data-property-id="${propertyId}">
                                    <i class="bi ${isFavorited ? 'bi-heart-fill' : 'bi-heart'} love-icon"></i>
                                </button>
                            </div>
                            <p class="card-text">
                                <i class="bi bi-geo-alt-fill"></i> ${addressLine}<br>
                                <i class="bi bi-house"></i> ${layoutInfo} | ${area}坪
                            </p>
                            <div class="tags d-flex flex-wrap mt-2">
                                ${features.map(feature => `<span class="badge bg-info text-dark me-1 mb-1">${feature}</span>`).join('')}
                            </div>
                        </div>
                        <div class="card-footer d-flex justify-content-between align-items-center">
                            <div class="price text-danger fw-bold">${rentText}</div>
                            <a href="${propertyUrl}" class="btn btn-sm btn-outline-primary">查看詳情</a>
                        </div>
                    </div>
                </div>
            `;
        });
        $youMayLikeContainer.html(htmlContent);

        // 重新初始化愛心按鈕的事件監聽器，使用事件委託
        $youMayLikeContainer.off('click', '.love-button').on('click', '.love-button', function () {
            const $button = $(this);
            const $icon = $button.find('.love-icon');
            let isFavorited = $button.data('favorited');
            const propertyId = $button.data('property-id');

            // 這裡可以發送 AJAX 請求到後端更新收藏狀態
            console.log(`嘗試更新收藏狀態：Property ID ${propertyId}, 當前收藏狀態: ${isFavorited}`);

            if (isFavorited) {
                $icon.removeClass('bi-heart-fill').addClass('bi-heart');
                $button.data('favorited', false);
                // 執行取消收藏的 AJAX 請求
            } else {
                $icon.removeClass('bi-heart').addClass('bi-heart-fill');
                $button.data('favorited', true);
                // 執行收藏的 AJAX 請求
            }
        });
    }

    // 核心邏輯：載入「猜你喜歡」房源
    async function loadYouMayLikeProperties() {
        let initialSearchParams = {};
        let targetCityCode = 'TPE'; // 預設城市代碼

        // 1. 優先從 localStorage 獲取上次搜尋的城市代碼
        const storedParams = localStorage.getItem('lastSearchCriteria');
        if (storedParams) {
            try {
                const parsedParams = JSON.parse(storedParams);
                // **[核心修正]：將所有解析出的參數都作為初始搜尋條件**
                initialSearchParams = { ...parsedParams }; // 使用展開運算符拷貝所有屬性

                if (initialSearchParams.cityCode) {
                    targetCityCode = initialSearchParams.cityCode; // 更新 targetCityCode
                    console.log("使用上次搜尋的城市偏好:", targetCityCode);
                } else {
                    console.log("上次搜尋未包含城市偏好。");
                }

            } catch (e) {
                console.error("Error parsing lastSearchCriteria from localStorage:", e);
                localStorage.removeItem('lastSearchCriteria'); // 清除無效數據
            }
        }

        // 2. 如果使用者已登入，且 initialSearchParams 中沒有 cityCode 或 districtCode
        //    則嘗試獲取租客的 PrimaryRentalDistrictID 對應的 CityCode (租客偏好優先於預設值)
        if (isLoggedIn() && (!initialSearchParams.cityCode && !initialSearchParams.districtCode)) {
            console.log("用戶已登入且無上次搜尋城市/區域偏好，嘗試獲取租客預設城市。");
            const tenantPreferredCityCode = await getTenantPreferredCityCode();
            if (tenantPreferredCityCode) {
                initialSearchParams.cityCode = tenantPreferredCityCode; // 覆蓋或添加 cityCode
                targetCityCode = tenantPreferredCityCode; // 也更新給 exploreMoreBtn 用
                console.log("使用租客偏好城市:", targetCityCode);
            }
        } else if (!isLoggedIn() && !initialSearchParams.cityCode && !initialSearchParams.districtCode) {
            console.log("用戶未登入且無上次搜尋城市/區域偏好，將使用預設城市 TPE。");
        }


        // 3. 如果經過上述判斷後仍然沒有城市/區域參數（即 initialSearchParams 中完全沒有城市或區域，且沒從 localStorage 或租客偏好獲取到）
        //    則為這次 API 請求設定一個預設城市
        if (!initialSearchParams.cityCode && !initialSearchParams.districtCode) { // 這裡條件應更精確
            initialSearchParams.cityCode = 'TPE'; // 確保至少有 TPE 作為搜尋條件
            targetCityCode = 'TPE'; // 也更新給 exploreMoreBtn 用
            console.log("最終推薦房源參數使用預設城市 TPE。");
        } else {
            console.log("最終推薦房源參數使用:", initialSearchParams);
        }


        // A. 第一輪 API 搜尋 (使用處理後的 initialSearchParams)
        const firstBatchProperties = await fetchPropertiesFromApi(initialSearchParams, 12); // 請求多一點以備去重

        let finalProperties = [];
        const uniquePropertyIds = new Set();

        firstBatchProperties.forEach(p => {
            if (finalProperties.length < 8 && !uniquePropertyIds.has(p.propertyId)) {
                finalProperties.push(p);
                uniquePropertyIds.add(p.propertyId);
            }
        });

        // B. 補足機制：如果房源少於 8 個，則發送第二次請求「無搜尋條件」的房源
        if (finalProperties.length < 8) {
            const neededCount = 8 - finalProperties.length;
            console.log(`第一輪房源不足 ${neededCount} 個，發送第二次請求補足。`);
            const noFilterParams = {}; // 空參數表示無篩選
            const secondBatchProperties = await fetchPropertiesFromApi(noFilterParams, neededCount);

            secondBatchProperties.forEach(p => {
                if (finalProperties.length < 8 && !uniquePropertyIds.has(p.propertyId)) {
                    finalProperties.push(p);
                    uniquePropertyIds.add(p.id); // 注意這裡應該是 p.propertyId 而不是 p.id
                }
            });
        } else {
            console.log("第一輪房源已足夠 8 個。");
        }

        // 渲染最終的房源列表 (取前 8 個)
        renderYouMayLikeSection(finalProperties.slice(0, 8));


        // 將最終確定的 targetCityCode 應用於「探索更多物件」按鈕的連結
        const $exploreMoreBtn = $('#exploreMoreBtn');
        const lastFullSearchUrl = localStorage.getItem('lastFullSearchUrl');
        let exploreLink = lastFullSearchUrl || `/Search/Search?cityCode=${encodeURIComponent(targetCityCode)}`;

        // 如果 initialSearchParams 包含 districtCode 且未從 lastFullSearchUrl 恢復，則更新連結
        if (!lastFullSearchUrl && initialSearchParams.districtCode) {
            exploreLink = `/Search/Search?cityCode=${encodeURIComponent(targetCityCode)}&districtCode=${encodeURIComponent(initialSearchParams.districtCode)}`;
        }


        if ($exploreMoreBtn.length > 0) {
            $exploreMoreBtn.attr('href', exploreLink); // 直接設置 href
            console.log("探索更多物件按鈕的 href 已設置為:", exploreLink);
        }
    }


    // ==========================================================
    // ==== 調用核心功能，確保在 DOMContentLoaded 事件中執行 ====
    // ==========================================================
    loadSearchHistory(); // 載入搜尋歷史
    loadYouMayLikeProperties(); // 載入「猜你喜歡」房源和設置「探索更多物件」按鈕連結

   //頁面載入時根據 localStorage 恢復搜尋框的城市和關鍵字
    const storedLastSearchCriteria = localStorage.getItem('lastSearchCriteria');
    if (storedLastSearchCriteria) {
        try {
            const parsedCriteria = JSON.parse(storedLastSearchCriteria);
            if (parsedCriteria.cityCode) {
                const cityLink = $(`.dropdown-menu a.dropdown-item[data-citycode="${parsedCriteria.cityCode}"]`);
                if (cityLink.length > 0) {
                    $cityDropdownBtn.text(cityLink.text()).data('citycode', parsedCriteria.cityCode);
                    $dropdownItems.removeClass('active');
                    cityLink.addClass('active');
                }
            }
            if (parsedCriteria.keyword) {
                $searchInput.val(parsedCriteria.keyword);
            }
        } catch (e) {
            console.error("Error restoring search input from localStorage:", e);
            localStorage.removeItem('lastSearchCriteria');
        }
    }

}); // End of DOMContentLoaded