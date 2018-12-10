component.data = function () {
    return {
        news: [],
        page: -1,
        noMore: false
    };
};

component.created = function () {
    this.loadMore();
};

component.methods = {
    loadMore: function () {
        var self = this;
        if (self.noMore) {
            return;
        }
        ++self.page;
        qv.get(`/api/v1/lang/${app.lang}/news`, { skip: 10 * self.page, take: 10 })
            .then(x => {
                if (!x.data.length || x.data.length < 10) {
                    self.noMore = true;
                }

                for (var i = 0; i < x.data.length; i++) {
                    var item = x.data[i];
                    item.content = '';
                    self.news.push(item);
                    var id = x.data[i].id;
                    qv.get(`/api/v1/lang/${app.lang}/news/${id}`)
                        .then(y => {
                            self.news.filter(z => z.id === id)[0].content = app.marked(y.data.content);
                        });
                }

                var time = new Date(item.time);
                item.time = time.getFullYear()
                    + '-'
                    + (time.getMonth() + 1)
                    + '-'
                    + time.getDate()
                    + ' '
                    + time.getHours()
                    + ':'
                    + time.getMinutes()
                    + ':'
                    + time.getSeconds();
            });
    }
};