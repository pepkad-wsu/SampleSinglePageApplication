class LanguageModel {
    LanguageItems: KnockoutObservableArray<optionPair> = ko.observableArray([]);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    /**
     * Gets the default language value for a given language item.
     * @param id {string} - The unique language item element name.
     */
    DefaultLanguageItem(id: string): string {
        let output: string = "";

        let def: optionPair = ko.utils.arrayFirst(this.MainModel().DefaultLanguageItems(), function (item) {
            return item.id() == id;
        });
        if (def != null) {
            output = def.value();
        }

        return output;
    }

    /**
     * Called when the URL view is "Language" to load the local values to display on the page to be edited.
     */
    GetLanguage():void {
        let items: optionPair[] = [];
        this.MainModel().LanguageItems().forEach(function (e) {
            let item: optionPair = new optionPair();
            item.Load(JSON.parse(ko.toJSON(e)));
            items.push(item);
        });
        items = items.sort(function (l, r) {
            return l.id() > r.id() ? 1 : -1;
        });
        this.LanguageItems(items);

    }

    /**
     * Handles resetting a given language item to the default value. When editing language items if an item
     * is different than the default value then a button is show to reset the value to the default.
     * @param id {string} - The unique language item.
     */
    ResetLanguageDefault(id: string): void {
        let def: string = this.DefaultLanguageItem(id);

        let languageItem: optionPair = ko.utils.arrayFirst(this.LanguageItems(), function (item) {
            return item.id() == id;
        });
        if (languageItem != null) {
            languageItem.value(def);
        }
    }

    /**
     * Saves the language items to the WebAPI endpoint.
     */
    SaveLanguage(): void {
        this.MainModel().Message_Saving();

        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();    

            if (data != null) {
                if (data.result) {
                    let items: optionPair[] = [];
                    this.LanguageItems().forEach(function (e) {
                        let item: optionPair = new optionPair();
                        item.Load(JSON.parse(ko.toJSON(e)));
                        items.push(item);
                    });
                    this.MainModel().LanguageItems(items);

                    this.MainModel().Message(this.MainModel().Language("Saved"), StyleType.Success, true, true);
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save language items.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveLanguage/" + window.tenantId, ko.toJSON(this.LanguageItems), success);
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged(): void {
        switch (this.MainModel().CurrentView()) {
            case "language":
                this.GetLanguage();
                break;
        }
    }
}