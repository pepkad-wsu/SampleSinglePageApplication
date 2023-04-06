class LoginModel {
    Authenticate: KnockoutObservable<authenticate> = ko.observable(new authenticate);
    ConfirmPassword: KnockoutObservable<string> = ko.observable("");
    HideCancelButton: KnockoutObservable<boolean> = ko.observable(false);
    LoginMessage: KnockoutObservable<string> = ko.observable("");
    LoginMessageClass: KnockoutObservable<string> = ko.observable("");
    LoginType: KnockoutObservable<string> = ko.observable("");
    LoginUseCustom: KnockoutObservable<boolean> = ko.observable(false);
    LoginUseEITSSO: KnockoutObservable<boolean> = ko.observable(false);
    LoginUseFacebook: KnockoutObservable<boolean> = ko.observable(false);
    LoginUseGoogle: KnockoutObservable<boolean> = ko.observable(false);
    LoginUseLocal: KnockoutObservable<boolean> = ko.observable(false);
    LoginUseMicrosoftAccount: KnockoutObservable<boolean> = ko.observable(false);
    LoginUseOpenId: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    ShowLocalLoginButton: KnockoutObservable<boolean> = ko.observable(false);
    User: KnockoutObservable<user> = ko.observable(new user);
    Validating: KnockoutObservable<boolean> = ko.observable(false);
    View: KnockoutObservable<string> = ko.observable("");

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    BackToLogin() {
        this.LoginMessage("");
        this.View("");
    }

    CustomLoginButtonText = ko.computed((): string => {
        let output: string = "";

        if (tsUtilities.HasValue(this.MainModel().Tenant().tenantSettings().customAuthenticationName())) {
            output = this.MainModel().Tenant().tenantSettings().customAuthenticationName();
        }

        if (output == "") {
            output = this.MainModel().Language("CustomLoginProvider");
        }

        return output;
    });

    ExternalLogin(url: string): void {
        $("#main-model").hide();
        $("#page-view-model-area").hide();
        window.location.href = url;
    }

    ForgotPassword(): void {
        this.User(new user);
        this.User().tenantId(this.MainModel().TenantId());
        this.User().userId(this.MainModel().GuidEmpty());
        this.View("forgotpassword");
        tsUtilities.DelayedFocus("forgotpassword-email");
    }

    ForgotPasswordValidateCode(): void {
        if (!tsUtilities.HasValue(this.User().location())) {
            tsUtilities.DelayedFocus("forgot-password-validate");
            return;
        }

        let success: Function = (data: server.user) => {
            this.HideMessage();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.ShowMessage(this.MainModel().Language("PasswordResetMessage"), "success");
                } else {
                    this.ShowMessage(tsUtilities.MessagesToString(data.actionResponse.messages), "danger");
                }
            } else {
                this.ShowMessage("An unknown error occurred attempting to validate the confirmation code.", "danger");
            }
        };

        this.ShowMessage("Validating Code, Please Wait...", "dark");
        tsUtilities.AjaxData(window.baseURL + "api/Data/ForgotPasswordConfirm", ko.toJSON(this.User), success);
    }

    ForgotPasswordReset(): void {
        if (!tsUtilities.HasValue(this.User().email())) {
            tsUtilities.DelayedFocus("forgotpassword-email");
            return;
        }
        if (!tsUtilities.HasValue(this.User().password())) {
            tsUtilities.DelayedFocus("forgotpassword-password");
            return;
        }

        let success: Function = (data: server.user) => {
            this.HideMessage();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.User().Load(data);
                    this.View("forgotpasswordvalidate");
                    tsUtilities.DelayedFocus("forgot-password-validate");
                } else {
                    this.ShowMessage(tsUtilities.MessagesToString(data.actionResponse.messages), "danger");
                }
            } else {
                this.ShowMessage("An unknown error occurred attempting to reset the password.", "danger");
            }
        };

        this.ShowMessage(this.MainModel().Language("ProcessingWait"), "dark");
        tsUtilities.AjaxData(window.baseURL + "api/Data/ForgotPassword", ko.toJSON(this.User), success);
    }

    HideMessage(): void {
        this.LoginMessage("");
        this.LoginMessageClass("");
    }

    /**
     * Called to perform a local login when the Local Login form is used.
     */
    LocalLogin(): boolean {
        if (!this.MainModel().UseTenantCodeInUrl() && this.MainModel().ShowTenantCodeFieldOnLoginForm()) {
            if (!tsUtilities.HasValue(this.Authenticate().tenantCode())) {
                tsUtilities.DelayedFocus("local-tenantCode");
                return;
            }
        }

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
        let eitSsoUrl: string = "";

        if (this.MainModel().TenantId() == this.MainModel().Guid1()) {
            output.push("local");
            this.LoginUseLocal(true);
        } else {
            if (this.MainModel().Tenant() != null && this.MainModel().Tenant().tenantSettings() != null) {
                eitSsoUrl = this.MainModel().Tenant().tenantSettings().eitSsoUrl();

                for (let x: number = 0; x < this.MainModel().Tenant().tenantSettings().loginOptions().length; x++) {
                    let item: string = this.MainModel().Tenant().tenantSettings().loginOptions()[x];

                    if (item.toLowerCase() == "local") {
                        this.LoginUseLocal(true);
                        output.push("local");
                    } else if (item.toLowerCase() == "eitsso" && tsUtilities.HasValue(eitSsoUrl)) {
                        // Only include this option if the SSO URL is set
                        this.LoginUseEITSSO(true);
                        output.push("eitsso");
                    } else if (item.toLowerCase() == "openid" && window.useAuthProviderOpenId) {
                        this.LoginUseOpenId(true);
                        output.push("openid");
                    } else if (item.toLowerCase() == "facebook" && window.useAuthProviderFacebook) {
                        this.LoginUseFacebook(true);
                        output.push("facebook");
                    } else if (item.toLowerCase() == "google" && window.useAuthProviderGoogle) {
                        this.LoginUseGoogle(true);
                        output.push("google");
                    } else if (item.toLowerCase() == "microsoft" && window.useAuthProviderMicrosoftAccount) {
                        this.LoginUseMicrosoftAccount(true);
                        output.push("microsoft");
                    } else if (item.toLowerCase() == "custom" && window.useAuthProviderCustom) {
                        this.LoginUseCustom(true);
                        output.push("custom");
                    }
                }
            }
        }

        if (output.length == 0) {
            output.push("local");
            this.LoginUseLocal(true);
        }

        if (this.LoginUseLocal()) {
            this.ShowLocalLoginButton(output.length > 1);
        }

        return output;
    });

    /**
     * Handles the click of the various login option buttons on the login page.
     * @param type {string} - The type of login. Currently we have "local" and "eitsso" (using our Okta authentication)
     */
    Login(type: string): void {
        this.MainModel().Message_Hide();
        this.LoginType(type);

        let url: string = "";

        switch (type.toLowerCase()) {
            case "custom":
                url = window.baseURL + "Authorization/Custom/" + this.MainModel().TenantId();
                break;

            case "local":
                this.Validating(false);
                this.Authenticate().username("");
                this.Authenticate().password("");

                let focus: string = "local-username";
                if (!this.MainModel().UseTenantCodeInUrl() && this.MainModel().ShowTenantCodeFieldOnLoginForm()) {
                    if (!tsUtilities.HasValue(this.Authenticate().tenantCode())) {
                        focus = "local-tenantCode";
                    }
                }

                tsUtilities.DelayedFocus(focus);
                break;

            case "eitsso":
                let eitSsoUrl: string = "";
                if (this.MainModel().Tenant() != null && this.MainModel().Tenant().tenantSettings() != null) {
                    eitSsoUrl = this.MainModel().Tenant().tenantSettings().eitSsoUrl();
                }

                if (tsUtilities.HasValue(eitSsoUrl)) {
                    url = eitSsoUrl + "Login/SSO2?Redirect=" + encodeURI(window.baseURL + window.tenantCode);
                } else {
                    this.MainModel().Message_Error("Missing the EIT SSO URL setting.");
                }
                break;

            case "facebook":
                url = window.baseURL + "Authorization/Facebook?TenantId=" + encodeURIComponent(this.MainModel().TenantId());
                break;

            case "google":
                url = window.baseURL + "Authorization/Google?TenantId=" + encodeURIComponent(this.MainModel().TenantId());
                break;

            case "microsoftaccount":
                url = window.baseURL + "Authorization/MicrosoftAccount?TenantId=" + encodeURIComponent(this.MainModel().TenantId());
                break;

            case "openid":
                url = window.baseURL + "Authorization/OpenId?TenantId=" + encodeURIComponent(this.MainModel().TenantId());
                break;
        }

        if (url != "") {
            this.ExternalLogin(url);
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

        let error: string = this.MainModel().QuerystringValue("Error");
        if (tsUtilities.HasValue(error)) {
            this.ShowMessage(error, "danger");
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
     * This is the view that is loaded when the server has been updated and the app needs to reload.
     */
    ServerUpdated(): void {
        let url: string = window.baseURL;
        if (window.useTenantCodeInUrl) {
            url += this.MainModel().TenantCode();
        }

        window.location.href = url;
    }

    ShowMessage(message: string, messageClass: string): void {
        let c: string = "alert-dark";
        if (tsUtilities.HasValue(messageClass)) {
            c = "alert-" + messageClass;
        }

        this.LoginMessage(message);
        this.LoginMessageClass(c);
    }

    ShowPasswordReset = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.LoginUseLocal()) {
            if (this.MainModel().Tenant().tenantSettings().allowUsersToResetPasswordsForLocalLogin()) {
                output = true;
            }
        }

        return output;
    });

    ShowSignup = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.LoginUseLocal()) {
            if (this.MainModel().Tenant().tenantSettings().allowUsersToSignUpForLocalLogin()) {
                output = true;
            }
        }

        return output;
    });

    SignUp(): void {
        this.ConfirmPassword("");
        this.User(new user);
        this.User().tenantId(this.MainModel().TenantId());
        this.User().userId(this.MainModel().GuidEmpty());
        this.View("signup");
        tsUtilities.DelayedFocus("signup-firstName");
    }

    SignUpSave(): void {
        this.HideMessage();
        let errors: string[] = [];
        let focus: string = "";

        if (!tsUtilities.HasValue(this.User().firstName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("FirstName")));
            if (focus == "") { focus = "signup-firstName"; }
        }
        if (!tsUtilities.HasValue(this.User().lastName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("LastName")));
            if (focus == "") { focus = "signup-lastName"; }
        }
        if (!tsUtilities.HasValue(this.User().email())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("EmailAddress")));
            if (focus == "") { focus = "signup-email"; }
        }
        if (!tsUtilities.HasValue(this.User().password())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("Password")));
            if (focus == "") { focus = "signup-password"; }
        }
        if (!tsUtilities.HasValue(this.ConfirmPassword())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("ConfirmPassword")));
            if (focus == "") { focus = "signup-confirmPassword"; }
        }

        if (tsUtilities.HasValue(this.User().password()) && tsUtilities.HasValue(this.ConfirmPassword())){
            if (this.User().password() != this.ConfirmPassword()) {
                errors.push(this.MainModel().Language("NewPasswordAndConfirmDontMatch"));
                this.User().password("");
                this.ConfirmPassword("");
                if (focus == "") { focus = "signup-password"; }
            }
        }

        if (errors.length > 0) {
            this.ShowMessage(tsUtilities.MessagesToString(errors), "danger");
            if (focus != "") {
                tsUtilities.DelayedFocus(focus);
            }
            return;
        }

        let success: Function = (data: server.user) => {
            this.HideMessage();
            if (data != null) {
                if (data.actionResponse.result) {
                    this.User().Load(data);
                    this.View("signupvalidate");
                    tsUtilities.DelayedFocus("signup-validate");
                } else {
                    this.ShowMessage(tsUtilities.MessagesToString(data.actionResponse.messages), "danger");
                }
            } else {
                this.ShowMessage("An unknown error occurred attempting to process the sign up.", "danger");
            }
        };

        this.ShowMessage(this.MainModel().Language("ProcessingWait"), "dark");
        tsUtilities.AjaxData(window.baseURL + "api/Data/UserSignUp", ko.toJSON(this.User), success);
    }

    SignupValidateCode(): void {
        if (!tsUtilities.HasValue(this.User().location())) {
            tsUtilities.DelayedFocus("signup-validate");
            return;
        }

        let success: Function = (data: server.user) => {
            this.HideMessage();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.ShowMessage("Your account has been activated. You may now return to the login page and log in.", "success");
                } else {
                    this.ShowMessage(tsUtilities.MessagesToString(data.actionResponse.messages), "danger");
                }
            } else {
                this.ShowMessage("An unknown error occurred attempting to validate the confirmation code.", "danger");
            }
        };

        this.ShowMessage(this.MainModel().Language("ValidateConfirmationCode"), "dark");
        tsUtilities.AjaxData(window.baseURL + "api/Data/UserSignupConfirm", ko.toJSON(this.User), success);
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        this.ConfirmPassword("");
        this.LoginMessage("");
        this.LoginMessageClass("");
        this.View("");

        switch (this.MainModel().CurrentView()) {
            case "login":
                this.LoginPageLoaded();
                break;

            case "serverupdated":
                setTimeout(() => this.ServerUpdated(), 5000);
                break;
        }
    }
}