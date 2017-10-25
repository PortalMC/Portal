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
        var name = $("#form-name").val();
        var description = $("#form-description").val();
        var minecraftVersion = $("#form-minecraft-version").val();
        var forgeVersion = $("#form-forge-version").val();


    });

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