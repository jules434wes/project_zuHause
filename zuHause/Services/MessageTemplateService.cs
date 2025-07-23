using zuHause.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace zuHause.Services
{
    /// <summary>
    /// 訊息模板服務 - 處理參數化模板的解析和替換
    /// </summary>
    public class MessageTemplateService
    {
        private readonly ZuHauseContext _context;

        public MessageTemplateService(ZuHauseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根據模板ID和參數字典生成最終訊息
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <param name="parameters">參數字典</param>
        /// <returns>處理後的訊息內容</returns>
        public async Task<MessageResult> GenerateMessageAsync(int templateId, Dictionary<string, string> parameters)
        {
            var template = await _context.AdminMessageTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == templateId && t.IsActive == true);

            if (template == null)
            {
                return new MessageResult
                {
                    Success = false,
                    ErrorMessage = "找不到指定的模板或模板已停用"
                };
            }

            var processedContent = ReplaceParameters(template.TemplateContent, parameters);
            var processedtitle = ReplaceParameters(template.Title, parameters);

            return new MessageResult
            {
                Success = true,
                title = processedtitle,
                Content = processedContent,
                categoryCode = template.CategoryCode,
                MissingParameters = GetMissingParameters(template.TemplateContent, parameters)
                    .Concat(GetMissingParameters(template.Title, parameters))
                    .Distinct()
                    .ToList()
            };
        }

        /// <summary>
        /// 根據分類和標題搜尋模板並生成訊息
        /// </summary>
        /// <param name="categoryCode">分類代碼</param>
        /// <param name="titleKeyword">標題關鍵字</param>
        /// <param name="parameters">參數字典</param>
        public async Task<MessageResult> GenerateMessageByCategoryAsync(string categoryCode, string titleKeyword, Dictionary<string, string> parameters)
        {
            var template = await _context.AdminMessageTemplates
                .Where(t => t.CategoryCode == categoryCode.ToUpper() && 
                           t.Title.Contains(titleKeyword) && 
                           t.IsActive == true)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                return new MessageResult
                {
                    Success = false,
                    ErrorMessage = $"找不到分類為 {categoryCode} 且標題包含 '{titleKeyword}' 的模板"
                };
            }

            return await GenerateMessageAsync(template.TemplateId, parameters);
        }

        /// <summary>
        /// 取得模板中所有參數名稱
        /// </summary>
        /// <param name="templateId">模板ID</param>
        public async Task<List<string>> GetTemplateParametersAsync(int templateId)
        {
            var template = await _context.AdminMessageTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null) return new List<string>();

            var titleParams = ExtractParameters(template.Title);
            var contentParams = ExtractParameters(template.TemplateContent);

            return titleParams.Concat(contentParams).Distinct().ToList();
        }

        /// <summary>
        /// 取得指定分類的所有可用模板
        /// </summary>
        /// <param name="categoryCode">分類代碼</param>
        public async Task<List<TemplateInfo>> GetTemplatesByCategoryAsync(string categoryCode)
        {
            return await _context.AdminMessageTemplates
                .Where(t => t.CategoryCode == categoryCode.ToUpper() && t.IsActive == true)
                .Select(t => new TemplateInfo
                {
                    templateID = t.TemplateId,
                    title = t.Title,
                    Parameters = ExtractParameters(t.Title + " " + t.TemplateContent)
                })
                .ToListAsync();
        }

        /// <summary>
        /// 驗證模板格式和參數語法
        /// </summary>
        /// <param name="templateContent">模板內容</param>
        public TemplateValidationResult ValidateTemplate(string templateContent)
        {
            var result = new TemplateValidationResult { IsValid = true };

            // 檢查未配對的大括號
            var openBraces = templateContent.Count(c => c == '{');
            var closeBraces = templateContent.Count(c => c == '}');

            if (openBraces != closeBraces)
            {
                result.IsValid = false;
                result.Errors.Add("模板中有未配對的大括號");
            }

            // 檢查空參數
            var emptyParams = Regex.Matches(templateContent, @"\{\s*\}");
            if (emptyParams.Count > 0)
            {
                result.IsValid = false;
                result.Errors.Add("發現空的參數定義 {}");
            }

            // 檢查無效的參數格式
            var invalidParams = Regex.Matches(templateContent, @"\{[^}]*\{[^}]*\}");
            if (invalidParams.Count > 0)
            {
                result.IsValid = false;
                result.Errors.Add("發現嵌套的參數定義");
            }

            // 提取所有參數
            result.Parameters = ExtractParameters(templateContent);

            return result;
        }

        #region 私有方法

        /// <summary>
        /// 替換模板中的參數
        /// </summary>
        private string ReplaceParameters(string template, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(template)) return template;

            return Regex.Replace(template, @"\{([^}]+)\}", match =>
            {
                var paramName = match.Groups[1].Value.Trim();
                return parameters.TryGetValue(paramName, out var value) ? value : match.Value;
            });
        }

        /// <summary>
        /// 提取模板中的所有參數名稱
        /// </summary>
        private List<string> ExtractParameters(string template)
        {
            if (string.IsNullOrEmpty(template)) return new List<string>();

            var matches = Regex.Matches(template, @"\{([^}]+)\}");
            return matches.Cast<Match>()
                         .Select(m => m.Groups[1].Value.Trim())
                         .Distinct()
                         .ToList();
        }

        /// <summary>
        /// 取得缺少的參數列表
        /// </summary>
        private List<string> GetMissingParameters(string template, Dictionary<string, string> parameters)
        {
            var allParams = ExtractParameters(template);
            return allParams.Where(param => !parameters.ContainsKey(param)).ToList();
        }

        #endregion
    }

    #region 資料模型

    /// <summary>
    /// 訊息生成結果
    /// </summary>
    public class MessageResult
    {
        public bool Success { get; set; }
        public string title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string categoryCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> MissingParameters { get; set; } = new List<string>();
    }

    /// <summary>
    /// 模板資訊
    /// </summary>
    public class TemplateInfo
    {
        public int templateID { get; set; }
        public string title { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new List<string>();
    }

    /// <summary>
    /// 模板驗證結果
    /// </summary>
    public class TemplateValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Parameters { get; set; } = new List<string>();
    }

    #endregion
}