class SamplePageModel {
    Html: KnockoutObservable<string> = ko.observable("");
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    User: KnockoutObservable<user> = ko.observable(new user);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    ColorboxTest(): void {
        this.MainModel().ViewImage("https://picsum.photos/600/400");
    }

    GenerateGuid(): void {
        $("#generated-guid").html("Generated Guid: " + tsUtilities.GenerateGuid());
        $("#generated-guid").show();
    }

    Load(): void {
        tsUtilities.HtmlEditor("sample-html-editor", {
            onupdate: (html: string) => {
                this.Html(html);
            },
            placeholderText: this.MainModel().Language("HtmlEditorPlaceholder")
        });

        this.User(new user);

        this.MainModel().AjaxUserAutocomplete("sample-user-lookup",
            (data: server.ajaxResults) => {
                if (data != null) {
                    this.User().userId(data.value);
                    this.User().displayName(data.label);
                }
            }, true
        );

        tsUtilities.DelayedFocus("sample-user-lookup");
    }

    ModalTest(): void {
        let closeDialog: Function = () => {
            tsUtilities.ModalDialogHide();
        };

        let okClicked: Function = () => {
            tsUtilities.Confirmation("Are you sure you want to close", false, [
                { Text: "Cancel", Icon: "auto", Class: "btn btn-dark", ClosesConfirmation: true },
                { Text: "Yes", Icon: "auto", Class: "btn btn-success", CallbackHandler: closeDialog }
            ]);
        };

        let cancelClicked: Function = () => {
            // Do whatever code you want here when the cancel button is clicked.
        };

        tsUtilities.Modal("Enter your message or <i>HTML</i> here.", "Test Modal", "50%", [
            { Text: "Cancel", Icon: "fas fa-times", Class: "btn btn-dark", CallbackHandler: cancelClicked, ClosesDialog: true, Position: "bottom" },
            { Text: "Ok", Icon: "fa-solid fa-check", Class: "btn btn-success", CallbackHandler: okClicked, ClosesDialog: false, Position: "bottom" }
        ]);
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "samplepage":
                this.Load();
                break;
        }
    }
}