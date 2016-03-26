define("kMeansClustersChartViewModel", function () {
    var vm = function KMeansClustersChartViewModel() {
        var thisself = this;
        
        this.showClustersChart = false;
        this.showNoClustersMessage = false;
        this.sets = [4, 5, 6, 7, 8];
        this.currentSet = 4;
        this.clusterDataDawnloaded = false;


        this.onSetChange = function () {
            getClusters(this);
        };

        this.clusters = new kendo.data.DataSource({
            data: []
        });

        this.onClusterClick = function (e) {
            var _tempThat = this;
            explodePie(e);
            setTimeout(function () { window.location = "/kmeans/" + _tempThat.get("currentSet") + "/" + e.category; }, 200);
        }


        function explodePie(e) {
            $.each(e.sender.dataSource.view(), function () {
                self.explode = false;
            });

            e.sender.options.transitions = false;
            e.dataItem.explode = true;
            e.sender.refresh();
        };

        this.initData = function () {
            getClusters(this);
        };


        function getClusters(that) {
            jQuery.ajax({
                url: "/KMeansCluster/GetClusters",
                type: "POST",
                data: { option: that.get("currentSet") },
                success: function(data) {
                    that.set("showClustersChart", data.ShowChart);
                    that.set("showNoClustersMessage", !data.ShowChart);
                    that.set("clusterDataDawnloaded", true);
                    if (data.ShowChart) {
                        that.clusters.data(data.PieClusters);
                    }
                },
                error: function() {
                },
                cache: false
            });
        }
    }

    return vm;
});