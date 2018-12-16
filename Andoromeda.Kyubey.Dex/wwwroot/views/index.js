app = new Vue({
    router: router,
    data: {
        chainId: 'aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906',
        host: 'nodes.get-scatter.com',
        account: null,
        uuid: null,
        loginMode: null,
        eos: null,
        requiredFields: null,
        currentHost: location.protocol + "//" + location.host,
        volume: 0,
        signalr: {
            simplewallet: {
                connection: null,
                listeners: []
            }
        },
        qrcodeIsValid: true,
        qrcodeTimer: null,
        _width: null,
        mobile: {
            nav: 'home'
        },
        control: {
            apiLock: false,
            currentNotification: null,
            notifications: [],
            notificationLock: false,
        }
    },
    created: function () {
        var self = this;
        this.initSignalR();
        qv.get(`/api/v1/lang/${this.lang}/info/volume`, {}).then(res => {
            if (res.code === 200) {
                self.volume = res.data;
            }
        });
        $(document).ready(function () {
            self._width = window.innerWidth;
        });
    },
    mounted: function () {
        var self = this;
        self.$nextTick(() => {
            window.addEventListener('resize', () => {
                if (self._width >= 768 && window.innerWidth < 768 || window.innerWidth >= 768 && self._width < 768) {
                    window.location.reload();
                }
                self._width = window.innerWidth;
            });
        });
    },
    watch: {
    },
    methods: {
        _getLoginRequestObj: function (uuid) {
            var _this = this;
            var loginObj = {
                "protocol": "SimpleWallet",
                "version": "1.0",
                "dappName": "Kyubey",
                "dappIcon": `${_this.currentHost}/img/KYUBEY_logo.png`,
                "action": "login",
                "uuID": uuid,
                "loginUrl": `${_this.currentHost}/api/v1/simplewallet/callback/login`,
                "expired": new Date().getTime() + (3 * 60 * 1000),
                "loginMemo": "kyubey login"
            };
            return loginObj;
        },
        generateLoginQRCode: function (idSelector, uuid) {
            $("#" + idSelector).empty();
            var loginObj = this._getLoginRequestObj(uuid);
            var qrcode = new QRCode(idSelector, {
                text: JSON.stringify(loginObj),
                width: 200,
                height: 200,
                colorDark: "#000000",
                colorLight: "#ffffff",
                correctLevel: QRCode.CorrectLevel.L
            });
        },
        getSimpleWalletUUID: function () {
            return this.uuid;
        },
        generateUUID: function () {
            var s = [];
            var hexDigits = "0123456789abcdef";
            for (var i = 0; i < 36; i++) {
                s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
            }
            s[14] = "4";
            s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);
            s[8] = s[13] = s[18] = s[23] = "-";

            var uuid = s.join("");
            return uuid;
        },
        initSignalR: function () {
            var self = this;
            const $t = this.$t.bind(this);
            self.signalr.simplewallet.connection = new signalR.HubConnectionBuilder()
                .configureLogging(signalR.LogLevel.Trace)
                .withUrl('/signalr/simplewallet', {})
                .withHubProtocol(new signalR.JsonHubProtocol())
                .build();

            // TODO: Receiving some signals for updating query view.
            self.signalr.simplewallet.connection.on('simpleWalletLoginSucceeded', (account) => {
                self.account = {
                    name: account
                };
                self.loginMode = 'Simple Wallet';
                $('#loginModal').modal('hide');
            });

            self.signalr.simplewallet.connection.on('simpleWalletExchangeSucceeded', () => {
                $('#exchangeModal').modal('hide');
                //todo notification.
            });

            self.signalr.simplewallet.connection.start().then(function () {
                self.uuid = self.generateUUID();
                return self.signalr.simplewallet.connection.invoke('bindUUID', self.uuid);
            });
        },
        login: function () {
            $('#loginModal').modal('show');
            this.refreshLoginQRCode();
        },
        refreshLoginQRCode: function () {
            var _this = this;
            this.generateLoginQRCode("loginQRCode", this.getSimpleWalletUUID());
            this.qrcodeIsValid = true;
            clearTimeout(_this.qrcodeTimer);
            _this.qrcodeTimer = setTimeout(function () {
                _this.qrcodeIsValid = false;
            }, 3 * 60 * 1000);
        },
        scatterLogin: function () {
            if (!('scatter' in window)) {
                showModal('Scatter插件没有找到', 'Scatter是一款EOS钱包，运行在Chrome浏览器中，请您确定已经安装Scatter插件. 参考：https://www.jianshu.com/p/a2e1e6204023');
            } else {
                var self = this;
                var network = {
                    blockchain: 'eos',
                    host: self.host,
                    port: 443,
                    protocol: 'https',
                    chainId: self.chainId
                };
                scatter.getIdentity({ accounts: [network] }).then(identity => {
                    self.account = identity.accounts.find(acc => acc.blockchain === 'eos');
                    self.loginMode = 'Scatter Addons';
                    self.eos = scatter.eos(network, Eos, {});
                    self.requiredFields = { accounts: [network] };
                });
            }
            $('#loginModal').modal('hide');
        },
        scatterLogout: function () {
            var self = this;
            if (self.loginMode && (self.loginMode === 'Scatter Addons' || self.loginMode === 'Scatter Desktop')) {
                scatter.forgetIdentity()
                    .then(() => {
                        self.account = null;
                        self.loginMode = null;
                    });
            } else {
                self.account = null;
                self.loginMode = null;
            }
        },
        switchAccount: function () {
            self = this;
            if (self.loginMode == null) return;
            if (self.loginMode === 'Simple Wallet') {
                account = null;
            } else {
                if (self.loginMode === 'Scatter Addons' || self.loginMode === 'Scatter Desktop') {
                    this.scatterLogout();
                }
            }
            this.login();
        },
        redirect: function (name, path, params, query) {
            if (name && !path)
                path = name;
            LazyRouting.RedirectTo(name, path, params, query);
        },
        setLang: function (param) {
            this.$i18n.locale = param;
        },
        marked: function (md) {
            return marked(md);
        },
        notification: function (level, title, detail, button) {
            var item = { level: level, title: title, detail: detail };
            if (level === 'important') {
                item.button = button;
            }
            this.control.notifications.push(item);
            if (this.control.currentNotification && this.control.currentNotification.level === 'pending') {
                this.control.notificationLock = false;
            }
            this._showNotification(level === 'important' ? true : false);
        },
        clickNotification: function () {
            this._releaseNotification();
        },
        _showNotification: function (manualRelease) {
            var self = this;
            if (!this.control.notificationLock && this.control.notifications.length) {
                this.control.notificationLock = true;
                var notification = this.control.notifications[0];
                this.control.notifications = this.control.notifications.slice(1);
                this.control.currentNotification = notification;
                if (!manualRelease) {
                    setTimeout(function () {
                        self._releaseNotification();
                    }, 5000);
                }
            }
        },
        _releaseNotification: function () {
            var self = this;
            self.control.currentNotification = null;
            setTimeout(function () {
                self.control.notificationLock = false;
                if (self.control.notifications.length) {
                    self._showNotification();
                }
            }, 250);
        },
    },
    computed: {
        isSignedIn: function () {
            return !(app.account == null || app.account.name == null);
        },
        lang: function () {
            return this.$i18n.locale;
        }
    },
    i18n: i18n
});
