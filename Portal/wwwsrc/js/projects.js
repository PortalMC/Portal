import ApiClient from './projects/api_client';

$(function () {
    const apiClient = new ApiClient(`${window.location.protocol}//${window.location.host}/api/v1/`);

    setupDropdown("new-version-minecraft");

    $("#form-minecraft-version-id").change(() => {
        const minecraftVersion = $("#form-minecraft-version-id").val();
        updateForgeVersion(minecraftVersion);
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

    function updateForgeVersion(minecraftVersionId) {
        const dropdown = $("#new-version-forge").find(".dropdown");
        const dropdownMenu = dropdown.find(".dropdown-menu");
        const dropdownButton = dropdown.find("button");
        dropdownMenu.empty();
        $("#form-forge-version-id").val("");
        dropdownButton.text(dropdownButton.attr("data-default-text"));
        dropdownButton.addClass("disabled");

        const template = $('#template-forge-version-item')[0].content;
        apiClient.getForgeVersions(minecraftVersionId)
            .done(data => {
                const forgeVersions = data["forge_versions"];
                for (let i = 0; i < forgeVersions.length; ++i) {
                    const forgeVersion = forgeVersions[i];
                    let versionText = forgeVersion["name"];
                    if (forgeVersion["is_recommend"]) {
                        versionText += " ★";
                    }
                    const menu = document.importNode(template, true);
                    menu.firstElementChild.innerText = versionText;
                    menu.firstElementChild.dataset.key = forgeVersion["id"];
                    dropdownMenu.append(menu);
                }
                setupDropdown("new-version-forge");
                dropdownButton.removeClass("disabled");
            })
            .fail((jqXhr, textStatus, errorThrown) => {
                console.log("error! : " + errorThrown.toString());
            });
    }

    function setupDropdown(id) {
        const dropdown = $(`#${id}`);
        const dummyFormId = dropdown.attr("data-dummy-form-id");
        dropdown.find(".dropdown-menu a").click(function () {
            const dropdown = $(`#${id}`);
            const button = dropdown.find("button");
            button.text($(this).text());
            $(`#${dummyFormId}`).val($(this).attr("data-key")).trigger("change");
        });
    }
});