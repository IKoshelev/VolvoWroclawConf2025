importScripts(
    "https://www.gstatic.com/firebasejs/11.5.0/firebase-app-compat.js"
);
importScripts(
    "https://www.gstatic.com/firebasejs/11.5.0/firebase-messaging-compat.js"
);

const config = {
    apiKey: "AIzaSyCMpPACei7tM6aG7D-V79IFOHNHwch59I4",
    authDomain: "volvowroclawconf2025.firebaseapp.com",
    projectId: "volvowroclawconf2025",
    storageBucket: "volvowroclawconf2025.firebasestorage.app",
    messagingSenderId: "1054734295033",
    appId: "1:1054734295033:web:eedbd8b1b0db456cae2728"
}
firebase.initializeApp(config);
firebase.messaging();