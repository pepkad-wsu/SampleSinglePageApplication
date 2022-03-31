var Page2Model = /** @class */ (function () {
    function Page2Model() {
        var _this = this;
        this.MainModel = ko.observable(window.mainModel);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    Page2Model.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "page2":
                break;
        }
    };
    return Page2Model;
}());
//# sourceMappingURL=page2.js.map