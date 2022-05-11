class UdfLabelsModel {
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    ManageUdfLabelsShowHelp: KnockoutObservable<boolean> = ko.observable(false);
    UDFLabels: KnockoutObservableArray<udfLabel> = ko.observableArray([]);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });

        let labels: udfLabel[] = [];
        if (this.MainModel().Tenant().udfLabels() != null) {
            this.MainModel().Tenant().udfLabels().forEach(function (e) {
                let item: udfLabel = new udfLabel();
                item.Load(JSON.parse(ko.toJSON(e)));
                labels.push(item);
            });
        }
        this.UDFLabels(labels);
    }

    GetUdfLabels() {
        let success: Function = (data: server.udfLabel[]) => {
            let labels: udfLabel[] = [];
            if (data != null) {
                data.forEach(function (e) {
                    let item: udfLabel = new udfLabel();
                    item.Load(e);
                    labels.push(item);
                });
            }
            this.UDFLabels(labels);
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUDFLabels/" + this.MainModel().TenantId(), null, success);
    }

    ManageUDFLabelsToggleHelp(): void {
        this.ManageUdfLabelsShowHelp(!this.ManageUdfLabelsShowHelp());
    }

    Save() {
        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    this.MainModel().Message(this.MainModel().Language("Saved"), StyleType.Success, true, true);
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save UDF Labels");
            }
        };

        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUDFLabels/" + this.MainModel().TenantId(), ko.toJSON(this.UDFLabels), success);
    }

    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "udflabels":
                this.GetUdfLabels();
                break;
        }
    }
}