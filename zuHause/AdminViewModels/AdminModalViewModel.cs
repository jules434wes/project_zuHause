using System.ComponentModel.DataAnnotations;

namespace zuHause.AdminViewModels
{
    /// <summary>
    /// Admin Modal 元件 ViewModel
    /// 用於統一管理 Modal 的配置與安全設定
    /// </summary>
    public class AdminModalViewModel
    {
        /// <summary>
        /// Modal ID (必填)
        /// </summary>
        [Required]
        public string ModalId { get; set; } = string.Empty;

        /// <summary>
        /// Modal 標題
        /// </summary>
        public string Title { get; set; } = "確認操作";

        /// <summary>
        /// Modal 內容 (HTML)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Modal 大小 (modal-sm, modal-lg, modal-xl)
        /// </summary>
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// 是否顯示關閉按鈕
        /// </summary>
        public bool ShowCloseButton { get; set; } = true;

        /// <summary>
        /// 是否需要確認勾選框
        /// </summary>
        public bool RequireConfirmCheckbox { get; set; } = true;

        /// <summary>
        /// 確認勾選框文字
        /// </summary>
        public string ConfirmCheckboxText { get; set; } = "我確認要執行此操作";

        /// <summary>
        /// 確認按鈕文字
        /// </summary>
        public string ConfirmButtonText { get; set; } = "確認";

        /// <summary>
        /// 取消按鈕文字
        /// </summary>
        public string CancelButtonText { get; set; } = "取消";

        /// <summary>
        /// 確認按鈕樣式 (btn-primary, btn-danger, btn-warning 等)
        /// </summary>
        public string ConfirmButtonStyle { get; set; } = "btn-danger";

        /// <summary>
        /// 是否自動關閉 (確認後自動關閉)
        /// </summary>
        public bool AutoClose { get; set; } = true;

        /// <summary>
        /// 確認回調函數名稱 (JavaScript)
        /// </summary>
        public string OnConfirmCallback { get; set; } = string.Empty;
    }

    /// <summary>
    /// 常用的 Modal 配置工廠類別
    /// </summary>
    public static class AdminModalFactory
    {
        /// <summary>
        /// 建立危險操作確認 Modal
        /// </summary>
        public static AdminModalViewModel CreateDangerConfirm(string modalId, string title, string message)
        {
            return new AdminModalViewModel
            {
                ModalId = modalId,
                Title = title,
                Content = $"<p>{message}</p><p class=\"text-danger fw-bold\">此操作無法復原，請謹慎確認。</p>",
                RequireConfirmCheckbox = true,
                ConfirmButtonStyle = "btn-danger",
                ConfirmButtonText = "確認執行"
            };
        }

        /// <summary>
        /// 建立一般確認 Modal
        /// </summary>
        public static AdminModalViewModel CreateGeneralConfirm(string modalId, string title, string message)
        {
            return new AdminModalViewModel
            {
                ModalId = modalId,
                Title = title,
                Content = $"<p>{message}</p>",
                RequireConfirmCheckbox = false,
                ConfirmButtonStyle = "btn-primary",
                ConfirmButtonText = "確認"
            };
        }

        /// <summary>
        /// 建立刪除確認 Modal
        /// </summary>
        public static AdminModalViewModel CreateDeleteConfirm(string modalId, string itemName)
        {
            return new AdminModalViewModel
            {
                ModalId = modalId,
                Title = "確認刪除",
                Content = $"<p>您確定要刪除「{itemName}」嗎？</p><p class=\"text-danger fw-bold\">此操作無法復原。</p>",
                RequireConfirmCheckbox = true,
                ConfirmButtonStyle = "btn-danger",
                ConfirmButtonText = "確認刪除",
                ConfirmCheckboxText = "我了解此操作無法復原"
            };
        }

        /// <summary>
        /// 建立審核確認 Modal
        /// </summary>
        public static AdminModalViewModel CreateReviewConfirm(string modalId, string action, string itemType)
        {
            var actionText = action switch
            {
                "approve" => "通過",
                "reject" => "拒絕",
                _ => "處理"
            };

            return new AdminModalViewModel
            {
                ModalId = modalId,
                Title = $"確認{actionText}",
                Content = $"<p>您確定要{actionText}此{itemType}嗎？</p>",
                RequireConfirmCheckbox = true,
                ConfirmButtonStyle = action == "approve" ? "btn-success" : "btn-warning",
                ConfirmButtonText = $"確認{actionText}"
            };
        }

        /// <summary>
        /// 建立大尺寸資訊 Modal
        /// </summary>
        public static AdminModalViewModel CreateInfoModal(string modalId, string title)
        {
            return new AdminModalViewModel
            {
                ModalId = modalId,
                Title = title,
                Size = "modal-lg",
                RequireConfirmCheckbox = false,
                ConfirmButtonStyle = "btn-secondary",
                ConfirmButtonText = "關閉",
                ShowCloseButton = true
            };
        }
    }
}