$(function() {

    var isConfirmClicked = false;

    setupDropdown("projects-new-version-minecraft");
    setupDropdown("projects-new-version-forge");

    checkInputChange("form-name");
    checkInputChange("form-description");
    checkDropdownChange("form-minecraft-version");
    checkDropdownChange("form-forge-version");

    $("#form-confirm").click((e) => {
        e.preventDefault();
        postNewProject();
    });

    function postNewProject() {
        isConfirmClicked = true;

        $("#form-confirm").prop("disabled", true);
        $("#form-confirm").text($("#form-confirm").attr("data-text-2"));

        var name = $("#form-name").val();
        var description = $("#form-description").val();
        var minecraftVersion = $("#form-minecraft-version").val();
        var forgeVersion = $("#form-forge-version").val();

        var xhr = new XMLHttpRequest();
        xhr.open("POST", getApiBaseAddress() + "projects");
        xhr.setRequestHeader("Content-Type", "application/json");
        var data = {
            name: name,
            description: description,
            minecraftVersion: minecraftVersion,
            forgeVersion: forgeVersion
        };
        xhr.onload = function() {
            var result = JSON.parse(xhr.responseText);
            if (result["success"]) {
                window.location.href = "/Projects/" + result["data"]["id"];
            } else {
                $("#form-confirm").prop("disabled", false);
                $("#form-confirm").text($("#form-confirm").attr("data-text-1"));
            }
        };
        xhr.onerror = function() {
            console.log("error!");
            $("#form-confirm").prop("disabled", false);
            $("#form-confirm").text($("#form-confirm").attr("data-text-1"));
        };
        xhr.send(JSON.stringify(data));
    };

    function getBaseAddress() {
        var url = window.location.protocol + "//";
        url += window.location.host;
        return url;
    }

    function getApiBaseAddress() {
        return getBaseAddress() + "/api/v1/";
    }

    function setupDropdown(id) {
        $(`#${id} .dropdown-menu li a`).click(function() {
            $(`#${id} .dropdown-text`).text($(this).text());
            $(`#${id} button`).val($(this).text()).trigger("change");
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
        var name = $("#form-name").val();
        var description = $("#form-description").val();
        var minecraftVersion = $("#form-minecraft-version").val();
        var forgeVersion = $("#form-forge-version").val();
        $("#form-confirm").prop("disabled",
            anyMatch("",
                name,
                description,
                minecraftVersion,
                forgeVersion));
    }

    function anyMatch() {
        for (var i = 1; i < arguments.length; ++i) {
            if (arguments[0] === arguments[i]) {
                return true;
            }
        }
        return false;
    }
});