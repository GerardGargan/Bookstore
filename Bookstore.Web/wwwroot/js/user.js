$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: '/admin/user/getall',
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "25%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'company.name', "width": "10%" },
            { data: 'role', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/Admin/User/Upsert/${data}" class="btn btn-primary mx-1"><i class="bi bi-pencil-square"></i> Edit</a>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}