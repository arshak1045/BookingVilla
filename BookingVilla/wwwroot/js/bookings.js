var dataTable;

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status')

    loadDataTable(status);
})

function loadDataTable(status) {
    dataTable = $('#tblBookings').DataTable(
        {
            "ajax": {
                url: '/booking/getall?status='+status
            },
            "columns": [
                { data: 'id', "width": "10%" },
                { data: 'name', "width": "10%" },
                { data: 'phone', "width": "10%" },
                { data: 'email', "width": "15%" },
                { data: 'status', "width": "10%" },
                { data: 'checkInDate', "width": "10%" },
                { data: 'nights', "width": "5%" },
                { data: 'totalCost', render: $.fn.dataTable.render.number(',','.',2), "width": "10%" },
                {
                    data: 'id',
                    "render": function (data) {
                        return `<div class="w-50 btn-group">
                            <a href="/booking/bookingDetails?bookingId=${data}" class="btn btn-outline-warning mx-2">
                            Details
                            </a>
                        </div>`
                    }
                }
            ]
        });
}