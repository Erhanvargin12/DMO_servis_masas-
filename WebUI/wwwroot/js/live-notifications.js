/**
 * Real-Time Notifications, Web Audio API Sound Chime & Read/Unread Management Engine
 * Devlet Malzeme Ofisi (DMO) IT Servis Masası — Anlık Bildirim ve Otomatik Yenileme Modülü
 */
(function () {
    'use strict';

    var STORAGE_KEY_HASH = 'dmo_live_notif_hash';
    var STORAGE_KEY_READ = 'dmo_read_notif_ids';
    
    var lastNotificationHash = sessionStorage.getItem(STORAGE_KEY_HASH) || '';
    var isPolling = false;
    var cachedNotifications = [];

    // Retrieve read notification IDs from localStorage
    function getReadIds() {
        try {
            var raw = localStorage.getItem(STORAGE_KEY_READ);
            return raw ? JSON.parse(raw) : [];
        } catch (e) {
            return [];
        }
    }

    function saveReadId(id) {
        var readIds = getReadIds();
        if (readIds.indexOf(id) === -1) {
            readIds.push(id);
            try { localStorage.setItem(STORAGE_KEY_READ, JSON.stringify(readIds)); } catch (e) {}
        }
    }

    function saveAllReadIds(ids) {
        var readIds = getReadIds();
        ids.forEach(function (id) {
            if (readIds.indexOf(id) === -1) {
                readIds.push(id);
            }
        });
        try { localStorage.setItem(STORAGE_KEY_READ, JSON.stringify(readIds)); } catch (e) {}
    }

    // Web Audio API Soft Pleasant Notification Chime
    function playNotificationChime() {
        try {
            var AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            var ctx = new AudioContext();
            
            // First Tone (659Hz - E5)
            var osc1 = ctx.createOscillator();
            var gain1 = ctx.createGain();
            osc1.type = 'sine';
            osc1.frequency.setValueAtTime(659.25, ctx.currentTime);
            gain1.gain.setValueAtTime(0.12, ctx.currentTime);
            gain1.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.3);
            osc1.connect(gain1);
            gain1.connect(ctx.destination);
            osc1.start(ctx.currentTime);
            osc1.stop(ctx.currentTime + 0.3);

            // Second Tone (880Hz - A5)
            var osc2 = ctx.createOscillator();
            var gain2 = ctx.createGain();
            osc2.type = 'sine';
            osc2.frequency.setValueAtTime(880, ctx.currentTime + 0.12);
            gain2.gain.setValueAtTime(0.15, ctx.currentTime + 0.12);
            gain2.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.45);
            osc2.connect(gain2);
            gain2.connect(ctx.destination);
            osc2.start(ctx.currentTime + 0.12);
            osc2.stop(ctx.currentTime + 0.45);
        } catch (e) {
            // Audio context blocked or unsupported
        }
    }

    window.toggleLiveNotifMenu = function (e) {
        if (e) {
            e.preventDefault();
            e.stopPropagation();
        }
        var menu = document.getElementById('liveNotifMenu');
        if (!menu) return;
        
        var isCurrentlyVisible = menu.style.display === 'block' || menu.classList.contains('active');
        
        if (isCurrentlyVisible) {
            menu.classList.remove('active');
            menu.style.setProperty('display', 'none', 'important');
        } else {
            menu.style.setProperty('display', 'block', 'important');
            menu.classList.add('active');
            fetchLiveUpdates();
        }
    };

    window.markAllNotificationsAsRead = function (e) {
        if (e) {
            e.preventDefault();
            e.stopPropagation();
        }
        if (cachedNotifications && cachedNotifications.length > 0) {
            var allIds = cachedNotifications.map(function (n) { return n.id; });
            saveAllReadIds(allIds);
        }
        renderBadge(0);
        if (cachedNotifications) renderNotifications(cachedNotifications);
        if (window.AdminUI && window.AdminUI.Toast) {
            AdminUI.Toast.info('Tüm canlı bildirimler okundu olarak işaretlendi.');
        }
    };

    window.onNotificationItemClick = function (id) {
        saveReadId(id);
        var readIds = getReadIds();
        var unreadCount = cachedNotifications.filter(function (n) { return readIds.indexOf(n.id) === -1; }).length;
        renderBadge(unreadCount);
    };

    // Close menu when clicking outside
    document.addEventListener('click', function (e) {
        var wrapper = e.target.closest('.notification-dropdown-wrapper');
        if (!wrapper) {
            var menu = document.getElementById('liveNotifMenu');
            if (menu) {
                menu.classList.remove('active');
                menu.style.setProperty('display', 'none', 'important');
            }
        }
    });

    function fetchLiveUpdates() {
        if (isPolling) return;
        isPolling = true;

        fetch('/Tickets/Ticket/GetLiveUpdates')
            .then(function (res) { return res.json(); })
            .then(function (data) {
                isPolling = false;
                if (!data || !data.success) return;

                cachedNotifications = data.notifications || [];
                var readIds = getReadIds();

                // Calculate actual unread count dynamically
                var unreadCount = cachedNotifications.filter(function (n) {
                    return readIds.indexOf(n.id) === -1;
                }).length;

                renderBadge(unreadCount);
                renderNotifications(cachedNotifications);

                var currentHash = JSON.stringify(cachedNotifications);

                if (lastNotificationHash && lastNotificationHash !== currentHash) {
                    if (cachedNotifications && cachedNotifications.length > 0) {
                        var latest = cachedNotifications[0];
                        // If latest notification is unread, play chime sound and toast!
                        if (readIds.indexOf(latest.id) === -1) {
                            playNotificationChime();
                            showLiveToast(latest);
                        }
                    }
                }

                lastNotificationHash = currentHash;
                try { sessionStorage.setItem(STORAGE_KEY_HASH, currentHash); } catch (e) {}

                updateStatChips(data);
            })
            .catch(function (err) {
                isPolling = false;
                console.warn('Canlı bildirim senkronizasyon hatası:', err);
            });
    }

    function renderBadge(count) {
        var badge = document.getElementById('livePulseBadge');
        if (!badge) return;
        if (count > 0) {
            badge.style.display = 'inline-block';
            badge.textContent = count > 9 ? '9+' : count;
        } else {
            badge.style.display = 'none';
        }
    }

    function renderNotifications(items) {
        var list = document.getElementById('liveNotifList');
        if (!list) return;

        if (!items || items.length === 0) {
            list.innerHTML = '<div style="padding:24px 16px;text-align:center;color:#64748b;font-size:12.5px;">Henüz bildirim bulunmuyor.</div>';
            return;
        }

        var readIds = getReadIds();
        var html = '';

        items.forEach(function (item) {
            var isRead = readIds.indexOf(item.id) !== -1;
            var badgeClass = item.statusId === 6 ? 'badge-error' : (item.statusId === 3 ? 'badge-warning' : 'badge-success');
            var iconClass = item.statusId === 3 ? 'fa-share-nodes' : (item.statusId === 6 ? 'fa-circle-xmark' : (item.statusId === 2 ? 'fa-user-check' : 'fa-bolt'));
            var ticketCode = item.ticketNo || ('TKT-' + item.id);

            var opacityStyle = isRead ? 'opacity: 0.65;' : 'opacity: 1; font-weight: 600;';
            var unreadDot = isRead 
                ? '<i class="fa-solid fa-check" style="color:#94a3b8; font-size:11px; margin-left:auto;"></i>' 
                : '<span style="width:8px; height:8px; border-radius:50%; background:#4f46e5; display:inline-block; margin-left:auto;"></span>';

            html += '' +
                '<a href="/Tickets/Ticket/Detail/' + item.id + '" class="live-notif-item" onclick="onNotificationItemClick(' + item.id + ')" style="' + opacityStyle + '">' +
                '  <div class="live-notif-icon"><i class="fa-solid ' + iconClass + '"></i></div>' +
                '  <div class="live-notif-content" style="flex:1;">' +
                '    <div class="live-notif-title-row" style="display:flex; align-items:center;">' +
                '      <span class="live-notif-code">' + escapeHtml(ticketCode) + '</span>' +
                '      <span class="live-notif-text">' + escapeHtml(item.title) + '</span>' +
                       unreadDot +
                '    </div>' +
                '    <div class="live-notif-meta">' +
                '      <span class="badge ' + badgeClass + '" style="font-size:10px;padding:2px 7px;">' + escapeHtml(item.statusText) + '</span>' +
                '      <span style="font-size:11px;color:#94a3b8;font-weight:500;">' + item.time + '</span>' +
                '    </div>' +
                '  </div>' +
                '</a>';
        });
        list.innerHTML = html;
    }

    function updateStatChips(data) {
        var ownBadge = document.querySelector('[data-stat="ownCount"]');
        if (ownBadge && data.activeCount !== undefined) {
            ownBadge.textContent = data.activeCount;
        }
        var assignedBadge = document.querySelector('[data-stat="assignedCount"]');
        if (assignedBadge && data.assignedCount !== undefined) {
            assignedBadge.textContent = data.assignedCount;
        }
        var completedBadge = document.querySelector('[data-stat="completedCount"]');
        if (completedBadge && data.completedCount !== undefined) {
            completedBadge.textContent = data.completedCount;
        }
    }

    function showLiveToast(item) {
        if (window.AdminUI && window.AdminUI.Toast) {
            window.AdminUI.Toast.info(
                (item.ticketNo || 'Talep') + ': ' + item.statusText,
                '⚡ Anlık Bildirim Akışı'
            );
        }
    }

    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    // Initial fetch & start 5-second live polling loop
    fetchLiveUpdates();
    setInterval(fetchLiveUpdates, 5000);
})();
