namespace zuHause.DTOs
{
    /// <summary>
    /// 自定義驗證結果類別
    /// </summary>
    public class PropertyValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<PropertyValidationError> Errors { get; set; } = new List<PropertyValidationError>();
    }

    /// <summary>
    /// 自定義驗證錯誤類別
    /// </summary>
    public class PropertyValidationError
    {
        public string? PropertyName { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}