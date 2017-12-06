const glob = require("glob");

const srcDir = "./wwwsrc/js/";
const targets = glob.sync(`${srcDir}*.js`);

const entries = {};
targets.forEach(value => {
    entries[value.replace(new RegExp(`^${srcDir}(.*).js$`, "g"), "$1")] = value;
});

// noinspection JSUnresolvedVariable
module.exports = {
    // メインのJS
    entry: entries,
    output: {
        filename: "[name].js"
    }
};