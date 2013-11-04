$(document).ready(function() {

    $(":input[placeholder]")
                .each(function () {
                    var _this = $(this);
                    if (_this.val() === "") {
                        _this.val(_this.attr("placeholder")).addClass("placeholderd");
                    }
                })
                .live("focus", function () {
                    var _this = $(this);
                    if (_this.val() === _this.attr("placeholder")) {
                        _this.val("").removeClass("placeholderd");
                    }
                })
                .live("blur", function () {
                    var _this = $(this);
                    if (_this.val() === "") {
                        _this.val(_this.attr("placeholder")).addClass("placeholderd");
                    }
                });

    //make sure the placeholder value is not taken as the input value when submitting forms
    $("form").live("submit", function () {
        $(":input[placeholder].placeholderd").val("");
    });
});
