﻿$(function() {
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

    (function() {
        var initAssetQuantityPopover = function () {
            var $elem = $('[data-toggle="popover"]');

            $elem.popover('destroy');
            $elem.popover({
                trigger: 'hover',
                container: this.parentNode,
                placement: 'auto'
            });
        }

        initAssetQuantityPopover();

        $('body').on('transactions-loaded', initAssetQuantityPopover);
    })();    
});