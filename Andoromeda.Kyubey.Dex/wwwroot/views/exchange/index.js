component.data = function () {
    return {
        sellOrders: [],
        maxAmountSellOrder: 0,
        maxAmountBuyOrder: 0,
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
            tokenSearchInput: ''
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
            loading_screen: { backgroundColor: '#292929' },
            studies_overrides: {
                "volume.precision": 0
            },
            overrides: {
                "paneProperties.background": "#292929",
                "paneProperties.vertGridProperties.color": "rgba(0,0,0,0)",
                "paneProperties.horzGridProperties.color": "rgba(0,0,0,0)",
                "scalesProperties.textColor": "#9194a4",
                volumePaneSize: "medium",
                "paneProperties.legendProperties.showStudyArguments": !0,
                "paneProperties.legendProperties.showStudyTitles": !0,
                "paneProperties.legendProperties.showStudyValues": !0
            },
            interval: 240,
            symbol: ''
        },
        chartWidget: null,
        tokenId: '',
        account: '',
        baseInfo: {},
        showIntroduction: false,
        latestTransactions: [],
        favoriteList: [],
        buyPrecent: 0,
        sellPrecent: 0,
        openOrders: [],
        orderHistory: [],
        executeControl: {
            buy: 0,
            sell: 0
        },
        eosBalance: 0,
        tokenBalance: 0,
        appAccount: app.account
    };
};

component.created = function () {
    this.tokenId = router.history.current.params.id;
    this.chart.symbol = this.tokenId;
    this.getOrders();
    this.getTokenInfo();
    this.getFavoriteList();
};

component.methods = {
    getOrders() {
        this.getSellOrders();
        this.getBuyOrders();
        this.getMatchList();
    },
    getCurrentOrders() { },
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
            period: period * 60
        }).then(x => {
            callback(x);
        });
    },
    exchangeBuy() {
        if (app.loginMode === 'Scatter Addons' || app.loginMode === 'Scatter Desktop') {
            this.scatterBuy();
        }
        else if (app.loginMode == "Simple Wallet") {
            $('#exchangeModal').modal('show');
            this.simpleWalletBuy();
        }
    },
    simpleWalletBuy() {
        var buySymbol = this.tokenId
        var buyPrice = parseFloat(parseFloat(this.inputs.buyPrice).toFixed(8));
        var buyAmount = parseFloat(parseFloat(this.inputs.buyAmount).toFixed(4));
        var buyEosTotal = parseFloat(buyAmount * buyPrice);
        if (this.control.trade === 'limit') {
            this.simpleWalletExchange("buy", app.account.name, "kyubeydex.bp", buyEosTotal, "eosio.token", buyPrice, buyAmount, buySymbol, "EOS", 4);
        }
        else if (this.control.trade === 'market') {
            this.simpleWalletExchange("buy-market", app.account.name, "kyubeydex.bp", buyEosTotal, "eosio.token", buyPrice, buyAmount, buySymbol, "EOS", 4);
        }
    },
    scatterBuy() {
        var self = this;
        const { account, requiredFields, eos } = app;
        const $t = this.$t.bind(this);
        if (this.control.trade === 'limit') {
            var price = parseFloat(parseFloat(this.inputs.buyPrice).toFixed(8));
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
                    self.handleScatterException(error);
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
                    self.handleScatterException(error);
                });
        }
    },
    handleScatterException(error) {
        const $t = this.$t.bind(this);
        if (typeof error === 'string') {
            error = JSON.parse(error)
        }
        if (error.error != null && error.error.code != null) {
            showModal($t('Transaction Failed'), $t(error.error.what));
        }
        else
            showModal($t('Transaction Failed'), error.message + $t('Please contact us if you have any questions'));
    },
    exchangeSell() {
        if (app.loginMode === 'Scatter Addons' || app.loginMode === 'Scatter Desktop') {
            this.scatterSell();
        }
        else if (app.loginMode == "Simple Wallet") {
            $('#exchangeModal').modal('show');
            this.simpleWalletSell();
        }
    },
    simpleWalletSell() {
        var sellSymbol = this.tokenId
        var sellPrice = parseFloat(parseFloat(this.inputs.sellPrice).toFixed(4));
        var sellAmount = parseFloat(parseFloat(this.inputs.sellAmount).toFixed(4));
        var sellTotal = parseFloat(sellPrice * sellAmount);
        if (this.control.trade === 'limit') {
            this.simpleWalletExchange("sell", app.account.name, "kyubeydex.bp", sellTotal, "dacincubator", sellPrice, sellAmount, "EOS", sellSymbol, 4);
        }
        else if (this.control.trade === 'market') {
            sellTotal = parseFloat(parseFloat(this.inputs.sellTotal).toFixed(4));
            this.simpleWalletExchange("sell-market", app.account.name, "kyubeydex.bp", sellTotal, "dacincubator", sellPrice, sellAmount, "EOS", sellSymbol, 4);
        }
    },
    scatterSell() {
        var self = this;
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
    simpleWalletExchange: function (type, from, to, amount, contract, targetPrice, taretAmount, taretSymbol, symbol, precision) {
        //set qrcode timer
        app.qrcodeIsValid = true;
        clearTimeout(app.qrcodeTimer);
        app.qrcodeTimer = setTimeout(function () {
            app.qrcodeIsValid = false;
        }, 3 * 60 * 1000);

        this.exchange.type = type;
        this.exchange.from = from;
        this.exchange.to = to;
        this.exchange.amount = amount;
        this.exchange.contract = contract;
        this.exchange.symbol = symbol;
        this.exchange.taretSymbol = taretSymbol;
        this.exchange.precision = precision;
        this.exchange.taretAmount = taretAmount;
        this.exchange.price = targetPrice;
        if (this.control.trade === 'limit') {
            this.generateExchangeQRCode("exchangeQRCodeBox", from, to, amount, contract, symbol, precision, app.uuid, `${taretAmount.toFixed(precision)} ${taretSymbol}`);
        }
        else if (this.control.trade === 'market') {
            this.generateExchangeQRCode("exchangeQRCodeBox", from, to, amount, contract, symbol, precision, app.uuid, `market`);
        }
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
            "callback": `${app.currentHost}/api/v1/simplewallet/callback/exchange?uuid=${uuid}&sign=${_this._getExchangeSign(uuid)}`,
        };
        return loginObj;
    },
    generateExchangeQRCode: function (idSelector, from, to, amount, contract, symbol, precision, uuid, dappData) {
        $("#" + idSelector).empty();
        var reqObj = this._getExchangeRequestObj(from, to, amount, contract, symbol, precision, uuid, dappData);
        var qrcode = new QRCode(idSelector, {
            text: JSON.stringify(reqObj),
            width: 160,
            height: 160,
            colorDark: "#000000",
            colorLight: "#ffffff",
            correctLevel: QRCode.CorrectLevel.L
        });
    },
    getSellOrders() {
        qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/sell-order`, {}, 6000).fetch(res => {
            if (res.code === 200 && res.request.symbol === this.tokenId) {
                this.sellOrders = res.data || [];
                let maxAmountSellOrder = 0;
                res.data.forEach(item => {
                    maxAmountSellOrder = Math.max(maxAmountSellOrder, item.amount)
                })
                this.maxAmountSellOrder = maxAmountSellOrder;
            }
        })
    },
    getBuyOrders() {
        qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/buy-order`, {}, 6000).fetch(res => {
            if (res.code === 200 && res.request.symbol === this.tokenId) {
                this.buyOrders = res.data || [];
                let maxAmountBuyOrder = 0;
                res.data.forEach(item => {
                    maxAmountBuyOrder = Math.max(maxAmountBuyOrder, item.amount)
                })
                this.maxAmountBuyOrder = maxAmountBuyOrder;
            }
        })
    },
    getTokenInfo() {
        qv.get(`/api/v1/lang/${app.lang}/token/${this.tokenId}`, {}).then(res => {
            if (res.code === 200) {
                this.baseInfo = res.data || {};
            }
        })
    },
    setPublish(price, amount, total) {
        price = parseFloat(price).toFixed(8);
        amount = parseFloat(amount).toFixed(4);
        total = parseFloat(total).toFixed(4);
        this.inputs.buyPrice = price;
        this.inputs.sellPrice = price;
        if (this.isSignedIn) {
            // calculate buyTotal & buyAmount
            if (total > parseFloat(this.eosBalance)) {
                this.inputs.buyTotal = parseFloat(this.eosBalance).toFixed(4);
                this.inputs.buyAmount = parseFloat(this.inputs.buyTotal / this.inputs.buyPrice).toFixed(4);
            } else {
                this.inputs.buyTotal = total;
                this.inputs.buyAmount = amount;
            }
            // calculate sellTotal & sellAmount
            if (amount > parseFloat(this.tokenBalance)) {
                this.inputs.sellAmount = parseFloat(this.tokenBalance).toFixed(4);
                this.inputs.sellTotal = parseFloat(this.inputs.sellAmount * this.inputs.sellPrice).toFixed(4);
            } else {
                this.inputs.sellAmount = amount;
                this.inputs.sellTotal = total;
            }
        } else {
            this.inputs.buyAmount = '0.0000';
            this.inputs.sellAmount = '0.0000';
            this.inputs.buyTotal = '0.0000';
            this.inputs.sellTotal = '0.0000';
        }
    },
    getcolorOccupationRatio: function (nowTotalPrice, historyTotalPrice) {
        var now = parseFloat(nowTotalPrice);
        var history = parseFloat(historyTotalPrice);
        if (now > history) return "100%";
        return parseInt(now * 100.0 / history) + "%";
    },
    getMatchList() {
        qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/match`, {}, 6000).fetch(res => {
            if (res.code === 200 && res.request.symbol === this.tokenId) {
                this.latestTransactions = res.data || [];
            }
        })
    },
    formatTime(time) {
        return moment(time + 'Z').format('YYYY-MM-DD HH:mm:ss')
    },
    formatShortTime(time) {
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
    },
    getBalances: function () {
        if (this.isSignedIn) {
            var self = this;
            qv.get(`/api/v1/lang/${app.lang}/Node/${app.account.name}/balance`, {}).then(res => {
                if (res.code - 0 === 200) {
                    self.eosBalance = parseFloat(res.data['EOS'] || 0);
                    self.tokenBalance = parseFloat(res.data[this.tokenId.toUpperCase()] || 0);
                }
            })
        }
    },
    handlePriceChange(type) {
        if (type === 'buy') {
            this.inputs.buyTotal = parseFloat(this.inputs.buyAmount * this.inputs.buyPrice).toFixed(4)
        } else {
            this.inputs.sellTotal = parseFloat(this.inputs.sellAmount * this.inputs.sellPrice).toFixed(4)
        }
    },
    handleAmountChange(type) {
        if (type === 'buy') {
            this.inputs.buyTotal = parseFloat(this.inputs.buyAmount * this.inputs.buyPrice).toFixed(4)
        } else {
            this.inputs.sellTotal = parseFloat(this.inputs.sellAmount * this.inputs.sellPrice).toFixed(4)
        }
    },
    handleTotalChange(type) {
        if (type === 'buy') {
            let isZero = this.inputs.buyPrice === '0.000000';
            this.inputs.buyAmount = isZero ? '0.00000' : parseFloat(this.inputs.buyTotal / this.inputs.buyPrice).toFixed(5);
        } else {
            let isZero = this.inputs.buyPrice === '0.000000';
            this.inputs.sellAmount = isZero ? '0.00000' : parseFloat(this.inputs.sellTotal / this.inputs.buyPrice).toFixed(5);
        }
    },
    handleBlur(n, m = 8) {
        this.inputs[n] = parseFloat(this.inputs[n]).toFixed(m);
    },
    handlePrecentChange(n, x) {
        if (this.isSignedIn) {
            this[n] = x;
            if (n === 'buyPrecent') {
                let isZero = this.inputs.buyPrice === '0.000000';
                this.inputs.buyTotal = parseFloat(this.eosBalance * x).toFixed(4);
                this.inputs.buyAmount = isZero ? '0.00000' : parseFloat(this.inputs.buyTotal / this.inputs.buyPrice).toFixed(5);
            } else {
                this.inputs.sellAmount = parseFloat(this.eosBalance * x).toFixed(5);
                this.inputs.sellTotal = parseFloat(this.inputs.sellAmount * this.inputs.sellPrice).toFixed(4);
            }
        }
    },
    pricePrecision(n) {
        return parseFloat(n).toFixed(8);
    },
    amountPrecision(n) {
        return parseFloat(n).toFixed(4);
    },
    totalPrecision(n) {
        return parseFloat(n).toFixed(4);
    },
    getOpenOrders() {
        qv.get(`/api/v1/lang/${app.lang}/User/${app.account.name}/current-delegate`, {}).then(res => {
            if (res.code === 200) {
                this.openOrders = res.data || [];
            }
        })
    },
    getHistroyOrders() {
        qv.get(`/api/v1/lang/${app.lang}/User/${app.account.name}/history-delegate`, {}).then(res => {
            if (res.code === 200) {
                this.orderHistory = res.data || [];
            }
        })
    },
    redirectToDetail(token) {
        app.redirect('/exchange/:id', '/exchange/' + token, { id: token }, {});
    }
};
component.computed = {
    isSignedIn: function () {
        return !(app.account == null || app.account.name == null);
    },
    favoriteListFilter() {
        if (this.inputs.tokenSearchInput === '') {
            return this.favoriteList
        } else {
            return this.favoriteList.filter(x => x.symbol.includes(this.inputs.tokenSearchInput.toUpperCase()))
        }
    }
};
component.watch = {
    'chart.interval': function (v) {
        //this.chartWidget.chart().setResolution(v);
        this.initCandlestick();
    },
    '$root.isSignedIn': function (val) {
        if (val === true) {
            this.getBalances()
            this.getOpenOrders();
            this.getHistroyOrders();
        }
    },
    //reload multi language ajax method
    '$root.lang': function () {
        this.getTokenInfo();
        //this.initCandlestick();
    },
    deep: true
}