$(function () {
    //Load tx history
    (function () {
        var $addressPanel = $('#js-address-transactions');
        var loadUrl = $addressPanel.data('load-url');

        $addressPanel.load(loadUrl, function () {
            $('.js-transactions-container.hidden:first').trigger('load-transactions');
        });
    })();
    
    //Balance
    (function () {
        var $loadContainer = $('#js-balance-load-contaner');
        var $loader = $('#js-balance-loader');
        var loadUrl = $loadContainer.data('load-url');

        var loadBalance = function() {
            $.ajax(loadUrl).done(function (resp) {
                $loadContainer.html(resp);
                $loadContainer.trigger('last-block-balance-loaded');
                $loader.hide();
            });
        };

        loadBalance();
    })();

    //Balance History go to time
    (function() {
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

            var submitData = function () {
                alert('submitData');

            }

            $time.add($date).on('dp.change', $.debounce(1500, function () {
                submitData();
            }));
        }

        $('body').on('last-block-balance-loaded', function () {
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