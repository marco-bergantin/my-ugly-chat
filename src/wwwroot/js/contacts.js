"use strict";

function addRecentContact(recentContactsElement, contact) {
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

function selectedContactChanged(contactElement) {
    var chatId = contactElement.id;
    var userId = contactElement.dataset.contactid;
    var chatBox = document.getElementById("chatBox");
    chatBox.innerHTML = "";
    if (chatId) {
        connection.invoke("GetRecentChat", chatId).catch(function (err) {
            return console.error(err.toString());
        });

        setSelectedContact(chatBox, chatId, userId);
        resetScrollPosition();
    }
}

function setSelectedContact(chatBox, chatId, userId) {
    var selectedContactElement = document.getElementById("selected-contact");
    if (!selectedContactElement) {
        var selectedContactElement = document.createElement("input");
        selectedContactElement.type = "hidden";
        selectedContactElement.id = "selected-contact";
        chatBox.appendChild(selectedContactElement);
    }

    selectedContactElement.value = chatId;
    selectedContactElement.dataset.contactid = userId;
}