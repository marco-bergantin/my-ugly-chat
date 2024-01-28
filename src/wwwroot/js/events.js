"use strict";

document.getElementById("sendButton").addEventListener("click", function (event) {
    sendMessage(event);
});

document.getElementById("messageInput").addEventListener("keydown", (event) => {
    if (event.isComposing || event.key !== "Enter") {
        return;
    }
    sendMessage(event);
});

document.getElementById("chatBox").addEventListener('scroll', () => {
    handleScrollEvent();
}, {
    passive: true
});