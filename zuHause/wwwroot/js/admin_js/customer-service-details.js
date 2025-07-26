// 客服案件詳情頁面 JavaScript
let currentTicketId = null;

// 客服詳情管理物件
const CustomerServiceDetailsManager = {
    // 初始化
    init() {
        console.log('客服案件詳情頁面初始化');
        this.getCurrentTicketId();
        this.initEventHandlers();
        this.initTemplateSystem();
        this.loadTicketData();
    },

    // 取得當前案件ID
    getCurrentTicketId() {
        const urlParams = new URLSearchParams(window.location.search);
        currentTicketId = urlParams.get('ticketId');
        console.log('當前案件ID:', currentTicketId);
    },

    // 初始化事件處理器
    initEventHandlers() {
        // 儲存回覆按鈕
        const saveReplyBtn = document.getElementById('saveReplyBtn');
        if (saveReplyBtn) {
            saveReplyBtn.addEventListener('click', () => this.saveReply());
        }

        // 送出回覆按鈕
        const sendReplyBtn = document.getElementById('sendReplyBtn');
        if (sendReplyBtn) {
            sendReplyBtn.addEventListener('click', () => this.showConfirmModal());
        }

        // 確認送出回覆
        const confirmSendReply = document.getElementById('confirmSendReply');
        if (confirmSendReply) {
            confirmSendReply.addEventListener('click', () => this.sendReply());
        }
    },

    // 初始化模板系統
    initTemplateSystem() {
        const categorySelect = document.getElementById('templateCategory');
        const templateSelect = document.getElementById('messageTemplate');
        const insertBtn = document.getElementById('insertTemplateBtn');

        if (!categorySelect || !templateSelect || !insertBtn) return;

        // 類別選擇事件
        categorySelect.addEventListener('change', async (e) => {
            const categoryCode = e.target.value;
            if (categoryCode) {
                await this.loadTemplates(categoryCode);
                templateSelect.disabled = false;
                this.updateInsertButtonState();
            } else {
                this.resetTemplateSelects();
            }
        });

        // 模板選擇事件
        templateSelect.addEventListener('change', () => {
            this.updateInsertButtonState();
        });

        // 插入模板按鈕
        insertBtn.addEventListener('click', () => {
            this.insertTemplate();
        });
    },

    // 載入案件資料
    async loadTicketData() {
        if (!currentTicketId) return;

        try {
            const response = await fetch(`/Admin/GetCustomerServiceDetails?ticketId=${currentTicketId}`);
            const result = await response.json();

            if (result.success) {
                this.populateTicketData(result.data);
            } else {
                this.showError('載入案件資料失敗');
            }
        } catch (error) {
            console.error('載入案件資料時發生錯誤:', error);
            this.showError('載入案件資料時發生錯誤');
        }
    },

    // 填入案件資料
    populateTicketData(data) {
        // 填入現有回覆內容
        const replyContent = document.getElementById('replyContent');
        if (replyContent && data.replyContent) {
            replyContent.value = data.replyContent;
        }

        // 設定當前狀態
        const statusUpdate = document.getElementById('statusUpdate');
        if (statusUpdate && data.statusCode) {
            statusUpdate.value = data.statusCode;
        }
    },

    // 載入模板
    async loadTemplates(categoryCode) {
        try {
            const response = await fetch(`/Admin/GetMessageTemplates?categoryCode=${categoryCode}`);
            const result = await response.json();

            const templateSelect = document.getElementById('messageTemplate');
            templateSelect.innerHTML = '<option value="">請選擇模板</option>';

            if (result.success && result.data.length > 0) {
                result.data.forEach(template => {
                    const option = document.createElement('option');
                    option.value = template.templateId;
                    option.textContent = template.title;
                    option.dataset.content = template.content;
                    templateSelect.appendChild(option);
                });
            } else {
                templateSelect.innerHTML = '<option value="">此類別暫無可用模板</option>';
            }
        } catch (error) {
            console.error('載入模板失敗:', error);
            this.showError('載入模板時發生錯誤');
        }
    },

    // 插入模板
    insertTemplate() {
        const templateSelect = document.getElementById('messageTemplate');
        const selectedOption = templateSelect.options[templateSelect.selectedIndex];
        const replyContent = document.getElementById('replyContent');

        if (selectedOption && selectedOption.dataset.content) {
            const currentContent = replyContent.value;
            const templateContent = selectedOption.dataset.content;
            
            // 如果有現有內容，在末尾添加模板
            if (currentContent.trim()) {
                replyContent.value = currentContent + '\n\n' + templateContent;
            } else {
                replyContent.value = templateContent;
            }

            // 重設模板選擇
            this.resetTemplateSelects();
            
            // 聚焦到回覆內容區域
            replyContent.focus();
            
            console.log('模板已插入');
        }
    },

    // 重設模板選擇器
    resetTemplateSelects() {
        const categorySelect = document.getElementById('templateCategory');
        const templateSelect = document.getElementById('messageTemplate');
        const insertBtn = document.getElementById('insertTemplateBtn');

        categorySelect.value = '';
        templateSelect.innerHTML = '<option value="">請先選擇服務類別</option>';
        templateSelect.disabled = true;
        insertBtn.disabled = true;
    },

    // 更新插入按鈕狀態
    updateInsertButtonState() {
        const templateSelect = document.getElementById('messageTemplate');
        const insertBtn = document.getElementById('insertTemplateBtn');
        
        insertBtn.disabled = !templateSelect.value;
    },

    // 儲存回覆
    async saveReply() {
        const replyData = this.getReplyData();
        if (!this.validateReplyData(replyData)) return;

        try {
            const response = await fetch('/Admin/UpdateCustomerServiceReply', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: new URLSearchParams(replyData)
            });

            const result = await response.json();

            if (result.success) {
                this.showSuccess('回覆已儲存');
                console.log('回覆儲存成功');
            } else {
                this.showError(result.message || '儲存回覆失敗');
            }
        } catch (error) {
            console.error('儲存回覆時發生錯誤:', error);
            this.showError('儲存回覆時發生錯誤');
        }
    },

    // 顯示確認Modal
    showConfirmModal() {
        const replyData = this.getReplyData();
        if (!this.validateReplyData(replyData)) return;

        // 顯示Modal
        const modal = new bootstrap.Modal(document.getElementById('confirmReplyModal'));
        modal.show();
    },

    // 送出回覆
    async sendReply() {
        const replyData = this.getReplyData();
        if (!this.validateReplyData(replyData)) return;

        try {
            const response = await fetch('/Admin/UpdateCustomerServiceReply', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: new URLSearchParams(replyData)
            });

            const result = await response.json();

            if (result.success) {
                // 關閉Modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('confirmReplyModal'));
                modal.hide();

                this.showSuccess('回覆已送出');
                
                // 刷新頁面
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            } else {
                this.showError(result.message || '送出回覆失敗');
            }
        } catch (error) {
            console.error('送出回覆時發生錯誤:', error);
            this.showError('送出回覆時發生錯誤');
        }
    },

    // 取得回覆資料
    getReplyData() {
        return {
            ticketId: currentTicketId,
            replyContent: document.getElementById('replyContent')?.value || '',
            statusCode: document.getElementById('statusUpdate')?.value || 'PROGRESS'
        };
    },

    // 驗證回覆資料
    validateReplyData(data) {
        if (!data.ticketId) {
            this.showError('案件ID錯誤');
            return false;
        }

        if (!data.replyContent.trim()) {
            this.showError('請輸入回覆內容');
            document.getElementById('replyContent').focus();
            return false;
        }

        if (data.replyContent.length > 2000) {
            this.showError('回覆內容不能超過2000字');
            return false;
        }

        return true;
    },

    // 顯示成功訊息
    showSuccess(message) {
        console.log('成功:', message);
        alert(message);
    },

    // 顯示錯誤訊息
    showError(message) {
        console.error('錯誤:', message);
        alert(message);
    }
};

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', function() {
    CustomerServiceDetailsManager.init();
});