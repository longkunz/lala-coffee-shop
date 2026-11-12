/**
 * LalaCoffee Admin Dashboard - Bootstrap 5.3 Interaction Script
 */
(function ($) {
    'use strict';

    $(document).ready(function () {
        // 1. Sidebar desktop / mobile unified toggle
        $(document).on('click', '#sidebarToggle', function (e) {
            e.preventDefault();
            if (window.innerWidth >= 992) {
                // Desktop: toggle collapsed state
                $('body').toggleClass('sidebar-collapsed');
                var isCollapsed = $('body').hasClass('sidebar-collapsed');
                try {
                    localStorage.setItem('admin_sidebar_collapsed', isCollapsed ? '1' : '0');
                } catch (err) {}
            } else {
                // Mobile / Tablet: toggle Bootstrap offcanvas drawer
                var sidebarEl = document.getElementById('adminSidebar');
                if (sidebarEl && window.bootstrap && window.bootstrap.Offcanvas) {
                    var bsOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(sidebarEl);
                    bsOffcanvas.toggle();
                } else if (sidebarEl) {
                    $(sidebarEl).toggleClass('show');
                }
            }
        });

        // 2. Restore sidebar state from localStorage (Desktop only)
        try {
            if (window.innerWidth >= 992 && localStorage.getItem('admin_sidebar_collapsed') === '1') {
                $('body').addClass('sidebar-collapsed');
            }
        } catch (err) {}

        // 3. Submenu collapse/expand click handling
        $(document).on('click', '.admin-sidebar [data-bs-toggle="collapse"]', function (e) {
            e.preventDefault();
            e.stopPropagation();

            // If sidebar is collapsed on desktop, auto-expand so submenu items are visible
            if ($('body').hasClass('sidebar-collapsed')) {
                $('body').removeClass('sidebar-collapsed');
                try {
                    localStorage.setItem('admin_sidebar_collapsed', '0');
                } catch (err) {}
            }

            var targetSelector = $(this).attr('data-bs-target') || $(this).attr('href');
            if (!targetSelector || targetSelector === '#' || targetSelector.indexOf('javascript:') === 0) return;

            var targetEl = document.querySelector(targetSelector);
            if (!targetEl) return;

            if (window.bootstrap && window.bootstrap.Collapse) {
                var bsCol = bootstrap.Collapse.getOrCreateInstance(targetEl);
                bsCol.toggle();
            } else {
                $(targetEl).toggleClass('show');
                var isExpanded = $(targetEl).hasClass('show');
                $(this).attr('aria-expanded', isExpanded ? 'true' : 'false');
            }
        });

        // 4. Sync arrow rotation and active states on collapse events
        $(document).on('show.bs.collapse', '.admin-sidebar .collapse', function () {
            var targetId = $(this).attr('id');
            var $toggle = $('[data-bs-target="#' + targetId + '"], [href="#' + targetId + '"]');
            $toggle.attr('aria-expanded', 'true');
            $toggle.addClass('active');
        });

        $(document).on('hide.bs.collapse', '.admin-sidebar .collapse', function () {
            var targetId = $(this).attr('id');
            var $toggle = $('[data-bs-target="#' + targetId + '"], [href="#' + targetId + '"]');
            $toggle.attr('aria-expanded', 'false');
            if (!$(this).find('.nav-link-admin.active').length) {
                $toggle.removeClass('active');
            }
        });

        // 5. Active route detection and auto-expansion
        var currentPath = window.location.pathname.toLowerCase();
        $('.admin-sidebar .nav-link-admin').each(function () {
            var href = $(this).attr('href');
            if (!href || href === '#' || href.indexOf('javascript:') === 0) return;

            var linkPath = href.toLowerCase();
            // Match exact or starts with (for controller actions)
            if (currentPath === linkPath || (linkPath !== '/admin' && currentPath.indexOf(linkPath) === 0)) {
                $(this).addClass('active');

                // If inside a collapse submenu, expand it using Bootstrap Collapse instance
                var parentCollapse = $(this).closest('.collapse');
                if (parentCollapse.length) {
                    var collapseEl = parentCollapse[0];
                    if (window.bootstrap && window.bootstrap.Collapse) {
                        var bsCol = bootstrap.Collapse.getOrCreateInstance(collapseEl, { toggle: false });
                        bsCol.show();
                    } else {
                        parentCollapse.addClass('show');
                    }
                    var targetId = parentCollapse.attr('id');
                    var $toggle = $('[data-bs-target="#' + targetId + '"], [href="#' + targetId + '"]');
                    $toggle.attr('aria-expanded', 'true');
                    $toggle.addClass('active');
                }
            }
        });

        // 6. Close mobile offcanvas sidebar only when clicking page navigation links (skip submenu toggles)
        $(document).on('click', '.admin-sidebar a.nav-link-admin', function () {
            var isCollapseToggle = $(this).is('[data-bs-toggle="collapse"]');
            var href = $(this).attr('href');
            if (isCollapseToggle || !href || href === '#' || href.indexOf('javascript:') === 0) {
                return; // Do NOT close offcanvas when toggling a submenu
            }

            if (window.innerWidth < 992) {
                var sidebarEl = document.getElementById('adminSidebar');
                if (sidebarEl && window.bootstrap && window.bootstrap.Offcanvas) {
                    var bsOffcanvas = bootstrap.Offcanvas.getInstance(sidebarEl);
                    if (bsOffcanvas) {
                        bsOffcanvas.hide();
                    }
                }
            }
        });
    });
})(jQuery);
