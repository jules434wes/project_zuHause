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

    // 初始化申請 / 預約通知互動
    initApplicationNotifications();
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
    img.src = '/images/default-picture.png';
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
        '/images/default-picture.png'
    ];
    
    iconPreload.forEach(src => {
        const img = new Image();
        img.src = src;
    });
});

/**
 * 初始化申請與預約通知互動
 */
function initApplicationNotifications() {
    // 綁定「查看詳情」按鈕
    document.querySelectorAll('.show-application-detail').forEach(btn => {
        btn.addEventListener('click', () => {
            const propertyId = parseInt(btn.dataset.propertyId);
            populateApplicationOffcanvas(propertyId, btn.closest('.application-summary').dataset.propertyAddress || '');
            const oc = bootstrap.Offcanvas.getOrCreateInstance(document.getElementById('applicationDetailsPanel'));
            oc.show();
        });
    });
}

/*********************** 示範資料 ***********************/
const demoRentalApplications = [
    { id: 1, propertyId: 1, applicantName: '張**', applicationDate: '2024/1/18 11:20', status: '申請中' },
    { id: 2, propertyId: 1, applicantName: '劉**', applicationDate: '2024/1/17 15:30', status: '申請中' },
    { id: 3, propertyId: 4, applicantName: '黃**', applicationDate: '2024/1/10 09:45', status: '已發出合約' },
    { id: 6, propertyId: 5, applicantName: '馬**', applicationDate: '2024/1/16 13:45', status: '申請中' },
    { id: 7, propertyId: 5, applicantName: '朱**', applicationDate: '2024/1/15 09:20', status: '已拒絕' },
    { id: 8, propertyId: 6, applicantName: '許**', applicationDate: '2024/1/19 10:30', status: '申請中' },
    { id: 9, propertyId: 7, applicantName: '趙**', applicationDate: '2024/1/16 14:20', status: '已發出合約' }
];

const demoViewingAppointments = [
    { id: 1, propertyId: 1, applicantName: '王**', requestDate: '2024/1/15 14:30', appointmentDate: '2024/1/20', appointmentTime: '14:00', status: '待處理' },
    { id: 2, propertyId: 1, applicantName: '李**', requestDate: '2024/1/16 09:15', appointmentDate: '2024/1/21', appointmentTime: '10:00', status: '待處理' },
    { id: 5, propertyId: 5, applicantName: '林**', requestDate: '2024/1/17 09:30', appointmentDate: '2024/1/22', appointmentTime: '14:00', status: '待處理' },
    { id: 6, propertyId: 5, applicantName: '張**', requestDate: '2024/1/15 16:20', appointmentDate: '2024/1/20', appointmentTime: '10:00', status: '已接受' }
];

/*********************** Helper ***********************/
function getApplicationBadgeClass(status) {
    switch (status) {
        case '申請中': return 'bg-primary text-white';
        case '已發出合約': return 'bg-success text-white';
        case '已拒絕': return 'bg-danger text-white';
        default: return 'bg-secondary text-white';
    }
}

function getViewingBadgeClass(status) {
    switch (status) {
        case '待處理': return 'bg-warning text-dark';
        case '已接受': return 'bg-success text-white';
        case '已拒絕': return 'bg-danger text-white';
        default: return 'bg-secondary text-white';
    }
}

/*********************** 詳情面板產生 ***********************/
function populateApplicationOffcanvas(propertyId, address) {
    // 設定標題地址
    document.getElementById('applicationDetailsAddress').textContent = address;

    // 過濾資料
    const applications = demoRentalApplications.filter(a => a.propertyId === propertyId);
    const appointments = demoViewingAppointments.filter(a => a.propertyId === propertyId);

    // 區分待處理 / 已處理
    const pendingApps = applications.filter(a => a.status === '申請中');
    const processedApps = applications.filter(a => a.status !== '申請中');

    const pendingViews = appointments.filter(a => a.status === '待處理');
    const processedViews = appointments.filter(a => a.status !== '待處理');

    let html = '';

    // 租屋申請
    if (applications.length > 0) {
        html += `<h4 class="fw-semibold text-dark mb-4 d-flex align-items-center application-section"><i class="fas fa-file-alt me-2"></i>租屋申請 (${applications.length})</h4>`;
        if (pendingApps.length > 0) {
            html += `<h5 class="text-sm fw-medium text-danger mb-3">待處理 (${pendingApps.length})</h5>`;
            html += buildApplicationList(pendingApps, true);
        }
        if (processedApps.length > 0) {
            html += `<h5 class="text-sm fw-medium text-muted mb-3 mt-4">已處理 (${processedApps.length})</h5>`;
            html += buildApplicationList(processedApps, false);
        }
    }

    // 預約看房
    if (appointments.length > 0) {
        html += `<h4 class="fw-semibold text-dark mb-4 mt-5 d-flex align-items-center application-section"><i class="fas fa-calendar-alt me-2"></i>預約看房 (${appointments.length})</h4>`;
        if (pendingViews.length > 0) {
            html += `<h5 class="text-sm fw-medium text-danger mb-3">待處理 (${pendingViews.length})</h5>`;
            html += buildViewingList(pendingViews, true);
        }
        if (processedViews.length > 0) {
            html += `<h5 class="text-sm fw-medium text-muted mb-3 mt-4">已處理 (${processedViews.length})</h5>`;
            html += buildViewingList(processedViews, false);
        }
    }

    if (applications.length === 0 && appointments.length === 0) {
        html = '<p class="text-center text-muted">此房源目前沒有任何申請或預約。</p>';
    }

    document.getElementById('applicationDetailsBody').innerHTML = html;
}

function buildApplicationList(list, isPending) {
    return list.map(app => {
        return `
            <div class="p-3 mb-2 rounded border application-details-item ${isPending ? 'pending' : ''} ${isPending ? 'bg-danger bg-opacity-10 border-danger' : 'bg-light'}">
                <div class="d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center gap-2">
                        <span class="fw-medium small">${app.applicantName}</span>
                        <span class="badge ${getApplicationBadgeClass(app.status)} small">${app.status}</span>
                    </div>
                    <button class="btn btn-outline-secondary btn-sm small">查看詳情</button>
                </div>
                <div class="text-muted small mt-1">申請時間：${app.applicationDate}</div>
            </div>`;
    }).join('');
}

function buildViewingList(list, isPending) {
    return list.map(v => {
        const actionButtons = isPending ? `
            <div class="d-flex gap-1 ms-4">
                <button class="btn btn-success btn-sm small" onclick="handleViewingAction(${v.id}, '接受')"><i class="fas fa-check-circle me-1"></i>接受</button>
                <button class="btn btn-outline-secondary btn-sm small" onclick="handleViewingAction(${v.id}, '更改')"><i class="fas fa-clock me-1"></i>更改</button>
                <button class="btn btn-outline-danger btn-sm small" onclick="handleViewingAction(${v.id}, '拒絕')"><i class="fas fa-times-circle me-1"></i>拒絕</button>
            </div>` : '';

        return `
            <div class="p-3 mb-2 rounded border application-details-item ${isPending ? 'pending' : ''} ${isPending ? 'bg-danger bg-opacity-10 border-danger' : 'bg-light'}">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <div class="d-flex align-items-center gap-2">
                        <span class="fw-medium small">${v.applicantName}</span>
                        <span class="badge ${getViewingBadgeClass(v.status)} small">${v.status}</span>
                    </div>
                </div>
                <div class="d-flex justify-content-between align-items-center">
                    <div class="text-muted small">
                        <div class="text-primary fw-medium">預約看房時間：${v.appointmentDate} ${v.appointmentTime}</div>
                        <div>申請時間：${v.requestDate}</div>
                    </div>
                    ${actionButtons}
                </div>
            </div>`;
    }).join('');
}

/*********************** Demo Handle ***********************/
function handleViewingAction(appointmentId, action) {
    console.log(`預約 ${appointmentId} - ${action}`);
    alert(`${action} 操作已觸發 (Demo)`);
}

// 暴露函式供HTML inline 呼叫
window.handleViewingAction = handleViewingAction;
window.confirmTakeDown = confirmTakeDown;
window.contactCustomerService = contactCustomerService;
window.copyPropertyLink = copyPropertyLink;
window.scrollToSection = scrollToSection;