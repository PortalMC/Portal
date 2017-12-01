export function getPanelLayoutSettings() {
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

export function getTreeSettings() {
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

        source: []
    };
}

export function getTreeContextMenuSettings() {
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
        ]
    };
}