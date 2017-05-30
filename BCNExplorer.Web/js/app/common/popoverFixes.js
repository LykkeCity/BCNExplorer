$(function () {
    //hide popover on click outside
    //https://stackoverflow.com/questions/20466903/bootstrap-popover-hide-on-click-outside
    $('body').on('click touchstart', function (e) {
        $('[data-toggle=popover]').each(function () {
            if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('[data-toggle=popover]').has(e.target).length === 0) {
                $(this).popover('hide');
            }
        });
    });
})