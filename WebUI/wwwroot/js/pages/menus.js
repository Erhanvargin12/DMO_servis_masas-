/**
 * Menus Page — Menü Yönetimi
 *
 * Sorumluluk:
 *   - Menü ağacı toggle (parent/child satır açma-kapama)
 *   - Tree durumunu sessionStorage'da saklama
 *   - Client-side pagination (tree yapısı nedeniyle server-side değil)
 *   - Client-side arama (tree yapısı nedeniyle server-side değil)
 *   - Rol atama modal açma/doldurma
 */
(function () {
    'use strict';

    var PAGE_SIZE = 10;
    var currentPage = 1;
    var searchTerm = '';

    var searchInput         = document.getElementById('tableSearch');
    var tableBody           = document.querySelector('#menusTable tbody');
    var paginationInfo      = document.getElementById('paginationInfo');
    var paginationContainer = document.getElementById('pagination');
    var countBadge          = document.querySelector('.stat-chip');

    if (!tableBody) return;

    var allParentRows = Array.from(tableBody.querySelectorAll('.menu-row-parent'));
    var emptyRow      = tableBody.querySelector('.empty-row');

    // ─── Tree Toggle ──────────────────────────────────────────────────────────

    function getChildRows(groupId) {
        return Array.from(tableBody.querySelectorAll('tr[data-group="' + groupId + '"]'));
    }

    function expandGroup(groupId, parentRow, persist) {
        parentRow.classList.add('expanded');
        getChildRows(groupId).forEach(function (r) { r.style.display = ''; });
        if (persist !== false) saveTreeState();
    }

    function collapseGroup(groupId, parentRow) {
        parentRow.classList.remove('expanded');
        getChildRows(groupId).forEach(function (r) { r.style.display = 'none'; });
        saveTreeState();
    }

    function saveTreeState() {
        var open = [];
        allParentRows.forEach(function (r) {
            if (r.classList.contains('expanded')) open.push(r.getAttribute('data-group-id'));
        });
        try { sessionStorage.setItem('menuTreeOpen', JSON.stringify(open)); } catch (e) { }
    }

    function initTree() {
        // Tüm child satırları başlangıçta gizle
        Array.from(tableBody.querySelectorAll('.menu-row-child')).forEach(function (r) {
            r.style.display = 'none';
        });

        // Önceki açık grupları geri yükle
        var saved = [];
        try { saved = JSON.parse(sessionStorage.getItem('menuTreeOpen') || '[]'); } catch (e) { }
        allParentRows.forEach(function (parentRow) {
            var gid = parentRow.getAttribute('data-group-id');
            if (gid && saved.includes(gid)) expandGroup(gid, parentRow, false);
        });
    }

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.menu-tree-toggle');
        if (!btn) return;
        var groupId   = btn.getAttribute('data-group');
        var parentRow = btn.closest('tr');
        if (!groupId || !parentRow) return;
        parentRow.classList.contains('expanded')
            ? collapseGroup(groupId, parentRow)
            : expandGroup(groupId, parentRow, true);
    });

    // ─── Pagination ───────────────────────────────────────────────────────────

    function getFilteredParents() {
        if (!searchTerm) return allParentRows.slice();
        return allParentRows.filter(function (row) {
            var d = safeJson(row);
            return [d.Name, d.Description, d.Url].join(' ').toLowerCase().includes(searchTerm);
        });
    }

    function safeJson(row) {
        try { return JSON.parse(row.getAttribute('data-item') || '{}'); } catch (e) { return {}; }
    }

    function renderPagination() {
        var filtered   = getFilteredParents();
        var totalPages = Math.ceil(filtered.length / PAGE_SIZE);
        var start      = (currentPage - 1) * PAGE_SIZE;
        var end        = start + PAGE_SIZE;

        allParentRows.forEach(function (r) { r.style.display = 'none'; });
        Array.from(tableBody.querySelectorAll('.menu-row-child, .menu-row-orphan')).forEach(function (r) {
            r.style.display = 'none';
        });

        filtered.slice(start, end).forEach(function (parentRow) {
            parentRow.style.display = '';
            var gid = parentRow.getAttribute('data-group-id');
            if (gid && parentRow.classList.contains('expanded')) {
                getChildRows(gid).forEach(function (r) { r.style.display = ''; });
            }
        });

        Array.from(tableBody.querySelectorAll('.menu-row-orphan')).forEach(function (r) {
            r.style.display = '';
        });

        if (emptyRow) emptyRow.style.display = filtered.length === 0 ? '' : 'none';

        var showStart = filtered.length > 0 ? start + 1 : 0;
        var showEnd   = Math.min(end, filtered.length);
        if (countBadge)     countBadge.textContent    = filtered.length + ' kayıt';
        if (paginationInfo) paginationInfo.textContent = showStart + '–' + showEnd + ' / ' + filtered.length + ' menü';

        if (!paginationContainer) return;
        if (totalPages <= 1) { paginationContainer.innerHTML = ''; return; }

        var html = '';
        html += pageBtn(currentPage === 1, currentPage - 1,
            '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>');
        for (var i = 1; i <= totalPages; i++) {
            if (totalPages <= 7 || i === 1 || i === totalPages || Math.abs(i - currentPage) <= 1) {
                html += '<button class="btn btn-sm ' + (i === currentPage ? 'btn-primary' : 'btn-ghost') +
                        '" ' + (i === currentPage ? 'disabled' : 'data-page="' + i + '"') + '>' + i + '</button>';
            } else if (i === currentPage - 2 || i === currentPage + 2) {
                html += '<span class="pagination-ellipsis">…</span>';
            }
        }
        html += pageBtn(currentPage === totalPages, currentPage + 1,
            '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="9 18 15 12 9 6"/></svg>');
        paginationContainer.innerHTML = html;
    }

    function pageBtn(disabled, page, inner) {
        return '<button class="btn btn-sm btn-ghost" ' +
               (disabled ? 'disabled' : 'data-page="' + page + '"') +
               '>' + inner + '</button>';
    }

    if (paginationContainer) {
        paginationContainer.addEventListener('click', function (e) {
            var b = e.target.closest('button[data-page]');
            if (b && !b.disabled) {
                currentPage = parseInt(b.getAttribute('data-page'));
                renderPagination();
            }
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            searchTerm  = this.value.toLowerCase().trim();
            currentPage = 1;
            renderPagination();
        });
    }

    // ─── Rol Atama Modal ──────────────────────────────────────────────────────
    var rolesModal = document.getElementById('rolesModal');
    if (rolesModal) {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-assign-roles]');
            if (!btn) return;
            e.preventDefault();

            var itemId = btn.getAttribute('data-item-id');
            var row    = document.querySelector('tr[data-item-id="' + itemId + '"]');
            if (!row) return;

            // Atanmış rol ID'lerini oku
            var roleIds = [];
            try { roleIds = JSON.parse(row.getAttribute('data-role-ids') || '[]'); } catch (ex) {}

            // Menü bilgisini oku
            var menuData = {};
            try { menuData = JSON.parse(row.getAttribute('data-item') || '{}'); } catch (ex) {}

            // Hidden field
            var idField = document.getElementById('rolesModalMenuId');
            if (idField) idField.value = itemId;

            // Menü adı / URL
            var nameEl = document.getElementById('rolesModalName');
            var metaEl = document.getElementById('rolesModalMeta');
            if (nameEl) nameEl.textContent = menuData.Name || '—';
            if (metaEl) metaEl.textContent = menuData.Url  || 'URL yok';

            // Checkboxları güncelle
            rolesModal.querySelectorAll('.role-checkbox').forEach(function (cb) {
                cb.checked = roleIds.indexOf(parseInt(cb.value)) !== -1;
            });

            // Modalı aç
            if (window.AdminUI && window.AdminUI.Modal) {
                window.AdminUI.Modal.open(rolesModal);
            } else {
                rolesModal.classList.add('active');
                document.body.style.overflow = 'hidden';
            }
        });
    }

    // ─── Init ─────────────────────────────────────────────────────────────────
    initTree();
    renderPagination();

})();
