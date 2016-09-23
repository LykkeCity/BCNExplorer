$(function() {
    var loadTransactions = function($loadContainer) {
        var transactionIds = $loadContainer.data('transaction-ids');
        var loadUrl = $loadContainer.data('load-url');
        var loadingClass = 'transactions-container-load';
        if (transactionIds != undefined && loadUrl != undefined) {
            $loadContainer.hide().addClass(loadingClass).removeClass('hidden').fadeIn('slow');
            return $.ajax(loadUrl, {
                data: {
                    ids: transactionIds
                },
                method: 'post'
            }).done(function(resp) {
                $loadContainer.removeClass(loadingClass).html(resp);
            });
        }

        return $.Deferred().promise();
    }

    var nextLoadTranContainerSelector = '.js-transactions-container.hidden:first';
    var transactionLoadContainerSelector = '.js-transactions-container';

    loadTransactions($(nextLoadTranContainerSelector)).done(function () {
        $(nextLoadTranContainerSelector).removeClass('hidden');
    });

    $('body').on('click', '.js-load-trans', function () {
        var $selfBtn = $(this);
        $($selfBtn).addClass('hidden');
        loadTransactions($selfBtn.parents(transactionLoadContainerSelector)).done(function () {
            $(nextLoadTranContainerSelector).removeClass('hidden');
        });
    });
});