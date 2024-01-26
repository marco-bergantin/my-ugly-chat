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
    var chatMessages = document.getElementById("chat-messages");
    if (!chatMessages) {
        var chatBox = document.getElementById("chatBox");
        chatMessages = document.createElement("div");
        chatMessages.id = "chat-messages";
        chatBox.appendChild(chatMessages);
    }

    var messageContainer = document.createElement("div");
    messageContainer.className = "message-container";

    var messageElement = document.createElement("div");
    messageElement.textContent = messageViewModel.content;
    messageElement.className = getMessageClassName(messageViewModel);
    messageContainer.appendChild(messageElement);

    var timestampElement = document.createElement("div");
    timestampElement.dataset.ticks = messageViewModel.timestamp;
    timestampElement.textContent = (new Date(messageViewModel.timestamp)).toLocaleString();
    timestampElement.className = "timestamp"
    messageElement.appendChild(timestampElement);

    if (messageViewModel.fromArchive) { // add to top
        var firstMessage = chatMessages.firstChild;
        chatMessages.insertBefore(messageContainer, firstMessage);
    }
    else { // append to bottom
        chatMessages.appendChild(messageContainer);
    }
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
        var chatBox = document.getElementById("chatBox");

        // add hidden input to flag that there's no more data
        var beginning = document.createElement("input");
        beginning.type = "hidden";
        beginning.id = "begin";
        chatBox.appendChild(beginning);
    }
});