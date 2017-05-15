$(function() {
    $('body').on('click','.open_hidden_content',  function (ev) {
        ev.preventDefault();
        var $this = $(this), id = $this.attr('href');

        $this.toggleClass('active');
        $(id).slideToggle('fast');
    });

    $('body').on('click', '.js-toggle-change-btn', function() {
        var $self = $(this);

        $self.find('.js-toggle-change-label').toggleClass('hidden');
        $($self.data('toggle-container')).find('.js-toggle-change-item').toggleClass('hidden');

        return false;
    });

    (function () {
        var popoverSelector = '[data-toggle="popover"]';
        var initAssetQuantityPopover = function () {
            var $elem = $(popoverSelector);

            var is_touch_device = ("ontouchstart" in window) || window.DocumentTouch && document instanceof DocumentTouch;

            $elem.popover('destroy');
            $elem.popover({
                trigger: is_touch_device ? 'click' : 'hover',
                container: this.parentNode,
                placement: 'auto'
            });
        }

        initAssetQuantityPopover();

        $('body').on('transactions-loaded', initAssetQuantityPopover);

        $('body').on('click touchstart', function (e) {
            $(popoverSelector).each(function () {
                if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('[data-toggle="popover"]').has(e.target).length === 0) {
                    $(this).popover('hide');
                }
            });
        });
    })();    
});