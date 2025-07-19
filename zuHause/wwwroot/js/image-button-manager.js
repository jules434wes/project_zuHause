/**
 * 圖片按鈕狀態管理器 - 處理動態按鈕狀態、操作邏輯和鍵盤導航
 * 實作需求 7 和鍵盤導航強化
 */
class ImageButtonManager {
    constructor(container, previewManager, modalManager) {
        this.container = container;
        this.previewManager = previewManager;
        this.modalManager = modalManager;
        
        // 按鈕元素
        this.addMoreBtn = container.querySelector('.btn-select-files');
        this.confirmUploadBtn = container.querySelector('.btn-upload-confirm');
        this.selectAllBtn = container.querySelector('.btn-select-all');
        this.deleteAllBtn = container.querySelector('.btn-clear-all');
        this.deleteSelectedBtn = container.querySelector('.btn-delete-selected');
        
        // 計數元素
        this.uploadCountSpan = container.querySelector('.upload-count');
        this.selectedCountSpan = container.querySelector('.selected-count');
        
        // 狀態追蹤
        this.fileCount = 0;
        this.selectedCount = 0;
        this.isSelectAllMode = false;
        this.keyboardShortcutsEnabled = container.dataset.keyboardShortcuts === 'true';
        
        // 鍵盤快捷鍵映射
        this.shortcuts = new Map([
            ['ctrl+o', () => this.triggerAddFiles()],
            ['ctrl+enter', () => this.triggerConfirmUpload()],
            ['ctrl+a', () => this.triggerSelectAll()],
            ['delete', () => this.triggerDeleteSelected()],
            ['ctrl+shift+delete', () => this.triggerDeleteAll()],
            ['?', () => this.toggleKeyboardHelp()]
        ]);
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        this.updateButtonStates();
        console.log('圖片按鈕管理器初始化完成');
    }
    
    setupEventListeners() {
        // 按鈕點擊事件
        if (this.addMoreBtn) {
            this.addMoreBtn.addEventListener('click', this.handleAddFiles.bind(this));
        }
        
        if (this.confirmUploadBtn) {
            this.confirmUploadBtn.addEventListener('click', this.handleConfirmUpload.bind(this));
        }
        
        if (this.selectAllBtn) {
            this.selectAllBtn.addEventListener('click', this.handleSelectAll.bind(this));
        }
        
        if (this.deleteAllBtn) {
            this.deleteAllBtn.addEventListener('click', this.handleDeleteAll.bind(this));
        }
        
        if (this.deleteSelectedBtn) {
            this.deleteSelectedBtn.addEventListener('click', this.handleDeleteSelected.bind(this));
        }
        
        // 監聽檔案變更事件
        this.container.addEventListener('filesChanged', this.handleFilesChanged.bind(this));
        
        // 監聽選擇變更事件
        this.container.addEventListener('selectionChanged', this.handleSelectionChanged.bind(this));
        
        // 鍵盤事件監聽
        if (this.keyboardShortcutsEnabled) {
            document.addEventListener('keydown', this.handleKeyboardShortcuts.bind(this));
        }
        
        // 無障礙功能：焦點管理
        this.container.addEventListener('focusin', this.handleFocusManagement.bind(this));
    }
    
    handleFilesChanged(event) {
        const { count } = event.detail;
        this.fileCount = count;
        this.updateButtonStates();
        this.updateCountDisplays();
    }
    
    handleSelectionChanged(event) {
        const { count } = event.detail;
        this.selectedCount = count;
        this.updateButtonStates();
        this.updateCountDisplays();
    }
    
    updateButtonStates() {
        // 需求 7: 確認上傳按鈕 - 圖片數 == 0 時不可點選
        if (this.confirmUploadBtn) {
            this.confirmUploadBtn.disabled = this.fileCount === 0;
            this.updateButtonAriaLabel(this.confirmUploadBtn, 
                this.fileCount === 0 ? '無圖片可上傳' : `準備上傳 ${this.fileCount} 張圖片`);
        }
        
        // 需求 7: 全部刪除按鈕 - 圖片數 == 0 時不可點選
        if (this.deleteAllBtn) {
            this.deleteAllBtn.disabled = this.fileCount === 0;
            this.updateButtonAriaLabel(this.deleteAllBtn, 
                this.fileCount === 0 ? '無圖片可刪除' : `刪除全部 ${this.fileCount} 張圖片`);
        }
        
        // 需求 7: 刪除選中按鈕 - 有勾選圖片時才會出現
        if (this.deleteSelectedBtn) {
            if (this.selectedCount > 0) {
                this.deleteSelectedBtn.style.display = 'flex';
                this.deleteSelectedBtn.disabled = false;
                this.updateButtonAriaLabel(this.deleteSelectedBtn, `刪除選中的 ${this.selectedCount} 張圖片`);
            } else {
                this.deleteSelectedBtn.style.display = 'none';
            }
        }
        
        // 全選按鈕狀態管理
        if (this.selectAllBtn) {
            if (this.fileCount > 0) {
                this.selectAllBtn.style.display = 'flex';
                this.updateSelectAllButtonText();
            } else {
                this.selectAllBtn.style.display = 'none';
            }
        }
        
        // 新增檔案按鈕始終可用
        if (this.addMoreBtn) {
            this.updateButtonAriaLabel(this.addMoreBtn, '選擇更多圖片檔案');
        }
    }
    
    updateSelectAllButtonText() {
        if (!this.selectAllBtn) return;
        
        const btnText = this.selectAllBtn.querySelector('.btn-text');
        const icon = this.selectAllBtn.querySelector('i');
        
        if (this.selectedCount === this.fileCount && this.fileCount > 0) {
            // 全選模式 -> 改為取消全選
            if (btnText) btnText.textContent = '取消全選';
            if (icon) {
                icon.className = 'fas fa-square';
            }
            this.isSelectAllMode = true;
            this.updateButtonAriaLabel(this.selectAllBtn, '取消選擇所有圖片');
        } else {
            // 部分選或無選 -> 改為全選
            if (btnText) btnText.textContent = '全選';
            if (icon) {
                icon.className = 'fas fa-check-square';
            }
            this.isSelectAllMode = false;
            this.updateButtonAriaLabel(this.selectAllBtn, '選擇所有圖片');
        }
    }
    
    updateCountDisplays() {
        if (this.uploadCountSpan) {
            this.uploadCountSpan.textContent = this.fileCount;
        }
        
        if (this.selectedCountSpan) {
            this.selectedCountSpan.textContent = this.selectedCount;
        }
    }
    
    updateButtonAriaLabel(button, label) {
        if (button) {
            button.setAttribute('aria-label', label);
        }
    }
    
    // 按鈕點擊處理器
    handleAddFiles() {
        this.triggerAddFiles();
    }
    
    async handleConfirmUpload() {
        if (this.fileCount === 0) return;
        
        const confirmed = await this.showUploadConfirmation();
        if (confirmed) {
            this.triggerConfirmUpload();
        }
    }
    
    handleSelectAll() {
        this.triggerSelectAll();
    }
    
    async handleDeleteAll() {
        if (this.fileCount === 0) return;
        
        const confirmed = await this.showDeleteConfirmation(this.fileCount, true);
        if (confirmed) {
            this.triggerDeleteAll();
        }
    }
    
    async handleDeleteSelected() {
        if (this.selectedCount === 0) return;
        
        const confirmed = await this.showDeleteConfirmation(this.selectedCount, false);
        if (confirmed) {
            this.triggerDeleteSelected();
        }
    }
    
    // 操作觸發器 (與現有管理器整合)
    triggerAddFiles() {
        const fileInput = this.container.querySelector('.file-input');
        if (fileInput) {
            fileInput.click();
            this.announceAction('開啟檔案選擇對話框');
        }
    }
    
    triggerConfirmUpload() {
        if (this.fileCount === 0) {
            this.announceAction('沒有圖片可以上傳');
            return;
        }
        
        const uploadEvent = new CustomEvent('confirmUpload', {
            detail: { fileCount: this.fileCount }
        });
        this.container.dispatchEvent(uploadEvent);
        this.announceAction(`開始上傳 ${this.fileCount} 張圖片`);
    }
    
    triggerSelectAll() {
        const selectEvent = new CustomEvent(this.isSelectAllMode ? 'clearSelection' : 'selectAll');
        this.container.dispatchEvent(selectEvent);
        
        const action = this.isSelectAllMode ? '取消選擇所有圖片' : '選擇所有圖片';
        this.announceAction(action);
    }
    
    triggerDeleteSelected() {
        if (this.selectedCount === 0) return;
        
        const deleteEvent = new CustomEvent('deleteSelectedFiles', {
            detail: { count: this.selectedCount }
        });
        this.container.dispatchEvent(deleteEvent);
        this.announceAction(`刪除 ${this.selectedCount} 張選中圖片`);
    }
    
    triggerDeleteAll() {
        if (this.fileCount === 0) return;
        
        const clearEvent = new CustomEvent('clearAllFiles', {
            detail: { count: this.fileCount }
        });
        this.container.dispatchEvent(clearEvent);
        this.announceAction(`刪除全部 ${this.fileCount} 張圖片`);
    }
    
    // 鍵盤快捷鍵處理
    handleKeyboardShortcuts(event) {
        // 如果焦點在輸入框中，忽略快捷鍵
        if (['INPUT', 'TEXTAREA', 'SELECT'].includes(event.target.tagName)) {
            return;
        }
        
        const key = this.getKeyCombo(event);
        const handler = this.shortcuts.get(key);
        
        if (handler) {
            event.preventDefault();
            handler();
        }
    }
    
    getKeyCombo(event) {
        const parts = [];
        if (event.ctrlKey) parts.push('ctrl');
        if (event.shiftKey) parts.push('shift');
        if (event.altKey) parts.push('alt');
        
        const key = event.key.toLowerCase();
        if (key !== 'control' && key !== 'shift' && key !== 'alt') {
            parts.push(key === ' ' ? 'space' : key);
        }
        
        return parts.join('+');
    }
    
    // 確認對話框
    async showUploadConfirmation() {
        const message = `確定要上傳 ${this.fileCount} 張圖片嗎？`;
        
        if (this.modalManager) {
            return await this.modalManager.showConfirmation(message);
        } else {
            return confirm(message);
        }
    }
    
    async showDeleteConfirmation(count, isAll) {
        const action = isAll ? '刪除全部' : '刪除選中的';
        const message = `確定要${action} ${count} 張圖片嗎？此操作無法復原。`;
        
        if (this.modalManager) {
            return await this.modalManager.showConfirmation(message);
        } else {
            return confirm(message);
        }
    }
    
    // 鍵盤說明切換
    toggleKeyboardHelp() {
        const helpElement = document.getElementById('keyboard-help');
        if (helpElement) {
            helpElement.classList.toggle('d-none');
            
            if (!helpElement.classList.contains('d-none')) {
                // 顯示時設定焦點和 ESC 關閉
                helpElement.setAttribute('tabindex', '-1');
                helpElement.focus();
                
                const closeOnEsc = (e) => {
                    if (e.key === 'Escape') {
                        helpElement.classList.add('d-none');
                        document.removeEventListener('keydown', closeOnEsc);
                    }
                };
                
                document.addEventListener('keydown', closeOnEsc);
            }
        }
    }
    
    // 焦點管理
    handleFocusManagement(event) {
        // 確保按鈕有適當的焦點指示
        if (event.target.classList.contains('btn')) {
            event.target.setAttribute('data-focused', 'true');
            
            const removeFocus = () => {
                event.target.removeAttribute('data-focused');
                event.target.removeEventListener('blur', removeFocus);
            };
            
            event.target.addEventListener('blur', removeFocus);
        }
    }
    
    // 無障礙語音通知
    announceAction(message) {
        // 建立臨時的 aria-live 元素來通知螢幕閱讀器
        const announcement = document.createElement('div');
        announcement.setAttribute('aria-live', 'polite');
        announcement.setAttribute('aria-atomic', 'true');
        announcement.className = 'sr-only';
        announcement.textContent = message;
        
        document.body.appendChild(announcement);
        
        // 短暫延遲後移除
        setTimeout(() => {
            document.body.removeChild(announcement);
        }, 1000);
    }
    
    // 公共 API
    getButtonStates() {
        return {
            fileCount: this.fileCount,
            selectedCount: this.selectedCount,
            canUpload: this.fileCount > 0,
            canDeleteAll: this.fileCount > 0,
            canDeleteSelected: this.selectedCount > 0,
            isSelectAllMode: this.isSelectAllMode
        };
    }
    
    updateFileCount(count) {
        this.fileCount = count;
        this.updateButtonStates();
        this.updateCountDisplays();
    }
    
    updateSelectedCount(count) {
        this.selectedCount = count;
        this.updateButtonStates();
        this.updateCountDisplays();
    }
    
    setKeyboardShortcuts(enabled) {
        this.keyboardShortcutsEnabled = enabled;
        
        if (enabled) {
            document.addEventListener('keydown', this.handleKeyboardShortcuts.bind(this));
        } else {
            document.removeEventListener('keydown', this.handleKeyboardShortcuts.bind(this));
        }
    }
    
    destroy() {
        // 清理事件監聽器
        if (this.keyboardShortcutsEnabled) {
            document.removeEventListener('keydown', this.handleKeyboardShortcuts.bind(this));
        }
        
        // 清理其他監聽器會由容器清理時自動處理
        console.log('圖片按鈕管理器已銷毀');
    }
}

// 導出供其他模組使用
window.ImageButtonManager = ImageButtonManager;