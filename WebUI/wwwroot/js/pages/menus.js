/**
 * Menus Page — Tree table + roles modal
 */
(function () {
    'use strict';

    var PAGE_SIZE = 10;
    var currentPage = 1;
    var currentMenuId = 0;
    var searchTerm = '';

    var searchInput        = document.getElementById('tableSearch');
    var tableBody          = document.querySelector('#menusTable tbody');
    var paginationInfo     = document.getElementById('paginationInfo');
    var paginationContainer= document.getElementById('pagination');
    var countBadge         = document.querySelector('.data-table-count');

    var rolesModal    = document.getElementById('rolesModal');
    var roleMenuIcon  = document.getElementById('roleMenuIcon');
    var roleMenuName  = document.getElementById('roleMenuName');
    var roleCheckboxes= document.querySelectorAll('.role-checkbox');
    var saveRolesBtn  = document.getElementById('saveRolesBtn');

    // Tüm parent ve child satırları
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
        // Tüm child satırları gizle
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

    // ─── Pagination (yalnızca parent satırlar sayılır) ────────────────────────
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

        // Tüm parent satırları gizle
        allParentRows.forEach(function (r) { r.style.display = 'none'; });
        // Tüm child satırları gizle (tree ile yönetilecek)
        Array.from(tableBody.querySelectorAll('.menu-row-child, .menu-row-orphan')).forEach(function (r) {
            r.style.display = 'none';
        });

        // Aktif sayfadaki parent'ları göster + child'larını duruma göre aç
        filtered.slice(start, end).forEach(function (parentRow) {
            parentRow.style.display = '';
            var gid = parentRow.getAttribute('data-group-id');
            if (gid && parentRow.classList.contains('expanded')) {
                getChildRows(gid).forEach(function (r) { r.style.display = ''; });
            }
        });

        // Orphan'lar her zaman görünür
        Array.from(tableBody.querySelectorAll('.menu-row-orphan')).forEach(function (r) {
            r.style.display = '';
        });

        if (emptyRow) emptyRow.style.display = filtered.length === 0 ? '' : 'none';

        var showStart = filtered.length > 0 ? start + 1 : 0;
        var showEnd   = Math.min(end, filtered.length);
        if (countBadge)    countBadge.textContent    = filtered.length + ' kayıt';
        if (paginationInfo) paginationInfo.textContent = showStart + '-' + showEnd + ' / ' + filtered.length + ' menü';

        if (!paginationContainer) return;
        if (totalPages <= 1) { paginationContainer.innerHTML = ''; return; }

        var html = '';
        html += btn(currentPage === 1, currentPage - 1, '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>');
        for (var i = 1; i <= totalPages; i++) {
            if (totalPages <= 7 || i === 1 || i === totalPages || (i >= currentPage - 1 && i <= currentPage + 1)) {
                html += '<button class="btn btn-sm ' + (i === currentPage ? 'btn-primary' : 'btn-ghost') + '" data-page="' + i + '">' + i + '</button>';
            } else if (i === currentPage - 2 || i === currentPage + 2) {
                html += '<span class="pagination-ellipsis">…</span>';
            }
        }
        html += btn(currentPage === totalPages, currentPage + 1, '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="9 18 15 12 9 6"/></svg>');
        paginationContainer.innerHTML = html;
    }

    function btn(disabled, page, inner) {
        return '<button class="btn btn-sm btn-ghost" ' + (disabled ? 'disabled' : 'data-page="' + page + '"') + '>' + inner + '</button>';
    }

    if (paginationContainer) {
        paginationContainer.addEventListener('click', function (e) {
            var b = e.target.closest('button[data-page]');
            if (b && !b.disabled) { currentPage = parseInt(b.getAttribute('data-page')); renderPagination(); }
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            searchTerm = this.value.toLowerCase().trim();
            currentPage = 1;
            renderPagination();
        });
    }

    // ─── Roles Modal ──────────────────────────────────────────────────────────
    function openRolesModal(menuId, menuName) {
        currentMenuId = menuId;
        if (roleMenuIcon) roleMenuIcon.textContent = menuName.charAt(0).toUpperCase();
        if (roleMenuName) roleMenuName.textContent  = menuName;
        roleCheckboxes.forEach(function (cb) { cb.checked = false; });

        fetch('/Menus/GetMenuRoles?menuId=' + menuId)
            .then(function (r) { return r.json(); })
            .then(function (result) {
                if (result.success && result.data) {
                    result.data.forEach(function (roleId) {
                        var cb = document.querySelector('.role-checkbox[value="' + roleId + '"]');
                        if (cb) cb.checked = true;
                    });
                }
            })
            .catch(function (err) { console.error('Rol getirme hatası:', err); });

        if (rolesModal) rolesModal.classList.add('active');
    }

    function saveRoles() {
        var selected = [];
        roleCheckboxes.forEach(function (cb) { if (cb.checked) selected.push(parseInt(cb.value)); });

        var token = document.querySelector('input[name="__RequestVerificationToken"]');

        fetch('/Menus/SaveRoles', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token ? token.value : '' },
            body: JSON.stringify({ menuId: currentMenuId, roleIds: selected })
        })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                if (window.AdminUI && AdminUI.Toast) AdminUI.Toast.success(result.message || 'Roller güncellendi');
                if (rolesModal) rolesModal.classList.remove('active');
            } else {
                if (window.AdminUI && AdminUI.Toast) AdminUI.Toast.error(result.message || 'Bir hata oluştu');
            }
        })
        .catch(function () {
            if (window.AdminUI && AdminUI.Toast) AdminUI.Toast.error('Bağlantı hatası');
        });
    }

    document.querySelectorAll('[data-role-modal]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            openRolesModal(parseInt(this.getAttribute('data-role-modal')), this.getAttribute('data-menu-name') || 'Menü');
        });
    });

    if (saveRolesBtn) saveRolesBtn.addEventListener('click', saveRoles);

    if (rolesModal) {
        rolesModal.querySelectorAll('[data-modal-close]').forEach(function (b) {
            b.addEventListener('click', function () { rolesModal.classList.remove('active'); });
        });
        rolesModal.addEventListener('click', function (e) {
            if (e.target === rolesModal) rolesModal.classList.remove('active');
        });
    }

    // ─── Init ─────────────────────────────────────────────────────────────────
    initTree();
    renderPagination();

})();
