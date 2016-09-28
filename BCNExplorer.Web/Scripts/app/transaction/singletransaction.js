$(function() {
    $('body').on('click', '.js-toggle-aggregated', function () {
        var $container = $(this).parents('.js-aggregated-transactions-container');
        $container.find('.hidden-transactions').slideToggle();
        $container.find('.toggle-indicatior').toggleClass('glyphicon-triangle-bottom').toggleClass('glyphicon-triangle-top');
    });
});