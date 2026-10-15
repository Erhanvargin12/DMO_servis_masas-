/**
 * AdminUI — Core JavaScript
 *
 * Bileşenler:
 *   Toast · Modal · Theme · Sidebar · Submenus · ProfileDropdown
 *   FileDropZone · TablePager · ClickableRow · MenuBadge
 */
(function () {
    'use strict';

    /* ─────────────────────────────────────────────────────────────────────────
       Helpers
    ───────────────────────────────────────────────────────────────────────── */
    function getUserKey() {
        return document.body.dataset.user || 'guest';
    }

    /* ─────────────────────────────────────────────────────────────────────────
       Toast
    ───────────────────────────────────────────────────────────────────────── */
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
            var icons  = { success: '<i class="fa-solid fa-check"></i>', error: '<i class="fa-solid fa-xmark"></i>', warning: '<i class="fa-solid fa-triangle-exclamation"></i>', info: '<i class="fa-solid fa-circle-info"></i>' };
            var labels = { success: 'Başarılı', error: 'Hata', warning: 'Uyarı', info: 'Bilgi' };
            var t = document.createElement('div');
            t.className = 'toast toast-' + o.type;
            t.innerHTML =
                '<div class="toast-icon">'    + icons[o.type] + '</div>' +
                '<div class="toast-content">' +
                    '<div class="toast-title">'   + (o.title || labels[o.type]) + '</div>' +
                    (o.message ? '<div class="toast-message">' + o.message + '</div>' : '') +
                '</div>' +
                '<button type="button" class="toast-close"><i class="fa-solid fa-xmark"></i></button>';
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

    /* ─────────────────────────────────────────────────────────────────────────
       Modal
       Attribute API:
         data-modal-open="modalId"   → buton
         data-modal-close            → kapat butonu / overlay tıklaması
         data-item-id="x"            → düzenle butonunda tablo satırından veri yükle
         data-delete / data-delete-modal / data-delete-name → silme onayı
    ───────────────────────────────────────────────────────────────────────── */
    var Modal = {
        _draftKey: function (modal) {
            var f = modal.querySelector('form');
            return f ? 'modal_draft_' + (f.action || modal.id) : null;
        },

        _saveDraft: function (modal) {
            var key = this._draftKey(modal);
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
            var key = this._draftKey(modal);
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
            var key = this._draftKey(modal);
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
            modal.querySelectorAll('[data-file-list]').forEach(function (el) { el.innerHTML = ''; });
        },

        openDelete: function (modalId, itemId, itemName) {
            var m = document.getElementById(modalId);
            if (!m) return;
            var idEl   = document.getElementById(modalId + 'ItemId');
            var nameEl = document.getElementById(modalId + 'ItemName');
            if (idEl)   idEl.value        = itemId;
            if (nameEl) nameEl.textContent = itemName || '';
            this.open(m);
        },

        reset: function (modal) {
            var self = this;
            var f = modal.querySelector('form');
            if (!f) return;
            f.reset();
            var id = f.querySelector('input[name="Id"]');
            if (id) id.value = '0';
            var title = modal.querySelector('.modal-title');
            var btn   = modal.querySelector('button[type="submit"]');
            if (title && title.dataset.createTitle) title.textContent = title.dataset.createTitle;
            if (btn   && btn.dataset.createText)    btn.textContent   = btn.dataset.createText;
            if (this._restoreDraft(modal)) {
                setTimeout(function () {
                    var banner = modal.querySelector('.draft-banner');
                    if (!banner) {
                        banner = document.createElement('div');
                        banner.className = 'draft-banner';
                        banner.innerHTML =
                            '<i class="fa-solid fa-pen-to-square"></i>' +
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

    /* ─────────────────────────────────────────────────────────────────────────
       Theme
    ───────────────────────────────────────────────────────────────────────── */
    var Theme = {
        init: function () {
            var t = localStorage.getItem('theme') || 'light';
            this.apply(t);
            this._updateIcon(t);
        },
        toggle: function () {
            var next = (document.documentElement.getAttribute('data-theme') === 'dark') ? 'light' : 'dark';
            localStorage.setItem('theme', next);
            this.apply(next);
            this._updateIcon(next);
        },
        apply: function (t) {
            if (t === 'dark') {
                document.documentElement.setAttribute('data-theme', 'dark');
                if (document.body) document.body.setAttribute('data-theme', 'dark');
            } else {
                document.documentElement.removeAttribute('data-theme');
                if (document.body) document.body.removeAttribute('data-theme');
            }
        },
        _updateIcon: function (t) {
            var sun  = document.querySelector('.icon-sun');
            var moon = document.querySelector('.icon-moon');
            if (!sun || !moon) return;
            sun.style.display  = t === 'dark' ? 'none' : '';
            moon.style.display = t === 'dark' ? '' : 'none';
        }
    };

    /* ─────────────────────────────────────────────────────────────────────────
       Sidebar
    ───────────────────────────────────────────────────────────────────────── */
    var Sidebar = {
        _key: function () { return 'sidebar_collapsed_' + getUserKey(); },

        init: function () {
            var sb = document.getElementById('sidebar');
            if (!sb) return;
            if (window.innerWidth > 768) {
                var saved = localStorage.getItem(this._key());
                if (saved !== 'false') {
                    sb.classList.add('collapsed');
                    if (saved === null) localStorage.setItem(this._key(), 'true');
                }
            }
            this._highlightActive();
        },

        toggle: function () {
            var sb = document.getElementById('sidebar');
            var ov = document.getElementById('mobileOverlay');
            if (!sb) return;
            if (window.innerWidth <= 768) {
                var isOpen = sb.classList.toggle('open');
                if (ov) ov.classList.toggle('active', isOpen);
                document.body.style.overflow = isOpen ? 'hidden' : '';
            } else {
                var collapsed = sb.classList.toggle('collapsed');
                localStorage.setItem(this._key(), String(collapsed));
            }
        },

        _highlightActive: function () {
            var path = window.location.pathname.toLowerCase();
            document.querySelectorAll('.menu-link').forEach(function (link) {
                var href = (link.getAttribute('href') || '').toLowerCase();
                if (href && href !== 'javascript:void(0)' && (path === href || (href !== '/' && path.startsWith(href)))) {
                    link.classList.add('active');
                }
            });
        }
    };

    /* ─────────────────────────────────────────────────────────────────────────
       Submenus (accordion)
    ───────────────────────────────────────────────────────────────────────── */
    function initSubmenus() {
        var KEY = 'submenu_open_' + getUserKey();
        function getIds()     { try { return JSON.parse(localStorage.getItem(KEY) || '[]'); } catch(e) { return []; } }
        function saveIds(ids) { try { localStorage.setItem(KEY, JSON.stringify(ids)); } catch(e) {} }

        var openIds = getIds();
        document.querySelectorAll('.submenu-link.active, .submenu .menu-link.active').forEach(function (a) {
            var g  = a.closest('.menu-group');
            if (!g) return;
            g.classList.add('open');
            var t  = g.querySelector('[data-menu-id]');
            var id = t ? t.getAttribute('data-menu-id') : null;
            if (id && !openIds.includes(id)) { openIds.push(id); saveIds(openIds); }
        });

        openIds = getIds();
        document.querySelectorAll('[data-menu-id]').forEach(function (t) {
            var id = t.getAttribute('data-menu-id');
            if (id && openIds.includes(id)) {
                var g = t.closest('.menu-group');
                if (g) g.classList.add('open');
            }
        });

        document.querySelectorAll('[data-toggle="submenu"]').forEach(function (toggle) {
            toggle.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                var g = toggle.closest('.menu-group');
                if (!g) return;
                var id     = toggle.getAttribute('data-menu-id');
                var isOpen = g.classList.toggle('open');
                if (!id) return;
                var ids = getIds();
                if (isOpen) { if (!ids.includes(id)) ids.push(id); }
                else        { ids = ids.filter(function (s) { return s !== id; }); }
                saveIds(ids);
            });
        });
    }

    /* ─────────────────────────────────────────────────────────────────────────
       Touch swipe sidebar
    ───────────────────────────────────────────────────────────────────────── */
    function initTouchSidebar() {
        var startX = 0, startY = 0;
        document.addEventListener('touchstart', function (e) {
            startX = e.touches[0].clientX; startY = e.touches[0].clientY;
        }, { passive: true });
        document.addEventListener('touchend', function (e) {
            if (window.innerWidth > 768) return;
            var dx = e.changedTouches[0].clientX - startX;
            var dy = e.changedTouches[0].clientY - startY;
            if (Math.abs(dy) > Math.abs(dx) || Math.abs(dx) < 50) return;
            var sb = document.getElementById('sidebar');
            var ov = document.getElementById('mobileOverlay');
            if (!sb) return;
            if (dx > 0 && startX < 40 && !sb.classList.contains('open')) {
                sb.classList.add('open'); if (ov) ov.classList.add('active');
            } else if (dx < 0 && sb.classList.contains('open')) {
                sb.classList.remove('open'); if (ov) ov.classList.remove('active');
            }
        }, { passive: true });
    }

    /* ─────────────────────────────────────────────────────────────────────────
       Resize → restore sidebar state
    ───────────────────────────────────────────────────────────────────────── */
    function initResize() {
        var timer;
        window.addEventListener('resize', function () {
            clearTimeout(timer);
            timer = setTimeout(function () {
                var sb = document.getElementById('sidebar');
                var ov = document.getElementById('mobileOverlay');
                if (!sb) return;
                if (window.innerWidth > 768) {
                    sb.classList.remove('open');
                    if (ov) ov.classList.remove('active');
                    document.body.style.overflow = '';
                    if (localStorage.getItem(Sidebar._key()) === 'false') sb.classList.remove('collapsed');
                    else sb.classList.add('collapsed');
                } else {
                    sb.classList.remove('collapsed');
                }
            }, 120);
        });
    }

    /* ─────────────────────────────────────────────────────────────────────────
       Profile Dropdown
    ───────────────────────────────────────────────────────────────────────── */
    var ProfileDropdown = {
        init: function () {
            var dd = document.getElementById('profileDropdown');
            var tg = document.getElementById('profileToggle');
            if (!dd || !tg) return;
            tg.addEventListener('click', function (e) { e.stopPropagation(); dd.classList.toggle('open'); });
            document.addEventListener('click', function (e) { if (!dd.contains(e.target)) dd.classList.remove('open'); });
        }
    };

    /* ─────────────────────────────────────────────────────────────────────────
       FileDropZone  (data-file-drop / data-file-list)
    ───────────────────────────────────────────────────────────────────────── */
    var FileDropZone = {
        ICONS: {
            pdf: 'fa-file-pdf', doc: 'fa-file-word', docx: 'fa-file-word',
            xls: 'fa-file-excel', xlsx: 'fa-file-excel',
            zip: 'fa-file-zipper', rar: 'fa-file-zipper',
            jpg: 'fa-file-image', jpeg: 'fa-file-image', png: 'fa-file-image', gif: 'fa-file-image', webp: 'fa-file-image',
            txt: 'fa-file-lines', mp4: 'fa-file-video', avi: 'fa-file-video', mov: 'fa-file-video'
        },

        _icon: function (name) {
            var ext = (name.split('.').pop() || '').toLowerCase();
            return this.ICONS[ext] || 'fa-file';
        },

        _size: function (b) {
            if (b < 1024)    return b + ' B';
            if (b < 1048576) return (b / 1024).toFixed(1) + ' KB';
            return (b / 1048576).toFixed(1) + ' MB';
        },

        init: function (zone) {
            var self  = this;
            var input = zone.querySelector('input[type="file"]');
            var list  = zone.parentElement && zone.parentElement.querySelector('[data-file-list]');
            if (!input || !list) return;
            var dt = new DataTransfer();

            function render() {
                list.innerHTML = '';
                Array.from(dt.files).forEach(function (f, i) {
                    var item = document.createElement('div');
                    item.className = 'file-list-item';
                    item.innerHTML =
                        '<i class="fa-solid ' + self._icon(f.name) + ' file-list-icon"></i>' +
                        '<span class="file-list-name" title="' + f.name + '">' + f.name + '</span>' +
                        '<span class="file-list-size">' + self._size(f.size) + '</span>' +
                        '<button type="button" class="file-list-remove" data-idx="' + i + '" title="Kaldır"><i class="fa-solid fa-xmark"></i></button>';
                    list.appendChild(item);
                });
                input.files = dt.files;
            }

            function add(files) {
                var arr = Array.from(files);
                arr.forEach(function (f) { dt.items.add(f); });
                render();
            }

            input.addEventListener('change', function () {
                var selected = Array.from(input.files);
                selected.forEach(function (f) { dt.items.add(f); });
                render();
            });
            list.addEventListener('click', function (e) {
                var btn = e.target.closest('[data-idx]');
                if (!btn) return;
                var nd = new DataTransfer();
                Array.from(dt.files).forEach(function (f, i) { if (i !== parseInt(btn.dataset.idx, 10)) nd.items.add(f); });
                dt = nd; render();
            });
            ['dragenter', 'dragover'].forEach(function (ev) {
                zone.addEventListener(ev, function (e) { e.preventDefault(); zone.classList.add('drag-over'); });
            });
            ['dragleave', 'drop'].forEach(function (ev) {
                zone.addEventListener(ev, function (e) {
                    e.preventDefault(); zone.classList.remove('drag-over');
                    if (ev === 'drop' && e.dataTransfer.files.length) add(e.dataTransfer.files);
                });
            });

            var form = input.closest('form');
            if (form) {
                form.addEventListener('submit', function () {
                    input.files = dt.files;
                });
            }
        },

        initAll: function () {
            var self = this;
            document.querySelectorAll('[data-file-drop]').forEach(function (z) { self.init(z); });
        }
    };

    /* ─────────────────────────────────────────────────────────────────────────
       TablePager  (data-paginate)
       HTML: <table data-paginate data-page-size="10"> ... </table>
       JS otomatik: stat-chip, page-size-select inject, footer pager
    ───────────────────────────────────────────────────────────────────────── */
    var TABLE_SIZES = [10, 25, 50, 100];

    function TablePager(table) {
        this.table       = table;
        this.pageSize    = parseInt(table.getAttribute('data-page-size')) || 10;
        this.currentPage = 1;
        this.sortCol     = -1;
        this.sortDir     = 'asc';
        this._all        = [];
        this._visible    = [];
        this._container  = table.closest('.data-table') || null;

        var footerId     = table.getAttribute('data-footer');
        this._footer     = footerId
            ? document.getElementById(footerId)
            : (this._container ? this._container.querySelector('.data-table-footer') : null);

        this._sizeSelect = this._container ? this._container.querySelector('.table-page-size-select') : null;
        if (!this._sizeSelect && this._container) this._sizeSelect = this._injectSizeSelect();

        this._init();
    }

    TablePager.prototype._injectSizeSelect = function () {
        var actionsEl = this._container.querySelector('.data-table-header .data-table-actions');
        if (!actionsEl) return null;
        var self = this;
        var wrap = document.createElement('div');
        wrap.className = 'table-page-size-wrap';
        var sel = document.createElement('select');
        sel.className = 'table-page-size-select';
        TABLE_SIZES.forEach(function (s) {
            var opt = document.createElement('option');
            opt.value = s; opt.textContent = s + ' kayıt';
            if (s === self.pageSize) opt.selected = true;
            sel.appendChild(opt);
        });
        wrap.appendChild(sel);
        actionsEl.appendChild(wrap);
        return sel;
    };

    TablePager.prototype._init = function () {
        var self  = this;
        var tbody = this.table.querySelector('tbody');
        if (!tbody) return;
        this._all     = Array.prototype.slice.call(tbody.querySelectorAll('tr'));
        this._visible = this._all.slice();

        var ths = Array.prototype.slice.call(this.table.querySelectorAll('thead th'));
        ths.forEach(function (th) {
            var st = th.getAttribute('data-sort');
            if (!st) return;
            th.classList.add('sortable-th');
            var icon = document.createElement('span');
            icon.className = 'sort-icon';
            icon.innerHTML = ' <i class="fa-solid fa-sort"></i>';
            th.appendChild(icon);
            th.addEventListener('click', function () { self._sortBy(ths.indexOf(th), st); });
        });

        if (this._sizeSelect) {
            this._sizeSelect.value = this.pageSize;
            this._sizeSelect.addEventListener('change', function () {
                self.pageSize = parseInt(this.value) || 10;
                self.currentPage = 1;
                self._render(); self._updateUI();
            });
        }
        this._render(); this._updateUI();
    };

    TablePager.prototype.setRows = function (rows) { this._visible = rows; this.currentPage = 1; this._render(); this._updateUI(); };
    TablePager.prototype.filter  = function (fn)   { this._visible = this._all.filter(fn); this.currentPage = 1; this._render(); this._updateUI(); };
    TablePager.prototype.allRows = function ()      { return this._all; };

    TablePager.prototype._render = function () {
        var tbody = this.table.querySelector('tbody');
        if (!tbody) return;
        var start = (this.currentPage - 1) * this.pageSize;
        var page  = this._visible.slice(start, start + this.pageSize);
        this._all.forEach(function (tr) { tr.style.display = 'none'; });
        page.forEach(function (tr) { tr.style.display = ''; if (tr.parentNode !== tbody) tbody.appendChild(tr); });
    };

    TablePager.prototype._updateUI = function () {
        var total      = this._visible.length;
        var totalPages = Math.max(1, Math.ceil(total / this.pageSize));
        var s          = total === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
        var e          = Math.min(this.currentPage * this.pageSize, total);
        var infoText   = total === 0 ? 'Kayıt bulunamadı' : s + '–' + e + ' / ' + total + ' kayıt';

        if (this._container) {
            var chip = this._container.querySelector('.data-table-header .stat-chip');
            if (chip) chip.textContent = total + ' kayıt';
        }
        if (!this._footer) return;

        var infoEl = this._footer.querySelector('.data-table-info');
        if (!infoEl) {
            infoEl = document.createElement('div');
            infoEl.className = 'data-table-info';
            this._footer.insertBefore(infoEl, this._footer.firstChild);
        }
        infoEl.textContent = infoText;

        var pagerEl = this._footer.querySelector('.table-pager-pages');
        if (!pagerEl) {
            pagerEl = document.createElement('div');
            pagerEl.className = 'table-pager-pages';
            this._footer.appendChild(pagerEl);
        }
        this._renderPager(pagerEl, totalPages);
    };

    TablePager.prototype._renderPager = function (el, totalPages) {
        var self  = this;
        var range = _tablePageRange(this.currentPage, totalPages);
        var html  = '';
        html += '<button class="pager-btn" data-page="prev"' + (this.currentPage <= 1 ? ' disabled' : '') + '><i class="fa-solid fa-chevron-left"></i></button>';
        range.forEach(function (p) {
            html += p === '…'
                ? '<span class="pager-ellipsis">…</span>'
                : '<button class="pager-btn' + (p === self.currentPage ? ' active' : '') + '" data-page="' + p + '">' + p + '</button>';
        });
        html += '<button class="pager-btn" data-page="next"' + (this.currentPage >= totalPages ? ' disabled' : '') + '><i class="fa-solid fa-chevron-right"></i></button>';
        el.innerHTML = html;

        el.querySelectorAll('.pager-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var pg = this.getAttribute('data-page');
                if      (pg === 'prev' && self.currentPage > 1)          self.currentPage--;
                else if (pg === 'next' && self.currentPage < totalPages) self.currentPage++;
                else if (pg !== 'prev' && pg !== 'next')                 self.currentPage = parseInt(pg);
                else return;
                self._render(); self._updateUI();
            });
        });
    };

    TablePager.prototype._sortBy = function (colIdx, dataType) {
        this.sortDir = (this.sortCol === colIdx && this.sortDir === 'asc') ? 'desc' : 'asc';
        this.sortCol = colIdx;
        var dir = this.sortDir;
        this._visible.sort(function (a, b) {
            var ac = a.cells[colIdx], bc = b.cells[colIdx];
            if (!ac || !bc) return 0;
            var av = (ac.getAttribute('data-sort-val') || ac.textContent || '').trim();
            var bv = (bc.getAttribute('data-sort-val') || bc.textContent || '').trim();
            if (dataType === 'num')  return dir === 'asc' ? (parseFloat(av)||0) - (parseFloat(bv)||0) : (parseFloat(bv)||0) - (parseFloat(av)||0);
            if (dataType === 'date') return dir === 'asc' ? new Date(av) - new Date(bv) : new Date(bv) - new Date(av);
            return dir === 'asc' ? av.localeCompare(bv, 'tr', { sensitivity: 'base' }) : bv.localeCompare(av, 'tr', { sensitivity: 'base' });
        });
        var ths = Array.prototype.slice.call(this.table.querySelectorAll('thead th'));
        ths.forEach(function (th) {
            var si = th.querySelector('.sort-icon');
            if (!si) return;
            si.innerHTML = ths.indexOf(th) === colIdx
                ? (dir === 'asc' ? ' <i class="fa-solid fa-sort-up"></i>' : ' <i class="fa-solid fa-sort-down"></i>')
                : ' <i class="fa-solid fa-sort"></i>';
        });
        this.currentPage = 1; this._render(); this._updateUI();
    };

    function _tablePageRange(current, total) {
        if (total <= 7) { var r = []; for (var i = 1; i <= total; i++) r.push(i); return r; }
        var result = [1];
        if (current > 3) result.push('…');
        for (var i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) result.push(i);
        if (current < total - 2) result.push('…');
        result.push(total);
        return result;
    }

    /* ─────────────────────────────────────────────────────────────────────────
       Menu Badge  (Sidebar rozeti)
       Kullanım: AdminUI.setMenuBadge('/Tickets', 5)
    ───────────────────────────────────────────────────────────────────────── */
    function setMenuBadge(urlPattern, count) {
        document.querySelectorAll('#sidebar .menu-link[href], #sidebar .submenu-link[href]').forEach(function (link) {
            if ((link.getAttribute('href') || '').toLowerCase().indexOf(urlPattern.toLowerCase()) === -1) return;
            var badge = link.querySelector('.menu-badge');
            if (!badge) { badge = document.createElement('span'); badge.className = 'menu-badge'; link.appendChild(badge); }
            badge.textContent    = count > 99 ? '99+' : count;
            badge.style.display  = count > 0 ? '' : 'none';
        });
    }

    /* ─────────────────────────────────────────────────────────────────────────
       Boot
    ───────────────────────────────────────────────────────────────────────── */
    function init() {
        Theme.init();
        Sidebar.init();
        ProfileDropdown.init();
        initSubmenus();
        initTouchSidebar();
        initResize();

        var stg = document.getElementById('sidebarToggle');
        if (stg) stg.addEventListener('click', function () { Sidebar.toggle(); });

        var ttg = document.getElementById('themeToggle');
        if (ttg) ttg.addEventListener('click', function () { Theme.toggle(); });

        var ov = document.getElementById('mobileOverlay');
        if (ov) ov.addEventListener('click', function () {
            var sb = document.getElementById('sidebar');
            if (sb) sb.classList.remove('open');
            ov.classList.remove('active');
        });

        // Modal: open
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-modal-open]');
            if (!btn) return;
            e.preventDefault();
            e.stopPropagation();
            var modalId = btn.getAttribute('data-modal-open');
            var m = document.getElementById(modalId);
            if (!m) { console.warn('AdminUI Modal: #' + modalId + ' bulunamadı'); return; }
            var itemId = btn.getAttribute('data-item-id');
            try {
                if (itemId) { Modal.loadData(m, itemId); } else { Modal.reset(m); }
            } catch (err) { console.error('AdminUI Modal.reset hatası:', err); }
            Modal.open(m);
        });

        // Modal: delete shortcut
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
            if (e.target.closest('[data-modal-close]')) { var m = e.target.closest('.modal-overlay'); if (m) Modal.close(m); }
            if (e.target.classList.contains('modal-overlay')) Modal.close(e.target);
        });

        // ESC → modal kapat
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { var a = document.querySelector('.modal-overlay.active'); if (a) Modal.close(a); }
        });

        // File drop zones
        FileDropZone.initAll();

        // Tıklanabilir satır: tr[data-href]
        document.addEventListener('click', function (e) {
            var row = e.target.closest('tr[data-href]');
            if (!row || e.target.closest('a, button, input, select, label')) return;
            window.location.href = row.getAttribute('data-href');
        });

        // TablePager auto-init
        document.querySelectorAll('table[data-paginate]').forEach(function (t) {
            t._pager = new TablePager(t);
        });
    }

    // localStorage migration
    (function () {
        for (var i = 0; i < localStorage.length; i++) {
            var k = localStorage.key(i);
            if (k && k.startsWith('sidebar_collapsed_')) {
                var v = localStorage.getItem(k);
                if (v !== 'true' && v !== 'false') localStorage.setItem(k, 'true');
            }
        }
    })();

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

    window.AdminUI = {
        Toast:        Toast,
        Modal:        Modal,
        Theme:        Theme,
        Sidebar:      Sidebar,
        FileDropZone: FileDropZone,
        TablePager:   TablePager,
        setMenuBadge: setMenuBadge
    };

})();
