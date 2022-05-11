var LanguageModel = /** @class */ (function () {
    function LanguageModel() {
        var _this = this;
        this.LanguageItems = ko.observableArray([]);
        this.MainModel = ko.observable(window.mainModel);
        this.ModifiedItemsOnly = ko.observable(false);
        /**
         * Determines whether the reset should be available
         */
        this.ResetAvailable = ko.computed(function () {
            var output = false;
            if (_this.LanguageItems() != null && _this.LanguageItems().length > 0) {
                var t_1 = _this;
                _this.LanguageItems().forEach(function (item) {
                    if (item.value() != t_1.DefaultLanguageItem(item.id())) {
                        output = true;
                    }
                });
            }
            return output;
        });
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
     * Gets the default language value for a given language item.
     * @param id {string} - The unique language item element name.
     */
    LanguageModel.prototype.DefaultLanguageItem = function (id) {
        var output = "";
        var def = ko.utils.arrayFirst(this.MainModel().DefaultLanguageItems(), function (item) {
            return item.id() == id;
        });
        if (def != null) {
            output = def.value();
        }
        return output;
    };
    /**
     * Called when the URL view is "Language" to load the local values to display on the page to be edited.
     */
    LanguageModel.prototype.GetLanguage = function () {
        var items = [];
        this.MainModel().LanguageItems().forEach(function (e) {
            var item = new optionPair();
            item.Load(JSON.parse(ko.toJSON(e)));
            items.push(item);
        });
        items = items.sort(function (l, r) {
            return l.id() > r.id() ? 1 : -1;
        });
        this.LanguageItems(items);
    };
    /**
     * Resets all language back to defaults.
     */
    LanguageModel.prototype.ResetAll = function () {
        if (window.objDefaultLanguage != undefined) {
            var options_1 = [];
            window.objDefaultLanguage.forEach(function (e) {
                var item = new optionPair();
                item.Load(e);
                options_1.push(item);
            });
            this.LanguageItems(options_1);
        }
    };
    /**
     * Handles resetting a given language item to the default value. When editing language items if an item
     * is different than the default value then a button is show to reset the value to the default.
     * @param id {string} - The unique language item.
     */
    LanguageModel.prototype.ResetLanguageDefault = function (id) {
        var def = this.DefaultLanguageItem(id);
        var languageItem = ko.utils.arrayFirst(this.LanguageItems(), function (item) {
            return item.id() == id;
        });
        if (languageItem != null) {
            languageItem.value(def);
        }
    };
    /**
     * Saves the language items to the WebAPI endpoint.
     */
    LanguageModel.prototype.SaveLanguage = function () {
        var _this = this;
        this.MainModel().Message_Saving();
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    var items_1 = [];
                    _this.LanguageItems().forEach(function (e) {
                        var item = new optionPair();
                        item.Load(JSON.parse(ko.toJSON(e)));
                        items_1.push(item);
                    });
                    _this.MainModel().LanguageItems(items_1);
                    _this.MainModel().Message(_this.MainModel().Language("Saved"), StyleType.Success, true, true);
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to save language items.");
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveLanguage/" + window.tenantId, ko.toJSON(this.LanguageItems), success);
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    LanguageModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "language":
                this.GetLanguage();
                break;
        }
    };
    return LanguageModel;
}());
//# sourceMappingURL=language.js.map