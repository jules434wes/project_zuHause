// 房源詳細頁面 - 錨點導航模式，使用 Intersection Observer API
class PropertyDetail {
    constructor() {
        // DOM 元素
        this.quickNav = document.getElementById('quickNavigationTabs');
        this.sidebar = document.getElementById('propertySidebar');
        this.sentinel = document.getElementById('sentinelElement');
        this.navLinks = document.querySelectorAll('.quick-navigation-tabs .nav-link');
        this.sections = document.querySelectorAll('.detail-section[id]');
        
        // 狀態管理
        this.isNavVisible = false;
        this.currentActiveSection = 'house-info';
        this.isScrollingToSection = false;
        
        // 初始化
        this.init();
    }

    init() {
        this.setupIntersectionObserver();
        this.setupSectionObserver();
        this.setupAnchorNavigation();
        this.setupImageGallery();
        this.setupGalleryNavigation();
        this.setupActionButtons();
        this.initializeNavigation();
    }

    // 設置 Intersection Observer API 用於偵測快速導覽列顯示
    setupIntersectionObserver() {
        if (!this.sentinel) return;

        // 創建 intersection observer 實例用於導覽列顯示/隱藏
        const navObserverOptions = {
            root: null,
            rootMargin: '0px',
            threshold: 0
        };

        this.navObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                const shouldShowNav = !entry.isIntersecting;
                this.updateNavigationVisibility(shouldShowNav);
            });
        }, navObserverOptions);

        // 開始觀察 sentinel 元素
        this.navObserver.observe(this.sentinel);
    }

    // 設置區塊觀察器用於同步導覽狀態
    setupSectionObserver() {
        if (!this.sections.length) return;

        // 創建 intersection observer 實例用於偵測當前可見區塊
        const sectionObserverOptions = {
            root: null,
            rootMargin: '-20% 0px -70% 0px', // 只在區塊進入視口中間部分時觸發
            threshold: 0
        };

        this.sectionObserver = new IntersectionObserver((entries) => {
            if (this.isScrollingToSection) return; // 防止自動滾動時的干擾

            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.updateActiveNavigation(entry.target.id);
                }
            });
        }, sectionObserverOptions);

        // 開始觀察所有區塊
        this.sections.forEach(section => {
            this.sectionObserver.observe(section);
        });
    }

    // 更新導覽列顯示狀態
    updateNavigationVisibility(shouldShow) {
        if (shouldShow === this.isNavVisible) return;
        
        this.isNavVisible = shouldShow;
        
        // 更新快速導覽列顯示狀態
        this.quickNav?.classList.toggle('show', shouldShow);
        
        // 更新側邊欄位置 - 當次導覽列出現時，側邊欄緊貼其下方
        if (this.sidebar) {
            this.sidebar.classList.toggle('scrolled', shouldShow);
        }

        // 計算 sticky 位置
        this.updateSidebarPosition(shouldShow);
    }

    // 更新側邊欄 sticky 位置
    updateSidebarPosition(navVisible) {
        if (!this.sidebar) return;

        const mainNavHeight = 72; // 主導覽列高度
        const quickNavHeight = 60; // 快速導覽列高度
        const padding = 20; // 額外間距

        if (navVisible) {
            // 快速導覽列可見時，緊貼在其下方
            const topOffset = mainNavHeight + quickNavHeight + padding;
            this.sidebar.style.top = `${topOffset}px`;
            this.sidebar.style.maxHeight = `calc(100vh - ${topOffset + padding}px)`;
        } else {
            // 快速導覽列隱藏時，回到原始位置
            this.sidebar.style.top = `${padding}px`;
            this.sidebar.style.maxHeight = `calc(100vh - ${padding * 2}px)`;
        }
    }

    // 初始化導覽狀態
    initializeNavigation() {
        // 確保第一個導覽連結為活躍狀態
        this.updateActiveNavigation('house-info');
    }

    // 更新活躍的導覽項目
    updateActiveNavigation(sectionId) {
        if (sectionId === this.currentActiveSection) return;
        
        this.currentActiveSection = sectionId;
        
        // 更新導覽連結狀態
        this.navLinks.forEach(link => {
            const targetId = link.getAttribute('href')?.replace('#', '');
            const isActive = targetId === sectionId;
            
            link.classList.toggle('active', isActive);
            link.setAttribute('aria-selected', isActive.toString());
            link.setAttribute('tabindex', isActive ? '0' : '-1');
        });
    }

    // 設置錨點導覽
    setupAnchorNavigation() {
        this.navLinks.forEach(link => {
            // 點擊事件 - 滾動到對應區塊
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = link.getAttribute('href')?.replace('#', '');
                if (targetId) {
                    this.scrollToSection(targetId);
                    this.updateActiveNavigation(targetId);
                }
            });

            // 鍵盤導覽支援
            link.addEventListener('keydown', (e) => {
                this.handleNavKeydown(e, link);
            });
        });
    }

    // 滾動到指定區塊
    scrollToSection(sectionId) {
        const element = document.getElementById(sectionId);
        if (!element) return;

        // 設置滾動標記，防止 intersection observer 干擾
        this.isScrollingToSection = true;

        // 計算滾動位置（考慮固定導覽列高度）
        const navOffset = this.isNavVisible ? 140 : 80; // 根據導覽列狀態調整偏移
        const elementTop = element.offsetTop - navOffset;

        window.scrollTo({
            top: elementTop,
            behavior: 'smooth'
        });

        // 等待滾動完成後重置標記
        setTimeout(() => {
            this.isScrollingToSection = false;
        }, 1000);
    }

    // 處理導覽鍵盤事件
    handleNavKeydown(e, currentLink) {
        const links = Array.from(this.navLinks);
        const currentIndex = links.indexOf(currentLink);
        let targetIndex = currentIndex;

        switch (e.key) {
            case 'ArrowLeft':
                e.preventDefault();
                targetIndex = currentIndex > 0 ? currentIndex - 1 : links.length - 1;
                break;
            case 'ArrowRight':
                e.preventDefault();
                targetIndex = currentIndex < links.length - 1 ? currentIndex + 1 : 0;
                break;
            case 'Home':
                e.preventDefault();
                targetIndex = 0;
                break;
            case 'End':
                e.preventDefault();
                targetIndex = links.length - 1;
                break;
            case 'Enter':
            case ' ':
                e.preventDefault();
                currentLink.click();
                return;
            default:
                return;
        }

        // 聚焦到目標連結並觸發點擊
        const targetLink = links[targetIndex];
        targetLink.focus();
        targetLink.click();
    }

    // 設置圖片畫廊
    setupImageGallery() {
        // 獲取主要元素
        const mainImage = document.getElementById('mainImage');
        const thumbnails = document.querySelectorAll('.thumbnail-image');
        
        // 獲取模態框元素
        const imageModal = document.getElementById('imageModal');
        const modalImage = document.getElementById('modalImage');
        const modalCounter = document.getElementById('modalImageCounter');
        const modalPrevBtn = document.getElementById('modalPrevBtn');
        const modalNextBtn = document.getElementById('modalNextBtn');
        
        // 獲取主圖區域的按鈕
        const galleryPrev = document.querySelector('.gallery-prev');
        const galleryNext = document.querySelector('.gallery-next');
        
        if (!mainImage || !imageModal || !modalImage) return;
        
        // 創建 Bootstrap 模態框實例
        const modalInstance = new bootstrap.Modal(imageModal);
        
        // 圖片來源陣列：主圖 + 所有縮圖
        const allImages = [
            { src: mainImage.src, alt: mainImage.alt || '房源圖片' },
            ...Array.from(thumbnails).map(thumb => ({
                src: thumb.getAttribute('data-image-src'),
                alt: thumb.alt || '房源圖片'
            }))
        ].filter(img => img.src); // 過濾掉沒有 src 的項目
        
        // 模態框圖片索引
        let currentModalIndex = 0;
        
        // 模態框顯示時的處理
        imageModal.addEventListener('show.bs.modal', () => {
            // 設置初始圖片
            modalImage.src = allImages[currentModalIndex].src;
            modalImage.alt = allImages[currentModalIndex].alt;
            
            // 更新計數器
            if (modalCounter) {
                modalCounter.textContent = `${currentModalIndex + 1} / ${allImages.length}`;
            }
        });
        
        // 更新模態框圖片函數
        const updateModalImage = (newIndex) => {
            if (!modalImage || allImages.length === 0) return;
            
            // 計算新索引（循環）
            currentModalIndex = (newIndex + allImages.length) % allImages.length;
            
            // 添加淡出效果
            modalImage.classList.remove('fade-in');
            modalImage.classList.add('fade-out');
            
            // 使用動畫結束事件來確保平滑過渡
            const handleAnimationEnd = () => {
                // 更換圖片源
                modalImage.src = allImages[currentModalIndex].src;
                modalImage.alt = allImages[currentModalIndex].alt;
                
                // 添加淡入效果
                modalImage.classList.remove('fade-out');
                modalImage.classList.add('fade-in');
                
                // 更新計數器
                if (modalCounter) {
                    modalCounter.textContent = `${currentModalIndex + 1} / ${allImages.length}`;
                }
                
                // 移除事件監聽器
                modalImage.removeEventListener('animationend', handleAnimationEnd);
            };
            
            // 監聽動畫結束事件
            modalImage.addEventListener('animationend', handleAnimationEnd, { once: true });
        };
        
        // 更新主圖函數
        const updateMainImage = (src, alt) => {
            if (!mainImage || !src) return;
            
            // 添加淡出效果
            mainImage.classList.remove('fade-in');
            mainImage.classList.add('fade-out');
            
            // 使用動畫結束事件來確保平滑過渡
            const handleMainImageAnimationEnd = () => {
                mainImage.src = src;
                mainImage.alt = alt || '';
                
                // 添加淡入效果
                mainImage.classList.remove('fade-out');
                mainImage.classList.add('fade-in');
                
                // 移除事件監聽器
                mainImage.removeEventListener('animationend', handleMainImageAnimationEnd);
            };
            
            // 監聽動畫結束事件
            mainImage.addEventListener('animationend', handleMainImageAnimationEnd, { once: true });
        };
        
        // 主圖點擊放大
        mainImage.addEventListener('click', () => {
            currentModalIndex = 0; // 重置為主圖
            modalInstance.show();
        });
        
        // 縮圖點擊切換主圖
        thumbnails.forEach((thumbnail, idx) => {
            thumbnail.addEventListener('click', () => {
                const newSrc = thumbnail.getAttribute('data-image-src');
                const newAlt = thumbnail.alt;
                
                // 更新主圖
                updateMainImage(newSrc, newAlt);
                
                // 更新縮圖活躍狀態
                thumbnails.forEach(t => t.parentElement.classList.remove('active'));
                thumbnail.parentElement.classList.add('active');
                
                // 更新模態框索引，這樣點擊縮圖後打開模態框時會顯示正確的圖片
                currentModalIndex = idx + 1; // +1 因為主圖是 index 0
            });
            
            // 縮圖雙擊直接打開模態框
            thumbnail.addEventListener('dblclick', () => {
                currentModalIndex = idx + 1;
                modalInstance.show();
            });
        });
        
        // 模態框中的左右切換按鈕
        if (modalPrevBtn) {
            modalPrevBtn.addEventListener('click', () => {
                updateModalImage(currentModalIndex - 1);
            });
        }
        
        if (modalNextBtn) {
            modalNextBtn.addEventListener('click', () => {
                updateModalImage(currentModalIndex + 1);
            });
        }
        
        // 主圖區域的左右切換按鈕
        if (galleryPrev) {
            galleryPrev.addEventListener('click', (e) => {
                e.stopPropagation();
                e.preventDefault();
                
                // 使用 setupGalleryNavigation 中的方法來切換主圖
                const prevEvent = new Event('gallery:prev');
                document.dispatchEvent(prevEvent);
            });
        }
        
        if (galleryNext) {
            galleryNext.addEventListener('click', (e) => {
                e.stopPropagation();
                e.preventDefault();
                
                // 使用 setupGalleryNavigation 中的方法來切換主圖
                const nextEvent = new Event('gallery:next');
                document.dispatchEvent(nextEvent);
            });
        }
        
        // 鍵盤導航
        document.addEventListener('keydown', (e) => {
            // 模態框開啟時的鍵盤導航
            if (imageModal.classList.contains('show')) {
                if (e.key === 'Escape') {
                    modalInstance.hide();
                } else if (e.key === 'ArrowLeft') {
                    updateModalImage(currentModalIndex - 1);
                } else if (e.key === 'ArrowRight') {
                    updateModalImage(currentModalIndex + 1);
                }
            }
        });
    }

    // 設置主圖與縮圖左右切換
    setupGalleryNavigation() {
        const prevBtn = document.querySelector('.gallery-prev');
        const nextBtn = document.querySelector('.gallery-next');
        const thumbPrev = document.querySelector('.thumb-prev');
        const thumbNext = document.querySelector('.thumb-next');
        const thumbnailsWrapper = document.querySelector('.thumbnails-wrapper');

        const mainImg = document.getElementById('mainImage');
        const thumbs = Array.from(document.querySelectorAll('.thumbnail-image'));
        if(!prevBtn || !nextBtn || thumbs.length === 0 || !mainImg) return;

        // 圖片來源陣列：index 0 = 主圖，之後依縮圖順序
        const sources = [mainImg.getAttribute('src'), ...thumbs.map(t=>t.getAttribute('data-image-src'))];

        let currentIdx = 0; // 對應 sources

        const updateMain = ()=>{
            mainImg.src = sources[currentIdx];
            // 更新縮圖高亮（currentIdx 0 代表主圖，無對應縮圖）
            thumbs.forEach(t=>t.parentElement.classList.remove('active'));
            if(currentIdx>0){
                thumbs[currentIdx-1].parentElement.classList.add('active');
            }
        };

        // 主圖區域左右切換按鈕點擊
        prevBtn.addEventListener('click',()=>{
            currentIdx = (currentIdx -1 + sources.length)%sources.length;
            updateMain();
        });
        nextBtn.addEventListener('click',()=>{
            currentIdx = (currentIdx +1)%sources.length;
            updateMain();
        });
        
        // 監聽自定義事件
        document.addEventListener('gallery:prev', () => {
            currentIdx = (currentIdx -1 + sources.length)%sources.length;
            updateMain();
        });
        document.addEventListener('gallery:next', () => {
            currentIdx = (currentIdx +1)%sources.length;
            updateMain();
        });

        // 點縮圖
        thumbs.forEach((t,idx)=>{
            t.addEventListener('click',()=>{
                currentIdx = idx+1; // 因主圖在 index0
                updateMain();
            });
        });

        // 縮圖橫向滾動按鈕
        if(thumbnailsWrapper){
            thumbPrev?.addEventListener('click',()=>{
                thumbnailsWrapper.scrollBy({left:-150,behavior:'smooth'});
            });
            thumbNext?.addEventListener('click',()=>{
                thumbnailsWrapper.scrollBy({left:150,behavior:'smooth'});
            });
        }
    }

    // 設置操作按鈕
    setupActionButtons() {
        // 分享按鈕
        const shareBtn = document.getElementById('shareBtn');
        if (shareBtn) {
            shareBtn.addEventListener('click', () => {
                if (navigator.share) {
                    navigator.share({
                        title: document.title,
                        url: window.location.href
                    });
                } else {
                    // 備選方案：複製到剪貼板
                    navigator.clipboard.writeText(window.location.href).then(() => {
                        this.showNotification('連結已複製到剪貼板');
                    });
                }
            });
        }

        // 收藏按鈕
        const favoriteBtn = document.getElementById('favoriteBtn');
        if (favoriteBtn) {
            favoriteBtn.addEventListener('click', () => {
                this.toggleFavorite(favoriteBtn);
            });
        }

        // 開發訊息已移除，功能由 Modal 元件處理
    }

    // 切換收藏狀態
    async toggleFavorite(btn) {
        const propertyId = btn.getAttribute('data-property-id');
        const isActive = btn.classList.contains('active');
        
        try {
            // 模擬 API 調用
            await this.simulateApiCall();
            
            // 更新 UI
            const icon = btn.querySelector('i');
            
            if (isActive) {
                btn.classList.remove('active');
                icon.classList.remove('fas');
                icon.classList.add('far');
                btn.childNodes[btn.childNodes.length - 1].textContent = '收藏';
                this.showNotification('已取消收藏');
            } else {
                btn.classList.add('active');
                icon.classList.remove('far');
                icon.classList.add('fas');
                btn.childNodes[btn.childNodes.length - 1].textContent = '已收藏';
                this.showNotification('已加入收藏');
            }
        } catch (error) {
            this.showNotification('操作失敗，請稍後再試', 'error');
        }
    }

    // 模擬 API 調用
    simulateApiCall() {
        return new Promise((resolve) => {
            setTimeout(resolve, 300);
        });
    }

    // 顯示通知
    showNotification(message, type = 'success') {
        // 創建通知元素
        const notification = document.createElement('div');
        notification.className = `alert alert-${type === 'success' ? 'success' : 'danger'} position-fixed`;
        notification.style.cssText = `
            top: 100px;
            right: 20px;
            z-index: 9999;
            min-width: 250px;
            animation: slideInRight 0.3s ease-out;
        `;
        notification.textContent = message;

        // 添加樣式
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideInRight {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @keyframes slideOutRight {
                from { transform: translateX(0); opacity: 1; }
                to { transform: translateX(100%); opacity: 0; }
            }
        `;
        document.head.appendChild(style);

        document.body.appendChild(notification);

        // 自動移除
        setTimeout(() => {
            notification.style.animation = 'slideOutRight 0.3s ease-out';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 300);
        }, 3000);
    }

    // 清理方法（頁面卸載時調用）
    destroy() {
        if (this.navObserver) {
            this.navObserver.disconnect();
        }
        if (this.sectionObserver) {
            this.sectionObserver.disconnect();
        }
    }
}

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', () => {
    const propertyDetail = new PropertyDetail();
    
    // 頁面卸載時清理
    window.addEventListener('beforeunload', () => {
        propertyDetail.destroy();
    });
});

// 模組導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = PropertyDetail;
}