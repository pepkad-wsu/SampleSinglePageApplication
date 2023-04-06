var ProfileModel = /** @class */ (function () {
    function ProfileModel() {
        var _this = this;
        this.ConfirmPassword = ko.observable("");
        this.MainModel = ko.observable(window.mainModel);
        this.PasswordReset = ko.observable(new userPasswordReset);
        this.ProfileMessage = ko.observable("");
        this.UserProfile = ko.observable(new user);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
        setTimeout("setupProfileDropZone()", 0);
    }
    /**
     * Called with the URL view is "ChangePassword" to show the change password form. However, this is only accessible
     * for users that have a local password. The flag is set on their User object when it loads. If the user gets to this
     * URL but doesn't have a local password they are redirected back to the Home page.
     */
    ProfileModel.prototype.ChangePassword = function () {
        if (!this.MainModel().User().hasLocalPassword()) {
            this.MainModel().Nav("");
        }
        this.PasswordReset(new userPasswordReset);
        this.PasswordReset().userId(this.MainModel().User().userId());
        this.PasswordReset().tenantId(this.MainModel().TenantId());
        this.ConfirmPassword("");
        tsUtilities.DelayedFocus("change-password-currentPassword");
    };
    /**
     * Validates that the user has entered their old password, the new and confirm passwords, and that those new passwords match.
     * Then, the Web API endpoint is called to perform the actual password reset if the data looks good.
     */
    ProfileModel.prototype.ChangePasswordValidate = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        var errors = [];
        var focus = "";
        if (!tsUtilities.HasValue(this.PasswordReset().currentPassword())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("CurrentPassword")));
            if (focus == "") {
                focus = "change-password-currentPassword";
            }
        }
        if (!tsUtilities.HasValue(this.PasswordReset().newPassword())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("NewPassword")));
            if (focus == "") {
                focus = "change-password-newPassword";
            }
        }
        if (!tsUtilities.HasValue(this.ConfirmPassword())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("ConfirmPassword")));
            if (focus == "") {
                focus = "change-password-confirmPassword";
            }
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
        var success = function (data) {
            if (data != null) {
                if (data.result) {
                    _this.MainModel().Nav("PasswordChanged");
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to reset the password.");
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/ResetUserPassword", ko.toJSON(this.PasswordReset), success);
    };
    /**
     * Deletes the current photo for the user.
     */
    ProfileModel.prototype.DeleteAvatarPhoto = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        var success = function (data) {
            if (data != null) {
                if (data.result) {
                    _this.MainModel().ReloadUser();
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred.");
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUserPhoto/" + this.MainModel().User().userId(), null, success);
    };
    /**
     * Called when the URL view is "Profile" to show the user profile page. Resets the Dropzone file upload control.
     */
    ProfileModel.prototype.EditProfile = function () {
        setTimeout("resetProfileDropZone()", 0);
        this.UserProfile().Load(JSON.parse(ko.toJSON(this.MainModel().User)));
    };
    /**
     * Saves the changes to the user profile.
     */
    ProfileModel.prototype.SaveProfileInfo = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        var success = function (data) {
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.ProfileMessage(_this.MainModel().Language("SavedAt") + " " + tsUtilities.FormatTime(new Date()));
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to save your profile information.");
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUser", ko.toJSON(this.UserProfile), success);
    };
    /**
     * Event fires when a photo upload has completed.
     * @param message {server.booleanResponse} - A JSON object returned from the WebAPI endpoint.
     */
    ProfileModel.prototype.UploadComplete = function (message) {
        this.MainModel().Message_Hide();
        var response = new booleanResponse();
        response.Load(JSON.parse(message));
        if (response.result()) {
            this.MainModel().ReloadUser();
        }
        else {
            this.MainModel().Message_Errors(response.messages());
        }
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ProfileModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "changepassword":
                if (this.MainModel().User().preventPasswordChange()) {
                    this.MainModel().Nav("AccessDenied");
                }
                else {
                    this.ChangePassword();
                }
                break;
            case "profile":
                this.EditProfile();
                break;
        }
    };
    return ProfileModel;
}());
//# sourceMappingURL=profile.js.map