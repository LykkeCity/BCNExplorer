$(function() {
    $('.open_hidden_content').on('click', function (ev) {
        ev.preventDefault();
        var $this = $(this), id = $this.attr('href');

        $this.toggleClass('active');
        $(id).slideToggle('fast');
    });
});