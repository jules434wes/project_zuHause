// zuHause 效能與記憶體洩漏測試腳本
// 用於測試 Dashboard 會員管理模組的效能和記憶體使用

(function() {
    'use strict';
    
    console.log('⚡ 開始 zuHause 效能與記憶體測試...');
    
    // 測試配置
    const testConfig = {
        tabSwitchIterations: 20,        // Tab 切換次數
        searchIterations: 15,           // 搜尋測試次數  
        memoryCheckInterval: 1000,      // 記憶體檢查間隔 (ms)
        performanceThreshold: 100       // 效能閾值 (ms)
    };
    
    // 測試結果
    const testResults = {
        tabSwitchTimes: [],
        searchTimes: [],
        memorySnapshots: [],
        errors: [],
        startTime: performance.now(),
        initialMemory: null,
        peakMemory: null,
        finalMemory: null
    };
    
    // 記憶體監控函數
    function getMemoryUsage() {
        if (performance.memory) {
            return {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit,
                timestamp: performance.now()
            };
        }
        return null;
    }
    
    // 格式化記憶體大小
    function formatMemory(bytes) {
        if (!bytes) return 'N/A';
        const mb = bytes / (1024 * 1024);
        return `${mb.toFixed(2)} MB`;
    }
    
    // Tab 切換效能測試
    async function testTabSwitching() {
        console.log('🔄 開始 Tab 切換效能測試...');
        
        const tabButtons = document.querySelectorAll('#dashboard-userTabs button[data-bs-toggle="tab"]');
        if (tabButtons.length < 2) {
            console.warn('⚠️ Tab 數量不足，跳過 Tab 切換測試');
            return;
        }
        
        for (let i = 0; i < testConfig.tabSwitchIterations; i++) {
            const targetTab = tabButtons[i % tabButtons.length];
            const startTime = performance.now();
            
            try {
                // 觸發 Tab 切換
                const tabInstance = bootstrap.Tab.getOrCreateInstance(targetTab);
                tabInstance.show();
                
                // 等待 Tab 切換完成
                await new Promise(resolve => {
                    const handler = () => {
                        targetTab.removeEventListener('shown.bs.tab', handler);
                        resolve();
                    };
                    targetTab.addEventListener('shown.bs.tab', handler);
                });
                
                const endTime = performance.now();
                const duration = endTime - startTime;
                testResults.tabSwitchTimes.push(duration);
                
                // 記錄記憶體使用
                const memoryUsage = getMemoryUsage();
                if (memoryUsage) {
                    testResults.memorySnapshots.push({
                        ...memoryUsage,
                        action: `tab_switch_${i}`,
                        iteration: i
                    });
                }
                
                console.log(`Tab 切換 ${i + 1}/${testConfig.tabSwitchIterations}: ${duration.toFixed(2)}ms`);
                
                // 短暫延遲避免過快切換
                await new Promise(resolve => setTimeout(resolve, 50));
                
            } catch (error) {
                testResults.errors.push(`Tab 切換錯誤 (第${i+1}次): ${error.message}`);
                console.error(`❌ Tab 切換錯誤 (第${i+1}次):`, error);
            }
        }
    }
    
    // 搜尋功能效能測試
    async function testSearchPerformance() {
        console.log('🔍 開始搜尋功能效能測試...');
        
        const searchInput = document.querySelector('#dashboard-searchInput');
        const searchBtn = document.querySelector('#dashboard-searchBtn');
        
        if (!searchInput || !searchBtn) {
            console.warn('⚠️ 搜尋元素未找到，跳過搜尋效能測試');
            return;
        }
        
        const testKeywords = ['測試', 'test', '123', 'admin', 'user', 'member'];
        
        for (let i = 0; i < testConfig.searchIterations; i++) {
            const keyword = testKeywords[i % testKeywords.length] + i;
            const startTime = performance.now();
            
            try {
                // 設定搜尋關鍵字
                searchInput.value = keyword;
                
                // 觸發搜尋
                searchBtn.click();
                
                // 等待搜尋完成（模擬網路請求時間）
                await new Promise(resolve => setTimeout(resolve, 200));
                
                const endTime = performance.now();
                const duration = endTime - startTime;
                testResults.searchTimes.push(duration);
                
                // 記錄記憶體使用
                const memoryUsage = getMemoryUsage();
                if (memoryUsage) {
                    testResults.memorySnapshots.push({
                        ...memoryUsage,
                        action: `search_${i}`,
                        iteration: i,
                        keyword: keyword
                    });
                }
                
                console.log(`搜尋測試 ${i + 1}/${testConfig.searchIterations}: ${duration.toFixed(2)}ms (關鍵字: ${keyword})`);
                
                // 清空搜尋框
                searchInput.value = '';
                
                // 延遲避免過於頻繁
                await new Promise(resolve => setTimeout(resolve, 100));
                
            } catch (error) {
                testResults.errors.push(`搜尋測試錯誤 (第${i+1}次): ${error.message}`);
                console.error(`❌ 搜尋測試錯誤 (第${i+1}次):`, error);
            }
        }
    }
    
    // 記憶體洩漏檢測
    function detectMemoryLeaks() {
        console.log('🧠 分析記憶體洩漏...');
        
        if (testResults.memorySnapshots.length < 2) {
            console.warn('⚠️ 記憶體快照不足，無法分析洩漏');
            return;
        }
        
        const initialMemory = testResults.memorySnapshots[0];
        const finalMemory = testResults.memorySnapshots[testResults.memorySnapshots.length - 1];
        
        testResults.initialMemory = initialMemory;
        testResults.finalMemory = finalMemory;
        
        // 找出記憶體使用峰值
        testResults.peakMemory = testResults.memorySnapshots.reduce((peak, current) => {
            return current.used > peak.used ? current : peak;
        });
        
        const memoryIncrease = finalMemory.used - initialMemory.used;
        const memoryIncreasePercent = ((memoryIncrease / initialMemory.used) * 100);
        
        console.log(`記憶體變化: ${formatMemory(memoryIncrease)} (${memoryIncreasePercent.toFixed(2)}%)`);
        console.log(`初始記憶體: ${formatMemory(initialMemory.used)}`);
        console.log(`峰值記憶體: ${formatMemory(testResults.peakMemory.used)}`);
        console.log(`最終記憶體: ${formatMemory(finalMemory.used)}`);
        
        // 記憶體洩漏判斷
        const leakThreshold = 5; // 5MB
        const leakPercentThreshold = 20; // 20%
        
        if (memoryIncrease > leakThreshold * 1024 * 1024 || memoryIncreasePercent > leakPercentThreshold) {
            testResults.errors.push(`疑似記憶體洩漏: 增加 ${formatMemory(memoryIncrease)} (${memoryIncreasePercent.toFixed(2)}%)`);
            console.warn(`⚠️ 疑似記憶體洩漏: 增加 ${formatMemory(memoryIncrease)}`);
        } else {
            console.log('✅ 記憶體使用正常，無明顯洩漏');
        }
    }
    
    // 效能統計分析
    function analyzePerformance() {
        console.log('📊 分析效能統計...');
        
        function getStats(times) {
            if (times.length === 0) return null;
            
            const sorted = times.slice().sort((a, b) => a - b);
            return {
                min: Math.min(...times),
                max: Math.max(...times),
                avg: times.reduce((sum, time) => sum + time, 0) / times.length,
                median: sorted[Math.floor(sorted.length / 2)],
                p95: sorted[Math.floor(sorted.length * 0.95)],
                count: times.length
            };
        }
        
        // Tab 切換效能統計
        if (testResults.tabSwitchTimes.length > 0) {
            const tabStats = getStats(testResults.tabSwitchTimes);
            console.log('🔄 Tab 切換效能統計:');
            console.log(`  平均: ${tabStats.avg.toFixed(2)}ms`);
            console.log(`  中位數: ${tabStats.median.toFixed(2)}ms`);
            console.log(`  最小: ${tabStats.min.toFixed(2)}ms`);
            console.log(`  最大: ${tabStats.max.toFixed(2)}ms`);
            console.log(`  95%: ${tabStats.p95.toFixed(2)}ms`);
            
            if (tabStats.avg > testConfig.performanceThreshold) {
                testResults.errors.push(`Tab 切換平均時間過長: ${tabStats.avg.toFixed(2)}ms (閾值: ${testConfig.performanceThreshold}ms)`);
            }
        }
        
        // 搜尋效能統計
        if (testResults.searchTimes.length > 0) {
            const searchStats = getStats(testResults.searchTimes);
            console.log('🔍 搜尋效能統計:');
            console.log(`  平均: ${searchStats.avg.toFixed(2)}ms`);
            console.log(`  中位數: ${searchStats.median.toFixed(2)}ms`);
            console.log(`  最小: ${searchStats.min.toFixed(2)}ms`);
            console.log(`  最大: ${searchStats.max.toFixed(2)}ms`);
            console.log(`  95%: ${searchStats.p95.toFixed(2)}ms`);
            
            if (searchStats.avg > testConfig.performanceThreshold * 2) {
                testResults.errors.push(`搜尋平均時間過長: ${searchStats.avg.toFixed(2)}ms (閾值: ${testConfig.performanceThreshold * 2}ms)`);
            }
        }
    }
    
    // 顯示最終測試結果
    function displayFinalResults() {
        const totalTime = performance.now() - testResults.startTime;
        
        console.log('\n📋 zuHause 效能與記憶體測試總結');
        console.log('==========================================');
        console.log(`測試總時間: ${(totalTime / 1000).toFixed(2)} 秒`);
        console.log(`Tab 切換測試: ${testResults.tabSwitchTimes.length} 次`);
        console.log(`搜尋測試: ${testResults.searchTimes.length} 次`);
        console.log(`記憶體快照: ${testResults.memorySnapshots.length} 個`);
        
        if (testResults.errors.length > 0) {
            console.log('\n❌ 發現問題:');
            testResults.errors.forEach((error, index) => {
                console.log(`  ${index + 1}. ${error}`);
            });
        } else {
            console.log('\n✅ 所有效能測試通過！');
        }
        
        console.log('\n📈 建議:');
        console.log('  - 定期執行此測試以監控效能回歸');
        console.log('  - 在不同瀏覽器中重複測試');
        console.log('  - 監控生產環境的真實使用者效能');
        
        return testResults;
    }
    
    // 執行主要測試流程
    async function runTests() {
        try {
            // 記錄初始記憶體
            const initialMemory = getMemoryUsage();
            if (initialMemory) {
                testResults.memorySnapshots.push({
                    ...initialMemory,
                    action: 'test_start',
                    iteration: -1
                });
            }
            
            // 執行 Tab 切換測試
            await testTabSwitching();
            
            // 短暫休息
            await new Promise(resolve => setTimeout(resolve, 500));
            
            // 執行搜尋效能測試
            await testSearchPerformance();
            
            // 最終記憶體檢查
            const finalMemory = getMemoryUsage();
            if (finalMemory) {
                testResults.memorySnapshots.push({
                    ...finalMemory,
                    action: 'test_end',
                    iteration: -1
                });
            }
            
            // 分析結果
            detectMemoryLeaks();
            analyzePerformance();
            displayFinalResults();
            
        } catch (error) {
            console.error('❌ 測試執行錯誤:', error);
            testResults.errors.push(`測試執行錯誤: ${error.message}`);
        }
    }
    
    // 開始測試
    runTests();
    
    // 將結果暴露給外部
    window.zuHausePerformanceTestResults = testResults;
    
})();