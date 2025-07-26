using Microsoft.AspNetCore.Mvc;
using zuHause.Interfaces;
using zuHause.Models;

namespace zuHause.Controllers.Api
{
    /// <summary>
    /// 遷移管理 API 控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly ILocalToBlobMigrationService _migrationService;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(
            ILocalToBlobMigrationService migrationService,
            ILogger<MigrationController> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        /// <summary>
        /// 掃描本地圖片檔案
        /// </summary>
        [HttpPost("scan")]
        public async Task<ActionResult<LocalImageScanResult>> ScanLocalImages([FromBody] LocalImageScanOptions? options = null)
        {
            try
            {
                var result = await _migrationService.ScanLocalImagesAsync(options);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "掃描本地圖片時發生錯誤");
                return StatusCode(500, new { message = "掃描失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 開始遷移作業
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<MigrationSession>> StartMigration([FromBody] MigrationConfiguration config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.Name))
                {
                    return BadRequest(new { message = "遷移名稱不能為空" });
                }

                var session = await _migrationService.StartMigrationAsync(config);
                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "開始遷移時發生錯誤");
                return StatusCode(500, new { message = "啟動遷移失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 獲取所有遷移會話
        /// </summary>
        [HttpGet("sessions")]
        public async Task<ActionResult<List<MigrationSession>>> GetMigrationSessions()
        {
            try
            {
                var sessions = await _migrationService.GetMigrationSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取遷移會話時發生錯誤");
                return StatusCode(500, new { message = "獲取會話失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 獲取遷移進度
        /// </summary>
        [HttpGet("{migrationId}/progress")]
        public async Task<ActionResult<MigrationProgress>> GetMigrationProgress(string migrationId)
        {
            try
            {
                var progress = await _migrationService.GetMigrationProgressAsync(migrationId);
                return Ok(progress);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取遷移進度時發生錯誤: MigrationId={MigrationId}", migrationId);
                return StatusCode(500, new { message = "獲取進度失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 暫停遷移作業
        /// </summary>
        [HttpPost("{migrationId}/pause")]
        public async Task<ActionResult> PauseMigration(string migrationId)
        {
            try
            {
                var result = await _migrationService.PauseMigrationAsync(migrationId);
                if (!result)
                {
                    return BadRequest(new { message = "暫停失敗，可能任務不存在或狀態不正確" });
                }
                
                return Ok(new { message = "遷移已暫停" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "暫停遷移時發生錯誤: MigrationId={MigrationId}", migrationId);
                return StatusCode(500, new { message = "暫停失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 恢復遷移作業
        /// </summary>
        [HttpPost("{migrationId}/resume")]
        public async Task<ActionResult> ResumeMigration(string migrationId)
        {
            try
            {
                var result = await _migrationService.ResumeMigrationAsync(migrationId);
                if (!result)
                {
                    return BadRequest(new { message = "恢復失敗，可能任務不存在或狀態不正確" });
                }
                
                return Ok(new { message = "遷移已恢復" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "恢復遷移時發生錯誤: MigrationId={MigrationId}", migrationId);
                return StatusCode(500, new { message = "恢復遷移失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 取消遷移作業
        /// </summary>
        [HttpPost("{migrationId}/cancel")]
        public async Task<ActionResult> CancelMigration(string migrationId)
        {
            try
            {
                var result = await _migrationService.CancelMigrationAsync(migrationId);
                if (!result)
                {
                    return BadRequest(new { message = "取消失敗，可能任務不存在或已完成" });
                }
                
                return Ok(new { message = "遷移已取消" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消遷移時發生錯誤: MigrationId={MigrationId}", migrationId);
                return StatusCode(500, new { message = "取消失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 驗證遷移結果
        /// </summary>
        [HttpPost("{migrationId}/validate")]
        public async Task<ActionResult<MigrationValidationResult>> ValidateMigration(string migrationId)
        {
            try
            {
                var result = await _migrationService.ValidateMigrationAsync(migrationId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證遷移時發生錯誤: MigrationId={MigrationId}", migrationId);
                return StatusCode(500, new { message = "驗證失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 清理本地檔案
        /// </summary>
        [HttpPost("{migrationId}/cleanup")]
        public async Task<ActionResult<MigrationCleanupResult>> CleanupLocalFiles(string migrationId)
        {
            try
            {
                var result = await _migrationService.CleanupLocalFilesAsync(migrationId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理本地檔案時發生錯誤: MigrationId={MigrationId}", migrationId);
                return StatusCode(500, new { message = "清理失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 回滾遷移（刪除已上傳的 Blob 檔案）
        /// </summary>
        [HttpPost("{migrationId}/rollback")]
        public async Task<ActionResult> RollbackMigration(string migrationId)
        {
            try
            {
                var result = await _migrationService.RollbackMigrationAsync(migrationId);
                if (!result)
                {
                    return BadRequest(new { message = "回滾失敗，可能任務不存在或無法回滾" });
                }
                
                return Ok(new { message = "遷移已回滾" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回滾遷移時發生錯誤: MigrationId={MigrationId}", migrationId);
                return StatusCode(500, new { message = "回滾失敗", error = ex.Message });
            }
        }
    }
}