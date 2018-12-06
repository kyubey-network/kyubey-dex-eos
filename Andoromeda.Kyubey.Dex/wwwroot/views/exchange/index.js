component.data = function () {
    return {
        sellOrders: [],
        buyOrders: [],
        control: {
            order: 'mixed'
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
        tokenId: 'KBY',
        baseInfo: {}
    };
};

component.created = function () {
    this.getSellOrders();
    this.getBuyOrders();
    this.getTokenInfo();
};

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
};