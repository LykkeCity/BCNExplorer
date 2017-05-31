$(function () {
    var selectors = {
        offchainPage: '.js-offchain-page',
        loader: '#js-offchain-page-loader',
        showMoreBtn: '.js-load-offchain-page',
        channel: '.js-offchain-channel',
        loadedChannelCount: '.js-offchain-loaded-channel-count'
    };


    (function() {
        var loadOffchainPage = function ($container) {
            if ($container.length === 0) {
                return;
            }
            $(selectors.loader).removeClass('hidden');

            var loadUrl = $container.data('load-url');
            var loadedClass = "js-offchain-loaded";

            if (!$container.hasClass(loadedClass)) {

                $.ajax(loadUrl).success(function(resp) {
                    $(selectors.loader).addClass('hidden');
                    $container.html(resp)
                    $container.removeClass('hidden');
                    $container.next().removeClass('hidden'); // show Load more btn on next page"

                    $container.trigger('transactions-loaded');
                    $container.addClass(loadedClass);
                    $(selectors.loadedChannelCount).html($(selectors.channel).length)
                });  
            }
        };

        $('body').on('click', selectors.showMoreBtn, function () {
            var $self = $(this);
            $self.addClass('hidden');

            var $container = $self.parents(selectors.offchainPage);
            loadOffchainPage($container);
        });

        $('body').on('js-tx-tab-toggled address-transaction-list-loaded', function (e) {
            loadOffchainPage($(e.target).find(selectors.offchainPage).first());
        });
    })();

})