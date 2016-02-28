function ControlPanelViewModel(clocks) {
    var self = this;
    this.selectedNumber = 0;
    this.allTime = "00:00:00";
    this.velocity = 0;
    this.statusText = "Start";

    this.updateIsWorking = false;

    this.toogleUpdateProgress = function () {
        if (this.updateIsWorking) {
            this.set("updateIsWorking", false);
            this.set("statusText", "Start");
        }
        else {
            this.set("updateIsWorking", true);
            this.set("statusText", "Stop");
            getControlDataWorker(this)
        }
    };

    function setControlData(that, data) {
        that.set("velocity", data.Velocity);
        that.set("allTime", data.AllTime);

        var gauge = $("#ctr-linear-progress").data("kendoLinearGauge");
        gauge.pointers[0].value(Number(data.CalcTime.substring(3, 5)));
        gauge.pointers[1].value(Number(data.ReadTime.substring(3, 5)));
    }

    function getControlDataWorker(that) {
        jQuery.ajax({
            url: "/Home/GetControlData",
            type: "GET",
            success: function (data) {
                if (!that.updateIsWorking)
                    return;

                setControlData(that, data.Result);
                setTimeout(function () { getControlDataWorker(that); }, 1000);
            },
            cache: false
        });
    };
}
