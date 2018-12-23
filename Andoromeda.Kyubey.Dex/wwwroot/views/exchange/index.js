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
        mobile: {
            nav: 'summary',
            summaryActive: 'candlestick',
            exchangeMode: 'buy',
            delegateActive: 'current'
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
        appAccount: app.account,
        //views
        openOrdersView: null,
        histroyOrdersView: null,
        sellOrdersView: null,
        buyOrdersView: null,
        matchListView: null,
        tokenInfoView: null,
        favoriteListView: null,
        balanceView: null,
        buyAndSellStatus: true
    };
};

component.created = function () {
    app.mobile.nav = null;
    this.tokenId = router.history.current.params.id;
    this.chart.symbol = this.tokenId;

    this.initCommonViews();

    this.getFavoriteList();

    if (app.isSignedIn) {
        this.initUserViews();
    }
};

component.methods = {
    initCommonViews() {
        this.getSellOrders();
        this.getBuyOrders();
        this.getMatchList();
        this.getTokenInfo();
    },
    initUserViews() {
        this.getBalances();
        this.getOpenOrders();
        this.getHistroyOrders();
    },
    delegateCallBack() {
        var self = this;
        this.delayRefresh(function () {
            self.refreshUserViews();
        });
    },
    cancelCallBack() {
        var self = this;
        this.delayRefresh(function () {
            self.refreshUserViews();
        });
    },
    doFavCallBack() {
        var self = this;
        this.delayRefresh(function () {
            self.getFavoriteList();
        });
    },
    refreshUserViews() {
        this.balanceView.refresh();
        this.openOrdersView.refresh();
        this.histroyOrdersView.refresh();
    },
    delayRefresh(callback) {
        setInterval(callback, 3000);
        setInterval(callback, 10000);
    },
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
    exchangeCancel: function (token, type, id) {
        const $t = this.$t.bind(this);
        if (app.loginMode === 'Scatter Addons' || app.loginMode === 'Scatter Desktop') {
            this.scatterCancel(token, type, id);
        }
        else if (app.loginMode == "Simple Wallet") {
            app.startQRCodeCancelOrder(id, token, type == 'buy');
        }
    },
    scatterCancel: function (token, type, id) {
        var self = this;
        const { account, requiredFields, eos } = app;
        const $t = this.$t.bind(this);
        eos.contract('kyubeydex.bp', { requiredFields })
            .then(contract => {
                if (type === 'buy') {
                    return contract.cancelbuy(
                        account.name,
                        token,
                        id,
                        {
                            authorization: [`${account.name}@${account.authority}`]
                        });
                } else {
                    return contract.cancelsell(
                        account.name,
                        token,
                        id,
                        {
                            authorization: [`${account.name}@${account.authority}`]
                        });
                }
            })
            .then(() => {
                self.delayRefresh(self.refreshUserViews);
                if (app.isMobile) {
                    app.notification("succeeded", $t('tip_cancel_succeed'));
                } else {
                    showModal($t('tip_cancel_succeed'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                }
            })
            .catch(error => {
                if (app.isMobile) {
                    app.notification("error", $t('tip_cancel_failed'));
                } else {
                    showModal($t('tip_cancel_failed'), error.message + $t('Please contact us if you have any questions'));
                }
            });
    },
    exchangeBuy() {
        const $t = this.$t.bind(this);
        var buyPrice = parseFloat(parseFloat(this.inputs.buyPrice).toFixed(8));
        var buyAmount = parseFloat(parseFloat(this.inputs.buyAmount).toFixed(4));
        var buyEosTotal = parseFloat(parseFloat(buyAmount * buyPrice).toFixed(4));
        if (buyEosTotal > this.eosBalance) {
            app.notification("error", $t('tip_balance_not_enough'));
            return;
        }
        if (buyEosTotal <= 0) {
            app.notification("error", $t('tip_correct_count'));
            return;
        }

        if (app.loginMode === 'Scatter Addons' || app.loginMode === 'Scatter Desktop') {
            this.scatterBuy();
        }
        else if (app.loginMode == "Simple Wallet") {
            $('#exchangeModal').modal('show');
            this.simpleWalletBuy();
        }
    },
    simpleWalletBuy() {
        var self = this;
        const $t = this.$t.bind(this);

        var buySymbol = this.tokenId
        var buyPrice = parseFloat(parseFloat(this.inputs.buyPrice).toFixed(8));
        var buyAmount = parseFloat(parseFloat(this.inputs.buyAmount).toFixed(4));
        var buyEosTotal = parseFloat(parseFloat(buyAmount * buyPrice).toFixed(4));

        if (this.control.trade === 'limit') {
            var reqObj = this._getExchangeRequestObj(app.account.name, "kyubeydex.bp", buyEosTotal, "eosio.token", "EOS", 4, app.uuid, `${buyAmount.toFixed(4)} ${buySymbol}`);
            app.startQRCodeExchange($t('exchange_tip'), JSON.stringify(reqObj),
                [
                    {
                        color: 'green',
                        text: `${$t('exchange_buy')} ${buySymbol}`
                    },
                    {
                        text: `${$t('exchange_price')}: ${parseFloat(buyPrice).toFixed(8)} EOS`
                    },
                    {
                        text: `${$t('exchange_amount')}: ${parseFloat(buyAmount).toFixed(4)} ${buySymbol}`
                    },
                    {
                        text: `${$t('exchange_total')}: ${parseFloat(buyEosTotal).toFixed(4)} EOS`
                    }
                ]);
        }
        else if (this.control.trade === 'market') {

            var reqObj = this._getExchangeRequestObj(app.account.name, "kyubeydex.bp", buyEosTotal, "eosio.token", "EOS", 4, app.uuid, `market`);
            app.startQRCodeExchange($t('exchange_tip'), JSON.stringify(reqObj),
                [
                    {
                        color: 'green',
                        text: `${$t('exchange_buy')} ${buySymbol}`
                    },
                    {
                        text: `${$t('exchange_total')}: ${parseFloat(buyEosTotal).toFixed(4)} EOS`
                    }
                ]);
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
                    self.delayRefresh(self.refreshUserViews);
                    if (app.isMobile) {
                        app.notification("succeeded", $t('delegate_succeed'));
                    } else {
                        showModal($t('delegate_succeed'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                    }
                })
                .catch(error => {
                    if (app.isMobile) {
                        app.notification("error", $t('delegate_failed'));
                    } else {
                        self.handleScatterException(error, $t('delegate_failed'));
                    }
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
                    self.delayRefresh(self.refreshUserViews);
                    if (app.isMobile) {
                        app.notification("succeeded", $t('Transaction Succeeded'));
                    } else {
                        showModal($t('Transaction Succeeded'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                    }
                })
                .catch(error => {
                    if (app.isMobile) {
                        app.notification("error", $t('Transaction Failed'));
                    } else {
                        self.handleScatterException(error, $t('Transaction Failed'));
                    }
                });
        }
    },
    handleScatterException(error, tipTitle) {
        const $t = this.$t.bind(this);
        if (typeof error === 'string') {
            error = JSON.parse(error)
        }
        if (error.error != null && error.error.code != null) {
            showModal(tipTitle, $t(error.error.what));
        }
        else
            showModal(tipTitle, error.message + $t('Please contact us if you have any questions'));
    },
    exchangeSell() {
        const $t = this.$t.bind(this);
        var sellAmount = parseFloat(parseFloat(this.inputs.sellAmount).toFixed(4));
        if (sellAmount > this.tokenBalance) {
            app.notification("error", $t('tip_balance_not_enough'));
            return;
        }
        if (sellAmount <= 0) {
            app.notification("error", $t('tip_correct_count'));
            return;
        }

        if (app.loginMode === 'Scatter Addons' || app.loginMode === 'Scatter Desktop') {
            this.scatterSell();
        }
        else if (app.loginMode == "Simple Wallet") {
            $('#exchangeModal').modal('show');
            this.simpleWalletSell();
        }
    },
    simpleWalletSell() {
        const $t = this.$t.bind(this);
        var sellSymbol = this.tokenId;
        var sellPrice = parseFloat(parseFloat(this.inputs.sellPrice).toFixed(4));
        var sellAmount = parseFloat(parseFloat(this.inputs.sellAmount).toFixed(4));
        var sellTotal = parseFloat(parseFloat(sellPrice * sellAmount).toFixed(4));

        if (this.control.trade === 'limit') {
            var reqObj = this._getExchangeRequestObj(app.account.name, "kyubeydex.bp", sellAmount, this.baseInfo.contract.transfer, sellSymbol, 4, app.uuid, `${sellTotal.toFixed(4)} EOS`);
            app.startQRCodeExchange($t('exchange_tip'), JSON.stringify(reqObj),
                [
                    {
                        color: 'green',
                        text: `${$t('exchange_sell')} ${sellSymbol}`
                    },
                    {
                        text: `${$t('exchange_sellprice')}: ${parseFloat(sellPrice).toFixed(8)} EOS`
                    },
                    {
                        text: `${$t('exchange_sellamount')}: ${parseFloat(sellAmount).toFixed(4)} ${sellSymbol}`
                    },
                    {
                        text: `${$t('exchange_total')}: ${parseFloat(sellTotal).toFixed(4)} EOS`
                    }
                ]);
        }
        else if (this.control.trade === 'market') {
            sellTotal = parseFloat(parseFloat(this.inputs.sellTotal).toFixed(4));

            var reqObj = this._getExchangeRequestObj(app.account.name, "kyubeydex.bp", sellAmount, this.baseInfo.contract.transfer, sellSymbol, 4, app.uuid, `market`);

            app.startQRCodeExchange($t('exchange_tip'), JSON.stringify(reqObj),
                [
                    {
                        color: 'green',
                        text: `${$t('exchange_sell')} ${sellSymbol}`
                    },
                    {
                        text: `${$t('exchange_total')}: ${parseFloat(sellTotal).toFixed(4)} EOS`
                    }
                ]);
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
            eos.contract(this.baseInfo.contract.transfer, { requiredFields })
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
                    self.delayRefresh(self.refreshUserViews);

                    showModal($t('delegate_succeed'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                })
                .catch(error => {
                    showModal($t('Transaction Failed'), error.message + $t('Please contact us if you have any questions'));
                });
        }
        else if (this.control.trade === 'market') {
            eos.contract(this.baseInfo.contract.transfer, { requiredFields })
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
                    self.delayRefresh(self.refreshUserViews);

                    showModal($t('Transaction Succeeded'), $t('You can confirm the result in your wallet') + ',' + $t('Please contact us if you have any questions'));
                })
                .catch(error => {
                    showModal($t('Transaction Failed'), error.message + $t('Please contact us if you have any questions'));
                });
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
            "callback": `${app.currentHost}/api/v1/simplewallet/callback/exchange?uuid=${uuid}&sign=${_this._getExchangeSign(uuid)}`
        };
        return loginObj;
    },
    getSellOrders() {
        this.sellOrdersView = qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/sell-order`, {}, 6000);
        this.sellOrdersView.fetch(res => {
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
        this.buyOrdersView = qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/buy-order`, {}, 6000);
        this.buyOrdersView.fetch(res => {
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
        this.tokenInfoView = qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}`, {});
        this.tokenInfoView.fetch(res => {
            if (res.code === 200) {
                this.baseInfo = res.data || {};
            }
        });
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
        this.matchListView = qv.createView(`/api/v1/lang/${app.lang}/token/${this.tokenId}/match`, {}, 6000);
        this.matchListView.fetch(res => {
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
        const name = app.account ? app.account.name : null
        this.favoriteListView = qv.createView(`/api/v1/lang/${app.lang}/user/${name}/favorite`, {});
        this.favoriteListView.fetch(res => {
            if (res.code === 200) {
                this.favoriteList = res.data || [];
            }
        });
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
            this.balanceView = qv.createView(`/api/v1/lang/${app.lang}/Node/${app.account.name}/balance/${this.tokenId}`, {});
            this.balanceView.fetch(res => {
                if (res.code - 0 === 200) {
                    self.eosBalance = parseFloat(res.data['EOS'] || 0);
                    self.tokenBalance = parseFloat(res.data[this.tokenId.toUpperCase()] || 0);
                }
            });
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
            let isZero = this.inputs.buyPrice === '0.0000';
            this.inputs.buyAmount = isZero ? '0.0000' : parseFloat(this.inputs.buyTotal / this.inputs.buyPrice).toFixed(4);
        } else {
            let isZero = this.inputs.buyPrice === '0.0000';
            this.inputs.sellAmount = isZero ? '0.0000' : parseFloat(this.inputs.sellTotal / this.inputs.buyPrice).toFixed(4);
        }
    },
    handleBlur(n, m = 8) {
        var currentVal = this.inputs[n];
        if (!currentVal) {
            this.inputs[n] = 0.0.toFixed(m);
        }
        else
            this.inputs[n] = parseFloat(currentVal).toFixed(m);
    },
    handlePrecentChange(n, x) {
        if (this.isSignedIn) {
            this[n] = x;
            if (n === 'buyPrecent') {
                let isZero = this.inputs.buyPrice === '0.0000';
                this.inputs.buyTotal = parseFloat(this.eosBalance * x).toFixed(4);
                this.inputs.buyAmount = isZero ? '0.0000' : parseFloat(this.inputs.buyTotal / this.inputs.buyPrice).toFixed(4);
            } else {
                this.inputs.sellAmount = parseFloat(this.tokenBalance * x).toFixed(4);
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
        var self = this;
        self.openOrdersView = qv.createView(`/api/v1/lang/${app.lang}/User/${app.account.name}/current-delegate`, {});
        self.openOrdersView.fetch(res => {
            if (res.code === 200) {
                self.openOrders = res.data || [];
            }
        })
    },
    getHistroyOrders() {
        var self = this;
        self.histroyOrdersView = qv.createView(`/api/v1/lang/${app.lang}/User/${app.account.name}/history-delegate`, {});
        self.histroyOrdersView.fetch(res => {
            if (res.code === 200) {
                self.orderHistory = res.data.result;
            }
        })
    },
    redirectToDetail(token) {
        app.redirect('/exchange/:id', '/exchange/' + token, { id: token }, {});
    },
    toggleFav(token, i) {
        const isAdd = !this.favoriteListFilter[i].favorite;
        app.toggleFav(token, isAdd, () => {
            this.favoriteListFilter[i].favorite = isAdd;
        })
    }
}
component.computed = {
    isSignedIn: function () {
        return !(app.account == null || app.account.name == null);
    },
    favoriteListFilter() {
        if (this.control.markets === 'favorite') {
            return this.favoriteList.filter(x => x.symbol.includes(this.inputs.tokenSearchInput.toUpperCase()) && x.favorite)
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
            this.initUserViews();
            this.getFavoriteList();
        }
        //logout
        else {
            //comments: stop qv job or use signalR
        }
    },
    //reload multi language ajax method
    '$root.lang': function () {
        this.getTokenInfo();
        //this.initCandlestick();
    },
    deep: true
}