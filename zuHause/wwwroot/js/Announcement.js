// wwwroot/js/Announcement.js

document.addEventListener('DOMContentLoaded', () => {
    // === 變數定義 ===
    const navItems = document.querySelectorAll('.sidebar .nav-item');
    const contentSections = document.querySelectorAll('.main-content .content-section');
    const announcementListContainer = document.querySelector('.announcement-list');
    const paginationContainer = document.querySelector('.pagination');

    const API_BASE_URL = '/api/Tenant'; // API 基礎 URL
    const MAX_PARTIAL_LENGTH = 100; // 定義部分展開的字數限制

    let currentPage = 1; // 當前公告頁碼
    const announcementCache = {}; // 新增：用於快取已載入的公告詳細內容 {id: {fullContent: '...', attachmentUrl: '...'}}

    const urlParams = new URLSearchParams(window.location.search);
    const targetSectionFromUrl = urlParams.get('section'); // 讀取 'section' 參數的值

    let initialTargetId = 'announcements'; // 預設顯示公告
    if (targetSectionFromUrl) {
        // 檢查 URL 參數是否對應到一個有效的 content-section id
        const validSection = document.getElementById(targetSectionFromUrl);
        if (validSection) {
            initialTargetId = targetSectionFromUrl;
        }
    }

    /**
     * 清空指定容器的內容。
     * @param {HTMLElement} container 要清空的 HTML 元素。
     */
    function clearContent(container) {
        if (container) {
            container.innerHTML = '';
        }
    }

    /**
     * 檢查 URL 是否為圖片。
     * @param {string} url - 要檢查的 URL 字串。
     * @returns {boolean} 如果是圖片 URL 則返回 true，否則返回 false。
     */
    function isImageUrl(url) {
        if (typeof url !== 'string' || !url) {
            return false;
        }
        // 檢查常見的圖片副檔名
        if (/\.(jpeg|jpg|gif|png|webp|bmp|svg)$/i.test(url)) {
            return true;
        }
        // 針對 picsum.photos 的特別處理，因為其 URL 不包含副檔名
        if (url.includes('picsum.photos')) {
            return true;
        }
        return false;
    }

    /**
     * 創建附件的 HTML 元素 (圖片或連結按鈕)。
     * @param {string} url - 附件的 URL。
     * @param {string} title - 公告標題，用於圖片 alt 屬性。
     * @returns {HTMLElement|null} 創建的附件包裝器元素，如果 URL 無效則返回 null。
     */
    function createAttachmentElement(url, title) {
        if (!url) return null;

        const attachmentWrapper = document.createElement('div');
        attachmentWrapper.classList.add('announcement-attachment-wrapper');

        if (isImageUrl(url)) {
            const img = document.createElement('img');
            img.src = url;
            img.alt = title + ' 附件圖片';
            img.classList.add('announcement-attachment-image');
            attachmentWrapper.appendChild(img);

            // 添加點擊圖片放大功能 (Lightbox / Modal)
            img.addEventListener('click', (e) => {
                e.stopPropagation(); // 阻止事件冒泡，避免點擊圖片時觸發公告標題的點擊事件
                const modal = document.createElement('div');
                modal.classList.add('image-modal');
                // 創建關閉按鈕和圖片內容
                modal.innerHTML = `<span class="close-button">&times;</span><img class="modal-content" src="${url}">`;
                document.body.appendChild(modal);
                modal.style.display = 'flex'; // 顯示模態框

                // 點擊關閉按鈕關閉模態框
                const closeButton = modal.querySelector('.close-button');
                closeButton.addEventListener('click', () => {
                    modal.style.display = 'none';
                    modal.remove(); // 從 DOM 中移除模態框
                });

                // 點擊模態框外部（背景）關閉模態框
                modal.addEventListener('click', (e) => {
                    if (e.target === modal) { // 確保只在點擊背景時觸發
                        modal.style.display = 'none';
                        modal.remove();
                    }
                });
            });
        } else {
            // 如果不是圖片 URL，則創建一個連結按鈕
            const link = document.createElement('a');
            link.href = url;
            link.textContent = '查看附件';
            link.target = '_blank'; // 在新標籤頁中打開
            link.classList.add('announcement-attachment-link');
            attachmentWrapper.appendChild(link);
        }
        return attachmentWrapper;
    }

    /**
     * 載入並顯示公告列表。
     * @param {number} page - 要載入的頁碼。
     */
    async function loadAnnouncements(page) {
        clearContent(announcementListContainer); // 清空現有公告列表
        clearContent(paginationContainer);     // 清空分頁控制項

        announcementListContainer.innerHTML = '<p>載入中，請稍候...</p>'; // 顯示載入提示

        console.log('Attempting to load announcements for page:', page); // <<<<<<<<<< 新增這行
        console.log('API URL:', `${API_BASE_URL}/Announcements/list?pageNumber=${page}&pageSize=6`); // <<<<<<<<<< 新增這行

        try {
            const response = await fetch(`${API_BASE_URL}/Announcements/list?pageNumber=${page}&pageSize=6`);
            console.log('API Response status:', response.status); // <<<<<<<<<< 新增這行

            if (!response.ok) {
                // 如果 HTTP 狀態碼不是 2xx，拋出錯誤
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json(); // 解析 JSON 回應
            console.log('API Response data:', data); // <<<<<<<<<< 新增這行

            if (data.announcements && data.announcements.length > 0) {
                clearContent(announcementListContainer); // 成功載入後清除載入提示

                for (const announcement of data.announcements) {
                    const announcementItem = document.createElement('div');
                    announcementItem.classList.add('announcement-item', 'closed'); // 初始狀態設為 closed
                    announcementItem.dataset.announcementId = announcement.id; // 儲存 ID 以便後續請求詳細內容

                    // 判斷內容是否超過預覽長度，決定是否需要展開按鈕
                    const isContentLong = announcement.contentPreview.length > MAX_PARTIAL_LENGTH;

                    // 構建公告項目的基本 HTML 結構
                    announcementItem.innerHTML = `
                        <div class="announcement-header">
                            <h2 class="announcement-title">${announcement.moduleScope}${announcement.title}</h2>
                            <span class="last-modified-date">最後修改日期: ${announcement.updatedAt}</span>
                        </div>
                        <div class="announcement-body">
                            <div class="announcement-content"></div>
                            ${isContentLong ? `<button type="button" class="toggle-button"><span>點擊展開 </span><span class="arrow"></span></button>` : ''}
                        </div>
                    `;
                    announcementListContainer.appendChild(announcementItem); // 將公告項目添加到列表中

                    // 獲取相關 DOM 元素
                    const announcementHeader = announcementItem.querySelector('.announcement-header');
                    const announcementBody = announcementItem.querySelector('.announcement-body');
                    const announcementContentDiv = announcementBody.querySelector('.announcement-content');
                    const toggleButton = announcementBody.querySelector('.toggle-button');

                    // 初始顯示部分內容，並在數據屬性中儲存原始預覽內容
                    announcementContentDiv.innerHTML = announcement.contentPreview;
                    announcementContentDiv.dataset.contentPreview = announcement.contentPreview;

                    // --- 公告標題點擊事件監聽器 ---
                    announcementHeader.addEventListener('click', () => {
                        const attachmentUrlToDisplay = announcement.attachmentUrl; // 從加載的公告數據中獲取附件 URL

                        if (announcementItem.classList.contains('fully-expanded') || announcementItem.classList.contains('partial-expanded')) {
                            // 從 Partial-Expanded 或 Fully-Expanded -> Closed
                            announcementItem.classList.remove('fully-expanded');
                            announcementItem.classList.remove('partial-expanded');
                            announcementItem.classList.add('closed');

                            // 重新設定為預覽內容，這會清除動態添加的附件
                            announcementContentDiv.innerHTML = announcementContentDiv.dataset.contentPreview;

                            // 移除任何已添加的附件元素 (僅當回到 closed 狀態時才移除，確保不會被重新添加)
                            const existingAttachment = announcementContentDiv.querySelector('.announcement-attachment-wrapper');
                            if (existingAttachment) {
                                existingAttachment.remove();
                            }

                            if (toggleButton) {
                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';
                            }

                        } else {
                            // 從 Closed -> Partial-Expanded (首次展開)
                            announcementItem.classList.remove('closed');
                            announcementItem.classList.add('partial-expanded');
                            announcementContentDiv.innerHTML = announcementContentDiv.dataset.contentPreview; // 顯示預覽內容

                            // *** 確保在進入 Partial-Expanded 狀態時就添加附件 ***
                            // 檢查是否已有附件，避免重複添加
                            if (attachmentUrlToDisplay && !announcementContentDiv.querySelector('.announcement-attachment-wrapper')) {
                                const attachmentElement = createAttachmentElement(attachmentUrlToDisplay, announcement.title);
                                if (attachmentElement) {
                                    announcementContentDiv.appendChild(attachmentElement); // 將附件元素添加到內容區域
                                }
                            }
                            // *** 結束添加 ***

                            if (toggleButton) {
                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';
                            }
                        }
                    });

                    // --- "點擊展開/收起" 按鈕點擊事件監聽器 ---
                    if (toggleButton) { // 只有當內容需要展開時才會有這個按鈕
                        toggleButton.addEventListener('click', async (event) => {
                            event.stopPropagation(); // 阻止事件冒泡到父元素（如公告標題）

                            const currentAnnouncementId = announcementItem.dataset.announcementId;
                            const attachmentUrlToDisplay = announcement.attachmentUrl; // 獲取附件 URL

                            if (announcementItem.classList.contains('fully-expanded')) {
                                // 從 Fully-Expanded -> Partial-Expanded (收起)
                                announcementItem.classList.remove('fully-expanded');
                                announcementItem.classList.add('partial-expanded');

                                // 重新設定為預覽內容。這會清空 div 中的所有內容。
                                announcementContentDiv.innerHTML = announcementContentDiv.dataset.contentPreview;

                                // *** 關鍵修正：重新將附件添加到 partial-expanded 狀態的內容中 ***
                                // 由於 innerHTML 被重置，需要重新創建並添加附件
                                if (attachmentUrlToDisplay) {
                                    const attachmentElement = createAttachmentElement(attachmentUrlToDisplay, announcement.title);
                                    if (attachmentElement) {
                                        announcementContentDiv.appendChild(attachmentElement);
                                    }
                                }
                                // *** 結束關鍵修正 ***

                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';

                            } else {
                                // 從 Partial-Expanded -> Fully-Expanded (展開)
                                announcementItem.classList.remove('partial-expanded');
                                announcementItem.classList.add('fully-expanded');
                                toggleButton.querySelector('span:first-child').textContent = '點擊收起 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(180deg)';

                                // 檢查快取中是否有完整內容，如果沒有則向後端請求
                                if (!announcementCache[currentAnnouncementId] || !announcementCache[currentAnnouncementId].fullContent) {
                                    announcementContentDiv.innerHTML = '<p>載入完整內容中...</p>'; // 顯示載入提示
                                    try {
                                        const detailResponse = await fetch(`${API_BASE_URL}/Announcements/${currentAnnouncementId}`);
                                        if (!detailResponse.ok) {
                                            throw new Error(`Failed to fetch detail for ID: ${currentAnnouncementId}`);
                                        }
                                        const detailData = await detailResponse.json();
                                        // 將完整內容和附件 URL 存入快取
                                        announcementCache[currentAnnouncementId] = {
                                            fullContent: detailData.content || '',
                                            attachmentUrl: detailData.attachmentUrl || ''
                                        };
                                    } catch (error) {
                                        console.error(`Error fetching full content for announcement ID ${currentAnnouncementId}:`, error);
                                        announcementContentDiv.innerHTML = '<p>載入內容失敗。</p>';
                                        announcementCache[currentAnnouncementId] = { fullContent: '<p>載入內容失敗。</p>', attachmentUrl: '' }; // 快取錯誤狀態
                                    }
                                }

                                // 顯示完整內容
                                announcementContentDiv.innerHTML = announcementCache[currentAnnouncementId].fullContent;

                                // 處理附件 (統一使用 createAttachmentElement 函數，確保即使替換 innerHTML 也能重新添加)
                                const attachmentUrl = announcementCache[currentAnnouncementId].attachmentUrl;
                                if (attachmentUrl) {
                                    // 由於 innerHTML 被重置了，這裡需要確保重新添加附件
                                    const attachmentElement = createAttachmentElement(attachmentUrl, announcement.title);
                                    if (attachmentElement) {
                                        announcementContentDiv.appendChild(attachmentElement); // 將附件添加到內容的底部
                                    }
                                }
                            }
                        });
                    }
                }

                // 渲染分頁控制項
                renderPagination(data.currentPage, data.totalPages);
            } else {
                // 如果沒有公告數據
                announcementListContainer.innerHTML = '<p>目前沒有公告。</p>';
            }

            currentPage = page; // 更新當前頁碼

            // 確保分頁容器在有數據時顯示
            const paginationDiv = document.querySelector('.pagination');
            if (paginationDiv) {
                paginationDiv.style.display = 'flex'; // 顯示分頁
            }

        } catch (error) {

            console.error('Error loading announcements:', error);
            announcementListContainer.innerHTML = '<p>載入公告失敗，請稍後再試。</p>';
            clearContent(paginationContainer); // 載入失敗時清空分頁
            // 載入失敗時隱藏分頁
            const paginationDiv = document.querySelector('.pagination');
            if (paginationDiv) {
                paginationDiv.style.display = 'none';
            }
        }
    }

    /**
     * 渲染分頁連結。
     * @param {number} currentPage - 當前頁碼。
     * @param {number} totalPages - 總頁數。
     */
    function renderPagination(currentPage, totalPages) {
        clearContent(paginationContainer); // 清空現有分頁
        if (totalPages <= 1) return; // 如果只有一頁或沒有，不顯示分頁

        // 添加「上一頁」按鈕
        const prevButton = document.createElement('a');
        prevButton.href = '#';
        prevButton.classList.add('page-link');
        if (currentPage === 1) {
            prevButton.classList.add('disabled'); // 禁用樣式
            prevButton.style.pointerEvents = 'none'; // 禁用點擊事件
        }
        prevButton.textContent = '上一頁';
        prevButton.addEventListener('click', (e) => {
            e.preventDefault();
            if (currentPage > 1) {
                loadAnnouncements(currentPage - 1);
            }
        });
        paginationContainer.appendChild(prevButton);

        // 添加頁碼連結
        for (let i = 1; i <= totalPages; i++) {
            const pageLink = document.createElement('a');
            pageLink.href = '#';
            pageLink.classList.add('page-link');
            if (i === currentPage) {
                pageLink.classList.add('active'); // 當前頁碼設置 active 樣式
            }
            pageLink.textContent = i;
            pageLink.dataset.page = i; // 儲存頁碼
            pageLink.addEventListener('click', (e) => {
                e.preventDefault();
                loadAnnouncements(parseInt(e.target.dataset.page));
            });
            paginationContainer.appendChild(pageLink);
        }

        // 添加「下一頁」按鈕
        const nextButton = document.createElement('a');
        nextButton.href = '#';
        nextButton.classList.add('page-link');
        if (currentPage === totalPages) {
            nextButton.classList.add('disabled'); // 禁用樣式
            nextButton.style.pointerEvents = 'none'; // 禁用點擊事件
        }
        nextButton.textContent = '下一頁';
        nextButton.addEventListener('click', (e) => {
            e.preventDefault();
            if (currentPage < totalPages) {
                loadAnnouncements(currentPage + 1);
            }
        });
        paginationContainer.appendChild(nextButton);
    }

    /**
     * 切換主要內容區塊的顯示。
     * @param {string} targetId - 要顯示的內容區塊的 ID (例如 'announcements', 'terms-of-service')。
     */
    async function switchContent(targetId) {
        // 隱藏所有內容區塊並移除 active 類別
        contentSections.forEach(section => {
            section.classList.remove('active');
            section.style.display = 'none'; // 確保元素不佔用空間
        });

        // 移除所有導航項目的 active 類別
        navItems.forEach(item => {
            item.classList.remove('active');
        });

        // 顯示目標內容區塊
        const targetSection = document.getElementById(targetId);
        if (targetSection) {
            targetSection.classList.add('active');
            targetSection.style.display = 'block'; // 顯示目標元素

            // 如果目標是公告列表，則載入公告
            if (targetId === 'announcements') {
                await loadAnnouncements(1); // 預設載入第一頁公告
                // 確保分頁在公告區塊顯示時是可見的
                const paginationDiv = document.querySelector('.pagination');
                if (paginationDiv) {
                    paginationDiv.style.display = 'flex';
                }
            } else {
                // 如果切換到非公告區塊，則隱藏分頁
                const paginationDiv = document.querySelector('.pagination');
                if (paginationDiv) {
                    paginationDiv.style.display = 'none';
                }
            }
        }

        // 為點擊的導航項目添加 active 類別
        const activeNavItem = document.querySelector(`.sidebar .nav-item[data-target="${targetId}"]`);
        if (activeNavItem) {
            activeNavItem.classList.add('active');
        }
    }


    // === 事件監聽器和初始呼叫 ===

    // 為側邊欄導航項目添加點擊事件監聽器
    navItems.forEach(item => {
        item.addEventListener('click', (event) => {
            event.preventDefault(); // 阻止默認行為
            const targetId = item.dataset.target; // 從 data-target 屬性獲取目標 ID
            if (targetId) {
                switchContent(targetId); // 切換內容
            }
        });
    });

 
    switchContent(initialTargetId);
});