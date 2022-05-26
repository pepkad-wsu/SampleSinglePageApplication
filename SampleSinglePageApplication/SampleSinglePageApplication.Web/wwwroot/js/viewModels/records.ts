class RecordsModel {
    AllowDelete: KnockoutObservable<boolean> = ko.observable(false);
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    Record: KnockoutObservable<record> = ko.observable(new record);
    Records: KnockoutObservableArray<record> = ko.observableArray([]);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

     /**
     * Called when the URL view is "EditRecord" to load the record and show the edit record interface.
     */
    EditRecord(): void {
        let recordId: string = this.MainModel().Id();

        this.AllowDelete(false);

        this.Record(new record);

        if (tsUtilities.HasValue(recordId)) {
            this.MainModel().Message_Loading();

            let success: Function = (data: server.record) => {
                this.Loading(false);
                this.MainModel().Message_Hide();
                if (data != null) {
                    if (data.actionResponse.result) {
                        this.Record().Load(data);
                    } else {
                        this.MainModel().Nav("Records");
                    }
                } else {
                    this.MainModel().Nav("Records");
                }
            };

            this.Loading(true);
            tsUtilities.AjaxData(window.baseURL + "api/Data/GetRecord/" + recordId, null, success);
        }
    }

    GetRecords(): void {
        console.log("records page loaded");
        let success: Function = (data: server.record[]) => {
            console.log("data records call: ", data);
            let records: record[] = [];
            if (data != null) {
                data.forEach(function (e) {
                    let item: record = new record();
                    item.Load(e);
                    records.push(item);
                });
            }
            this.Records(records);
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetRecords", null, success);
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "editrecord":
                this.EditRecord();
                break;
            case "records":
                this.GetRecords();
                break;
        }
    }
}