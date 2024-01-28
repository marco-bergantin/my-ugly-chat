"use strict";

var lastScrollPosition = 0;
function resetScrollPosition() {
    lastScrollPosition = 0;
}

function handleScrollEvent() {
    if (window.isLoading) { // already loading data, do nothing
        return;
    }

    var chatBox = document.getElementById("chatBox");
    const {
        scrollTop, // is a negative number
        scrollHeight,
        clientHeight
    } = chatBox;

    var isScrollingUp = lastScrollPosition > scrollTop;
    lastScrollPosition = scrollTop;
    if (!isScrollingUp) { // do nothing
        return;
    }

    var beginning = document.getElementById("begin");
    if (beginning) { // already received all data
        return;
    }

    var scrollValue = scrollTop + scrollHeight; // is equal to clientHeight when scrolled all the way to the top
    if (scrollValue <= clientHeight * 1.005) { // if at the top (scrollValue gets smaller as you scroll higher up)
        var timestamp = getFirstDisplayedMessageTimestamp();
        var chatId = document.getElementById("selected-contact").value;

        window.isLoading = true;

        connection.invoke("GetMoreMessages", chatId, timestamp).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function getFirstDisplayedMessageTimestamp() {
    var chatMessages = document.getElementById("chat-messages");
    var firstMessage = chatMessages.firstChild;
    var messageElement = firstMessage.firstChild;
    var timestampElement = messageElement.lastChild;

    return parseInt(timestampElement.dataset.ticks);
}

function setChatBeginning() {
    var chatMessages = getOrAddChatMessages();

    var messageContainer = document.createElement("div");
    messageContainer.className = "message-container";

    // flag that there's no more data
    var beginning = document.createElement("div");
    beginning.id = "begin";
    beginning.className = "system-msg";
    beginning.textContent = "CHAT BEGIN";

    messageContainer.appendChild(beginning);
    chatMessages.insertBefore(messageContainer, chatMessages.firstChild);
}