$(function() {
    var loadTransactions = function($loadContainer) {
        var transactionIds = $loadContainer.data('transaction-ids');
        var loadUrl = $loadContainer.data('load-url');
        if (transactionIds != undefined && loadUrl != undefined) {
            $loadContainer.hide().removeClass('hidden').fadeIn('slow');
            $.ajax(loadUrl, {
                data: {
                    ids: transactionIds
                },
                method: 'post'
            }).done(function(resp) {

                $loadContainer.removeClass('transactions-container-load').html(resp);
            });
        }
    }

    var pagination = {
        renderItem: function(page, isActive) {
            var a = $('<a>');
            a.addClass('transaction-pagination-pointer');
            a.attr('href', '#');
            a.attr('data-page', page);
            a.html(page);

            var li = $('<li>');
            if (isActive) {
                li.addClass('active');
            }

            li.append(a);

            var wrapper = $('<div>');
            wrapper.append(li);
            return wrapper.html();
        },
        totalPages: function() {
            return pagination.$container().data('total-pages');
        },
        $container: function() {
            return $('.transaction-pagination-container');
        },
        render: function(currentPage) {
            var $container = pagination.$container();
            $container.empty();
            $container.hide();

            var totalPages = pagination.totalPages();

            var showPagesArray = pagination.getShowPagesArray(currentPage, totalPages, 2);

            var currentlyShowing = true;
            for (var pageCursor = 1; pageCursor <= totalPages; pageCursor++) {
                var $item = pagination.renderItem(pageCursor, currentPage === pageCursor);

                if (showPagesArray.includes(pageCursor)) {
                    $container.append($item);
                    currentlyShowing = true;
                } else {
                    if (currentlyShowing) {
                        var dotItem = pagination.renderItem('...', false);
                        $container.append(dotItem);
                    }
                    currentlyShowing = false;
                }
            }

            $container.show();
        },
        getShowPagesArray: function(currentPage, lastPage, nearItemsCount) {

            var startPage = 1;
            nearItemsCount--;

            var nearStartNumbersArray = [];
            for (var i = startPage; i <= (startPage + nearItemsCount); i++) {
                nearStartNumbersArray.push(i);
            }

            var nearEndNumberArray = [];

            for (var j = (lastPage - nearItemsCount); j <= lastPage; j++) {
                nearEndNumberArray.push(j);
            }

            var nearCurrentPageArray = [];

            for (var k = currentPage - nearItemsCount; k <= currentPage + nearItemsCount; k++) {
                nearCurrentPageArray.push(k);
            }

            var result = nearStartNumbersArray.concat(nearEndNumberArray).concat(nearCurrentPageArray);

            result = $.grep(result, function(item) {
                return item >= startPage && item <= lastPage;
            });

            return result.getUnique();
        }
    }

    pagination.render(1);
    loadTransactions($('.transactions-container:visible'));

    $('body').on('click', '.transaction-pagination-pointer', function() {
        var page = $(this).data('page'),
            $loadContainer = $('.transactions-container[data-page=' + page + ']').not('transactions-container-load');

        pagination.render(page);
        
        $('.transactions-container').not('[data-page=' + page + ']').addClass('hidden');

        loadTransactions($loadContainer);
        return false;
    });
});

Array.prototype.getUnique = function () {
    var u = {}, a = [];
    for (var i = 0, l = this.length; i < l; ++i) {
        if (u.hasOwnProperty(this[i])) {
            continue;
        }
        a.push(this[i]);
        u[this[i]] = 1;
    }
    return a;
}