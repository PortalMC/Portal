﻿$(document).ready(function () {
    // ======================
    // Initialize
    // ======================

    var projectUuid = window.location.pathname.match(/\/Projects\/([a-zA-Z0-9-]*)\/Editor/)[1];
    var tabMap = {};

    $("#editor-root").layout(getPanelLayoutSettings());
    $("#tree-container").find("> .content").fancytree(getTreeSettings());

    var tabCounter = 1;
    var tabTemplate =
        "<li id='#{id}'><span class='editor-tab-icon editor-tab-icon-unsaved'></span><a href='#{href}'>#{label}</a> <span class='editor-tab-icon editor-tab-icon-close'>×</span></li>";
    var tabs = $("#editor-tabs").tabs();
    tabs.find(".ui-tabs-nav").sortable({
        axis: "x",
        stop: function () {
            tabs.tabs("refresh");
        }
    });
    tabs.on("click", "span.editor-tab-icon-close", function () {
        var panelId = $(this).closest("li").remove().attr("aria-controls");
        $("#" + panelId).remove();
        var path = getPathByPanelId(panelId);
        delete tabMap[path];
        tabs.tabs("refresh");
    });

    $(window).bind('keydown', function (event) {
        if (event.ctrlKey || event.metaKey) {
            switch (String.fromCharCode(event.which).toLowerCase()) {
                case 's':
                    event.preventDefault();
                    trySaveCurrentEditor();
                    break;
            }
        }
    });

    // ======================
    // Functions
    // ======================

    function getPathByPanelId(id) {
        return Object.keys(tabMap).filter(function (key) {
            return tabMap[key] === id;
        })[0];
    }

    function trySaveCurrentEditor() {
        var tab = $("#editor-tab-root").find(".ui-tabs-active");
        var id = tab.attr("aria-controls");
        if (id === "editor-pane-empty") {
            console.log("on empty");
            return;
        }
        var editor = ace.edit("editor-" + id);
        var path = getPathByPanelId(id);
        var content = editor.getValue();

        tab.removeClass("editor-tab-unsaved");

        var xhr = new XMLHttpRequest();
        xhr.open("POST", getApiBaseAddress() + "file/" + projectUuid + "/edit");
        xhr.setRequestHeader("Content-Type", "application/json");
        var data = {
            path: path,
            content: content
        };
        xhr.onload = function () {
            alert("Saved!")
        };
        xhr.onerror = function () {
            console.log("error!");
        };
        xhr.send(JSON.stringify(data));
    }

    function tryOpenExistFile(path) {
        if (path in tabMap) {
            tabs.tabs({active: getIndexOfEditorPanel(tabMap[path])});
            return;
        }

        // ReSharper disable once InconsistentNaming
        var xhr = new XMLHttpRequest();
        xhr.open("POST", getApiBaseAddress() + "file/" + projectUuid + "/get");
        xhr.setRequestHeader("Content-Type", "application/json");
        var data = {
            path: path
        };
        xhr.onload = function () {
            addTab(JSON.parse(xhr.responseText));
        };
        xhr.onerror = function () {
            console.log("error!");
        };
        xhr.send(JSON.stringify(data));
    }

    function addTab(data) {
        var label = data["name"];
        var id = "pane-" + tabCounter;

        tabMap[data["path"]] = id;

        var li = $(tabTemplate.replace(/#\{id\}/g, "tab-" + id).replace(/#\{href\}/g, "#" + id).replace(/#\{label\}/g, label));

        tabs.find(".ui-tabs-nav").append(li);
        tabs.append("<div id='" + id + "' class='editor-pane-root'><div id='editor-" + id + "' class='editor-pane'></div></div>");
        tabs.tabs("refresh");
        tabCounter++;

        tabs.tabs({active: getIndexOfEditorPanel(id)});

        setupEditor(id, data);
    }

    function setupEditor(id, data) {
        var editor = ace.edit("editor-" + id);
        editor.setTheme("ace/theme/monokai");
        editor.getSession().setMode("ace/mode/java");
        editor.setValue(data["content"], -1);
        editor.getSession().on("change", function () {
            $("#tab-" + id).addClass("editor-tab-unsaved")
        });
    }

    function getIndexOfEditorPanel(id) {
        var result = -1;
        $(".ui-tabs-tab").each(function (i) {
            if ($(this).attr("aria-controls") === id) {
                result = i;
                return false;
            }
        });
        return result;
    }

    function getFullPath(node) {
        return getFullPathInternal(node).substr(1);
    }

    function getFullPathInternal(node) {
        if (node.parent === null) {
            return "";
        } else {
            return getFullPathInternal(node.parent) + "/" + node.title;
        }
    }

    function getApiBaseAddress() {
        var url = window.location.protocol + "//";
        url += window.location.host;
        url += "/api/v1/";
        return url;
    }

    // ======================
    // Configuration
    // ======================
    function getPanelLayoutSettings() {
        return {
            name: "editorLayout",
            defaults: {
                size: "auto",
                minSize: 50,
                resizerClass: "resizer",
                togglerClass: "toggler",
                buttonClass: "button",
                contentSelector: ".content",
                contentIgnoreSelector: "span",
                togglerLength_open: 35,
                togglerLength_closed: 35,
                hideTogglerOnSlide: true
            },
            west: {
                paneSelector: "#tree-container",
                size: 250,
                spacing_closed: 21,
                togglerLength_closed: 21,
                togglerAlign_closed: "top",
                togglerLength_open: 0,
                togglerTip_open: "Close East Pane",
                togglerTip_closed: "Open East Pane",
                resizerTip_open: "Resize East Pane",
                slideTrigger_open: "mouseover",
                initClosed: false
            },
            center: {
                paneSelector: "#editor-container",
                minWidth: 200,
                minHeight: 200
            }
        };
    }

    function getTreeSettings() {
        return {
            activeVisible: true, // Make sure, active nodes are visible (expanded)
            aria: true, // Enable WAI-ARIA support
            autoActivate: true, // Automatically activate a node when it is focused using keyboard
            autoCollapse: false, // Automatically collapse all siblings, when a node is expanded
            autoScroll: false, // Automatically scroll nodes into visible area
            clickFolderMode: 4, // 1:activate, 2:expand, 3:activate and expand, 4:activate (dblclick expands)
            checkbox: false, // Show checkboxes
            debugLevel: 2, // 0:quiet, 1:normal, 2:debug
            disabled: false, // Disable control
            focusOnSelect: false, // Set focus when node is checked by a mouse click
            escapeTitles: false, // Escape `node.title` content for display
            generateIds: false, // Generate id attributes like <span id='fancytree-id-KEY'>
            idPrefix: "ft_", // Used to generate node idÂ´s like <span id='fancytree-id-<key>'>
            icon: true, // Display node icons
            keyboard: true, // Support keyboard navigation
            keyPathSeparator: "/", // Used by node.getKeyPath() and tree.loadKeyPath()
            minExpandLevel: 1, // 1: root node is not collapsible
            quicksearch: false, // Navigate to next node by typing the first letters
            selectMode: 2, // 1:single, 2:multi, 3:multi-hier
            tabindex: "0", // Whole tree behaves as one single control
            titlesTabbable: false, // Node titles can receive keyboard focus
            tooltip: false, // Use title as tooltip (also a callback could be specified)

            focus: function () {
                $("#tree-container").find("> .header").css("background-color", "#c6cfdf");
            },

            blur: function () {
                $("#tree-container").find("> .header").css("background-color", "#e4e4e4");
            },

            dblclick: function (event, data) {
                if (!data.node.folder) {
                    tryOpenExistFile(getFullPath(data.node));
                }
            },

            source: [// Typically we would load using ajax instead...
                {title: "build.gradle"},
                {title: "setting.properties"},
                {
                    title: "src",
                    folder: true,
                    expanded: true,
                    children: [
                        {title: "Main.java"},
                        {title: "Main.kt"}
                    ]
                }
            ]
        };
    }
});