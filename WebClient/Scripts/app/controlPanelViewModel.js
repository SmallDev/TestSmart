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

    String.prototype.stringToSeconds = function () {
        var secFromHours = Number(this.substring(0, 2)) * 1200;
        var secFromMinuts = Number(this.substring(3, 5)) * 60;
        var sec = Number(this.substring(6, 8));
        var totalSec = secFromHours + secFromMinuts + sec;
        return totalSec;
    }

    function setControlData(that, data) {
        that.set("velocity", data.Velocity);
        that.set("allTime", data.AllTime);

        var allTimeInSec = data.AllTime.stringToSeconds();
        var calcTimeInSec = data.CalcTime.stringToSeconds();
        var readTimeInSec = data.ReadTime.stringToSeconds();

        var calcTimeInPercent = calcTimeInSec / allTimeInSec * 100;
        var readTimeInPercent = readTimeInSec / allTimeInSec * 100;

        var gauge = $("#ctr-linear-progress").data("kendoLinearGauge");
        gauge.pointers[0].value(calcTimeInPercent);
        gauge.pointers[1].value(readTimeInPercent);
    }

    function getControlDataWorker(that) {
        jQuery.ajax({
            url: "/Home/GetControlData",
            type: "GET",
            success: function (data) {
                if (!that.updateIsWorking)
                    return;

                setControlData(that, data);
                setTimeout(function () { getControlDataWorker(that); }, 1000);
            },
            cache: false
        });
    };
}
