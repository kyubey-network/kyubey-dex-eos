LazyRouting.SetRoute({
    '/home': null,
    '/wallet': null,
    '/market': null,
    '/exchange/:id': { id: '[A-Z]{1,10}' },
    '/token/all': null,
    '/token/:id': { id: '[A-Z]{1,10}' },
    '/home/contact': null,
    '/home/news': null
});

LazyRouting.SetMirror({
    '/': '/home',
});