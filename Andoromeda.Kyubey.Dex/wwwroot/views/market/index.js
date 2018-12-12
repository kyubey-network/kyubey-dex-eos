component.data = function () {
    return {
        tokenTableSource: [],
        tokenTable: [],
        searchText: '',
        control: {
            tab: 'eos'
        }
    };
};

component.methods = {
    searchToken () {
        if (this.searchText !== '') {
            this.tokenTable = this.tokenTableSource.filter(item => {
                return item.symbol.toUpperCase().includes(this.searchText.toUpperCase())
            })
        } else {
            this.tokenTable = this.tokenTableSource;
        }
    },
} 

component.computed = {
    isSignedIn: function () {
        return !!app.account;
    }
};

component.created = function () {
    qv.get(`/api/v1/lang/${app.lang}/token`, {}).then(res => {
        if (res.code - 0 === 200) {
            this.tokenTableSource = res.data;
            this.tokenTable = res.data;
        }
    })
};