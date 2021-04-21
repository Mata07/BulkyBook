var dataTable;

// load Databable when document is ready
$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    // get table id and fetch it
    dataTable = $('#tblData').DataTable({
        // call API
        "ajax": {            
            "url": "/Admin/CoverType/GetAll"
        },
        // define DataTable columns
        "columns": [
            // first column
            { "data": "name", "width": "60%" },
            // second column (edit,delete buttons)
            {                
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/CoverType/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>
                                <a  onclick=Delete("/Admin/CoverType/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
                                </a>
                            </div>
                           `;
                }, "width": "40%"
            }
        ]
    })
}

function Delete(url) {
    //use SweetAlerts
    swal({
        title: "Are you sure you want to Delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        //using Toaster
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    } else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}