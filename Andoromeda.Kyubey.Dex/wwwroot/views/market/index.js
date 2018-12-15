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
        },
        mobile: {
            mode: 'normal',
            search: ''
        }
    };
};

component.methods = {
    searchToken() {
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
    },
    focusMobileSearch() {
        setTimeout(function () { $('#mobileTokenSearch').focus(); }, 50)
    }
}

component.computed = {
    isSignedIn: function () {
        return !!app.account;
    }
};

component.created = function () {
    app.mobile.nav = 'market';
    qv.get(`/api/v1/lang/${app.lang}/token`, {}).then(res => {
        if (res.code - 0 === 200) {
            this.tokenTable = res.data;
            this.tokenTable.forEach(x => {
                x.current_price = x.current_price.toFixed(8);
                x.max_price_recent_day = x.max_price_recent_day.toFixed(8);
                x.min_price_recent_day = x.min_price_recent_day.toFixed(8);
                var symbol = '';
                x.change_recent_day *= 100;
                if (x.change_recent_day > 0) {
                    x.up = true;
                    x.down = false;
                    symbol = '+';
                } else if (x.change_recent_day < 0) {
                    x.up = false;
                    x.down = true;
                }
                x.change_recent_day = symbol + x.change_recent_day.toFixed(2) + '%';
            });
            this.tokenTableSource = this.tokenTable;
        }
    })

};