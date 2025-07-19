// 客服案件詳情頁面 JavaScript
// 全域函數 - 選擇回覆模板
function selectTemplate(templateId) {
    const templates = {
        template1: '親愛的用戶，感謝您的詢問。關於您提到的租金問題，我們網站上所標示的價格均為最新且最準確的資訊。租金已包含管理費，但不包含網路費用，網路需由承租方自行申請。希望這個回覆能幫助到您！',
        template2: '親愛的用戶，我們已收到您的預約看房申請，專員將會在 24 小時內透過電話與您聯繫，確認詳細時間與細節，請您留意來電。',
        template3: '親愛的用戶，關於您的帳號問題，為了更好地協助您，請您提供註冊時使用的 Email 或手機號碼，以便我們為您查詢。',
        template4: '親愛的用戶，感謝您的來信。關於您提到的租約問題，我們將協助您處理相關事宜。為了提供更精準的服務，請您提供租約編號，我們會儘快為您查詢並回覆。',
        template5: '親愛的用戶，關於您的傢俱訂單查詢，我們為您確認訂單狀態。請您提供訂單編號，以便我們為您查詢詳細的配送時間和處理進度。',
        template6: '親愛的用戶，感謝您的來信，我們已收到您的意見與建議。您的回饋對我們非常重要，我們會持續改善服務品質。如有其他問題歡迎隨時與我們聯繫。'
    };
    
    document.getElementById('replyContent').value = templates[templateId];
    
    // 關閉模板選擇 Modal
    var templateModal = bootstrap.Modal.getInstance(document.getElementById('templateModal'));
    templateModal.hide();
}

// 全域函數 - 篩選模板
function filterTemplates() {
    const category = document.getElementById('templateCategory').value;
    const templateItems = document.querySelectorAll('.template-item');
    
    templateItems.forEach(function(item) {
        const itemCategory = item.getAttribute('data-category');
        
        if (category === '' || itemCategory === category) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

document.addEventListener('DOMContentLoaded', function() {
    // 狀態變更事件
    document.getElementById('statusChange').addEventListener('change', function() {
        if (this.value) {
            // 更新確認 Modal 文字
            var statusText = this.options[this.selectedIndex].text;
            document.getElementById('newStatusText').textContent = statusText;
            
            // 顯示確認 Modal
            var confirmModal = new bootstrap.Modal(document.getElementById('confirmStatusChangeModal'), {
                backdrop: 'static',
                keyboard: false
            });
            confirmModal.show();
        }
    });

    // 確認變更狀態按鈕事件
    document.getElementById('confirmStatusChangeBtn').addEventListener('click', function() {
        var newStatus = document.getElementById('statusChange').value;
        console.log('變更案件狀態為:', newStatus);
        
        // 這裡應該發送請求到後端更新狀態
        // 目前只是示範，實際需要實作後端API
        
        // 關閉 Modal
        var confirmModal = bootstrap.Modal.getInstance(document.getElementById('confirmStatusChangeModal'));
        confirmModal.hide();
        
        // 顯示成功訊息
        alert('狀態已成功變更');
    });

    // 確認送出回覆按鈕事件
    document.getElementById('confirmReplyBtn').addEventListener('click', function() {
        var replyContent = document.getElementById('replyContent').value;
        
        if (!replyContent.trim()) {
            alert('請輸入回覆內容');
            return;
        }
        
        console.log('送出回覆:', replyContent);
        
        // 這裡應該發送請求到後端儲存回覆
        // 目前只是示範，實際需要實作後端API
        
        // 關閉 Modal
        var confirmModal = bootstrap.Modal.getInstance(document.getElementById('confirmReplyModal'));
        confirmModal.hide();
        
        // 顯示成功訊息並清空輸入框
        alert('回覆已成功送出');
        document.getElementById('replyContent').value = '';
    });

    // 取消狀態變更時重置選擇
    document.getElementById('confirmStatusChangeModal').addEventListener('hidden.bs.modal', function () {
        document.getElementById('statusChange').value = '';
    });
});