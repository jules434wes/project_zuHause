using Microsoft.AspNetCore.Mvc;

namespace zuHause.Controllers
{
    /// <summary>
    /// 暫時的調試控制器 - 用於測試Session轉導機制
    /// </summary>
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult CheckSession()
        {
            var lastModule = HttpContext.Session.GetString("LastModule");
            var referer = Request.Headers["Referer"].ToString();
            var currentPath = Request.Path.Value;
            
            return Json(new
            {
                CurrentPath = currentPath,
                LastModule = lastModule ?? "null",
                Referer = referer ?? "null",
                SessionId = HttpContext.Session.Id,
                SessionKeys = HttpContext.Session.Keys.ToArray()
            });
        }
        
        [HttpGet]
        public IActionResult SetTestModule(string module)
        {
            HttpContext.Session.SetString("LastModule", module);
            return Json(new { Message = $"設定LastModule為: {module}" });
        }
    }
}