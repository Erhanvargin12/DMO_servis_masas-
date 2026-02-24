/**
 * Settings Page JavaScript
 * Ayarlar sayfası için JavaScript işlevleri
 */
(function() {
    'use strict';

    // Sayfa koruması: Ayarlar sayfası değilse çık
    if (!document.querySelector('.settings-container')) {
        return;
    }

    // Settings Manager
    const SettingsManager = {
        // Varsayılan ayarlar
        defaults: {
            theme: 'light',
            accentColor: '#0891b2',
            sidebarColor: '#1e293b',
            bgColor: '#f8f9fb',
            cardColor: '#ffffff',
            preset: 'default',
            useCustomColors: false,
            sidebarStyle: 'dark',
            compactMode: false,
            fontSize: 14,
            fontFamily: 'system',
            sidebarWidth: 240,
            sidebarCollapsed: false,
            menuStyle: 'default',
            borderRadius: 6,
            showAnimations: true,
            shadowIntensity: 'medium',
            pageSize: 8,
            compactTables: false,
            showTableBorders: true,
            zebraStripes: false,
            toastPosition: 'top-right',
            toastDuration: 3000,
            enableSounds: false,
            highContrast: false,
            reducedMotion: false,
            largeText: false,
            focusIndicator: true,
            lazyLoading: true,
            useCache: true,
            lineHeight: '1.5'
        },

    // Renk paletleri
        // Modern paletler (light/dark varyant)
        colorPresets: {
            default: { // Teal & Slate
                accent: '#0ea5e9',
                sidebar: '#0f172a',
                bg: '#f8fafc',
                card: '#ffffff',
                accentDark: '#38bdf8',
                sidebarDark: '#0b1220',
                bgDark: '#0f172a',
                cardDark: '#111827'
            },
            aurora: { // Mor + Camgöbeği
                accent: '#8b5cf6',
                sidebar: '#0b1021',
                bg: '#f5f3ff',
                card: '#ffffff',
                accentDark: '#a78bfa',
                sidebarDark: '#0b1021',
                bgDark: '#0f172a',
                cardDark: '#0f172a'
            },
            emerald: { // Yeşil + Koyu gri
                accent: '#22c55e',
                sidebar: '#0f1f17',
                bg: '#f1f5f3',
                card: '#ffffff',
                accentDark: '#34d399',
                sidebarDark: '#0c1510',
                bgDark: '#0f1f17',
                cardDark: '#0f1f17'
            },
            sunrise: { // Turuncu/Pembe pastel
                accent: '#f97316',
                sidebar: '#31102b',
                bg: '#fff7ed',
                card: '#ffffff',
                accentDark: '#fb923c',
                sidebarDark: '#2a0d24',
                bgDark: '#1b0a16',
                cardDark: '#1f0d1a'
            },
            ocean: { // Mavi tonları
                accent: '#2563eb',
                sidebar: '#0b1a2e',
                bg: '#eaf2ff',
                card: '#ffffff',
                accentDark: '#3b82f6',
                sidebarDark: '#0a1626',
                bgDark: '#0b1a2e',
                cardDark: '#0f2135'
            },
            mono: { // Minimal gri
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

        // Mevcut ayarları yükle
        load() {
            const stored = localStorage.getItem('appSettings');
            if (stored) {
                try {
                    const parsed = JSON.parse(stored);
                    const settings = { ...this.defaults, ...parsed };

                    // Geçersiz preset ise default'a dön
                    if (settings.preset && !this.colorPresets[settings.preset]) {
                        settings.preset = 'default';
                    }

                    // Eksik zorunlu alanları tamamla
                    ['accentColor', 'sidebarColor', 'bgColor', 'cardColor', 'theme'].forEach(k => {
                        if (!settings[k]) settings[k] = this.defaults[k];
                    });

                    return settings;
                } catch (e) {
                    console.error('Ayarlar yüklenemedi:', e);
                }
            }
            return this.defaults;
        },

        // Ayarları kaydet
        save(settings) {
            localStorage.setItem('appSettings', JSON.stringify(settings));
            this.apply(settings);
            return true;
        },

        // Ayarları uygula
        apply(settings) {
            const root = document.documentElement;
            const isDark = settings.theme === 'dark' || (settings.theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches);
            
            // Tema
            if (isDark) {
                root.setAttribute('data-theme', 'dark');
            } else {
                root.removeAttribute('data-theme');
            }

            // Font boyutu
            root.style.fontSize = settings.fontSize + 'px';

            // Font ailesi
            if (settings.fontFamily !== 'system') {
                const fontMap = {
                    'inter': '"Inter", sans-serif',
                    'roboto': '"Roboto", sans-serif',
                    'poppins': '"Poppins", sans-serif',
                    'montserrat': '"Montserrat", sans-serif',
                    'opensans': '"Open Sans", sans-serif',
                    'lato': '"Lato", sans-serif',
                    'raleway': '"Raleway", sans-serif'
                };
                root.style.setProperty('--font-family', fontMap[settings.fontFamily]);
            }

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

            // Sidebar genişliği
            root.style.setProperty('--sidebar-width', settings.sidebarWidth + 'px');

            // Border radius
            root.style.setProperty('--radius', settings.borderRadius + 'px');
            root.style.setProperty('--radius-sm', Math.max(settings.borderRadius - 2, 0) + 'px');
            root.style.setProperty('--radius-lg', (settings.borderRadius + 2) + 'px');
            root.style.setProperty('--radius-xl', (settings.borderRadius + 6) + 'px');

            // Kompakt mod
            if (settings.compactMode) {
                document.body.classList.add('compact-mode');
            } else {
                document.body.classList.remove('compact-mode');
            }

            // Animasyonlar
            if (!settings.showAnimations || settings.reducedMotion) {
                root.style.setProperty('--transition', '0s');
            } else {
                root.style.setProperty('--transition', '0.15s ease');
            }

            // Gölge yoğunluğu
            const shadowMap = {
                'none': {
                    sm: 'none',
                    base: 'none',
                    lg: 'none'
                },
                'light': {
                    sm: '0 1px 2px rgba(0, 0, 0, 0.02)',
                    base: '0 2px 4px rgba(0, 0, 0, 0.04)',
                    lg: '0 4px 12px rgba(0, 0, 0, 0.06)'
                },
                'medium': {
                    sm: '0 1px 2px rgba(0, 0, 0, 0.04)',
                    base: '0 2px 8px rgba(0, 0, 0, 0.06)',
                    lg: '0 8px 24px rgba(0, 0, 0, 0.08)'
                },
                'strong': {
                    sm: '0 2px 4px rgba(0, 0, 0, 0.08)',
                    base: '0 4px 12px rgba(0, 0, 0, 0.12)',
                    lg: '0 12px 32px rgba(0, 0, 0, 0.16)'
                }
            };

            const shadows = shadowMap[settings.shadowIntensity] || shadowMap.medium;
            root.style.setProperty('--shadow-sm', shadows.sm);
            root.style.setProperty('--shadow', shadows.base);
            root.style.setProperty('--shadow-lg', shadows.lg);

            // Erişilebilirlik
            if (settings.highContrast) {
                document.body.classList.add('high-contrast');
            } else {
                document.body.classList.remove('high-contrast');
            }

            if (settings.largeText) {
                document.body.classList.add('large-text');
            } else {
                document.body.classList.remove('large-text');
            }

            // Satır yüksekliği
            root.style.setProperty('--line-height', settings.lineHeight);

            // Tablolar
            if (settings.compactTables) {
                document.body.classList.add('compact-tables');
            } else {
                document.body.classList.remove('compact-tables');
            }

            if (settings.zebraStripes) {
                document.body.classList.add('zebra-stripes');
            } else {
                document.body.classList.remove('zebra-stripes');
            }

            // Sidebar durumu
            const sidebar = document.querySelector('.sidebar');
            if (sidebar && settings.sidebarCollapsed) {
                sidebar.classList.add('collapsed');
            }
        },

        // Ayarları sıfırla
        reset() {
            localStorage.removeItem('appSettings');
            this.apply(this.defaults);
            return this.defaults;
        },

        // Ayarları dışa aktar
        export() {
            const settings = this.load();
            const dataStr = JSON.stringify(settings, null, 2);
            const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr);
            
            const exportFileDefaultName = 'settings_' + new Date().toISOString().slice(0,10) + '.json';
            
            const linkElement = document.createElement('a');
            linkElement.setAttribute('href', dataUri);
            linkElement.setAttribute('download', exportFileDefaultName);
            linkElement.click();
        },

        // Ayarları içe aktar
        import(file) {
            return new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = (e) => {
                    try {
                        const settings = JSON.parse(e.target.result);
                        this.save(settings);
                        resolve(settings);
                    } catch (err) {
                        reject(err);
                    }
                };
                reader.readAsText(file);
            });
        }
    };

    // Sayfa yüklendiğinde ayarları uygula
    const currentSettings = SettingsManager.load();
    SettingsManager.apply(currentSettings);

    // DOM elementleri
    const elements = {
        // Tabs
        navItems: document.querySelectorAll('.settings-nav-item'),
        tabs: document.querySelectorAll('.settings-tab'),
        
        // Görünüm
        compactMode: document.getElementById('compactMode'),
        showAnimations: document.getElementById('showAnimations'),
        borderRadius: document.getElementById('borderRadius'),
        shadowIntensity: document.getElementById('shadowIntensity'),
        
        // Tema
        themeRadios: document.querySelectorAll('input[name="theme"]'),
        colorPresets: document.querySelectorAll('.color-preset'),
        accentColor: document.getElementById('accentColor'),
        sidebarColor: document.getElementById('sidebarColor'),
        bgColor: document.getElementById('bgColor'),
        cardColor: document.getElementById('cardColor'),
        
        // Typography
        fontFamily: document.getElementById('fontFamily'),
        fontSize: document.getElementById('fontSize'),
        lineHeight: document.getElementById('lineHeight'),
        
        // Layout
        sidebarWidth: document.getElementById('sidebarWidth'),
        sidebarStyle: document.getElementById('sidebarStyle'),
        menuStyle: document.getElementById('menuStyle'),
        sidebarCollapsed: document.getElementById('sidebarCollapsed'),
        
        // Tables
        pageSize: document.getElementById('pageSize'),
        compactTables: document.getElementById('compactTables'),
        showTableBorders: document.getElementById('showTableBorders'),
        zebraStripes: document.getElementById('zebraStripes'),
        
        // Notifications
        toastPosition: document.getElementById('toastPosition'),
        toastDuration: document.getElementById('toastDuration'),
        enableSounds: document.getElementById('enableSounds'),
        
        // Accessibility
        highContrast: document.getElementById('highContrast'),
        reducedMotion: document.getElementById('reducedMotion'),
        largeText: document.getElementById('largeText'),
        focusIndicator: document.getElementById('focusIndicator'),
        
        // Advanced
        lazyLoading: document.getElementById('lazyLoading'),
        useCache: document.getElementById('useCache'),
        
        // Buttons
        saveBtn: document.getElementById('saveSettingsBtn'),
        resetBtn: document.getElementById('resetSettingsBtn'),
        exportBtn: document.getElementById('exportSettingsBtn'),
        importInput: document.getElementById('importSettings'),
        clearCacheBtn: document.getElementById('clearCacheBtn'),
        testToastBtn: document.getElementById('testToastBtn')
    };

    let currentPreset = SettingsManager.defaults.preset;

    // Mevcut ayarları form'a yükle
    function loadSettingsToForm() {
        const settings = SettingsManager.load();
        currentPreset = settings.preset || 'default';
        
        // Checkboxes
        if (elements.compactMode) elements.compactMode.checked = settings.compactMode;
        if (elements.showAnimations) elements.showAnimations.checked = settings.showAnimations;
        if (elements.compactTables) elements.compactTables.checked = settings.compactTables;
        if (elements.showTableBorders) elements.showTableBorders.checked = settings.showTableBorders;
        if (elements.zebraStripes) elements.zebraStripes.checked = settings.zebraStripes;
        if (elements.enableSounds) elements.enableSounds.checked = settings.enableSounds;
        if (elements.sidebarCollapsed) elements.sidebarCollapsed.checked = settings.sidebarCollapsed;
        if (elements.highContrast) elements.highContrast.checked = settings.highContrast;
        if (elements.reducedMotion) elements.reducedMotion.checked = settings.reducedMotion;
        if (elements.largeText) elements.largeText.checked = settings.largeText;
        if (elements.focusIndicator) elements.focusIndicator.checked = settings.focusIndicator;
        if (elements.lazyLoading) elements.lazyLoading.checked = settings.lazyLoading;
        if (elements.useCache) elements.useCache.checked = settings.useCache;
        
        // Selects
        if (elements.shadowIntensity) elements.shadowIntensity.value = settings.shadowIntensity;
        if (elements.fontFamily) elements.fontFamily.value = settings.fontFamily;
        if (elements.lineHeight) elements.lineHeight.value = settings.lineHeight;
        if (elements.sidebarStyle) elements.sidebarStyle.value = settings.sidebarStyle;
        if (elements.menuStyle) elements.menuStyle.value = settings.menuStyle;
        if (elements.pageSize) elements.pageSize.value = settings.pageSize;
        if (elements.toastPosition) elements.toastPosition.value = settings.toastPosition;
        
        // Ranges
        if (elements.borderRadius) {
            elements.borderRadius.value = settings.borderRadius;
            elements.borderRadius.nextElementSibling.textContent = settings.borderRadius + 'px';
        }
        if (elements.fontSize) {
            elements.fontSize.value = settings.fontSize;
            elements.fontSize.nextElementSibling.textContent = settings.fontSize + 'px';
        }
        if (elements.sidebarWidth) {
            elements.sidebarWidth.value = settings.sidebarWidth;
            elements.sidebarWidth.nextElementSibling.textContent = settings.sidebarWidth + 'px';
        }
        if (elements.toastDuration) {
            elements.toastDuration.value = settings.toastDuration;
            elements.toastDuration.nextElementSibling.textContent = settings.toastDuration + 'ms';
        }
        
        // Theme
        elements.themeRadios.forEach(radio => {
            if (radio.value === settings.theme) {
                radio.checked = true;
            }
        });

        // Preset aktifliği
        elements.colorPresets.forEach(p => {
            p.classList.toggle('active', p.dataset.preset === currentPreset);
        });
        const customColors = document.querySelector('.custom-colors');
        if (customColors) customColors.style.display = currentPreset === 'custom' ? 'block' : 'none';
        
        // Colors
        if (elements.accentColor) {
            elements.accentColor.value = settings.accentColor;
            elements.accentColor.nextElementSibling.value = settings.accentColor;
        }
        if (elements.sidebarColor) {
            elements.sidebarColor.value = settings.sidebarColor;
            elements.sidebarColor.nextElementSibling.value = settings.sidebarColor;
        }
        if (elements.bgColor) {
            elements.bgColor.value = settings.bgColor;
            elements.bgColor.nextElementSibling.value = settings.bgColor;
        }
        if (elements.cardColor) {
            elements.cardColor.value = settings.cardColor;
            elements.cardColor.nextElementSibling.value = settings.cardColor;
        }
    }

    // Form'dan ayarları topla
    function collectSettingsFromForm() {
        const presetBtn = document.querySelector('.color-preset.active');
        const presetName = presetBtn ? presetBtn.dataset.preset : 'default';
        const useCustomColors = presetName === 'custom';

        // Preset seçilmişse renkleri preset'ten al
        let accentColor = elements.accentColor?.value || '#0891b2';
        let sidebarColor = elements.sidebarColor?.value || '#1e293b';
        let bgColor = elements.bgColor?.value || '#f8f9fb';
        let cardColor = elements.cardColor?.value || '#ffffff';

        if (!useCustomColors && SettingsManager.colorPresets[presetName]) {
            const colors = SettingsManager.colorPresets[presetName];
            accentColor = colors.accent;
            sidebarColor = colors.sidebar;
            bgColor = colors.bg;
            cardColor = colors.card;
        }

        return {
            // Görünüm
            compactMode: elements.compactMode?.checked || false,
            showAnimations: elements.showAnimations?.checked || true,
            borderRadius: parseInt(elements.borderRadius?.value || 6),
            shadowIntensity: elements.shadowIntensity?.value || 'medium',
            
            // Tema
            theme: document.querySelector('input[name="theme"]:checked')?.value || 'light',
            preset: presetName,
            useCustomColors: useCustomColors,
            accentColor,
            sidebarColor,
            bgColor,
            cardColor,
            
            // Typography
            fontFamily: elements.fontFamily?.value || 'system',
            fontSize: parseInt(elements.fontSize?.value || 14),
            lineHeight: elements.lineHeight?.value || '1.5',
            
            // Layout
            sidebarWidth: parseInt(elements.sidebarWidth?.value || 240),
            sidebarStyle: elements.sidebarStyle?.value || 'dark',
            menuStyle: elements.menuStyle?.value || 'default',
            sidebarCollapsed: elements.sidebarCollapsed?.checked || false,
            
            // Tables
            pageSize: parseInt(elements.pageSize?.value || 8),
            compactTables: elements.compactTables?.checked || false,
            showTableBorders: elements.showTableBorders?.checked || true,
            zebraStripes: elements.zebraStripes?.checked || false,
            
            // Notifications
            toastPosition: elements.toastPosition?.value || 'top-right',
            toastDuration: parseInt(elements.toastDuration?.value || 3000),
            enableSounds: elements.enableSounds?.checked || false,
            
            // Accessibility
            highContrast: elements.highContrast?.checked || false,
            reducedMotion: elements.reducedMotion?.checked || false,
            largeText: elements.largeText?.checked || false,
            focusIndicator: elements.focusIndicator?.checked || true,
            
            // Advanced
            lazyLoading: elements.lazyLoading?.checked || true,
            useCache: elements.useCache?.checked || true
        };
    }

    // Tab değiştirme
    elements.navItems.forEach(item => {
        item.addEventListener('click', function() {
            const tabName = this.dataset.tab;
            
            // Nav items
            elements.navItems.forEach(nav => nav.classList.remove('active'));
            this.classList.add('active');
            
            // Tabs
            elements.tabs.forEach(tab => tab.classList.remove('active'));
            document.getElementById(tabName + '-tab')?.classList.add('active');
        });
    });

    // Range input değer gösterimi
    document.querySelectorAll('.range-input').forEach(input => {
        input.addEventListener('input', function() {
            const valueSpan = this.nextElementSibling;
            if (valueSpan && valueSpan.classList.contains('range-value')) {
                valueSpan.textContent = this.value + (this.id.includes('Duration') ? 'ms' : 'px');
            }
            
            // Canlı önizleme
            const settings = collectSettingsFromForm();
            SettingsManager.apply(settings);
        });
    });

    // Renk paletleri
    elements.colorPresets.forEach(preset => {
        preset.addEventListener('click', function() {
            elements.colorPresets.forEach(p => p.classList.remove('active'));
            this.classList.add('active');
            
            currentPreset = this.dataset.preset || 'default';
            const customColors = document.querySelector('.custom-colors');
            
            if (currentPreset === 'custom') {
                customColors.style.display = 'block';
            } else {
                customColors.style.display = 'none';
                
                if (SettingsManager.colorPresets[currentPreset]) {
                    const colors = SettingsManager.colorPresets[currentPreset];
                    if (elements.accentColor) {
                        elements.accentColor.value = colors.accent;
                        elements.accentColor.nextElementSibling.value = colors.accent;
                    }
                    if (elements.sidebarColor) {
                        elements.sidebarColor.value = colors.sidebar;
                        elements.sidebarColor.nextElementSibling.value = colors.sidebar;
                    }
                    if (elements.bgColor) {
                        elements.bgColor.value = colors.bg;
                        elements.bgColor.nextElementSibling.value = colors.bg;
                    }
                    if (elements.cardColor) {
                        elements.cardColor.value = colors.card;
                        elements.cardColor.nextElementSibling.value = colors.card;
                    }
                }
            }

            // Canlı önizleme
            const settings = collectSettingsFromForm();
            SettingsManager.apply(settings);
        });
    });

    // Renk input senkronizasyonu
    document.querySelectorAll('.color-input-group').forEach(group => {
        const colorInput = group.querySelector('input[type="color"]');
        const textInput = group.querySelector('.color-text');
        
        if (colorInput && textInput) {
            colorInput.addEventListener('input', function() {
                textInput.value = this.value;
                const settings = collectSettingsFromForm();
                SettingsManager.apply(settings);
            });
            
            textInput.addEventListener('input', function() {
                if (/^#[0-9A-Fa-f]{6}$/.test(this.value)) {
                    colorInput.value = this.value;
                    const settings = collectSettingsFromForm();
                    SettingsManager.apply(settings);
                }
            });
        }
    });

    // Canlı önizleme için tüm inputlar
    document.querySelectorAll('input, select').forEach(input => {
        input.addEventListener('change', function() {
            const settings = collectSettingsFromForm();
            SettingsManager.apply(settings);
        });
    });

    // Kaydet butonu
    if (elements.saveBtn) {
        elements.saveBtn.addEventListener('click', function() {
            const settings = collectSettingsFromForm();
            if (SettingsManager.save(settings)) {
                if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                    AdminUI.Toast.success('Ayarlar başarıyla kaydedildi');
                } else {
                    alert('Ayarlar kaydedildi');
                }
            }
        });
    }

    // Sıfırla butonu
    if (elements.resetBtn) {
        elements.resetBtn.addEventListener('click', function() {
            if (confirm('Tüm ayarlar varsayılan değerlere döndürülecek. Emin misiniz?')) {
                const defaults = SettingsManager.reset();
                loadSettingsToForm();
                if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                    AdminUI.Toast.info('Ayarlar sıfırlandı');
                } else {
                    alert('Ayarlar sıfırlandı');
                }
            }
        });
    }

    // Export butonu
    if (elements.exportBtn) {
        elements.exportBtn.addEventListener('click', function() {
            SettingsManager.export();
            if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                AdminUI.Toast.success('Ayarlar dışa aktarıldı');
            }
        });
    }

    // Import
    if (elements.importInput) {
        elements.importInput.addEventListener('change', function(e) {
            const file = e.target.files[0];
            if (file) {
                SettingsManager.import(file)
                    .then(settings => {
                        loadSettingsToForm();
                        if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                            AdminUI.Toast.success('Ayarlar başarıyla içe aktarıldı');
                        } else {
                            alert('Ayarlar içe aktarıldı');
                        }
                    })
                    .catch(err => {
                        if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                            AdminUI.Toast.error('Ayarlar içe aktarılamadı');
                        } else {
                            alert('Hata: ' + err.message);
                        }
                    });
            }
        });
    }

    // Cache temizle
    if (elements.clearCacheBtn) {
        elements.clearCacheBtn.addEventListener('click', function() {
            if (confirm('Önbellek temizlenecek. Devam etmek istiyor musunuz?')) {
                localStorage.clear();
                sessionStorage.clear();
                if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                    AdminUI.Toast.success('Önbellek temizlendi');
                } else {
                    alert('Önbellek temizlendi');
                }
                setTimeout(() => location.reload(), 1000);
            }
        });
    }

    // Test toast
    if (elements.testToastBtn) {
        elements.testToastBtn.addEventListener('click', function() {
            if (typeof AdminUI !== 'undefined' && AdminUI.Toast) {
                AdminUI.Toast.success('Test bildirimi', 'Bu bir test mesajıdır');
            } else {
                alert('Test bildirimi');
            }
        });
    }

    // İlk yükleme
    loadSettingsToForm();

})();
