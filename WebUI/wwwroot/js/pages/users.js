/**
 * Users Page — Kullanıcı Yönetimi
 *
 * Sorumluluk:
 *   - Edit modal için şifre alanı görünürlük kontrolü
 *   - Rol atama modal açma/kapatma ve veri doldurma
 */
(function () {
    'use strict';

    // ─── Şifre alanı: yeni kullanıcıda zorunlu, düzenlemede gizli ────────────
    var userModal = document.getElementById('userModal');
    if (userModal) {
        function updatePasswordField() {
            var idInput       = userModal.querySelector('input[name="Id"]');
            var passwordGroup = document.getElementById('passwordGroup');
            var passwordInput = document.getElementById('passwordInput');
            if (!passwordGroup || !passwordInput) return;
            var isEdit = idInput && parseInt(idInput.value) > 0;
            passwordGroup.style.display = isEdit ? 'none' : '';
            passwordInput.required      = !isEdit;
        }
        new MutationObserver(updatePasswordField)
            .observe(userModal, { attributes: true, attributeFilter: ['class'] });
        updatePasswordField();
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

            // Kullanıcı bilgisini oku
            var userData = {};
            try { userData = JSON.parse(row.getAttribute('data-item') || '{}'); } catch (ex) {}

            // Hidden field
            var idField = document.getElementById('rolesModalUserId');
            if (idField) idField.value = itemId;

            // Kullanıcı adı / e-posta
            var nameEl  = document.getElementById('rolesModalName');
            var metaEl  = document.getElementById('rolesModalMeta');
            var badgeEl = document.getElementById('rolesModalBadge');
            var fullName = ((userData.FirstName || '') + ' ' + (userData.LastName || '')).trim();
            if (nameEl)  nameEl.textContent  = fullName || '—';
            if (metaEl)  metaEl.textContent  = userData.Mail || '—';
            if (badgeEl) badgeEl.textContent = (userData.FirstName || '?')[0].toUpperCase();

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

})();
