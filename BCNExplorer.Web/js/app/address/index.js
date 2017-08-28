$(function () {
    //set tx count in dom
    (function () {
        var txCount = undefined;
        var retrieveTxCount = function () {
            var $elem = $('#js-total-transactions');
            if ($elem.length !== 0) {
                txCount = $elem.val();
            }
        };

        var putTxCountInDom = function () {
            if (txCount != undefined) {
                var $tabs = $('.js-tx-toggle-container');
                var $allTxs = $('#js-all-tx'); //txs in "All" tab
                var $context = $tabs.add($allTxs);

                $('.js-tx-count', $context).html(txCount);
                $('.js-tx-count-container', $context).removeClass('hidden');
            }
        }

        $('body').one('tx-history-loaded', function () {
            putTxCountInDom();
        });

        $('body').one('balance-loaded', function () {
            retrieveTxCount();
            putTxCountInDom();
        });
    })();

    //Load tx history
    (function () {
        var $addressPanel = $('#js-address-transactions');
        var loadUrl = $addressPanel.data('load-url');

        $addressPanel.load(loadUrl, function () {
            $addressPanel.trigger('tx-history-loaded');
            $('.js-transactions-container.hidden:first').trigger('load-transactions');
        });
    })();

    //Balance
    (function () {
        var $loadContainer = $('#js-balance-load-contaner');
        var $loader = $('#js-balance-loader');
        var loadUrl = $loadContainer.data('load-url');

        var loadBalance = function () {
            $.ajax(loadUrl).done(function (resp) {
                $loadContainer.html(resp);
                
                $loadContainer.trigger('balance-loaded');
                $loader.hide();
            });
        };

        loadBalance();
    })();
    
    //Balance History go to time
    (function () {
        var init = function () {
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

            var submit = function () {
                $time.data("DateTimePicker").destroy();
                $date.data("DateTimePicker").destroy();
                var url = $('#js-go-to-block-time-url').val();

                var $loader = $('#js-balance-loader');
                var $panelToUpdate = $('#js-balance-load-contaner');
                var $panelToHide = $('#js-balance-data');

                var date = moment.utc($date.val(), dateFormat);
                var time = moment($time.val(), timeFormat);
                var fullDate = date;
                fullDate.add(time.get('hour'), 'hour');
                fullDate.add(time.get('minute'), 'minute');


                $('.js-set-readonly-on-submit').attr('readonly', true);
                $('.js-change-go-to-block').attr('disabled', true);
                $loader.show();
                $panelToHide.hide();
                //alert(fullDate.utc().format());
                $.ajax(url, {
                    data: { at: fullDate.utc().format() },
                    async: true
                }).done(function (resp) {
                    $panelToUpdate.html(resp);
                    $loader.hide();
                    $panelToUpdate.trigger('balance-loaded');
                });
            }

            $date.on('dp.change', function() {
                submit();
            });

            $time.on('dp.change', $.debounce(1500, function () {
                submit();
            }));
        }

        $('body').on('balance-loaded', function () {
            init();
        });
    })();

    //Balance by height
    (function () {
        var init = function () {
            var submit = function (height) {
                var url = $('#js-go-to-block-height-url').val();

                var $loader = $('#js-balance-loader');
                var $panelToUpdate = $('#js-balance-load-contaner');
                var $panelToHide = $('#js-balance-data');

                $('.js-set-readonly-on-submit').attr('readonly', true);
                $('.js-change-go-to-block').attr('disabled', true);
                $loader.show();
                $panelToHide.hide();

                $.ajax(url, {
                    data: { at: height },
                    async: true
                }).done(function (resp) {
                    $panelToUpdate.html(resp);
                    $loader.hide();
                    $panelToUpdate.trigger('balance-loaded');
                });
            };

            $('.js-change-go-to-block').on('click', function () {
                var $self = $(this);

                if (!$self.attr('disabled')) {
                    var height = $self.data('block');
                    submit(height);
                }


                return false;
            });

            $('#js-go-to-block').on('keyup', $.debounce(1500, function () {
                submit($(this).val());
            }));
        };

        $('body').on('balance-loaded', function () {
            init();
        });
    })();

    //Highlight address
    (function () {
        var coloredAddress = $('#colored-address').val();
        var unColoredAddress = $('#uncolored-address').val();
        var currentAddress = $('#current-address').val();
        $('body').on('transactions-loaded', function () {
            $('[data-address="' + coloredAddress + '"],[data-address="' + unColoredAddress + '"],[data-address="' + currentAddress + '"]')
                .addClass('current-address-transaction');
        });
    })();
})