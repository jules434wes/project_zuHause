#!/bin/bash

# Azure Blob Storage 遷移 API 測試腳本
# 測試所有 MigrationController 端點功能

API_BASE="http://localhost:5000/api/migration"
CONTENT_TYPE="Content-Type: application/json"

echo "=== Azure Blob Storage 遷移 API 測試 ==="
echo "API Base URL: $API_BASE"
echo ""

# 函數：測試 API 端點
test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    
    echo "測試: $description"
    echo "方法: $method $endpoint"
    
    if [ -z "$data" ]; then
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}\n" -X $method "$API_BASE$endpoint" -H "$CONTENT_TYPE")
    else
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}\n" -X $method "$API_BASE$endpoint" -H "$CONTENT_TYPE" -d "$data")
    fi
    
    echo "回應: $response"
    echo "---"
    echo ""
}

# 1. 測試掃描本地圖片
echo "1. 掃描本地圖片"
SCAN_DATA='{
    "validateFileIntegrity": true,
    "maxConcurrency": 5,
    "excludePaths": [],
    "modifiedAfter": null,
    "modifiedBefore": null
}'
test_endpoint "POST" "/scan" "$SCAN_DATA" "掃描本地圖片檔案"

# 2. 測試開始遷移
echo "2. 開始遷移作業"
MIGRATION_DATA='{
    "name": "API測試遷移",
    "batchSize": 10,
    "maxConcurrency": 2,
    "deleteLocalFilesAfterMigration": false,
    "validateAfterUpload": true,
    "includeImageIds": [],
    "excludeImageIds": [],
    "entityTypes": [],
    "categories": []
}'
test_endpoint "POST" "/start" "$MIGRATION_DATA" "開始遷移作業"

# 3. 測試獲取遷移會話列表
echo "3. 獲取遷移會話"
test_endpoint "GET" "/sessions" "" "獲取所有遷移會話"

# 4. 測試獲取遷移進度（使用假的ID）
echo "4. 獲取遷移進度"
FAKE_MIGRATION_ID="test-migration-id"
test_endpoint "GET" "/$FAKE_MIGRATION_ID/progress" "" "獲取遷移進度（預期 404）"

# 5. 測試暫停遷移
echo "5. 暫停遷移"
test_endpoint "POST" "/$FAKE_MIGRATION_ID/pause" "" "暫停遷移作業（預期失敗）"

# 6. 測試恢復遷移
echo "6. 恢復遷移"
test_endpoint "POST" "/$FAKE_MIGRATION_ID/resume" "" "恢復遷移作業（預期失敗）"

# 7. 測試取消遷移
echo "7. 取消遷移"
test_endpoint "POST" "/$FAKE_MIGRATION_ID/cancel" "" "取消遷移作業（預期失敗）"

# 8. 測試驗證遷移
echo "8. 驗證遷移"
test_endpoint "POST" "/$FAKE_MIGRATION_ID/validate" "" "驗證遷移結果（預期 404）"

# 9. 測試清理本地檔案
echo "9. 清理本地檔案"
test_endpoint "POST" "/$FAKE_MIGRATION_ID/cleanup" "" "清理本地檔案（預期 404）"

# 10. 測試回滾遷移
echo "10. 回滾遷移"
test_endpoint "POST" "/$FAKE_MIGRATION_ID/rollback" "" "回滾遷移（預期失敗）"

echo "=== API 測試完成 ==="
echo ""
echo "預期結果："
echo "- 掃描和開始遷移應該返回 200"
echo "- 獲取會話應該返回 200 和空陣列或現有會話"
echo "- 其他測試使用假ID，預期返回 404 或錯誤狀態"
echo ""
echo "請檢查上述回應是否符合預期。"