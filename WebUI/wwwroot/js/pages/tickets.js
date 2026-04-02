/**
 * tickets.js — Ticket modülü (ticket-specific kod)
 *
 * Global olanlar (app.js içinde):
 *   Modal aç/kapat  → AdminUI.Modal.open / .close  + data-modal-open / data-modal-close attribute'ları
 *   Dosya drop zone → AdminUI.FileDropZone.initAll  + data-file-drop / data-file-list attribute'ları
 *   Tıklanabilir satır → tr[data-href]
 *
 * Buradaki sorumluluk:
 *   - Filtre form'larında select değişince otomatik submit
 */
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var filterForm = document.getElementById('ticketFilterForm');
        if (!filterForm) return;

        // Status / priority dropdown değişince formu otomatik gönder
        ['statusFilter', 'priorityFilter'].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.addEventListener('change', function () { filterForm.submit(); });
        });

        // Enter ile arama
        var searchInput = document.getElementById('ticketSearch');
        if (searchInput) {
            searchInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') { e.preventDefault(); filterForm.submit(); }
            });
        }
    });

}());
