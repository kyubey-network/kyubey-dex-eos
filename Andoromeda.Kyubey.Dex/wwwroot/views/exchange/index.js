component.data = function () {
    return {
        sellOrders: [],
        buyOrders: [],
        control: {
            order: 'mixed', // 订单类型控制
            markets: 'eos', // 自选和eos
            trade: 'limit' // 限价交易和市价交易
        },
        inputs: {
            pair: null,
            buyPrice: 0,
            sellPrice: 0,
            buyAmount: 0,
            sellAmount: 0,
            buyTotal: 0,
            sellTotal: 0,
            vaildbuyPriceInput: 0,
            vaildbuyAmountInput: 0,
            vaildbuyTotalInput: 0,
            vaildsellPriceInput: 0,
            vaildsellAmountInput: 0,
            vaildsellTotalInput: 0
        },
        tokenId: '',
        account: '',
        baseInfo: {},
        showIntroduction: false,
        historyOrders: [],
        favoriteList: []
    };
};

component.created = function () {
    this.tokenId = this.$route.query.token;
    this.getSellOrders();
    this.getBuyOrders();
    this.getTokenInfo();
    this.getMatchList();
    this.getFavoriteList();
    this.getCandlestick();
};
component.mounted = function() {
    ['.buySlider', '.sellSilder'].forEach((item) => {
        $(item).slider({
            ticks: [0, 25, 50, 75, 100],
            ticks_labels: ['0%', '25%', '50%', '75%', '100%'],
            ticks_snap_bounds: 1
        });
    })
}
component.methods = {
    getSellOrders() {
        qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/sell-order`, {}, 6000).fetch(res => {
            if (res.code === 200) {
                this.sellOrders = res.data || [];
            }
        })
    },
    getBuyOrders() {
        qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/buy-order`, {}, 6000).fetch(res => {
            if (res.code === 200) {
                this.buyOrders = res.data || [];
            }
        })
    },
    getTokenInfo() {
        qv.get(`/api/v1/lang/${app.lang}/token/${this.tokenId}`, {}).then(res => {
            if (res.code - 0 === 200) {
                this.baseInfo = res.data || {};
            }
        })
    },
    setPublish(price, amount) {
        this.inputs.buyPrice = price;
        this.inputs.buyAmount = amount;
        this.inputs.sellPrice = price;
        this.inputs.sellAmount = amount;
    },
    getcolorOccupationRatio: function (nowTotalPrice, historyTotalPrice) {
        var now = parseFloat(nowTotalPrice);
        var history = parseFloat(historyTotalPrice);
        if (now > history) return "100%";
        return parseInt(now * 100.0 / history) + "%";
    },
    getMatchList() {
        qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/match`, {}, 6000).fetch(res => {
            if (res.code === 200) {
                this.historyOrders = res.data || [];
            }
        })
    },
    formatTime(time) {
        return moment(time + 'Z').format('MM-DD HH:mm:ss')
    },
    getFavoriteList() {
        qv.get(`/api/v1/lang/${app.lang}/user/${account}/favorite`, {}).then(res => {
            if (res.code === 200) {
                this.favoriteList = res.data || [];
            }
        })
    },
    getCandlestick() {
        qv.get(`/api/v1/lang/${app.lang}/token/${this.tokenId}/candlestick`, {}).then(res => {
            if (res.code === 200) {
            }
        })
    },
    setTradeType(type) {
        this.control.trade = type;
    }
};