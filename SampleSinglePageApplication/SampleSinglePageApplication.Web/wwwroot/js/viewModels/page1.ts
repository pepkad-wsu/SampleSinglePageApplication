class Page1Model {
    Html: KnockoutObservable<string> = ko.observable("");
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    User: KnockoutObservable<user> = ko.observable(new user);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }


    Page1Loaded(): void {
        tsUtilities.HtmlEditor("sample-html-editor", {
            onupdate: (html: string) => {
                this.Html(html);
            },
            placeholderText: "Enter Your HTML Here"
        });

        this.User(new user);

        this.MainModel().AjaxUserAutocomplete("sample-user-lookup",
            (data: server.ajaxResults) => {
                if (data != null) {
                    this.User().userId(data.value);
                    this.User().displayName(data.label);
                }
            }
        );
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "page1":
                this.Page1Loaded();
                break;
        }
    }
}