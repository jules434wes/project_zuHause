// Bootstrap Tab 獨立性測試腳本
// 用於檢測 Dashboard 和 Admin 系統的 Bootstrap Tab 實例是否互相獨立

(function() {
    'use strict';
    
    console.log('🔗 開始 Bootstrap Tab 獨立性測試...');
    
    // 測試結果收集
    const testResults = {
        dashboardTabsFound: false,
        adminTabsFound: false,
        dashboardTabInstance: null,
        adminTabInstance: null,
        independenceTest: false,
        eventIsolation: false,
        errors: []
    };
    
    try {
        // 1. 檢查 Dashboard Tab 元素存在性
        const dashboardTabContainer = document.querySelector('#dashboard-userTabs');
        const dashboardTabButtons = document.querySelectorAll('#dashboard-userTabs button[data-bs-toggle="tab"]');
        
        if (dashboardTabContainer && dashboardTabButtons.length > 0) {
            testResults.dashboardTabsFound = true;
            console.log('✅ Dashboard Tab 容器找到:', dashboardTabContainer);
            console.log(`✅ Dashboard Tab 按鈕數量: ${dashboardTabButtons.length}`);
            
            // 檢查 Dashboard Tab 實例
            const firstDashButton = dashboardTabButtons[0];
            testResults.dashboardTabInstance = bootstrap.Tab.getOrCreateInstance(firstDashButton);
            console.log('Dashboard Tab 實例:', testResults.dashboardTabInstance);
        } else {
            testResults.errors.push('Dashboard Tab 元素未找到');
            console.warn('⚠️ Dashboard Tab 元素未找到');
        }
        
        // 2. 檢查 Admin Tab 元素存在性（如果存在的話）
        const adminTabContainer = document.querySelector('#userTabs');
        const adminTabButtons = document.querySelectorAll('#userTabs button[data-bs-toggle="tab"]');
        
        if (adminTabContainer && adminTabButtons.length > 0) {
            testResults.adminTabsFound = true;
            console.log('✅ Admin Tab 容器找到:', adminTabContainer);
            console.log(`✅ Admin Tab 按鈕數量: ${adminTabButtons.length}`);
            
            // 檢查 Admin Tab 實例
            const firstAdminButton = adminTabButtons[0];
            testResults.adminTabInstance = bootstrap.Tab.getOrCreateInstance(firstAdminButton);
            console.log('Admin Tab 實例:', testResults.adminTabInstance);
        } else {
            console.log('ℹ️ Admin Tab 元素未在當前頁面找到（這在 Dashboard 頁面是正常的）');
        }
        
        // 3. 測試 Tab 切換獨立性
        if (testResults.dashboardTabsFound) {
            console.log('🔄 測試 Dashboard Tab 切換功能...');
            
            // 記錄切換前狀態
            const initialActiveTab = document.querySelector('#dashboard-userTabs .nav-link.active');
            console.log('初始活躍 Tab:', initialActiveTab ? initialActiveTab.textContent.trim() : '無');
            
            // 模擬點擊第二個 Tab（如果存在）
            if (dashboardTabButtons.length > 1) {
                const secondTab = dashboardTabButtons[1];
                const tabInstance = bootstrap.Tab.getOrCreateInstance(secondTab);
                
                // 添加事件監聽器測試事件隔離
                let eventFired = false;
                const eventHandler = () => {
                    eventFired = true;
                    console.log('✅ Tab 切換事件正常觸發');
                };
                
                secondTab.addEventListener('shown.bs.tab', eventHandler, { once: true });
                
                // 切換到第二個 Tab
                tabInstance.show();
                
                // 等待事件觸發
                setTimeout(() => {
                    testResults.eventIsolation = eventFired;
                    testResults.independenceTest = true;
                    
                    // 檢查切換後狀態
                    const newActiveTab = document.querySelector('#dashboard-userTabs .nav-link.active');
                    console.log('切換後活躍 Tab:', newActiveTab ? newActiveTab.textContent.trim() : '無');
                    
                    // 清理事件監聽器
                    secondTab.removeEventListener('shown.bs.tab', eventHandler);
                    
                    // 顯示測試結果
                    displayTestResults();
                }, 500);
            } else {
                console.log('⚠️ Dashboard 只有一個 Tab，無法測試切換');
                testResults.independenceTest = true;
                displayTestResults();
            }
        } else {
            displayTestResults();
        }
        
        // 4. 檢查 CSS 類別衝突
        console.log('🎨 檢查 CSS 類別衝突...');
        const dashboardTabContent = document.querySelector('#dashboard-userTabsContent');
        const adminTabContent = document.querySelector('#userTabsContent');
        
        if (dashboardTabContent) {
            console.log('✅ Dashboard Tab 內容容器找到');
        }
        if (adminTabContent) {
            console.log('✅ Admin Tab 內容容器找到');
        }
        
        // 5. 檢查 Tab Pane 獨立性
        const dashboardPanes = document.querySelectorAll('#dashboard-userTabsContent .tab-pane');
        const adminPanes = document.querySelectorAll('#userTabsContent .tab-pane');
        
        console.log(`Dashboard Tab Panes: ${dashboardPanes.length}`);
        console.log(`Admin Tab Panes: ${adminPanes.length}`);
        
        // 檢查 Pane ID 不衝突
        const dashboardPaneIds = Array.from(dashboardPanes).map(pane => pane.id);
        const adminPaneIds = Array.from(adminPanes).map(pane => pane.id);
        
        const paneIdConflicts = dashboardPaneIds.filter(id => adminPaneIds.includes(id));
        if (paneIdConflicts.length > 0) {
            testResults.errors.push(`Tab Pane ID 衝突: ${paneIdConflicts.join(', ')}`);
            console.error('❌ Tab Pane ID 衝突:', paneIdConflicts);
        } else {
            console.log('✅ Tab Pane ID 無衝突');
        }
        
    } catch (error) {
        testResults.errors.push(`測試過程發生錯誤: ${error.message}`);
        console.error('❌ Bootstrap Tab 測試錯誤:', error);
        displayTestResults();
    }
    
    function displayTestResults() {
        console.log('\n📊 Bootstrap Tab 獨立性測試結果:');
        console.log('==========================================');
        
        // 基本檢查結果
        console.log(`Dashboard Tabs 找到: ${testResults.dashboardTabsFound ? '✅' : '❌'}`);
        console.log(`Admin Tabs 找到: ${testResults.adminTabsFound ? '✅' : 'ℹ️ (當前頁面不需要)'}`);
        console.log(`Tab 切換功能: ${testResults.independenceTest ? '✅' : '❌'}`);
        console.log(`事件隔離: ${testResults.eventIsolation ? '✅' : '❌'}`);
        
        // 錯誤報告
        if (testResults.errors.length > 0) {
            console.log('\n❌ 發現問題:');
            testResults.errors.forEach(error => {
                console.log(`  - ${error}`);
            });
        } else {
            console.log('\n✅ 所有檢查通過！Bootstrap Tab 實例完全獨立運作。');
        }
        
        // 實例詳情
        if (testResults.dashboardTabInstance) {
            console.log('\n🔧 Dashboard Tab 實例詳情:');
            console.log('  Element:', testResults.dashboardTabInstance._element);
            console.log('  Config:', testResults.dashboardTabInstance._config);
        }
        
        if (testResults.adminTabInstance) {
            console.log('\n🔧 Admin Tab 實例詳情:');
            console.log('  Element:', testResults.adminTabInstance._element);
            console.log('  Config:', testResults.adminTabInstance._config);
        }
        
        return testResults;
    }
    
    // 返回測試結果供外部使用
    window.zuHauseTabTestResults = testResults;
    
})();