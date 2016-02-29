function ClustersChartViewModel() {
    this.clusters = new kendo.data.DataSource({
        transport: {
            read: function (options) {
                jQuery.ajax({
                    url: "/Cluster/GetJsonList",
                    type: "GET",
                    success: function (data) {
                        options.success(data);
                    },
                    async: false,
                    cache: false
                });
            }
        }
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