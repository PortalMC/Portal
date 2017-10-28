$(function () {
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

    function postNewProject() {
        isConfirmClicked = true;

        const form_confirm = $("#form-confirm");
        form_confirm.prop("disabled", true);
        form_confirm.text(form_confirm.attr("data-text-2"));

        const name = $("#form-name").val();
        const description = $("#form-description").val();
        const minecraftVersion = $("#form-minecraft-version").val();
        const forgeVersion = $("#form-forge-version").val();
        const data = {
            name: name,
            description: description,
            minecraftVersion: minecraftVersion,
            forgeVersion: forgeVersion
        };
        $.ajax({
            url: getApiBaseAddress() + "projects",
            type: "post",
            data: JSON.stringify(data),
            contentType: 'application/json',
            dataType: "json",
        }).done(result => {
            if (result["success"]) {
                window.location.href = "/Projects/" + result["data"]["id"];
            } else {
                form_confirm.prop("disabled", false);
                form_confirm.text(form_confirm.attr("data-text-1"));
            }
        }).fail((jqXHR, textStatus, errorThrown) => {
            console.log("error!");
            form_confirm.prop("disabled", false);
            form_confirm.text(form_confirm.attr("data-text-1"));
        });
    }

    function updateForgeVersion(minecraftVersion) {
        $.ajax({
            url: getApiBaseAddress() + "minecraft/versions/" + minecraftVersion,
            type: 'get',
            dataType: 'json',
            timeout: 5000,
        }).done(data => {
            const forgeVersions = data["forge_versions"];
            for (let i = 0; i < forgeVersions.length; ++i) {
                const forgeVersion = forgeVersions[i];
                let versionText = forgeVersion["name"];
                if (forgeVersion["is_recommend"]) {
                    versionText += " ★";
                }
                const projects_new_version_forge = $("#projects-new-version-forge-list");
                projects_new_version_forge.empty();
                projects_new_version_forge.append(`<li><a href="#" data-value="${forgeVersion["name"]}">${versionText}</a></li>`);
            }
            const dropdown = $("#projects-new-version-forge");
            const dropdownText = dropdown.find(".dropdown-text");
            dropdown.find("button").val("");
            dropdownText.text(dropdownText.attr("data-default"));
            setupDropdown("projects-new-version-forge");
        }).fail((jqXHR, textStatus, errorThrown) => {
        });
    }

    function getBaseAddress() {
        let url = window.location.protocol + "//";
        url += window.location.host;
        return url;
    }

    function getApiBaseAddress() {
        return getBaseAddress() + "/api/v1/";
    }

    function setupDropdown(id) {
        $(`#${id}`).find(`.dropdown-menu li a`).click(function () {
            const dropdown = $(`#${id}`);
            dropdown.find(`.dropdown-text`).text($(this).text());
            dropdown.find(`button`).val($(this).attr("data-value")).trigger("change");
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
