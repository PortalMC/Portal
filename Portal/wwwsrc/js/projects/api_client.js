export default class {
    constructor(baseAddress) {
        this.baseAddress = baseAddress;
    }

    updateProjectDescription(projectUuid, description) {
        const data = {
            description: description
        };
        // noinspection ES6ModulesDependencies
        return $.ajax({
            url: `${this.baseAddress}projects/${projectUuid}`,
            type: "patch",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json"
        });
    }

    createNewProject(name, description, minecraftVersionId, forgeVersionId) {
        const data = {
            name: name,
            description: description,
            minecraftVersionId: minecraftVersionId,
            forgeVersionId: forgeVersionId
        };
        // noinspection ES6ModulesDependencies
        return $.ajax({
            url: `${this.baseAddress}projects`,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json"
        });
    }

    getForgeVersions(minecraftVersionId) {
        // noinspection ES6ModulesDependencies
        return $.ajax({
            url: `${this.baseAddress}minecraft/versions/${minecraftVersionId}`,
            type: "get",
            dataType: "json"
        })
    }
};