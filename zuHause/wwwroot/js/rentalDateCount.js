function initializeRentalDateCount() {
    const startDateInput = document.getElementById("RentalStartDate");
    const endDateInput = document.getElementById("RentalEndDate");
    const displayText = document.getElementById("rentalDurationText");

    if (!startDateInput || !endDateInput || !displayText) return;

    // 移除之前的事件監聽器（避免重複綁定）
    startDateInput.removeEventListener("change", updateDuration);
    endDateInput.removeEventListener("change", updateDuration);

    function updateDuration() {
        const startValue = startDateInput.value;
        const endValue = endDateInput.value;

        if (!startValue || !endValue) {
            displayText.textContent = "請選擇租期";
            return;
        }

        const start = new Date(startValue);
        const end = new Date(endValue);

        if (end < start) {
            displayText.textContent = "結束日不可早於開始日";
            return;
        }

        const diff = calculateDateDifference(start, end);

        if (diff.years === 0 && diff.months === 0 && diff.days === 0) {
            displayText.textContent = `${startValue} 至 ${endValue}，共 1 天`;
            return;
        }

        displayText.textContent = `${startValue} 至 ${endValue}，共 ${diff.years} 年 ${diff.months} 月 ${diff.days} 日`;
    }

    startDateInput.addEventListener("change", updateDuration);
    endDateInput.addEventListener("change", updateDuration);

    function calculateDateDifference(start, end) {
        let years = end.getFullYear() - start.getFullYear();
        let months = end.getMonth() - start.getMonth();
        let days = end.getDate() - start.getDate();

        if (days < 0) {
            // 向前借一個月
            months--;
            const previousMonth = new Date(end.getFullYear(), end.getMonth(), 0);
            days += previousMonth.getDate();
        }

        if (months < 0) {
            years--;
            months += 12;
        }

        return { years, months, days };
    }
}

// 在 Modal 顯示時初始化
document.addEventListener('DOMContentLoaded', function() {
    // 監聽 Modal 顯示事件
    document.addEventListener('shown.bs.modal', function(e) {
        if (e.target.id === 'applyRental') {
            setTimeout(initializeRentalDateCount, 100);
        }
    });
    
    // 如果元素已經存在於頁面中，直接初始化
    setTimeout(initializeRentalDateCount, 500);
});