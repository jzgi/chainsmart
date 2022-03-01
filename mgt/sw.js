// Files to cache
const cacheName = 'skyiah-v1';

// Installing Service Worker
self.addEventListener('install', (e) => {
    console.log('[Service Worker] Install');
    e.waitUntil((async () => {
        const cache = await caches.open(cacheName);
        console.log('[Service Worker] Caching all: app shell and content');
        await cache.addAll([
            // '/app.js',
            // '/manifest.json',
            // '/uikit.min.js'
        ]);
    })());
});

