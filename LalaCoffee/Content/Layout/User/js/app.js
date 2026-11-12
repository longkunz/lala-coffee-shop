(function ($) {
    'use strict';

    $(document).ready(function () {
        CheckPositionMobile();

        /* Revolution Slider (Legacy) */
        if ($('.tp-banner').length && typeof $.fn.revolution === 'function') {
            jQuery('.tp-banner').show().revolution({
                dottedOverlay: "none",
                delay: 3500,
                startwidth: 1170,
                startheight: 700,
                hideThumbs: 200,
                thumbWidth: 100,
                thumbHeight: 50,
                thumbAmount: 5,
                navigationType: "bullet",
                navigationArrows: "hide",
                navigationStyle: "preview1",
                navigationInGrid: "off",
                touchenabled: "on",
                onHoverStop: "off",
                swipe_velocity: 0.7,
                swipe_min_touches: 1,
                swipe_max_touches: 1,
                drag_block_vertical: false,
                parallax: "mouse",
                parallaxBgFreeze: "on",
                parallaxLevels: [7, 4, 3, 2, 5, 4, 3, 2, 1, 0],
                keyboardNavigation: "off",
                navigationHAlign: "center",
                navigationVAlign: "bottom",
                navigationHOffset: 0,
                navigationVOffset: 20,
                soloArrowLeftHalign: "left",
                soloArrowLeftValign: "center",
                soloArrowLeftHOffset: 20,
                soloArrowLeftVOffset: 0,
                soloArrowRightHalign: "right",
                soloArrowRightValign: "center",
                soloArrowRightHOffset: 20,
                soloArrowRightVOffset: 0,
                shadow: 0,
                fullWidth: "off",
                fullScreen: "on",
                spinner: "spinner4",
                stopLoop: "off",
                stopAfterLoops: -1,
                stopAtSlide: -1,
                shuffle: "off",
                autoHeight: "off",
                forceFullWidth: "off",
                hideThumbsOnMobile: "off",
                hideNavDelayOnMobile: 1500,
                hideBulletsOnMobile: "off",
                hideArrowsOnMobile: "off",
                hideThumbsUnderResolution: 0,
                hideSliderAtLimit: 0,
                hideCaptionAtLimit: 0,
                hideAllCaptionAtLilmit: 0,
                startWithSlide: 0,
                fullScreenOffsetContainer: ""
            });
        }

        /* Stellar Parallax */
        if (typeof $.stellar === 'function') {
            $.stellar({
                horizontalScrolling: false,
                verticalScrolling: true,
                horizontalOffset: 0,
                verticalOffset: 0,
                responsive: true,
                scrollProperty: 'scroll',
                positionProperty: 'position',
                parallaxBackgrounds: true,
                parallaxElements: true,
                hideDistantElements: true,
                hideElement: function ($elem) { $elem.hide(); },
                showElement: function ($elem) { $elem.show(); }
            });
        }

        /* Owl Carousel For All sliders (Blog, Sản phẩm liên quan, ...) */
        if (typeof $.fn.owlCarousel === 'function' && $('.owl-carousel').length) {
            $('.owl-carousel').each(function () {
                var $carousel = $(this);
                var itemsDesktop = parseInt($carousel.data('items'), 10) || 3;
                var itemsTablet = parseInt($carousel.data('tablet'), 10) || 2;
                var itemsMobile = parseInt($carousel.data('mobile'), 10) || 1;
                var itemCount = $carousel.children('.item, div').length;
                var shouldLoop = itemCount > itemsDesktop;

                $carousel.owlCarousel({
                    margin: 30,
                    loop: shouldLoop,
                    rewind: !shouldLoop,
                    autoplayTimeout: $carousel.data('autotime') || 5000,
                    smartSpeed: $carousel.data('speed') || 1000,
                    autoplay: $carousel.data('autoplay') !== undefined ? $carousel.data('autoplay') : true,
                    items: itemsDesktop,
                    nav: $carousel.data('nav') !== undefined ? $carousel.data('nav') : true,
                    dots: $carousel.data('dots') !== undefined ? $carousel.data('dots') : false,
                    responsive: {
                        0: {
                            items: itemsMobile,
                            margin: 15
                        },
                        768: {
                            items: itemsTablet
                        },
                        992: {
                            items: itemsDesktop
                        }
                    }
                });
            });
        }

        /* Slick Slider Call Here... */
        if (typeof $.fn.slick === 'function') {
            if ($('.slider-for').length) {
                $('.slider-for').slick({
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    arrows: false,
                    fade: false,
                    asNavFor: $('.slider-nav').length ? '.slider-nav' : null
                });
            }

            if ($('.slider-nav').length) {
                $('.slider-nav').slick({
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    asNavFor: $('.slider-for').length ? '.slider-for' : null,
                    dots: false,
                    centerMode: false,
                    focusOnSelect: true,
                    responsive: [{
                        breakpoint: 991,
                        settings: {
                            arrows: true,
                            centerMode: false,
                            slidesToShow: 3
                        }
                    },
                    {
                        breakpoint: 480,
                        settings: {
                            arrows: true,
                            centerMode: false,
                            slidesToShow: 3
                        }
                    }]
                });
            }
        }

        /* Light Box Plugin */
        if (typeof $.fn.simpleLightbox === 'function' && $('.gallery-box .gallery-popup').length) {
            $('.gallery-box .gallery-popup').simpleLightbox();
        }

        /* Magnific Popup call here... */
        if (typeof $.fn.magnificPopup === 'function') {
            if ($('.magnific-popup').length) {
                $('.magnific-popup').magnificPopup({ type: 'image' });
            }
            if ($('.magnific-youtube').length) {
                $('.magnific-youtube').magnificPopup({ type: 'iframe' });
            }
        }

        /* Masonry call here... */
        if (typeof $.fn.isotope === 'function' && $('.galereya-grid').length) {
            setTimeout(function () {
                $('.galereya-grid').isotope({
                    itemSelector: '.msrItem',
                    layoutMode: 'packery',
                    packery: { fitWidth: true }
                });
            }, 1000);
        }

        /* Form Style Call Here... */
        if (typeof $.fn.styler === 'function') {
            $('select.select-dropbox, input[type="radio"], input[type="checkbox"]').styler({
                selectSearch: true,
            });
        }

        /* Filter Slider */
        if (typeof $.fn.slider === 'function' && $('#ex2').length) {
            $("#ex2").slider({});
        }
    });

    /* Progress Circle */
    function skillProcess() {
        if (typeof $.fn.waterbubble !== 'function') return;
        $('[data-ride="skillProcess"]').each(function () {
            var myPos = $(this).offset().top;
            var myBottomPos = $(this).offset().top + $(this).height();
            var winTopPos = $(window).scrollTop();
            var winBottomPos = $(window).scrollTop() + $(window).height();

            if (myPos < winBottomPos && myPos > winTopPos || myBottomPos < winBottomPos && myBottomPos > winTopPos) {
                if (!$(this).hasClass('init')) {
                    $(this).addClass('init');
                    setTimeout(function () {
                        $('.progress-skill-circle').each(function () {
                            if (typeof $(this).waterbubble === 'function') {
                                $(this).waterbubble({
                                    txt: $(this).data('text'),
                                    data: $(this).data('value'),
                                    waterColor: $(this).data('watercolor'),
                                    textColor: $(this).data('watertxtcolor'),
                                });
                            }
                        });
                    }, 800);
                }
            }
        });
    }

    function CheckPositionMobile() {
        if (window.matchMedia('(max-width: 991px)').matches) {
            /* Scroll Bar Plugin Call here... */
            if (typeof $.fn.mCustomScrollbar === 'function') {
                $(window).on("load", function () {
                    $(".menu-nav-main").mCustomScrollbar({
                        theme: "minimal"
                    });
                });
            }
        }
    }

    // Run window resize time
    $(window).on('resize', function () {
        CheckPositionMobile();
    });

    $(window).on('load', function () {
        skillProcess();
        // Initiate WOW JS safely
        if (typeof WOW === 'function') {
            new WOW().init();
        }
    });

    // Set the skill process element
    $(window).on('scroll', function () {
        skillProcess();
    });

})(jQuery);