$.fn.contains = function (target) {
    var result = null;
    $(this).each(function (index, item) {
        if (item === target) {
            result = item;
        }
    });
    return result ? result : false;
}



var NormalizeDate = function (datePart) {
    if (datePart < 10) {
        return "0" + datePart;
    }

    return datePart.toString();
}

var EventID = function () {

}

$(function () {
    var dates = [];
    for (var i = 0; i < calendarData.length; i++) {
        dates.push(calendarData[i].Datetime)
    }

    $('.calendar .inner').datepicker({
        inline: true,
        beforeShowDay: function (date) {
            var theday = NormalizeDate(date.getDate()) + '/' + NormalizeDate(date.getMonth() + 1) + '/' + date.getFullYear();
            var dashday = NormalizeDate(date.getDate()) + '-' + NormalizeDate(date.getMonth() + 1) + '-' + date.getFullYear();

            for (var i = 0; i < calendarData.length; i++) {
                var obj = calendarData[i];
                if (obj.Datetime == theday || obj.Datetime == dashday) {
                    return [true, "hasevent event_" + obj.Url, obj.Title];
                }
            }

            return [true, ''];

        },
        onSelect: function (dateText, inst) {
            var classes = $('td[data-year="' + inst.selectedYear + '"][data-month="' + inst.selectedMonth + '"] a:contains("' + inst.selectedDay + '")').first().parent().attr('class').split(' ');
            var url = "";
            for (var i = 0; i < classes.length; i++) {
                if (classes[i].indexOf('event_') > -1) {
                    url = classes[i].replace('event_', "");
                    window.location.href = "/events/" + url;
                }
            }
        }
    });
});