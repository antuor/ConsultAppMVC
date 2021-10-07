var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/Home/getall/",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "userName", "width": "20%" },
            { "data": "email", "width": "20%" },
            { "data": "email", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                        <a class='btn btn-success text-white compItem' href="/Home/ChatModal/2"'>
                            Создать консультацию
                        </a>
                        </div>`;
                }, "width": "20%"
            }
        ],
        "language": {
            "emptyTable": "no data found"
        },
        "width": "100%"
    });
}
