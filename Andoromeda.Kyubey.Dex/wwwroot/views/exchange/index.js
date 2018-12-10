component.data = function () {
    return {
        sellOrders: [],
        buyOrders: [],
        control: {
            order: 'mixed',
            markets: 'eos',
            trade: 'limit'
        },
        exchange: {
            type: "buy",
            from: null,
            to: null,
            amount: 0,
            contract: null,
            symbol: null,
            precision: 0,
            taretSymbol: null,
            taretAmount: 0
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
        favoriteList: [],
        buyPrecent: 0,
        sellPrecent: 0
    };
};

component.created = function () {
    this.tokenId = router.history.current.params.id;
    this.getSellOrders();
    this.getBuyOrders();
    this.getTokenInfo();
    this.getMatchList();
    this.getFavoriteList();
};
component.mounted = function () {
    //sample
    //this.simpleWalletExchange("buy", "qinxiaowen11", "kyubeydex.bp", 0.001, "eosio.token", 5, "KBY", "EOS", 4);
    //this.simpleWalletExchange("sell", "qinxiaowen11", "kyubeydex.bp", 1, "dacincubator", 1, "EOS", "KBY", 4);
}
component.methods = {
    scatterBuy() {
        const {account, requiredFields, eos} = app;
        const $t = this.$t.bind(this);
        if (this.control.trade === 'limit') {
            var price = parseFloat(parseFloat(this.inputs.buyPrice).toFixed(4));
            var ask = parseFloat(parseFloat(this.inputs.buyAmount).toFixed(4));
            var bid = parseFloat(ask * price);
            eos.contract('eosio.token', { requiredFields })
                .then(contract => {
                    return contract.transfer(
                        account.name,
                        'kyubeydex.bp',
                        bid.toFixed(4) + ' EOS',
                        ask.toFixed(4) + ' ' + this.tokenId,
                        {
                            authorization: [`${account.name}@${account.authority}`]
                        });
                })
                .then(() => {
                    self.getCurrentOrders();
                    self.getOrders();
                    self.getBalances();
                    showModal($t('Transaction Succeeded'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                })
                .catch(error => {
                    showModal($t('Transaction Failed'), error.message + $t('Please contact us if you have any questions'));
                });
        } 
        else if (this.control.trade === 'market') {
            eos.contract('eosio.token', { requiredFields })
            .then(contract => {
                return contract.transfer(
                    account.name,
                    'kyubeydex.bp',
                    parseFloat(this.inputs.buyTotal).toFixed(4) + ' EOS',
                    'market',
                    {
                        authorization: [`${account.name}@${account.authority}`]
                    });
            })
            .then(() => {
                self.getCurrentOrders();
                self.getOrders();
                self.getBalances();
                showModal($t('Transaction Succeeded'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
            })
            .catch(error => {
                showModal($t('Transaction Failed'), error.message + $t('Please contact us if you have any questions'));
            });
        }
    },
    scatterSell() {
        const {account, requiredFields, eos} = app;
        const $t = this.$t.bind(this);
        if (this.control.trade === 'limit') {
            var price = parseFloat(parseFloat(this.inputs.sellPrice).toFixed(4));
            var bid = parseFloat(parseFloat(this.inputs.sellAmount).toFixed(4));
            var ask = parseFloat(bid * price);
            eos.contract('dacincubator', { requiredFields })
                .then(contract => {
                    return contract.transfer(
                        account.name,
                        'kyubeydex.bp',
                        bid.toFixed(4) + ' ' + this.tokenId,
                        ask.toFixed(4) + ' EOS',
                        {
                            authorization: [`${account.name}@${account.authority}`]
                        });
                })
                .then(() => {
                    self.getCurrentOrders();
                    self.getOrders();
                    self.getBalances();
                    showModal($t('Transaction Succeeded'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                })
                .catch(error => {
                    showModal($t('Transaction Failed'), error.message + $t('Please contact us if you have any questions'));
                });
        } 
        else if (this.control.trade === 'market') {
            eos.contract('dacincubator', { requiredFields })
                .then(contract => {
                    return contract.transfer(
                        account.name,
                        'kyubeydex.bp',
                        parseFloat(this.inputs.sellAmount).toFixed(4) + ' ' + this.tokenId,
                        'market',
                        {
                            authorization: [`${account.name}@${account.authority}`]
                        });
                })
                .then(() => {
                    self.getCurrentOrders();
                    self.getOrders();
                    self.getBalances();
                    showModal($t('Transaction Succeeded'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                })
                .catch(error => {
                    showModal($t('Transaction Failed'), error.message + $t('Please contact us if you have any questions'));
                });
        }
    },
    handlePriceChange(e) {},
    handleAmountChange() {},
    handleTotalChange() {},
    simpleWalletExchange: function (type, from, to, amount, contract, taretAmount, taretSymbol, symbol, precision) {
        $('#exchangeModal').modal('show');
        this.exchange.type = type;
        this.exchange.from = from;
        this.exchange.to = to;
        this.exchange.amount = amount;
        this.exchange.contract = contract;
        this.exchange.symbol = symbol;
        this.exchange.taretSymbol = taretSymbol;
        this.exchange.precision = precision;
        this.exchange.taretAmount = taretAmount;
        this.generateExchangeQRCode("exchangeQRCodeBox", from, to, amount, contract, symbol, precision, app.uuid, `${taretAmount.toFixed(precision)} ${taretSymbol}`);
    },
    _getExchangeSign: function (uuid) {
        return uuid;
    },
    _getExchangeRequestObj: function (from, to, amount, contract, symbol, precision, uuid, dappData) {
        var _this = this;
        var loginObj = {
            "protocol": "SimpleWallet",
            "version": "1.0",
            "dappName": "Kyubey",
            "dappIcon": `${app._currentHost}/img/KYUBEY_logo.png`,
            "action": "transfer",
            "from": from,
            "to": to,
            "amount": amount,
            "contract": contract,
            "symbol": symbol,
            "precision": precision,
            "dappData": dappData,
            "desc": `${symbol} exchange`,
            "expired": new Date().getTime() + (3 * 60 * 1000),
            "callback": `${app._currentHost}/api/simplewallet/callback/exchange?uuid=${uuid}&sign=${_this._getExchangeSign(uuid)}`,
        };
        return loginObj;
    },
    generateExchangeQRCode: function (idSelector, from, to, amount, contract, symbol, precision, uuid, dappData) {
        $("#" + idSelector).empty();
        var reqObj = this._getExchangeRequestObj(from, to, amount, contract, symbol, precision, uuid, dappData);
        var qrcode = new QRCode(idSelector, {
            text: JSON.stringify(reqObj),
            width: 114,
            height: 114,
            colorDark: "#000000",
            colorLight: "#ffffff",
            correctLevel: QRCode.CorrectLevel.L
        });
    },
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
        qv.get(`/api/v1/lang/${app.lang}/user/${app.account}/favorite`, {}).then(res => {
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
    },
    login() {
        $('#loginModal').modal('show');
    },
    isValidInput: function (value, precision) {
        if (precision!=null&&precision == 4) {
            if (! /^\d*(?:\.\d{0,4})?$/.test(value)) {
                return false;
            }
        }
        else
        {
            if (! /^\d*(?:\.\d{0,8})?$/.test(value)) {
                return false;
            }
        }
        return true;
    }
};
component.computed = {
    isSignedIn: function () {
        return !!app.account;
    }
};
component.watch = {
    'inputs.pair': function () {
        this.getPairs();
    },
    'inputs.buyPrice': function (val) {
        if (!this.isValidInput(val)) {
            this.inputs.buyPrice = this.inputs.vaildbuyPriceInput;
            val = this.inputs.vaildbuyPriceInput;
        }
        this.inputs.vaildbuyPriceInput = val;
        this.inputs.buyTotal = val * this.inputs.buyAmount;
    },
    'inputs.buyAmount': function (val) {
        if (!this.isValidInput(val,4)) {
            this.inputs.buyAmount = this.inputs.vaildbuyAmountInput;
            val = this.inputs.vaildbuyAmountInput;
        }
        this.inputs.vaildbuyAmountInput = val;
        this.inputs.buyTotal = val * this.inputs.buyPrice;
    },
    'inputs.buyTotal': function (val) {
        if (isNaN(val)) {
            return;
        }
        if (this.control.trade === 'limit') {
            this.inputs.buyPrice = val / (this.inputs.buyAmount || 1);
        }
    },
    'inputs.sellPrice': function (val) {
        if (!this.isValidInput(val)) {
            this.inputs.sellPrice = this.inputs.vaildsellPriceInput;
            val = this.inputs.vaildsellPriceInput;
        }
        this.inputs.vaildsellPriceInput = val;
        this.inputs.sellTotal = val * this.inputs.sellAmount;
    },
    'inputs.sellAmount': function (val) {
        if (!this.isValidInput(val, 4)) {
            this.inputs.sellAmount = this.inputs.vaildsellAmountInput;
            val = this.inputs.vaildsellAmountInput;
        }
        this.inputs.vaildsellAmountInput = val;
        this.inputs.sellTotal = val * this.inputs.sellPrice;
    },
    'inputs.sellTotal': function (val) {
        if (isNaN(val)) {
            return;
        }
        if (this.control.trade === 'limit') {
            this.inputs.sellPrice = val / (this.inputs.sellAmount || 1);
        }
    },
    deep: true
};