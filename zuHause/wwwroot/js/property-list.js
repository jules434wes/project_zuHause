// 房源列表頁面 JavaScript
document.addEventListener('DOMContentLoaded', function() {
    initSortSelect();
    initFavoriteButtons();
    initImageLazyLoading();
    initSearchForm();
});

// 排序選擇器
function initSortSelect() {
    const sortSelect = document.getElementById('sortSelect');
    if (!sortSelect) return;
    
    sortSelect.addEventListener('change', function() {
        const [sortBy, sortOrder] = this.value.split('-');
        const url = new URL(window.location);
        
        url.searchParams.set('search.SortBy', sortBy);
        url.searchParams.set('search.SortOrder', sortOrder);
        url.searchParams.set('page', '1'); // 重置到第一頁
        
        window.location.href = url.toString();
    });
}

// 收藏按鈕功能
function initFavoriteButtons() {
    const favoriteButtons = document.querySelectorAll('.btn-favorite');
    
    favoriteButtons.forEach(button => {
        button.addEventListener('click', async function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const propertyId = this.dataset.propertyId;
            const icon = this.querySelector('i');
            const card = this.closest('.property-card');
            
            try {
                // 添加載入狀態
                card.classList.add('loading');
                this.disabled = true;
                
                const response = await fetch(`/property/favorite/${propertyId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': getAntiForgeryToken()
                    }
                });
                
                const result = await response.json();
                
                if (result.success) {
                    // 更新 UI
                    if (result.isFavorite) {
                        icon.className = 'fas fa-heart';
                        this.classList.add('active');
                    } else {
                        icon.className = 'far fa-heart';
                        this.classList.remove('active');
                    }
                    
                    // 顯示簡短的成功提示
                    showQuickToast(result.isFavorite ? '已收藏' : '已取消收藏', 'success');
                } else {
                    showQuickToast(result.message || '操作失敗', 'error');
                }
            } catch (error) {
                console.error('收藏操作失敗:', error);
                showQuickToast('操作失敗', 'error');
            } finally {
                // 移除載入狀態
                card.classList.remove('loading');
                this.disabled = false;
            }
        });
    });
}

// 圖片懶加載
function initImageLazyLoading() {
    const images = document.querySelectorAll('.property-image img[loading="lazy"]');
    
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    
                    // 添加載入效果
                    img.style.opacity = '0';
                    img.style.transition = 'opacity 0.3s ease';
                    
                    img.onload = () => {
                        img.style.opacity = '1';
                    };
                    
                    img.onerror = () => {
                        img.src = '/images/no-image.jpg';
                        img.style.opacity = '1';
                    };
                    
                    imageObserver.unobserve(img);
                }
            });
        }, {
            rootMargin: '50px'
        });
        
        images.forEach(img => imageObserver.observe(img));
    }
}

// 搜尋表單增強
function initSearchForm() {
    const form = document.querySelector('.search-container form');
    if (!form) return;
    
    // 城市/區域聯動
    const citySelect = form.querySelector('select[name="search.CityId"]');
    const districtSelect = form.querySelector('select[name="search.DistrictId"]');
    
    if (citySelect && districtSelect) {
        citySelect.addEventListener('change', function() {
            updateDistrictOptions(this.value, districtSelect);
        });
    }
    
    // 價格範圍驗證
    const minPriceInput = form.querySelector('input[name="search.MinPrice"]');
    const maxPriceInput = form.querySelector('input[name="search.MaxPrice"]');
    
    if (minPriceInput && maxPriceInput) {
        minPriceInput.addEventListener('blur', validatePriceRange);
        maxPriceInput.addEventListener('blur', validatePriceRange);
    }
    
    function validatePriceRange() {
        const minPrice = parseInt(minPriceInput.value) || 0;
        const maxPrice = parseInt(maxPriceInput.value) || Infinity;
        
        if (minPrice > maxPrice && maxPrice > 0) {
            showQuickToast('最低租金不能高於最高租金', 'warning');
            minPriceInput.focus();
        }
    }
}

// 更新區域選項 (AJAX)
async function updateDistrictOptions(cityId, districtSelect) {
    if (!cityId) {
        districtSelect.innerHTML = '<option value="">選擇區域</option>';
        return;
    }
    
    try {
        districtSelect.disabled = true;
        districtSelect.innerHTML = '<option value="">載入中...</option>';
        
        const response = await fetch(`/api/districts?cityId=${cityId}`);
        const districts = await response.json();
        
        let options = '<option value="">選擇區域</option>';
        districts.forEach(district => {
            options += `<option value="${district.districtId}">${district.districtName}</option>`;
        });
        
        districtSelect.innerHTML = options;
    } catch (error) {
        console.error('載入區域失敗:', error);
        districtSelect.innerHTML = '<option value="">載入失敗</option>';
    } finally {
        districtSelect.disabled = false;
    }
}

// 獲取防偽令牌
function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

// 快速提示
function showQuickToast(message, type = 'info') {
    // 移除現有提示
    const existing = document.querySelector('.quick-toast');
    if (existing) existing.remove();
    
    const toast = document.createElement('div');
    toast.className = `quick-toast toast-${type}`;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${getToastColor(type)};
        color: white;
        padding: 8px 16px;
        border-radius: 4px;
        z-index: 9999;
        font-size: 14px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.15);
        transform: translateX(100%);
        transition: transform 0.3s ease;
    `;
    toast.textContent = message;
    
    document.body.appendChild(toast);
    
    // 動畫顯示
    requestAnimationFrame(() => {
        toast.style.transform = 'translateX(0)';
    });
    
    // 自動移除
    setTimeout(() => {
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 2000);
}

function getToastColor(type) {
    const colors = {
        success: '#22c55e',
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    };
    return colors[type] || colors.info;
}

// 無限滾動 (可選功能)
function initInfiniteScroll() {
    let loading = false;
    let hasMore = true;
    let currentPage = 1;
    
    const loadMore = async () => {
        if (loading || !hasMore) return;
        
        loading = true;
        const url = new URL(window.location);
        url.searchParams.set('page', currentPage + 1);
        
        try {
            const response = await fetch(url.toString(), {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const newCards = doc.querySelectorAll('.property-card');
            
            if (newCards.length === 0) {
                hasMore = false;
                return;
            }
            
            const grid = document.querySelector('.property-grid');
            newCards.forEach(card => {
                grid.appendChild(card);
            });
            
            currentPage++;
            
            // 重新初始化新加載的元素
            initFavoriteButtons();
            initImageLazyLoading();
            
        } catch (error) {
            console.error('載入更多失敗:', error);
        } finally {
            loading = false;
        }
    };
    
    // 滾動檢測
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                loadMore();
            }
        });
    }, {
        rootMargin: '100px'
    });
    
    // 觀察最後一個元素
    const lastCard = document.querySelector('.property-card:last-child');
    if (lastCard) {
        observer.observe(lastCard);
    }
}