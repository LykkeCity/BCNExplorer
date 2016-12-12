$(function() {
    var $initialLoadPanel = $('#js-load-panels').find('[data-load-url]:visible').first();
    var loadedClass = "js-loaded";

    var $loader = $('#js-panel-loader');
    $initialLoadPanel.load($initialLoadPanel.data('load-url'), function() {
        $loader.hide();
        $initialLoadPanel.addClass(loadedClass);
    });

    $('body').on('click', '.js-load-panel-loader', function () {
        var $self = $(this);
        var url = $self.attr('href');
        var $panel = $($self.data('load-container'));

        $loader.show();
        $panel.hide();
        $panel.load(url, function () {
            $loader.hide();
            $panel.show();
            $panel.addClass(loadedClass);
        });

        return false;
    });

    $('body').on('click', '.js-tx-toggle', function () {
        var $self = $(this);
        var idToShow = $self.attr('href');
        var $btnGroup = $self.parents('.js-tx-toggle-container');
        var $panelToShow = $('#js-load-panels').find(idToShow);
        var $panelsToHide = $('#js-load-panels').find('.js-load-panel').not(idToShow);

        $btnGroup.find('.js-tx-toggle').removeClass('active');
        $self.addClass('active');


        $panelsToHide.addClass('hidden');
        $panelToShow.removeClass('hidden');

        if (!$panelToShow.hasClass(loadedClass)) {
            $loader.show();

            var url = $panelToShow.data('load-url');
            $panelToShow.hide();
            $panelToShow.load(url, function () {
                $loader.hide();
                $panelToShow.show();
                $panelToShow.addClass(loadedClass);

                $panelToShow.trigger('toggle-loaded');
            });
        }

        return false;
    });

    $('body').on('toggle-loaded', '#js-asset-transactions', function (e) {
        $(e.target).find('.js-transactions-container').first().trigger('load-transactions');
    });
})