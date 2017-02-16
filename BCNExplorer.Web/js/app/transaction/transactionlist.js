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

                    var $cath = $loadContainer.parents('.js-transaction-cathegory');
                    $cath.find('.js-loaded-tx-count').html($cath.find('.js-transaction-details').length);
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
        $($selfBtn).parent().addClass('hidden');
        var $cat = $selfBtn.parents('.js-transaction-cathegory');
        loadTransactions($selfBtn.parents(transactionLoadContainerSelector)).done(function () {
            if ($cat.find('.js-load-trans:visible').length == 0) {
                $cat.find(nextLoadTranContainerSelector).removeClass('hidden');
            }
        });
    });

    $('body').on('click', '.js-tx-toggle', function () {
        var $self = $(this);
        var idToShow = $self.attr('href');
        var $btnGroup = $self.parents('.js-tx-toggle-container');
        var $panelToShow = $('#js-tx-select-result-container').find(idToShow);
        var $panelsToHide = $('#js-tx-select-result-container').find('.js-select-result').not(idToShow);

        $btnGroup.find('.js-tx-toggle').parent('.js-tab_item').removeClass('tab_item--active');
        $self.parent('.js-tab_item').addClass('tab_item--active');

        $panelToShow.find('.js-transactions-container').first().trigger('load-transactions');
        $panelsToHide.addClass('hidden');
        $panelToShow.removeClass('hidden');

        return false;
    });
});