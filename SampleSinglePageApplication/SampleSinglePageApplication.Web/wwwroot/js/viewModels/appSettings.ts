class AppSettingsModel {
    AppSettings: KnockoutObservable<applicationSettings> = ko.observable(new applicationSettings);
    LastKey: KnockoutObservable<string> = ko.observable("");
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    Module: KnockoutObservable<addModule> = ko.observable(new addModule);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    AddModule(): void {
        this.Module(new addModule);
    }

    AddModuleSave(): void {
        if (!tsUtilities.HasValue(this.Module().module())) {
            tsUtilities.DelayedFocus("edit-appsettings-addModule-module");
            return;
        }
        if (!tsUtilities.HasValue(this.Module().name())) {
            tsUtilities.DelayedFocus("edit-appsettings-addModule-name");
            return;
        }

        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    this.MainModel().Message("Force a Restart in Visual Studio (Ctrl + Shift + F5) to See the Changes", StyleType.Warning, false, true);
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to create the new Module.");
            }
        };

        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/AddModule", ko.toJSON(this.Module), success);
    }

    AllowAddModule(): boolean {
        let output: boolean = false;

        // This feature can only be used in development mode, not in production. So, the button is only shown
        // when running on localhost.
        if (tsUtilities.HasValue(window.baseURL)) {
            if (window.baseURL.toLowerCase().indexOf("https://localhost") > -1) {
                output = true;
            }
        }

        return output;
    }

    Decrypt(item: string): void {
        let value: string = "";

        switch (item) {
            case "mailServerUsername":
                value = this.AppSettings().mailServerUsername();
                break;

            case "mailServerPassword":
                value = this.AppSettings().mailServerPassword();
                break;
        }

        if (!tsUtilities.HasValue(value)) {
            return;
        }

        let success: Function = (data: server.booleanResponse) => {
            if (data != null && data.result) {
                let decrypted: string = data.messages[0];

                switch (item) {
                    case "mailServerUsername":
                        this.AppSettings().mailServerUsername(decrypted);
                        break;

                    case "mailServerPassword":
                        this.AppSettings().mailServerPassword(decrypted);
                        break;
                }
            } else if (data != null && !data.result) {
                this.MainModel().Message_Errors(data.messages);
            }
        };

        let post: simplePost = new simplePost();
        post.singleItem(value);

        tsUtilities.AjaxData(window.baseURL + "api/Data/Decrypt", ko.toJSON(post), success);
    }

    Encrypt(item: string): void {
        let value: string = "";

        switch (item) {
            case "mailServerUsername":
                value = this.AppSettings().mailServerUsername();
                break;

            case "mailServerPassword":
                value = this.AppSettings().mailServerPassword();
                break;
        }

        if (!tsUtilities.HasValue(value)) {
            return;
        }

        let success: Function = (data: server.booleanResponse) => {
            if (data != null && data.result) {
                let encrypted: string = data.messages[0];

                switch (item) {
                    case "mailServerUsername":
                        this.AppSettings().mailServerUsername(encrypted);
                        break;

                    case "mailServerPassword":
                        this.AppSettings().mailServerPassword(encrypted);
                        break;
                }
            }
        };

        let post: simplePost = new simplePost();
        post.singleItem(value);

        tsUtilities.AjaxData(window.baseURL + "api/Data/Encrypt", ko.toJSON(post), success);
    }

    GetNewEncryptionKey(): void {
        let currentKey: string = this.AppSettings().encryptionKey();
        if (tsUtilities.HasValue(currentKey) && currentKey.indexOf(",0x") > -1) {
            this.LastKey(currentKey);
        }

        let success: Function = (data: server.booleanResponse) => {
            if (data != null) {
                if (data.result) {
                    let key: string = data.messages[0];
                    this.AppSettings().encryptionKey(key);
                    this.LastKey(key);
                } else {
                    this.AppSettings().encryptionKey(this.LastKey());
                }
            } else {
                this.AppSettings().encryptionKey(this.LastKey());
            }
        };

        this.AppSettings().encryptionKey("Getting New Key from Server");
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetNewEncryptionKey", null, success);
    }

    Load(): void {
        if (tsUtilities.HasValue(this.MainModel().Id()) && this.MainModel().Id().toLowerCase() == "addmodule") {
            if (this.AllowAddModule()) {
                this.AddModule();
                return;
            } else {
                this.MainModel().Nav("AppSettings");
            }
        }

        this.MainModel().Message_Hide();

        let success: Function = (data: server.applicationSettings) => {
            if (data != null) {
                if (data.actionResponse.result) {
                    this.AppSettings().Load(data);
                    this.Loading(false);
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load the application settings.");
            }
        };

        this.Loading(true);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetApplicationSettings", null, success);
    }

    Save(): void {
        let success: Function = (data: server.applicationSettings) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.AppSettings().Load(data);

                    if (this.AppSettings().useTenantCodeInUrl() != window.useTenantCodeInUrl) {
                        //window.location.href = window.baseURL;
                    } else {
                        this.MainModel().Message("Saved at " + tsUtilities.FormatDateTimeLong(new Date), StyleType.Success, false, true);
                    }
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save the Application Settings.");
            }
        };

        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveApplicationSettings", ko.toJSON(this.AppSettings), success);
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        this.Loading(false);

        let allowed: boolean = this.MainModel().User().appAdmin();

        switch (this.MainModel().CurrentView()) {
            case "appsettings":
                if (allowed) {
                    this.Load();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
        }
    }
}