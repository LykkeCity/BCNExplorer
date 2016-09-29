$(function () {
    var currentAddress = $('#current-address').val();
    var currentColoredAddress = $('#current-colored-address').val();
    $('body').on('transactions-loaded', function() {
        $('[data-address=' + currentAddress + '],[data-address=' + currentColoredAddress + ']')
            .addClass('current-address-transaction');
    });
})