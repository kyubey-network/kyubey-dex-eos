component.data = function () {
    return {
        news: [],
        slides: []
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
};

component.methods = {

};