document.addEventListener('DOMContentLoaded', function () {
  // ══════════════════════════════════════
  // Sidebar Toggle (desktop + mobile)
  // ══════════════════════════════════════
  var sidebar = document.getElementById('sidebar');
  var overlay = document.getElementById('sidebarOverlay');
  var toggle = document.getElementById('sidebarToggle');
  var mainContent = document.querySelector('.main-content');
  var headerBar = document.querySelector('.header-bar');

  function isMobile() {
    return window.innerWidth <= 991;
  }

  function openSidebar() {
    if (isMobile()) {
      if (sidebar) sidebar.classList.add('open');
      if (overlay) overlay.classList.add('show');
      document.body.style.overflow = 'hidden';
    } else {
      if (sidebar) sidebar.classList.remove('collapsed');
      if (mainContent) mainContent.classList.remove('sidebar-collapsed');
      if (headerBar) headerBar.classList.remove('sidebar-collapsed');
    }
    if (toggle) toggle.classList.add('active');
  }

  function closeSidebar() {
    if (isMobile()) {
      if (sidebar) sidebar.classList.remove('open');
      if (overlay) overlay.classList.remove('show');
      document.body.style.overflow = '';
    } else {
      if (sidebar) sidebar.classList.add('collapsed');
      if (mainContent) mainContent.classList.add('sidebar-collapsed');
      if (headerBar) headerBar.classList.add('sidebar-collapsed');
    }
    if (toggle) toggle.classList.remove('active');
  }

  if (toggle) {
    toggle.addEventListener('click', function () {
      var isOpen = isMobile()
        ? (sidebar && sidebar.classList.contains('open'))
        : (sidebar && !sidebar.classList.contains('collapsed'));
      if (isOpen) {
        closeSidebar();
      } else {
        openSidebar();
      }
    });
  }

  if (overlay) {
    overlay.addEventListener('click', closeSidebar);
  }

  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') closeSidebar();
  });

  window.addEventListener('resize', function () {
    if (!sidebar) return;
    if (isMobile()) {
      sidebar.classList.remove('collapsed');
      if (mainContent) mainContent.classList.remove('sidebar-collapsed');
      if (headerBar) headerBar.classList.remove('sidebar-collapsed');
    } else {
      sidebar.classList.remove('open');
      if (overlay) overlay.classList.remove('show');
      document.body.style.overflow = '';
    }
  });

  // ══════════════════════════════════════
  // Sidebar Collapse Menus
  // ══════════════════════════════════════
  if (sidebar) {
    var STORAGE_KEY = 'sidebarCollapse';

    function loadState() {
      try {
        var raw = sessionStorage.getItem(STORAGE_KEY);
        return raw ? JSON.parse(raw) : {};
      } catch (e) {
        return {};
      }
    }

    function saveState(id, isOpen) {
      var state = loadState();
      state[id] = isOpen;
      try {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(state));
      } catch (e) {
        try { localStorage.setItem(STORAGE_KEY, JSON.stringify(state)); } catch (ex) {}
      }
    }

    var saved = loadState();

    document.querySelectorAll('.sidebar-collapse-menu').forEach(function (menu) {
      var id = menu.id;
      if (!id) return;

      if (saved.hasOwnProperty(id)) {
        if (saved[id]) {
          menu.classList.add('show');
        } else {
          menu.classList.remove('show');
        }
      } else {
        menu.classList.add('show');
        saveState(id, true);
      }
    });

    document.querySelectorAll('.sidebar-collapse-menu.show').forEach(function (menu) {
      menu.style.maxHeight = menu.scrollHeight + 'px';
    });

    document.querySelectorAll('.sidebar-collapse-header').forEach(function (header) {
      header.addEventListener('click', function (e) {
        e.preventDefault();
        var targetId = this.getAttribute('data-target');
        var target = document.getElementById(targetId);
        if (!target) return;

        var isOpening = !target.classList.contains('show');

        if (isOpening) {
          target.classList.add('show');
          target.style.maxHeight = target.scrollHeight + 'px';
        } else {
          target.classList.remove('show');
          target.style.maxHeight = '0px';
        }
        saveState(targetId, isOpening);

        var icon = this.querySelector('.sidebar-collapse-icon');
        if (icon) {
          icon.classList.toggle('rotated', isOpening);
        }
      });
    });
  }

  // ══════════════════════════════════════
  // Active Nav Link
  // ══════════════════════════════════════
  var currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll('.sidebar-nav-link').forEach(function (link) {
    var href = link.getAttribute('href');
    if (href && currentPath.indexOf(href.toLowerCase()) !== -1) {
      link.classList.add('active');
    }
  });

  // ══════════════════════════════════════
  // Dark Mode
  // ══════════════════════════════════════
  var htmlEl = document.documentElement;
  var themeKey = 'emitTheme';

  function applyTheme(theme) {
    htmlEl.setAttribute('data-theme', theme);
    var icon = document.querySelector('#themeToggle i');
    if (icon) {
      icon.className = theme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
    }
    var titleKey = theme === 'dark' ? 'header.lightMode' : 'header.darkMode';
    var btn = document.getElementById('themeToggle');
    if (btn && typeof t === 'function') {
      btn.title = t(titleKey);
    }
  }

  var savedTheme = localStorage.getItem(themeKey) || 'light';
  applyTheme(savedTheme);

  var themeBtn = document.getElementById('themeToggle');
  if (themeBtn) {
    themeBtn.addEventListener('click', function () {
      var current = htmlEl.getAttribute('data-theme');
      var next = current === 'dark' ? 'light' : 'dark';
      localStorage.setItem(themeKey, next);
      applyTheme(next);
    });
  }

  // ══════════════════════════════════════
  // Language Switcher
  // ══════════════════════════════════════
  var langKey = 'emitLanguage';
  var langLabels = { fr: 'FR', mg: 'MG', en: 'EN' };
  var langFlags = { fr: 'fi-fr', mg: 'fi-mg', en: 'fi-gb' };

  function applyLanguage(lang) {
    localStorage.setItem(langKey, lang);
    var label = document.getElementById('currentLangLabel');
    if (label) label.textContent = langLabels[lang] || lang.toUpperCase();
    var flag = document.getElementById('currentLangFlag');
    if (flag) flag.className = 'fi ' + (langFlags[lang] || 'fi-fr');
    if (typeof applyTranslations === 'function') {
      applyTranslations();
    }
    document.querySelectorAll('.language-option').forEach(function(opt) {
      opt.classList.toggle('active', opt.getAttribute('data-lang') === lang);
    });
  }

  var savedLang = localStorage.getItem(langKey) || 'fr';
  applyLanguage(savedLang);

  document.querySelectorAll('.language-option').forEach(function(opt) {
    opt.addEventListener('click', function (e) {
      e.preventDefault();
      var lang = this.getAttribute('data-lang');
      applyLanguage(lang);
    });
  });

  // ══════════════════════════════════════
  // Global Search
  // ══════════════════════════════════════
  var searchInput = document.getElementById('globalSearch');
  var searchResults = document.getElementById('searchResults');

  if (searchInput && searchResults) {
    var searchIndex = buildSearchIndex();
    var searchTimeout = null;

    searchInput.addEventListener('input', function () {
      var self = this;
      clearTimeout(searchTimeout);
      searchTimeout = setTimeout(function() {
        var query = self.value.trim().toLowerCase();
        if (query.length < 1) {
          searchResults.classList.remove('show');
          searchResults.innerHTML = '';
          return;
        }

        var results = searchIndex.filter(function(item) {
          return item.title.toLowerCase().indexOf(query) !== -1 ||
                 (item.subtitle && item.subtitle.toLowerCase().indexOf(query) !== -1);
        });

        if (results.length === 0) {
          searchResults.innerHTML = '<div class="search-no-results">' + (typeof t === 'function' ? t('common.noResults') : 'Aucun résultat trouvé') + '</div>';
        } else {
          var html = '';
          var maxResults = Math.min(results.length, 8);
          for (var i = 0; i < maxResults; i++) {
            var r = results[i];
            var highlightedTitle = highlightText(r.title, query);
            var highlightedSubtitle = r.subtitle ? highlightText(r.subtitle, query) : '';
            html += '<a href="' + r.url + '" class="search-result-item">' +
              '<div class="search-result-icon ' + r.iconBg + '">' +
                '<i class="' + r.icon + '"></i>' +
              '</div>' +
              '<div class="search-result-text">' +
                '<div class="search-result-title">' + highlightedTitle + '</div>' +
                (highlightedSubtitle ? '<div class="search-result-subtitle">' + highlightedSubtitle + '</div>' : '') +
              '</div>' +
              '<span class="search-result-category">' + r.category + '</span>' +
            '</a>';
          }
          searchResults.innerHTML = html;
        }
        searchResults.classList.add('show');
      }, 150);
    });

    searchInput.addEventListener('focus', function () {
      if (this.value.trim().length >= 1 && searchResults.innerHTML) {
        searchResults.classList.add('show');
      }
    });

    document.addEventListener('click', function (e) {
      if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
        searchResults.classList.remove('show');
      }
    });

    searchInput.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') {
        searchResults.classList.remove('show');
        searchInput.blur();
      }
    });

    searchResults.addEventListener('click', function (e) {
      var link = e.target.closest('.search-result-item');
      if (link) {
        searchResults.classList.remove('show');
        searchInput.value = '';
      }
    });
  }

  function highlightText(text, query) {
    var idx = text.toLowerCase().indexOf(query.toLowerCase());
    if (idx === -1) return text;
    return text.substring(0, idx) +
      '<span class="search-highlight">' + text.substring(idx, idx + query.length) + '</span>' +
      text.substring(idx + query.length);
  }

  function buildSearchIndex() {
    var items = [];
    var lang = localStorage.getItem('emitLanguage') || 'fr';
    var navLabels = (typeof I18N !== 'undefined' && I18N[lang]) ? I18N[lang] : (typeof I18N !== 'undefined' ? I18N['fr'] : {});

    items.push({
      title: navLabels['nav.timetable'] || 'Emploi du temps',
      subtitle: navLabels['timetable.subtitle'] || 'Consultation',
      url: '/Home/Timetable',
      icon: 'fa-regular fa-calendar',
      iconBg: 'bg-primary bg-opacity-10 text-primary',
      category: navLabels['nav.timetable'] || 'Navigation'
    });
    items.push({
      title: navLabels['nav.dashboard'] || 'Tableau de bord',
      subtitle: navLabels['dashboard.subtitle'] || 'Administration',
      url: '/Dashboard/Index',
      icon: 'fa-solid fa-gauge-high',
      iconBg: 'bg-primary bg-opacity-10 text-primary',
      category: navLabels['nav.administration'] || 'Admin'
    });
    items.push({
      title: navLabels['nav.planning'] || 'Planification',
      subtitle: navLabels['schedules.subtitle'] || 'Gestion des emplois du temps',
      url: '/Schedules/Index',
      icon: 'fa-regular fa-clock',
      iconBg: 'bg-warning bg-opacity-10 text-warning',
      category: navLabels['nav.administration'] || 'Admin'
    });
    items.push({
      title: navLabels['nav.attendance'] || 'Présences',
      subtitle: navLabels['attendance.title'] || 'Gestion des présences',
      url: '/Attendance/SelectSchedule',
      icon: 'fa-regular fa-circle-check',
      iconBg: 'bg-success bg-opacity-10 text-success',
      category: navLabels['nav.administration'] || 'Admin'
    });
    items.push({
      title: navLabels['nav.connected'] || 'Connectés',
      subtitle: navLabels['users.connected'] || 'Utilisateurs connectés',
      url: '/Account/ConnectedUsers',
      icon: 'fa-solid fa-users',
      iconBg: 'bg-success bg-opacity-10 text-success',
      category: navLabels['nav.administration'] || 'Admin'
    });
    items.push({
      title: navLabels['nav.rooms'] || 'Salles',
      subtitle: navLabels['rooms.title'] || 'Gestion des salles',
      url: '/Rooms/Index',
      icon: 'fa-solid fa-door-open',
      iconBg: 'bg-info bg-opacity-10 text-info',
      category: navLabels['nav.resources'] || 'Ressources'
    });
    items.push({
      title: navLabels['nav.teachers'] || 'Enseignants',
      subtitle: navLabels['teachers.title'] || 'Gestion des enseignants',
      url: '/Teachers/Index',
      icon: 'fa-solid fa-chalkboard-user',
      iconBg: 'bg-primary bg-opacity-10 text-primary',
      category: navLabels['nav.resources'] || 'Ressources'
    });
    items.push({
      title: navLabels['nav.students'] || 'Étudiants',
      subtitle: navLabels['students.title'] || 'Gestion des étudiants',
      url: '/Students/Index',
      icon: 'fa-solid fa-user-group',
      iconBg: 'bg-purple bg-opacity-10 text-purple',
      category: navLabels['nav.resources'] || 'Ressources'
    });
    items.push({
      title: navLabels['nav.profile'] || 'Profil',
      subtitle: '',
      url: '/Profile/Index',
      icon: 'fa-regular fa-user',
      iconBg: 'bg-secondary bg-opacity-10 text-secondary',
      category: ''
    });

    document.querySelectorAll('table.table tbody tr').forEach(function(tr) {
      var cells = tr.querySelectorAll('td');
      if (cells.length < 2) return;

      var rowText = tr.textContent.trim().replace(/\s+/g, ' ');
      var url = '';
      var editLink = tr.querySelector('a[href*="Edit"]');
      if (editLink) {
        url = editLink.getAttribute('href');
      } else {
        var firstLink = tr.querySelector('a');
        if (firstLink) url = firstLink.getAttribute('href');
      }

      if (url && rowText.length > 2) {
        var isRoom = rowText.indexOf('Emplacement') !== -1 || rowText.indexOf('Capacité') !== -1;
        var isTeacher = rowText.indexOf('Matière') !== -1 && rowText.indexOf('Numéro') !== -1;
        var isStudent = rowText.indexOf('Matricule') !== -1 || rowText.indexOf('Niveau') !== -1;
        var isSchedule = rowText.indexOf('Horaire') !== -1;
        var isUser = rowText.indexOf('Rôle') !== -1 && rowText.indexOf('Inscrit') !== -1;

        var iconClass = isRoom ? 'fa-solid fa-door-open' : isTeacher ? 'fa-solid fa-chalkboard-user' : isStudent ? 'fa-solid fa-user-group' : isSchedule ? 'fa-regular fa-clock' : isUser ? 'fa-regular fa-user' : 'fa-solid fa-table';
        var bgClass = isRoom ? 'bg-info bg-opacity-10 text-info' : isTeacher ? 'bg-primary bg-opacity-10 text-primary' : isStudent ? 'bg-purple bg-opacity-10 text-purple' : isSchedule ? 'bg-warning bg-opacity-10 text-warning' : 'bg-secondary bg-opacity-10 text-secondary';
        var cat = isRoom ? (navLabels['nav.rooms'] || 'Salles') : isTeacher ? (navLabels['nav.teachers'] || 'Enseignants') : isStudent ? (navLabels['nav.students'] || 'Étudiants') : isSchedule ? (navLabels['nav.planning'] || 'Planification') : (navLabels['users.title'] || 'Utilisateurs');

        items.push({
          title: rowText.substring(0, 80),
          subtitle: cat,
          url: url,
          icon: iconClass,
          iconBg: bgClass,
          category: cat
        });
      }
    });

    return items;
  }

  // ══════════════════════════════════════
  // Ripple Effect on Buttons
  // ══════════════════════════════════════
  document.addEventListener('click', function (e) {
    var btn = e.target.closest('.btn');
    if (!btn) return;

    var ripple = document.createElement('span');
    ripple.className = 'ripple-effect';

    var rect = btn.getBoundingClientRect();
    var size = Math.max(rect.width, rect.height);
    var x = e.clientX - rect.left - size / 2;
    var y = e.clientY - rect.top - size / 2;

    ripple.style.width = ripple.style.height = size + 'px';
    ripple.style.left = x + 'px';
    ripple.style.top = y + 'px';

    btn.appendChild(ripple);

    ripple.addEventListener('animationend', function () {
      ripple.remove();
    });
  });

  // ══════════════════════════════════════
  // Scroll Reveal (Intersection Observer)
  // ══════════════════════════════════════
  if ('IntersectionObserver' in window) {
    var revealObserver = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add('revealed');
          revealObserver.unobserve(entry.target);
        }
      });
    }, {
      threshold: 0.1,
      rootMargin: '0px 0px -40px 0px'
    });

    document.querySelectorAll('.reveal-on-scroll').forEach(function (el) {
      revealObserver.observe(el);
    });
  }

  // ══════════════════════════════════════
  // Stagger Animation for Lists/Tables
  // ══════════════════════════════════════
  if ('IntersectionObserver' in window) {
    var staggerObserver = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          var children = entry.target.querySelectorAll('.stagger-item');
          children.forEach(function (child, index) {
            child.style.animationDelay = (index * 0.05) + 's';
            child.classList.add('animate-in');
          });
          staggerObserver.unobserve(entry.target);
        }
      });
    }, {
      threshold: 0.05
    });

    document.querySelectorAll('.stagger-container').forEach(function (el) {
      staggerObserver.observe(el);
    });
  }

  // ══════════════════════════════════════
  // Animated Counter for Stat Numbers
  // ══════════════════════════════════════
  if ('IntersectionObserver' in window) {
    var counterObserver = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          animateCounter(entry.target);
          counterObserver.unobserve(entry.target);
        }
      });
    }, { threshold: 0.5 });

    document.querySelectorAll('.stat-number[data-count]').forEach(function (el) {
      counterObserver.observe(el);
    });
  }

  function animateCounter(el) {
    var target = parseInt(el.getAttribute('data-count'), 10);
    if (isNaN(target) || target === 0) return;

    var duration = 800;
    var start = 0;
    var startTime = null;

    function step(timestamp) {
      if (!startTime) startTime = timestamp;
      var progress = Math.min((timestamp - startTime) / duration, 1);
      var eased = 1 - Math.pow(1 - progress, 3);
      var current = Math.floor(eased * target);
      el.textContent = current;
      if (progress < 1) {
        requestAnimationFrame(step);
      } else {
        el.textContent = target;
      }
    }

    requestAnimationFrame(step);
  }

  // ══════════════════════════════════════
  // Smooth Scroll for Anchor Links
  // ══════════════════════════════════════
  document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
    anchor.addEventListener('click', function (e) {
      var target = document.querySelector(this.getAttribute('href'));
      if (target) {
        e.preventDefault();
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    });
  });

  // ══════════════════════════════════════
  // Tooltip Simple (CSS-based)
  // ══════════════════════════════════════
  document.querySelectorAll('[data-tooltip]').forEach(function (el) {
    el.style.position = 'relative';
    el.addEventListener('mouseenter', function () {
      var tip = document.createElement('div');
      tip.className = 'custom-tooltip';
      tip.textContent = this.getAttribute('data-tooltip');
      document.body.appendChild(tip);

      var rect = this.getBoundingClientRect();
      tip.style.position = 'fixed';
      tip.style.left = rect.left + rect.width / 2 - tip.offsetWidth / 2 + 'px';
      tip.style.top = rect.top - tip.offsetHeight - 8 + 'px';
      tip.style.zIndex = '9999';
      tip.style.background = '#1E293B';
      tip.style.color = '#fff';
      tip.style.padding = '6px 12px';
      tip.style.borderRadius = '8px';
      tip.style.fontSize = '0.75rem';
      tip.style.fontWeight = '500';
      tip.style.whiteSpace = 'nowrap';
      tip.style.pointerEvents = 'none';
      tip.style.animation = 'fadeIn 0.15s ease';
      tip.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';

      this._tooltipEl = tip;
    });

    el.addEventListener('mouseleave', function () {
      if (this._tooltipEl) {
        this._tooltipEl.remove();
        this._tooltipEl = null;
      }
    });
  });

  // ══════════════════════════════════════
  // Auto-dismiss Alerts after 5s
  // ══════════════════════════════════════
  document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
    setTimeout(function () {
      alert.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
      alert.style.opacity = '0';
      alert.style.transform = 'translateY(-10px)';
      setTimeout(function () { alert.remove(); }, 300);
    }, 5000);
  });
});
