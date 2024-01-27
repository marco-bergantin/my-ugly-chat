"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    sendMessage(event);
});

document.getElementById("messageInput").addEventListener("keydown", (event) => {
    if (event.isComposing || event.key !== "Enter") {
        return;
    }
    sendMessage(event);
});

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

connection.on("ReceiveMessage", function (messageViewModel) {
    var selectedContact = document.getElementById("selected-contact").value;
    if (selectedContact == messageViewModel.chatId) {
        addMessage(messageViewModel);
    }
});

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
        var firstMessage = chatMessages.firstChild;
        if (firstMessage && !firstMessage.firstChild.id.startsWith('date')) {
            var firstMessageTimestamp = firstMessage.firstChild.lastChild.dataset.ticks;
            var datePreviousMessage = new Date(Number(firstMessageTimestamp));
            if (datePreviousMessage.getDate() !== dateNewMessage.getDate()) {
                firstMessage = addDateSystemNote(datePreviousMessage, false);
            }
        }

        chatMessages.insertBefore(messageContainer, firstMessage);
    }
    else { // append to bottom
        var lastMessage = chatMessages.lastChild;
        if (lastMessage && !lastMessage.firstChild.id.startsWith('date')) {
            var lastMessageTimestamp = lastMessage.firstChild.lastChild.dataset.ticks;
            var datePreviousMessage = new Date(Number(lastMessageTimestamp));
            if (datePreviousMessage.getDate() !== dateNewMessage.getDate()) {
                addDateSystemNote(dateNewMessage, true);
            }
        }

        chatMessages.appendChild(messageContainer);
    }
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

connection.on("RefreshRecentContacts", function (recentContactsViewModel) {
    var recentContactsElement = document.getElementById("recent-contacts");
    recentContactsElement.innerHTML = "";
    for (const contact of recentContactsViewModel.contacts) {
        var contactElement = document.createElement("div");
        contactElement.textContent = contact.displayName;
        contactElement.className = "contact";
        contactElement.id = contact.chatId;
        contactElement.dataset.contactid = contact.userId;
        contactElement.onclick = function () {
            var recentContactsElement = document.getElementById("recent-contacts");
            for (const child of recentContactsElement.childNodes) {
                child.className = "contact";
            }
            this.className = "selected-contact";
            selectedContactChanged(this);
        };
        recentContactsElement.appendChild(contactElement);
    }   
});

function selectedContactChanged(contactElement) {
    var chatId = contactElement.id;
    var userId = contactElement.dataset.contactid;
    var chatBox = document.getElementById("chatBox");
    chatBox.innerHTML = "";
    if (chatId) {
        connection.invoke("GetRecentChat", chatId).catch(function (err) {
            return console.error(err.toString());
        });

        var selectedContactElement = document.getElementById("selected-contact");
        if (!selectedContactElement) {
            var selectedContactElement = document.createElement("input");
            selectedContactElement.type = "hidden";
            selectedContactElement.id = "selected-contact";
            chatBox.appendChild(selectedContactElement);
        }

        selectedContactElement.value = chatId;
        selectedContactElement.dataset.contactid = userId;

        lastScrollPosition = 0;
    }
}  

var lastScrollPosition = 0;
document.getElementById("chatBox").addEventListener('scroll', () => {
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
        var chatMessages = document.getElementById("chat-messages");
        var firstMessage = chatMessages.firstChild;
        var messageElement = firstMessage.firstChild;
        var timestampElement = messageElement.lastChild;

        var timestamp = parseInt(timestampElement.dataset.ticks);
        var chatId = document.getElementById("selected-contact").value;

        window.isLoading = true;

        connection.invoke("GetMoreMessages", chatId, timestamp).catch(function (err) {
            return console.error(err.toString());
        });
    }
}, {
    passive: true
});

connection.on("FinishedLoading", function (paramsObject) {
    window.isLoading = false;

    if (!paramsObject.moreData) {
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
});