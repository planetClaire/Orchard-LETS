// setup defaults for $.validator outside domReady handler
if ($.validator) {
    $.validator.setDefaults({
        highlight: function(element) {
            $(element).closest(".control-group").addClass("error");
        },
        unhighlight: function(element) {
            $(element).closest(".control-group").removeClass("error");
        }
    });

}