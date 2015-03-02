$(function () {
    $('.tabs ul li a').click(function (e) {
        e.preventDefault();
        var index = $(this).parent().index();
        $(this).parent().parent().find('li').removeClass('active');
        $(this).parent().addClass('active');
        $('div.main').hide();
        $('div.main:eq(' + index + ')').show();
    });

    $('#add-new-announcement').ajaxForm({
        dataType: "json",
        success: function (data) {
            if (data.success == true) {
                window.location.reload();
            }
            else {
                alert(data.message);
            }
        }
    });

    $('ul.user-menu > li > a').click(function (e) {
        e.preventDefault();
        $(this).parent().parent().find('li').removeClass('active');
        $(this).parent().addClass('active');
        var id = $(this).attr('id');
        $('.content .list').hide();
        $('.content .list.' + id).show();
    });

    $('body').on('click', '.list.remind-password input[type=submit]', function (e) {
        e.preventDefault();
        $.post("Profile/ChangePassword", { oldpass: $('#user-old-pass').val(), newpass: $('#user-new-pass').val(), confirmpass: $('#user-confirm-pass').val() }, function (data) {
            var form = $('.list.remind-password');
            if (data.success == true) {
                form.find('.row').remove();
                form.append(data.message);
            }
            else {
                form.find('div.error').html('&nbsp;');
                for (var i = 0; i < data.errors.length; i++) {
                    var item = data.errors[i];
                    console.log(form.find('#' + item.field).parents('div.field')/*.find('div.error')*/)
                    form.find('#' + item.field).parents('div.field').find('div.error').html(item.error);
                }
            }
        }, 'json');
    });
//});

//$('#summary').click(function (e) {
//    e.preventDefault();

//    $.post("/Profile/Summary", function (data) {
//        if (data.success == true) {
//            $('div.list').empty();
//            $('div.list').append(data.text);
//        }
//        else {
//            alert(data.text);
//        }
//    });
//});

function AjaxError() {
    alert("Client Error");
}

function SubmitSuccesful(result, statusText) {
    if (result.success == true) {
        window.location.href = '?tab=2'
        //window.location.reload();
    }
    else {
        alert(result.message);
    }

}

});