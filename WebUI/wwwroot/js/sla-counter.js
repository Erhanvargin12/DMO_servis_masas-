/**
 * DMO IT Servis Masası - Canlı SLA Geri Sayım Sayacı (SLA Countdown Timer)
 * Biletlerin öncelik seviyesine göre kalan çözüm süresini saniye saniye canlı günceller.
 */

function initSlaCounters() {
    const elements = document.querySelectorAll('[data-sla-created]');
    if (!elements.length) return;

    function update() {
        const now = new Date().getTime();

        elements.forEach(el => {
            const createdStr = el.getAttribute('data-sla-created');
            const priorityId = parseInt(el.getAttribute('data-sla-priority') || '2', 10);
            const statusId = parseInt(el.getAttribute('data-sla-status') || '1', 10);

            // Çözülmüş veya kapatılmış biletler için sabit yeşil durum
            if (statusId === 5 || statusId === 6) {
                el.innerHTML = '<span class="sla-badge sla-completed"><i class="fa-solid fa-circle-check"></i> SLA Süresinde Tamamlandı</span>';
                return;
            }

            if (!createdStr) return;

            const createdDate = new Date(createdStr).getTime();
            if (isNaN(createdDate)) return;

            // SLA Hedef Süreleri (Saat Cinsinden)
            let slaHours = 8; // Orta (varsayılan)
            if (priorityId === 4) slaHours = 2;       // Kritik
            else if (priorityId === 3) slaHours = 4;  // Yüksek
            else if (priorityId === 2) slaHours = 8;  // Orta
            else if (priorityId === 1) slaHours = 24; // Düşük

            const targetTime = createdDate + (slaHours * 60 * 60 * 1000);
            const diff = targetTime - now;

            if (diff > 0) {
                const hours = Math.floor(diff / (1000 * 60 * 60));
                const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
                const seconds = Math.floor((diff % (1000 * 60)) / 1000);

                let timeStr = "";
                if (hours > 0) timeStr += `${hours}sa `;
                timeStr += `${minutes < 10 ? '0' + minutes : minutes}dk ${seconds < 10 ? '0' + seconds : seconds}sn`;

                let badgeClass = "sla-normal";
                let iconClass = "fa-clock";

                if (hours === 0 && minutes < 30) {
                    badgeClass = "sla-warning pulse-sla";
                    iconClass = "fa-triangle-exclamation";
                }

                el.innerHTML = `<span class="sla-badge ${badgeClass}"><i class="fa-solid ${iconClass}"></i> Kalan: ${timeStr}</span>`;
            } else {
                // SLA İhlali (Süre Aşımı)
                const overdueDiff = Math.abs(diff);
                const overdueHours = Math.floor(overdueDiff / (1000 * 60 * 60));
                const overdueMins = Math.floor((overdueDiff % (1000 * 60 * 60)) / (1000 * 60));

                let overdueStr = "";
                if (overdueHours > 0) overdueStr += `${overdueHours}sa `;
                overdueStr += `${overdueMins}dk`;

                el.innerHTML = `<span class="sla-badge sla-breached pulse-breach"><i class="fa-solid fa-fire"></i> SLA İhlali (-${overdueStr})</span>`;
            }
        });
    }

    update();
    setInterval(update, 1000);
}

document.addEventListener('DOMContentLoaded', initSlaCounters);
