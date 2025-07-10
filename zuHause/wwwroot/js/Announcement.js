console.log('Announcement JS loaded!');

document.addEventListener('DOMContentLoaded', function () {

    // --- 分頁邏輯 ---
    const announcementItems = document.querySelectorAll('.announcement-item');
    const pageLinks = document.querySelectorAll('.pagination .page-link');
    const nextPageLink = document.querySelector('.pagination .next-page'); // 獲取下一頁連結
    const itemsPerPage = 6; // 每頁顯示的公告數量
    let currentPage = 1; // 當前頁碼，初始化為第一頁

    // 函數：顯示指定頁碼的公告
    function displayPage(pageNumber) {
        // 隱藏所有公告
        announcementItems.forEach(item => {
            item.classList.remove('active-page');
            item.style.display = 'none'; // 確保隱藏
        });

        // 顯示當前頁的公告
        const startIndex = (pageNumber - 1) * itemsPerPage;
        const endIndex = startIndex + itemsPerPage;
        const itemsToDisplay = Array.from(announcementItems).slice(startIndex, endIndex);

        itemsToDisplay.forEach(item => {
            item.classList.add('active-page');
            item.style.display = 'block'; // 或您公告列表項目的原始 display 屬性
        });

        // 更新分頁連結的 active 狀態
        pageLinks.forEach(link => {
            link.classList.remove('active');
            if (parseInt(link.dataset.pageNumber) === pageNumber) {
                link.classList.add('active');
            }
        });

        currentPage = pageNumber; // 更新當前頁碼

        // 這裡可以處理「下一頁」和「上一頁」按鈕的啟用/禁用狀態
        // 簡單示範「下一頁」：如果還有下一頁，則啟用
        if (nextPageLink) {
            const totalPages = Math.ceil(announcementItems.length / itemsPerPage);
            if (currentPage < totalPages) {
                nextPageLink.style.display = 'inline-block'; // 顯示
            } else {
                nextPageLink.style.display = 'none'; // 隱藏
            }
        }
    }

    // 頁碼連結點擊事件
    pageLinks.forEach(link => {
        if (!link.classList.contains('next-page')) { // 排除「下一頁」連結
            link.addEventListener('click', function (e) {
                e.preventDefault();
                const pageNumber = parseInt(this.dataset.pageNumber);
                displayPage(pageNumber);
            });
        }
    });

    // 「下一頁」連結點擊事件
    if (nextPageLink) {
        nextPageLink.addEventListener('click', function (e) {
            e.preventDefault();
            const totalPages = Math.ceil(announcementItems.length / itemsPerPage);
            if (currentPage < totalPages) {
                displayPage(currentPage + 1);
            }
        });
    }

    // 初始載入時顯示第一頁公告
    displayPage(1);

    // --- 公告展開/收起邏輯 (可能需要微調) ---
    // (將您之前的公告展開/收起邏輯貼在這裡，注意選擇器是否會衝突，
    // 尤其是在 displayPage 隱藏/顯示元素後，事件監聽器是否仍能工作)

    // *** 確保以下現有邏輯在 DOM 載入後和分頁初始化後執行 ***
    // 處理公告的展開/收起功能
    const announcementItemsAccordion = document.querySelectorAll('.announcement-item'); // 重新選擇，確保選到所有

    announcementItemsAccordion.forEach(item => {
        const header = item.querySelector('.announcement-header');
        const toggleButton = item.querySelector('.toggle-button');
        const readMoreToggle = item.querySelector('.read-more-toggle');

        if (header) {
            header.addEventListener('click', function () {
                toggleAnnouncement(item);
            });
        }
        if (toggleButton) {
            toggleButton.addEventListener('click', function (e) {
                e.stopPropagation();
                toggleAnnouncement(item);
            });
        }

        if (readMoreToggle) {
            readMoreToggle.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                const preview = item.querySelector('.announcement-preview');
                const fullContent = item.querySelector('.announcement-full-content');
                const button = item.querySelector('.toggle-button');

                if (this.dataset.action === 'expand') {
                    preview.style.display = 'none';
                    fullContent.style.display = 'block';
                    this.dataset.action = 'collapse';
                    this.style.display = 'none';

                    if (!item.classList.contains('expanded')) {
                        toggleAnnouncement(item, true);
                    }
                    button.querySelector('span').textContent = '收起';
                    button.querySelector('.arrow.up').style.transform = 'rotate(0deg)';
                    button.querySelector('.arrow.up').style.borderBottom = '5px solid #555';
                    button.querySelector('.arrow.up').style.borderTop = 'none';
                }
            });
        }

        // 初始設定展開狀態的邏輯，可能需要根據分頁後的顯示情況進行微調
        const preview = item.querySelector('.announcement-preview');
        const fullContent = item.querySelector('.announcement-full-content');
        const currentReadMoreToggle = item.querySelector('.read-more-toggle');

        if (item.classList.contains('expanded')) {
            item.querySelector('.announcement-body').style.maxHeight = '500px';
            item.querySelector('.announcement-body').style.paddingTop = '15px';
            if (item.querySelector('.toggle-button .arrow.up')) {
                item.querySelector('.toggle-button .arrow.up').style.transform = 'rotate(0deg)';
                item.querySelector('.toggle-button .arrow.up').style.borderBottom = '5px solid #555';
                item.querySelector('.toggle-button .arrow.up').style.borderTop = 'none';
            }
            if (preview && fullContent) {
                preview.style.display = 'none';
                fullContent.style.display = 'block';
                if (currentReadMoreToggle) currentReadMoreToggle.style.display = 'none';
            }
        } else {
            item.querySelector('.announcement-body').style.maxHeight = '0';
            item.querySelector('.announcement-body').style.paddingTop = '0';
            if (item.querySelector('.toggle-button .arrow.up')) {
                item.querySelector('.toggle-button .arrow.up').style.transform = 'rotate(180deg)';
                item.querySelector('.toggle-button .arrow.up').style.borderTop = '5px solid #555';
                item.querySelector('.toggle-button .arrow.up').style.borderBottom = 'none';
            }
            if (preview && fullContent) {
                preview.style.display = 'block';
                fullContent.style.display = 'none';
                if (currentReadMoreToggle) currentReadMoreToggle.style.display = 'inline';
            }
        }
    });

    // 修正 toggleAnnouncement 函數，使其可以強制展開
    function toggleAnnouncement(item, forceExpand = false) {
        const body = item.querySelector('.announcement-body');
        const arrow = item.querySelector('.toggle-button .arrow.up');
        const preview = item.querySelector('.announcement-preview');
        const fullContent = item.querySelector('.announcement-full-content');
        const readMoreToggle = item.querySelector('.read-more-toggle');

        if (item.classList.contains('expanded') && !forceExpand) {
            // 收起
            body.style.maxHeight = '0';
            body.style.paddingTop = '0';
            item.classList.remove('expanded');
            if (arrow) {
                arrow.style.transform = 'rotate(180deg)';
                arrow.style.borderTop = '5px solid #555';
                arrow.style.borderBottom = 'none';
            }
            if (preview && fullContent) {
                preview.style.display = 'block';
                fullContent.style.display = 'none';
                if (readMoreToggle) readMoreToggle.style.display = 'inline';
            }
        } else {
            announcementItemsAccordion.forEach(otherItem => { // 注意這裡用 announcementItemsAccordion
                if (otherItem !== item && otherItem.classList.contains('expanded')) {
                    otherItem.classList.remove('expanded');
                    otherItem.querySelector('.announcement-body').style.maxHeight = '0';
                    otherItem.querySelector('.announcement-body').style.paddingTop = '0';
                    const otherArrow = otherItem.querySelector('.toggle-button .arrow.up');
                    if (otherArrow) {
                        otherArrow.style.transform = 'rotate(180deg)';
                        otherArrow.style.borderTop = '5px solid #555';
                        otherArrow.style.borderBottom = 'none';
                    }
                    const otherPreview = otherItem.querySelector('.announcement-preview');
                    const otherFullContent = otherItem.querySelector('.announcement-full-content');
                    const otherReadMoreToggle = otherItem.querySelector('.read-more-toggle');
                    if (otherPreview && otherFullContent) {
                        otherPreview.style.display = 'block';
                        otherFullContent.style.display = 'none';
                        if (otherReadMoreToggle) otherReadMoreToggle.style.display = 'inline';
                    }
                }
            });

            // 展開當前點擊的公告
            body.style.maxHeight = '500px';
            body.style.paddingTop = '15px';
            item.classList.add('expanded');
            if (arrow) {
                arrow.style.transform = 'rotate(0deg)';
                arrow.style.borderBottom = '5px solid #555';
                arrow.style.borderTop = 'none';
            }
            if (preview && fullContent) {
                preview.style.display = 'none';
                fullContent.style.display = 'block';
                if (readMoreToggle) readMoreToggle.style.display = 'none';
            }
        }
    }

    // 初始載入時的邏輯 (這段保持不變，因為分頁邏輯會處理第一個公告的顯示)
    const initialActiveNavItem = document.querySelector('.sidebar .nav-item.active');
    if (initialActiveNavItem) {
        const initialContentId = initialActiveNavItem.dataset.contentId;
        const initialContentSection = document.getElementById(initialContentId);
        if (initialContentSection) {
            initialContentSection.classList.add('active');
        }
    }
    // 預設展開第一個公告（如果需要的話，但分頁已經顯示第一頁了）
    // 如果您希望第一頁的第一個公告在載入時就展開，可以在此處呼叫 toggleAnnouncement(firstAnnouncement, true);
    // 但請注意，分頁邏輯會先顯示第一頁的公告。
    const firstAnnouncementOnLoad = document.querySelector('.announcement-item[data-page="1"]');
    if (firstAnnouncementOnLoad && !firstAnnouncementOnLoad.classList.contains('expanded')) {
        toggleAnnouncement(firstAnnouncementOnLoad, true);
    }
});