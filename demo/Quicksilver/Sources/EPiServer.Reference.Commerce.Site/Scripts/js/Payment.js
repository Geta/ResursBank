var Payment = {
    init: function () {
        var allOptions = $('.payment-method__option-item .radio__container .radio__input');
        if (allOptions && allOptions.length > 0) {
            for (var i = 0; i < allOptions.length; i++) {
                (function (option, index, allOptions) {
                    option.on('click', function () {
                        for (var i = 0; i < allOptions.length; i++) {
                            if (i != index) {
                                var detail = allOptions.eq(i).parent().parent().children('.payment-method__detail-container');
                                if (detail && detail.length > 0 && detail.css('display') != 'none') {
                                    detail.slideUp(250);
                                }
                            }
                        }

                        var detail = $(this).parent().parent().children('.payment-method__detail-container');
                        if (detail && detail.length > 0 && detail.css('display') == 'none') {
                            detail.slideDown(250);
                        }
                    });
                })(allOptions.eq(i), i, allOptions);
            }
        }
    }
};