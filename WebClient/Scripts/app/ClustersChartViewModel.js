define("clustersChartViewModel", function () {
    var vm = function ClustersChartViewModel() {
        var self = this;
        self.clusterData = [];
        self.showClustersChart = false;
        self.showNoClustersMessage = false;
        self.clusterDataDawnloaded = false;
        

        self.clusters = new kendo.data.DataSource({
            data: self.clusterData
        });

        self.onClusterClick = function (e) {
            explodePie(e);
            setTimeout(function () { window.location = "/cluster/" + e.category; }, 200);
        }


        function explodePie(e) {
            $.each(e.sender.dataSource.view(), function () {
                this.explode = false;
            });

            e.sender.options.transitions = false;
            e.dataItem.explode = true;
            e.sender.refresh();
        };

        self.startDataWorker = function() {
            getDataWork(this);
        };

        function getDataWork(that) {
            jQuery.ajax({
                url: "/Cluster/GetClusters",
                type: "POST",
                success: function(data) {
                    that.set("showClustersChart", data.ShowChart);
                    that.set("showNoClustersMessage", !data.ShowChart);
                    that.set("clusterDataDawnloaded", true);
                    if (data.ShowChart) {
                        self.clusters.data(data.PieClusters);
                    }
                    //setTimeout(function () { getDataWork(that); }, 5000);
                },
                error: function() {
                    //setTimeout(function () { getDataWork(that); }, 5000);
                },
                cache: false
            });
        }
    }

    return vm;
});