/**
 * Settings Page - AdminUI.Theme ile entegre ayarlar yönetimi
 */
(function () {
    'use strict';

    if (!document.querySelector('.settings-container')) return;

    // AdminUI.Theme hazır olana kadar bekle
    function waitForAdminUI(cb) {
        if (window.AdminUI && window.AdminUI.Theme) { cb(); return; }
        setTimeout(function () { waitForAdminUI(cb); }, 20);
    }

    waitForAdminUI(function () {
        var Theme = window.AdminUI.Theme;

        // ─── DOM Referansları ────────────────────────────────────────────────
        var el = {
            navItems:        document.querySelectorAll('.settings-nav-item'),
            tabs:            document.querySelectorAll('.settings-tab'),
            // Görünüm
            compactMode:     document.getElementById('compactMode'),
            showAnimations:  document.getElementById('showAnimations'),
            borderRadius:    document.getElementById('borderRadius'),
            shadowIntensity: document.getElementById('shadowIntensity'),
            topbarStyle:     document.getElementById('topbarStyle'),
            // Tema
            themeRadios:     document.querySelectorAll('input[name="theme"]'),
            colorPresets:    document.querySelectorAll('.color-preset'),
            accentColor:     document.getElementById('accentColor'),
            sidebarColor:    document.getElementById('sidebarColor'),
            bgColor:         document.getElementById('bgColor'),
            cardColor:       document.getElementById('cardColor'),
            // Typography
            fontFamily:      document.getElementById('fontFamily'),
            fontSize:        document.getElementById('fontSize'),
            lineHeight:      document.getElementById('lineHeight'),
            // Layout
            sidebarWidth:    document.getElementById('sidebarWidth'),
            sidebarStyle:    document.getElementById('sidebarStyle'),
            menuStyle:       document.getElementById('menuStyle'),
            sidebarCollapsed:document.getElementById('sidebarCollapsed'),
            // Tablolar
            pageSize:        document.getElementById('pageSize'),
            compactTables:   document.getElementById('compactTables'),
            showTableBorders:document.getElementById('showTableBorders'),
            zebraStripes:    document.getElementById('zebraStripes'),
            tableHoverRow:   document.getElementById('tableHoverRow'),
            stickyTableHeader:document.getElementById('stickyTableHeader'),
            tableRowNumbers: document.getElementById('tableRowNumbers'),
            // Bildirimler
            toastPosition:   document.getElementById('toastPosition'),
            toastDuration:   document.getElementById('toastDuration'),
            // Erişilebilirlik
            highContrast:    document.getElementById('highContrast'),
            reducedMotion:   document.getElementById('reducedMotion'),
            largeText:       document.getElementById('largeText'),
            focusIndicator:  document.getElementById('focusIndicator'),
            // Gelişmiş
            confirmDelete:   document.getElementById('confirmDelete'),
            customCss:       document.getElementById('customCss'),
            lazyLoading:     document.getElementById('lazyLoading'),
            useCache:        document.getElementById('useCache'),
            // Butonlar
            saveBtn:         document.getElementById('saveSettingsBtn'),
            resetBtn:        document.getElementById('resetSettingsBtn'),
            exportBtn:       document.getElementById('exportSettingsBtn'),
            importInput:     document.getElementById('importSettings'),
            clearCacheBtn:   document.getElementById('clearCacheBtn'),
            testToastBtn:    document.getElementById('testToastBtn')
        };

        // ─── Form → Settings ────────────────────────────────────────────────
        function collect() {
            var presetBtn = document.querySelector('.color-preset.active');
            var presetName = presetBtn ? (presetBtn.dataset.preset || 'default') : 'default';
            var useCustom = presetName === 'custom';

            var accentColor  = el.accentColor  ? el.accentColor.value  : '#0ea5e9';
            var sidebarColor = el.sidebarColor ? el.sidebarColor.value : '#0f172a';
            var bgColor      = el.bgColor      ? el.bgColor.value      : '#f8fafc';
            var cardColor    = el.cardColor    ? el.cardColor.value    : '#ffffff';

            if (!useCustom && Theme.colorPresets[presetName]) {
                var p = Theme.colorPresets[presetName];
                accentColor = p.accent; sidebarColor = p.sidebar;
                bgColor = p.bg; cardColor = p.card;
            }

            return {
                theme:            document.querySelector('input[name="theme"]:checked')?.value || 'light',
                preset:           presetName,
                useCustomColors:  useCustom,
                accentColor,  sidebarColor,  bgColor,  cardColor,
                compactMode:      el.compactMode?.checked || false,
                showAnimations:   el.showAnimations ? el.showAnimations.checked : true,
                borderRadius:     parseInt(el.borderRadius?.value || 6),
                shadowIntensity:  el.shadowIntensity?.value || 'medium',
                topbarStyle:      el.topbarStyle?.value || 'solid',
                fontFamily:       el.fontFamily?.value || 'system',
                fontSize:         parseInt(el.fontSize?.value || 14),
                lineHeight:       el.lineHeight?.value || '1.5',
                sidebarWidth:     parseInt(el.sidebarWidth?.value || 240),
                sidebarStyle:     el.sidebarStyle?.value || 'dark',
                menuStyle:        el.menuStyle?.value || 'default',
                sidebarCollapsed: el.sidebarCollapsed?.checked || false,
                pageSize:         parseInt(el.pageSize?.value || 10),
                compactTables:    el.compactTables?.checked || false,
                showTableBorders: el.showTableBorders ? el.showTableBorders.checked : true,
                zebraStripes:     el.zebraStripes?.checked || false,
                tableHoverRow:    el.tableHoverRow ? el.tableHoverRow.checked : true,
                stickyTableHeader:el.stickyTableHeader?.checked || false,
                tableRowNumbers:  el.tableRowNumbers?.checked || false,
                toastPosition:    el.toastPosition?.value || 'top-right',
                toastDuration:    parseInt(el.toastDuration?.value || 4000),
                highContrast:     el.highContrast?.checked || false,
                reducedMotion:    el.reducedMotion?.checked || false,
                largeText:        el.largeText?.checked || false,
                focusIndicator:   el.focusIndicator ? el.focusIndicator.checked : true,
                confirmDelete:    el.confirmDelete ? el.confirmDelete.checked : true,
                customCss:        el.customCss?.value || '',
                lazyLoading:      el.lazyLoading ? el.lazyLoading.checked : true,
                useCache:         el.useCache ? el.useCache.checked : true
            };
        }

        // ─── Settings → Form ────────────────────────────────────────────────
        function loadToForm() {
            var s = Theme.loadSettings();

            // Checkboxes
            var checks = ['compactMode','showAnimations','compactTables','showTableBorders',
                'zebraStripes','tableHoverRow','stickyTableHeader','tableRowNumbers',
                'sidebarCollapsed','highContrast','reducedMotion','largeText',
                'focusIndicator','confirmDelete','lazyLoading','useCache'];
            checks.forEach(function (k) {
                if (el[k]) el[k].checked = s[k] !== undefined ? s[k] : (Theme.defaults[k] !== undefined ? Theme.defaults[k] : false);
            });

            // Selects
            var selects = ['shadowIntensity','fontFamily','lineHeight','sidebarStyle',
                'menuStyle','topbarStyle','pageSize','toastPosition'];
            selects.forEach(function (k) {
                if (el[k] && s[k] !== undefined) el[k].value = s[k];
            });

            // Ranges
            setRange('borderRadius', s.borderRadius, 'px');
            setRange('fontSize', s.fontSize, 'px');
            setRange('sidebarWidth', s.sidebarWidth, 'px');
            setRange('toastDuration', s.toastDuration, 'ms');

            // Theme radios
            el.themeRadios.forEach(function (r) { r.checked = r.value === s.theme; });

            // Preset buttons
            var currentPreset = s.preset || 'default';
            el.colorPresets.forEach(function (p) {
                p.classList.toggle('active', p.dataset.preset === currentPreset);
            });
            var customBlock = document.querySelector('.custom-colors');
            if (customBlock) customBlock.style.display = currentPreset === 'custom' ? 'block' : 'none';

            // Color pickers
            syncColorPicker('accentColor', s.accentColor);
            syncColorPicker('sidebarColor', s.sidebarColor);
            syncColorPicker('bgColor', s.bgColor);
            syncColorPicker('cardColor', s.cardColor);

            // Custom CSS textarea
            if (el.customCss) el.customCss.value = s.customCss || '';

            // Font preview
            updateFontPreview(s.fontFamily);
        }

        function setRange(id, value, suffix) {
            var input = el[id];
            if (!input || value === undefined) return;
            input.value = value;
            var span = input.nextElementSibling;
            if (span && span.classList.contains('range-value')) span.textContent = value + suffix;
        }

        function syncColorPicker(id, value) {
            if (!el[id] || !value) return;
            el[id].value = value;
            var text = el[id].nextElementSibling;
            if (text) text.value = value;
        }

        function updateFontPreview(fontFamily) {
            var preview = document.querySelector('.font-preview');
            if (!preview) return;
            var fontMap = {
                'inter': 'Inter', 'roboto': 'Roboto', 'poppins': 'Poppins',
                'montserrat': 'Montserrat', 'nunito': 'Nunito', 'opensans': 'Open Sans'
            };
            if (fontFamily && fontFamily !== 'system' && fontMap[fontFamily]) {
                preview.style.fontFamily = '"' + fontMap[fontFamily] + '", sans-serif';
            } else {
                preview.style.fontFamily = '';
            }
        }

        // ─── Canlı önizleme ─────────────────────────────────────────────────
        function livePreview() {
            Theme.applyGlobal(collect());
        }

        // ─── Tab navigasyonu ─────────────────────────────────────────────────
        el.navItems.forEach(function (item) {
            item.addEventListener('click', function () {
                var tab = this.dataset.tab;
                el.navItems.forEach(function (n) { n.classList.remove('active'); });
                el.tabs.forEach(function (t) { t.classList.remove('active'); });
                this.classList.add('active');
                var tabEl = document.getElementById(tab + '-tab');
                if (tabEl) tabEl.classList.add('active');
            });
        });

        // ─── Range inputs ────────────────────────────────────────────────────
        document.querySelectorAll('.range-input').forEach(function (input) {
            input.addEventListener('input', function () {
                var span = this.nextElementSibling;
                if (span && span.classList.contains('range-value')) {
                    span.textContent = this.value + (this.id.toLowerCase().includes('duration') ? 'ms' : 'px');
                }
                livePreview();
            });
        });

        // ─── Preset tıklama ──────────────────────────────────────────────────
        el.colorPresets.forEach(function (preset) {
            preset.addEventListener('click', function () {
                el.colorPresets.forEach(function (p) { p.classList.remove('active'); });
                this.classList.add('active');

                var name = this.dataset.preset || 'default';
                var customBlock = document.querySelector('.custom-colors');

                if (name === 'custom') {
                    if (customBlock) customBlock.style.display = 'block';
                } else {
                    if (customBlock) customBlock.style.display = 'none';
                    var p = Theme.colorPresets[name];
                    if (p) {
                        syncColorPicker('accentColor', p.accent);
                        syncColorPicker('sidebarColor', p.sidebar);
                        syncColorPicker('bgColor', p.bg);
                        syncColorPicker('cardColor', p.card);
                    }
                }
                livePreview();
            });
        });

        // ─── Renk picker senkronizasyonu ─────────────────────────────────────
        document.querySelectorAll('.color-input-group').forEach(function (group) {
            var colorIn = group.querySelector('input[type="color"]');
            var textIn  = group.querySelector('.color-text');
            if (colorIn && textIn) {
                colorIn.addEventListener('input', function () {
                    textIn.value = this.value;
                    livePreview();
                });
                textIn.addEventListener('input', function () {
                    if (/^#[0-9A-Fa-f]{6}$/.test(this.value)) {
                        colorIn.value = this.value;
                        livePreview();
                    }
                });
            }
        });

        // ─── Genel input değişim → canlı önizleme ────────────────────────────
        document.querySelectorAll('.settings-container input:not([type="range"]):not([type="color"]):not([type="file"]), .settings-container select, .settings-container textarea').forEach(function (inp) {
            inp.addEventListener('change', function () {
                livePreview();
                if (inp.id === 'fontFamily') updateFontPreview(inp.value);
            });
        });

        // ─── Kaydet ──────────────────────────────────────────────────────────
        if (el.saveBtn) {
            el.saveBtn.addEventListener('click', function () {
                var settings = collect();
                Theme.saveSettings(settings);
                Theme.applyGlobal(settings);
                window.AdminUI.Toast.success('Ayarlar başarıyla kaydedildi');
            });
        }

        // ─── Sıfırla ─────────────────────────────────────────────────────────
        if (el.resetBtn) {
            el.resetBtn.addEventListener('click', function () {
                if (confirm('Tüm ayarlar varsayılana döndürülecek. Emin misiniz?')) {
                    localStorage.removeItem('appSettings');
                    Theme.applyGlobal(Theme.defaults);
                    loadToForm();
                    window.AdminUI.Toast.info('Ayarlar sıfırlandı');
                }
            });
        }

        // ─── Dışa aktar ──────────────────────────────────────────────────────
        if (el.exportBtn) {
            el.exportBtn.addEventListener('click', function () {
                var data = JSON.stringify(Theme.loadSettings(), null, 2);
                var uri = 'data:application/json;charset=utf-8,' + encodeURIComponent(data);
                var a = document.createElement('a');
                a.href = uri;
                a.download = 'settings_' + new Date().toISOString().slice(0, 10) + '.json';
                a.click();
                window.AdminUI.Toast.success('Ayarlar dışa aktarıldı');
            });
        }

        // ─── İçe aktar ───────────────────────────────────────────────────────
        if (el.importInput) {
            el.importInput.addEventListener('change', function (e) {
                var file = e.target.files[0];
                if (!file) return;
                var reader = new FileReader();
                reader.onload = function (ev) {
                    try {
                        var imported = JSON.parse(ev.target.result);
                        Theme.saveSettings({ ...Theme.loadSettings(), ...imported });
                        loadToForm();
                        livePreview();
                        window.AdminUI.Toast.success('Ayarlar içe aktarıldı');
                    } catch (err) {
                        window.AdminUI.Toast.error('Geçersiz ayar dosyası');
                    }
                };
                reader.readAsText(file);
            });
        }

        // ─── Önbellek temizle ────────────────────────────────────────────────
        if (el.clearCacheBtn) {
            el.clearCacheBtn.addEventListener('click', function () {
                if (confirm('Tarayıcı önbelleği temizlenecek. Emin misiniz?')) {
                    var appSettings = localStorage.getItem('appSettings');
                    localStorage.clear();
                    sessionStorage.clear();
                    if (appSettings) localStorage.setItem('appSettings', appSettings);
                    window.AdminUI.Toast.success('Önbellek temizlendi');
                    setTimeout(function () { location.reload(); }, 1000);
                }
            });
        }

        // ─── Test bildirimi ───────────────────────────────────────────────────
        if (el.testToastBtn) {
            el.testToastBtn.addEventListener('click', function () {
                var pos = el.toastPosition ? el.toastPosition.value : 'top-right';
                var container = document.getElementById('toastContainer');
                if (container) container.className = 'toast-container toast-pos-' + pos;
                window.AdminUI.Toast.success('Test bildirimi başarıyla gösterildi!', 'Bildirim Testi');
            });
        }

        // ─── İlk yükleme ─────────────────────────────────────────────────────
        loadToForm();
    });
})();
