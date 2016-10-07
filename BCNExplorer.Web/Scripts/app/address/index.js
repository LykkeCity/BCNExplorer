$(function () {
    var coloredAddress = $('#colored-address').val();
    var unColoredAddress = $('#uncolored-address').val();
    var currentAddress = $('#current-address').val();
    $('body').on('transactions-loaded', function() {
        $('[data-address="' + coloredAddress + '"],[data-address="' + unColoredAddress + '"],[data-address="' + currentAddress + ']"')
            .addClass('current-address-transaction');
    });
})