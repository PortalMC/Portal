export function showMessageDialog() {

}

export function showSingleInputDialog(title, message, defaultValue, placeholder, yesButton, noButton, yesAction, noAction = undefined) {
    const dialog = $(
        `<div>
           <p class="dialog-message">${message}</p>
           <form>
             <input type="text" name="name" id="name" value="${defaultValue}" placeholder="${placeholder}" class="text ui-widget-content ui-corner-all">
             <input type="submit" tabindex="-1" style="position:absolute; top:-1000px">
           </form>
         </div>`).dialog({
        title: title,
        dialogClass: "dialog-root",
        width: 400,
        buttons: [
            {
                text: yesButton,
                click: function () {
                    $(this).dialog("close");
                    yesAction($(this).find("input[type=text]").val());
                }
            },
            {
                text: noButton,
                click: function () {
                    $(this).dialog("close");
                    if (noAction) {
                        noAction();
                    }
                }
            }
        ],
        modal: true,
        close: function () {
            $(this).remove();
        }
    });
    dialog.find("form").on("submit", function (event) {
        event.preventDefault();
        dialog.dialog("close");
        yesAction($(this).find("input[type=text]").val());
    });
}

export function showConfirmDialog(title, message, yesButton, noButton, yesAction, noAction = undefined) {
    $(
        `<div>
           <p class="dialog-message">${message}</p>
         </div>`).dialog({
        title: title,
        dialogClass: "dialog-root",
        width: 400,
        buttons: [
            {
                text: yesButton,
                click: function () {
                    $(this).dialog("close");
                    yesAction();
                }
            },
            {
                text: noButton,
                click: function () {
                    $(this).dialog("close");
                    if (noAction) {
                        noAction();
                    }
                }
            }
        ],
        modal: true,
        close: function () {
            $(this).remove();
        }
    });
}