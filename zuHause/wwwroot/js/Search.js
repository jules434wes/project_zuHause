document.addEventListener('DOMContentLoaded', function () {
    // ... (保留您現有的所有 JS 程式碼，除了重複的排序按鈕邏輯) ...

    // 處理展開/收合按鈕的文字和圖示切換
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

        // 初始狀態檢查 (如果頁面載入時是展開的)
        if (moreFiltersCollapse.classList.contains('show')) {
            expandCollapseBtn.querySelector('.collapsed-text').classList.add('d-none');
            expandCollapseBtn.querySelector('.expanded-text').classList.add('d-inline-block'); // 確保顯示
            expandCollapseBtn.querySelector('.bi').classList.remove('bi-chevron-down');
            expandCollapseBtn.querySelector('.bi').classList.add('bi-chevron-up');
        }
    }

    // 處理收藏圖示點擊事件 (模擬切換收藏狀態)
    document.querySelectorAll('.favorite-icon').forEach(function (icon) {
        icon.addEventListener('click', function () {
            // 這裡可以添加收藏/取消收藏的 AJAX 請求
            if (this.classList.contains('bi-heart-fill')) {
                this.classList.remove('bi-heart-fill');
                this.classList.add('bi-heart');
                this.style.color = ''; // 移除顏色
                console.log('已取消收藏');
            } else {
                this.classList.remove('bi-heart');
                this.classList.add('bi-heart-fill');
                this.style.color = 'red'; // 收藏後變紅色
                console.log('已收藏');
            }
        });
    });

    // 處理「不限」按鈕的邏輯 (簡單示例，您可以根據需求擴展)
    document.querySelectorAll('.filter-group .btn-outline-secondary.btn-sm').forEach(function (button) {
        button.addEventListener('click', function () {
            const filterGroup = this.closest('.filter-group');
            const checkboxes = filterGroup.querySelectorAll('input[type="checkbox"]');
            const radiobuttons = filterGroup.querySelectorAll('input[type="radio"]');
            const inputNumbers = filterGroup.querySelectorAll('input[type="number"]');

            // 延遲執行以確保 Bootstrap 的 data-bs-toggle="button" 已處理完按鈕的 active 狀態
            setTimeout(() => {
                const isActive = this.classList.contains('active');

                if (isActive) {
                    // 如果「不限」被點擊為 active 狀態，則取消選擇組內所有其他選項
                    checkboxes.forEach(cb => {
                        cb.checked = false;
                    });
                    radiobuttons.forEach(rb => {
                        rb.checked = false;
                    });
                    inputNumbers.forEach(input => {
                        input.value = '';
                    });
                }
                // 如果「不限」被點擊為 inactive 狀態，則不做任何事。
            }, 0);
        });
    });

    // 當其他篩選條件被選中時，取消「不限」按鈕的 active 狀態
    document.querySelectorAll('.filter-group input[type="checkbox"], .filter-group input[type="radio"], .filter-group input[type="number"]').forEach(function (input) {
        input.addEventListener('change', function () {
            const filterGroup = this.closest('.filter-group');
            const noLimitButton = filterGroup.querySelector('.btn-outline-secondary.btn-sm');

            // 檢查是否有任何其他篩選條件被選中或有值
            const anyOtherSelected = Array.from(filterGroup.querySelectorAll('input[type="checkbox"]:checked, input[type="radio"]:checked')).some(input => input.value !== '');
            const anyNumberInputWithValue = Array.from(filterGroup.querySelectorAll('input[type="number"]')).some(input => input.value !== '');

            if (noLimitButton && noLimitButton.classList.contains('active') && (anyOtherSelected || anyNumberInputWithValue)) {
                // 如果「不限」是 active 狀態，但用戶選擇了其他篩選，則取消「不限」的 active 狀態
                noLimitButton.classList.remove('active');
            } else if (noLimitButton && !noLimitButton.classList.contains('active') && !anyOtherSelected && !anyNumberInputWithValue) {
                // 如果「不限」不是 active 狀態，但所有其他選項都未選且數字輸入為空，則將「不限」設為 active
                // 這是一個可選的行為，根據您的 UX 設計決定是否需要
                // noLimitButton.classList.add('active');
            }
        });
    });

    // === 這才是唯一正確的排序按鈕處理邏輯 ===
    const sortButtons = document.querySelectorAll('.sort-btn');

    sortButtons.forEach(button => {
        button.addEventListener('click', function () {
            const currentSortField = this.dataset.sortField;
            let currentSortOrder = this.dataset.sortOrder;
            const currentIcon = this.querySelector('i');

            // 移除所有按鈕的 active 狀態和重置圖標
            sortButtons.forEach(btn => {
                btn.classList.remove('active');
                const icon = btn.querySelector('i');
                if (icon) {
                    // 重置所有非當前選中按鈕的圖標為預設向上
                    icon.classList.remove('bi-sort-down-alt');
                    icon.classList.add('bi-sort-up');
                    btn.dataset.sortOrder = 'asc'; // 重置數據屬性
                }
            });

            // 為當前點擊的按鈕添加 active 狀態
            this.classList.add('active');

            // 切換圖標方向和 data-sort-order
            if (currentSortOrder === 'desc') {
                currentIcon.classList.remove('bi-sort-down-alt');
                currentIcon.classList.add('bi-sort-up');
                this.dataset.sortOrder = 'asc';
            } else {
                currentIcon.classList.remove('bi-sort-up');
                currentIcon.classList.add('bi-sort-down-alt');
                this.dataset.sortOrder = 'desc';
            }

            // 在這裡可以觸發實際的排序邏輯，例如發送 AJAX 請求
            console.log(`排序字段: ${currentSortField}, 排序順序: ${this.dataset.sortOrder}`);
            // 範例: 呼叫一個排序函數
            // performSearchSort(currentSortField, this.dataset.sortOrder);
        });
    });
});

// 假設的排序函數，您可以根據實際需求來實現 (此函數可以在 DOMContentLoaded 外部定義，以便全局訪問，如果需要的話)
// function performSearchSort(field, order) {
//      // 這裡可以發送 AJAX 請求到後端，根據 field 和 order 獲取排序後的數據
//      // 例如: fetch(`/api/houses?sortField=${field}&sortOrder=${order}`)
//      // .then(response => response.json())
//      // .then(data => {
//      //      // 更新頁面上的房屋列表
//      // });
//      console.log(`執行實際排序：根據 ${field} 字段，${order} 順序`);
// }