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

        this.MainModel().SignalRUpdate.subscribe(() => {
            this.SignalrUpdate();
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

    SaveRecord(newRecord: boolean): void {
        this.MainModel().Message_Hide();

        let errors: string[] = [];
        let focus: string = "";

        let labelPrefix: string = newRecord ? "new-" : "edit-";

        if (!tsUtilities.HasValue(this.Record().name())) {
            errors.push("Name is Required");
            if (focus == "") { focus = labelPrefix + "record-name"; }
        }

        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        } else {
            this.MainModel().Message_Saving();

            let success: Function = (data: server.record) => {
                this.MainModel().Message_Hide();
                if (data != null) {
                    if (data.actionResponse.result) {
                        this.MainModel().Nav("Records");
                    } else {
                        this.MainModel().Message_Errors(data.actionResponse.messages);
                    }
                } else {
                    this.MainModel().Message_Error("An unknown error occurred attempting to save this record.");
                }
            };

            let newData: server.record = {
                actionResponse: {
                    messages: this.Record().actionResponse().messages(),
                    result: this.Record().actionResponse().result()
                },
                recordId: this.Record().recordId(),
                name: this.Record().name(),
                number: this.Record().number(),
                boolean: this.Record().boolean(),
                text: this.Record().text(),
                tenantId: this.Record().tenantId(),
                userId: this.Record().userId()
            };

            //tsUtilities.AjaxData(window.baseURL + "api/Data/SaveRecord", ko.toJSON(this.Record), success);
            tsUtilities.AjaxData(window.baseURL + "api/Data/SaveRecord", newData, success);
        }
    }

    /**
     * This model subscribes to the SignalR updates from the MainModel to update record data when records are changed.
     */
    SignalrUpdate(): void {
        //console.log("In Records, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
        switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
            case "setting":
                switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
                    case "recordsaved":
                        // Update the item in the Records list.
                        let recordId: string = this.MainModel().SignalRUpdate().recordId();
                        let t: record = ko.utils.arrayFirst(this.Records(), function (item) {
                            return item.recordId() == recordId;
                        });
                        if (t != null) {
                            t.Load(JSON.parse(this.MainModel().SignalRUpdate().object()));
                        } else {
                            let newRecord: record = new record();
                            newRecord.Load(JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate().object)));
                            this.Records.push(newRecord);
                            this.Records().sort(function (l, r) {
                                return l.name() > r.name() ? 1 : -1;
                            });
                        }
                        break;
                }

                break;
        }
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