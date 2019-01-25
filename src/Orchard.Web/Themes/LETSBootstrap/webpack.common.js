const path = require('path');
module.exports = {
    entry: './Scripts/index.js',
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: 'index.bundle.js'
    },
    resolve: {
        extensions: [".js"]
    },
    module: {
        rules: [
            {
                test: /\.(eot|ttf|woff)$/,
                loader: 'url-loader?limit=65000&mimetype=application/vnd.ms-fontobject'
            }
        ]
    }
};

