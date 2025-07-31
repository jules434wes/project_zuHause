/**
 * 房源付款功能 JavaScript
 * 整合 Stripe 處理房源刊登費用付款
 */

class PropertyPaymentManager {
    constructor() {
        this.stripe = null;
        this.isProcessing = false;
        this.init();
    }

    /**
     * 初始化付款管理器
     */
    init() {
        // 初始化 Stripe（如果存在）
        if (typeof Stripe !== 'undefined') {
            // 從 meta tag 取得 Stripe 公鑰，或使用配置中的公鑰
            const stripeKey = document.querySelector('meta[name="stripe-publishable-key"]')?.content || 
                            'pk_test_51RbtpfRwMK1W2l7jjzQmokPLxTYrXDsSE10FzcGvuyPnad2jnfTfvBBOWiGyJmfmpCMGTSZ0O5hJwTa77DIChcKQ00BTQmlBE9';
            this.stripe = Stripe(stripeKey);
        }

        // 綁定付款按鈕事件
        this.bindPaymentButtons();
    }

    /**
     * 綁定付款按鈕點擊事件
     */
    bindPaymentButtons() {
        // 使用事件委託處理動態生成的按鈕
        document.addEventListener('click', (e) => {
            if (e.target.matches('.property-payment-btn') || 
                e.target.closest('.property-payment-btn')) {
                
                e.preventDefault();
                const button = e.target.matches('.property-payment-btn') ? 
                              e.target : e.target.closest('.property-payment-btn');
                
                this.handlePaymentClick(button);
            }
        });
    }

    /**
     * 處理付款按鈕點擊
     */
    async handlePaymentClick(button) {
        if (this.isProcessing) {
            return;
        }

        const propertyId = parseInt(button.dataset.propertyId);
        const propertyTitle = button.dataset.propertyTitle || '房源';

        if (!propertyId) {
            this.showError('無效的房源資訊');
            return;
        }

        try {
            this.isProcessing = true;
            this.setButtonLoading(button, true);

            // 1. 先取得費用資訊
            const feeInfo = await this.getListingFee(propertyId);
            if (!feeInfo.success) {
                this.showError(feeInfo.message);
                return;
            }

            // 2. 顯示確認對話框
            const confirmed = await this.showPaymentConfirmation(propertyTitle, feeInfo);
            if (!confirmed) {
                return;
            }

            // 3. 建立付款 Session
            const sessionResult = await this.createPaymentSession(propertyId);
            if (!sessionResult.success) {
                this.showError(sessionResult.message);
                return;
            }

            // 4. 重定向到 Stripe Checkout
            if (this.stripe && sessionResult.sessionId) {
                const { error } = await this.stripe.redirectToCheckout({
                    sessionId: sessionResult.sessionId
                });

                if (error) {
                    this.showError(`付款頁面載入失敗：${error.message}`);
                }
            } else {
                // 備用方案：直接重定向
                if (sessionResult.sessionUrl) {
                    window.location.href = sessionResult.sessionUrl;
                } else {
                    this.showError('無法開啟付款頁面');
                }
            }

        } catch (error) {
            console.error('Payment process error:', error);
            this.showError('付款過程中發生錯誤，請稍後再試');
        } finally {
            this.isProcessing = false;
            this.setButtonLoading(button, false);
        }
    }

    /**
     * 取得房源刊登費用資訊
     */
    async getListingFee(propertyId) {
        try {
            const response = await fetch(`/landlord/property/${propertyId}/listing-fee`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Get listing fee error:', error);
            return { success: false, message: '無法取得費用資訊' };
        }
    }

    /**
     * 建立付款 Session
     */
    async createPaymentSession(propertyId) {
        try {
            const response = await fetch(`/landlord/property/${propertyId}/create-payment-session`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Create payment session error:', error);
            return { success: false, message: '無法建立付款 Session' };
        }
    }

    /**
     * 顯示付款確認對話框
     */
    async showPaymentConfirmation(propertyTitle, feeInfo) {
        return new Promise((resolve) => {
            // 使用 Bootstrap Modal 或 SweetAlert
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: '確認付款',
                    html: `
                        <div class="text-start">
                            <p><strong>房源：</strong>${propertyTitle}</p>
                            <p><strong>刊登方案：</strong>${feeInfo.planName}</p>
                            <p><strong>刊登天數：</strong>${feeInfo.listingDays} 天</p>
                            <p><strong>每日費用：</strong>NT$ ${feeInfo.pricePerDay.toLocaleString()}</p>
                            <hr>
                            <p class="h5"><strong>總計：NT$ ${feeInfo.amount.toLocaleString()}</strong></p>
                        </div>
                    `,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonText: '確認付款',
                    cancelButtonText: '取消',
                    confirmButtonColor: '#28a745',
                    cancelButtonColor: '#6c757d'
                }).then((result) => {
                    resolve(result.isConfirmed);
                });
            } else {
                // 備用的原生確認對話框
                const message = `確認付款房源「${propertyTitle}」的刊登費用？\n\n` +
                              `刊登方案：${feeInfo.planName}\n` +
                              `刊登天數：${feeInfo.listingDays} 天\n` +
                              `總計：NT$ ${feeInfo.amount.toLocaleString()}`;
                resolve(confirm(message));
            }
        });
    }

    /**
     * 設定按鈕載入狀態
     */
    setButtonLoading(button, isLoading) {
        const icon = button.querySelector('i');
        const text = button.querySelector('span') || button;

        if (isLoading) {
            button.disabled = true;
            if (icon) {
                icon.className = 'fas fa-spinner fa-spin me-1';
            }
            if (text.textContent) {
                text.textContent = '處理中...';
            }
        } else {
            button.disabled = false;
            if (icon) {
                icon.className = 'fas fa-credit-card me-1';
            }
            if (text.textContent) {
                text.textContent = '立即付款';
            }
        }
    }

    /**
     * 顯示錯誤訊息
     */
    showError(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: '付款錯誤',
                text: message,
                icon: 'error',
                confirmButtonText: '確定'
            });
        } else {
            alert(`付款錯誤：${message}`);
        }
    }

    /**
     * 顯示成功訊息
     */
    showSuccess(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: '付款成功',
                text: message,
                icon: 'success',
                confirmButtonText: '確定'
            });
        } else {
            alert(`付款成功：${message}`);
        }
    }

    /**
     * 取得防偽 Token
     */
    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
                     document.querySelector('meta[name="__RequestVerificationToken"]')?.content;
        return token || '';
    }
}

// 初始化付款管理器
document.addEventListener('DOMContentLoaded', function() {
    window.propertyPaymentManager = new PropertyPaymentManager();
    
    console.log('Property Payment Manager initialized');
});

// 全域函數供其他腳本使用
window.PropertyPaymentManager = PropertyPaymentManager;