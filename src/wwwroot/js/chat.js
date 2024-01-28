"use strict";

function sendMessage(event) {
    var selectedContactElement = document.getElementById("selected-contact");
    var selectedContact = selectedContactElement.value;
    var userId = selectedContactElement.dataset.contactid;
    var messageBox = document.getElementById("messageInput");
    var message = messageBox.value;
    if (selectedContact && message && message.toString().length > 0) {
        connection.invoke("SendMessage", selectedContact, userId, message).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();

        addMessage({
            user: "you",
            content: message,
            direction: 0,
            timestamp: Date.now()
        });

        messageBox.value = "";
    }
}

function addMessage(messageViewModel) {
    var chatMessages = getOrAddChatMessages();
    var messageContainer = createMessageContainer(messageViewModel);
    addToMessageList(chatMessages, messageContainer, messageViewModel);
}

function createMessageContainer(messageViewModel) {
    var messageContainer = document.createElement("div");
    messageContainer.className = "message-container";

    var messageElement = document.createElement("div");
    messageElement.textContent = messageViewModel.content;
    messageElement.className = getMessageClassName(messageViewModel);
    messageContainer.appendChild(messageElement);

    var timestampElement = document.createElement("div");
    var dateNewMessage = new Date(messageViewModel.timestamp);
    timestampElement.dataset.ticks = messageViewModel.timestamp;
    timestampElement.textContent = dateNewMessage.toLocaleTimeString();
    timestampElement.className = "timestamp"
    messageElement.appendChild(timestampElement);

    return messageContainer;
}

function addToMessageList(chatMessages, messageContainer, messageViewModel) {
    var dateNewMessage = new Date(messageViewModel.timestamp);
    if (messageViewModel.fromArchive) { // add to top
        var firstMessage = handleMessageAndTimestamp(chatMessages.firstChild, dateNewMessage, false);
        chatMessages.insertBefore(messageContainer, firstMessage);
    }
    else { // append to bottom
        handleMessageAndTimestamp(chatMessages.lastChild, dateNewMessage, true);
        chatMessages.appendChild(messageContainer);
    }
}

function handleMessageAndTimestamp(siblingMessage, dateNewMessage, append) {
    if (siblingMessage && !siblingMessage.firstChild.id.startsWith('date')) {
        var siblingTimestamp = siblingMessage.firstChild.lastChild.dataset.ticks;
        var siblingDate = new Date(Number(siblingTimestamp));
        if (siblingDate.getDate() !== dateNewMessage.getDate()) {
            return addDateSystemNote(siblingDate, append);
        }
    }

    return siblingMessage;
}

function getOrAddChatMessages() {
    var chatMessages = document.getElementById("chat-messages");
    if (!chatMessages) {
        var chatBox = document.getElementById("chatBox");
        chatMessages = document.createElement("div");
        chatMessages.id = "chat-messages";
        chatBox.appendChild(chatMessages);
    }
    return chatMessages;
}

function addDateSystemNote(date, append) {
    var chatMessages = document.getElementById("chat-messages");

    var messageContainer = document.createElement("div");
    messageContainer.className = "message-container";

    var dateElement = document.createElement("div");
    dateElement.id = "date" + date.toLocaleDateString();
    dateElement.className = "system-msg";
    dateElement.textContent = date.toLocaleDateString();

    messageContainer.appendChild(dateElement);

    if (append) {
        chatMessages.appendChild(messageContainer);
    }
    else { // prepend
        chatMessages.insertBefore(messageContainer, chatMessages.firstChild);
    }

    return messageContainer;
}

function getMessageClassName(messageViewModel) {
    return messageViewModel.direction === 0
        ? "sent"
        : "received";
}