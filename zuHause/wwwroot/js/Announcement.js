// wwwroot/js/Announcement.js

document.addEventListener('DOMContentLoaded', () => {
    const navItems = document.querySelectorAll('.sidebar .nav-item');
    const contentSections = document.querySelectorAll('.main-content .content-section');
    const announcementListContainer = document.querySelector('.announcement-list');
    const paginationContainer = document.querySelector('.pagination');

    const API_BASE_URL = '/api/Tenant'; // API 基礎 URL
    const MAX_PARTIAL_LENGTH = 50; // 定義部分展開的字數限制

    let currentPage = 1; // 當前公告頁碼

    // 輔助函數：清除指定容器的內容
    function clearContent(container) {
        if (container) {
            container.innerHTML = '';
        }
    }

    // 載入公告列表的函數
    async function loadAnnouncements(page) {
        clearContent(announcementListContainer); // 清空現有公告
        clearContent(paginationContainer);       // 清空分頁

        announcementListContainer.innerHTML = '<p>載入中，請稍候...</p>'; // 顯示載入提示

        try {
            const response = await fetch(`${API_BASE_URL}/Announcements/list?pageNumber=${page}&pageSize=6`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();

            if (data.announcements && data.announcements.length > 0) {
                clearContent(announcementListContainer); // 清除載入提示

                for (const announcement of data.announcements) { // 使用 for...of 以便在循環中使用 await
                    const announcementItem = document.createElement('div');
                    announcementItem.classList.add('announcement-item', 'closed'); // 初始狀態設為 closed

                    // Fetch full content for each announcement initially, but do not display it yet.
                    let fullContent = '';
                    try {
                        const detailResponse = await fetch(`${API_BASE_URL}/Announcements/${announcement.id}`);
                        if (!detailResponse.ok) {
                            throw new Error(`Failed to fetch detail for ID: ${announcement.id}`);
                        }
                        const detailData = await detailResponse.json();
                        fullContent = detailData.content || ''; // Ensure content is a string
                    } catch (error) {
                        console.error(`Error fetching full content for announcement ID ${announcement.id}:`, error);
                        fullContent = '<p>載入內容失敗。</p>'; // fallback content as HTML
                    }

                    const isContentLong = fullContent.length > MAX_PARTIAL_LENGTH;
                    // Note: fullContent might contain HTML tags, so direct substring for partialContent
                    // might cut off tags. For robust solution, parse HTML or use a library.
                    // For now, assuming basic text, or if HTML is cut, it's okay for preview.
                    const partialContent = isContentLong ? fullContent.substring(0, MAX_PARTIAL_LENGTH) + '...' : fullContent;


                    // Initial HTML structure for each item
                    announcementItem.innerHTML = `
                        <div class="announcement-header">
                            <h2 class="announcement-title">${announcement.title}</h2>
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

                    // Store full content and partial content for easy toggling
                    // Using innerHTML to set, as fullContent might contain HTML
                    announcementContentDiv.dataset.fullContent = fullContent;
                    announcementContentDiv.dataset.partialContent = partialContent;


                    // Set initial content based on 'closed' state (CSS will hide it)
                    // When partial-expanded, this content will appear.
                    announcementContentDiv.innerHTML = partialContent;


                    // --- Event Listener for announcement header ---
                    announcementHeader.addEventListener('click', () => {
                        if (announcementItem.classList.contains('fully-expanded') || announcementItem.classList.contains('partial-expanded')) {
                            // From Partial-Expanded or Fully-Expanded -> Closed
                            announcementItem.classList.remove('fully-expanded');
                            announcementItem.classList.remove('partial-expanded');
                            announcementItem.classList.add('closed'); // Ensure 'closed' state is explicit
                            if (toggleButton) { // If button exists, reset its text/arrow
                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';
                            }
                            announcementContentDiv.innerHTML = partialContent; // Reset content to partial for next expansion
                        } else {
                            // From Closed -> Partial-Expanded
                            announcementItem.classList.remove('closed');
                            announcementItem.classList.add('partial-expanded');
                            announcementContentDiv.innerHTML = partialContent; // Display partial content
                            if (toggleButton) { // If button exists, ensure correct text/arrow
                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';
                            }
                        }
                    });

                    // --- Event Listener for the "點擊展開/收起" button ---
                    if (toggleButton) { // Only attach if the button exists (i.e., content is long)
                        toggleButton.addEventListener('click', (event) => {
                            event.stopPropagation(); // Prevent header click from also firing

                            if (announcementItem.classList.contains('fully-expanded')) {
                                // From Fully-Expanded -> Partial-Expanded
                                announcementItem.classList.remove('fully-expanded');
                                announcementItem.classList.add('partial-expanded');
                                announcementContentDiv.innerHTML = partialContent;
                                toggleButton.querySelector('span:first-child').textContent = '點擊展開 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(0deg)';
                            } else if (announcementItem.classList.contains('partial-expanded')) {
                                // From Partial-Expanded -> Fully-Expanded
                                announcementItem.classList.remove('partial-expanded');
                                announcementItem.classList.add('fully-expanded');
                                announcementContentDiv.innerHTML = fullContent; // Show full content
                                toggleButton.querySelector('span:first-child').textContent = '點擊收起 ';
                                toggleButton.querySelector('.arrow').style.transform = 'rotate(180deg)';
                            }
                            // No need for 'else' here, as button only exists in partial/full states.
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

    // 渲染分頁按鈕的函數 (與之前相同)
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

    // 處理導航切換內容的函數 (與之前相同)
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
            loadAnnouncements(currentPage);
        } else {
            const paginationDiv = document.querySelector('.pagination');
            if (paginationDiv) {
                paginationDiv.style.display = 'none';
            }
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