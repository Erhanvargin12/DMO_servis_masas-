/**
 * AdminUI — Core JavaScript
 * Toast · Modal · Form · Theme · Sidebar · Submenus
 */
(function () {
    'use strict';

    // ── Helpers ──────────────────────────────────────────────────────────────
    function getUserKey() {
        return document.body.dataset.user || 'guest';
    }

    // ── Toast ─────────────────────────────────────────────────────────────────
    var Toast = {
        container: null,

        init: function () {
            if (this.container) return;
            this.container = document.createElement('div');
            this.container.id = 'toastContainer';
            this.container.className = 'toast-container';
            document.body.appendChild(this.container);
        },

        show: function (opts) {
            this.init();
            var o = Object.assign({ type: 'info', title: '', message: '', duration: 4000 }, opts);
            var icons = {
                success: '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 6L9 17l-5-5"/></svg>',
                error:   '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>',
                warning: '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>',
                info:    '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'
            };
            var labels = { success: 'Başarılı', error: 'Hata', warning: 'Uyarı', info: 'Bilgi' };
            var t = document.createElement('div');
            t.className = 'toast toast-' + o.type;
            t.innerHTML =
                '<div class="toast-icon">' + icons[o.type] + '</div>' +
                '<div class="toast-content">' +
                    '<div class="toast-title">' + (o.title || labels[o.type]) + '</div>' +
                    (o.message ? '<div class="toast-message">' + o.message + '</div>' : '') +
                '</div>' +
                '<button type="button" class="toast-close"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg></button>';
            this.container.appendChild(t);
            var self = this;
            t.querySelector('.toast-close').addEventListener('click', function () { self.remove(t); });
            if (o.duration > 0) setTimeout(function () { self.remove(t); }, o.duration);
            return t;
        },

        remove: function (t) {
            if (!t || t.classList.contains('removing')) return;
            t.classList.add('removing');
            setTimeout(function () { if (t.parentNode) t.parentNode.removeChild(t); }, 300);
        },

        success: function (msg, title) { return this.show({ type: 'success', message: msg, title: title }); },
        error:   function (msg, title) { return this.show({ type: 'error',   message: msg, title: title }); },
        warning: function (msg, title) { return this.show({ type: 'warning', message: msg, title: title }); },
        info:    function (msg, title) { return this.show({ type: 'info',    message: msg, title: title }); }
    };

    // ── Modal ─────────────────────────────────────────────────────────────────
    var Modal = {
        _key: function (modal) {
            var f = modal.querySelector('form');
            return f ? 'modal_draft_' + (f.action || modal.id) : null;
        },

        _saveDraft: function (modal) {
            var key = this._key(modal);
            if (!key) return;
            var f = modal.querySelector('form');
            if (!f) return;
            var id = f.querySelector('input[name="Id"]');
            if (id && id.value !== '0') return;
            var data = {};
            f.querySelectorAll('input,select,textarea').forEach(function (el) {
                if (!el.name || el.name === '__RequestVerificationToken' || el.type === 'hidden') return;
                data[el.name] = el.type === 'checkbox' ? el.checked : el.value;
            });
            try { localStorage.setItem(key, JSON.stringify(data)); } catch (e) {}
        },

        _restoreDraft: function (modal) {
            var key = this._key(modal);
            if (!key) return false;
            var raw; try { raw = localStorage.getItem(key); } catch (e) {}
            if (!raw) return false;
            var data; try { data = JSON.parse(raw); } catch (e) { return false; }
            var f = modal.querySelector('form');
            if (!f) return false;
            var hasData = false;
            f.querySelectorAll('input,select,textarea').forEach(function (el) {
                if (!el.name || el.name === '__RequestVerificationToken' || el.type === 'hidden') return;
                if (data[el.name] === undefined) return;
                if (el.type === 'checkbox') { el.checked = data[el.name]; }
                else { el.value = data[el.name]; }
                if (data[el.name] !== '' && data[el.name] !== false) hasData = true;
            });
            return hasData;
        },

        _clearDraft: function (modal) {
            var key = this._key(modal);
            if (key) try { localStorage.removeItem(key); } catch (e) {}
        },

        _bindDraft: function (modal) {
            var self = this;
            var f = modal.querySelector('form');
            if (!f || f._draftBound) return;
            f._draftBound = true;
            f.querySelectorAll('input,select,textarea').forEach(function (el) {
                el.addEventListener('input',  function () { self._saveDraft(modal); });
                el.addEventListener('change', function () { self._saveDraft(modal); });
            });
        },

        open: function (modalId) {
            var m = typeof modalId === 'string' ? document.getElementById(modalId) : modalId;
            if (!m) return;
            m.classList.add('active');
            document.body.style.overflow = 'hidden';
            this._bindDraft(m);
            setTimeout(function () {
                var inp = m.querySelector('input:not([type="hidden"]):not([type="checkbox"])');
                if (inp) inp.focus();
            }, 100);
        },

        close: function (modal) {
            if (typeof modal === 'string') modal = document.getElementById(modal);
            if (!modal) return;
            modal.classList.remove('active');
            document.body.style.overflow = '';
        },

        openDelete: function (modalId, itemId, itemName) {
            var m = document.getElementById(modalId);
            if (!m) return;
            var idEl   = document.getElementById(modalId + 'ItemId');
            var nameEl = document.getElementById(modalId + 'ItemName');
            if (idEl)   idEl.value         = itemId;
            if (nameEl) nameEl.textContent = itemName || '';
            this.open(m);
        },

        reset: function (modal) {
            var f = modal.querySelector('form');
            if (!f) return;
            f.reset();
            var id = f.querySelector('input[name="Id"]');
            if (id) id.value = '0';
            var title = modal.querySelector('.modal-title');
            var btn   = modal.querySelector('button[type="submit"]');
            if (title && title.dataset.createTitle) title.textContent = title.dataset.createTitle;
            if (btn   && btn.dataset.createText)    btn.textContent   = btn.dataset.createText;
            var self = this;
            var restored = this._restoreDraft(modal);
            if (restored) {
                setTimeout(function () {
                    var banner = modal.querySelector('.draft-banner');
                    if (!banner) {
                        banner = document.createElement('div');
                        banner.className = 'draft-banner';
                        banner.innerHTML =
                            '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>' +
                            '<span>Kaydedilmemiş taslak geri yüklendi</span>' +
                            '<button type="button" class="draft-discard">Temizle</button>';
                        var body = modal.querySelector('.modal-body') || f;
                        body.insertBefore(banner, body.firstChild);
                        banner.querySelector('.draft-discard').addEventListener('click', function () {
                            self._clearDraft(modal);
                            f.reset();
                            if (id) id.value = '0';
                            banner.remove();
                        });
                    }
                    banner.style.display = 'flex';
                }, 50);
            }
        },

        loadData: function (modal, itemId) {
            var row = document.querySelector('tr[data-item-id="' + itemId + '"]');
            if (!row) return;
            var f = modal.querySelector('form');
            if (!f) return;
            var banner = modal.querySelector('.draft-banner');
            if (banner) banner.style.display = 'none';
            try {
                var data = JSON.parse(row.getAttribute('data-item') || '{}');
                f.querySelectorAll('input,select,textarea').forEach(function (el) {
                    if (!el.name || el.name === '__RequestVerificationToken') return;
                    var v = data[el.name];
                    if (el.type === 'checkbox') { el.checked = v === true || v === 'True' || v === 'true'; }
                    else if (v !== null && v !== undefined) { el.value = v; }
                });
            } catch (e) { console.error(e); }
            var title = modal.querySelector('.modal-title');
            var btn   = modal.querySelector('button[type="submit"]');
            if (title && title.dataset.editTitle) title.textContent = title.dataset.editTitle;
            if (btn   && btn.dataset.editText)    btn.textContent   = btn.dataset.editText;
        }
    };

    // ── Form (AJAX) ───────────────────────────────────────────────────────────
    var Form = {
        submit: function (form, opts) {
            opts = Object.assign({ onSuccess: null, onError: null }, opts);
            var btn = form.querySelector('button[type="submit"]');
            if (btn) { btn.disabled = true; btn.classList.add('btn-loading'); }
            var data = {};
            new FormData(form).forEach(function (v, k) {
                if (k !== '__RequestVerificationToken') data[k] = v;
            });
            form.querySelectorAll('input[type="checkbox"]').forEach(function (cb) { data[cb.name] = cb.checked; });
            ['Id', 'RegistirationId', 'OrganizationUnitId', 'PositionId'].forEach(function (f) {
                if (data[f] !== undefined) data[f] = parseInt(data[f]) || 0;
            });
            var token = form.querySelector('input[name="__RequestVerificationToken"]');
            fetch(form.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token ? token.value : '' },
                body: JSON.stringify(data)
            })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                if (btn) { btn.disabled = false; btn.classList.remove('btn-loading'); }
                if (res.success) {
                    Toast.success(res.message || 'İşlem başarılı');
                    var m = form.closest('.modal-overlay');
                    if (m) Modal.close(m);
                    if (opts.onSuccess) { opts.onSuccess(res); } else { setTimeout(function () { location.reload(); }, 800); }
                } else {
                    Toast.error(res.message || 'Bir hata oluştu');
                    if (opts.onError) opts.onError(res);
                }
            })
            .catch(function (err) {
                if (btn) { btn.disabled = false; btn.classList.remove('btn-loading'); }
                Toast.error('Bağlantı hatası');
                if (opts.onError) opts.onError(err);
            });
        }
    };

    // ── Theme ─────────────────────────────────────────────────────────────────
    var Theme = {
        init: function () {
            var theme = localStorage.getItem('theme') || 'light';
            this.apply(theme);
            this._updateIcon(theme);
        },

        toggle: function () {
            var current = document.documentElement.getAttribute('data-theme') || 'light';
            var next = current === 'dark' ? 'light' : 'dark';
            localStorage.setItem('theme', next);
            this.apply(next);
            this._updateIcon(next);
            return next;
        },

        apply: function (theme) {
            if (theme === 'dark') {
                document.documentElement.setAttribute('data-theme', 'dark');
            } else {
                document.documentElement.removeAttribute('data-theme');
            }
        },

        _updateIcon: function (theme) {
            var sun  = document.querySelector('.icon-sun');
            var moon = document.querySelector('.icon-moon');
            if (!sun || !moon) return;
            if (theme === 'dark') {
                sun.style.display  = 'none';
                moon.style.display = '';
            } else {
                sun.style.display  = '';
                moon.style.display = 'none';
            }
        }
    };

    // ── Sidebar ───────────────────────────────────────────────────────────────
    var Sidebar = {
        _key: function () { return 'sidebar_collapsed_' + getUserKey(); },

        init: function () {
            var sb = document.getElementById('sidebar');
            if (!sb) return;
            if (localStorage.getItem(this._key()) === 'true' && window.innerWidth > 768) {
                sb.classList.add('collapsed');
            }
            this.highlightActive();
        },

        toggle: function () {
            var sb  = document.getElementById('sidebar');
            var ov  = document.getElementById('mobileOverlay');
            if (!sb) return;
            if (window.innerWidth <= 768) {
                sb.classList.toggle('open');
                if (ov) ov.classList.toggle('active');
            } else {
                sb.classList.toggle('collapsed');
                localStorage.setItem(this._key(), sb.classList.contains('collapsed'));
            }
        },

        highlightActive: function () {
            var path = window.location.pathname.toLowerCase();
            document.querySelectorAll('.menu-link').forEach(function (link) {
                var href = (link.getAttribute('href') || '').toLowerCase();
                if (href && href !== 'javascript:void(0)' && (path === href || (href !== '/' && path.startsWith(href)))) {
                    link.classList.add('active');
                }
            });
        }
    };

    // ── Submenus (accordion + state per user) ────────────────────────────────
    function initSubmenus() {
        var storageKey = 'submenu_open_' + getUserKey();
        var openIds = [];
        try { openIds = JSON.parse(localStorage.getItem(storageKey) || '[]'); } catch (e) {}

        // Auto-expand groups containing the active link
        document.querySelectorAll('.submenu .menu-link.active, .submenu .submenu-link.active').forEach(function (a) {
            var g = a.closest('.menu-group');
            if (!g) return;
            g.classList.add('open');
            var id = (g.querySelector('[data-menu-id]') || {}).dataset && g.querySelector('[data-menu-id]').dataset.menuId;
            if (id && !openIds.includes(id)) openIds.push(id);
        });
        try { localStorage.setItem(storageKey, JSON.stringify(openIds)); } catch (e) {}

        // Restore previously open groups
        document.querySelectorAll('[data-menu-id]').forEach(function (link) {
            var id = link.getAttribute('data-menu-id');
            if (id && openIds.includes(id)) {
                var g = link.closest('.menu-group');
                if (g) g.classList.add('open');
            }
        });

        // Toggle on click
        document.addEventListener('click', function (e) {
            var link = e.target.closest('[data-toggle="submenu"]');
            if (!link) return;
            e.preventDefault();
            var g = link.closest('.menu-group');
            if (!g) return;
            var id     = link.getAttribute('data-menu-id');
            var isOpen = g.classList.toggle('open');
            if (!id) return;
            var saved = [];
            try { saved = JSON.parse(localStorage.getItem(storageKey) || '[]'); } catch (e) {}
            if (isOpen) { if (!saved.includes(id)) saved.push(id); }
            else         { saved = saved.filter(function (s) { return s !== id; }); }
            try { localStorage.setItem(storageKey, JSON.stringify(saved)); } catch (e) {}
        });
    }

    // ── Touch: swipe to open/close sidebar on mobile ─────────────────────────
    function initTouchSidebar() {
        var startX = 0, startY = 0;
        var EDGE = 40;   // px from left edge to trigger open
        var MIN  = 50;   // minimum horizontal swipe distance

        document.addEventListener('touchstart', function (e) {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        }, { passive: true });

        document.addEventListener('touchend', function (e) {
            if (window.innerWidth > 768) return;
            var dx = e.changedTouches[0].clientX - startX;
            var dy = e.changedTouches[0].clientY - startY;
            // Reject diagonal / vertical swipes
            if (Math.abs(dy) > Math.abs(dx)) return;
            if (Math.abs(dx) < MIN) return;

            var sb = document.getElementById('sidebar');
            var ov = document.getElementById('mobileOverlay');
            if (!sb) return;

            if (dx > 0 && startX < EDGE && !sb.classList.contains('open')) {
                // Swipe right from left edge → open
                sb.classList.add('open');
                if (ov) ov.classList.add('active');
            } else if (dx < 0 && sb.classList.contains('open')) {
                // Swipe left while open → close
                sb.classList.remove('open');
                if (ov) ov.classList.remove('active');
            }
        }, { passive: true });
    }

    // ── Resize: restore sidebar state on viewport change ─────────────────────
    function initResize() {
        var timer;
        window.addEventListener('resize', function () {
            clearTimeout(timer);
            timer = setTimeout(function () {
                var sb = document.getElementById('sidebar');
                var ov = document.getElementById('mobileOverlay');
                if (!sb) return;

                if (window.innerWidth > 768) {
                    // Switching to desktop: close mobile drawer, restore collapsed pref
                    sb.classList.remove('open');
                    if (ov) ov.classList.remove('active');
                    document.body.style.overflow = '';
                    if (localStorage.getItem(Sidebar._key()) === 'true') {
                        sb.classList.add('collapsed');
                    } else {
                        sb.classList.remove('collapsed');
                    }
                } else {
                    // Switching to mobile: strip collapsed (it means hidden, not narrow)
                    sb.classList.remove('collapsed');
                }
            }, 120);
        });
    }

    // ── Profile Dropdown ──────────────────────────────────────────────────────
    var ProfileDropdown = {
        init: function () {
            var dd = document.getElementById('profileDropdown');
            var tg = document.getElementById('profileToggle');
            if (!dd || !tg) return;
            tg.addEventListener('click', function (e) { e.stopPropagation(); dd.classList.toggle('open'); });
            document.addEventListener('click', function (e) {
                if (!dd.contains(e.target)) dd.classList.remove('open');
            });
        }
    };

    // ── Boot ──────────────────────────────────────────────────────────────────
    function init() {
        Theme.init();
        Sidebar.init();
        ProfileDropdown.init();
        initSubmenus();
        initTouchSidebar();
        initResize();

        // Sidebar toggle
        var stg = document.getElementById('sidebarToggle');
        if (stg) stg.addEventListener('click', function () { Sidebar.toggle(); });

        // Theme toggle
        var ttg = document.getElementById('themeToggle');
        if (ttg) ttg.addEventListener('click', function () { Theme.toggle(); });

        // Mobile overlay
        var ov = document.getElementById('mobileOverlay');
        if (ov) ov.addEventListener('click', function () {
            var sb = document.getElementById('sidebar');
            if (sb) sb.classList.remove('open');
            ov.classList.remove('active');
        });

        // Modal: open buttons
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-modal-open]');
            if (!btn) return;
            e.preventDefault();
            var m = document.getElementById(btn.getAttribute('data-modal-open'));
            if (!m) return;
            var itemId = btn.getAttribute('data-item-id');
            if (itemId) { Modal.loadData(m, itemId); } else { Modal.reset(m); }
            Modal.open(m);
        });

        // Modal: delete buttons
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-delete]');
            if (!btn) return;
            e.preventDefault();
            Modal.openDelete(
                btn.getAttribute('data-delete-modal') || 'deleteModal',
                btn.getAttribute('data-delete'),
                btn.getAttribute('data-delete-name') || ''
            );
        });

        // Modal: close
        document.addEventListener('click', function (e) {
            if (e.target.closest('[data-modal-close]')) {
                var m = e.target.closest('.modal-overlay');
                if (m) Modal.close(m);
            }
            if (e.target.classList.contains('modal-overlay')) Modal.close(e.target);
        });

        // ESC closes modal
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                var a = document.querySelector('.modal-overlay.active');
                if (a) Modal.close(a);
            }
        });

        // AJAX forms
        document.querySelectorAll('.ajax-form').forEach(function (f) {
            f.addEventListener('submit', function (e) { e.preventDefault(); Form.submit(f); });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    window.AdminUI = { Toast: Toast, Modal: Modal, Form: Form, Theme: Theme, Sidebar: Sidebar };
})();
