class LoginModel {
    Authenticate: KnockoutObservable<authenticate> = ko.observable(new authenticate);
    HideCancelButton: KnockoutObservable<boolean> = ko.observable(false);
    LoginError: KnockoutObservable<string> = ko.observable("");
    LoginType: KnockoutObservable<string> = ko.observable("");
    LoginUseLocal: KnockoutObservable<boolean> = ko.observable(false);
    LoginUseEITSSO: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    ShowLocalLoginButton: KnockoutObservable<boolean> = ko.observable(false);
    Validating: KnockoutObservable<boolean> = ko.observable(false);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    /**
     * Called to perform a local login when the Local Login form is used.
     */
    LocalLogin(): boolean {
        if (!tsUtilities.HasValue(this.Authenticate().username())) {
            tsUtilities.DelayedFocus("local-username");
            return false;
        }
        if (!tsUtilities.HasValue(this.Authenticate().password())) {
            tsUtilities.DelayedFocus("local-password");
            return false;
        }

        return true;
    }

    /**
     * Called when the user clicks the cancel button on the Local Login view.
     */
    LocalLoginCancel(): void {
        this.LoginType("");
    }

    /**
     * A computed observable that determines which login options to show and whether or not
     * to even show options if there is only a single option available.
     */
    LoginOptions = ko.computed((): string[] => {
        let output: string[] = [];
        let allowLocalLogin: boolean = false;
        let allowLoginEITSSO: boolean = false;

        if (this.MainModel().TenantId() == this.MainModel().Guid1()) {
            output.push("local");
            allowLocalLogin = true;
        } else {
            if (this.MainModel().Tenant() != null && this.MainModel().Tenant().tenantSettings() != null) {
                this.MainModel().Tenant().tenantSettings().loginOptions().forEach(function (item) {
                    if (item.toLowerCase() == "local") {
                        allowLocalLogin = true;
                    } else if (item.toLowerCase() == "eitsso") {
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
            this.LoginUseLocal(allowLocalLogin);
            this.ShowLocalLoginButton(output.length > 1);
        }

        this.LoginUseEITSSO(allowLoginEITSSO);

        return output;
    });

    /**
     * Handles the click of the various login option buttons on the login page.
     * @param type {string} - The type of login. Currently we have "local" and "eitsso" (using our Okta authentication)
     */
    Login(type: string): void {
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

                let url: string = "https://sso.em.wsu.edu/Login/SSO?Redirect=" + encodeURI(window.baseURL + "SSO/Authenticate?redirect=" + encodeURI(window.baseURL + window.tenantCode));
                window.location.href = url;
                break;
        }
    }

    /**
     * Called when the URL view is "Login" to configure the view for the page.
     */
    LoginPageLoaded(): void {
        if (this.LoginOptions().length == 1 && this.LoginOptions()[0] == "local") {
            this.HideCancelButton(true);
            this.Login("local");
        }

        if (tsUtilities.HasValue(this.MainModel().Id()) && this.MainModel().Id().toLowerCase() == "localloginerror") {
            this.Login("local");
            let error: string = tsUtilities.CookieRead("login-error");
            if (!tsUtilities.HasValue(error)) {
                error = this.MainModel().Language("InvalidUsernameOrPassword");
            } else {
                error = decodeURIComponent(error);
            }

            this.LoginError(error);
        }
    }

    /**
     * The URL to this login page
     */
    LoginUrl(): string {
        let output: string = window.baseURL;
        if (window.useTenantCodeInUrl) {
            output += this.MainModel().Tenant().tenantCode() + "/";
        }
        output += "Login/";
        return output;
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        this.LoginError("");

        switch (this.MainModel().CurrentView()) {
            case "login":
                this.LoginPageLoaded();
                break;
        }
    }
}