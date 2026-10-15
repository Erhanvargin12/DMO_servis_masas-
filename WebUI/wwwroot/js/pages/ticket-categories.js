/**
 * ticket-categories.js — Problem Kategorileri Yönetimi
 */
(function () {
    'use strict';

    var branchesModal = document.getElementById('branchesModal');
    if (!branchesModal) return;

    var routeOrderEl = document.getElementById('branchRouteOrder');
    var routeEmptyEl = document.getElementById('branchRouteEmpty');
    var orderedInput = document.getElementById('orderedBranchIds');
    var routeItems = [];

    function syncOrderedInput() {
        if (orderedInput) {
            orderedInput.value = routeItems.map(function (x) { return x.id; }).join(',');
        }
    }

    function renderRouteOrder() {
        if (!routeOrderEl) return;

        routeOrderEl.querySelectorAll('.branch-route-item').forEach(function (el) { el.remove(); });

        if (routeEmptyEl) {
            routeEmptyEl.style.display = routeItems.length === 0 ? '' : 'none';
        }

        routeItems.forEach(function (item, index) {
            var row = document.createElement('div');
            row.className = 'role-item branch-route-item';
            row.innerHTML =
                '<div class="role-info" style="flex:1;">' +
                    '<span class="role-name">' + (index + 1) + '. ' + item.name + '</span>' +
                    '<span class="role-desc">Sıradaki sorumlu şube</span>' +
                '</div>' +
                '<div class="table-actions">' +
                    '<button type="button" class="btn btn-icon btn-ghost btn-sm branch-move-up" title="Yukarı"><i class="fa-solid fa-arrow-up"></i></button>' +
                    '<button type="button" class="btn btn-icon btn-ghost btn-sm branch-move-down" title="Aşağı"><i class="fa-solid fa-arrow-down"></i></button>' +
                    '<button type="button" class="btn btn-icon btn-ghost btn-sm btn-icon-danger branch-remove" title="Kaldır"><i class="fa-solid fa-xmark"></i></button>' +
                '</div>';
            routeOrderEl.appendChild(row);
        });

        syncOrderedInput();
    }

    function addRouteItem(id, name) {
        if (routeItems.some(function (x) { return x.id === id; })) return;
        routeItems.push({ id: id, name: name });
        renderRouteOrder();
    }

    function removeRouteItem(id) {
        routeItems = routeItems.filter(function (x) { return x.id !== id; });
        renderRouteOrder();
    }

    function loadRouteFromIds(ids) {
        routeItems = [];
        ids.forEach(function (id) {
            var picker = branchesModal.querySelector('.branch-picker-item[data-branch-id="' + id + '"]');
            if (picker) {
                addRouteItem(id, picker.getAttribute('data-branch-name') || ('Şube #' + id));
            }
        });
    }

    branchesModal.addEventListener('change', function (e) {
        var checkbox = e.target.closest('.branch-checkbox');
        if (!checkbox) return;

        var picker = checkbox.closest('.branch-picker-item');
        if (!picker) return;

        var id = parseInt(checkbox.value, 10);
        var name = picker.getAttribute('data-branch-name') || ('Şube #' + id);

        if (checkbox.checked) {
            addRouteItem(id, name);
        } else {
            removeRouteItem(id);
        }
    });

    branchesModal.addEventListener('click', function (e) {
        var up = e.target.closest('.branch-move-up');
        var down = e.target.closest('.branch-move-down');
        var remove = e.target.closest('.branch-remove');
        if (!up && !down && !remove) return;

        var row = e.target.closest('.branch-route-item');
        if (!row) return;

        var index = Array.from(routeOrderEl.querySelectorAll('.branch-route-item')).indexOf(row);
        if (index < 0) return;

        if (up && index > 0) {
            var tmp = routeItems[index - 1];
            routeItems[index - 1] = routeItems[index];
            routeItems[index] = tmp;
            renderRouteOrder();
        }

        if (down && index < routeItems.length - 1) {
            var tmp2 = routeItems[index + 1];
            routeItems[index + 1] = routeItems[index];
            routeItems[index] = tmp2;
            renderRouteOrder();
        }

        if (remove) {
            var removed = routeItems[index];
            removeRouteItem(removed.id);
            var checkbox = branchesModal.querySelector('.branch-checkbox[value="' + removed.id + '"]');
            if (checkbox) checkbox.checked = false;
        }
    });

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-assign-branches]');
        if (!btn) return;

        e.preventDefault();

        var itemId = btn.getAttribute('data-item-id');
        var row = document.querySelector('tr[data-item-id="' + itemId + '"]');
        if (!row) return;

        var branchIds = [];
        var categoryData = {};

        try { branchIds = JSON.parse(row.getAttribute('data-branch-ids') || '[]'); } catch (ex) {}
        try { categoryData = JSON.parse(row.getAttribute('data-item') || '{}'); } catch (ex) {}

        var idField = document.getElementById('branchesModalCategoryId');
        var nameEl = document.getElementById('branchesModalName');
        var metaEl = document.getElementById('branchesModalMeta');

        if (idField) idField.value = itemId;
        if (nameEl) nameEl.textContent = categoryData.Name || '—';
        if (metaEl) metaEl.textContent = categoryData.Description || 'Açıklama yok';

        branchesModal.querySelectorAll('.branch-checkbox').forEach(function (cb) {
            cb.checked = false;
        });

        routeItems = [];
        loadRouteFromIds(branchIds);

        branchesModal.querySelectorAll('.branch-checkbox').forEach(function (cb) {
            var id = parseInt(cb.value, 10);
            cb.checked = branchIds.indexOf(id) !== -1;
        });

        AdminUI.Modal.open(branchesModal);
    });
})();
