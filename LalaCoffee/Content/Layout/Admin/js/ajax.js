$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    if (options.type && options.type.toUpperCase() === "POST") {
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            if (typeof options.data === "string" && options.data.indexOf("__RequestVerificationToken") === -1) {
                options.data += (options.data ? "&" : "") + "__RequestVerificationToken=" + encodeURIComponent(token);
            } else if ($.isPlainObject(options.data) && !options.data.__RequestVerificationToken) {
                options.data.__RequestVerificationToken = token;
            }
        }
    }
});

function removeRow(id, url) {
    if (confirm('Bạn không thể khôi phục sau khi xóa. Bạn chắc chắn xóa?')) {
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            type: "POST",
            dataType: "JSON",
            data: { id: id, __RequestVerificationToken: token },
            url: url,
            success: function (result) {
                if (result.error == false || result.success == true) {
                    alert(result.message);
                    location.reload();
                } else {
                    alert(result.message);
                }
            }
        });
    }
}
