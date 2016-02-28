function CustomClocks(timeObjects) {
    var self = this;
    this.hours = timeObjects.hours;
    this.minutes = timeObjects.minutes;
    this.sec = timeObjects.sec;

    this.setTime = function (time) {
        var seconds = time.getSeconds();
        self.sec.html((seconds < 10 ? "0" : "") + seconds);

        var minutes = time.getMinutes();
        self.minutes.html((minutes < 10 ? "0" : "") + minutes);

        var hours = time.getHours();
        self.hours.html((hours < 10 ? "0" : "") + hours);
    }
}