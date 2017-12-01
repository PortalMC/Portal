import * as dialog from "./editor/dialog";

$(document).ready(() => {

    // ======================
    // Initialize
    // ======================

    const projectUuid = window.location.pathname.match(/\/Projects\/([a-zA-Z0-9-]*)\/Editor/)[1];
    const tabMap = {};
    const keyPathMap = {};
    const setting = {
        "show-navigation-bar": true
    };

    const editor_root = $("#editor-root");
    const tree_container = $("#tree-container");
    editor_root.layout(getPanelLayoutSettings());
    const tree = tree_container.find("> .content").fancytree(getTreeSettings());

    let tabCounter = 1;
    const tabTemplate = "<li id='#{id}'><span class='editor-tab-icon editor-tab-icon-unsaved'></span><a href='#{href}'>#{label}</a> <span class='editor-tab-icon editor-tab-icon-close'>×</span></li>";
    const tabs = $("#editor-tabs").tabs({
        activate: (e, ui) => {
            const panelId = ui.newTab.attr("aria-controls");
            const path = getPathByPanelId(panelId);
            updateToolbarBreadcrumbPath(path);
        }
    });
    tabs.find(".ui-tabs-nav").sortable({
        axis: "x",
        stop: () => {
            tabs.tabs("refresh");
        }
    });
    tabs.on("click", "span.editor-tab-icon-close", (e) => {
        const panelId = $(e.target).closest("li").remove().attr("aria-controls");
        $(`#${panelId}`).remove();
        const path = getPathByPanelId(panelId);
        delete tabMap[path];
        tabs.tabs("refresh");
    });

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

    // Menu
    $("#editor-menu").find("li").on("click", (e) => {
        const command = $(e.target).attr("data-command") === undefined
            ? $(e.target.parentElement).attr("data-command")
            : $(e.target).attr("data-command");
        onClickCommand(command);
    });

    // Toolbar
    $(".editor-toolbar-command").on("click", (e) => {
        const command = $(e.target).attr("data-command");
        onClickCommand(command);
    });

    // Context Menu
    tree_container.contextmenu(getTreeContextMenuSettings());

    let webSocket = null;
    connectLogWebsocket();

    fetchProjectTree();

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
        $.ajax({
            url: `${getApiBaseAddress()}projects/${projectUuid}/file/list`,
            type: "get",
            dataType: "json"
        }).done(data => {
            $("#editor-toolbar-breadcrumb-root").text(data[0].title);
            $("#tree-container").find("> .content").fancytree("option", "source", data);
            data[0].children.forEach(v => updateKeyPathMapping(v, "/"))
        }).fail((jqXhr, textStatus, errorThrown) => {
            console.log("error!");
        });
    }

    function updateKeyPathMapping(data, parentPath) {
        keyPathMap[data.key] = {
            parent: parentPath,
            name: data.title,
            path: parentPath + data.title,
            folder: !!data.folder
        };
        if (data.folder === true) {
            data.children.forEach(v => updateKeyPathMapping(v, parentPath + data.title + "/"))
        }
    }

    function onClickCommand(command) {
        switch (command) {
            case "navigation-bar":
                if (setting["show-navigation-bar"]) {
                    $(".navbar").hide();
                    $('[data-menu-command="navigation-bar"]').find(".dropdown-menu-icon").addClass("dropdown-menu-icon-none")
                    setting["show-navigation-bar"] = false;
                } else {
                    $(".navbar").show();
                    $('[data-menu-command="navigation-bar"]').find(".dropdown-menu-icon").removeClass("dropdown-menu-icon-none")
                    setting["show-navigation-bar"] = true;
                }
                $("#editor-root").layout().resizeAll();
                break;
            case "build":
                console.log("Start build");
                $.ajax({
                    url: `${getApiBaseAddress()}projects/${projectUuid}/build`,
                    type: "post",
                    dataType: "json"
                }).done(() => {
                    alert("Start build");
                }).fail((jqXhr, textStatus, errorThrown) => {
                    console.log("error!");
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
                        console.log("Yes" + name);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-java-interface":
                dialog.showSingleInputDialog(`Cleate: Interface`, "Enter name: ", "NewInterface", "Interface name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-java-enum":
                dialog.showSingleInputDialog(`Cleate: Enum`, "Enter name: ", "NewEnum", "Enum name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-json-blockstate":
                dialog.showSingleInputDialog(`Cleate: Blockstate JSON`, "Enter name: ", "new_block", "Blockstate JSON name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-json-item":
                dialog.showSingleInputDialog(`Cleate: Item JSON`, "Enter name: ", "new_item", "Blockstate Item name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-json-model":
                dialog.showSingleInputDialog(`Cleate: Model JSON`, "Enter name: ", "new_model", "Model JSON name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-json":
                dialog.showSingleInputDialog(`Cleate: JSON`, "Enter name: ", "new_json", "JSON name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-file":
                dialog.showSingleInputDialog(`Cleate: File`, "Enter name: ", "NewFile.txt", "File name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "new-directory":
                dialog.showSingleInputDialog(`Cleate: Directory`, "Enter name: ", "NewDirectory", "Directory name", "OK", "Cancel",
                    (newName) => {
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
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
                        console.log("Yes" + newName);
                    }, () => {
                        console.log("Cancel");
                    });
                break;
            case "delete":
                break;
            default:
                console.error(`Unknown tree command : ${command}`);
                break;
        }
    }

    function updateToolbarBreadcrumbPath(path) {
        const container = $("#editor-toolbar-breadcrumb-container");
        container.children(":not(#editor-toolbar-breadcrumb-root)").remove();
        if (path === undefined) {
            return
        }
        const paths = path.split("/");
        for (let p of paths) {
            container.append(`<li>${p}</li>`);
        }
    }

    function getPathByPanelId(id) {
        return Object.keys(tabMap).filter(key => tabMap[key] === id)[0];
    }

    function trySaveCurrentEditor() {
        const tab = $("#editor-tab-root").find(".ui-tabs-active");
        const id = tab.attr("aria-controls");
        if (id === "editor-pane-empty") {
            console.log("on empty");
            return;
        }
        const editor = ace.edit("editor-" + id);
        const path = getPathByPanelId(id);
        const content = editor.getValue();

        tab.removeClass("editor-tab-unsaved");

        const data = {
            path: path,
            content: content
        };
        $.ajax({
            url: `${getApiBaseAddress()}projects/${projectUuid}/file/edit`,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json"
        }).done(() => {
            alert("Saved!");
        }).fail((jqXhr, textStatus, errorThrown) => {
            console.log("error!");
        });
    }

    function tryOpenExistFile(path) {
        if (path in tabMap) {
            tabs.tabs({active: getIndexOfEditorPanel(tabMap[path])});
            return;
        }

        const data = {
            path: path
        };
        $.ajax({
            url: `${getApiBaseAddress()}projects/${projectUuid}/file/get`,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json"
        }).done(result => {
            addTab(result);
        }).fail((jqXhr, textStatus, errorThrown) => {
            console.log("error!");
        });
    }

    function addTab(data) {
        const label = data["name"];
        const id = "pane-" + tabCounter;

        tabMap[data["path"]] = id;

        const li = $(tabTemplate.replace(/#\{id\}/g, `tab-${id}`).replace(/#\{href\}/g, `#${id}`)
            .replace(/#\{label\}/g, label));

        tabs.find(".ui-tabs-nav").append(li);
        tabs.append(`<div id='${id}' class='editor-pane-root'><div id='editor-${id}' class='editor-pane'></div></div>`);
        tabs.tabs("refresh");
        tabCounter++;

        tabs.tabs({active: getIndexOfEditorPanel(id)});

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

    function getFullPath(node) {
        const fullPathOrigin = getFullPathInternal(node);
        return fullPathOrigin.substr(fullPathOrigin.indexOf("/", 1) + 1);
    }

    function getFullPathInternal(node) {
        if (node.parent === null) {
            return "";
        } else {
            return `${getFullPathInternal(node.parent)}/${node.title}`;
        }
    }

    function getApiBaseAddress() {
        return `${window.location.protocol}//${window.location.host}/api/v1/`;
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
            south: {
                paneSelector: "#log-container",
                size: 70,
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
            debugLevel: 0, // 0:quiet, 1:normal, 2:debug
            disabled: false, // Disable control
            focusOnSelect: false, // Set focus when node is checked by a mouse click
            escapeTitles: true, // Escape `node.title` content for display
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
            toggleEffect: false,

            focus: (event, data) => {
                let node = data.node;
                let paths = [];
                while (node !== undefined && node !== null) {
                    paths.unshift(node.title);
                    node = node.parent;
                }
                paths.shift();
                paths.shift();
                updateToolbarBreadcrumbPath(paths.join("/"));
                $("#tree-container").addClass("pane-focused");
            },

            blur: () => {
                $("#tree-container").removeClass("pane-focused");
            },

            dblclick: (event, data) => {
                if (!data.node.folder) {
                    tryOpenExistFile(getFullPath(data.node));
                }
            },

            renderNode: (event, data) => {
                const node = data.node;
                const $nodeSpan = $(node.span);
                if (!$nodeSpan.data("rendered")) {
                    const backgroundDiv = $("<div class='fancytree-node-background'><span></span></div>");
                    $nodeSpan.append(backgroundDiv);
                    $nodeSpan.data("rendered", true);
                    $nodeSpan.attr("data-key", node.key)
                }
            },

            source: []
        };
    }

    function getTreeContextMenuSettings() {
        return {
            delegate: ".fancytree-node",
            autoFocus: true,
            preventContextMenuForPopup: true,
            show: false,
            hide: false,
            menu: [
                {

                    title: "New",
                    children: [
                        {
                            title: "Class",
                            cmd: "new-java-class",
                            uiIcon: "fa-file-code-o"
                        },
                        {
                            title: "Interface",
                            cmd: "new-java-interface",
                            uiIcon: "fa-file-code-o"
                        },
                        {
                            title: "Enum",
                            cmd: "new-java-enum",
                            uiIcon: "fa-file-code-o"
                        },
                        {
                            title: "----"
                        },
                        {
                            title: "BlockState JSON File",
                            cmd: "new-json-blockstate",
                            uiIcon: "fa-file-text-o"
                        },
                        {
                            title: "Item JSON File",
                            cmd: "new-json-item",
                            uiIcon: "fa-file-text-o"
                        },
                        {
                            title: "Model JSON File",
                            cmd: "new-json-model",
                            uiIcon: "fa-file-text-o"
                        },
                        {
                            title: "JSON File",
                            cmd: "new-json",
                            uiIcon: "fa-file-text-o"
                        },
                        {
                            title: "----"
                        },
                        {
                            title: "File",
                            cmd: "new-file",
                            uiIcon: "fa-file-o"
                        },
                        {
                            title: "Directory",
                            cmd: "new-directory",
                            uiIcon: "fa-folder-o"
                        }
                    ]
                },
                {
                    title: "Upload",
                    cmd: "upload",
                    uiIcon: "fa-upload"
                },
                {
                    title: "----"
                },
                {
                    title: "Copy",
                    cmd: "copy",
                    uiIcon: "fa-files-o"
                },
                {
                    title: "Paste",
                    cmd: "paste",
                    uiIcon: "fa-clipboard"
                },
                {
                    title: "Rename",
                    cmd: "rename"
                },
                {
                    title: "----"
                },
                {
                    title: "Delete",
                    cmd: "delete",
                    uiIcon: "fa-trash-o"
                }
            ],
            select: function (event, ui) {
                onFileTreeCommand(ui.cmd, keyPathMap[ui.target.parent().attr("data-key")]);
            },
            beforeOpen: function (event, ui) {
                const menu = ui.menu;
                tree.fancytree("getTree").activateKey(ui.target.parent().attr("data-key"));
                $("#tree-container").toggleClass("tree-contextmenu-open", true);
                menu.find(".ui-icon").addClass("fa");
                menu.find(".ui-icon-caret-1-e").addClass("fa-caret-right").removeClass("ui-icon-caret-1-e");
            },
            open: function () {
                $("#tree-container").toggleClass("tree-contextmenu-open", true);
            },
            close: function () {
                $("#tree-container").toggleClass("tree-contextmenu-open", false);
            }
        };
    }
});