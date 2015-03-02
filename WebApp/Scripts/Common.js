jQuery.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", Math.max(0, (($(window).height() - $(this).outerHeight()) / 2) +
                                                $(window).scrollTop()) + "px");
    this.css("left", Math.max(0, (($(window).width() - $(this).outerWidth()) / 2) +
                                                $(window).scrollLeft()) + "px");
    return this;
}

var ShowModal = function () {
    if ($('div.overlay').length == 0) {
        $('body').append('<div class="overlay"></div><div class="lightbox"></div>')
    }

    $('div.overlay').fadeIn('fast', function () { $('.lightbox').center(); });
}

var HideModal = function () {
    $('div.overlay').fadeOut('fast');
}

$(function () {

    $('#logout').click(function (e) {
        e.preventDefault();
        $.get("/Profile/LogOut", function (data) {
            if (data.success == true) {
                window.location.reload();
            }
        });
    });

    $('body').on('click', 'a.lightbox-close, .overlay', function (e) {
        e.preventDefault();
        $('.lightbox').fadeOut('fast');
        HideModal('fast');
        if ($('.calendar .inner').hasClass('open')) {
            $('.calendar .hover').trigger('click');
        }
    });

    $('body').on('click', '.general .main .comments.announcements ul li .announcement-title > a', function (e) {
        e.preventDefault();
        $('.announcement-title').removeClass('open')
        $(this).parent().addClass('open');
    });

    $('body').on('click', '.calendar .hover', function () {

        if (!$(this).parent().find('.inner').hasClass('open')) {

            if ($('div.overlay').length == 0) {
                $('body').append('<div class="overlay"></div>')
            }

            $('.overlay').show();
        } else {
            $('.overlay').hide();
            $('div.overlay').remove();
        }

        $(this).parent().find('.inner').toggleClass('open');

    });
});