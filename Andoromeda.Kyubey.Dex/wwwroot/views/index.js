app = new Vue({
    router: router,
    data: {
        chainId: 'aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906',
        host: 'nodes.get-scatter.com',
        account: null,
        loginMode: null,
        eos: null,
        requiredFields: null,
        volume: 0,
        lang: 'en',
        signalr: {
            simplewallet: {
                connection: null,
                listeners: []
            }
        },
    },
    created: function () {
        this.initSignalR();
        qv.get(`/api/v1/lang/${this.lang}/volume`, {}).then(res => {
            if (res.code === 200) {
                this.volume = res.data;
            }
        });
    },
    watch: {
    },
    methods: {
        getSimpleWalletUUID: function () {
            return self.signalr.simplewallet.connection.id;
        },
        initSignalR: function () {
            var self = this;
            self.signalr.simplewallet.connection = new signalR.HubConnectionBuilder()
                .configureLogging(signalR.LogLevel.Trace)
                .withUrl('/signalr/simplewallet', {})
                .withHubProtocol(new signalR.JsonHubProtocol())
                .build();

            // TODO: Receiving some signals for updating query view.

            self.signalr.simplewallet.connection.on('simpleWalletLoginSucceeded', (account) => {
                self.account = account;
                self.loginMode = 'Simple Wallet';
            });

            this.signalr.simplewallet.connection.start();

        },
        login: function () {
            $('#loginModal').modal('show');
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
        redirect: function (name, path, params, query) {
            if (name && !path)
                path = name;
            LazyRouting.RedirectTo(name, path, params, query);
        },
        setLang: function (param) { 
            this.$i18n.locale = param;
        }
    },
    computed: {
        isSignedIn: function () {
            return !!this.account;
        }
    },
    i18n: i18n
});