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

    $('body').on('click', '.js-tx-toggle', function () {
        var $self = $(this);
        var idToShow = $self.attr('href');
        var $btnGroup = $self.parents('.js-tx-toggle-container');
        var $panelToShow = $('#js-tx-select-result-container').find(idToShow);
        var $panelsToHide = $('#js-tx-select-result-container').find('.js-select-result').not(idToShow);

        $btnGroup.find('.js-tx-toggle').removeClass('active');
        $self.addClass('active');
        
        $panelToShow.find('.js-transactions-container').first().trigger('load-transactions');
        $panelsToHide.addClass('hidden');
        $panelToShow.removeClass('hidden');


        return false;
    });
})