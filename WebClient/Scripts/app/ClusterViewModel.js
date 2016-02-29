function ClusterViewModel(initData) {
    this.id = initData.Id;
    this.name = initData.Name;
  
    this.size = kendo.toString(initData.Size, "00") + "%";

    this.navigateBack = function () {
        window.location = "/";
    };
}