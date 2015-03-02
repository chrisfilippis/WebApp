jQuery.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", Math.max(0, ($(window).scrollTop() + 100)) + "px");
    //this.css("left", Math.max(0, (($(window).width() - $(this).outerWidth()) / 2) + $(window).scrollLeft()) + "px");
    return this;
}

// Create (if not exists) and show overlay
var ShowOverlay = function () {
    if ($('div.overlay').length == 0) {
        $('body').append('<div class="overlay"></div>')
    }
    $('div.overlay').fadeIn('fast');
}

// Hide overlay
var HideOverlay = function () {
    $('div.overlay').fadeOut('fast');
}

// Create (if not exists) and show modal
var ShowModal = function () {
    if ($('div.overlay').length == 0) {
        $('body').append('<div class="overlay"></div>');
    }

    if ($('div.lightbox').length == 0) {
        $('body').append('<div class="lightbox"></div>');
    }

    $('.lightbox').center();
    $('div.overlay').fadeIn('fast');
}

// Hide modal
var HideModal = function () {
    $('.lightbox').fadeOut('fast');
    $('div.overlay').fadeOut('fast');
}

// Show submenu item content
var ShowTab = function (li, url) {
    if (typeof url !== "string") {
        url = '';
    }
    var elm = li.find('a');
    var title = elm.html();

    var action = url == '' ? elm.data('view') : url;
    ShowOverlay();
    $.get(action, function (data) {
        elm.parents('.general').find('.main').html(data);
        elm.parent().parent().find('li').removeClass('active');
        elm.parent().addClass('active');
        $('.page-title').html(title);
        HideOverlay();
    }, 'html');
}

$(function () {

    $('body').on('click', '.lightbox span.add-project-users a', function (e) {
        e.preventDefault();
        $(this).parent().find('select').show();
        $(this).parent().find('.save-users').show();
    });

    //Show User Dropdown
    $('body').on('click', 'a.delete-img', function () {
        alert('delete')
    });

    $('body').on('click', '.lightbox span.add-project-users select option', function () {
        if ($(this).index() == 0) {
            $(this).parent().trigger.change()
        }
    });

    $('body').on('click', '.lightbox span.add-project-users .save-users', function (e) {
        e.preventDefault();
        var select = $(this).parent().find('select');
        var prid = $(this).parent().data('id');
        $.post("Administration/AddProjectUser/" + prid, { uid: select.find('option:selected').val() }, function (data) {
            var action = $('.lightbox > form > div').data('after-remove');
            $('.lightbox').fadeOut('fast', function () {
                $('.lightbox').empty();
                $.get(action, function (data) {
                    ShowModal();
                    $('.lightbox').empty();
                    $('.lightbox').append(data).show();
                }, 'html');
            });
        }, 'json');
    });

    //Delete Users From Edit Form
    $('body').on('click', '#edit-project div.field span a.delete', function (e) {
        e.preventDefault();
        if (confirm('Είσαι σίγουρος ότι θέλεις να διαγραφεί αυτός ο χρήστης')) {
            var elm = $(this).find('img');
            $.post("Administration/RemoveProjectUser/" + elm.data('prid'), { uid: elm.data('id') }, function (data) {
                var action = $('.lightbox > form > div').data('after-remove');
                $('.lightbox').fadeOut('fast', function () {
                    $('.lightbox').empty();
                    $.get(action, function (data) {
                        ShowModal();
                        $('.lightbox').empty();
                        $('.lightbox').append(data).show();
                    }, 'html');
                });
            }, 'json');
        }
    });

    // Tab Click
    $('body').on('click', '.admin-tabs li a', function (e) {
        e.preventDefault();
        var elm = $(this).parent();
        ShowTab(elm);
    });

    // Add new User, Announcement, Project
    $('body').on('click', '.invite .invite-btn', function (e) {
        e.preventDefault();
        var action = $(this).data('view');
        var lightboxForm = $('.admin-list').data('type') == 'projects' ? $('#add-project') : $('#add-user');
        if (lightboxForm.length == 0) {
            $.get(action, function (data) {
                ShowModal();
                $('.lightbox').empty();
                $('.lightbox').append(data).show();
                $('.lightbox #Datetime, .lightbox #StartDate, .lightbox #EndDate').each(function () {
                    console.log($(this))
                    $(this).datepicker({
                        inline: true,
                        dateFormat: "dd/mm/yy"
                    });
                });
            }, 'html');
        } else {
            ShowModal();
            $('.lightbox').show();
        }
    });

    // Edit User, Announcement, Project
    $('body').on('click', '.admin-list a.edit', function (e) {
        e.preventDefault();
        var elm = $(this);
        var action = elm.data('view');
        $.get(action, function (data) {
            ShowModal();
            $('.lightbox').empty();
            $('.lightbox').append(data).show();
            //$('.lightbox #Datetime').datepicker({
            //    inline: true,
            //    dateFormat: "dd/mm/yy"
            //});
            $('.lightbox #Datetime, .lightbox #StartDate, .lightbox #EndDate').each(function () {
                console.log($(this))
                $(this).datepicker({
                    inline: true,
                    dateFormat: "dd/mm/yy"
                });
            });
        }, 'html');
    });

    // Remove User, Project or Announcement
    $('body').on('click', 'div.page a.delete', function (e) {
        e.preventDefault();
        if (confirm('Are you sure you want to delete this user from database?')) {
            var elm = $(this);
            $.get(elm.attr('href'), function (data) {
                if (data.success == true) {
                    var elm = $('.admin-tabs li.active');
                    ShowTab(elm);
                }
                else {
                    alert(data.message);
                }
            });
        }
    });

    //Pager
    $('body').on('click', 'ul.pager li a', function (e) {
        e.preventDefault();
        var elm = $('.admin-tabs li.active');
        ShowTab(elm, elm.find('a').data('view') + "/?pg=" + $(this).data('index'));
    });

    $('body').on('click', '.calendar a[data-editid != ""]', function (e) {
        e.preventDefault();
        console.log('dddd')
        var id = $(this).data('editid');
        $('#' + id).click();
    });

});