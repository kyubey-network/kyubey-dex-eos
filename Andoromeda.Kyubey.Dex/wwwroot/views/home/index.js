component.data = function () {
    return {
        news: [],
        slides: [],
        tokenTable: []
    };
};

component.created = function () {
    var self = this;
    qv.createView(`/api/v1/lang/${app.lang}/news`, {}, 60000)
        .fetch(x => {
            self.news = x.data;
        });
    qv.createView(`/api/v1/lang/${app.lang}/slides`, {}, 60000)
        .fetch(x => {
            self.slides = x.data;
        });
    qv.get(`/api/v1/lang/${app.lang}/token`, {}).then(res => {
        if (res.code - 0 === 200) {
            self.tokenTable = res.data;
        }
    })
};

component.methods = {

};