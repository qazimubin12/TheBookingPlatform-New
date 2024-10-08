﻿// This is a public sample test API key.
// Don’t submit any personally identifiable information in requests made with this key.
// Sign in to see your own test API key embedded in code samples.
const stripe = Stripe("pk_test_51IIfsiAqnKymwprXZUv9eAbxnFHJvuvhBvuFzQx2zSjpUXiGBGeCtgnqb4YMZw71jMhC6MpT7w46dBl60sf0Zz2O00wZlTiKXx");

initialize();

// Create a Checkout Session as soon as the page loads
async function initialize() {
    const response = await fetch("/create-checkout-session", {
        method: "POST",
    });

    const { clientSecret } = await response.json();

    const checkout = await stripe.initEmbeddedCheckout({
        clientSecret,
    });

    // Mount Checkout
    checkout.mount('#checkout');
}