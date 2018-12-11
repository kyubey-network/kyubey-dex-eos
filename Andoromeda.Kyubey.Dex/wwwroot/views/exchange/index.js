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
        chart: {
            fullscreen: true,
            timezone: "Asia/Shanghai",
            container_id: "tv_chart_container",
            datafeed: new FeedBase(),
            library_path: "/js/candlestick/charting_library/",
            locale: app.lang,
            disabled_features: ["control_bar", "timeframes_toolbar", "main_series_scale_menu", "symbol_search_hot_key", "header_symbol_search", "header_resolutions", "header_settings", "save_chart_properties_to_local_storage", "header_chart_type", "header_compare", "header_undo_redo", "header_screenshot", "use_localstorage_for_settings", "volume_force_overlay"],
            enabled_features: ["keep_left_toolbar_visible_on_small_screens", "side_toolbar_in_fullscreen_mode", "hide_left_toolbar_by_default", "left_toolbar", "keep_left_toolbar_visible_on_small_screens", "hide_last_na_study_output", "move_logo_to_main_pane", "dont_show_boolean_study_arguments"],
            custom_css_url: "chart.css",
            studies_overrides: {
                "volume.precision": 0
            },

            interval: 240,
            symbol: ''
        },
        chartWidget: null,
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
    this.chart.symbol = this.tokenId;
    this.getSellOrders();
    this.getBuyOrders();
    this.getTokenInfo();
    this.getMatchList();
    this.getFavoriteList();
};

component.methods = {
    dateObjToString: function (date) {
        return `${date.getFullYear()}/${(date.getMonth() + 1)}/${date.getDate()} ${date.getHours()}:${date.getMinutes()}:${date.getSeconds()} `;
    },
    initCandlestick: function () {
        var self = this;
        this.chartWidget = new window.TradingView.widget(this.chart);
        FeedBase.prototype.getBars = function (symbolInfo, resolution, rangeStartDate, rangeEndDate, onResult, onError) {
            self.getCandlestickData(self.tokenId, new Date(rangeStartDate * 1000), new Date(rangeEndDate * 1000), self.chart.interval, function (apiResult) {
                var data = apiResult.data;
                if (data && Array.isArray(data)) {
                    var meta = { noData: false };
                    var bars = [];
                    if (data.length) {
                        for (var i = 0; i < data.length; i += 1) {
                            bars.push({
                                time: Number(new Date(data[i].time)),
                                close: data[i].closing,
                                open: data[i].opening,
                                high: data[i].max,
                                low: data[i].min,
                                volume: data[i].volume
                            });
                        }
                    } else {
                        meta = { noData: true };
                    }
                    onResult(bars, meta);
                }
            });
        }
    },
    getCandlestickData: function (tokenId, startDate, endDate, period, callback) {
        var _this = this;
        var begin = _this.dateObjToString(startDate);
        var end = _this.dateObjToString(endDate);
        qv.get(`/api/v1/lang/${app.lang}/token/${tokenId}/candlestick`, {
            begin: begin,
            end: end,
            period: period
        }).then(x => {
            callback(x);
        });
    },
    scatterBuy() {
        const { account, requiredFields, eos } = app;
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
        const { account, requiredFields, eos } = app;
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
    handlePriceChange(e) { },
    handleAmountChange() { },
    handleTotalChange() { },
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
            "dappIcon": `${app.currentHost}/img/KYUBEY_logo.png`,
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
            "callback": `${app.currentHost}/api/simplewallet/callback/exchange?uuid=${uuid}&sign=${_this._getExchangeSign(uuid)}`,
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
        if (precision != null && precision == 4) {
            if (! /^\d*(?:\.\d{0,4})?$/.test(value)) {
                return false;
            }
        }
        else {
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
    'chart.interval': function (v) {
        //this.chartWidget.chart().setResolution(v);
        this.initCandlestick();
    },
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
        if (!this.isValidInput(val, 4)) {
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