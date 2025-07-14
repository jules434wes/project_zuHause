// wwwroot/js/Announcement.js

document.addEventListener('DOMContentLoaded', () => {
    const navItems = document.querySelectorAll('.sidebar .nav-item');
    const contentSections = document.querySelectorAll('.main-content .content-section');
    const announcementListContainer = document.querySelector('.announcement-list');
    const paginationContainer = document.querySelector('.pagination');

    const API_BASE_URL = '/api/Tenant'; // API 基礎 URL
    const MAX_PARTIAL_LENGTH = 100; // 定義部分展開的字數限制

    let currentPage = 1; // 當前公告頁碼
    const announcementCache = {}; // 新增：用於快取已載入的公告詳細內容 {id: {fullContent: '...', attachmentUrl: '...'}}

    // 輔助函數：清除指定容器的內容
    function clearContent(container) {
        if (container) {
            container.innerHTML = '';
        }
    }

    // 輔助函數：檢查 URL 是否為圖片
    function isImageUrl(url) {
        return /\.(jpeg|jpg|gif|png|webp|bmp|svg)$/i.test(url);
    }

    // 輔助函數：創建附件的 HTML 元素
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

            // 添加點擊放大功能 (Lightbox / Modal)
            img.addEventListener('click', (e) => {
                e.stopPropagation(); // 防止點擊圖片時也觸發外部事件
                const modal = document.createElement('div');
                modal.classList.add('image-modal');
                modal.innerHTML = `<span class="close-button">&times;</span><img class="modal-content" src="${url}">`;
                document.body.appendChild(modal);

                modal.style.display = 'flex'; // 使用 flex 方便圖片居中

                const closeButton = modal.querySelector('.close-button');
                closeButton.addEventListener('click', () => {
                    modal.style.display = 'none';
                    modal.remove();
                });

                // 點擊 modal 外部也關閉
                modal.addEventListener('click', (e) => {
                    if (e.target === modal) {
                        modal.style.display = 'none';
                        modal.remove();
                    }
                });
            });
        } else {
            const link = document.createElement('a');
            link.href = url;
            link.textContent = '查看附件';
            link.target = '_blank';
            link.classList.add('announcement-attachment-link');
            attachmentWrapper.appendChild(link);
        }
        return attachmentWrapper;
    }

    // 載入公告列表的函數
    async function loadAnnouncements(page) {
        clearContent(announcementListContainer); // 清空現有公告
        clearContent(paginationContainer);     // 清空分頁

        announcementListContainer.innerHTML = '<p>載入中，請稍候...</p>'; // 顯示載入提示

        try {
            const response = await fetch(`${API_BASE_URL}/Announcements/list?pageNumber=${page}&pageSize=6`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();

            if (data.announcements && data.announcements.length > 0) {
                clearContent(announcementListContainer); // 清除載入提示

                for (const announcement of data.announcements) { //
                    const announcementItem = document.createElement('div');
                    announcementItem.classList.add('announcement-item', 'closed'); // 初始狀態設為 closed
                    announcementItem.dataset.announcementId = announcement.id; // 儲存 ID 以便後續請求詳細內容

                    const isContentLong = announcement.contentPreview.length > MAX_PARTIAL_LENGTH; // 判斷是否需要展開按鈕

                    announcementItem.innerHTML = `
                        <div class="announcement-header">
                            <h2 class="announcement-title">${announcement.moduleScope || ''}${announcement.title}</h2>
                            <span class="last-modified-date">最後修改日期: ${announcement.updatedAt}</span>
                        </div>
                        <div class="announcement-body">
                            <div class="announcement-content"></div>
                            ${isContentLong ? `<button type="button" class="toggle-button"><span>點擊展開 </span><span class="arrow"></span></button>` : ''}
                        </div>
                    `;
                    announcementListContainer.appendChild(announcementItem);

                    const announcementHeader = announcementItem.querySelector('.announcement-header');
                    const announcementBody = announcementItem.querySelector('.announcement-body');
                    const announcementContentDiv = announcementBody.querySelector('.announcement-content');
                    const toggleButton = announcementBody.querySelector('.toggle-button');

                    // 初始顯示部分內容或完整內容（如果內容不長）
                    announcementContentDiv.innerHTML = announcement.contentPreview;
                    // 在數據屬性中儲存 contentPreview，方便切換回 partial 狀態
                    announcementContentDiv.dataset.contentPreview = announcement.contentPreview;


                    // --- Event Listener for announcement header ---
                    announcementHeader.addEventListener('click', () => {
                        // 切換公告項目的狀態
                        if (announcementItem.classList.contains('fully-expanded') || announcementItem.classList.contains('partial-expanded')) {
                            // 從 Partial-Expanded 或 Fully-Expanded -> Closed
                            announcementItem.classList.remove('fully-expanded');
                            announcementItem.classList.remove('partial-expanded');
                            announcementItem.classList.add('closed');
                            announcementContentDiv.innerHTML = announcementContentDiv.dataset.contentPreview; // 回到預覽內容
                            if (toggleButton) {
                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';
                            }
                            // 移除任何已添加的附件元素
                            const existingAttachment = announcementContentDiv.querySelector('.announcement-attachment-wrapper');
                            if (existingAttachment) {
                                existingAttachment.remove();
                            }

                        } else {
                            // 從 Closed -> Partial-Expanded
                            announcementItem.classList.remove('closed');
                            announcementItem.classList.add('partial-expanded');
                            announcementContentDiv.innerHTML = announcementContentDiv.dataset.contentPreview; // 顯示預覽內容

                            // *** 在這裡添加處理附件的程式碼塊，確保文字少時也能顯示圖片 ***
                            const attachmentUrlToDisplay = announcement.attachmentUrl; // 從加載的公告數據中獲取 URL

                            // 如果有附件 URL，並且該附件還沒有被添加到 DOM 中（避免重複添加）
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

                    // --- Event Listener for the "點擊展開/收起" button ---
                    if (toggleButton) { // Only attach if the button exists (i.e., content is long)
                        toggleButton.addEventListener('click', async (event) => {
                            event.stopPropagation(); // Prevent header click from also firing

                            const currentAnnouncementId = announcementItem.dataset.announcementId;

                            if (announcementItem.classList.contains('fully-expanded')) {
                                // 從 Fully-Expanded -> Partial-Expanded
                                announcementItem.classList.remove('fully-expanded');
                                announcementItem.classList.add('partial-expanded');
                                announcementContentDiv.innerHTML = announcementContentDiv.dataset.contentPreview; // 回到部分內容
                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';
                                // 移除任何已添加的附件元素
                                const existingAttachment = announcementContentDiv.querySelector('.announcement-attachment-wrapper');
                                if (existingAttachment) {
                                    existingAttachment.remove();
                                }
                            } else {
                                // 從 Partial-Expanded -> Fully-Expanded
                                announcementItem.classList.remove('partial-expanded');
                                announcementItem.classList.add('fully-expanded');
                                toggleButton.querySelector('span:first-child').textContent = '點擊收起 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(180deg)';

                                // 檢查快取中是否有完整內容和附件，如果沒有則請求
                                if (!announcementCache[currentAnnouncementId] || !announcementCache[currentAnnouncementId].fullContent) {
                                    announcementContentDiv.innerHTML = '<p>載入完整內容中...</p>'; // 顯示載入提示
                                    try {
                                        const detailResponse = await fetch(`${API_BASE_URL}/Announcements/${currentAnnouncementId}`);
                                        if (!detailResponse.ok) {
                                            throw new Error(`Failed to fetch detail for ID: ${currentAnnouncementId}`);
                                        }
                                        const detailData = await detailResponse.json();
                                        announcementCache[currentAnnouncementId] = {
                                            fullContent: detailData.content || '',
                                            attachmentUrl: detailData.attachmentUrl || ''
                                        };
                                    } catch (error) {
                                        console.error(`Error fetching full content for announcement ID ${currentAnnouncementId}:`, error);
                                        announcementContentDiv.innerHTML = '<p>載入內容失敗。</p>';
                                        announcementCache[currentAnnouncementId] = { fullContent: '<p>載入內容失敗。</p>', attachmentUrl: '' }; // 仍快取錯誤狀態
                                    }
                                }

                                // 顯示完整內容
                                announcementContentDiv.innerHTML = announcementCache[currentAnnouncementId].fullContent;

                                // 處理附件 (統一使用 createAttachmentElement 函數)
                                const attachmentUrl = announcementCache[currentAnnouncementId].attachmentUrl;
                                if (attachmentUrl) {
                                    // 先移除舊的附件（如果有的話，避免重複添加）
                                    const existingAttachment = announcementContentDiv.querySelector('.announcement-attachment-wrapper');
                                    if (existingAttachment) {
                                        existingAttachment.remove();
                                    }
                                    const attachmentElement = createAttachmentElement(attachmentUrl, announcement.title);
                                    if (attachmentElement) {
                                        announcementContentDiv.appendChild(attachmentElement); // 將附件添加到內容的底部
                                    }
                                }
                            }
                        });
                    }
                }

                renderPagination(data.currentPage, data.totalPages);
            } else {
                announcementListContainer.innerHTML = '<p>目前沒有公告。</p>';
            }

            currentPage = page; // 更新當前頁碼

            const paginationDiv = document.querySelector('.pagination');
            if (paginationDiv) {
                paginationDiv.style.display = 'flex'; // 確保分頁顯示
            }

        } catch (error) {
            console.error('Error loading announcements:', error);
            announcementListContainer.innerHTML = '<p>載入公告失敗，請稍後再試。</p>';
            clearContent(paginationContainer);
            const paginationDiv = document.querySelector('.pagination');
            if (paginationDiv) {
                paginationDiv.style.display = 'none'; // 載入失敗也隱藏分頁
            }
        }
    }

    // 渲染分頁按鈕的函數 (保持不變)
    function renderPagination(currentPage, totalPages) {
        clearContent(paginationContainer);
        if (totalPages <= 1) {
            paginationContainer.style.display = 'none';
            return;
        }

        paginationContainer.style.display = 'flex';

        // 添加「上一頁」按鈕
        if (currentPage > 1) {
            const prevPageButton = document.createElement('button');
            prevPageButton.textContent = '上一頁';
            prevPageButton.classList.add('page-button', 'page-link');
            prevPageButton.addEventListener('click', () => {
                loadAnnouncements(currentPage - 1);
            });
            paginationContainer.appendChild(prevPageButton);
        }

        // 渲染頁碼按鈕
        for (let i = 1; i <= totalPages; i++) {
            const pageButton = document.createElement('button');
            pageButton.textContent = i;
            pageButton.classList.add('page-button', 'page-link');
            if (i === currentPage) {
                pageButton.classList.add('active');
            }
            pageButton.addEventListener('click', () => {
                loadAnnouncements(i);
            });
            paginationContainer.appendChild(pageButton);
        }

        // 添加「下一頁」按鈕
        if (currentPage < totalPages) {
            const nextPageButton = document.createElement('button');
            nextPageButton.textContent = '下一頁';
            nextPageButton.classList.add('page-button', 'page-link');
            nextPageButton.addEventListener('click', () => {
                loadAnnouncements(currentPage + 1);
            });
            paginationContainer.appendChild(nextPageButton);
        }
    }

    // 處理導航切換內容的函數 (保持不變，因為現在公告都在同一個列表中)
    async function switchContent(targetId) {
        contentSections.forEach(section => section.classList.remove('active'));
        navItems.forEach(item => item.classList.remove('active'));

        const targetSection = document.getElementById(targetId);
        const targetNavItem = document.querySelector(`.nav-item[data-content-id="${targetId}"]`);

        if (targetSection) {
            targetSection.classList.add('active');
        }
        if (targetNavItem) {
            targetNavItem.classList.add('active');
        }

        if (targetId === 'announcements') {
            currentPage = 1;
            // 確保公告列表顯示，並隱藏詳細內容容器（此處已移除詳細內容容器的 JS 控制，因為現在只在單個 item 內部處理）
            announcementListContainer.style.display = 'block';
            loadAnnouncements(currentPage);
        } else {
            // 切換到非公告頁面時，隱藏分頁
            paginationContainer.style.display = 'none';
            // 清空公告列表，避免顯示舊內容 (這裡可以保留，確保切換後公告列表是空的)
            clearContent(announcementListContainer);
        }
    }

    // 為側邊欄導航項目添加點擊事件監聽器
    navItems.forEach(item => {
        item.addEventListener('click', (event) => {
            event.preventDefault();
            const targetId = item.dataset.contentId;
            switchContent(targetId);
        });
    });

    // 頁面載入後，預設顯示「公告」內容
    switchContent('announcements');
});