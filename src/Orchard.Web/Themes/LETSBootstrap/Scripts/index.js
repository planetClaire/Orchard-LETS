import 'bootstrap';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';
require("../scss/app.scss");
if (process.env.NODE_ENV !== 'production') {
    console.log('Looks like we are in development mode!');
}
