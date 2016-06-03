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

                        $('input[name="ResursPaymentMethod"]').val($(this).val());

                        $('input[name="MinLimit"]').val($(this).parent().parent().find('.payment-method__detail-container').attr('data-min'));
                        $('input[name="MaxLimit"]').val($(this).parent().parent().find('.payment-method__detail-container').attr('data-max'));

                    });
                })(allOptions.eq(i), i, allOptions);
            }
        }

        this.getGovermentId();
    },

    getGovermentId: function () {
        $('input[name="govid"]').on('change', function () {
            $('input[name="GovernmentId"]').val($(this).val());
        });
    }
};