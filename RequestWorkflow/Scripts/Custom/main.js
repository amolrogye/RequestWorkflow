
var mainManager = (function () {

    var dateConverter = function (jsonDate) {
        var shortDate = null;
        if (jsonDate) {
            var regex = /-?\d+/;
            var matches = regex.exec(jsonDate);
            var dt = new Date(parseInt(matches[0]));
            var month = dt.getMonth() + 1;
            var monthString = month > 9 ? month : '0' + month;
            var day = dt.getDate();
            var dayString = day > 9 ? day : '0' + day;
            var year = dt.getFullYear();
            shortDate = dayString + '-' + monthString + '-' + year;
        }
        return shortDate;
    };

    var tblBuilder = function (parentId, headerList, data, isSrNo = true) {

        var tbl = document.createElement("table");

        var $head =
            `<thead>
                    <tr>`
        if (isSrNo) {
            $head += "<th>Sr No</th>";
        }
        $.each(headerList, function (i, v) {
            $head += `<th>${v.headerName}</th>`;
        });
        $head += `</tr></thead>`

        var $body = document.createElement("tbody");

        $("#" + parentId).append(tbl);
        $("#" + parentId + " table").append($head).addClass("table");

        $("#" + parentId + " table").append($body);

        $.each(data, function (i, v) {

            var rowElement = "<tr>";
            if (isSrNo) {
                rowElement += `<td>${i + 1}</td>`;
            };


            $.each(headerList, function (j, k) {

                if (v[k.actualName].indexOf('/Date(') == 0 && v[k.actualName].endsWith(")/")) {
                    rowElement += `<td>${dateConverter(v[k.actualName])}</td>`;
                } else {
                    rowElement += `<td>${v[k.actualName]}</td>`;
                }

                
            });

            rowElement += "</tr>";
            $("#" + parentId + " table tbody").append(rowElement);

        });

    }

    return {
        dateConverter: dateConverter,
        tblBuilder: tblBuilder
    }

})();