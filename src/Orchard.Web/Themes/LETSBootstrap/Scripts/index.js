import 'bootstrap';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';
import flatpickr from "flatpickr";
import * as $ from "jquery";

require("../scss/app.scss");

if (process.env.NODE_ENV !== 'production') {
    console.log('Looks like we are in development mode!');
}

// this is needed for bootstrap4 validation to work well with jquery unobtrusive
window.addEventListener('load', function () {
    var forms = document.getElementsByClassName('needs-validation');
    var validation = Array.prototype.filter.call(forms, function (form) {
        form.addEventListener('submit', function (event) {
            if (form.checkValidity() === false) {
                event.preventDefault();
                event.stopPropagation();
                // show validation error for radiobuttonlist
                $(".form-check-input.input-validation-error").closest(".form-group").find(".invalid-feedback").css("display", "block");
            }
            form.classList.add('was-validated');
        }, false);
    });
}, false);

$(".wrap-row").wrapAll("<div class='no-gutters row'/>");

flatpickr(".transaction-datepicker", {
    dateFormat: 'd-m-Y',
    maxDate: $("#today").val(),
    minDate: $("#minDate").val()
});

let archiveDatePicker = flatpickr(".archive-datepicker", {
    dateFormat: 'd-m-Y',
    minDate: $("#today").val(),
    maxDate: $("#maxDate").val()
});

$("#archiveQuickPick a.btn").click(function (event) {
    event.preventDefault();
    var days = $(this).children("input:hidden").val();
    var today = new Date();
    var pickedDay = new Date(today.getFullYear(), today.getMonth(), today.getDate() + parseInt(days));
    $(".archive-datepicker").val(flatpickr.formatDate(pickedDay, 'd-m-Y'));
    archiveDatePicker.setDate(pickedDay);
    $("#archiveQuickPick a.btn").each(function () {
        $(this).removeClass('active');
    })
    $(this).addClass('active');
});

// ensure the default archive notice value is sent if date has no value
$(".edit-create-notice button:submit").click(function () {
    if ($(".archive-datepicker").val() === '') {
        $(".archive-datepicker").val($("#maxDate").val());
    }
});

// highlight relevant archive notice buttons
$(function () {
    $("#archiveQuickPick").each(function () {
        var dateVal = $(".archive-datepicker").val();
        if (dateVal === $("#maxDate").val()) {
            $("#aMaxDays").addClass('active');
        }
        else if (dateVal === $("#twoDays").val()) {
            $("#aTwoDays").addClass('active');
        }
        else if (dateVal === $("#sevenDays").val()) {
            $("#aSevenDays").addClass('active');
        }
    })
});