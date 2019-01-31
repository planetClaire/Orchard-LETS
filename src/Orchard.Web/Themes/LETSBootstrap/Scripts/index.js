import 'bootstrap';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';
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