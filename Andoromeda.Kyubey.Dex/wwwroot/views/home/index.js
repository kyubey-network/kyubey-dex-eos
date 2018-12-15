component.data = function () {
    return {
        news: [],
        slides: [],
        tokenTable: [],
        tokenTableSource: [],
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

component.created = function () {
    var self = this;
    app.mobile.nav = 'home';
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
            self.tokenTable.forEach(x => {
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
            self.tokenTableSource = self.tokenTable;
        }
    })
};

component.methods = {
    searchToken: function () {
        if (this.searchText !== '') {
            this.tokenTable = this.tokenTableSource.filter(item => {
                return item.symbol.toUpperCase().includes(this.searchText.toUpperCase())
            })
        } else {
            this.tokenTable = this.tokenTableSource;
        }
    },
    formatTime(time) {
        return moment(time).format('MM-DD');
    },
    sortTokenOnClick(row) {
        this.sortControl.desc = (this.sortControl.desc + 1) % 3;
        this.sortToken(row, this.sortControl.desc);
    },
    sortToken(row,desc) {
        this.sortControl.row = row;
        this.sortControl.desc = desc;
        if (this.sortControl.desc === 2){
            this.tokenTable.sort((a, b)=>{
                return parseFloat(b[row])-parseFloat(a[row])
            })
        }else if (this.sortControl.desc === 1) {
            this.tokenTable.sort((a, b)=>{
                return parseFloat(a[row])-parseFloat(b[row])
            })
        } else {
            this.tokenTable = this.tokenTableSource;
        }
    }
};

component.computed = {
    isSignedIn: function () {
        return !(app.account == null || app.account.name == null);
    }
};