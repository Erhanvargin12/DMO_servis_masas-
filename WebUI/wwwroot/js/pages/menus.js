/**
 * Menus Page JavaScript
 * Menü yönetimi sayfası için JavaScript işlevleri
 */
(function() { 'use strict';

    // Constants
    var PAGE_SIZE = 10;
    var currentPage = 1;
    var currentMenuId = 0;
    
    // DOM Elements
    var searchInput = document.getElementById('tableSearch');
    var tableBody = document.querySelector('#menusTable tbody');
    var paginationInfo = document.getElementById('paginationInfo');
    var paginationContainer = document.getElementById('pagination');
    var countBadge = document.querySelector('.data-table-count');
    
    // Data
    var allRows = Array.from(tableBody.querySelectorAll('tr[data-item-id]'));
    var emptyRow = tableBody.querySelector('.empty-row');
    var filteredRows = allRows.slice();
    
    // Roles Modal Elements
    var rolesModal = document.getElementById('rolesModal');
    var roleMenuIcon = document.getElementById('roleMenuIcon');
    var roleMenuName = document.getElementById('roleMenuName');
    var roleCheckboxes = document.querySelectorAll('.role-checkbox');
    var saveRolesBtn = document.getElementById('saveRolesBtn');

    // Pagination render
    function renderPagination() {
        if (allRows.length === 0) return;
        
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
        if (paginationInfo) paginationInfo.textContent = showStart + '-' + showEnd + ' / ' + filteredRows.length + ' menü';
        
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

    // Filter rows based on search term
    function filterRows(term) {
        currentPage = 1;
        
        if (term === '') {
            filteredRows = allRows.slice();
        } else {
            filteredRows = allRows.filter(function(row) {
                var data = JSON.parse(row.getAttribute('data-item') || '{}');
                var searchText = [
                    data.Name || '',
                    data.Description || '',
                    data.Url || '',
                    data.Icon || ''
                ].join(' ').toLowerCase();
                return searchText.includes(term);
            });
        }
        
        renderPagination();
    }

    //Open roles modal
    function openRolesModal(menuId, menuName) {
        currentMenuId = menuId;
        
        roleMenuIcon.textContent = menuName.charAt(0).toUpperCase();
        roleMenuName.textContent = menuName;
        
        // Reset checkboxes
        roleCheckboxes.forEach(function(cb) { cb.checked = false; });
        
        // Fetch current roles
        fetch('/Menus/GetMenuRoles?menuId=' + menuId)
            .then(function(res) { return res.json(); })
            .then(function(result) {
                if (result.success && result.data) {
                    result.data.forEach(function(roleId) {
                        var cb = document.querySelector('.role-checkbox[value="' + roleId + '"]');
                        if (cb) cb.checked = true;
                    });
                }
            })
            .catch(function(err) { 
                console.error('Rol getirme hatası:', err); 
            });
        
        rolesModal.classList.add('active');
    }

    // Save roles
    function saveRoles() {
        var selectedRoles = [];
        roleCheckboxes.forEach(function(cb) {
            if (cb.checked) selectedRoles.push(parseInt(cb.value));
        });
        
        var token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        fetch('/Menus/SaveRoles', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ menuId: currentMenuId, roleIds: selectedRoles })
        })
        .then(function(res) { return res.json(); })
        .then(function(result) {
            if (result.success) {
                if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                    AdminUI.Toast.success(result.message || 'Menü rolleri güncellendi');
                }
                rolesModal.classList.remove('active');
            } else {
                if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                    AdminUI.Toast.error(result.message || 'Bir hata oluştu');
                }
            }
        })
        .catch(function(err) {
            console.error('Rol kaydetme hatası:', err);
            if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                AdminUI.Toast.error('Bağlantı hatası');
            }
        });
    }

    //Close modal
    function closeModal(modal) {
        modal.classList.remove('active');
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
    
    // Role modal triggers
    document.querySelectorAll('[data-role-modal]').forEach(function(btn) {
        btn.addEventListener('click', function() {
            var menuId = parseInt(this.getAttribute('data-role-modal'));
            var menuName = this.getAttribute('data-menu-name') || 'Menü';
            openRolesModal(menuId, menuName);
        });
    });
    
    // Save roles button
    if (saveRolesBtn)
    {
        saveRolesBtn.addEventListener('click', saveRoles);
    }
    
    // Close modals
    rolesModal.querySelectorAll('[data-modal-close]').forEach(function (btn)
    {
        btn.addEventListener('click', function ()
        {
            closeModal(rolesModal);
        });
    });
    
    rolesModal.addEventListener('click', function (e)
    {
        if (e.target === rolesModal)
        {
            closeModal(rolesModal);
        }
    });
    
    // Initialize
    renderPagination();
    
})();

