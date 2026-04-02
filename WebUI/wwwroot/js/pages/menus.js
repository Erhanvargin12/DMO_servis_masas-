/**
 * menus.js — Menü Yönetimi (sayfa-özgü)
 *   - Menü ağacı açma/kapama + sessionStorage ile durum saklama
 *   - Client-side arama (tree yapısı nedeniyle)
 *   - Rol atama modalini doldurma
 */
(function () {
    'use strict';

    var searchInput  = document.getElementById('tableSearch');
    var tableBody    = document.querySelector('[data-paginate] tbody');
    var countBadge   = document.querySelector('.stat-chip');
    if (!tableBody) return;

    var allParentRows = Array.from(tableBody.querySelectorAll('.menu-row-parent'));
    var emptyRow      = tableBody.querySelector('.empty-row');

    /* ── Tree Toggle ───────────────────────────────────────────────────────── */
    function childRows(gid) {
        return Array.from(tableBody.querySelectorAll('tr[data-group="' + gid + '"]'));
    }

    function expand(gid, row, persist) {
        row.classList.add('expanded');
        childRows(gid).forEach(function (r) { r.style.display = ''; });
        if (persist !== false) saveTree();
    }

    function collapse(gid, row) {
        row.classList.remove('expanded');
        childRows(gid).forEach(function (r) { r.style.display = 'none'; });
        saveTree();
    }

    function saveTree() {
        var open = [];
        allParentRows.forEach(function (r) { if (r.classList.contains('expanded')) open.push(r.getAttribute('data-group-id')); });
        try { sessionStorage.setItem('menuTreeOpen', JSON.stringify(open)); } catch (e) {}
    }

    // Başlangıç: child satırları gizle, kaydedilen açık grupları geri yükle
    Array.from(tableBody.querySelectorAll('.menu-row-child')).forEach(function (r) { r.style.display = 'none'; });
    var saved = []; try { saved = JSON.parse(sessionStorage.getItem('menuTreeOpen') || '[]'); } catch (e) {}
    allParentRows.forEach(function (r) {
        var gid = r.getAttribute('data-group-id');
        if (gid && saved.includes(gid)) expand(gid, r, false);
    });

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.menu-tree-toggle');
        if (!btn) return;
        var gid = btn.getAttribute('data-group');
        var row = btn.closest('tr');
        if (!gid || !row) return;
        row.classList.contains('expanded') ? collapse(gid, row) : expand(gid, row, true);
    });

    /* ── Arama ─────────────────────────────────────────────────────────────── */
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            var term = this.value.toLowerCase().trim();
            var count = 0;
            allParentRows.forEach(function (row) {
                var d = {};
                try { d = JSON.parse(row.getAttribute('data-item') || '{}'); } catch (e) {}
                var match = !term || [d.Name, d.Description, d.Url].join(' ').toLowerCase().includes(term);
                row.style.display = match ? '' : 'none';
                var gid = row.getAttribute('data-group-id');
                if (gid) childRows(gid).forEach(function (r) {
                    r.style.display = (match && row.classList.contains('expanded')) ? '' : 'none';
                });
                if (match) count++;
            });
            if (emptyRow)   emptyRow.style.display  = count === 0 ? '' : 'none';
            if (countBadge) countBadge.textContent   = count + ' kayıt';
        });
    }

    /* ── Rol Atama Modalini Doldur ─────────────────────────────────────────── */
    var rolesModal = document.getElementById('rolesModal');
    if (rolesModal) {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-assign-roles]');
            if (!btn) return;
            e.preventDefault();
            var itemId   = btn.getAttribute('data-item-id');
            var row      = document.querySelector('tr[data-item-id="' + itemId + '"]');
            if (!row) return;
            var roleIds  = []; try { roleIds  = JSON.parse(row.getAttribute('data-role-ids') || '[]'); } catch (ex) {}
            var menuData = {}; try { menuData = JSON.parse(row.getAttribute('data-item')     || '{}'); } catch (ex) {}
            var idField  = document.getElementById('rolesModalMenuId');
            var nameEl   = document.getElementById('rolesModalName');
            var metaEl   = document.getElementById('rolesModalMeta');
            if (idField) idField.value      = itemId;
            if (nameEl)  nameEl.textContent = menuData.Name || '—';
            if (metaEl)  metaEl.textContent = menuData.Url  || 'URL yok';
            rolesModal.querySelectorAll('.role-checkbox').forEach(function (cb) {
                cb.checked = roleIds.indexOf(parseInt(cb.value)) !== -1;
            });
            AdminUI.Modal.open(rolesModal);
        });
    }

})();
