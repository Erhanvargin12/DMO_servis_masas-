/**
 * Admin UI Kit - Modular JavaScript Framework
 * Toast, Modal, Form, Theme, Sidebar management
 */
(function() {
    'use strict';

    // ===== Toast Notification System =====
    const Toast = {
        container: null,

        init: function() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.className = 'toast-container';
                this.container.id = 'toastContainer';
                document.body.appendChild(this.container);
            }
        },

        show: function(options) {
            this.init();

            const defaults = {
                type: 'info',
                title: '',
                message: '',
                duration: 4000
            };

            const opts = { ...defaults, ...options };
            
            const icons = {
                success: '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 6L9 17l-5-5"/></svg>',
                error: '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>',
                warning: '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>',
                info: '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'
            };

            const titles = {
                success: 'Başarılı',
                error: 'Hata',
                warning: 'Uyarı',
                info: 'Bilgi'
            };

            const toast = document.createElement('div');
            toast.className = 'toast toast-' + opts.type;
            toast.innerHTML = 
                '<div class="toast-icon">' + icons[opts.type] + '</div>' +
                '<div class="toast-content">' +
                    '<div class="toast-title">' + (opts.title || titles[opts.type]) + '</div>' +
                    (opts.message ? '<div class="toast-message">' + opts.message + '</div>' : '') +
                '</div>' +
                '<button type="button" class="toast-close">' +
                    '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>' +
                '</button>';

            this.container.appendChild(toast);

            // Close button
            toast.querySelector('.toast-close').addEventListener('click', function() {
                Toast.remove(toast);
            });

            // Auto remove
            if (opts.duration > 0) {
                setTimeout(function() {
                    Toast.remove(toast);
                }, opts.duration);
            }

            return toast;
        },

        remove: function(toast) {
            if (!toast || toast.classList.contains('removing')) return;
            toast.classList.add('removing');
            setTimeout(function() {
                if (toast.parentNode) toast.parentNode.removeChild(toast);
            }, 300);
        },

        success: function(message, title) {
            return this.show({ type: 'success', message: message, title: title });
        },

        error: function(message, title) {
            return this.show({ type: 'error', message: message, title: title });
        },

        warning: function(message, title) {
            return this.show({ type: 'warning', message: message, title: title });
        },

        info: function(message, title) {
            return this.show({ type: 'info', message: message, title: title });
        }
    };

    // ===== Modal System =====
    const Modal = {
        open: function(modalId) {
            var modal = typeof modalId === 'string' ? document.getElementById(modalId) : modalId;
            if (!modal) return;
            
            modal.classList.add('active');
            document.body.style.overflow = 'hidden';
            
            setTimeout(function() {
                var input = modal.querySelector('input:not([type="hidden"]):not([type="checkbox"])');
                if (input) input.focus();
            }, 100);
        },

        close: function(modal) {
            if (typeof modal === 'string') modal = document.getElementById(modal);
            if (!modal) return;
            
            modal.classList.remove('active');
            document.body.style.overflow = '';
        },

        openDelete: function(modalId, itemId, itemName) {
            var modal = document.getElementById(modalId);
            if (!modal) return;
            
            var idInput = document.getElementById(modalId + 'ItemId');
            var nameEl = document.getElementById(modalId + 'ItemName');
            
            if (idInput) idInput.value = itemId;
            if (nameEl) nameEl.textContent = itemName || '';
            
            this.open(modal);
        },

        reset: function(modal) {
            var form = modal.querySelector('form');
            if (!form) return;
            
            form.reset();
            
            var idInput = form.querySelector('input[name="Id"]');
            if (idInput) idInput.value = '0';
            
            var title = modal.querySelector('.modal-title');
            var submitBtn = modal.querySelector('button[type="submit"]');
            
            if (title && title.dataset.createTitle) {
                title.textContent = title.dataset.createTitle;
            }
            if (submitBtn && submitBtn.dataset.createText) {
                submitBtn.textContent = submitBtn.dataset.createText;
            }
        },

        loadData: function(modal, itemId) {
            var row = document.querySelector('tr[data-item-id="' + itemId + '"]');
            if (!row) {
                console.warn('Row not found for id:', itemId);
                return;
            }
            
            var form = modal.querySelector('form');
            if (!form) return;

            try {
                var rawData = row.getAttribute('data-item');
                var data = JSON.parse(rawData || '{}');
                
                var inputs = form.querySelectorAll('input, select, textarea');
                inputs.forEach(function(input) {
                    var name = input.name;
                    if (!name || name === '__RequestVerificationToken') return;
                    
                    var value = data[name];
                    
                    if (input.type === 'checkbox') {
                        input.checked = value === true || value === 'True' || value === 'true';
                    } else if (value !== null && value !== undefined) {
                        input.value = value;
                    }
                });
            } catch (e) {
                console.error('Parse error:', e);
            }

            var title = modal.querySelector('.modal-title');
            var submitBtn = modal.querySelector('button[type="submit"]');
            
            if (title && title.dataset.editTitle) {
                title.textContent = title.dataset.editTitle;
            }
            if (submitBtn && submitBtn.dataset.editText) {
                submitBtn.textContent = submitBtn.dataset.editText;
            }
        }
    };

    // ===== Form Handler =====
    const Form = {
        submit: function(form, options) {
            var defaults = {
                onSuccess: null,
                onError: null,
                resetOnSuccess: true
            };
            var opts = { ...defaults, ...options };
            
            var submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.classList.add('btn-loading');
            }

            var formData = new FormData(form);
            var data = {};
            
            formData.forEach(function(value, key) {
                if (key !== '__RequestVerificationToken') {
                    data[key] = value;
                }
            });

            // Handle checkboxes
            form.querySelectorAll('input[type="checkbox"]').forEach(function(cb) {
                data[cb.name] = cb.checked;
            });

            // Convert numeric fields
            ['Id', 'RegistirationId', 'OrganizationUnitId', 'PositionId'].forEach(function(field) {
                if (data[field] !== undefined) {
                    data[field] = parseInt(data[field]) || 0;
                }
            });

            var token = form.querySelector('input[name="__RequestVerificationToken"]');

            fetch(form.action, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token ? token.value : ''
                },
                body: JSON.stringify(data)
            })
            .then(function(res) { return res.json(); })
            .then(function(result) {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.classList.remove('btn-loading');
                }
                
                if (result.success) {
                    Toast.success(result.message || 'İşlem başarıyla tamamlandı');
                    
                    var modal = form.closest('.modal-overlay');
                    if (modal) Modal.close(modal);
                    
                    if (opts.onSuccess) {
                        opts.onSuccess(result);
                    } else {
                        setTimeout(function() { location.reload(); }, 800);
                    }
                } else {
                    Toast.error(result.message || 'Bir hata oluştu');
                    if (opts.onError) opts.onError(result);
                }
            })
            .catch(function(err) {
                console.error('Form submit error:', err);
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.classList.remove('btn-loading');
                }
                Toast.error('Bağlantı hatası oluştu');
                if (opts.onError) opts.onError(err);
            });
        }
    };

    // ===== Theme Manager =====
    const Theme = {
        defaults: {
            theme: 'light',
            accentColor: '#0ea5e9',     // default preset (teal/soft)
            sidebarColor: '#0f172a',
            bgColor: '#f8fafc',
            cardColor: '#ffffff',
            preset: 'default',
            useCustomColors: false,
            fontSize: 14,
            borderRadius: 6,
            lineHeight: '1.5',
            compactMode: false,
            compactTables: false,
            zebraStripes: false,
            showAnimations: true,
            highContrast: false,
            largeText: false
        },

        // Aynı paletler (light/dark varyant) - settings.js ile uyumlu
        colorPresets: {
            default: {
                accent: '#0ea5e9',
                sidebar: '#0f172a',
                bg: '#f8fafc',
                card: '#ffffff',
                accentDark: '#38bdf8',
                sidebarDark: '#0b1220',
                bgDark: '#0f172a',
                cardDark: '#111827'
            },
            aurora: {
                accent: '#8b5cf6',
                sidebar: '#0b1021',
                bg: '#f5f3ff',
                card: '#ffffff',
                accentDark: '#a78bfa',
                sidebarDark: '#0b1021',
                bgDark: '#0f172a',
                cardDark: '#0f172a'
            },
            emerald: {
                accent: '#22c55e',
                sidebar: '#0f1f17',
                bg: '#f1f5f3',
                card: '#ffffff',
                accentDark: '#34d399',
                sidebarDark: '#0c1510',
                bgDark: '#0f1f17',
                cardDark: '#0f1f17'
            },
            sunrise: {
                accent: '#f97316',
                sidebar: '#31102b',
                bg: '#fff7ed',
                card: '#ffffff',
                accentDark: '#fb923c',
                sidebarDark: '#2a0d24',
                bgDark: '#1b0a16',
                cardDark: '#1f0d1a'
            },
            ocean: {
                accent: '#2563eb',
                sidebar: '#0b1a2e',
                bg: '#eaf2ff',
                card: '#ffffff',
                accentDark: '#3b82f6',
                sidebarDark: '#0a1626',
                bgDark: '#0b1a2e',
                cardDark: '#0f2135'
            },
            mono: {
                accent: '#6b7280',
                sidebar: '#1f2937',
                bg: '#f6f7f9',
                card: '#ffffff',
                accentDark: '#9ca3af',
                sidebarDark: '#111827',
                bgDark: '#0f172a',
                cardDark: '#0f172a'
            }
        },

        loadSettings() {
            try {
                const stored = JSON.parse(localStorage.getItem('appSettings') || '{}');
                const settings = { ...this.defaults, ...stored };

                // Geçersiz preset ise default'a dön
                if (settings.preset && !this.colorPresets[settings.preset]) {
                    settings.preset = 'default';
                }

                // Zorunlu alanlar eksikse defaults ile tamamla
                ['accentColor', 'sidebarColor', 'bgColor', 'cardColor', 'theme'].forEach(k => {
                    if (!settings[k]) settings[k] = this.defaults[k];
                });

                return settings;
            } catch {
                return { ...this.defaults };
            }
        },

        saveSettings(settings) {
            localStorage.setItem('appSettings', JSON.stringify(settings));
        },

        applyGlobal(settings) {
            const root = document.documentElement;
            const isDark = settings.theme === 'dark' || (settings.theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches);

            // Tema
            root.removeAttribute('data-theme');
            if (isDark) root.setAttribute('data-theme', 'dark');

            // Renkler: preset + dark varyant (custom ise verilen renkler)
            let accent = settings.accentColor;
            let sidebar = settings.sidebarColor;
            let bg = settings.bgColor;
            let card = settings.cardColor;

            const presetName = settings.preset || 'default';
            const preset = (!settings.useCustomColors && this.colorPresets[presetName]) ? this.colorPresets[presetName] : null;

            if (preset && !settings.useCustomColors) {
                accent = isDark ? (preset.accentDark || preset.accent) : preset.accent;
                sidebar = isDark ? (preset.sidebarDark || preset.sidebar) : preset.sidebar;
                bg = isDark ? (preset.bgDark || preset.bg) : preset.bg;
                card = isDark ? (preset.cardDark || preset.card) : preset.card;
            }

            root.style.setProperty('--accent', accent);
            root.style.setProperty('--bg-sidebar', sidebar);
            root.style.setProperty('--bg-body', bg);
            root.style.setProperty('--bg-card', card);

            // Font ve layout
            root.style.fontSize = (settings.fontSize || 14) + 'px';
            root.style.setProperty('--line-height', settings.lineHeight || '1.5');
            root.style.setProperty('--radius', (settings.borderRadius || 6) + 'px');
            root.style.setProperty('--radius-sm', Math.max((settings.borderRadius || 6) - 2, 0) + 'px');
            root.style.setProperty('--radius-lg', ((settings.borderRadius || 6) + 2) + 'px');
            root.style.setProperty('--radius-xl', ((settings.borderRadius || 6) + 6) + 'px');

            // Animasyonlar
            if (!settings.showAnimations) {
                root.style.setProperty('--transition', '0s');
            } else {
                root.style.setProperty('--transition', '0.15s ease');
            }

            // Compact sınıfları
            document.body.classList.toggle('compact-mode', !!settings.compactMode);
            document.body.classList.toggle('compact-tables', !!settings.compactTables);
            document.body.classList.toggle('zebra-stripes', !!settings.zebraStripes);
            document.body.classList.toggle('high-contrast', !!settings.highContrast);
            document.body.classList.toggle('large-text', !!settings.largeText);
        },

        init: function() {
            const settings = this.loadSettings();
            this.applyGlobal(settings);
        },

        toggle: function() {
            const settings = this.loadSettings();
            const currentTheme = settings.theme || 'light';
            const next = currentTheme === 'dark' ? 'light' : 'dark';
            settings.theme = next;
            this.saveSettings(settings);
            this.applyGlobal(settings);
            return next;
        },

        set: function(theme) {
            const settings = this.loadSettings();
            settings.theme = theme;
            this.saveSettings(settings);
            this.applyGlobal(settings);
        }
    };

    // ===== Sidebar Manager =====
    const Sidebar = {
        init: function() {
            var sidebar = document.getElementById('sidebar');
            if (!sidebar) return;

            if (localStorage.getItem('sidebarCollapsed') === 'true' && window.innerWidth > 768) {
                sidebar.classList.add('collapsed');
            }

            this.highlightActive();
        },

        toggle: function() {
            var sidebar = document.getElementById('sidebar');
            var overlay = document.getElementById('mobileOverlay');
            if (!sidebar) return;

            if (window.innerWidth <= 768) {
                sidebar.classList.toggle('open');
                if (overlay) overlay.classList.toggle('active');
            } else {
                sidebar.classList.toggle('collapsed');
                localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
            }
        },

        highlightActive: function() {
            var path = window.location.pathname.toLowerCase();
            document.querySelectorAll('.menu-link').forEach(function(link) {
                var href = (link.getAttribute('href') || '').toLowerCase();
                if (path === href || (href !== '/' && path.indexOf(href) === 0)) {
                    link.classList.add('active');
                }
            });
        }
    };

    // ===== Profile Dropdown =====
    const ProfileDropdown = {
        init: function() {
            var dropdown = document.getElementById('profileDropdown');
            var toggle = document.getElementById('profileToggle');
            if (!dropdown || !toggle) return;

            toggle.addEventListener('click', function(e) {
                e.stopPropagation();
                dropdown.classList.toggle('open');
            });

            document.addEventListener('click', function(e) {
                if (!dropdown.contains(e.target)) {
                    dropdown.classList.remove('open');
                }
            });
        }
    };

    // ===== Initialize =====
    function init() {
        Theme.init();
        Sidebar.init();
        ProfileDropdown.init();
        
        // Sidebar toggle
        var sidebarToggle = document.getElementById('sidebarToggle');
        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function() {
                Sidebar.toggle();
            });
        }

        // Theme toggle
        var themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', function() {
                Theme.toggle();
            });
        }

        // Mobile overlay
        var mobileOverlay = document.getElementById('mobileOverlay');
        if (mobileOverlay) {
            mobileOverlay.addEventListener('click', function() {
                var sidebar = document.getElementById('sidebar');
                if (sidebar) sidebar.classList.remove('open');
                mobileOverlay.classList.remove('active');
            });
        }

        // Modal open buttons
        document.addEventListener('click', function(e) {
            var btn = e.target.closest('[data-modal-open]');
            if (btn) {
                e.preventDefault();
                var modalId = btn.getAttribute('data-modal-open');
                var modal = document.getElementById(modalId);
                if (modal) {
                    var itemId = btn.getAttribute('data-item-id');
                    if (itemId) {
                        Modal.loadData(modal, itemId);
                    } else {
                        Modal.reset(modal);
                    }
                    Modal.open(modal);
                }
            }
        });

        // Delete buttons
        document.addEventListener('click', function(e) {
            var btn = e.target.closest('[data-delete]');
            if (btn) {
                e.preventDefault();
                var id = btn.getAttribute('data-delete');
                var name = btn.getAttribute('data-delete-name') || '';
                var modalId = btn.getAttribute('data-delete-modal') || 'deleteModal';
                Modal.openDelete(modalId, id, name);
            }
        });

        // Modal close buttons and overlay click
        document.addEventListener('click', function(e) {
            if (e.target.closest('[data-modal-close]')) {
                var modal = e.target.closest('.modal-overlay');
                if (modal) Modal.close(modal);
            }
            if (e.target.classList.contains('modal-overlay')) {
                Modal.close(e.target);
            }
        });

        // ESC key closes modal
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                var active = document.querySelector('.modal-overlay.active');
                if (active) Modal.close(active);
            }
        });

        // AJAX form submit
        document.querySelectorAll('.ajax-form').forEach(function(form) {
            form.addEventListener('submit', function(e) {
                e.preventDefault();
                Form.submit(form);
            });
        });
    }

    // Start
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Export to global
    window.AdminUI = {
        Toast: Toast,
        Modal: Modal,
        Form: Form,
        Theme: Theme,
        Sidebar: Sidebar
    };

})();
