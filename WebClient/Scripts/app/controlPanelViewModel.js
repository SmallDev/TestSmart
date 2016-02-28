function ControlPanelViewModel(clocks) {
    var self = this;
    this.selectedNumber = 0;
    this.isVisible = true;
    this.clocks = clocks;

    setInterval(function () {
        var nowDate = new Date()
        self.clocks.setTime(nowDate);
    }, 1000);
}
