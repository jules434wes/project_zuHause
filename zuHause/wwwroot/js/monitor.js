(() => {
    window.loadHotRankings = async function () {
        try {
            const res = await fetch("/Dashboard/hot-rankings");
            const data = await res.json();

            const furnitureList = document.getElementById("furnitureRankings");
            const rentalList = document.getElementById("rentalCityRankings");
            const pendingList = document.getElementById("pendingList");
            const verifiedList = document.getElementById("verifiedList");

            if (furnitureList) {
                furnitureList.innerHTML = data.furniture
                    .map(item => `<li>${item.name}（${item.count} 次承租）</li>`)
                    .join("");
            }

            if (rentalList) {
                rentalList.innerHTML = data.rental
                    .map(item => `<li>${item.name}（${item.count} 筆成交）</li>`)
                    .join("");
            }

            if (pendingList) {
                pendingList.innerHTML = data.pending
                    .map(item => `<li>${item.name}（${item.count} 筆）</li>`)
                    .join("");
            }

            if (verifiedList) {
                verifiedList.innerHTML = data.verified
                    .map(item => `<li>${item.name}（${item.count} 筆）</li>`)
                    .join("");
            }

        }catch (err) {
            console.error("🔥 熱門排行榜載入失敗", err);
        }
    };

   
})();
