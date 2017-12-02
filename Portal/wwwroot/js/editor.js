import * as config from "./editor/config";
import ApiClient from './editor/api_client';
import Path from './editor/path';
import * as dialog from "./editor/dialog";
import * as util from "./editor/util";

$(document).ready(() => {

    // ======================
    // Initialize
    // ======================

    const projectUuid = window.location.pathname.match(/\/Projects\/([a-zA-Z0-9-]*)\/Editor/)[1];
    const apiClient = new ApiClient(`${window.location.protocol}//${window.location.host}/api/v1/`, projectUuid);
    const components = {};
    const keyPathMap = {};
    const setting = {
        "show-navigation-bar": true
    };
    let webSocket = null;

    components.editor_root = $("#editor-root");
    components.tree_container = $("#tree-container");
    components.editor_root.layout(config.getPanelLayoutSettings());

    setupProjectTree();
    setupTab();
    setupMenu();
    setupToolbar();
    setupProjectTreeMenu();
    setupKeyboardEvent();

    connectLogWebsocket();
    fetchProjectTree();

    // ======================
    // Setup Functions
    // ======================

    function setupProjectTree() {
        const tree = components.tree_container.find("> .content").fancytree(config.getTreeSettings());
        components.tree = tree;
        tree.fancytree("option", "focus", (event, data) => {
            let node = data.node;
            let paths = [];
            while (node !== undefined && node !== null) {
                paths.unshift(node.title);
                node = node.parent;
            }
            paths.shift();
            paths.shift();
            updateToolbarBreadcrumbPath(paths.join("/"));
            components.tree_container.addClass("pane-focused");
        });
        tree.fancytree("option", "blur", () => {
            components.tree_container.removeClass("pane-focused");
        });
        tree.fancytree("option", "dblclick", (event, data) => {
            if (!data.node.folder) {
                tryOpenExistFile(data.node.key);
            }
        });
        tree.fancytree("option", "renderNode", (event, data) => {
            const node = data.node;
            const $nodeSpan = $(node.span);
            if (!$nodeSpan.data("rendered")) {
                const backgroundDiv = $("<div class='fancytree-node-background'><span></span></div>");
                $nodeSpan.append(backgroundDiv);
                $nodeSpan.data("rendered", true);
                $nodeSpan.attr("data-key", node.key)
            }
        });
    }

    function setupTab() {
        const tabs = $("#editor-tabs").tabs({
            activate: (e, ui) => {
                const panelId = ui.newTab.attr("aria-controls");
                if (panelId === "editor-pane-empty") {
                    updateToolbarBreadcrumbPath("/");
                } else {
                    updateToolbarBreadcrumbPath(keyPathMap[getKeyByPanelId(panelId)].path);
                }
            }
        });
        components.tab = tabs;
        tabs.find(".ui-tabs-nav").sortable({
            axis: "x",
            stop: () => {
                tabs.tabs("refresh");
            }
        });
        tabs.on("click", "span.editor-tab-icon-close", (e) => {
            const panelId = $(e.target).closest("li").remove().attr("aria-controls");
            $(`#${panelId}`).remove();
            tabs.tabs("refresh");
        });
    }

    function setupMenu() {
        $("#editor-menu").find("li").on("click", (e) => {
            const command = $(e.target).attr("data-command") === undefined
                ? $(e.target.parentElement).attr("data-command")
                : $(e.target).attr("data-command");
            onClickCommand(command);
        });
    }

    function setupToolbar() {
        $(".editor-toolbar-command").on("click", (e) => {
            const command = $(e.target).attr("data-command");
            onClickCommand(command);
        });
    }

    function setupProjectTreeMenu() {
        const menu = components.tree_container.contextmenu(config.getTreeContextMenuSettings());
        menu.contextmenu("option", "select", function (event, ui) {
            onFileTreeCommand(ui.cmd, keyPathMap[ui.target.parent().attr("data-key")]);
        });
        menu.contextmenu("option", "beforeOpen", function (event, ui) {
            const menu = ui.menu;
            components.tree.fancytree("getTree").activateKey(ui.target.parent().attr("data-key"));
            components.tree_container.toggleClass("tree-contextmenu-open", true);
            menu.find(".ui-icon").addClass("fa");
            menu.find(".ui-icon-caret-1-e").addClass("fa-caret-right").removeClass("ui-icon-caret-1-e");
        });
        menu.contextmenu("option", "open", function () {
            components.tree_container.toggleClass("tree-contextmenu-open", true);
        });
        menu.contextmenu("option", "close", function () {
            components.tree_container.toggleClass("tree-contextmenu-open", false);
        });
    }

    function setupKeyboardEvent() {
        $(window).bind("keydown", (event) => {
            if (event.ctrlKey || event.metaKey) {
                switch (String.fromCharCode(event.which).toLowerCase()) {
                    case "s":
                        event.preventDefault();
                        trySaveCurrentEditor();
                        break;
                }
            }
        });
    }

    // ======================
    // Functions
    // ======================

    function connectLogWebsocket() {
        const uri = `ws://${window.location.host}/api/v1/projects/${projectUuid}/ws`;
        const buildLogContainer = $("#log-container").find(".content");

        if (webSocket === undefined || webSocket === null) {
            webSocket = new WebSocket(uri);
            webSocket.onopen = onOpen;
            webSocket.onmessage = onMessage;
            webSocket.onclose = onClose;
            webSocket.onerror = onError;
        }

        function onOpen(event) {
            appendToBuildLog("接続しました。\r\n");
        }

        function onMessage(event) {
            if (event && event.data) {
                appendToBuildLog(event.data);
            }
        }

        function onError(event) {
            appendToBuildLog("Error");
        }

        function onClose(event) {
            appendToBuildLog("Disconnected (" + event.code + ")");
            webSocket = null;
        }

        let lastLine = "";

        function appendToBuildLog(message) {
            lastLine += message;
            lastLine = checkLastLine(lastLine, "\r\n");
            lastLine = checkLastLine(lastLine, "\r");
            lastLine = checkLastLine(lastLine, "\n");
            $("#log-last-line").text(lastLine);
            buildLogContainer.animate({scrollTop: buildLogContainer[0].scrollHeight}, 0);
        }

        function checkLastLine(lastLine, newLineStr) {
            const arr = lastLine.split(newLineStr);
            if (arr.length > 2) {

                for (let i = 0; i < arr.length - 1; ++i) {
                    $("#log-last-line").before(`<div class="log-line">${arr[i]}</div>`);
                }
                return arr[arr.length - 1];
            }
            return lastLine;
        }
    }

    function fetchProjectTree() {
        apiClient.getProjectFileList()
            .done(data => {
                $("#editor-toolbar-breadcrumb-root").text(data[0].title);
                components.tree_container.find("> .content").fancytree("option", "source", data);
                keyPathMap[data[0].key] = new Path("", "", true);
                data[0].children.forEach(v => updateKeyPathMapping(v, "/"))
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
            });
    }

    function updateKeyPathMapping(data, parentPath) {
        const isDirectory = !!data.folder;
        keyPathMap[data.key] = new Path(parentPath, data.title, isDirectory);
        if (isDirectory) {
            data.children.forEach(v => updateKeyPathMapping(v, parentPath + data.title + "/"))
        }
    }

    function onClickCommand(command) {
        switch (command) {
            case "navigation-bar":
                if (setting["show-navigation-bar"]) {
                    $(".navbar").hide();
                    $('[data-menu-command="navigation-bar"]').find(".dropdown-menu-icon").addClass("dropdown-menu-icon-none");
                    setting["show-navigation-bar"] = false;
                } else {
                    $(".navbar").show();
                    $('[data-menu-command="navigation-bar"]').find(".dropdown-menu-icon").removeClass("dropdown-menu-icon-none");
                    setting["show-navigation-bar"] = true;
                }
                components.editor_root.layout().resizeAll();
                break;
            case "build":
                console.log("Start build");
                apiClient.startProjectBuild()
                    .done(() => {
                        alert("Start build");
                    })
                    .fail((jqXhr, textStatus, errorThrown) => {
                        console.log("error! : " + errorThrown.toString());
                    });
                break;
            default:
                console.error("Unknown command : " + command);
                break;
        }
    }

    function onFileTreeCommand(command, path) {
        console.log(`Received file tree command : ${command}`, path);
        switch (command) {
            case "new-java-class":
                dialog.showSingleInputDialog(`Create: Class`, "Enter name: ", "NewClass", "Class name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, util.checkExtension(name, "java"), false, "java-class");
                    });
                break;
            case "new-java-interface":
                dialog.showSingleInputDialog(`Cleate: Interface`, "Enter name: ", "NewInterface", "Interface name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, util.checkExtension(name, "java"), false, "java-interface");
                    });
                break;
            case "new-java-enum":
                dialog.showSingleInputDialog(`Cleate: Enum`, "Enter name: ", "NewEnum", "Enum name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, util.checkExtension(name, "java"), false, "java-enum");
                    });
                break;
            case "new-json-blockstate":
                dialog.showSingleInputDialog(`Cleate: Blockstate JSON`, "Enter name: ", "new_block", "Blockstate JSON name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, util.checkExtension(name, "json"), false, "json-blockstate");
                    });
                break;
            case "new-json-item":
                dialog.showSingleInputDialog(`Cleate: Item JSON`, "Enter name: ", "new_item", "Blockstate Item name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, util.checkExtension(name, "json"), false, "json-item");
                    });
                break;
            case "new-json-model":
                dialog.showSingleInputDialog(`Cleate: Model JSON`, "Enter name: ", "new_model", "Model JSON name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, util.checkExtension(name, "json"), false, "json-model");
                    });
                break;
            case "new-json":
                dialog.showSingleInputDialog(`Cleate: JSON`, "Enter name: ", "new_json", "JSON name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, util.checkExtension(name, "json"), false, "json");
                    });
                break;
            case "new-file":
                dialog.showSingleInputDialog(`Cleate: File`, "Enter name: ", "NewFile.txt", "File name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, name, false);
                    });
                break;
            case "new-directory":
                dialog.showSingleInputDialog(`Cleate: Directory`, "Enter name: ", "NewDirectory", "Directory name", "OK", "Cancel",
                    (name) => {
                        createNewFile(path, name, true);
                    });
                break;
            case "upload":
                break;
            case "copy":
                break;
            case "paste":
                break;
            case "rename":
                dialog.showSingleInputDialog(`Rename ${path.name}`, "Enter a new name: ", path.name, "New name", "OK", "Cancel",
                    (newName) => {
                        apiClient.moveProjectFile(path.path, path.parent + newName, path.folder)
                            .done(() => {
                                fetchProjectTree();
                                console.log("Yes" + newName);
                            })
                            .fail((jqXhr, textStatus, errorThrown) => {
                                console.log("error! : " + errorThrown.toString());
                            });
                    });
                break;
            case "delete":
                break;
            default:
                console.error(`Unknown tree command : ${command}`);
                break;
        }
    }

    function createNewFile(path, name, isDirectory, snippetName = undefined) {
        let p;
        if (path.folder) {
            p = path.path + name;
        } else {
            p = path.parent + name;
        }
        apiClient.createProjectFile(p, isDirectory, snippetName)
            .done(() => {
                fetchProjectTree();
                console.log("Yes" + p);
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
            });
    }

    function updateToolbarBreadcrumbPath(path) {
        const container = $("#editor-toolbar-breadcrumb-container");
        // noinspection JSValidateTypes
        container.children(":not(#editor-toolbar-breadcrumb-root)").remove();
        if (path === undefined) {
            return
        }
        const paths = path.split("/");
        for (let p of paths) {
            container.append(`<li>${p}</li>`);
        }
    }

    function getKeyByPanelId(id) {
        return id.substr(5);
        // return Object.keys(keyTabMap).filter(key => keyTabMap[key] === id)[0];
    }

    function trySaveCurrentEditor() {
        const tab = $("#editor-tab-root").find(".ui-tabs-active");
        const id = tab.attr("aria-controls");
        if (id === "editor-pane-empty") {
            console.log("on empty");
            return;
        }
        const editor = ace.edit("editor-" + id);
        const path = keyPathMap[getKeyByPanelId(id)].path;
        const content = editor.getValue();

        tab.removeClass("editor-tab-unsaved");
        apiClient.saveProjectFile(path, content)
            .done(() => {
                alert("Saved!");
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
            });
    }

    function tryOpenExistFile(key) {
        const id = `pane-${key}`;
        if ($(`#${id}`).length !== 0) {
            components.tab.tabs({active: getIndexOfEditorPanel(id)});
            return;
        }
        const path = keyPathMap[key].path;
        apiClient.getProjectFile(path)
            .done(result => {
                addTab(key, result);
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
            });
    }

    function addTab(key, data) {
        const label = data["name"];
        const id = `pane-${key}`;
        const li = `<li id='tab-${id}'><span class='editor-tab-icon editor-tab-icon-unsaved'></span><a href='#${id}'>${label}</a> <span class='editor-tab-icon editor-tab-icon-close'>×</span></li>`;

        components.tab.find(".ui-tabs-nav").append(li);
        components.tab.append(`<div id='${id}' class='editor-pane-root'><div id='editor-${id}' class='editor-pane'></div></div>`);
        components.tab.tabs("refresh");
        components.tab.tabs({active: getIndexOfEditorPanel(id)});

        setupEditor(id, data);
    }

    function setupEditor(id, data) {
        const editor = ace.edit(`editor-${id}`);
        editor.$blockScrolling = Infinity;
        editor.setTheme("ace/theme/monokai");
        editor.getSession().setMode("ace/mode/java");
        editor.setValue(data["content"], -1);
        editor.getSession().on("change", () => {
            $(`#tab-${id}`).addClass("editor-tab-unsaved");
        });
        editor.on("focus", () => {
            updateToolbarBreadcrumbPath(data.path);
        });
    }

    function getIndexOfEditorPanel(id) {
        let result = -1;
        $(".ui-tabs-tab").each((i, e) => {
            if ($(e).attr("aria-controls") === id) {
                result = i;
                return false;
            }
            // ReSharper disable once NotAllPathsReturnValue
        });
        return result;
    }
});