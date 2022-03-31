var LoginModel = /** @class */ (function () {
    function LoginModel() {
        var _this = this;
        this.Authenticate = ko.observable(new authenticate);
        this.HideCancelButton = ko.observable(false);
        this.LoginError = ko.observable("");
        this.LoginType = ko.observable("");
        this.LoginUseLocal = ko.observable(false);
        this.LoginUseEITSSO = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.ShowLocalLoginButton = ko.observable(false);
        this.Validating = ko.observable(false);
        /**
         * A computed observable that determines which login options to show and whether or not
         * to even show options if there is only a single option available.
         */
        this.LoginOptions = ko.computed(function () {
            var output = [];
            var allowLocalLogin = false;
            var allowLoginEITSSO = false;
            if (_this.MainModel().TenantId() == _this.MainModel().Guid1()) {
                output.push("local");
                allowLocalLogin = true;
            }
            else {
                if (_this.MainModel().Tenant() != null && _this.MainModel().Tenant().tenantSettings() != null) {
                    _this.MainModel().Tenant().tenantSettings().loginOptions().forEach(function (item) {
                        if (item.toLowerCase() == "local") {
                            allowLocalLogin = true;
                        }
                        else if (item.toLowerCase() == "eitsso") {
                            allowLoginEITSSO = true;
                        }
                        output.push(item);
                    });
                }
            }
            if (output.length == 0) {
                output.push("local");
                allowLocalLogin = true;
            }
            if (allowLocalLogin) {
                _this.LoginUseLocal(allowLocalLogin);
                _this.ShowLocalLoginButton(output.length > 1);
            }
            _this.LoginUseEITSSO(allowLoginEITSSO);
            return output;
        });
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
     * Called to perform a local login when the Local Login form is used.
     */
    LoginModel.prototype.LocalLogin = function () {
        if (!tsUtilities.HasValue(this.Authenticate().username())) {
            tsUtilities.DelayedFocus("local-username");
            return false;
        }
        if (!tsUtilities.HasValue(this.Authenticate().password())) {
            tsUtilities.DelayedFocus("local-password");
            return false;
        }
        return true;
    };
    /**
     * Called when the user clicks the cancel button on the Local Login view.
     */
    LoginModel.prototype.LocalLoginCancel = function () {
        this.LoginType("");
    };
    /**
     * Handles the click of the various login option buttons on the login page.
     * @param type {string} - The type of login. Currently we have "local" and "eitsso" (using our Okta authentication)
     */
    LoginModel.prototype.Login = function (type) {
        this.MainModel().Message_Hide();
        this.LoginType(type);
        switch (type.toLowerCase()) {
            case "local":
                this.Validating(false);
                this.Authenticate().username("");
                this.Authenticate().password("");
                tsUtilities.DelayedFocus("local-username");
                break;
            case "eitsso":
                $("#main-model").hide();
                $("#page-view-model-area").hide();
                var url = "https://sso.em.wsu.edu/Login/SSO?Redirect=" + encodeURI(window.baseURL + "SSO/Authenticate?redirect=" + encodeURI(window.baseURL + window.tenantCode));
                window.location.href = url;
                break;
        }
    };
    /**
     * Called when the URL view is "Login" to configure the view for the page.
     */
    LoginModel.prototype.LoginPageLoaded = function () {
        if (this.LoginOptions().length == 1 && this.LoginOptions()[0] == "local") {
            this.HideCancelButton(true);
            this.Login("local");
        }
        if (tsUtilities.HasValue(this.MainModel().Id()) && this.MainModel().Id().toLowerCase() == "localloginerror") {
            this.Login("local");
            var error = tsUtilities.CookieRead("login-error");
            if (!tsUtilities.HasValue(error)) {
                error = this.MainModel().Language("InvalidUsernameOrPassword");
            }
            else {
                error = decodeURIComponent(error);
            }
            this.LoginError(error);
        }
    };
    /**
     * The URL to this login page
     */
    LoginModel.prototype.LoginUrl = function () {
        var output = window.baseURL;
        if (window.useTenantCodeInUrl) {
            output += this.MainModel().Tenant().tenantCode() + "/";
        }
        output += "Login/";
        return output;
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    LoginModel.prototype.ViewChanged = function () {
        this.LoginError("");
        switch (this.MainModel().CurrentView()) {
            case "login":
                this.LoginPageLoaded();
                break;
        }
    };
    return LoginModel;
}());
//# sourceMappingURL=login.js.map