$(document).ready(function () {
    // 獲取儲存在 window 物件上的 CSS Scope ID
    // 這個 rawCssScopeId 應該是 'b-qju16rztvb' 這種乾淨的格式
    const rawCssScopeId = window.CollectionAndComparisonCssScopeId;

    // 這個 cssScopeAttribute 是用於 HTML 模板中的佔位符，前面可以加空格，
    // 因為它會被解析成一個獨立的屬性，例如：<div b-qju16rztvb class="col">
    let cssScopeAttribute = rawCssScopeId ? ` ${rawCssScopeId}` : '';

    // --- Toast 輔助函式 ---
    function showToast(message, title = '通知') {
        const toastContainer = $('#toastContainer');
        const toastTemplate = $('#toastTemplate').html();

        // 如果 toastContainer 或 toastTemplate 不存在，則不執行
        if (!toastContainer.length || !toastTemplate) {
            console.error('Toast container or template not found. Cannot show toast.');
            return;
        }

        const newToastElement = $(toastTemplate);

        // 核心修正點：確保傳遞給 attr() 的屬性名稱是乾淨的
        // 我們要用的就是 rawCssScopeId，它就是那個乾淨的 b-xxxxxxxxxx
        const cssIsolationAttributeName = rawCssScopeId;

        function applyCssScope(element) {
            if (element.prop("tagName")) { // 檢查元素是否為有效的 HTML 標籤
                // 只有當 cssIsolationAttributeName 存在且非空時才添加屬性
                if (cssIsolationAttributeName) {
                    element.attr(cssIsolationAttributeName, ''); // 使用乾淨的 Scope ID 作為屬性名
                }
            }
            // 遞歸地對所有子元素應用 Scope ID
            element.children().each(function () {
                applyCssScope($(this));
            });
        }
        // 對新創建的 Toast 元素應用 CSS Scope ID
        applyCssScope(newToastElement);


        newToastElement.find('.toast-title').text(title);
        newToastElement.find('.toast-body').text(message);

        toastContainer.append(newToastElement);

        const bsToast = new bootstrap.Toast(newToastElement[0], {
            delay: 3000 // 訊息顯示 3 秒後自動隱藏
        });
        bsToast.show();

        // 移除 Toast 元素，避免 DOM 中累積過多
        newToastElement.on('hidden.bs.toast', function () {
            $(this).remove();
        });
    }

    // --- 收藏與比較 Tab 切換邏輯 ---
    // 確保 View Toggle 按鈕的狀態與當前激活的視圖同步
    $('.btn-group button').on('click', function () {
        const targetView = $(this).data('target-view');
        // 移除所有按鈕的 active 和 aria-pressed
        $('.btn-group button').removeClass('active').attr('aria-pressed', 'false');
        // 給點擊的按鈕添加 active 和 aria-pressed
        $(this).addClass('active').attr('aria-pressed', 'true');

        // 隱藏所有視圖
        $('[data-view-type]').hide();
        // 顯示目標視圖
        $(`[data-view-type="${targetView}"]`).show();
    });


    // --- 「新增比較」按鈕功能 (動態生成 Tab Pane) ---
    $('#addNewCompareTabButton').on('click', function () {
        const currentTabCount = $('#collectionTabs .nav-item').length - 1; // 減去「點選新增...」按鈕
        const newCompareIndex = currentTabCount; // 因為有 收藏、比較1、比較2，所以從 3 開始
        const newId = `compare${newCompareIndex}-tab`;
        const newContentId = `compare${newCompareIndex}-content`;

        if (newCompareIndex > 2) { // 限制最多只有收藏 + 2 個比較頁籤
            showToast('最多只能有兩個比較頁籤。', '提示');
            return;
        }

        // 移除舊的「新增比較」按鈕所在的 li 元素
        $(this).parent('.nav-item').remove();

        // 動態新增比較 Tab 按鈕
        const newTabHtml = `
            <li${cssScopeAttribute} class="nav-item" role="presentation">
                <button${cssScopeAttribute} class="nav-link" id="${newId}" data-bs-toggle="tab" data-bs-target="#${newContentId}" type="button" role="tab" aria-controls="${newContentId}" aria-selected="false">
                    比較${newCompareIndex}
                </button>
            </li>
        `;
        $('#collectionTabs').append(newTabHtml);

        // 動態新增比較 Tab Pane (內容區)
        const newPaneHtml = `
            <div${cssScopeAttribute} class="tab-pane fade flex-grow-1 d-flex flex-column p-3" id="${newContentId}" role="tabpanel" aria-labelledby="${newId}">
                <div${cssScopeAttribute} class="tab-pane-content-wrapper flex-grow-1 d-flex flex-column">
                    <h3${cssScopeAttribute} class="mb-4">比較${newCompareIndex}</h3>
                    <div${cssScopeAttribute} class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4 compare-items-grid">
                        </div>
                    <nav${cssScopeAttribute} aria-label="Page navigation for compare${newCompareIndex}" class="pagination-custom mt-auto">
                        <ul${cssScopeAttribute} class="pagination justify-content-center">
                            <li${cssScopeAttribute} class="page-item disabled">
                                <a${cssScopeAttribute} class="page-link" href="#" aria-label="Previous">
                                    <span${cssScopeAttribute} aria-hidden="true">&laquo;</span>
                                </a>
                            </li>
                            <li${cssScopeAttribute} class="page-item active"><a${cssScopeAttribute} class="page-link" href="#">1</a></li>
                            <li${cssScopeAttribute} class="page-item"><a${cssScopeAttribute} class="page-link" href="#">2</a></li>
                            <li${cssScopeAttribute} class="page-item"><a${cssScopeAttribute} class="page-link" href="#">3</a></li>
                            <li${cssScopeAttribute} class="page-item">
                                <a${cssScopeAttribute} class="page-link" href="#" aria-label="Next">
                                    <span${cssScopeAttribute} aria-hidden="true">&raquo;</span>
                                </a>
                            </li>
                        </ul>
                    </nav>
                </div>
            </div>
        `;
        $('#collectionTabContent').append(newPaneHtml);

        // 重新添加「點選新增...」按鈕
        if (newCompareIndex < 2) { // 如果還沒達到最大頁籤數量
            const addTabHtml = `
                <li${cssScopeAttribute} class="nav-item add-new-compare-tab-item" role="presentation">
                    <button${cssScopeAttribute} class="nav-link" id="addNewCompareTabButton" type="button" role="button">
                        點選新增...
                    </button>
                </li>
            `;
            $('#collectionTabs').append(addTabHtml);
        }

        // 切換到新建立的 Tab
        $(`#${newId}`).tab('show');
        showToast(`已新增「比較${newCompareIndex}」頁籤`, '成功');
    });

    // 處理「新增至比較」按鈕的邏輯 (使用事件委託)
    $(document).on('click', '.dropdown-item.add-to-compare-btn', function (e) {
        e.preventDefault();
        const compareTarget = $(this).data('compare-target'); // 1 或 2
        let itemId;
        let title;
        let imgSrc; // 宣告 imgSrc 變數

        // 找到點擊的按鈕所屬的房源卡片或列表項目
        const closestCardOrListItem = $(this).closest('.house-card, .list-group-item');

        if (closestCardOrListItem.length > 0) {
            itemId = closestCardOrListItem.data('item-id');
            // 從 closestCardOrListItem 裡找到圖片的 src 和標題
            imgSrc = closestCardOrListItem.find('img').attr('src');
            title = closestCardOrListItem.find('.card-title, h5.mb-1').text().trim(); // 兼容 grid 和 list 視圖
        } else {
            // 如果無法從最近的卡片獲取，嘗試從下拉按鈕本身的 data-item-id 獲取
            itemId = $(this).data('item-id');
            // 如果仍未獲取到 title 或 imgSrc，給一個預設值
            title = `項目 ID: ${itemId || '未知'}`;
            imgSrc = 'https://via.placeholder.com/250x180/cccccc/808080?text=No+Image'; // 提供預設圖片
        }

        if (!itemId) {
            showToast('無法獲取項目 ID，請確認 HTML 結構中包含 data-item-id 屬性。', '錯誤');
            return;
        }

        // 檢查目標比較組是否已存在該項目
        const targetGrid = $(`#compare${compareTarget}-content .compare-items-grid`);
        if (targetGrid.find(`.compare-card[data-item-id="${itemId}"]`).length > 0) {
            showToast(`項目 ID: ${itemId} 已存在於比較${compareTarget}中。`, '提示');
            return;
        }

        // 模擬從伺服器獲取詳細資訊 (這裡使用靜態數據)
        const detailedInfo = {
            address: "台中市南屯區公益路二段51號", // 範例數據
            rentDetail: "NT$ 25,000 / 月",
            size: "30 坪",
            type: "整層",
            parking: "有機車位",
            floor: "18F",
            minTerm: "12個月",
            includes: "水、電、瓦斯、網路",
            notes: "備註很長很長很長很長很長很長"
        };
        const petFriendly = "可養寵物"; // 這裡也需要獲取，如果它不是固定的話，可以從父元素獲取

        // 處理圖片 URL 替換（如果你希望使用不同尺寸的圖片）
        let finalImgSrc = imgSrc;
        if (finalImgSrc && finalImgSrc.includes('via.placeholder.com')) {
            // 針對 placeholder 圖片，替換為固定尺寸，避免多次替換
            finalImgSrc = finalImgSrc.replace(/\/\d+x\d+\//, '/250x180/');
        } else if (!finalImgSrc) {
            finalImgSrc = 'https://via.placeholder.com/250x180/cccccc/808080?text=No+Image'; // 如果原始圖片路徑為空，給一個預設值
        }

        const compareCardHtml = `
            <div${cssScopeAttribute} class="col">
                <div${cssScopeAttribute} class="compare-card" data-item-id="${itemId}">
                    <button${cssScopeAttribute} class="close-btn position-absolute top-0 end-0 m-1"><i${cssScopeAttribute} class="bi bi-x-circle-fill fs-5 text-danger"></i></button>
                    <div${cssScopeAttribute} class="text-center mb-3">
                        <img${cssScopeAttribute} src="${finalImgSrc}" class="img-fluid rounded" alt="比較項目 ${title}">
                        <h5${cssScopeAttribute} class="mt-2 text-truncate">${title}</h5>
                    </div>
                    <ul${cssScopeAttribute} class="list-group list-group-flush border-top border-bottom mb-3">
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>地址:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.address}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>租金:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.rentDetail}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>坪數:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.size}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>類型:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.type}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>單位:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.parking}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>樓層:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.floor}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>最短租期:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.minTerm}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>租金包括:</span> <span${cssScopeAttribute} class="fw-bold">${detailedInfo.includes}</span>
                        </li>
                        <li${cssScopeAttribute} class="list-group-item d-flex justify-content-between align-items-center small py-1">
                            <span${cssScopeAttribute}>備註:</span> <span${cssScopeAttribute} class="fw-bold text-wrap">${detailedInfo.notes}</span>
                        </li>
                    </ul>
                    <p${cssScopeAttribute} class="text-muted small">${petFriendly}</p>
                </div>
            </div>
        `;
        targetGrid.append(compareCardHtml);
        showToast(`項目已新增至比較${compareTarget}！`, '成功');

        // 切換到對應的比較Tab
        $(`#compare${compareTarget}-tab`).tab('show');
    });

    // 處理移除比較項目 (通用於所有比較組)
    $(document).on('click', '.compare-card .close-btn', function () {
        const cardToRemove = $(this).closest('.col');
        const itemIdToRemove = cardToRemove.find('.compare-card').data('item-id');
        if (confirm(`確定要從比較中移除項目 ID: ${itemIdToRemove} 嗎？`)) {
            cardToRemove.remove();
            showToast(`項目 ID: ${itemIdToRemove} 已從比較中移除。`, '移除');
        }
    });

    // 處理收藏頁籤的移除按鈕
    $(document).on('click', '#collection-content .house-card .close-btn, #collection-content .list-group-item .close-btn', function () {
        const itemToRemove = $(this).closest('.col, .list-group-item');
        const itemId = itemToRemove.find('[data-item-id]').data('item-id');
        if (confirm(`確定要從收藏中移除項目 ID: ${itemId} 嗎？`)) {
            itemToRemove.remove();
            showToast(`項目 ID: ${itemId} 已從收藏中移除。`, '移除');
        }
    });
});