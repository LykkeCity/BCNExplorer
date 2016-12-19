$(function () {
    var loadedClass = "js-loaded";

    var loadPanel = function($target) {
        var $panel = $target.find('.js-panel-content');
        var $loader = $target.find('.js-panel-loader');

        if (!$panel.hasClass(loadedClass)) {
            var url = $panel.data('load-url');
            $loader.show();
            $panel.addClass(loadedClass);
            $panel.load(url, function () {
                $loader.hide();
                $panel.show();
                $panel.trigger('panel-loaded');
            });
        }
    };

    var initialPanelToShowSelector = window.location.hash ? window.location.hash : '#owners';
    $('[href="' + initialPanelToShowSelector + '"]').tab('show');
    loadPanel($(initialPanelToShowSelector));


    $('.js-tx-toggle').on('show.bs.tab', function (e) {
        var target = e.target;

        if (history.pushState) {
            history.pushState(null, null, target);
        }
        else {
            location.hash = target;
        }

        loadPanel($($(target).attr('href')));
    });

    $('body').on('panel-loaded', '#transactions', function (e) {
        $(e.target).find('.js-transactions-container').first().trigger('load-transactions');
    });


    $('body').on('click', '.js-change-go-to-block', function () {
        var block = $(this).data('block');
        var $input = $('#js-go-to-block');

        $input.val(block);
        $input.trigger('change');

        return false;
    });

    $('body').on('change', '.js-submit-on-change', function() {
        $(this).parents('form').submit();
    });

    $('body').on('submit', '.js-coinholders-history-form', function () {
        var $self = $(this);

        var data = $self.serialize();
        var url = $self.attr('action');

        var $panelToUpdate = $('#owners .js-panel-content');
        var $panelToHide = $('#owners .js-coinholders-data');
        var $loader = $('#owners .js-panel-loader');

        $loader.show();
        $panelToHide.hide();
        $.ajax(url, {
            data: data,
            method: 'get'
        }).done(function (resp) {
            $loader.hide();
            $panelToUpdate.html(resp);
        });
        return false;
    });
})