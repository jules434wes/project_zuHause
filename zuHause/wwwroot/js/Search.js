




// 確保 jQuery 已經載入
if (typeof jQuery === 'undefined') {
    console.error('jQuery is not loaded. Some functionalities may not work.');
    // 阻止腳本進一步執行，如果 jQuery 是必要的
    // return; 
}

// 1. Initialize tooltips (使用 Bootstrap 官方的 JS 方式)
var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
});

// 全局變數定義
let pageNumber = 1;
let pageSize = 6; // 每頁顯示 6 筆

// 搜尋結果、分頁容器和提示訊息的 DOM 元素
const $paginationContainer = $('#paginationContainer');
const $noResultsMessage = $('#noResultsMessage');
const totalResultsCountSpan = $('#totalResultsCount');

// 主要搜尋相關的 DOM 元素 (使用 jQuery 選擇器)
const $keywordSearch = $('#keywordSearch');
const $searchButton = $('#searchButton');
const $districtSelect = $('#districtSelect'); // 行政區下拉選單
const $districtCheckboxes = $('#districtCheckboxes');
const $searchHistoryDiv = $('#searchHistory'); // 搜尋歷史紀錄容器
const $cityDropdownBtn = $('#cityDropdownBtn'); // 城市下拉按鈕
const $cityDropdownItems = $('.dropdown-menu .custom-city-list .dropdown-item'); // 城市下拉選單中的所有城市項目


// 篩選條件相關的 DOM 元素 (為了保持原始邏輯的引用)
const rentNoLimitRadio = document.getElementById('rentNoLimit');
const minRentInput = document.getElementById('minRentInput');
const maxRentInput = document.getElementById('maxRentInput');
const rentNoLimitLabel = document.querySelector('label[for="rentNoLimit"]');

const areaNoLimitRadio = document.getElementById('areaNoLimit');
const minAreaInput = document.getElementById('minAreaInput');
const maxAreaInput = document.getElementById('maxAreaInput');
const areaNoLimitLabel = document.querySelector('label[for="areaNoLimit"]');


// 排序按鈕
const sortButtons = document.querySelectorAll('.sort-btn');
let currentSortField = 'PublishedAt'; // 預設排序字段
let currentSortOrder = 'desc'; // 預設排序順序 (新加入的，與 HTML 中初始設定一致)


// 全局變數用於儲存選中的城市代碼和行政區代碼
let cityCode = 'TPE'; // 預設臺北市的城市代碼


// 用於儲存城市名稱與代碼的映射，方便顯示
let cityCodeToNameMap = {};


// 搜尋歷史紀錄邏輯
const searchHistoryKey = 'searchHistory';
const maxHistoryItems = 5; // 限制最多顯示 5 個歷史紀錄

//設置旗標，阻止事件監聽器中的自動觸發
let isApplyingUrlParams = false;

function loadSearchHistory() {
    const history = JSON.parse(localStorage.getItem(searchHistoryKey) || '[]');
    $searchHistoryDiv.empty();
    history.forEach(item => {
        $searchHistoryDiv.append(`
            <span class="badge bg-secondary p-2 d-flex align-items-center search-history-item" data-value="${item}">
                ${item}
                <button type="button" class="btn-close btn-close-white ms-2" aria-label="Close" data-value="${item}"></button>
            </span>
        `);
    });

    // Add click listener for close buttons (delegated)
    $searchHistoryDiv.off('click', '.search-history-item .btn-close').on('click', '.search-history-item .btn-close', function (e) {
        e.stopPropagation(); // Prevent parent span click
        const valueToRemove = $(this).data('value');
        removeSearchHistory(valueToRemove);
    });
}

function addSearchHistory(value) {
    if (!value || value.trim() === '') return; // 不添加空值

    let history = JSON.parse(localStorage.getItem(searchHistoryKey) || '[]');
    history = history.filter(item => item !== value); // 移除舊的相同項
    history.unshift(value); // 添加到最前面
    history = history.slice(0, maxHistoryItems); // 限制數量
    localStorage.setItem(searchHistoryKey, JSON.stringify(history));
    loadSearchHistory();
}

function removeSearchHistory(value) {
    let history = JSON.parse(localStorage.getItem(searchHistoryKey) || '[]');
    history = history.filter(item => item !== value);
    localStorage.setItem(searchHistoryKey, JSON.stringify(history));
    loadSearchHistory();
}

//url

// --- 獲取篩選參數函數 ---
function getFilterParams() {
    const params = new URLSearchParams();

    // 關鍵字
    const keyword = $keywordSearch.val().trim();
    if (keyword) {
        params.append('keyword', keyword);
    }

    // 城市和行政區
    params.append('cityCode', cityCode);
    const selectedDistricts = [];
    if (!$('#districtNoLimit').is(':checked')) {

        // 遍歷所有 district-checkbox，收集被選中的值
        $('.district-checkbox:checked').each(function () {
            selectedDistricts.push($(this).val());
        });
        // 如果有選中的行政區，則將它們作為多個參數傳遞
        selectedDistricts.forEach(districtCode => {
            params.append('districtCode', districtCode);
        });
    }

    // 租金範圍
    const hasMinRentInput = minRentInput && minRentInput.value !== '';
    const hasMaxRentInput = maxRentInput && maxRentInput.value !== '';

    if (hasMinRentInput || hasMaxRentInput) {
        // 如果手動輸入框有值，優先使用手動輸入的值
        const min = parseInt(minRentInput.value);
        const max = parseInt(maxRentInput.value);
        if (!isNaN(min) && minRentInput.value !== '') params.append('minRent', min);
        if (!isNaN(max) && maxRentInput.value !== '') params.append('maxRent', max);
    } else {
        const selectedRentCheckboxes = document.querySelectorAll('input[name="rent_range"][type="checkbox"]:checked');
        if (selectedRentCheckboxes && selectedRentCheckboxes.length > 0) {
            selectedRentCheckboxes.forEach(checkbox => {
                    params.append('rentRanges', checkbox.value);
            });
        }
    }

    // 格局 (多選)
    if (layoutNoLimit && !layoutNoLimit.checked) {
        const selectedRoomCheckboxes = document.querySelectorAll('.filter-group input[name="layout_group"]:checked');
        selectedRoomCheckboxes.forEach(checkbox => {
            if (checkbox.value) { // 只要有值就加入，因為 noLimit 的情況已經在外面排除了                   
                params.append('roomCounts', checkbox.value);
            }
        });
    }

    // 衛浴 (多選) 
    if (bathroomNoLimit && !bathroomNoLimit.checked) {
        // *** 修正: 確保這裡的 name 屬性與 HTML 中一致，例如 'bathroom_group' ***
        const selectedBathroomCheckboxes = document.querySelectorAll('.filter-group input[name="bathroom_group"]:checked');
        selectedBathroomCheckboxes.forEach(checkbox => {
            if (checkbox.value) { // 只要有值就加入，因為 noLimit 的情況已經在外面排除了
                params.append('bathroomCounts', checkbox.value);
            }
        });
    }


    if (floorNoLimit && !floorNoLimit.checked) {
        const selectedFloorCheckboxes = document.querySelectorAll('.floor-checkbox:checked');
        selectedFloorCheckboxes.forEach(checkbox => {
            params.append('selectedFloorRanges', checkbox.value);
        });
    }

    // 坪數範圍
    const hasMinAreaInput = minAreaInput && minAreaInput.value !== '';
    const hasMaxAreaInput = maxAreaInput && maxAreaInput.value !== '';

    if (hasMinAreaInput || hasMaxAreaInput) {
        const min = parseInt(minAreaInput.value);
        const max = parseInt(maxAreaInput.value);
        if (!isNaN(min) && minAreaInput.value !== '') params.append('minArea', min);
        if (!isNaN(max) && maxAreaInput.value !== '') params.append('maxArea', max);
    } else {
        const selectedAreaCheckboxes = Array.from(document.querySelectorAll('input[name="areaSize"][type="checkbox"]:checked'));
        if (!areaNoLimitRadio.checked && selectedAreaCheckboxes.length > 0) {
            selectedAreaCheckboxes.forEach(checkbox => {
                selectedAreaCheckboxes.forEach(checkbox => {
                    params.append('areaRanges', checkbox.value);
                });
            });
        }
    }


    // 特色 (多選)
    // 修正: 假設 HTML name 是 feature_group
    const selectedFeatures = Array.from(document.querySelectorAll('.filter-group input[name="feature_group"]:checked'))
        .filter(checkbox => checkbox.id !== 'featureNoLimit')
        .map(checkbox => checkbox.value);
    if (selectedFeatures.length > 0) {
        params.append('features', selectedFeatures.join(','));
    }

    // 設施 (多選)
    // 修正: 假設 HTML name 是 facilities_group
    const selectedFacilities = Array.from(document.querySelectorAll('.filter-group input[name="facilities_group"]:checked'))
        .filter(checkbox => checkbox.id !== 'facilitiesNoLimit')
        .map(checkbox => checkbox.value);
    if (selectedFacilities.length > 0) {
        params.append('facilities', selectedFacilities.join(',')); // 參數名改回 facilities
    }

    // 設備 (多選)
    // 修正: 假設 HTML name 是 equipment_group
    const selectedEquipments = Array.from(document.querySelectorAll('.filter-group input[name="equipment_group"]:checked'))
        .filter(checkbox => checkbox.id !== 'equipmentNoLimit')
        .map(checkbox => checkbox.value);
    if (selectedEquipments.length > 0) {
        params.append('equipments', selectedEquipments.join(','));
    }

    // 分頁和排序
    params.append('pageNumber', pageNumber);
    params.append('pageSize', pageSize);
    params.append('sortField', currentSortField);
    params.append('sortOrder', currentSortOrder);

    return params;
}

// --- 執行搜尋函數 ---

async function performSearch() {
    // 顯示載入中的動畫或文字 (請確保 searchResultsContainer 已定義)
    if (searchResultsContainer) {
        searchResultsContainer.innerHTML = '<div class="text-center my-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div><p class="mt-2">載入中，請稍候...</p></div>';
    } else {
        console.error("searchResultsContainer 元素未找到！");
        return; // 終止執行，避免錯誤
    }

    // 隱藏沒有結果的提示 (如果 $noResultsMessage 是 jQuery 對象)
    if (typeof $noResultsMessage !== 'undefined' && $noResultsMessage.hide) {
        $noResultsMessage.hide();
    }


    try {
        const queryParams = getFilterParams(); // 獲取所有篩選參數

        //更新url
        const queryString = queryParams.toString();
        const newUrl = window.location.pathname + '?' + queryString;
        window.history.pushState({}, '', newUrl);
        //console.log('完整的搜尋 URL 已保存到 localStorage:', newUrl);

        //存取url，更新猜你喜歡房源
        localStorage.setItem('lastFullSearchUrl', newUrl);
        const paramsForRecommendation = {};
        for (const [key, value] of queryParams.entries()) {
            paramsForRecommendation[key] = value; // 先複製所有參數
        }
        //delete paramsForRecommendation.keyword; // 移除關鍵字
        delete paramsForRecommendation.pageNumber; // 移除分頁信息
        delete paramsForRecommendation.pageSize;   // 移除分頁信息

        localStorage.setItem('lastSearchCriteria', JSON.stringify(paramsForRecommendation));

        // 發送 AJAX 請求
        // 注意：這裡假設後端 URL 仍是 /api/Tenant/Search/list
        const response = await fetch(`/api/Tenant/Search/list?${queryParams.toString()}`);
        //console.log("搜索", queryParams.toString())
        //console.log("結果", response)
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // 假設後端返回的 JSON 結構為 { totalCount: N, properties: [...], currentPage: X, totalPages: Y }
        const data = await response.json();
        //console.log("API 返回的數據:", data); // 方便除錯

        // 清空舊的搜尋結果和分頁
        searchResultsContainer.innerHTML = '';
        if (paginationContainer) {
            paginationContainer.innerHTML = '';
        }

        // 更新總結果數量
        if (totalResultsCountSpan) {
            totalResultsCountSpan.text = `已為您找到 ${data.totalCount || 0} 閒房屋`;
        } else {
            console.warn("Element with ID 'totalResultsCount' not found.");
        }

        // 渲染房源卡片
        if (data.properties && data.properties.length > 0) {
            data.properties.forEach(property => {
                // --- 數據處理與格式化 (根據 SearchViewModel.cs) ---
                const propertyId = property.propertyId;
                const title = property.title || '無標題';
                const addressLine = property.addressLine || '地址不詳';
                const roomCount = property.roomCount !== undefined && property.roomCount !== null ? property.roomCount : 0;
                const livingRoomCount = property.livingRoomCount;
                const bathroomCount = property.bathroomCount;
                const currentFloor = property.currentFloor;
                const totalFloors = property.totalFloors;
                const area = property.area !== undefined && property.area !== null ? property.area : 0;
                const monthlyRent = property.monthlyRent;
                const features = Array.isArray(property.features) ? property.features : [];
                const imagePath = property.imagePath || '/images/apartment.jpg'; // 使用 property.ImagePath 並添加預設圖片
                const isFavorited = property.isFavorited;
                const publishedAt = property.publishedAt;

                // 日期格式化邏輯
                let formattedDate = '';
                if (publishedAt) {
                    const publishedDateObj = new Date(publishedAt);
                    const year = publishedDateObj.getFullYear();
                    const month = String(publishedDateObj.getMonth() + 1).padStart(2, '0');
                    const day = String(publishedDateObj.getDate()).padStart(2, '0');
                    formattedDate = `${year}-${month}-${day}`;
                }

                // 組合格局資訊
                const layoutInfo = `${roomCount}房${livingRoomCount}廳${bathroomCount}衛`;
                // 組合樓層資訊
                const floorInfo = `${currentFloor}/${totalFloors}樓`;

                const cardHtml = `
                    <div class="result-card">
                        <div class="img-placeholder">
                            <img src="${imagePath}" alt="${title}">
                        </div>
                        <div class="card-body">
                            <h5 class="card-title d-flex align-items-center">
                                <a href="/Property/${propertyId}" class="text-truncate me-2">${title}</a>
                            </h5>
                            <div class="location-info">
                                <i class="bi bi-geo-alt-fill"></i> ${addressLine}
                            </div>
                            <div class="details-info">
                                <i class="bi bi-house"></i> ${layoutInfo} | ${floorInfo} | ${area}坪
                            </div>
                            <div class="tags d-flex flex-wrap mt-2">
                                ${features.map(feature => `<span class="badge">${feature}</span>`).join('')}
                            </div>
                        </div>
                        <div class="d-flex flex-column align-items-end ms-auto">
                           <span data-property-id="${propertyId}"></span>
                            <div class="price">NT$ ${monthlyRent !== undefined && monthlyRent !== null ? monthlyRent.toLocaleString() : '價格面議'}</div>
                            <div class="last-updated">發布於: ${formattedDate}</div>
                        </div>
                    </div>
                `;
                searchResultsContainer.insertAdjacentHTML('beforeend', cardHtml);
            });

         
            //if (typeof $propertyResultsContainer !== 'undefined' && $propertyResultsContainer.length > 0) {
            //    $propertyResultsContainer.off('click', '.favorite-icon').on('click', '.favorite-icon', function () {
            //        const clickedPropertyId = $(this).data('property-id');
            //        // $(this) 就是 favorite-icon span 本身
            //        toggleFavorite(clickedPropertyId, this);
            //    });
            //} else {
            //    console.warn("$propertyResultsContainer 元素未找到或未定義，收藏按鈕事件無法綁定。");
            //}


        } else {
            searchResultsContainer.innerHTML = '<p class="text-center text-muted mt-5">沒有找到符合條件的房源。</p>';
            // 顯示沒有結果的提示 (如果 $noResultsMessage 是 jQuery 對象)
            if (typeof $noResultsMessage !== 'undefined' && $noResultsMessage.show) {
                $noResultsMessage.show();
            }
        }

        // 渲染分頁 (假設 data 中有 currentPage 和 totalPages 屬性)
        if (data.currentPage !== undefined && data.totalPages !== undefined) {
            renderPagination(data.currentPage, data.totalPages);
        } else {
            console.warn("分頁數據 (currentPage, totalPages) 在 API 響應中未找到。");
            if (paginationContainer) {
                paginationContainer.innerHTML = ''; // 清空分頁
            }
        }

    } catch (error) {
        console.error('搜尋失敗:', error);
        if (searchResultsContainer) {
            searchResultsContainer.innerHTML = '<p class="text-center text-danger mt-5">載入搜尋結果失敗，請稍後再試。</p>';
        }
        if (totalResultsCountSpan) {
            totalResultsCountSpan.textContent = '載入失敗';
        }
        if (paginationContainer) {
            paginationContainer.innerHTML = ''; // 清空分頁
        }
        // 顯示錯誤提示 (如果 $noResultsMessage 是 jQuery 對象)
        if (typeof $noResultsMessage !== 'undefined' && $noResultsMessage.show) {
            $noResultsMessage.text('載入資料失敗，請稍後再試。').show();
        }
    }
}


// --- 渲染分頁導航函數 (保持不變) ---
function renderPagination(currentPage, totalPages) {
    if (totalPages <= 1) {
        $paginationContainer.empty(); // 只有一頁或沒有結果時不顯示分頁
        return;
    }

    let paginationHtml = '<ul class="pagination justify-content-center">'; // 確保 ul 標籤包裹分頁

    // 上一頁按鈕
    paginationHtml += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${currentPage - 1}" aria-label="Previous">
            <span aria-hidden="true">&laquo;</span>
        </a>
    </li>`;

    // 頁碼按鈕
    const maxPagesToShow = 10; // 最多顯示10個頁碼按鈕
    let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

    // 調整 startPage 和 endPage 以確保顯示 maxPagesToShow 個按鈕
    if (endPage - startPage + 1 < maxPagesToShow) {
        startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
        paginationHtml += `<li class="page-item ${i === currentPage ? 'active' : ''}">
            <a class="page-link" href="#" data-page="${i}">${i}</a>
        </li>`;
    }

    // 下一頁按鈕
    paginationHtml += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${currentPage + 1}" aria-label="Next">
            <span aria-hidden="true">&raquo;</span>
        </a>
    </li>`;

    paginationHtml += '</ul>'; // 結束 ul 標籤

    $paginationContainer.html(paginationHtml);

    // 為分頁按鈕添加點擊事件監聽器 (使用事件委派)
    $paginationContainer.off('click', '.page-link').on('click', '.page-link', function (e) {
        e.preventDefault();
        const newPage = parseInt($(this).data('page'));
        if (!isNaN(newPage) && newPage > 0 && newPage <= totalPages && newPage !== currentPage) {
            pageNumber = newPage;
            performSearch(); // 重新執行搜尋
        }
    });
}


async function loadDistricts(cityCode) {
    try {
        // 先清空行政區複選框容器
        $districtCheckboxes.empty();
        $districtCheckboxes.append('<div class="text-muted w-100">載入中...</div>');

        const response = await fetch(`/api/Common/District/listByCityCode?cityCode=${cityCode}`);
        if (!response.ok) {
            throw new Error('Failed to load districts');
        }
        const districts = await response.json();

        $districtCheckboxes.empty(); // 清空載入中訊息

        // 為每個行政區生成複選框按鈕
        districts.forEach(district => {
            const checkboxHtml = `
                <input type="checkbox" class="btn-check district-checkbox" id="district_${district.districtCode}" value="${district.districtCode}" autocomplete="off">
                <label class="btn btn-outline-secondary btn-sm me-1 mb-1" for="district_${district.districtCode}">${district.districtName}</label>
            `;
            $districtCheckboxes.append(checkboxHtml);
        });

        // 預設選中「不限」，並觸發其 change 事件，讓通用監聽器處理後續邏輯和樣式
        // 這會確保行政區篩選器在載入後有一個明確的初始狀態
        $('#districtNoLimit').prop('checked', true);
        $('label[for="districtNoLimit"]').addClass('active'); // 確保其 label 初始就是 active


    } catch (error) {
        console.error('載入行政區失敗:', error);
        $districtCheckboxes.empty().append('<div class="text-danger">載入失敗</div>');
    }
}


// --- 初始化城市和行政區函數 (適應新的下拉選單結構) ---
async function loadCitiesAndDistricts() {
    try {
        const response = await fetch(`/api/Common/City/list`);
        if (!response.ok) {
            throw new Error('Failed to load cities');
        }
        const cities = await response.json();

        // 建立城市代碼到名稱的映射
        cityCodeToNameMap = {};
        cities.forEach(city => {
            cityCodeToNameMap[city.cityCode] = city.cityName;
        });

        // 為城市下拉菜單中的每個項目添加點擊事件
        $cityDropdownItems.on('click', function (e) {
            e.preventDefault();
            const clickedCityCode = $(this).data('city-code');
            const clickedCityName = $(this).text().trim(); // 獲取顯示的城市名稱

            // 更新按鈕文本
            $cityDropdownBtn.text(clickedCityName);

            // 移除所有 active 類別並為當前點擊的項目添加 active
            $cityDropdownItems.removeClass('active');
            $(this).addClass('active');

            // 更新全局城市代碼
            cityCode = clickedCityCode;
            pageNumber = 1; // 城市改變時重置到第一頁
            loadDistricts(cityCode);
        });

        // 首次載入行政區（基於初始的 cityCode）
        await loadDistricts(cityCode);

    } catch (error) {
        console.error('載入城市失敗:', error);
        // 可以顯示錯誤訊息或預設值
        $cityDropdownBtn.text('載入城市失敗');
        cityCode = '';
    }
}


// --- 處理搜尋按鈕點擊事件 ---
$searchButton.on('click', function () {
    const currentCityText = $('#cityDropdownBtn').text().trim();
    const currentKeyword = $keywordSearch.val() ? $keywordSearch.val().trim() : ''; // 確保 $keywordSearch 已定義

    let historyItem = '';
    // 判斷城市是否有效
    if (currentCityText && currentCityText !== "請選擇城市" && currentKeyword.length>0) {
        historyItem += currentCityText;
        historyItem += (historyItem ? ' ' : '') + currentKeyword;
    }

    // 只有當實際有有效的搜尋內容時才添加到歷史紀錄
    if (historyItem) {
        addSearchHistory(historyItem); // 調用 addSearchHistory 函數
    }

    
        pageNumber = 1; // 搜尋按鈕點擊時重置到第一頁
        performSearch();

});

// 處理關鍵字輸入框的 Enter 鍵事件
$keywordSearch.on('keypress', function (e) {
    if (e.which === 13) { // 檢查是否是 Enter 鍵
        e.preventDefault(); // 阻止表單提交的預設行為
        pageNumber = 1; // 重置到第一頁
        performSearch();
    }
});



// --- 處理所有篩選條件的改變事件 (保持不變) ---
document.querySelectorAll('.filters-container .filter-group input[type="checkbox"], .filters-container .filter-group input[type="radio"], .filters-container .filter-group input[type="number"]').forEach(function (input) {
    input.addEventListener('change', function () {
        const filterGroup = this.closest('.filter-group');

        // 判斷當前 filterGroup 是否包含數字輸入框 (以此來區分處理邏輯)
        const hasNumericInput = filterGroup.querySelector('input[type="number"]') !== null;
        if (hasNumericInput) {
            let noLimitRadio, targetCheckboxesName, minInput, maxInput;

            // 判斷是租金還是坪數群組，並設定對應的變數
            if (filterGroup.contains(minRentInput)) { // 判斷是否為租金篩選群組
                noLimitRadio = rentNoLimitRadio;
                targetCheckboxesName = 'rent_range';
                minInput = minRentInput;
                maxInput = maxRentInput;
            } else if (filterGroup.contains(minAreaInput)) { // 判斷是否為坪數篩選群組
                noLimitRadio = areaNoLimitRadio;
                targetCheckboxesName = 'areaSize';
                minInput = minAreaInput;
                maxInput = maxAreaInput;
            }

            // 確保找到了有效的篩選器組對應變數
            if (noLimitRadio) {
                // --- 處理數字輸入框的互動邏輯 ---
                if (this.type === 'number') {
                    if (minInput.value !== '' || maxInput.value !== '') {
                        if (noLimitRadio.checked) {
                            noLimitRadio.checked = false;
                        }
                        filterGroup.querySelectorAll(`input[name="${targetCheckboxesName}"][type="checkbox"]`).forEach(cb => {
                            if (cb.checked) {
                                cb.checked = false;
                            }
                        });
                    } else {
                        const anyCheckboxChecked = Array.from(filterGroup.querySelectorAll(`input[name="${targetCheckboxesName}"][type="checkbox"]:checked`)).some(cb => true);
                        if (!anyCheckboxChecked && !noLimitRadio.checked) {
                            noLimitRadio.checked = true;
                        }
                    }
                }
                // --- 處理 radio/checkbox 的互動邏輯 (針對租金/坪數) ---
                else if (this.name === targetCheckboxesName) {
                    if (this.id === noLimitRadio.id) {
                        if (this.checked) {
                            minInput.value = '';
                            maxInput.value = '';
                            filterGroup.querySelectorAll(`input[name="${targetCheckboxesName}"][type="checkbox"]`).forEach(cb => {
                                if (cb.checked) {
                                    cb.checked = false;
                                }
                            });
                        }
                    } else {
                        if (this.checked) {
                            minInput.value = '';
                            maxInput.value = '';
                            if (noLimitRadio.checked) {
                                noLimitRadio.checked = false;
                            }
                        } else {
                            const anyCheckboxChecked = Array.from(filterGroup.querySelectorAll(`input[name="${targetCheckboxesName}"][type="checkbox"]:checked`)).some(cb => true);
                            const hasManualInput = minInput.value !== '' || maxInput.value !== '';
                            if (!anyCheckboxChecked && !hasManualInput) {
                                if (!noLimitRadio.checked) {
                                    noLimitRadio.checked = true;
                                }
                            }
                        }
                    }
                }
            }
            // --- 在租金/坪數群組的所有互斥邏輯處理完畢後，統一更新 active 樣式 ---
            updateFilterGroupActiveStates(filterGroup);
        }
        // --- 處理沒有數字輸入框的其他篩選器群組 (例如：格局、衛浴、樓層、特色、設施、設備等) ---
        else {
            // 首先，處理所有 radio 的 active 狀態 (因為 radio 會自動互斥，只要更新樣式即可)
            if (this.type === 'radio') {
                filterGroup.querySelectorAll(`input[type="radio"][name="${this.name}"]`).forEach(radioInput => {
                    const labelForRadio = document.querySelector(`label[for="${radioInput.id}"]`);
                    if (labelForRadio) {
                        if (radioInput.checked) {
                            labelForRadio.classList.add('active');
                        } else {
                            labelForRadio.classList.remove('active');
                        }
                    }
                });

                // 如果點擊了 radio，並且這個 radio 所屬的組內也有 checkbox
                // 且點擊的 radio 是「不限」類型的 radio (假設其 ID 或 class 標識為「不限」)
                // 那麼需要取消所有同組 checkbox 的選中狀態和樣式
                if (this.id.includes('NoLimit') || this.id.includes('Unlimited')) { // 假設「不限」radio 的 ID 包含 'NoLimit' 或 'Unlimited'
                    filterGroup.querySelectorAll(`input[type="checkbox"]`).forEach(cb => {
                        if (cb.checked) {
                            cb.checked = false;
                            document.querySelector(`label[for="${cb.id}"]`)?.classList.remove('active');
                        }
                    });
                }
            }
            // 處理 checkbox 的 active 狀態及與「不限」radio 的互斥
            else if (this.type === 'checkbox') {
                const labelForCheckbox = document.querySelector(`label[for="${this.id}"]`);
                if (labelForCheckbox) {
                    if (this.checked) {
                        labelForCheckbox.classList.add('active'); // 點擊的 checkbox 變亮

                        // 檢查同組是否有「不限」radio，如果有且被選中，則取消選中並變暗
                        const noLimitRadioInGroup = filterGroup.querySelector(`input[type="radio"][name="${this.name}"][id*="NoLimit"], input[type="radio"][name="${this.name}"][id*="Unlimited"]`);
                        if (noLimitRadioInGroup && noLimitRadioInGroup.checked) {
                            noLimitRadioInGroup.checked = false;
                            document.querySelector(`label[for="${noLimitRadioInGroup.id}"]`)?.classList.remove('active');
                        }

                    } else {
                        labelForCheckbox.classList.remove('active'); // 點擊的 checkbox 變暗

                        // 如果這個 checkbox 被取消選中，且同組所有其他 checkbox 都未選中
                        // 那麼檢查「不限」radio 是否應該被激活
                        const anyOtherCheckboxChecked = Array.from(filterGroup.querySelectorAll(`input[type="checkbox"][name="${this.name}"]:checked`)).some(cb => true);

                        // 找到同組的「不限」radio
                        const noLimitRadioInGroup = filterGroup.querySelector(`input[type="radio"][name="${this.name}"][id*="NoLimit"], input[type="radio"][name="${this.name}"][id*="Unlimited"]`);

                        if (!anyOtherCheckboxChecked && noLimitRadioInGroup && !noLimitRadioInGroup.checked) {
                            // 如果沒有其他 checkbox 被選中，並且存在「不限」radio 且未被選中，則激活「不限」
                            noLimitRadioInGroup.checked = true;
                            document.querySelector(`label[for="${noLimitRadioInGroup.id}"]`)?.classList.add('active');
                        }
                    }
                }
            }
        }
        pageNumber = 1; // 篩選條件改變時重置到第一頁
        performSearch();
    });
});


// --- 輔助函數：根據 filterGroup 內 input 的 checked 狀態更新 active 類 (保持不變) ---
function updateFilterGroupActiveStates(filterGroup) {
    // 處理數字輸入框旁邊的「不限」選項
    const noLimitRadio = filterGroup.querySelector('input[type="radio"][id*="NoLimit"], input[type="radio"][id*="Unlimited"]');
    if (noLimitRadio) {
        const labelForNoLimit = document.querySelector(`label[for="${noLimitRadio.id}"]`);
        if (labelForNoLimit) {
            if (noLimitRadio.checked) {
                labelForNoLimit.classList.add('active');
            } else {
                labelForNoLimit.classList.remove('active');
            }
        }
    }

    // 處理 checkbox 的 active 狀態
    filterGroup.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
        const labelForCheckbox = document.querySelector(`label[for="${checkbox.id}"]`);
        if (labelForCheckbox) {
            if (checkbox.checked) {
                labelForCheckbox.classList.add('active');
            } else {
                labelForCheckbox.classList.remove('active');
            }
        }
    });

    // 處理 radio (非「不限」類型) 的 active 狀態
    filterGroup.querySelectorAll('input[type="radio"]:not([id*="NoLimit"]):not([id*="Unlimited"])').forEach(radio => {
        const labelForRadio = document.querySelector(`label[for="${radio.id}"]`);
        if (labelForRadio) {
            if (radio.checked) {
                labelForRadio.classList.add('active');
            } else {
                labelForRadio.classList.remove('active');
            }
        }
    });
}



// --- 處理排序按鈕點擊事件 (保持不變) ---
sortButtons.forEach(button => {
    button.addEventListener('click', function () {
        const clickedSortField = this.dataset.sortField;
        let clickedSortOrder = this.dataset.sortOrder; // 獲取當前數據屬性值
        const clickedIcon = this.querySelector('i');

        // 移除所有按鈕的 active 狀態和重置圖標
        sortButtons.forEach(btn => {
            btn.classList.remove('active');
            const icon = btn.querySelector('i');
            if (icon) {
                // 重置所有非當前選中按鈕的圖標為預設向上 (降序)
                icon.classList.remove('bi-sort-down-alt');
                icon.classList.add('bi-sort-up');
                btn.dataset.sortOrder = 'desc'; // 重置數據屬性
            }
        });

        // 為當前點擊的按鈕添加 active 狀態
        this.classList.add('active');

        // 切換圖標方向和 data-sort-order
        if (currentSortField === clickedSortField) { // 如果點擊的是當前已選中的排序方式
            if (clickedSortOrder === 'desc') {
                clickedIcon.classList.remove('bi-sort-up');
                clickedIcon.classList.add('bi-sort-down-alt');
                this.dataset.sortOrder = 'asc';
                currentSortOrder = 'asc';
            } else {
                clickedIcon.classList.remove('bi-sort-down-alt');
                clickedIcon.classList.add('bi-sort-up');
                this.dataset.sortOrder = 'desc';
                currentSortOrder = 'desc';
            }
        } else { // 如果點擊的是新的排序方式
            clickedIcon.classList.remove('bi-sort-up');
            clickedIcon.classList.add('bi-sort-down-alt'); // 預設為降序 (最新發布/租金高/面積大)
            this.dataset.sortOrder = 'asc'; // data-sort-order 設為 'asc'，但圖標顯示降序，這點與您的原始邏輯保持一致
            currentSortOrder = 'asc'; // 更新全局排序順序
            currentSortField = clickedSortField; // 更新全局排序字段
        }

        pageNumber = 1; // 排序改變時重置到第一頁
        performSearch(); // 重新執行搜尋
    });
});


//// 收藏功能 (保持不變)
//async function toggleFavorite(propertyId, iconElement) {
//    try {
//        const isFavorited = iconElement.classList.contains('bi-heart-fill');
//        const url = `/api/User/Favorite/${propertyId}`; // 後端 API 路徑

//        const response = await fetch(url, {
//            method: isFavorited ? 'DELETE' : 'POST', // 如果已收藏則刪除，否則新增
//            headers: {
//                'Content-Type': 'application/json',
//                // 如果需要認證，請在這裡添加 Token
//                // 'Authorization': 'Bearer YOUR_TOKEN_HERE'
//            },
//            body: JSON.stringify({ propertyId: propertyId }) // 根據您的 API 需求調整 body
//        });

//        if (!response.ok) {
//            const errorData = await response.json();
//            throw new Error(errorData.message || '收藏操作失敗');
//        }

//        // 切換圖標樣式
//        if (isFavorited) {
//            iconElement.classList.remove('bi-heart-fill');
//            iconElement.classList.add('bi-heart');
//        } else {
//            iconElement.classList.remove('bi-heart');
//            iconElement.classList.add('bi-heart-fill');
//        }
//        console.log(`收藏狀態已更新為：${!isFavorited}`);

//    } catch (error) {
//        console.error('更新收藏狀態失敗:', error);
//        alert('收藏操作失敗，請稍後再試。');
//    }
//}

async function applyUrlParamsToUI() {
    isApplyingUrlParams = true; // 設置旗標，阻止事件監聽器中的自動觸發

    const urlParams = new URLSearchParams(window.location.search);

    const keyword = urlParams.get('keyword');
    if (keyword) {
        $keywordSearch.val(keyword); // 假設關鍵字輸入框的ID是 keywordSearch
    } else {
        $keywordSearch.val(''); // 如果URL沒有關鍵字，清空輸入框
    }

     cityCode = urlParams.get('cityCode');
    const $targetCityItem = $(`.dropdown-menu .custom-city-list .dropdown-item[data-city-code="${cityCode}"]`);
    $cityDropdownItems.removeClass('active');
    if (cityCode && $targetCityItem.length) {
        $cityDropdownBtn.text($targetCityItem.text().trim()); // 將按鈕文本設置為城市名稱
        $targetCityItem.addClass('active'); // 設置對應城市為active
    } 
    else {
        const $TPECityItem = $(`.dropdown-menu .custom-city-list .dropdown-item[data-city-code="TPE"]`);
        if ($TPECityItem) {
            $TPECityItem.addClass('active');
            $cityDropdownBtn.text($TPECityItem.text().trim());
        }
        else {
            const firstCityItem = $cityDropdownItems.first();
            if (firstCityItem.length > 0) {
                firstCityItem.addClass('active');
                $cityDropdownBtn.text(firstCityItem.text().trim());
            } else {
                $cityDropdownBtn.text('選擇城市');
            }
}
       
    }
    await loadCitiesAndDistricts()
    await loadDistricts(cityCode)
    const districtCode = urlParams.getAll('districtCode'); // 區域通常是多選，用 getAll，假設參數名是 'districtCode'
    $('#districtNoLimit').prop('checked', false);
    $('.district-checkbox').prop('checked', false);
    $('label[for="districtNoLimit"]').removeClass('active');
    if (districtCode.length > 0) {
        // 根據 URL 參數選中對應的區域複選框
        districtCode.forEach(Selected => {
            $('.district-checkbox').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    } else {
        $('#districtNoLimit').prop('checked', true);
    }

    const minRent = urlParams.get('minRent');
    const maxRent = urlParams.get('maxRent');
    const rentRanges = urlParams.getAll('rentRanges');
    minRentInput.value = '';
    maxRentInput.value = '';
    $('input[name="rent_range"][type="checkbox"]').prop('checked', false);
    $('#rentNoLimit').prop('checked', false);


    if (minRent || maxRent) {
        minRentInput.value = minRent || ''; // 更新最低租金輸入框
        maxRentInput.value = maxRent || ''; // 更新最高租金輸入框
    }
    else if (rentRanges.length > 0) {
        rentRanges.forEach(Selected => {
            $('input[name="rent_range"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    }
    else {
        $('#rentNoLimit').prop('checked', true);
    }

    const roomCounts = urlParams.getAll('roomCounts');
    $('input[name="layout_group"][type="checkbox"]').prop('checked', false);
    $('#layoutNoLimit').prop('checked', false);
    if (roomCounts.length > 0) {
        // 根據 URL 參數選中對應的區域複選框
        roomCounts.forEach(Selected => {
            $('input[name="layout_group"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    } else {
        $('#layoutNoLimit').prop('checked', true);
    }

    const bathroomCounts = urlParams.getAll('bathroomCounts');
    $('input[name="bathroom_group"][type="checkbox"]').prop('checked', false);
    $('#bathroomNoLimit').prop('checked', false);
    if (bathroomCounts.length > 0) {
        // 根據 URL 參數選中對應的區域複選框
        bathroomCounts.forEach(Selected => {
            $('input[name="bathroom_group"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    } else {
        $('#bathroomNoLimit').prop('checked', true);
    }

    const selectedFloorRanges = urlParams.getAll('selectedFloorRanges');
    $('input[name="floor"][type="checkbox"]').prop('checked', false);
    $('#floorNoLimit').prop('checked', false);
    if (selectedFloorRanges.length>0) {
        selectedFloorRanges.forEach(Selected => {
            $('input[name="floor"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    } else {
        $('#floorNoLimit').prop('checked', true);
    }

    const minArea = urlParams.get('minArea');
    const maxArea = urlParams.get('maxArea');
    const areaRanges = urlParams.getAll('areaRanges');
    $('input[name="areaSize"][type="checkbox"]').prop('checked', false);
    $('#areaNoLimit').prop('checked', false);
    minAreaInput.value=''; // 更新最低租金輸入框
    maxAreaInput.value=''; // 更新最高租金輸入框
    if (minArea || maxArea) {
        minAreaInput.value=minArea || ''; // 更新最低租金輸入框
        maxAreaInput.value=maxArea || ''; // 更新最高租金輸入框     
    } else if (areaRanges.length > 0) {
        areaRanges.forEach(Selected => {
            $('input[name="areaSize"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    }
    else {
        $('#areaNoLimit').prop('checked', true);
    }

    const features = urlParams.getAll('features');
    $('input[name="feature_group"][type="checkbox"]').prop('checked', false);
    $('#featureNoLimit').prop('checked', false);

    if (features.length > 0) {
        features.forEach(Selected => {
            $('input[name="feature_group"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    }
    else {
        $('#featureNoLimit').prop('checked', true);
    }

    const facilities = urlParams.getAll('facilities');
    $('input[name="facilities_group"][type="checkbox"]').prop('checked', false);
    $('#facilitiesNoLimit').prop('checked', false);

    if (facilities.length > 0) {
        facilities.forEach(Selected => {
            $('input[name="facilities_group"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    }
    else {
        $('#facilitiesNoLimit').prop('checked', true);
    }

    const equipments = urlParams.getAll('equipments');
    $('input[name="equipment_group"][type="checkbox"]').prop('checked', false);
    $('#equipmentNoLimit').prop('checked', false);
    if (equipments.length > 0) {
        equipments.forEach(Selected => {
            $('input[name="equipment_group"][type="checkbox"]').filter(`[value="${Selected}"]`).prop('checked', true);
        });
    }
    else {
        $('#equipmentNoLimit').prop('checked', true);
    }

    const sortField = urlParams.get('sortField');
    const sortOrder = urlParams.get('sortOrder');
    sortButtons.forEach(btn => {
        btn.classList.remove('active');
        const icon = btn.querySelector('i');
        if (icon) {
            // 重置所有非當前選中按鈕的圖標為預設向上 (降序)
            icon.classList.remove('bi-sort-down-alt');
            icon.classList.add('bi-sort-up');
            btn.dataset.sortOrder = 'desc'; // 重置數據屬性
        }
    });

    if (sortField && sortOrder) {
        const $targetSortBtn = $(`.sort-btn[data-sort-field="${sortField}"]`);
        $targetSortBtn.addClass('active');
        $targetSortBtn.find('i').removeClass('bi-sort-up').addClass(sortOrder === 'desc' ? 'bi-sort-up' : 'bi-sort-down-alt');
    }
    else if (sortField) {
        const $targetSortBtn = $(`.sort-btn[data-sort-field="${sortField}"]`);
        $targetSortBtn.addClass('active');
    }
    else if (sortOrder) {
        $(`.sort-btn[data-sort-field="${currentSortField}"]`).addClass('active');
        $targetSortBtn.find('i').removeClass('bi-sort-up').addClass(sortOrder === 'desc' ? 'bi-sort-up' : 'bi-sort-down-alt');
    }
    else {
        $(`.sort-btn[data-sort-field="${currentSortField}"]`).addClass('active');
    }

    if (urlParams.get('pageSize')) {
        pageSize = urlParams.get('pageSize');
    }
    
    if (urlParams.get('pageNumber')) {
        pageNumber = urlParams.get('pageNumber');
    }
    //console.log('--- 呼叫 applyUrlParamsToUI 中 ---', cityCode); // 觀察點 C
    isApplyingUrlParams = false; // UI 設置完成，解除旗標
}

document.addEventListener('DOMContentLoaded', async () => {
    $(document).on('change', '#districtNoLimit, .district-checkbox', function () {
        const clickedElement = $(this);
        const isNoLimitRadio = clickedElement.attr('id') === 'districtNoLimit';

        if (isNoLimitRadio) {
            // 如果點擊的是「不限」radio
            if (clickedElement.is(':checked')) {
                // 確保「不限」radio 被選中並 active
                $('#districtNoLimit').prop('checked', true);
                $('label[for="districtNoLimit"]').addClass('active');

                // 取消所有其他行政區 checkbox 的選中狀態和 active 樣式
                $('.district-checkbox').prop('checked', false).each(function () {
                    $('label[for="' + $(this).attr('id') + '"]').removeClass('active');
                });
            }
        } else { // 如果點擊的是具體的行政區 checkbox (.district-checkbox)
            // 當點擊任何一個行政區 checkbox 時，取消「不限」radio 的選中狀態和 active 樣式
            if ($('#districtNoLimit').is(':checked')) {
                $('#districtNoLimit').prop('checked', false);
                $('label[for="districtNoLimit"]').removeClass('active');
            }

            // 更新當前被點擊的行政區 checkbox 的 active 樣式
            const labelForCheckbox = $('label[for="' + clickedElement.attr('id') + '"]');
            if (clickedElement.is(':checked')) {
                labelForCheckbox.addClass('active');
            } else {
                labelForCheckbox.removeClass('active');
            }

            // 檢查是否所有行政區 checkbox 都被取消選中
            const anyDistrictChecked = $('.district-checkbox:checked').length > 0;

            if (!anyDistrictChecked) {
                // 如果沒有任何行政區 checkbox 被選中，則自動勾選「不限」radio
                $('#districtNoLimit').prop('checked', true);
                $('label[for="districtNoLimit"]').addClass('active');


            }
        }



        pageNumber = 1; // 篩選條件改變時重置到第一頁
        performSearch(); // 執行搜尋
    });

    if (window.location.search) {
        //console.log('--- 呼叫 applyUrlParamsToUI 開始 ---', cityCode); // 觀察點 A
        await applyUrlParamsToUI()
        //console.log('--- applyUrlParamsToUI 完成，檢查 UI ---',cityCode); // 觀察點 B (你目前的斷點位置)

    }
    else {
        loadCitiesAndDistricts()
    }

    loadSearchHistory();
   
    performSearch(); // 執行搜尋
});
   
  
