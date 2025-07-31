//GetDistrictsByCity

function initializeDistrictSelector() {
    let AddressArea = document.querySelectorAll(".address-area");
    
    AddressArea.forEach((area) => {
        let cityArea = area.querySelector(".address-area-city");
        let districtArea = area.querySelector(".address-area-district");

        if (!cityArea || !districtArea) return;

        let citySelect = cityArea.querySelector("select");
        let districtSelect = districtArea.querySelector("select");
        
        if (!citySelect || !districtSelect) return;

        // 移除之前的事件監聽器（避免重複綁定）
        citySelect.removeEventListener("change", handleCityChange);
        
        function handleCityChange() {
            const cityId = this.value;
            console.log(cityId);

            fetch(`/Member/GetDistrictsByCity?cityId=${cityId}`)
                .then(res => res.json())
                .then(data => {
                    console.log(data);
                    let options = `<option selected>區域</option>`;
                    let elems = document.createDocumentFragment();
                    districtSelect.innerHTML = options;

                    data.forEach(district => {
                        let option = document.createElement("option");
                        option.value = district.districtId;
                        option.textContent = district.districtName;
                        elems.appendChild(option);
                    });
                    districtSelect.appendChild(elems);
                })
                .catch(error => {
                    console.error('Error fetching districts:', error);
                });
        }
        
        citySelect.addEventListener("change", handleCityChange);
    });
}

// 在 Modal 顯示時初始化
document.addEventListener('DOMContentLoaded', function() {   
    // 監聽 Modal 顯示事件
    document.addEventListener('shown.bs.modal', function(e) {
        if (e.target.id === 'applyRental' || e.target.id === 'applyHouse') {
            setTimeout(initializeDistrictSelector, 100);
        }
    });
    
    // 如果元素已經存在於頁面中，直接初始化
    setTimeout(initializeDistrictSelector, 500);
});