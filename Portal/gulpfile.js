/// <binding BeforeBuild='create' Clean='clean' />

var gulp = require("gulp");
var sequence = require("run-sequence");
var del = require("del");

gulp.task("clean",
    function(cb) {
        return del(["./wwwroot/lib"], cb);
    });

gulp.task("create_bootstrap",
    function() {
        return gulp.src([
                "./node_modules/bootstrap/dist/**/*",
                "!./node_modules/bootstrap/dist/js/npm.js"
            ])
            .pipe(gulp.dest("./wwwroot/lib/bootstrap"));
    });

gulp.task("create_ress",
    function() {
        return gulp.src([
                "./node_modules/ress/ress.css",
                "./node_modules/ress/dist/ress.min.css"
            ])
            .pipe(gulp.dest("./wwwroot/lib/ress"));
    });

gulp.task("create_jquery",
    function() {
        return gulp.src("./node_modules/jquery/dist/**/*")
            .pipe(gulp.dest("./wwwroot/lib/jquery"));
    });

gulp.task("create_jquery_ui",
    function() {
        return gulp.src([
                "./node_modules/jquery-ui-bundle/jquery-ui.js",
                "./node_modules/jquery-ui-bundle/jquery-ui.min.js",
                "./node_modules/jquery-ui-bundle/jquery-ui.css",
                "./node_modules/jquery-ui-bundle/jquery-ui.min.css"
            ])
            .pipe(gulp.dest("./wwwroot/lib/jquery-ui"));
    });

gulp.task("create_jquery_fancytree",
    function() {
        return gulp.src("./node_modules/jquery.fancytree/dist/**/*")
            .pipe(gulp.dest("./wwwroot/lib/jquery.fancytree"));
    });

gulp.task("create",
    function(callback) {
        return sequence(
            ["create_bootstrap", "create_ress", "create_jquery", "create_jquery_ui", "create_jquery_fancytree"],
            callback
        );
    });

gulp.task("default", ["create"]);