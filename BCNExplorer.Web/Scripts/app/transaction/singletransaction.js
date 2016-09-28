$(function() {
    $('body').on('click', '.js-toggle-aggregated', function () {
        var address = $(this).data('address');
        $(this).parents('.js-aggregated-transactions-container').find('.hidden-transactions').slideToggle();
    });
});