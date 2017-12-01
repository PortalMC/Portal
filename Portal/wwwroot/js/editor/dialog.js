export function showMessageDialog() {

}

export function showSingleInputDialog(title, message, defaultValue, placeholder, yesButton, noButton, yesAction, noAction) {
    $(`<div>
             <p class="dialog-message">${message}</p>
             <form>
               <input type="text" name="name" id="name" value="${defaultValue}" placeholder="${placeholder}" class="text ui-widget-content ui-corner-all">
             </form>
           </div>`).dialog({
        title: title,
        dialogClass: "dialog-root",
        width: 400,
        buttons: [
            {
                text: yesButton,
                click: function () {
                    $(this).dialog('close');
                    yesAction($(this).find("input[type=text]").val());
                }
            },
            {
                text: noButton,
                click: function () {
                    $(this).dialog('close');
                    noAction();
                }
            }
        ],
        modal: true,
        close: function () {
            $(this).remove();
        }
    });
}