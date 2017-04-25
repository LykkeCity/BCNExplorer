var wH = $(window).height(),
    wW = $(window).width(),
    ua = navigator.userAgent,
    touchendOrClick = (ua.match(/iPad|iPhone|iPad/i)) ? "touchend" : "click",

    deviceAgent = navigator.userAgent.toLowerCase(),
    isMobile = deviceAgent.match(/(iphone|ipod|ipad)/),
    is_touch_device = ("ontouchstart" in window) || window.DocumentTouch && document instanceof DocumentTouch;

FastClick.attach(document.body);

function initEventsOnResize() {
    $(window).resize(function () {
        $('.header_container').css({
            height: $('.header').outerHeight()
        });

        $('body').css({
            paddingBottom: $('footer').outerHeight()
        })
    }).trigger('resize');
}

function initEventsOnClick() {
    $('[data-control="select"] ._value').text($(this).siblings('select').val());
    $('[data-control="select"] select').on('change', function () {
        $(this).siblings('.select__value').find('._value').text(this.value);
    });

    $('.action_follow').on('click', function () {
        $(this).toggleClass('active')
    })

    // Tel
    if (!isMobile) {
        $('body').on('click', 'a[href^="tel:"]', function () {
            $(this).attr('href',
                $(this).attr('href').replace(/^tel:/, 'callto:'));
        });
    }
    //moved to tx file
    //$('.open_hidden_content').on('click', function (ev) {
    //    ev.preventDefault();
    //    var $this = $(this), id = $this.attr('href');

    //    $this.toggleClass('active');
    //    $(id).slideToggle('fast');
    //});
}

function initClipboard() {
    var clipboard = new Clipboard('.copy_code', {
        text: function (trigger) {
            return trigger.previousElementSibling.innerHTML;
        }
    });

    clipboard.on('success', function (e) {
        e.trigger.innerHTML = '<i class="icon icon--check_thin"></i> Copied';
        e.clearSelection();
    });
}

function initTabs() {
    $('._go_to_tab').on('shown.bs.tab', function (e) {
        var href = $(this).attr('href');

        $('.nav-tabs a[href="' + href + '"]').tab('show');
    });

    var url = document.location.toString();
    var prefix = '#tab_';

    if (url.match('#')) {
        $('.nav-tabs a[href="#' + url.split(prefix)[1] + '"]').tab('show');
    }

    $('.nav-tabs a').on('shown.bs.tab', function (e) {
        window.location.hash = prefix + e.target.hash.split('#')[1];
    });
}

function initHeader() {
    $('.btn_menu').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();

        $('body').addClass('body--menu_opened');
        $('.sidebar_menu').addClass('sidebar_menu--open');
    });

    $('.sidebar_menu, .header_search').on('click', function (e) {
        e.stopPropagation();
    });

    $('body, .btn_close_menu, .menu_overlay, .btn_close_header').on('click', function (e) {
        $('body').removeClass('body--menu_opened body--search_showed');
        $('.sidebar_menu').removeClass('sidebar_menu--open');
        $('.header_search').removeClass('header_search--show');
    });

    $('.btn_open_search').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();

        $('body').addClass('body--search_showed');
        $('.header_search').addClass('header_search--show');
    });
}

function initSmoothScroll() {
    $('.smooth_scroll').click(function () {
        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top
                }, 1000);
                return false;
            }
        }
    });
}

function initAffix() {
    $('.btn_affix').affix({
        offset: {
            top: function () {
                return (this.top = $('.site_nav').offset().top - $('.header_container').outerHeight())
            }
        }
    });

    $(window).resize(function () {
        $('.btn_affix').css({
            right: $('.header .container').offset().left + 15
        })
    }).trigger('resize')
}

$(document).ready(function () {
    initEventsOnResize();
    initEventsOnClick();
    initClipboard();
    //initTabs();
    initHeader();
    initSmoothScroll();
    initAffix();
});