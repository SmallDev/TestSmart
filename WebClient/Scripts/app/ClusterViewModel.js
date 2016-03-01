function ClusterViewModel(initData) {
    this.id = initData.Id;
    this.name = initData.Name;
  
    this.size = (initData.Size).toFixed(2) + "%";

    this.navigateBack = function () {
        window.location = "/cluster";
    };
}