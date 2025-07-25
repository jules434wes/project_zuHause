// 房源管理頁面 JavaScript - v1.2

document.addEventListener('DOMContentLoaded', function() {
    initPropertyManagement();
});

/**
 * 初始化房源管理頁面
 */
function initPropertyManagement() {
    console.log('房源管理頁面已載入');
    
    // 初始化卡片hover效果
    initCardHoverEffects();
    
    // 初始化工具提示
    initTooltips();
    
    // 初始化統計卡片動畫
    initStatsAnimation();
}

/**
 * 下架房源確認
 */
function confirmTakeDown(propertyId) {
    const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
    const confirmMessage = document.getElementById('confirmMessage');
    const confirmButton = document.getElementById('confirmButton');
    
    confirmMessage.textContent = '確定要下架此房源嗎？下架後將無法被租客搜尋到。';
    confirmButton.textContent = '確認下架';
    confirmButton.className = 'btn btn-danger';
    
    // 移除之前的事件監聽器
    const newConfirmButton = confirmButton.cloneNode(true);
    confirmButton.parentNode.replaceChild(newConfirmButton, confirmButton);
    
    // 添加新的事件監聽器
    newConfirmButton.addEventListener('click', function() {
        executePropertyAction('takedown', propertyId);
        modal.hide();
    });
    
    modal.show();
}

/**
 * 聯絡客服（BANNED狀態專用）
 */
function contactCustomerService(propertyId) {
    const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
    const confirmMessage = document.getElementById('confirmMessage');
    const confirmButton = document.getElementById('confirmButton');
    
    confirmMessage.innerHTML = '您的房源因檢舉過多被封鎖。<br>將為您轉接客服處理此問題。';
    confirmButton.textContent = '聯絡客服';
    confirmButton.className = 'btn btn-info';
    
    // 移除之前的事件監聽器
    const newConfirmButton = confirmButton.cloneNode(true);
    confirmButton.parentNode.replaceChild(newConfirmButton, confirmButton);
    
    // 添加新的事件監聽器
    newConfirmButton.addEventListener('click', function() {
        executePropertyAction('contact-service', propertyId);
        modal.hide();
    });
    
    modal.show();
}

/**
 * 執行房源操作
 */
function executePropertyAction(action, propertyId) {
    // 顯示載入狀態
    showActionLoading(propertyId, action);
    
    // TODO: 實際的API調用邏輯
    setTimeout(() => {
        switch(action) {
            case 'takedown':
                showSuccessMessage('房源已成功下架');
                break;
            case 'contact-service':
                showSuccessMessage('客服聯絡請求已發送，客服人員將於24小時內與您聯繫');
                break;
            default:
                showErrorMessage('操作失敗，請稍後再試');
        }
        
        // 3秒後重新載入頁面
        setTimeout(() => {
            location.reload();
        }, 3000);
        
    }, 1500); // 模擬API調用延遲
}

/**
 * 顯示操作載入狀態
 */
function showActionLoading(propertyId, action) {
    const card = document.querySelector(`[data-property-id="${propertyId}"]`);
    if (card) {
        card.classList.add('property-card-loading');
        
        // 禁用所有按鈕
        const buttons = card.querySelectorAll('.btn');
        buttons.forEach(btn => {
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>處理中...';
        });
    }
}

/**
 * 顯示成功訊息
 */
function showSuccessMessage(message) {
    showAlert(message, 'success');
}

/**
 * 顯示錯誤訊息
 */
function showErrorMessage(message) {
    showAlert(message, 'danger');
}

/**
 * 顯示警告訊息
 */
function showAlert(message, type) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // 插入到頁面頂部
    const container = document.querySelector('.container-fluid');
    const headerDiv = container.querySelector('.d-flex.justify-content-between');
    headerDiv.insertAdjacentHTML('afterend', alertHtml);
    
    // 自動消失
    setTimeout(() => {
        const alert = container.querySelector('.alert');
        if (alert) {
            alert.remove();
        }
    }, 5000);
}

/**
 * 初始化卡片hover效果
 */
function initCardHoverEffects() {
    const cards = document.querySelectorAll('.property-management-card, .property-card-simple, .property-card-compact');
    
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = this.classList.contains('property-card-compact') 
                ? 'translateY(-1px)' 
                : 'translateY(-3px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
}

/**
 * 初始化工具提示
 */
function initTooltips() {
    // 如果有Bootstrap 5的工具提示，初始化它們
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"], [title]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

/**
 * 初始化統計卡片動畫
 */
function initStatsAnimation() {
    const statsCards = document.querySelectorAll('.stats-card');
    
    // 使用Intersection Observer實現進入視窗時的動畫
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '0';
                entry.target.style.transform = 'translateY(20px)';
                
                setTimeout(() => {
                    entry.target.style.transition = 'all 0.6s ease';
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, 100);
            }
        });
    }, { threshold: 0.1 });
    
    statsCards.forEach(card => {
        observer.observe(card);
    });
}

/**
 * 圖片載入錯誤處理
 */
function handleImageError(img) {
    img.src = '/images/property-placeholder.jpg';
    img.onerror = null; // 防止無限循環
}

/**
 * 格式化數字顯示
 */
function formatNumber(num) {
    return num.toLocaleString('zh-TW');
}

/**
 * 複製房源連結到剪貼板
 */
function copyPropertyLink(propertyId) {
    const link = `${window.location.origin}/Property/Detail/${propertyId}`;
    
    if (navigator.clipboard) {
        navigator.clipboard.writeText(link).then(() => {
            showSuccessMessage('房源連結已複製到剪貼板');
        });
    } else {
        // 降級方案
        const textArea = document.createElement('textarea');
        textArea.value = link;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        showSuccessMessage('房源連結已複製到剪貼板');
    }
}

/**
 * 滾動到指定區塊
 */
function scrollToSection(sectionClass) {
    const section = document.querySelector(`.${sectionClass}`);
    if (section) {
        section.scrollIntoView({ 
            behavior: 'smooth',
            block: 'start'
        });
    }
}

/**
 * 鍵盤快捷鍵支援
 */
document.addEventListener('keydown', function(e) {
    // Ctrl+N 或 Cmd+N：新增房源
    if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
        e.preventDefault();
        window.location.href = '/Property/Create';
    }
    
    // Esc：關閉所有Modal
    if (e.key === 'Escape') {
        const modals = document.querySelectorAll('.modal.show');
        modals.forEach(modal => {
            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                modalInstance.hide();
            }
        });
    }
});

/**
 * 頁面載入完成後的最佳化
 */
window.addEventListener('load', function() {
    // 延遲載入非關鍵圖片
    const images = document.querySelectorAll('img[data-src]');
    images.forEach(img => {
        img.src = img.dataset.src;
        img.removeAttribute('data-src');
    });
    
    // 預載入常用圖示
    const iconPreload = [
        '/images/property-placeholder.jpg'
    ];
    
    iconPreload.forEach(src => {
        const img = new Image();
        img.src = src;
    });
});

// 導出函數供全域使用
window.confirmTakeDown = confirmTakeDown;
window.contactCustomerService = contactCustomerService;
window.copyPropertyLink = copyPropertyLink;
window.scrollToSection = scrollToSection;