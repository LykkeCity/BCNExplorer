$(function () {
    var loadedClass = "js-loaded";

    var loadPanel = function ($target) {
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


    $('.js-tx-toggle-tab').on('show.bs.tab', function (e) {
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


    var submitGoToBlock = function () {
        $('.js-set-readonly-on-submit').attr('readonly', true);
        $('.js-change-go-to-block').attr('disabled', true);

        var at = $('#js-go-to-block').val();
        var helpData = $.parseJSON($('#js-submit-go-to-block-height').val());

        var submitData = {
            at: at
        };

        var $panelToUpdate = $('#owners .js-panel-content');
        var $panelToHide = $('#owners .js-coinholders-data');
        var $loader = $('#owners .js-panel-loader');

        $loader.show();
        $panelToHide.hide();
        $.ajax(helpData.url, {
            data: submitData,
            method: 'get'
        }).done(function (resp) {
            $loader.hide();
            $panelToUpdate.html(resp);

            $panelToUpdate.trigger('owners-data-loaded');
            $('#js-go-to-block').focus();
        });
    }

    $('body').on('click', '.js-change-go-to-block', function () {
        var $self = $(this);

        if (!$self.attr('disabled')) {
            var block = $self.data('block');
            var $input = $('#js-go-to-block');

            $input.val(block);
            submitGoToBlock();
        }

        return false;
    });

    $('body').on('keyup', '#js-go-to-block', $.debounce(1500, function () {
        submitGoToBlock();
    }));

    (function () {
        var initDatapickers = function () {
            var dateFormat = 'DD.MM.YYYY';
            var timeFormat = 'HH:mm';

            var $date = $('#datetimepicker');
            var $time = $('#timepicker');

            $date.datetimepicker({
                format: dateFormat,
                icons: {
                    time: "icon--clock",
                    date: "icon--cal",
                    up: "icon--chevron-thin-up",
                    down: "icon--chevron-thin-down",
                    previous: "icon--chevron-thin-left",
                    next: "icon--chevron-thin-right"
                }
            });

            $time.datetimepicker({
                format: timeFormat,
                icons: {
                    time: "icon--clock",
                    date: "icon--cal",
                    up: "icon--chevron-thin-up",
                    down: "icon--chevron-thin-down",
                    previous: "icon--chevron-thin-left",
                    next: "icon--chevron-thin-right"
                }
            });

            var submitData = function () {
                $time.data("DateTimePicker").destroy();
                $date.data("DateTimePicker").destroy();
                var url = $.parseJSON($('#js-submit-go-to-block-time').val()).url;

                var date = moment.utc($date.val(), dateFormat);
                var time = moment($time.val(), timeFormat);
                var fullDate = date;
                fullDate.add(time.get('hour'), 'hour');
                fullDate.add(time.get('minute'), 'minute');

                $('.js-set-readonly-on-submit').attr('readonly', true);
                $('.js-change-go-to-block').attr('disabled', true);
                var submitData = {
                    at: fullDate.utc().format()
                };

                var $panelToUpdate = $('#owners .js-panel-content');
                var $panelToHide = $('#owners .js-coinholders-data');
                var $loader = $('#owners .js-panel-loader');

                $loader.show();
                $panelToHide.hide();
                $.ajax(url, {
                    data: submitData,
                    method: 'get'
                }).done(function (resp) {
                    $loader.hide();
                    $panelToUpdate.html(resp);

                    $panelToUpdate.trigger('owners-data-loaded');
                });

            }

            $date.on('dp.change', function () {

                submitData();
            });

            $time.on('dp.change', $.debounce(1500, function () {

                submitData();
            }));
        }



        $('body').on('panel-loaded owners-data-loaded', '#owners', function (e) {
            initDatapickers();
        });
    })();
})