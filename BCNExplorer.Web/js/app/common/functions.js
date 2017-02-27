// CODE ========================

var wH = $(window).height(),
    wW = $(window).width(),
    ua = navigator.userAgent,
    touchendOrClick = (ua.match(/iPad|iPhone|iPad/i)) ? "touchend" : "click",

    deviceAgent = navigator.userAgent.toLowerCase(),
    isMobile = deviceAgent.match(/(iphone|ipod|ipad)/);

FastClick.attach(document.body);

$(window).resize(function () {
    $('.content').css({
        paddingTop: $('.header').outerHeight()
    });

    $('body').css({
        paddingBottom: $('footer').outerHeight()
    })
}).trigger('resize');

// Tel
if (!isMobile) {
    $('body').on('click', 'a[href^="tel:"]', function () {
        $(this).attr('href',
            $(this).attr('href').replace(/^tel:/, 'callto:'));
    });
}

$(function () {
    $('#navbar-collapse')
        .on('click', function (e) {
            $('body').toggleClass('menu-collapsed');
        });
});

$(function () {
    //caches a jQuery object containing the header element
    //var header = $(".header");
    //$(window).scroll(function () {
    //    var scroll = $(window).scrollTop();

    //    if (scroll >= 10) {
    //        header.addClass("fixed");
    //    } else {
    //        header.removeClass("fixed")
    //    }
    //});
});


//$('.open_hidden_content').on('click', function (ev) {
//    ev.preventDefault();
//    var $this = $(this), id = $this.attr('href');

//    $this.toggleClass('active');
//    $(id).slideToggle('fast');
//});

$(function () {
    var clipboard = new Clipboard('.copy_code', {
        text: function (trigger) {
            return trigger.previousElementSibling.innerHTML;
        }
    });

    clipboard.on('success', function (e) {
        e.trigger.innerHTML = '<i class="icon icon--check_thin"></i> Copied';
        e.clearSelection();
    });
});
