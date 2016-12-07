$(function() {
    var loadTransactions = function ($loadContainer) {
        var loadedClass = 'js-transaction-container-loaded';
        if (!$loadContainer.hasClass(loadedClass)) {
            var transactionIds = $loadContainer.data('transaction-ids');
            var loadUrl = $loadContainer.data('load-url');
            var $loader = $('.js-loader');
            if (transactionIds != undefined && loadUrl != undefined) {
                $loader.show();
                $loadContainer.hide().removeClass('hidden').fadeIn('slow');

                return $.ajax(loadUrl, {
                    data: {
                        ids: transactionIds
                    },
                    method: 'post'
                }).done(function (resp) {
                    $loader.hide();
                    $loadContainer.html(resp);
                    $loadContainer.trigger('transactions-loaded');
                    $loadContainer.addClass(loadedClass);
                });
            }
        }
        
        return $.Deferred().promise();
    }

    var nextLoadTranContainerSelector = '.js-transactions-container.hidden:first';
    var transactionLoadContainerSelector = '.js-transactions-container';

    loadTransactions($(nextLoadTranContainerSelector)).done(function () {
        $(nextLoadTranContainerSelector).removeClass('hidden');
    });

    $('body').on('load-transactions', function (e) {
        var $target = $(e.target);
        loadTransactions($target).done(function () {
            var $cat = $target.parents('.js-transaction-cathegory');
            if ($cat.find('.js-load-trans:visible').length == 0) {
                $cat.find(nextLoadTranContainerSelector).removeClass('hidden');
            }
        });
    });

    $('body').on('click', '.js-load-trans', function () {
        var $selfBtn = $(this);
        $($selfBtn).addClass('hidden');
        var $cat = $selfBtn.parents('.js-transaction-cathegory');
        loadTransactions($selfBtn.parents(transactionLoadContainerSelector)).done(function () {
            if ($cat.find('.js-load-trans:visible').length == 0) {
                $cat.find(nextLoadTranContainerSelector).removeClass('hidden');
            }
        });
    });
});