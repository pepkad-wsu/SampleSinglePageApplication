var Page1Model = /** @class */ (function () {
    function Page1Model() {
        var _this = this;
        this.Html = ko.observable("");
        this.MainModel = ko.observable(window.mainModel);
        this.User = ko.observable(new user);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    Page1Model.prototype.Page1Loaded = function () {
        var _this = this;
        tsUtilities.HtmlEditor("sample-html-editor", {
            onupdate: function (html) {
                _this.Html(html);
            },
            placeholderText: "Enter Your HTML Here"
        });
        this.User(new user);
        this.MainModel().AjaxUserAutocomplete("sample-user-lookup", function (data) {
            if (data != null) {
                _this.User().userId(data.value);
                _this.User().displayName(data.label);
            }
        });
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    Page1Model.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "page1":
                this.Page1Loaded();
                break;
        }
    };
    return Page1Model;
}());
//# sourceMappingURL=page1.js.map