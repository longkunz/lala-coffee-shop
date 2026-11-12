/**
 * LalaCoffee Admin Dashboard - Bootstrap 5.3 Interaction Script
 */
(function ($) {
    'use strict';

    $(document).ready(function () {
        // 1. Sidebar desktop toggle
        $('#sidebarToggle').on('click', function (e) {
            e.preventDefault();
            $('body').toggleClass('sidebar-collapsed');
            var isCollapsed = $('body').hasClass('sidebar-collapsed');
            try {
                localStorage.setItem('admin_sidebar_collapsed', isCollapsed ? '1' : '0');
            } catch (err) {}
        });

        // Restore sidebar state from localStorage
        try {
            if (localStorage.getItem('admin_sidebar_collapsed') === '1') {
                $('body').addClass('sidebar-collapsed');
            }
        } catch (err) {}

        // 2. Active route highlighting in sidebar
        var currentPath = window.location.pathname.toLowerCase();
        $('.admin-sidebar .nav-link-admin').each(function () {
            var href = $(this).attr('href');
            if (!href || href === '#' || href === 'javascript:void(0);') return;

            var linkPath = href.toLowerCase();
            // Match exact or starts with (for controller actions)
            if (currentPath === linkPath || (linkPath !== '/admin' && currentPath.indexOf(linkPath) === 0)) {
                $(this).addClass('active');

                // If inside a collapse submenu, expand it
                var parentCollapse = $(this).closest('.collapse');
                if (parentCollapse.length) {
                    parentCollapse.addClass('show');
                    var toggleBtn = $('[data-bs-target="#' + parentCollapse.attr('id') + '"]');
                    toggleBtn.attr('aria-expanded', 'true');
                    toggleBtn.addClass('active');
                }
            }
        });

        // 3. Close mobile offcanvas sidebar when clicking a link
        $('.admin-sidebar .nav-link-admin').on('click', function () {
            if (window.innerWidth < 992) {
                var offcanvasEl = document.getElementById('adminSidebar');
                if (offcanvasEl && typeof bootstrap !== 'undefined' && bootstrap.Offcanvas) {
                    var bsOffcanvas = bootstrap.Offcanvas.getInstance(offcanvasEl);
                    if (bsOffcanvas) {
                        bsOffcanvas.hide();
                    }
                }
            }
        });
    });
})(jQuery);
