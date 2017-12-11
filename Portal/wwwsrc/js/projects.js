import ApiClient from './projects/api_client';

$(function () {
    const apiClient = new ApiClient(`${window.location.protocol}//${window.location.host}/api/v1/`);

    let isConfirmClicked = false;

    setupDropdown("projects-new-version-minecraft");
    setupDropdown("projects-new-version-forge");

    checkInputChange("form-name");
    checkInputChange("form-description");
    checkDropdownChange("form-minecraft-version");
    checkDropdownChange("form-forge-version");

    $("#form-minecraft-version").change(() => {
        const minecraftVersion = $("#form-minecraft-version").val();
        updateForgeVersion(minecraftVersion);
    });

    $("#form-confirm").click((e) => {
        e.preventDefault();
        postNewProject();
    });

    $("#project-description-edit").click((e) => {
        e.preventDefault();
        $("#project-description-container").css("display", "none");
        $("#project-description-edit-container").css("display", "flex");
    });

    $("#project-description-edit-cancel").click((e) => {
        e.preventDefault();
        $("#project-description-container").css("display", "flex");
        $("#project-description-edit-container").css("display", "none");
        $("#form-description").val($("#project-description").text().trim());
    });

    $("#project-description-edit-confirm").click((e) => {
        e.preventDefault();
        updateDescription();
    });

    function updateDescription() {
        const projectUuid = window.location.pathname.match(/\/Projects\/([a-zA-Z0-9-]*)\/?/)[1];
        const description = $("#form-description").val();
        apiClient.updateProjectDescription(projectUuid, description)
            .done(() => {
                const projectDescription = $("#project-description");
                if (description === null || description.length === 0) {
                    projectDescription.addClass("project-description-empty");
                    projectDescription.text(projectDescription.attr("data-empty"));
                } else {
                    projectDescription.removeClass("project-description-empty");
                    projectDescription.text(description);
                }
                $("#project-description-container").css("display", "flex");
                $("#project-description-edit-container").css("display", "none");
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
            });
    }

    function postNewProject() {
        isConfirmClicked = true;

        const formConfirm = $("#form-confirm");
        formConfirm.prop("disabled", true);
        formConfirm.text(formConfirm.attr("data-text-2"));

        const name = $("#form-name").val();
        const description = $("#form-description").val();
        const minecraftVersion = $("#form-minecraft-version").val();
        const forgeVersion = $("#form-forge-version").val();
        apiClient.createNewProject(name, description, minecraftVersion, forgeVersion)
            .done(result => {
                if (result["success"]) {
                    window.location.href = `/Projects/${result["data"]["id"]}`;
                } else {
                    formConfirm.prop("disabled", false);
                    formConfirm.text(formConfirm.attr("data-text-1"));
                }
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
                formConfirm.prop("disabled", false);
                formConfirm.text(formConfirm.attr("data-text-1"));
            });
    }

    function updateForgeVersion(minecraftVersionId) {
        apiClient.getForgeVersions(minecraftVersionId)
            .done(data => {
                const projectsNewVersionForge = $("#projects-new-version-forge-list");
                projectsNewVersionForge.empty();
                const forgeVersions = data["forge_versions"];
                for (let i = 0; i < forgeVersions.length; ++i) {
                    const forgeVersion = forgeVersions[i];
                    let versionText = forgeVersion["name"];
                    if (forgeVersion["is_recommend"]) {
                        versionText += " ★";
                    }
                    projectsNewVersionForge.append(
                        `<li><a href="#" data-key="${forgeVersion["id"]}">${versionText}</a></li>`);
                }
                const dropdown = $("#projects-new-version-forge");
                const dropdownText = dropdown.find(".dropdown-text");
                dropdown.find("button").val("");
                dropdownText.text(dropdownText.attr("data-default"));
                setupDropdown("projects-new-version-forge");
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
            });
    }

    function setupDropdown(id) {
        $(`#${id}`).find(".dropdown-menu li a").click(function () {
            const dropdown = $(`#${id}`);
            dropdown.find(".dropdown-text").text($(this).text());
            dropdown.find("button").val($(this).attr("data-key")).trigger("change");
        });
    }

    function checkInputChange(id) {
        $(`#${id}`).keyup(() => {
            validateToConfirm();
        });
    }

    function checkDropdownChange(id) {
        $(`#${id}`).change(() => {
            validateToConfirm();
        });
    }

    function validateToConfirm() {
        if (isConfirmClicked) {
            return;
        }
        const name = $("#form-name").val();
        const minecraftVersion = $("#form-minecraft-version").val();
        const forgeVersion = $("#form-forge-version").val();
        $("#form-confirm").prop("disabled",
            anyMatch("",
                name,
                minecraftVersion,
                forgeVersion));
    }

    function anyMatch() {
        for (let i = 1; i < arguments.length; ++i) {
            if (arguments[0] === arguments[i]) {
                return true;
            }
        }
        return false;
    }
});