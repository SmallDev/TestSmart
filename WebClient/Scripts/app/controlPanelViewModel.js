define("controlPanelViewModel", function () {
    var vm = function ControlPanelViewModel() {
        var self = this;
        this.selectedNumber = 0;
        this.allTime = "00:00:00";
        this.readVelocity = 0;
        this.learnVelocity = 0;
        this.readVelocityControl = 0;
        this.learnVelocityControl = 0;
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

        this.onReadVelocityChange = function () {
            setReadVelocity(this.readVelocityControl);
        };

        this.onLearnVelocityChange = function () {
            setLearnVelocity(this.learnVelocityControl);
        };

        this.saveAllTime = function () {
            setAllTime(this.allTime);
        };

        String.prototype.stringToSeconds = function () {
            var secFromHours = Number(this.substring(0, 2)) * 1200;
            var secFromMinuts = Number(this.substring(3, 5)) * 60;
            var sec = Number(this.substring(6, 8));
            var totalSec = secFromHours + secFromMinuts + sec;
            return totalSec;
        }

        function setControlData(that, data) {
            that.set("readVelocity", data.ReadVelocity);
            that.set("learnVelocity", data.LearnVelocity);
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
                    Cookies.set('smartSpyWorking', true, { expires: 7 });
                    startControlDataWorker(that);
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

        function setReadVelocity(velocityControl) {
            jQuery.ajax({
                url: "/Home/SetReadVelocity",
                type: "POST",
                data: { velocity: velocityControl },
                cache: false
            });
        }

        function setLearnVelocity(velocityControl) {
            jQuery.ajax({
                url: "/Home/SetLearningVelocity",
                type: "POST",
                data: { velocity: velocityControl },
                cache: false
            });
        }

        function setAllTime(allTime) {
            jQuery.ajax({
                url: "/Home/SetAllTime",
                type: "POST",
                data: { allTime: allTime },
                cache: false
            });
        }
    }

    return vm;
});