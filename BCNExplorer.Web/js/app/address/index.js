$(function () {
    (function () {
        var $addressPanel = $('#js-address-transactions');
        var loadUrl = $addressPanel.data('load-url');

        $addressPanel.load(loadUrl, function () {
            $('.js-transactions-container.hidden:first').trigger('load-transactions');
        });
    })();

    var coloredAddress = $('#colored-address').val();
    var unColoredAddress = $('#uncolored-address').val();
    var currentAddress = $('#current-address').val();
    $('body').on('transactions-loaded', function() {
        $('[data-address="' + coloredAddress + '"],[data-address="' + unColoredAddress + '"],[data-address="' + currentAddress + '"]')
            .addClass('current-address-transaction');
    });


})