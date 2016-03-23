define("clustersInitViewModel", function () {
    var vm = function ClustersInitViewModel() {

        var self = this;
        this.clustersNumber = 0;
        this.clearData = false;
        this.showErrorMessage = false;


        this.saveData = function () {
            jQuery.ajax({
                url: "/Home/InitClusters",
                type: "post",
                data: getData(this.ClustersInitVM),

                success: function () {
                    $('#init-clusters-modal').modal('hide');
                    this.clustersNumber = 0;
                    this.clearData = false;
                },
                cache: false
            });
        }

        this.cancel = function () {
            $('#init-clusters-modal').modal('hide');
            this.ClustersInitVM.clustersNumber = 0;
            this.ClustersInitVM.clearData = false;
        }

        function getData(vm) {
            return {
                ClustersNumber: vm.clustersNumber,
                ClearData: vm.clearData
            }
        }
    };

    return vm;
});