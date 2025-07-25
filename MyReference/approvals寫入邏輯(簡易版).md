### 提交身分證驗證申請 (Identity Verification)

   - `approvals` 表 (建立一筆新的審核案件)
       - moduleCode: 'IDENTITY' (明確標示為身分驗證)
       - applicantMemberID: 提交申請的會員 ID
       - sourcePropertyID: NULL (因為此審核不針對任何房源)
       - statusCode: 'PENDING' (表示案件狀態為「待審核」)

   - `approvalItems` 表 (建立一筆初始操作紀錄)
       - approvalID: 關聯到上述 approvals 表中新建案件的 ID
       - actionType: 'SUBMIT' (表示此紀錄為使用者的「提交」動作)
       - actionBy: NULL (因為此操作由使用者發起，而非管理員)
       - actionNote: 可為 NULL 或記錄「使用者提交身分驗證申請」

   - `userUploads` 表 (建立至少兩筆檔案上傳紀錄)
       - 紀錄一 (身分證正面):
           - approvalID: 關聯到上述 approvals 表中新建案件的 ID
           - moduleCode: 'MemberInfo'
           - uploadTypeCode: 'USER_ID_FRONT'
           - filePath: 儲存身分證正面圖片的 URL 或路徑
       - 紀錄二 (身分證反面):
           - approvalID: 關聯到上述 approvals 表中新建案件的 ID
           - moduleCode: 'MemberInfo'
           - uploadTypeCode: 'USER_ID_BACK'
           - filePath: 儲存身分證反面圖片的 URL 或路徑

   - `members` 表
       - 在此提交階段，不會對此表進行任何修改。nationalIdNo 和 identityVerifiedAt 欄位會保持
         NULL，等待管理員審核通過後才會更新。

### 提交房東資格申請 (Landlord Verification)

  此流程的目的是讓已驗證身分的會員取得房東資格。

#####   情境 A：已通過身分驗證的會員申請

   - `approvals` 表 (建立一筆新的審核案件)
       - moduleCode: 'LANDLORD' (明確標示為房東資格驗證)
       - applicantMemberID: 提交申請的會員 ID
       - sourcePropertyID: NULL
       - statusCode: 'PENDING'

   - `approvalItems` 表 (建立一筆初始操作紀錄)
       - approvalID: 關聯到新建的房東審核案件 ID
       - actionType: 'SUBMIT'
       - actionBy: NULL

   - `members` 表
       - 在提交階段不會有任何變動。isLandlord 和 memberTypeID 會在審核通過後才更新。

#####   情境 B：未通過身分驗證的會員直接申請 (複合申請) → 可用前端設計避免此狀態被觸發

  根據規格文件，此時系統會將其視為一個複合申請，並同時建立兩筆獨立的審核案件：

   1. 一筆身分證驗證案件：完全比照上述第 1 點的所有資料表寫入操作。
   2. 一筆房東資格案件：完全比照情境 A 的所有資料表寫入操作。

  核心重點：會員必須兩筆審核都通過，其身分才會最終變更為房東。

### 提交房源刊登申請 (Property Verification)

  此流程由已具備房東資格的會員發起，目的是將一筆新的房源資料上架。

   - `properties` 表 (建立一筆新的房源資料)
       - statusCode: 'PENDING' (表示此房源為「待審核」狀態)
       - landlordMemberID: 提交申請的房東會員 ID
       - propertyProofURL: 儲存房源證明文件的 URL
       - publishedAt: 保持 NULL (等待審核通過並付費後才會設定)
       - 其他房源相關欄位 (如 title, addressLine 等) 根據房東填寫的資料寫入。

   - `approvals` 表 (建立一筆新的審核案件)
       - moduleCode: 'PROPERTY' (明確標示為房源審核)
       - applicantMemberID: 提交申請的房東會員 ID
       - sourcePropertyID: 關聯到上述 `properties` 表中新建房源的 ID (這是與會員審核最大的不同點)
       - statusCode: 'PENDING'

   - `approvalItems` 表 (建立一筆初始操作紀錄)
       - approvalID: 關聯到上述 approvals 表中新建案件的 ID
       - actionType: 'SUBMIT'
       - actionBy: NULL

   - `userUploads` 表 (建立一筆檔案上傳紀錄)
       - approvalID: 關聯到上述 approvals 表中新建案件的 ID
       - moduleCode: 'PropertyInfo'
       - uploadTypeCode: 'PROPERTY_PROOF'
       - filePath: 儲存房源證明文件的 URL 或路徑

### 審核類型對應表

| 審核類型   | approvals.moduleCode | approvals.sourcePropertyID | 主要檔案上傳 userUploads    | 前置條件     |
| ---------- | -------------------- | -------------------------- | --------------------------- | ------------ |
| 身分證驗證 | IDENTITY             | NULL                       | USER_ID_FRONT, USER_ID_BACK | 已註冊會員   |
| 房東申請   | LANDLORD             | NULL                       | 無                          | 已驗證身分證 |
| 房源審核   | PROPERTY             | propertyID                 | PROPERTY_PROOF              | 已驗證房東   |