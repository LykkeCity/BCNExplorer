$(function() {
    $('body').on('click', '.js-toggle-aggregated', function () {
        $(this).parents('.inputs,.outputs').find('.aggregated-item-container').slideToggle();
    });
});