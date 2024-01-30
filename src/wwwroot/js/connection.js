"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    connection.invoke("GetRecentContacts");
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveMessage", function (messageViewModel) {
    var selectedContact = document.getElementById("selected-contact").value;
    if (selectedContact == messageViewModel.chatId) {
        addMessage(messageViewModel);
    }
});

connection.on("RefreshRecentContacts", function (recentContactsViewModel) {
    var recentContactsElement = document.getElementById("recent-contacts");
    recentContactsElement.innerHTML = "";
    for (const contact of recentContactsViewModel.contacts) {
        addRecentContact(recentContactsElement, contact);
    }
});

connection.on("FinishedLoading", function (paramsObject) {
    window.isLoading = false;

    if (!paramsObject.moreData) {
        setChatBeginning();
    }
});