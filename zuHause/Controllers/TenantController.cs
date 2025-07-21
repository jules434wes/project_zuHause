using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore; 
using zuHause.Models;

namespace zuhause.Controllers
{
    public class TenantController : Controller
    {
        // 將 DbContext 類型更正為 ZuHauseContext
        private readonly ZuHauseContext _context; // 依賴注入您的 ZuHauseContext

        // Controller 的建構子，用於接收 ZuHauseContext
        // 確保您的 Startup.cs (或 Program.cs) 已註冊 ZuHauseContext
        public TenantController(ZuHauseContext context)
        {
            _context = context;
        }

        // --- 現有的 View Action 方法 ---
        public IActionResult FrontPage()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }

        public IActionResult CollectionAndComparison()
        {
            return View();
        }












       
    }
}