class LanguageModel {
    Culture: KnockoutObservable<string> = ko.observable("");
    Language: KnockoutObservable<language> = ko.observable(new language);
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    ModifiedItemsOnly: KnockoutObservable<boolean> = ko.observable(false);
    NewLanguage: KnockoutObservable<string> = ko.observable("");
    View: KnockoutObservable<string> = ko.observable("");

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    Add(): void {
        this.MainModel().Nav("Language", "Add");
    }

    AddLanguage(language: optionPair): void {
        //console.log("AddLanguage", ko.toJSON(language));
        this.MainModel().Nav("Language", language.id());
    }

    CurrentLanguages = ko.computed((): optionPair[] => {
        let output: optionPair[] = [];

        if (this.MainModel().Cultures() != null && this.MainModel().Cultures().length > 0) {

        }

        return output;
    });

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
    GetLanguage(): void {
        //let items: optionPair[] = [];
        //this.MainModel().LanguageItems().forEach(function (e) {
        //    let item: optionPair = new optionPair();
        //    item.Load(JSON.parse(ko.toJSON(e)));
        //    items.push(item);
        //});
        //items = items.sort(function (l, r) {
        //    return l.id() > r.id() ? 1 : -1;
        //});
        //this.LanguageItems(items);

        let success: Function = (data: server.language) => {
            if (data != null) {
                this.Language().Load(data);
                this.Loading(false);
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load the language.");
            }
        };

        this.View("edit");
        this.Loading(true);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetLanguage/" + this.MainModel().Id(), null, success);
    }

    LanguageTitle = ko.computed((): string => {
        let output: string = this.MainModel().Language("Language");;

        if (tsUtilities.HasValue(this.MainModel().Id())) {
            if (this.MainModel().Id().toLowerCase() != "add") {
                output =
                    this.MainModel().Language("EditLanguage") +
                    " - " + this.MainModel().LanguageName(this.MainModel().Id()) +
                    " [" + this.MainModel().Id() + "]";
            } else {
                output = this.MainModel().Language("AddLanguage");
            }
        }

        return output;
    });

    Load(): void {
        if (tsUtilities.HasValue(this.MainModel().Id())) {
            if (this.MainModel().Id().toLowerCase() == "add") {
                this.View("add");
            } else {
                this.GetLanguage();
            }
        }
    }

    /**
     * Resets all language back to defaults.
     */
    ResetAll(): void {
        if (window.objDefaultLanguage != undefined) {
            let options: optionPair[] = [];
            window.objDefaultLanguage.forEach(function (e) {
                let item: optionPair = new optionPair();
                item.Load(e);
                options.push(item);
            });
            this.Language().phrases(options);
        }
    }

    /**
     * Determines whether the reset should be available
     */
    ResetAvailable = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Language().phrases() != null && this.Language().phrases().length > 0) {
            let t: this = this;
            this.Language().phrases().forEach(function (item) {
                if (item.value() != t.DefaultLanguageItem(item.id())) {
                    output = true;
                }
            });
        }

        return output;
    });

    /**
     * Handles resetting a given language item to the default value. When editing language items if an item
     * is different than the default value then a button is show to reset the value to the default.
     * @param id {string} - The unique language item.
     */
    ResetLanguageDefault(id: string): void {
        let def: string = this.DefaultLanguageItem(id);

        let languageItem: optionPair = ko.utils.arrayFirst(this.Language().phrases(), function (item) {
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
                    this.MainModel().Nav("Language");
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save language items.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveLanguage/" + window.tenantId, ko.toJSON(this.Language), success);
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged(): void {
        this.Culture("");
        this.Language(new language);
        this.Loading(false);
        this.ModifiedItemsOnly(false);
        this.View("");

        let allowed: boolean = this.MainModel().AdminUser();

        switch (this.MainModel().CurrentView()) {
            case "language":
                if (allowed) {
                    this.Load();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
        }
    }
}