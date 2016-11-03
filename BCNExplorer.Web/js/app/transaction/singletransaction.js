$(function() {
    $('body').on('click','.open_hidden_content',  function (ev) {
        ev.preventDefault();
        var $this = $(this), id = $this.attr('href');

        $this.toggleClass('active');
        $(id).slideToggle('fast');
    });
});