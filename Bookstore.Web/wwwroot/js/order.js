$(document).ready(function () {
    let url = window.location.search;
    const statusTypes = ["inprocess", "pending", "completed", "approved"];

    const status = statusTypes.find(type => url.includes(type)) || "all";

    loadDataTable(status);
    
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        ajax: '/admin/order/getall?status='+status,
        "columns": [
            { data: 'id', "width": "5%" },
            { data: 'name', "width": "25%" },
            { data: 'phoneNumber', "width": "20%" },
            { data: 'applicationUser.email', "width": "20%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/Admin/order/details?orderId=${data}" class="btn btn-primary mx-1"><i class="bi bi-pencil-square"></i></a>`
                },
                "width": "10%"
            }
        ]
    });
}