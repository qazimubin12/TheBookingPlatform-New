self.addEventListener('push', function (event) {
    const data = event.data.json(); // Parse the push data
    self.registration.showNotification(data.title, {
        body: data.body,
        icon: '/Content/TBPContent/v2color.png', // Optional: Add your app's icon
        tag: data.tag, // Optional: Group similar notifications
    });
});




// Handle notification click events
self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    if (event.notification.data) {
        event.waitUntil(clients.openWindow(event.notification.data));
    }
});
