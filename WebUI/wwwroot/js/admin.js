(function() {
    'use strict';

    // DOM Ready
    document.addEventListener('DOMContentLoaded', function() {
        initTheme();
        initSidebar();
        initProfileDropdown();
        highlightActiveMenu();
    });

    // Theme Toggle
    function initTheme() {
        var savedTheme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);
        
        var themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', function() {
                var current = document.documentElement.getAttribute('data-theme');
                var next = current === 'dark' ? 'light' : 'dark';
                document.documentElement.setAttribute('data-theme', next);
                localStorage.setItem('theme', next);
            });
        }
    }

    // Sidebar Toggle
    function initSidebar() {
        var sidebar = document.getElementById('sidebar');
        var toggle = document.getElementById('sidebarToggle');
        var overlay = document.getElementById('mobileOverlay');
        
        if (!sidebar || !toggle) return;

        // Desktop: collapsed state
        var isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (isCollapsed && window.innerWidth > 768) {
            sidebar.classList.add('collapsed');
        }

        toggle.addEventListener('click', function() {
            if (window.innerWidth <= 768) {
                // Mobile
                sidebar.classList.toggle('open');
                if (overlay) overlay.classList.toggle('active');
            } else {
                // Desktop
                sidebar.classList.toggle('collapsed');
                localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
            }
        });

        // Overlay click
        if (overlay) {
            overlay.addEventListener('click', function() {
                sidebar.classList.remove('open');
                overlay.classList.remove('active');
            });
        }

        // Resize handler
        window.addEventListener('resize', function() {
            if (window.innerWidth > 768) {
                sidebar.classList.remove('open');
                if (overlay) overlay.classList.remove('active');
            }
        });
    }

    // Profile Dropdown
    function initProfileDropdown() {
        var dropdown = document.getElementById('profileDropdown');
        var toggle = document.getElementById('profileToggle');
        
        if (!dropdown || !toggle) return;

        toggle.addEventListener('click', function(e) {
            e.stopPropagation();
            dropdown.classList.toggle('open');
        });

        // Close on outside click
        document.addEventListener('click', function(e) {
            if (!dropdown.contains(e.target)) {
                dropdown.classList.remove('open');
            }
        });

        // Close on escape key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                dropdown.classList.remove('open');
            }
        });
    }

    // Highlight Active Menu
    function highlightActiveMenu() {
        var path = window.location.pathname.toLowerCase();
        var links = document.querySelectorAll('.menu-link');
        
        links.forEach(function(link) {
            var href = link.getAttribute('href');
            if (!href) return;
            
            href = href.toLowerCase();
            
            if (path === href || (href !== '/' && path.indexOf(href) === 0)) {
                link.classList.add('active');
            }
        });
    }

})();
