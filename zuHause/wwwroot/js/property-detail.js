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
        this.setupActionButtons();
        this.initializeNavigation();
        this.updateSidebarPosition(this.isNavVisible); // 初始化時調用一次

        // 動態偵測視窗滾動與尺寸變化，確保側邊欄不被導航列遮蓋
        this.handleDynamicSidebarPosition = () => this.updateSidebarPosition(this.isNavVisible);
        window.addEventListener('scroll', this.handleDynamicSidebarPosition);
        window.addEventListener('resize', this.handleDynamicSidebarPosition);
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
        // 取得主導覽列高度（若找不到則使用預設值）
        const mainNav = document.querySelector('header');
        const mainNavHeight = mainNav ? mainNav.getBoundingClientRect().height : 72; // 主導覽列高度
        const quickNavHeight = 60; // 次導覽列高度（固定，若需要也可動態計算）
        const padding = 20; // 額外間距

        let topOffset;

        if (navVisible) {
            // 次導覽列可見時，側邊欄需位於主導覽列 + 次導覽列之下
            topOffset = mainNavHeight + quickNavHeight + padding;
        } else {
            // 只須位於主導覽列之下
            topOffset = mainNavHeight + padding;
        }

        // 設定側邊欄位置及可視高度
        this.sidebar.style.top = `${topOffset}px`;
        this.sidebar.style.maxHeight = `calc(100vh - ${topOffset + padding}px)`;
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
        const mainImage = document.getElementById('mainImage');
        const modalImage = document.getElementById('modalImage');
        const thumbnails = document.querySelectorAll('.thumbnail-image');

        // 主圖點擊放大
        if (mainImage && modalImage) {
            mainImage.addEventListener('click', () => {
                modalImage.src = mainImage.src;
                modalImage.alt = mainImage.alt;
            });
        }

        // 縮圖點擊切換主圖
        thumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', () => {
                const newSrc = thumbnail.getAttribute('data-image-src');
                const newAlt = thumbnail.alt;
                
                if (mainImage && newSrc) {
                    // 添加淡出效果
                    mainImage.style.opacity = '0.5';
                    
                    setTimeout(() => {
                        mainImage.src = newSrc;
                        mainImage.alt = newAlt;
                        mainImage.style.opacity = '1';
                    }, 150);
                }

                // 更新縮圖活躍狀態
                thumbnails.forEach(t => t.parentElement.classList.remove('active'));
                thumbnail.parentElement.classList.add('active');
            });
        });

        // 鍵盤導航
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                const modal = bootstrap.Modal.getInstance(document.getElementById('imageModal'));
                if (modal) modal.hide();
            }
        });
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

        // 回報按鈕
        const reportBtn = document.getElementById('reportBtn');
        if (reportBtn) {
            reportBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.showNotification('回報功能開發中...');
            });
        }

        // 申租按鈕
        const applyBtn = document.querySelector('.btn-apply-rent');
        if (applyBtn) {
            applyBtn.addEventListener('click', () => {
                this.showNotification('申租功能開發中...');
            });
        }

        // 預約看房按鈕
        const scheduleBtn = document.querySelector('.btn-schedule-visit');
        if (scheduleBtn) {
            scheduleBtn.addEventListener('click', () => {
                this.showNotification('預約看房功能開發中...');
            });
        }
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
        if (this.handleDynamicSidebarPosition) {
            window.removeEventListener('scroll', this.handleDynamicSidebarPosition);
            window.removeEventListener('resize', this.handleDynamicSidebarPosition);
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