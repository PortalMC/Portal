export default class {
    constructor(baseAddress, projectId) {
        this.baseAddress = baseAddress;
        this.projectId = projectId;
    }

    getProjectFileList() {
        // noinspection ES6ModulesDependencies
        return $.ajax({
            url: `${this.baseAddress}projects/${this.projectId}/file/list`,
            type: "get",
            dataType: "json"
        })
    }

    getProjectFile(path) {
        const data = {
            path: path
        };
        // noinspection ES6ModulesDependencies
        return $.ajax({
            url: `${this.baseAddress}projects/${this.projectId}/file/get`,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json"
        })
    }

    saveProjectFile(path, content) {
        const data = {
            path: path,
            content: content
        };
        // noinspection ES6ModulesDependencies
        return $.ajax({
            url: `${this.baseAddress}projects/${this.projectId}/file/edit`,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json"
        });
    }

    startProjectBuild() {
        // noinspection ES6ModulesDependencies
        return $.ajax({
            url: `${this.baseAddress}projects/${this.projectId}/build`,
            type: "post",
            dataType: "json"
        });
    }
};