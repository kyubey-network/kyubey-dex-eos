component.data = function () {
    return { 
        tokens: [],
        listWithFiter :[],
        searchText: ''
    };
};

component.methods = {
    searchToken: function () {
        if (this.searchText !== '') {
            this.listWithFiter = this.tokens.filter(item => {
                return item.symbol.toUpperCase().includes(this.searchText.toUpperCase())
            })
        } else {
            this.listWithFiter = this.tokens;
        }
    },

    goToEosExchange: function (symbol) {
        if (symbol == 'EOS') {
            return;
        }
        app.redirect('/exchange/:id', '/exchange/' + symbol, { id: symbol }, {})
    }
} 

component.computed = {
    account: function () {
        return app.account;
    },

    totalEvaluated: function () {
        if (!this.tokens.length) {
            return 0;
        }
        var values = this.tokens.map(x => x.valid);
        return values.reduce(function (prev, curr) {
            return prev + curr;
        });
    }
};

component.created = function () {
    var self = this
    if (!app.isSignedIn) {
        app.redirect('/');
    }
    qv.get(`/api/v1/lang/${app.lang}/User/${app.account.name}/wallet`, {}).then(res => {
        if (res.code - 0 === 200) {
            self.tokens = res.data;
            self.listWithFiter = res.data;
        }
    })
};