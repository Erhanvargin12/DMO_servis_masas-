/**
 * talepler.js - Talep Yönetim Modülü
 */

(function () {
    'use strict';

    // ── Modal Yönetimi ────────────────────────────────────────────────────────

    function openModal(id) {
        const overlay = document.getElementById(id);
        if (!overlay) return;
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
        // İlk focuslanabilir elemana odaklan
        const first = overlay.querySelector('input:not([type=hidden]), select, textarea, button:not(.modal-close)');
        if (first) setTimeout(() => first.focus(), 50);
    }

    function closeModal(overlay) {
        if (!overlay) return;
        overlay.classList.remove('active');
        document.body.style.overflow = '';
    }

    function closeAllModals() {
        document.querySelectorAll('.modal-overlay.active').forEach(closeModal);
    }

    // data-modal-open="modalId" butonları
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('[data-modal-open]');
        if (btn) {
            e.preventDefault();
            const modalId = btn.dataset.modalOpen;
            openModal(modalId);

            // ── Düzenle modalı için dinamik doldurma ─────────────────────────
            if (modalId === 'duzenleModal') {
                document.getElementById('duzenleId').value         = btn.dataset.talepId    || '';
                document.getElementById('duzenleBaslik').value     = btn.dataset.baslik      || '';
                document.getElementById('duzenleAciklama').value   = btn.dataset.aciklama    || '';
                const kategoriSel = document.getElementById('duzenleKategori');
                if (kategoriSel) kategoriSel.value                 = btn.dataset.kategoriId  || '';
                const oncelikSel = document.getElementById('duzenleOncelik');
                if (oncelikSel) oncelikSel.value                   = btn.dataset.oncelik     || '2';
            }

            // ── Sil modalı için dinamik doldurma ─────────────────────────────
            if (modalId === 'silModal') {
                const silId     = document.getElementById('silTalepId');
                const silBaslik = document.getElementById('silBaslik');
                if (silId)     silId.value      = btn.dataset.talepId || '';
                if (silBaslik) silBaslik.textContent = btn.dataset.baslik  || '';
            }
        }

        // Kapat: data-modal-close veya overlay'e tıklama
        const closeBtn = e.target.closest('[data-modal-close]');
        if (closeBtn) {
            e.preventDefault();
            const overlay = closeBtn.closest('.modal-overlay');
            closeModal(overlay);
            return;
        }

        if (e.target.classList.contains('modal-overlay')) {
            closeModal(e.target);
        }
    });

    // ESC ile kapat
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeAllModals();
    });

    // ── Chat: Otomatik Kaydırma ────────────────────────────────────────────

    const chatContainer = document.getElementById('chatContainer');
    if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }

    // ── Chat: Enter ile Gönder ────────────────────────────────────────────

    const chatInput = document.querySelector('.chat-input-row input[name="mesaj"]');
    if (chatInput) {
        chatInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.closest('form').submit();
            }
        });
    }

    // ── Alert: Otomatik Kapat (4sn) ───────────────────────────────────────

    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            alert.style.transition = 'opacity 0.4s';
            alert.style.opacity    = '0';
            setTimeout(function () { alert.remove(); }, 400);
        }, 4000);
    });

    // ── Dosya Input: Seçilen dosya adı göster ────────────────────────────

    document.querySelectorAll('input[type="file"]').forEach(function (input) {
        input.addEventListener('change', function () {
            const count = this.files.length;
            if (count > 0) {
                const label = this.nextElementSibling;
                if (label && label.tagName === 'SPAN') {
                    label.textContent = count === 1
                        ? this.files[0].name
                        : count + ' dosya seçildi';
                }
            }
        });
    });

}());
