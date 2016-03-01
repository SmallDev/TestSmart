function ControlPanelViewModel() {
    var self = this;
    this.selectedNumber = 0;
    this.allTime = "00:00:00";
    this.velocity = 0;
    this.statusText = "Start";

    this.updateIsWorking = false;
    this.stopWorkInProgress = false;

    this.toogleUpdateProgress = function () {
        if (this.updateIsWorking) {
            this.set("stopWorkInProgress", true);
            this.set("updateIsWorking", false);
            stopWork(this);
        }
        else {
            startWork(this);
        }
    };

    this.resumeWork = function () {
        startWork(this);
    };

    this.controlPanelLoaded = true;

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

        var calcTimeInPercent = allTimeInSec >= calcTimeInSec ? (calcTimeInSec / allTimeInSec * 100) : 100;
        var readTimeInPercent = allTimeInSec >= readTimeInSec ? (readTimeInSec / allTimeInSec * 100) : 100;

        var gauge = $("#ctr-linear-progress").data("kendoLinearGauge");
        gauge.pointers[0].value(calcTimeInPercent);
        gauge.pointers[1].value(readTimeInPercent);
    }

    function startControlDataWorker(that) {
        jQuery.ajax({
            url: "/Home/GetControlData",
            type: "GET",
            success: function (data) {
                if (!that.updateIsWorking)
                    return;

                setControlData(that, data);
                setTimeout(function () { startControlDataWorker(that); }, 1000);
            },
            cache: false
        });
    };

    function startWork(that) {
        jQuery.ajax({
            url: "/Home/Start",
            type: "GET",
            success: function () {
                that.set("updateIsWorking", true);
                that.set("statusText", "Stop");
                Cookies.set('smartSpyWorking', true , { expires: 7 });
                startControlDataWorker(that)
            },
            cache: false
        });
    }

    function stopWork(that) {
        jQuery.ajax({
            url: "/Home/Stop",
            type: "GET",
            success: function (data) {
                that.set("updateIsWorking", false);
                that.set("statusText", "Start");
                that.set("stopWorkInProgress", false);
                Cookies.remove('smartSpyWorking');
            },
            cache: false
        });
    }
}
