$("form").addClass("form-horizontal");
$(".noticeTypesRadioList label").addClass("inline").addClass("radio");
$(document).ready(function () {
    $('a[rel="tooltip"]').tooltip();
    $('a[rel="popover"]').popover();
});
$("article").addClass("clearfix");

$("h1").not($("header h1")).wrap('<header class="page-header">');
