﻿/// <binding BeforeBuild='create' Clean='clean' />

const gulp = require("gulp");
const babel = require("gulp-babel");
const uglify = require("gulp-uglify");
const cleanCSS = require("gulp-clean-css");
const rename = require("gulp-rename");
const sequence = require("run-sequence");
const del = require("del");
const merge = require("merge-stream");
const webpack = require("webpack");
const webpackStream = require("webpack-stream");
const webpackConfig = require("./webpack.config");
const sass = require("gulp-sass");
const postcss = require("gulp-postcss");
const cssnext = require("postcss-cssnext");

gulp.task("clean", function (cb) {
    return del([
        "./wwwroot/js",
        "./wwwroot/css",
        "./wwwroot/lib"], cb);
});

gulp.task("watch", function () {
    gulp.watch(["./wwwsrc/js/**/*.js"], ["webpack"]);
    gulp.watch('./wwwsrc/scss/**/*.scss', ['scss']);
});

gulp.task("webpack", function () {
    return gulp.src("./wwwsrc/js/*.js")
        .pipe(webpackStream(webpackConfig, webpack))
        .on("error", function (error) {
            console.log("* error handler: " + error);
            this.emit("end");
        })
        .pipe(gulp.dest("./wwwroot/js/"));
});

gulp.task("scss", function () {
    const processors = [
        cssnext()
    ];
    return gulp.src("./wwwsrc/scss/*.scss")
        .pipe(sass())
        .on("error", function (error) {
            console.log("* error handler: " + error);
            this.emit("end");
        })
        .pipe(postcss(processors))
        .pipe(gulp.dest("./wwwroot/css/"))
});

gulp.task("minify_js", ["webpack"], function () {
    return gulp.src("./wwwroot/js/*.js")
        .pipe(babel())
        .pipe(uglify())
        .on("error", function (e) {
            console.log(e);
        })
        .pipe(rename({
            extname: ".min.js"
        }))
        .pipe(gulp.dest("./wwwroot/js/"));
});

gulp.task("minify_css", ["scss"], function () {
    return gulp.src("./wwwroot/css/*.css")
        .pipe(cleanCSS())
        .pipe(rename({
            extname: ".min.css"
        }))
        .pipe(gulp.dest("./wwwroot/css/"));
});

gulp.task("create_bootstrap", function () {
    return gulp.src([
        "./node_modules/bootstrap/dist/**/*",
        "!./node_modules/bootstrap/dist/js/npm.js"
    ])
        .pipe(gulp.dest("./wwwroot/lib/bootstrap"));
});

gulp.task("create_ress", function () {
    return gulp.src([
        "./node_modules/ress/ress.css",
        "./node_modules/ress/dist/ress.min.css"
    ])
        .pipe(gulp.dest("./wwwroot/lib/ress"));
});

gulp.task("create_jquery", function () {
    return merge(
        gulp.src("./node_modules/jquery/dist/**/*")
            .pipe(gulp.dest("./wwwroot/lib/jquery")),
        gulp.src("./node_modules/jquery-validation/dist/**/*")
            .pipe(gulp.dest("./wwwroot/lib/jquery-validation")),
        gulp.src("./node_modules/jquery-validation-unobtrusive/*.js")
            .pipe(gulp.dest("./wwwroot/lib/jquery-validation-unobtrusive"))
    );
});

gulp.task("create_jquery_ui", function () {
    return merge(
        gulp.src([
            "./node_modules/jquery-ui-bundle/jquery-ui.js",
            "./node_modules/jquery-ui-bundle/jquery-ui.min.js",
            "./node_modules/jquery-ui-bundle/jquery-ui.css",
            "./node_modules/jquery-ui-bundle/jquery-ui.min.css",
            "./node_modules/jquery-ui-bundle/images/*"])
            .pipe(gulp.dest("./wwwroot/lib/jquery-ui")),
        gulp.src("./node_modules/jquery-ui-bundle/images/*")
            .pipe(gulp.dest("./wwwroot/lib/jquery-ui/images"))
    )
});

gulp.task("create_jquery_fancytree", function () {
    return gulp.src("./node_modules/jquery.fancytree/dist/**/*")
        .pipe(gulp.dest("./wwwroot/lib/jquery.fancytree"));
});

gulp.task("create_jquery_layout", function () {
    return gulp.src("./node_modules/jquery.layout-custom/dist/**/*")
        .pipe(gulp.dest("./wwwroot/lib/jquery.layout"));
});

gulp.task("create_jquery_ui_contextmenu", function () {
    return gulp.src([
        "./node_modules/ui-contextmenu/jquery.ui-contextmenu.js",
        "./node_modules/ui-contextmenu/jquery.ui-contextmenu.min.js"
    ])
        .pipe(gulp.dest("./wwwroot/lib/jquery.ui-contextmenu"));
});

gulp.task("create_ace_editor", function () {
    return gulp.src("./node_modules/ace-builds/src/**/*")
        .pipe(gulp.dest("./wwwroot/lib/ace"));
});

gulp.task("create_font_awesome", function () {
    return merge(
        gulp.src("./node_modules/font-awesome/css/*.css")
            .pipe(gulp.dest("./wwwroot/lib/font-awesome/css")),
        gulp.src("./node_modules/font-awesome/fonts/*")
            .pipe(gulp.dest("./wwwroot/lib/font-awesome/fonts"))
    );
});

gulp.task("create_fine_uploader", function () {
    return gulp.src("./node_modules/fine-uploader/jquery.fine-uploader/**")
        .pipe(gulp.dest("./wwwroot/lib/fine-uploader"))
});

gulp.task("create_popper_js", function () {
    return gulp.src("./node_modules/popper.js/dist/umd/**")
        .pipe(gulp.dest("./wwwroot/lib/popper.js"))
});


gulp.task("create", function (callback) {
    return sequence(
        [
            "minify_js",
            "minify_css",
            "create_bootstrap",
            "create_ress",
            "create_jquery",
            "create_jquery_ui",
            "create_jquery_fancytree",
            "create_jquery_layout",
            "create_jquery_ui_contextmenu",
            "create_ace_editor",
            "create_font_awesome",
            "create_fine_uploader",
            "create_popper_js"
        ],
        callback
    );
});

gulp.task("default", ["create"]);