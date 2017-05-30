$(function () {
    var selectors = {
        offchainPage: '.js-offchain-page',
        loader: '#js-offchain-page-loader',
        showMoreBtn: '.js-load-offchain-page'
    };


    (function() {
        var loadOffchainPage = function ($container) {
            if ($container.length == 0) {
                return;
            }
            $(selectors.loader).removeClass('hidden');

            var loadUrl = $container.data('load-url');
            $container.load(loadUrl, function() {
                $(selectors.loader).addClass('hidden');
                $container.removeClass('hidden');
                $container.next().removeClass('hidden'); // show Load more btn on next page"
            });
        };

        $('body').on('click', selectors.showMoreBtn, function() {
            var $container = $(this).parents(selectors.offchainPage);
            loadOffchainPage($container);
        });

        $('body').on('address-transaction-list-loaded', function () {
            loadOffchainPage($(selectors.offchainPage).first());
        });
    })();

})