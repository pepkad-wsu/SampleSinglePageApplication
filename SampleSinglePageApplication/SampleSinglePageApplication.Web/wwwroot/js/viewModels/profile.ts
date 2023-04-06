class ProfileModel {
    ConfirmPassword: KnockoutObservable<string> = ko.observable("");
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    PasswordReset: KnockoutObservable<userPasswordReset> = ko.observable(new userPasswordReset);
    ProfileMessage: KnockoutObservable<string> = ko.observable("");
    UserProfile: KnockoutObservable<user> = ko.observable(new user);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });

        setTimeout("setupProfileDropZone()", 0);
    }

    /**
     * Called with the URL view is "ChangePassword" to show the change password form. However, this is only accessible
     * for users that have a local password. The flag is set on their User object when it loads. If the user gets to this
     * URL but doesn't have a local password they are redirected back to the Home page.
     */
    ChangePassword(): void {
        if (!this.MainModel().User().hasLocalPassword()) {
            this.MainModel().Nav("");
        }

        this.PasswordReset(new userPasswordReset);
        this.PasswordReset().userId(this.MainModel().User().userId());
        this.PasswordReset().tenantId(this.MainModel().TenantId());
        this.ConfirmPassword("");

        tsUtilities.DelayedFocus("change-password-currentPassword");
    }

    /**
     * Validates that the user has entered their old password, the new and confirm passwords, and that those new passwords match.
     * Then, the Web API endpoint is called to perform the actual password reset if the data looks good.
     */
    ChangePasswordValidate(): void {
        this.MainModel().Message_Hide();
        let errors: string[] = [];
        let focus: string = "";

        if (!tsUtilities.HasValue(this.PasswordReset().currentPassword())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("CurrentPassword")));
            if (focus == "") { focus = "change-password-currentPassword"; }
        }
        if (!tsUtilities.HasValue(this.PasswordReset().newPassword())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("NewPassword")));
            if (focus == "") { focus = "change-password-newPassword"; }
        }
        if (!tsUtilities.HasValue(this.ConfirmPassword())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("ConfirmPassword")));
            if (focus == "") { focus = "change-password-confirmPassword"; }
        }
        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
        }

        if (this.PasswordReset().newPassword() != this.ConfirmPassword()) {
            this.PasswordReset().newPassword("");
            this.ConfirmPassword("");
            this.MainModel().Message_Error(this.MainModel().Language("NewPasswordAndConfirmDontMatch"));
            tsUtilities.DelayedFocus("change-password-newPassword");
            return;
        }

        let success: Function = (data: server.booleanResponse) => {
            if (data != null) {
                if (data.result) {
                    this.MainModel().Nav("PasswordChanged");
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to reset the password.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/ResetUserPassword", ko.toJSON(this.PasswordReset), success);
    }

    /**
     * Deletes the current photo for the user.
     */
    DeleteAvatarPhoto(): void {
        this.MainModel().Message_Hide();

        let success: Function = (data: server.booleanResponse) => {
            if (data != null) {
                if (data.result) {
                    this.MainModel().ReloadUser();
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUserPhoto/" + this.MainModel().User().userId(), null, success);
    }

    /**
     * Called when the URL view is "Profile" to show the user profile page. Resets the Dropzone file upload control.
     */
    EditProfile(): void {
        setTimeout("resetProfileDropZone()", 0);

        this.UserProfile().Load(JSON.parse(ko.toJSON(this.MainModel().User)));
    }

    /**
     * Saves the changes to the user profile.
     */
    SaveProfileInfo(): void {
        this.MainModel().Message_Hide();
        let success: Function = (data: server.user) => {
            if (data != null) {
                if (data.actionResponse.result) {
                    this.ProfileMessage(this.MainModel().Language("SavedAt") + " " + tsUtilities.FormatTime(new Date()));
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save your profile information.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUser", ko.toJSON(this.UserProfile), success);
    }

    /**
     * Event fires when a photo upload has completed.
     * @param message {server.booleanResponse} - A JSON object returned from the WebAPI endpoint.
     */
    UploadComplete(message: any): void {
        this.MainModel().Message_Hide();

        let response: booleanResponse = new booleanResponse();
        response.Load(JSON.parse(message));

        if (response.result()) {
            this.MainModel().ReloadUser();
        } else {
            this.MainModel().Message_Errors(response.messages());
        }
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged(): void {
        switch (this.MainModel().CurrentView()) {
            case "changepassword":
                if (this.MainModel().User().preventPasswordChange()) {
                    this.MainModel().Nav("AccessDenied");
                } else {
                    this.ChangePassword();
                }
                break;

            case "profile":
                this.EditProfile();
                break;
        }
    }
}