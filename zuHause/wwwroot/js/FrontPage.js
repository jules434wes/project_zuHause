document.addEventListener('DOMContentLoaded', function () {

    // 確保 jQuery 已經載入
    if (typeof jQuery === 'undefined') {
        console.error('jQuery is not loaded. Some functionalities may not work.');
        return;
    }


    // 1. Initialize tooltips (使用 Bootstrap 官方的 JS 方式)
    // 這裡使用原生 JS 集合轉陣列，並用 map 遍歷，是 Bootstrap 官方推薦寫法，可以保留
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // 2. Back to top button logic
    const $mybutton = $('#backToTopBtn'); // 使用 jQuery 選擇器

    if ($mybutton.length) { // 檢查按鈕是否存在
        // 綁定點擊事件 (jQuery 方式)
        $mybutton.on('click', function () {
            // 方案 A: 無延遲，直接跳轉到頂部 (推薦，如果不需要動畫)
            document.body.scrollTop = 0; // For Safari
            document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera

            // 方案 B: 縮短延遲，例如 200 毫秒 (0.2 秒)
            // $('html, body').animate({ scrollTop: 0 }, 200);
        });
        // 監聽滾動事件來顯示/隱藏按鈕
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

    // 處理城市下拉選單點擊事件
    $dropdownItems.on('click', function (event) {
        event.preventDefault();
        // 移除所有 active 類別
        $dropdownItems.removeClass('active');
        // 為當前點擊的項目添加 active 類別
        $(this).addClass('active');
        // 更新按鈕顯示的城市名稱
        selectedCity = $(this).data('city');
        $cityDropdownBtn.text(selectedCity);
        // 關閉下拉選單 (Bootstrap 會自動處理，但如果遇到問題，可以嘗試手動觸發隱藏)
        // $cityDropdownBtn.dropdown('hide'); // Requires Bootstrap JS for jQuery methods
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
                <span class="badge bg-secondary p-2 d-flex align-items-center search-history-item" data-value="${item}">
                    ${item}
                    <button type="button" class="btn-close btn-close-white ms-2" aria-label="Close" data-value="${item}"></button>
                </span>
            `);
        });

        // Add click listener for history items (delegated for dynamically added elements)
        // 使用事件委託，避免重複綁定和確保動態元素也能響應
        $searchHistoryDiv.off('click', '.search-history-item').on('click', '.search-history-item', function (e) {
            // Prevent event from bubbling to close button if it's the target
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
        const city = $cityDropdownBtn.text().trim(); // Get the selected city from button text
        let searchUrl = `/Search/Results`;

        let queryParams = [];
        if (city && city !== "請選擇城市") {
            queryParams.push(`city=${encodeURIComponent(city)}`);
        }
        if (keyword) {
            queryParams.push(`keyword=${encodeURIComponent(keyword)}`);
        }

        if (queryParams.length > 0) {
            searchUrl += `?${queryParams.join('&')}`;
            // 只有當城市或關鍵字不為空時才加入歷史紀錄
            if (city || keyword) {
                addSearchHistory(`${city} ${keyword}`.trim());
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

    // 4. 公告跑馬燈的高度調整 (從您的 HTML 判斷需要這段邏輯)
    function adjustMarqueeHeight() {
        const $navbar = $('#mainNavbar'); // 假設您的導航欄有這個ID
        const $marqueeContainer = $('.announcement-marquee-container');
        if ($navbar.length && $marqueeContainer.length) {
            const navbarHeight = $navbar.outerHeight(); // 使用 outerHeight 包含 padding 和 border
            $marqueeContainer.css({
                'height': `${navbarHeight}px`,
                'line-height': `${navbarHeight}px`
            });
            // 如果導航欄是 fixed/sticky，可能還需要調整 body 的 padding-top
            // $('body').css('padding-top', `${navbarHeight}px`);
        }
    }

    // 頁面載入時和視窗大小改變時調整跑馬燈高度
    adjustMarqueeHeight();
    $(window).on('resize', adjustMarqueeHeight);


    // Load search history on page load
    loadSearchHistory();
}); // End of DOMContentLoaded