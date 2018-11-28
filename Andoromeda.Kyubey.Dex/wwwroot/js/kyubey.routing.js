LazyRouting.SetRoute({
    '/home': null,
    '/token/all': null,
    '/token/:id': { id: '[A-Z]{1,10}' }
});

LazyRouting.SetMirror({
    '/': '/home',
    '/token': '/currency/all'
});