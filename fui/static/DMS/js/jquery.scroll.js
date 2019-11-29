// anchor in page
$(window).bind('load', function () {
    "use strict";
    $(function () {
        $('a[href^="#"]').click(function () {
            if ($($(this).attr('href')).length) {
                var p = $($(this).attr('href')).offset();
                if ($(window).width() > 991) {
                    $('html,body').animate({scrollTop: p.top}, 400);
                } else {
                    $('html,body').animate({ scrollTop: p.top - 80}, 400);
                }
            }
            return false;
        });
    });
});

// anchor top page #
$(window).bind('load', function () {
    "use strict";
    var hash = location.hash;
    if (hash) {
        var p1 = $(hash).offset();
        if ($(window).width > 991) {

            $('html,body').animate({scrollTop: p1.top}, 400);
        } else {
            $('html,body').animate({scrollTop: p1.top - 80}, 400);
        }
    }
});
