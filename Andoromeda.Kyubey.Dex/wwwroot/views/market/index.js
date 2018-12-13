component.data = function () {
    return {
        tokenTableSource: [],
        tokenTable: [],
        searchText: '',
        control: {
            tab: 'eos'
        },
        sortControl: {
            desc: null,
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
    sortToken(row, desc) {
        this.sortControl.row = row;
        this.sortControl.desc = desc;
        if (this.sortControl.desc === true){
            this.tokenTable.sort((a, b)=>{
                return parseFloat(b[row])-parseFloat(a[row])
            })
        }
        if (this.sortControl.desc === false) {
            this.tokenTable.sort((a, b)=>{
                return parseFloat(a[row])-parseFloat(b[row])
            })
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