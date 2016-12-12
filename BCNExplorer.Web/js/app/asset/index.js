$(function() {
    var $initialLoadPanel = $('#js-load-panels').find('[data-load-url]:visible').first();
    var $loader = $('#js-panel-loader');
    $initialLoadPanel.load($initialLoadPanel.data('load-url'), function() {
        $loader.hide();
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
        });

        return false;
    });
})