/**
 * 圖片拖曳排序管理器 - 處理拖曳排序和順序管理功能
 * 實作需求 5, 6, 9, 10
 */
class ImageSortManager {
    constructor(container) {
        this.container = container;
        this.sortableContainers = container.querySelectorAll('.sortable-container');
        this.enableDragSort = container.dataset.enableDragSort === 'true';
        
        // 拖曳狀態
        this.draggedItem = null;
        this.draggedOverItem = null;
        this.dragStartPosition = null;
        this.isDragging = false;
        
        // 排序管理
        this.orderMapping = new Map(); // 存儲項目與順序的對應關係
        
        if (this.enableDragSort) {
            this.init();
        }
    }
    
    init() {
        this.setupEventListeners();
        this.initializeSortableContainers();
        this.updateOrderMapping();
        console.log('圖片拖曳排序管理器初始化完成');
    }
    
    setupEventListeners() {
        // 為每個可排序容器設定事件監聽
        this.sortableContainers.forEach(container => {
            this.setupContainerEvents(container);
        });
        
        // 監聽檔案變更事件，重新初始化排序
        this.container.addEventListener('filesChanged', this.handleFilesChanged.bind(this));
        
        // 監聽檔案刪除事件，重新排序
        this.container.addEventListener('deleteSelectedFiles', this.handleFilesDeleted.bind(this));
        this.container.addEventListener('clearAllFiles', this.handleFilesCleared.bind(this));
        
        // 全域拖曳事件（處理在容器外的拖曳）
        document.addEventListener('dragover', this.handleGlobalDragOver.bind(this));
        document.addEventListener('drop', this.handleGlobalDrop.bind(this));
    }
    
    setupContainerEvents(container) {
        // 拖曳開始
        container.addEventListener('dragstart', this.handleDragStart.bind(this));
        
        // 拖曳進入
        container.addEventListener('dragenter', this.handleDragEnter.bind(this));
        
        // 拖曳經過
        container.addEventListener('dragover', this.handleDragOver.bind(this));
        
        // 拖曳離開
        container.addEventListener('dragleave', this.handleDragLeave.bind(this));
        
        // 拖曳放下
        container.addEventListener('drop', this.handleDrop.bind(this));
        
        // 拖曳結束
        container.addEventListener('dragend', this.handleDragEnd.bind(this));
        
        // 觸控支援
        container.addEventListener('touchstart', this.handleTouchStart.bind(this), { passive: false });
        container.addEventListener('touchmove', this.handleTouchMove.bind(this), { passive: false });
        container.addEventListener('touchend', this.handleTouchEnd.bind(this));
        
        // 鍵盤支援
        container.addEventListener('keydown', this.handleKeyboardSort.bind(this));
    }
    
    initializeSortableContainers() {
        this.sortableContainers.forEach(container => {
            container.setAttribute('data-sortable', 'true');
            
            // 為每個圖片項目設定拖曳屬性
            const imageItems = container.querySelectorAll('.image-item');
            imageItems.forEach(item => {
                this.makeItemDraggable(item);
            });
        });
    }
    
    makeItemDraggable(item) {
        item.setAttribute('draggable', 'true');
        item.setAttribute('tabindex', '0');
        
        // 添加拖曳手柄事件
        const dragHandle = item.querySelector('.drag-handle');
        if (dragHandle) {
            dragHandle.addEventListener('mousedown', (e) => {
                item.setAttribute('draggable', 'true');
            });
            
            dragHandle.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.initKeyboardSort(item);
                }
            });
        }
    }
    
    handleDragStart(e) {
        const item = e.target.closest('.image-item');
        if (!item) return;
        
        this.draggedItem = item;
        this.isDragging = true;
        this.dragStartPosition = this.getItemOrder(item);
        
        // 設定拖曳資料
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', item.outerHTML);
        
        // 視覺效果
        setTimeout(() => {
            item.classList.add('dragging');
            this.addSortingClass();
        }, 0);
        
        // 建立拖曳提示
        this.createDragHint();
    }
    
    handleDragEnter(e) {
        e.preventDefault();
        const item = e.target.closest('.image-item');
        if (item && item !== this.draggedItem) {
            this.setDragOverItem(item);
        }
    }
    
    handleDragOver(e) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        
        const item = e.target.closest('.image-item');
        if (item && item !== this.draggedItem) {
            this.setDragOverItem(item);
        }
    }
    
    handleDragLeave(e) {
        const item = e.target.closest('.image-item');
        if (item) {
            this.clearDragOverItem(item);
        }
    }
    
    handleDrop(e) {
        e.preventDefault();
        
        if (!this.draggedItem || !this.draggedOverItem) {
            return;
        }
        
        const draggedOrder = this.getItemOrder(this.draggedItem);
        const targetOrder = this.getItemOrder(this.draggedOverItem);
        
        if (draggedOrder !== targetOrder) {
            this.reorderItems(this.draggedItem, this.draggedOverItem);
        }
    }
    
    handleDragEnd(e) {
        this.cleanup();
    }
    
    setDragOverItem(item) {
        // 清除之前的拖曳目標
        if (this.draggedOverItem) {
            this.clearDragOverItem(this.draggedOverItem);
        }
        
        this.draggedOverItem = item;
        item.classList.add('drag-over');
        
        // 顯示排序指示器
        this.showSortIndicator(item);
    }
    
    clearDragOverItem(item) {
        item.classList.remove('drag-over');
        this.hideSortIndicator(item);
        
        if (this.draggedOverItem === item) {
            this.draggedOverItem = null;
        }
    }
    
    reorderItems(draggedItem, targetItem) {
        const draggedOrder = this.getItemOrder(draggedItem);
        const targetOrder = this.getItemOrder(targetItem);
        
        const container = draggedItem.closest('.sortable-container');
        const allItems = Array.from(container.querySelectorAll('.image-item'));
        
        // 計算新的排序
        const newOrder = this.calculateNewOrder(allItems, draggedItem, targetItem);
        
        // 更新 DOM 順序
        this.updateDOMOrder(container, newOrder);
        
        // 更新排序號碼 (需求 6, 10)
        this.updateOrderNumbers(container);
        
        // 觸發重新排序事件
        this.notifyReorder(draggedOrder, targetOrder);
        
        // 顯示成功指示
        this.showReorderSuccess();
    }
    
    calculateNewOrder(allItems, draggedItem, targetItem) {
        const draggedIndex = allItems.indexOf(draggedItem);
        const targetIndex = allItems.indexOf(targetItem);
        
        // 建立新的排序陣列
        const newOrder = [...allItems];
        
        // 移除拖曳項目
        newOrder.splice(draggedIndex, 1);
        
        // 在目標位置插入
        const insertIndex = draggedIndex < targetIndex ? targetIndex : targetIndex;
        newOrder.splice(insertIndex, 0, draggedItem);
        
        return newOrder;
    }
    
    updateDOMOrder(container, newOrder) {
        // 重新排列 DOM 元素
        newOrder.forEach((item, index) => {
            container.appendChild(item);
            item.classList.add('reordering');
            
            // 移除動畫類別
            setTimeout(() => {
                item.classList.remove('reordering');
            }, 500);
        });
    }
    
    updateOrderNumbers(container) {
        const items = container.querySelectorAll('.image-item');
        
        items.forEach((item, index) => {
            const newOrder = index + 1;
            const orderBadge = item.querySelector('.display-order-badge');
            const orderSpan = orderBadge ? orderBadge.querySelector('.order-number') : null;
            
            if (orderSpan) {
                // 添加更新動畫
                orderBadge.classList.add('updating');
                orderSpan.textContent = newOrder;
                
                // 更新 dataset
                item.dataset.displayOrder = newOrder;
                
                // 移除動畫類別
                setTimeout(() => {
                    orderBadge.classList.remove('updating');
                }, 600);
            }
            
            // 更新內部順序對映
            const imageId = item.dataset.imageId;
            if (imageId) {
                this.orderMapping.set(imageId, newOrder);
            }
        });
        
        // 檢查排序完整性 (需求 10)
        this.validateOrderIntegrity(container);
    }
    
    validateOrderIntegrity(container) {
        const items = container.querySelectorAll('.image-item');
        const orders = Array.from(items).map(item => 
            parseInt(item.dataset.displayOrder) || 0
        );
        
        // 檢查是否連續且不重複
        const expectedOrders = Array.from({ length: orders.length }, (_, i) => i + 1);
        const sortedOrders = [...orders].sort((a, b) => a - b);
        
        const isValid = JSON.stringify(sortedOrders) === JSON.stringify(expectedOrders);
        
        if (!isValid) {
            console.warn('排序號碼不連續，正在修復...');
            this.fixOrderIntegrity(container);
        }
    }
    
    fixOrderIntegrity(container) {
        // 修復排序完整性 (需求 9, 10)
        const items = Array.from(container.querySelectorAll('.image-item'));
        
        items.forEach((item, index) => {
            const correctOrder = index + 1;
            const orderBadge = item.querySelector('.display-order-badge');
            const orderSpan = orderBadge ? orderBadge.querySelector('.order-number') : null;
            
            if (orderSpan) {
                orderSpan.textContent = correctOrder;
                item.dataset.displayOrder = correctOrder;
            }
            
            const imageId = item.dataset.imageId;
            if (imageId) {
                this.orderMapping.set(imageId, correctOrder);
            }
        });
    }
    
    handleFilesChanged(event) {
        // 檔案變更後重新初始化
        setTimeout(() => {
            this.initializeSortableContainers();
            this.updateOrderMapping();
        }, 100);
    }
    
    handleFilesDeleted(event) {
        // 檔案刪除後重新排序 (需求 9)
        const { fileIds } = event.detail;
        
        fileIds.forEach(fileId => {
            this.orderMapping.delete(fileId);
        });
        
        // 重新計算所有容器的排序
        this.sortableContainers.forEach(container => {
            this.updateOrderNumbers(container);
        });
    }
    
    handleFilesCleared(event) {
        // 清空後重設排序
        this.orderMapping.clear();
    }
    
    // 觸控支援
    handleTouchStart(e) {
        const item = e.target.closest('.image-item');
        if (!item) return;
        
        this.touchStartItem = item;
        this.touchStartTime = Date.now();
        
        // 長按檢測
        this.longPressTimer = setTimeout(() => {
            this.startTouchDrag(item, e.touches[0]);
        }, 500);
    }
    
    handleTouchMove(e) {
        if (this.longPressTimer) {
            clearTimeout(this.longPressTimer);
            this.longPressTimer = null;
        }
        
        if (this.isDragging) {
            e.preventDefault();
            this.updateTouchDrag(e.touches[0]);
        }
    }
    
    handleTouchEnd(e) {
        if (this.longPressTimer) {
            clearTimeout(this.longPressTimer);
        }
        
        if (this.isDragging) {
            this.endTouchDrag(e.changedTouches[0]);
        }
    }
    
    // 鍵盤排序支援
    handleKeyboardSort(e) {
        const item = e.target.closest('.image-item');
        if (!item) return;
        
        const currentOrder = this.getItemOrder(item);
        const container = item.closest('.sortable-container');
        const totalItems = container.querySelectorAll('.image-item').length;
        
        switch (e.key) {
            case 'ArrowUp':
            case 'ArrowLeft':
                e.preventDefault();
                if (currentOrder > 1) {
                    this.moveItemToOrder(item, currentOrder - 1);
                }
                break;
                
            case 'ArrowDown':
            case 'ArrowRight':
                e.preventDefault();
                if (currentOrder < totalItems) {
                    this.moveItemToOrder(item, currentOrder + 1);
                }
                break;
                
            case 'Home':
                e.preventDefault();
                if (currentOrder > 1) {
                    this.moveItemToOrder(item, 1);
                }
                break;
                
            case 'End':
                e.preventDefault();
                if (currentOrder < totalItems) {
                    this.moveItemToOrder(item, totalItems);
                }
                break;
        }
    }
    
    moveItemToOrder(item, targetOrder) {
        const container = item.closest('.sortable-container');
        const items = Array.from(container.querySelectorAll('.image-item'));
        const currentOrder = this.getItemOrder(item);
        
        if (targetOrder === currentOrder) return;
        
        // 計算目標項目
        const targetItem = items.find(i => this.getItemOrder(i) === targetOrder);
        
        if (targetItem) {
            this.reorderItems(item, targetItem);
        }
    }
    
    // 輔助方法
    getItemOrder(item) {
        return parseInt(item.dataset.displayOrder) || 0;
    }
    
    updateOrderMapping() {
        this.orderMapping.clear();
        
        this.sortableContainers.forEach(container => {
            const items = container.querySelectorAll('.image-item');
            items.forEach(item => {
                const imageId = item.dataset.imageId;
                const order = this.getItemOrder(item);
                if (imageId && order) {
                    this.orderMapping.set(imageId, order);
                }
            });
        });
    }
    
    addSortingClass() {
        this.sortableContainers.forEach(container => {
            container.classList.add('sorting', 'drag-active');
        });
    }
    
    removeSortingClass() {
        this.sortableContainers.forEach(container => {
            container.classList.remove('sorting', 'drag-active');
        });
    }
    
    createDragHint() {
        if (this.container.querySelector('.sort-hint')) return;
        
        const hint = document.createElement('div');
        hint.className = 'sort-hint';
        hint.textContent = '拖曳到目標位置調整順序';
        
        this.sortableContainers.forEach(container => {
            container.style.position = 'relative';
            container.appendChild(hint.cloneNode(true));
        });
    }
    
    removeDragHint() {
        const hints = this.container.querySelectorAll('.sort-hint');
        hints.forEach(hint => hint.remove());
    }
    
    showSortIndicator(item) {
        const indicator = document.createElement('div');
        indicator.className = 'sort-indicator';
        indicator.textContent = '放置於此';
        item.appendChild(indicator);
    }
    
    hideSortIndicator(item) {
        const indicator = item.querySelector('.sort-indicator');
        if (indicator) {
            indicator.remove();
        }
    }
    
    showReorderSuccess() {
        // 顯示重新排序成功提示
        const indicator = document.createElement('div');
        indicator.className = 'sort-complete-indicator';
        indicator.innerHTML = '<i class="fas fa-check me-2"></i>排序已更新';
        
        document.body.appendChild(indicator);
        
        setTimeout(() => {
            indicator.classList.add('show');
        }, 100);
        
        setTimeout(() => {
            indicator.classList.remove('show');
            setTimeout(() => {
                indicator.remove();
            }, 300);
        }, 2000);
    }
    
    cleanup() {
        // 清理拖曳狀態
        if (this.draggedItem) {
            this.draggedItem.classList.remove('dragging');
        }
        
        if (this.draggedOverItem) {
            this.clearDragOverItem(this.draggedOverItem);
        }
        
        this.removeSortingClass();
        this.removeDragHint();
        
        this.draggedItem = null;
        this.draggedOverItem = null;
        this.isDragging = false;
        this.dragStartPosition = null;
    }
    
    handleGlobalDragOver(e) {
        if (this.isDragging) {
            e.preventDefault();
        }
    }
    
    handleGlobalDrop(e) {
        if (this.isDragging) {
            e.preventDefault();
        }
    }
    
    notifyReorder(fromOrder, toOrder) {
        const event = new CustomEvent('itemReordered', {
            detail: {
                fromOrder,
                toOrder,
                orderMapping: new Map(this.orderMapping)
            }
        });
        this.container.dispatchEvent(event);
    }
    
    // 公共 API
    getOrderMapping() {
        return new Map(this.orderMapping);
    }
    
    setItemOrder(imageId, newOrder) {
        const item = this.container.querySelector(`[data-image-id="${imageId}"]`);
        if (item) {
            this.moveItemToOrder(item, newOrder);
        }
    }
    
    reorderAllItems() {
        this.sortableContainers.forEach(container => {
            this.updateOrderNumbers(container);
        });
    }
    
    enable() {
        this.enableDragSort = true;
        this.initializeSortableContainers();
    }
    
    disable() {
        this.enableDragSort = false;
        
        this.sortableContainers.forEach(container => {
            const items = container.querySelectorAll('.image-item');
            items.forEach(item => {
                item.setAttribute('draggable', 'false');
            });
        });
    }
}

// 導出供其他模組使用
window.ImageSortManager = ImageSortManager;