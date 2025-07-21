// Search.js
document.addEventListener('DOMContentLoaded', function () {

    // 獲取 DOM 元素
    const searchResultsContainer = document.getElementById('searchResultsContainer');
    const paginationContainer = document.getElementById('paginationContainer');
    const totalResultsCountSpan = document.getElementById('totalResultsCount');
    const sortButtons = document.querySelectorAll('.sort-btn');

    // 新增的 DOM 元素
    const citySelect = document.getElementById('citySelect');
    const keywordSearchInput = document.getElementById('keywordSearch');
    const searchButton = document.getElementById('searchButton');
    const districtCheckboxesContainer = document.getElementById('districtCheckboxes');
    const districtNoLimitRadio = document.getElementById('districtNoLimit');
    const districtNoLimitLabel = document.querySelector('label[for="districtNoLimit"]');



    // 當前的搜尋狀態
    let currentSortField = "PublishedAt";
    let currentSortOrder = "desc";
    let currentPageNumber = 1;
    const pageSize = 6; // 每頁顯示數量

    // 新增的篩選狀態變數
    let currentCityCode = ''; // 儲存當前選中的城市代碼
    let currentSelectedDistricts = []; // 儲存當前選中的行政區


    // 為動態生成的收藏按鈕添加事件監聽器 (使用事件委派)
    if (searchResultsContainer) {
        searchResultsContainer.addEventListener('click', function (event) {
            const favoriteIcon = event.target.closest('.favorite-icon');
            if (favoriteIcon) {
                const propertyId = favoriteIcon.dataset.propertyId;
                toggleFavorite(propertyId, favoriteIcon); // 調用處理收藏邏輯的函數
            }
        });
    }

    // --- 輔助函數：獲取所有選中的篩選條件 ---
    function getFilterParams() {
        const params = new URLSearchParams();

        // 關鍵字
        if (keywordSearchInput.value.trim() !== '') {
            params.append('keyword', keywordSearchInput.value.trim());
        }

        // 城市 (只傳遞城市代碼，如果已選中且不是「請選擇城市」的空值)
        // 注意：這裡您在前端用 currentCityCode 變數來追蹤，但最終還是從 select.value 獲取確保同步
        if (citySelect.value && citySelect.value !== '') { // 確保不是空選項的值
            params.append('cityCode', citySelect.value); // 使用 citySelect.value
        }

        // 行政區 (多選)
        // 只有當「不限」未被選中時才傳遞行政區參數
        if (!districtNoLimitRadio.checked && currentSelectedDistricts.length > 0) {
            currentSelectedDistricts.forEach(districtCode => {
                params.append('districtCodes', districtCode); // *** 注意：這裡將參數名從 'districts' 改為 'districtCodes' 以便後端接收陣列 ***
            });
        }

        // 租金範圍
        const selectedRentRangeRadio = document.querySelector('input[name="rent"]:checked');
        const minRentInput = document.getElementById('minRentInput');
        const maxRentInput = document.getElementById('maxRentInput');

        if (selectedRentRangeRadio && selectedRentRangeRadio.id !== 'rentNoLimit') {
            const rangeValue = selectedRentRangeRadio.value.split('-');
            const min = parseInt(rangeValue[0]);
            const max = rangeValue[1] === 'max' ? '' : parseInt(rangeValue[1]); // 處理 'max'
            if (!isNaN(min)) params.append('minRent', min);
            if (max !== '') params.append('maxRent', max);
        } else if ((minRentInput && minRentInput.value !== '') || (maxRentInput && maxRentInput.value !== '')) { // 檢查元素是否存在
            const min = parseInt(minRentInput ? minRentInput.value : '');
            const max = parseInt(maxRentInput ? maxRentInput.value : '');
            if (!isNaN(min) && minRentInput.value !== '') params.append('minRent', min); // 確保只在輸入有值時才添加
            if (!isNaN(max) && maxRentInput.value !== '') params.append('maxRent', max); // 確保只在輸入有值時才添加
        }

        // 格局 (房間數，這裡假設為多選)
        const selectedRoomCheckboxes = document.querySelectorAll('.filter-group input[name="layout"]:checked');
        let roomCounts = [];
        selectedRoomCheckboxes.forEach(checkbox => {
            if (checkbox.id !== 'layoutNoLimit' && checkbox.value) {
                roomCounts.push(checkbox.value);
            }
        });
        if (roomCounts.length > 0) {
            params.append('roomCounts', roomCounts.join(',')); // 假設後端接收逗號分隔的字符串
        }

        // 衛浴 (新加的)
        const selectedBathroomCheckboxes = document.querySelectorAll('.filter-group input[name="bathroom"]:checked');
        let bathroomCounts = [];
        selectedBathroomCheckboxes.forEach(checkbox => {
            if (checkbox.id !== 'bathroomNoLimit' && checkbox.value) {
                bathroomCounts.push(checkbox.value);
            }
        });
        if (bathroomCounts.length > 0) {
            params.append('bathroomCounts', bathroomCounts.join(',')); // 假設後端接收逗號分隔的字符串
        }


        // 樓層 (單選，處理範圍)
        const selectedFloorRadio = document.querySelector('input[name="floor"]:checked');
        if (selectedFloorRadio && selectedFloorRadio.id !== 'floorNoLimit') {
            const rangeValue = selectedFloorRadio.value.split('-');
            const min = parseInt(rangeValue[0]);
            const max = rangeValue[1] === 'up' ? '' : parseInt(rangeValue[1]); // 處理 'up'
            if (!isNaN(min)) params.append('minFloor', min);
            if (max !== '') params.append('maxFloor', max);
        }

        // 坪數範圍
        const selectedAreaRangeRadio = document.querySelector('input[name="areaSize"]:checked');
        const minAreaInput = document.getElementById('minAreaInput');
        const maxAreaInput = document.getElementById('maxAreaInput');

        if (selectedAreaRangeRadio && selectedAreaRangeRadio.id !== 'areaNoLimit') {
            const rangeValue = selectedAreaRangeRadio.value.split('-');
            const min = parseInt(rangeValue[0]);
            const max = rangeValue[1] === 'max' ? '' : parseInt(rangeValue[1]); // 處理 'max'
            if (!isNaN(min)) params.append('minArea', min);
            if (max !== '') params.append('maxArea', max);
        } else if ((minAreaInput && minAreaInput.value !== '') || (maxAreaInput && maxAreaInput.value !== '')) { // 檢查元素是否存在
            const min = parseInt(minAreaInput ? minAreaInput.value : '');
            const max = parseInt(maxAreaInput ? maxAreaInput.value : '');
            if (!isNaN(min) && minAreaInput.value !== '') params.append('minArea', min);
            if (!isNaN(max) && maxAreaInput.value !== '') params.append('maxArea', max);
        }

        // 特色 (多選)
        const selectedFeatures = Array.from(document.querySelectorAll('.filter-group input[name="feature"]:checked'))
            .filter(checkbox => checkbox.id !== 'featureNoLimit')
            .map(checkbox => checkbox.value);
        if (selectedFeatures.length > 0) {
            params.append('features', selectedFeatures.join(',')); // 假設後端接收逗號分隔的字符串
        }

        // 設施 (多選)
        const selectedFacilities = Array.from(document.querySelectorAll('.filter-group input[name="Facilities"]:checked'))
            .filter(checkbox => checkbox.id !== 'facilities')
            .map(checkbox => checkbox.value);
        if (selectedFacilities.length > 0) {
            params.append('facilities', selectedFacilities.join(',')); // 假設後端接收逗號分隔的字符串
        }

        // 設備 (多選)
        const selectedEquipments = Array.from(document.querySelectorAll('.filter-group input[name="equipment"]:checked'))
            .filter(checkbox => checkbox.id !== 'equipmentNoLimit')
            .map(checkbox => checkbox.value);
        if (selectedEquipments.length > 0) {
            params.append('equipments', selectedEquipments.join(',')); // 假設後端接收逗號分隔的字符串
        }

        return params;
    }


    // --- 核心函數：執行搜尋並更新 UI ---
    async function performSearch() {
        try {
            const queryParams = getFilterParams(); // 獲取所有篩選參數

            // 添加排序和分頁參數
            queryParams.append('sortField', currentSortField);
            queryParams.append('sortOrder', currentSortOrder);
            queryParams.append('pageNumber', currentPageNumber);
            queryParams.append('pageSize', pageSize);

            // 發送 AJAX 請求
            const response = await fetch(`/api/Tenant/Search/list?${queryParams.toString()}`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json(); // 後端現在返回 JSON

            // 清空舊的搜尋結果和分頁
            searchResultsContainer.innerHTML = '';
            paginationContainer.innerHTML = '';

            // 更新總結果數量 (如果您的頁面上有這個元素)
            if (totalResultsCountSpan) {
                totalResultsCountSpan.textContent = `已為您找到 ${data.totalCount || 0} 閒房屋`;
            } else {
                console.warn("Element with ID 'totalResultsCount' not found.");
            }


            // 渲染房源卡片
            if (data.properties && data.properties.length > 0) {
                data.properties.forEach(property => {
                    // 日期格式化邏輯
                    const publishedDate = new Date(property.publishedAt);
                    const year = publishedDate.getFullYear();
                    const month = String(publishedDate.getMonth() + 1).padStart(2, '0'); // 月份從0開始，需要加1，並補零
                    const day = String(publishedDate.getDate()).padStart(2, '0');        // 日期補零
                    const formattedDate = `${year}-${month}-${day}`;

                    // 處理 images 字段，如果 imagePath 為 null 或空，則使用預設圖片
                    const imageUrl = property.imagePath && property.imagePath.trim() !== ''
                        ? property.imagePath
                        : '/images/default-house.jpg'; // 請替換為您的預設圖片路徑

                    const cardHtml = `
    <div class="result-card">
        <div class="img-placeholder">
            <img src="${imageUrl}" alt="${property.title || '房源圖片'}">
        </div>
        <div class="card-body">
            <h5 class="card-title d-flex align-items-center">
                <a href="/Property/Details/${property.propertyId}" class="text-truncate me-2">${property.title}</a>
            </h5>
            <div class="location-info">
                <i class="bi bi-geo-alt-fill"></i> ${property.addressLine}
            </div>
            <div class="details-info">
                <i class="bi bi-house"></i> ${property.roomCount}房${property.livingRoomCount}廳${property.bathroomCount}衛 | ${property.currentFloor}/${property.totalFloors}層 | ${property.area}坪
            </div>
            <div class="tags d-flex flex-wrap mt-2">
                ${property.features.map(feature => `<span class="badge">${feature}</span>`).join('')}
            </div>
        </div>
        <div class="d-flex flex-column align-items-end ms-auto">
           <span class="favorite-icon bi ${property.isFavorited ? 'bi-heart-fill' : 'bi-heart'}" data-property-id="${property.propertyId}"></span>
            <div class="price">NT$ ${property.monthlyRent.toLocaleString()}</div>
            <div class="last-updated">發布於: ${formattedDate}</div>
        </div>
    </div>
`;
                    searchResultsContainer.insertAdjacentHTML('beforeend', cardHtml);
                });

            } else {
                searchResultsContainer.innerHTML = '<p class="text-center text-muted mt-5">沒有找到符合條件的房源。</p>';
            }

            renderPagination(data.currentPage, data.totalPages);

        } catch (error) {
            console.error('搜尋失敗:', error);
            searchResultsContainer.innerHTML = '<p class="text-center text-danger mt-5">載入搜尋結果失敗，請稍後再試。</p>';
        }
    }

    let allCities = []; // 用於儲存所有城市數據，方便查找台中市

    // --- 載入城市和行政區資料 ---
    async function loadCitiesAndDistricts() {
        try {
            // 載入城市
            // *** 修改: 將 API 路徑從 /api/City/list 改為 /api/Common/City/list ***
            const citiesResponse = await fetch('/api/Common/City/list');
            if (!citiesResponse.ok) throw new Error('Failed to load cities');
            allCities = await citiesResponse.json(); // 儲存所有城市數據
            console.log('所有城市數據:', allCities);

            citySelect.innerHTML = ''; // 清空現有選項

            allCities.forEach(city => {
                const option = new Option(city.cityName, city.cityCode);
                citySelect.add(option);
            });

                // 預設選中「臺中市」並載入其行政區，基於先前的記憶和指令
                // 從記憶中得知當前位置是 Nantun District, Taichung City, Taiwan.
            const taichungCityCode = allCities.find(city => city.cityName === '臺中市')?.cityCode;
                if (taichungCityCode) {
                    citySelect.value = taichungCityCode;
                    currentCityCode = taichungCityCode; // 更新當前城市代碼
                    await loadDistricts(taichungCityCode);
                } else {
                    console.warn('Could not find "臺中市" in the city list, please check API response.');
                    // 如果找不到臺中市，可以選擇預設第一個城市或不選
                    if (allCities.length > 0) {
                        citySelect.value = allCities[0].cityCode;
                        currentCityCode = allCities[0].cityCode;
                        await loadDistricts(allCities[0].cityCode);
                    }
                }

        } catch (error) {
            console.error('載入城市失敗:', error);
            citySelect.innerHTML = '<option value="" disabled>載入城市失敗</option>';
        }
    }

    async function loadDistricts(cityCode) {
        try {
            // *** 修改: 將 API 路徑從 /api/District/list?cityCode= 改為 /api/Common/District/list/ ***
            const districtsResponse = await fetch(`/api/Common/District/list/${cityCode}`);
            if (!districtsResponse.ok) throw new Error('Failed to load districts');
            const districts = await districtsResponse.json();

            districtCheckboxesContainer.innerHTML = ''; // 清空現有行政區複選框
            currentSelectedDistricts = []; // 清空選中的行政區

            let nantunFoundAndSelected = false;

            // 如果沒有行政區，則直接將「不限」設為選中
            if (!districts || districts.length === 0) {
                districtNoLimitRadio.checked = true;
                districtNoLimitLabel.classList.add('active');
                districtCheckboxesContainer.innerHTML = '<p class="text-muted">該城市無行政區數據</p>';
                return; // 沒有行政區就直接返回
            }


            districts.forEach(district => {
                const isNantun = district.districtName === '北屯區' &&
                    cityCode === (allCities.find(c => c.cityName === '臺中市')?.cityCode || '');

                const checkboxHtml = `
                    <input type="checkbox" class="btn-check district-checkbox" id="district_${district.districtCode}" value="${district.districtCode}" ${isNantun ? 'checked' : ''}>
                    <label class="btn btn-outline-secondary btn-sm me-1 mb-1 ${isNantun ? 'active' : ''}" for="district_${district.districtCode}">${district.districtName}</label>
                `;
                districtCheckboxesContainer.insertAdjacentHTML('beforeend', checkboxHtml);

                if (isNantun) {
                    currentSelectedDistricts.push(district.districtCode);
                    nantunFoundAndSelected = true;
                }
            });

            // 根據是否有行政區被預選（北屯區）來設置「不限」的狀態
            if (nantunFoundAndSelected) {
                districtNoLimitRadio.checked = false;
                districtNoLimitLabel.classList.remove('active');
            } else {
                districtNoLimitRadio.checked = true;
                districtNoLimitLabel.classList.add('active');
            }

        } catch (error) {
            console.error(`載入行政區失敗 (City Code: ${cityCode}):`, error);
            districtCheckboxesContainer.innerHTML = '<p class="text-muted">載入行政區失敗</p>';
            // 如果載入行政區失敗，確保「不限」是選中的
            districtNoLimitRadio.checked = true;
            districtNoLimitLabel.classList.add('active');
        }
    }

    // --- 事件監聽器 ---

    // 城市選擇變更事件
    citySelect.addEventListener('change', async function () {
        currentCityCode = this.value; // 更新當前城市代碼
        currentSelectedDistricts = []; // 清空選中的行政區

        // 重置「不限」行政區的狀態
        districtNoLimitRadio.checked = true;
        districtNoLimitLabel.classList.add('active');

        if (currentCityCode) {
            await loadDistricts(currentCityCode);
        } else {
            districtCheckboxesContainer.innerHTML = ''; // 清空行政區
            districtNoLimitRadio.checked = true; // 如果沒有選城市，行政區選項應該回到「不限」
            districtNoLimitLabel.classList.add('active');
        }
        currentPageNumber = 1; // 篩選條件改變時重置到第一頁
        performSearch(); // 重新執行搜尋
    });

    // 關鍵字搜尋按鈕點擊事件 (和關鍵字輸入框回車事件)
    searchButton.addEventListener('click', function () {
        currentPageNumber = 1; // 重置到第一頁
        performSearch(); // 執行搜尋
    });

    keywordSearchInput.addEventListener('keypress', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault(); // 阻止表單提交，如果有的話
            currentPageNumber = 1; // 重置到第一頁
            performSearch(); // 執行搜尋
        }
    });

    // 行政區複選框變更事件 (使用事件委派)
    districtCheckboxesContainer.addEventListener('change', function (event) {
        if (event.target.classList.contains('district-checkbox')) {
            const checkbox = event.target;
            const districtCode = checkbox.value;
            const label = document.querySelector(`label[for="${checkbox.id}"]`);


            if (checkbox.checked) {
                if (!currentSelectedDistricts.includes(districtCode)) {
                    currentSelectedDistricts.push(districtCode);
                }
                // 當選中任何行政區時，取消「不限」的選中狀態和 label 的 active 狀態
                if (districtNoLimitRadio.checked) {
                    districtNoLimitRadio.checked = false;
                    districtNoLimitLabel.classList.remove('active');
                }
                if (label) label.classList.add('active'); // 讓選中的行政區 label 顯示 active 樣式
            } else {
                currentSelectedDistricts = currentSelectedDistricts.filter(d => d !== districtCode);
                // 如果所有行政區都被取消選中，則選中「不限」
                if (currentSelectedDistricts.length === 0) {
                    districtNoLimitRadio.checked = true;
                    districtNoLimitLabel.classList.add('active');
                }
                if (label) label.classList.remove('active'); // 取消選中的行政區 label 的 active 樣式
            }
            currentPageNumber = 1; // 篩選條件改變時重置到第一頁
            performSearch(); // 重新執行搜尋
        }
    });

    // 行政區「不限」按鈕的點擊事件 (確保它能取消所有行政區選擇)
    districtNoLimitLabel.addEventListener('click', function (event) {
        // 使用 setTimeout 確保 Bootstrap 的 active 狀態切換先完成
        setTimeout(() => {
            if (districtNoLimitRadio.checked) { // 如果「不限」是選中的
                document.querySelectorAll('.district-checkbox:checked').forEach(cb => {
                    cb.checked = false;
                    document.querySelector(`label[for="${cb.id}"]`)?.classList.remove('active'); // 移除 label 的 active 狀態
                });
                currentSelectedDistricts = []; // 清空選中的行政區
                currentPageNumber = 1; // 篩選條件改變時重置到第一頁
                performSearch(); // 重新執行搜尋
            }
        }, 0);
    });

    // 其他篩選條件（租金、格局、樓層、坪數、特色、設備）的事件監聽器
    // 您現有的通用「不限」按鈕和 input 變更事件監聽器已經可以處理這些
    document.querySelectorAll('.filters-container .filter-group input[type="checkbox"], .filters-container .filter-group input[type="radio"], .filters-container .filter-group input[type="number"]').forEach(function (input) {
        input.addEventListener('change', function () {
            const filterGroup = this.closest('.filter-group');
            // 找到該組的「不限」按鈕 (假定每個 filter-group 都有一個 `data-filter-type` 的「不限」radio)
            const noLimitRadio = filterGroup.querySelector('input[type="radio"][id$="NoLimit"]');
            const noLimitLabel = filterGroup.querySelector(`label[for="${noLimitRadio.id}"]`);

            // 判斷除了「不限」之外，是否有其他選項被選中或輸入框有值
            const anyOtherSelected = Array.from(filterGroup.querySelectorAll('input[type="checkbox"]:checked, input[type="radio"]:checked')).some(cb => cb !== noLimitRadio);
            const anyNumberInputWithValue = Array.from(filterGroup.querySelectorAll('input[type="number"]')).some(numInput => numInput.value !== '');

            // 處理租金和坪數範圍輸入框和按鈕的互斥邏輯
            if (this.type === 'number') { // 如果是數字輸入框改變
                // 取消同組的 radio 選中狀態
                filterGroup.querySelectorAll('input[type="radio"][name="' + this.name + '"]').forEach(radio => {
                    radio.checked = false;
                    filterGroup.querySelector(`label[for="${radio.id}"]`)?.classList.remove('active');
                });
            } else if (this.type === 'radio' && (this.name === 'rent' || this.name === 'areaSize' || this.name === 'floor')) { // 如果是租金、坪數或樓層 radio 改變
                // 清空同組的數字輸入框
                filterGroup.querySelectorAll('input[type="number"]').forEach(numInput => {
                    numInput.value = '';
                });
            }

            // 更新「不限」按鈕的狀態
            if (noLimitRadio && noLimitLabel) {
                if (anyOtherSelected || anyNumberInputWithValue) {
                    // 如果有其他選項被選中或輸入框有值，則取消「不限」
                    noLimitRadio.checked = false;
                    noLimitLabel.classList.remove('active');
                } else {
                    // 如果沒有其他選項被選中且輸入框沒有值，則選中「不限」
                    noLimitRadio.checked = true;
                    noLimitLabel.classList.add('active');
                }
            }

            // 處理 checkbox 的 active 狀態
            if (this.type === 'checkbox') {
                const labelForCheckbox = document.querySelector(`label[for="${this.id}"]`);
                if (labelForCheckbox) {
                    if (this.checked) {
                        labelForCheckbox.classList.add('active');
                    } else {
                        labelForCheckbox.classList.remove('active');
                    }
                }
            } else if (this.type === 'radio') {
                // 處理 radio 的 active 狀態 (除「不限」外)
                filterGroup.querySelectorAll('input[type="radio"][name="' + this.name + '"]').forEach(radioInput => {
                    const labelForRadio = document.querySelector(`label[for="${radioInput.id}"]`);
                    if (labelForRadio) {
                        if (radioInput.checked) {
                            labelForRadio.classList.add('active');
                        } else {
                            labelForRadio.classList.remove('active');
                        }
                    }
                });
            }


            currentPageNumber = 1; // 篩選條件改變時重置到第一頁
            performSearch(); // 重新執行搜尋
        });
    });

    // --- 處理「不限」按鈕的通用點擊邏輯 (確保點擊後取消同組其他選項) ---
    // 獨立於其他輸入框的 change 事件，因為它是 radio button
    document.querySelectorAll('.filter-group input[type="radio"][id$="NoLimit"]').forEach(function (radio) {
        const label = document.querySelector(`label[for="${radio.id}"]`);
        if (label) {
            label.addEventListener('click', function () {
                const filterGroup = this.closest('.filter-group');
                // 使用 setTimeout 確保 Bootstrap 的 active 狀態切換先完成
                setTimeout(() => {
                    if (radio.checked) { // 如果「不限」是選中的
                        // 取消所有同組的 checkbox 和其他 radio
                        filterGroup.querySelectorAll('input[type="checkbox"]:checked, input[type="radio"]:checked').forEach(input => {
                            if (input !== radio) { // 除了「不限」本身
                                input.checked = false;
                                document.querySelector(`label[for="${input.id}"]`)?.classList.remove('active');
                            }
                        });
                        // 清空同組的數字輸入框
                        filterGroup.querySelectorAll('input[type="number"]').forEach(input => {
                            input.value = '';
                        });

                        currentPageNumber = 1; // 篩選條件改變時重置到第一頁
                        performSearch(); // 重新執行搜尋
                    }
                }, 0);
            });
        }
    });


    // --- 渲染分頁導航函數 (保持不變) ---
    function renderPagination(currentPage, totalPages) {
        if (totalPages <= 1) {
            paginationContainer.innerHTML = ''; // 只有一頁或沒有結果時不顯示分頁
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
        const maxPagesToShow = 5; // 最多顯示5個頁碼按鈕
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

        paginationContainer.innerHTML = paginationHtml;

        // 為分頁按鈕添加點擊事件監聽器
        paginationContainer.querySelectorAll('.page-link').forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                const newPage = parseInt(this.dataset.page);
                if (!isNaN(newPage) && newPage > 0 && newPage <= totalPages && newPage !== currentPage) {
                    currentPageNumber = newPage;
                    performSearch(); // 重新執行搜尋
                }
            });
        });
    }

    
    // --- 處理排序按鈕點擊事件 (更新 currentSortField 和 currentSortOrder) ---
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
                    // 重置所有非當前選中按鈕的圖標為預設向上
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
                clickedIcon.classList.remove('bi-sort-down-alt');
                clickedIcon.classList.add('bi-sort-up'); // 預設為降序 (最新發布/租金高/面積大)
                this.dataset.sortOrder = 'desc';
                currentSortOrder = 'desc'; // 更新全局排序順序
                currentSortField = clickedSortField; // 更新全局排序字段
            }

            currentPageNumber = 1; // 排序改變時重置到第一頁
            performSearch(); // 重新執行搜尋
        });
    });

    // --- 處理收藏圖示點擊事件 (模擬切換收藏狀態) ---
    // 這個函數被事件委派調用
    async function toggleFavorite(propertyId, iconElement) {
        console.log(`Toggling favorite for Property ID: ${propertyId}`);

        const isFavorited = iconElement.classList.contains('bi-heart-fill');
        const url = `/api/User/Favorite/${propertyId}`; // 假設您的收藏 API 端點

        try {
            const method = isFavorited ? 'DELETE' : 'POST'; // 如果已收藏則刪除，否則新增
            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json'
                    // 如果需要認證 token，請在這裡添加
                }
            });

            if (!response.ok) {
                // 如果後端返回的不是 2xx 狀態碼
                const errorData = await response.json();
                throw new Error(`Failed to toggle favorite: ${errorData.message || response.statusText}`);
            }

            // 成功後才切換圖示類別和狀態
            if (isFavorited) {
                iconElement.classList.remove('bi-heart-fill');
                iconElement.classList.add('bi-heart');
                console.log(`Property ${propertyId} un-favorited.`);
            } else {
                iconElement.classList.remove('bi-heart');
                iconElement.classList.add('bi-heart-fill');
                console.log(`Property ${propertyId} favorited.`);
            }

        } catch (error) {
            console.error('收藏操作失敗:', error);
            alert('收藏/取消收藏操作失敗，請稍後再試。'); // 給用戶友好的提示
        }
    }

    // --- 處理展開/收合按鈕的文字和圖示切換 ---
    var moreFiltersCollapse = document.getElementById('moreFilters');
    var expandCollapseBtn = document.querySelector('.expand-collapse-btn');

    if (moreFiltersCollapse && expandCollapseBtn) {
        moreFiltersCollapse.addEventListener('show.bs.collapse', function () {
            expandCollapseBtn.querySelector('.collapsed-text').classList.add('d-none');
            expandCollapseBtn.querySelector('.expanded-text').classList.remove('d-none');
            expandCollapseBtn.querySelector('.bi').classList.remove('bi-chevron-down');
            expandCollapseBtn.querySelector('.bi').classList.add('bi-chevron-up');
        });

        moreFiltersCollapse.addEventListener('hide.bs.collapse', function () {
            expandCollapseBtn.querySelector('.collapsed-text').classList.remove('d-none');
            expandCollapseBtn.querySelector('.expanded-text').classList.add('d-none');
            expandCollapseBtn.querySelector('.bi').classList.remove('bi-chevron-up');
            expandCollapseBtn.querySelector('.bi').classList.add('bi-chevron-down');
        });

        // 頁面加載時檢查折疊狀態並更新按鈕顯示
        // Initial state check for the collapse button
        // Use a small timeout to ensure Bootstrap's collapse initialization has completed
        setTimeout(() => {
            if (moreFiltersCollapse.classList.contains('show')) {
                expandCollapseBtn.querySelector('.collapsed-text').classList.add('d-none');
                expandCollapseBtn.querySelector('.expanded-text').classList.remove('d-none');
                expandCollapseBtn.querySelector('.bi').classList.remove('bi-chevron-down');
                expandCollapseBtn.querySelector('.bi').classList.add('bi-chevron-up');
            } else {
                expandCollapseBtn.querySelector('.collapsed-text').classList.remove('d-none');
                expandCollapseBtn.querySelector('.expanded-text').classList.add('d-none');
                expandCollapseBtn.querySelector('.bi').classList.remove('bi-chevron-up');
                expandCollapseBtn.querySelector('.bi').classList.add('bi-chevron-down');
            }
        }, 100); // Small delay
    }

    // 頁面首次載入時執行：載入城市和行政區，然後執行搜尋
    loadCitiesAndDistricts().then(() => {
        performSearch();
    });
});