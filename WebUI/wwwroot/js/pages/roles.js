/**
 * Roles Page JavaScript
 * Rol yönetimi sayfası için JavaScript işlevleri
 */
(function() {
    'use strict';

    // Constants
    var PAGE_SIZE = 8;
    var currentPage = 1;
    
    // DOM Elements
    var searchInput = document.getElementById('tableSearch');
    var tableBody = document.querySelector('#rolesTable tbody');
    var countBadge = document.querySelector('.data-table-count');
    var paginationInfo = document.getElementById('paginationInfo');
    var paginationContainer = document.getElementById('pagination');
    
    // Data
    var allRows = Array.from(tableBody.querySelectorAll('tr[data-item-id]'));
    var emptyRow = tableBody.querySelector('tr:not([data-item-id])');
    var filteredRows = allRows.slice();
    
    // Exit if no data
    if (allRows.length === 0) return;

    /**
     * Render pagination
     */
    function renderPagination() {
        var totalPages = Math.ceil(filteredRows.length / PAGE_SIZE);
        var start = (currentPage - 1) * PAGE_SIZE;
        var end = start + PAGE_SIZE;
        
        // Hide all rows
        allRows.forEach(function(row) { row.style.display = 'none'; });
        
        // Show filtered rows for current page
        filteredRows.slice(start, end).forEach(function(row) { row.style.display = ''; });
        
        // Empty state
        if (emptyRow) {
            emptyRow.style.display = filteredRows.length === 0 ? '' : 'none';
        }
        
        // Info text
        var showStart = filteredRows.length > 0 ? start + 1 : 0;
        var showEnd = Math.min(end, filteredRows.length);
        if (countBadge) countBadge.textContent = filteredRows.length + ' kayıt';
        if (paginationInfo) paginationInfo.textContent = showStart + '-' + showEnd + ' / ' + filteredRows.length + ' rol';
        
        // No pagination needed
        if (totalPages <= 1) {
            paginationContainer.innerHTML = '';
            return;
        }
        
        // Build pagination HTML
        var html = '';
        
        // Previous button
        html += '<button type="button" class="btn btn-sm btn-ghost" ' + (currentPage === 1 ? 'disabled' : '') + ' data-page="' + (currentPage - 1) + '">';
        html += '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>';
        html += '</button>';
        
        // Page numbers
        for (var i = 1; i <= totalPages; i++) {
            if (totalPages <= 7 || i === 1 || i === totalPages || (i >= currentPage - 1 && i <= currentPage + 1)) {
                html += '<button type="button" class="btn btn-sm ' + (i === currentPage ? 'btn-primary' : 'btn-ghost') + '" data-page="' + i + '">' + i + '</button>';
            } else if (i === currentPage - 2 || i === currentPage + 2) {
                html += '<span class="pagination-ellipsis">...</span>';
            }
        }
        
        // Next button
        html += '<button type="button" class="btn btn-sm btn-ghost" ' + (currentPage === totalPages ? 'disabled' : '') + ' data-page="' + (currentPage + 1) + '">';
        html += '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="9 18 15 12 9 6"/></svg>';
        html += '</button>';
        
        paginationContainer.innerHTML = html;
    }

    /**
     * Filter rows based on search term
     */
    function filterRows(term) {
        currentPage = 1;
        
        if (term === '') {
            filteredRows = allRows.slice();
        } else {
            filteredRows = allRows.filter(function(row) {
                var data = JSON.parse(row.getAttribute('data-item') || '{}');
                var searchText = [
                    data.Name || '',
                    data.Description || ''
                ].join(' ').toLowerCase();
                return searchText.includes(term);
            });
        }
        
        renderPagination();
    }

    // Event Listeners
    
    // Search
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            filterRows(this.value.toLowerCase().trim());
        });
    }
    
    // Pagination clicks
    if (paginationContainer) {
        paginationContainer.addEventListener('click', function(e) {
            var btn = e.target.closest('button[data-page]');
            if (btn && !btn.disabled) {
                currentPage = parseInt(btn.getAttribute('data-page'));
                renderPagination();
            }
        });
    }
    
    // Initialize
    renderPagination();
    
})();

