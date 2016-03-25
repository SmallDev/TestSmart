define("clusterViewModel", function () {
    var vm = function ClusterViewModel(initData) {
        this.name = initData.Name;

        this.users = initData.Users;
        this.properties = initData.Properties;
    };

    return vm;
});