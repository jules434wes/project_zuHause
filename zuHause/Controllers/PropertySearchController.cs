using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using zuHause.Data;
using zuHause.DTOs;
using zuHause.Models;

namespace zuHause.Controllers
{
    /// <summary>
    /// 房源搜尋 API 控制器 - 暫時註解以解決 DTO 不一致問題
    /// TODO: 需要重新實作以配合新的 DTO 架構
    /// </summary>
    [ApiController]
    [Route("api/properties")]
    public class PropertySearchController : ControllerBase
    {
        private readonly ZuHauseContext _context;
        private readonly ILogger<PropertySearchController> _logger;

        public PropertySearchController(ZuHauseContext context, ILogger<PropertySearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 暫時的佔位符方法 - 避免編譯錯誤
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult> SearchProperties()
        {
            // TODO: 重新實作房源搜尋功能以配合新的 DTO 架構
            return Ok(new { message = "房源搜尋功能暫時停用，等待重新實作" });
        }

        // 原有的實作已暫時註解，等待與新 DTO 架構整合後重新實作
        // 原檔案備份在 PropertySearchController.cs.backup
    }
}