/**
 * users.js — Kullanıcı Yönetimi (sayfa-özgü)
 *   - Düzenleme modunda şifre alanını gizle
 *   - Rol atama modalini doldur
 */
(function () {
    'use strict';

    /* ── Şifre alanı görünürlüğü ───────────────────────────────────────────── */
    var userModal = document.getElementById('userModal');
    if (userModal) {
        function syncPasswordField() {
            var idInput       = userModal.querySelector('input[name="Id"]');
            var passwordGroup = document.getElementById('passwordGroup');
            var passwordInput = document.getElementById('passwordInput');
            if (!passwordGroup || !passwordInput) return;
            var isEdit = idInput && parseInt(idInput.value) > 0;
            passwordGroup.style.display = isEdit ? 'none' : '';
            passwordInput.required      = !isEdit;
        }
        new MutationObserver(syncPasswordField).observe(userModal, { attributes: true, attributeFilter: ['class'] });
        syncPasswordField();
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
            var userData = {}; try { userData = JSON.parse(row.getAttribute('data-item')     || '{}'); } catch (ex) {}
            var idField  = document.getElementById('rolesModalUserId');
            var nameEl   = document.getElementById('rolesModalName');
            var metaEl   = document.getElementById('rolesModalMeta');
            var badgeEl  = document.getElementById('rolesModalBadge');
            var fullName = ((userData.FirstName || '') + ' ' + (userData.LastName || '')).trim();
            if (idField)  idField.value      = itemId;
            if (nameEl)   nameEl.textContent = fullName || '—';
            if (metaEl)   metaEl.textContent = userData.Mail || '—';
            if (badgeEl)  badgeEl.textContent = (userData.FirstName || '?')[0].toUpperCase();
            rolesModal.querySelectorAll('.role-checkbox').forEach(function (cb) {
                cb.checked = roleIds.indexOf(parseInt(cb.value)) !== -1;
            });
            AdminUI.Modal.open(rolesModal);
        });
    }

})();
