import 'bootstrap';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';
import flatpickr from "flatpickr";
import * as $ from "jquery";
import Dropzone from 'dropzone';

require("../scss/app.scss");

if (process.env.NODE_ENV !== 'production') {
    console.log('Looks like we are in development mode!');
}

(function (d) { d.className = "dyn" + d.className.substring(6, d.className.length); })(document.documentElement);

// UnsafeUrl links -> form POST
$(function () {
    var magicToken = $("input[name=__RequestVerificationToken]").first();
    if (!magicToken) { return; } // no sense in continuing if form POSTS will fail
    $("body").on("click", "a[itemprop~=UnsafeUrl], a[data-unsafe-url]", function () {
        var _this = $(this);
        var hrefParts = _this.attr("href").split("?");
        var form = $("<form action=\"" + hrefParts[0] + "\" method=\"POST\" />");
        form.append(magicToken.clone());
        if (hrefParts.length > 1) {
            var queryParts = hrefParts[1].split("&");
            for (var i = 0; i < queryParts.length; i++) {
                var queryPartKVP = queryParts[i].split("=");
                //trusting hrefs in the page here
                form.append($("<input type=\"hidden\" name=\"" + decodeURIComponent(queryPartKVP[0]) + "\" value=\"" + decodeURIComponent(queryPartKVP[1]) + "\" />"));
            }
        }
        form.css({ "position": "absolute", "left": "-9999em" });
        $("body").append(form);

        var unsafeUrlPrompt = _this.data("unsafe-url");

        if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
            if (!confirm(unsafeUrlPrompt)) {
                return false;
            }
        }

        if (_this.filter("[itemprop~='RemoveUrl']").length == 1) {
            // use a custom message if its set in data-message
            var dataMessage = _this.data('message');
            if (dataMessage === undefined) {
                dataMessage = "Are you sure you want to delete this?";
            }

            if (!confirm(dataMessage)) {
                return false;
            }
        }

        form.submit();
        return false;

    });
});

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

// notice images
Dropzone.autoDiscover = false;

$(function () {

    $(".dropzone-container").each(function () {

        var $filenamesInput = $("#filenames");
        var images = $filenamesInput.val().split(";");

        var dz = new Dropzone("#dropzoneContainer", {
            url: $("#upload-url").val(),
            thumbnailWidth: 170,
            thumbnailHeight: 170,
            resizeWidth: $("#resize-width").val(),
            maxFiles: $("#file-limit").val(),
            acceptedFiles: 'image/gif,image/jpeg,image/png,image/svg+xml',
            addRemoveLinks: true,
            dictDefaultMessage: "Click or drop image files here (limit " + $("#file-limit").val() + ")",
            init: function () {
                var self = this;
                this.on("sending", function (file, xhr, formData) {
                    formData.append("__RequestVerificationToken", $("#anti-forgery-token").val());
                    formData.append("dropzoneMediaFolder", $("#dropzone-folder").val());
                });
                this.on("success", function (file) {
                    var newVal = file.xhr.responseText;
                    var val = $filenamesInput.val();
                    if (val !== "")
                        val += ";";
                    val += newVal;
                    $filenamesInput.val(val);
                    file.uploadedName = newVal;
                });
                this.on("removedfile", function (file) {
                    $filenamesInput.val($.grep($filenamesInput.val().split(';'), function (fileName) { return fileName !== file.dataURL; }).join(';'));
                });
                this.on("maxfilesexceeded", function (file) {
                    this.removeFile(file);
                });
                for (var i = 0; i < images.length; i++) {
                    let image = images[i];
                    if (image === "")
                        continue;

                    let imagePathComps = image.split("/");
                    let filename = imagePathComps[imagePathComps.length - 1];
                    let mockFile = { name: filename, size: 1, dataURL: image };
                    this.emit("addedfile", mockFile);
                    this.createThumbnailFromUrl(mockFile,
                        this.options.thumbnailWidth, this.options.thumbnailHeight,
                        this.options.thumbnailMethod, true, function (thumbnail) {
                            self.emit('thumbnail', mockFile, thumbnail);
                        });
                    this.emit("complete", mockFile);
                }
            }
        })
    })
})
