component.data = function () {
    return {
        tokenTableSource: [],
        tokenTable: [],
        searchText: '',
        control: {
            tab: 'eos'
        },
        sortControl: {
            desc: 0,
            row: null
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
    sortTokenOnClick(row) {
        this.sortControl.desc = (this.sortControl.desc + 1) % 3;
        this.sortToken(row, this.sortControl.desc);
    },
    sortToken(row, desc) {
        this.sortControl.row = row;
        this.sortControl.desc = desc;
        if (this.sortControl.desc === 2) {
            this.tokenTable.sort((a, b) => {
                return parseFloat(b[row]) - parseFloat(a[row])
            })
        } else if (this.sortControl.desc === 1) {
            this.tokenTable.sort((a, b) => {
                return parseFloat(a[row]) - parseFloat(b[row])
            })
        } else {
            this.tokenTable = this.tokenTableSource;
        }
    }
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