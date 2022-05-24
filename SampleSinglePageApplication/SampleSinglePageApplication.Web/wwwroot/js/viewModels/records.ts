class RecordsModel {
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    Record: KnockoutObservable<record> = ko.observable(new record);
    Records: KnockoutObservableArray<record> = ko.observableArray([]);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
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
            case "records":
                this.GetRecords();
                break;
        }
    }
}