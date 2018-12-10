LazyRouting.SetRoute({
    '/home': null,
    '/exchange/:id': { id: '[A-Z]{1,10}' },
    '/token/all': null,
    '/token/:id': { id: '[A-Z]{1,10}' },
    '/home/contact': null,
});

LazyRouting.SetMirror({
    '/': '/home',
    '/token': '/currency/all'
});