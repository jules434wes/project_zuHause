const startDateInput = document.getElementById("RentalStartDate");
const endDateInput = document.getElementById("RentalEndDate");
const displayText = document.getElementById("rentalDurationText");

startDateInput.addEventListener("change", updateDuration);
endDateInput.addEventListener("change", updateDuration);

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
    displayText.textContent = `${startValue} 至 ${endValue}，共 ${diff.years} 年 ${diff.months} 月 ${diff.days} 日`;
}

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
