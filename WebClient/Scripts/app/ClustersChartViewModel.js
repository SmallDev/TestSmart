function ClustersChartViewModel(data) {
    var clusterData = data;

    this.clusters = new kendo.data.DataSource({
        data: clusterData
    });

    this.onClusterClick = function(e){
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
}