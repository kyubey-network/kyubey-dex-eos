component.data = function () {
    return {
        tokens: [],
        searchText: ''
    };
};

component.computed = {
    totalEvaluated: function () {
        if (!this.tokens.length) {
            return 0;
        }
        var values = this.tokens.map(x => x.evaluated);
        return values.reduce(function (prev, curr) {
            return prev + curr;
        });
    }
};

component.created = function () {
    if (!app.isSignedIn) {
        app.redirect('/');
    }
};