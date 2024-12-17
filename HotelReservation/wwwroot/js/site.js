//import * as signalR from "../lib/signalr/dist/browser/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// Start the connection and handle errors
async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error(err);
        setTimeout(start, 5000); // Retry after 5 seconds
    }
}

// Start the SignalR connection
start();


// Define a function to receive admin notifications from the hub
connection.on("AdminNotification", function (contactUsInfo) {
    console.log("Admin notification: " + contactUsInfo);

    // Assuming contactUsInfo is JSON
    const contact = JSON.parse(contactUsInfo);

    // Display Toastr notification
    toastr.info(`New Contact Us request from ${contact.Name}: ${contact.RequestType}`);

    // Update the table if it exists on the page
    const table = document.getElementById("contactRequestsTable");
    if (table) {
        const tbody = table.getElementsByTagName('tbody')[0];
        const newRow = tbody.insertRow();

        // Create cells for the new row
        const cell1 = newRow.insertCell(0);
        const cell2 = newRow.insertCell(1);
        const cell3 = newRow.insertCell(2);
        const cell4 = newRow.insertCell(3);

        // Populate cells with contact information
        cell1.innerHTML = contact.Name;
        cell2.innerHTML = contact.RequestType;
        cell3.innerHTML = contact.UserRequestString.length > 20 ? contact.UserRequestString.substring(0, 18) + "..." : contact.UserRequestString;

        // Create actions cell with buttons
        cell4.className = "text-center";
        cell4.innerHTML = `
        <div class="btn-group">
            <a href="/Admin/ContactUs/Details?reqId=${contact.Id}" class="btn btn-info btn-sm" title="View Details">
                <i class="bi bi-eye"></i> Details
            </a>
            <button type="button" class="btn btn-danger btn-sm delete-contact" data-bs-toggle="modal" data-bs-target="#deleteModal" data-id="${contact.Id}" title="Delete">
                <i class="bi bi-trash"></i> Delete
            </button>
            <a href="/Admin/ContactUs/ReadMessage?messageId=${contact.Id}" class="btn btn-outline-primary btn-sm read-status" title="Mark as Read">
                <i class="bi bi-check2-all"></i>
            </a>
        </div>
    `;

        // Scroll to the new row for visibility
        newRow.scrollIntoView();
    }
    // Update the unread contact counter dynamically
    unreadContactCount += 1; // Increment the count
    updateUnreadContactCounter(unreadContactCount);
});

// Import SignalR if necessary
// import * as signalR from "../lib/signalr/dist/browser/signalr";

const hotelConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hotelHub")
    .build();

// Start the connection
async function startHotelConnection() {
    try {
        await hotelConnection.start();
        console.log("Connected to HotelHub.");
    } catch (err) {
        console.error("Error connecting to HotelHub:", err);
        setTimeout(startHotelConnection, 5000); // Retry connection
    }
}

startHotelConnection();

// Listen for the "NewHotelAdded" event
hotelConnection.on("NewHotelAdded", function (hotelJson) {
    console.log("New hotel notification received:", hotelJson);

    const hotel = JSON.parse(hotelJson);

    // Display a Toastr notification
    toastr.success(`A new hotel "${hotel.Name}" has been added in ${hotel.City}.`);

    // Update the hotels list if it exists on the page
    const hotelsTable = document.getElementById("hotelsTable");
    if (hotelsTable) {
        const tbody = hotelsTable.getElementsByTagName("tbody")[0];
        const newRow = tbody.insertRow();

        // Add table cells
        const cell1 = newRow.insertCell(0); // ID
        const cell2 = newRow.insertCell(1); // Name
        const cell3 = newRow.insertCell(2); // City
        const cell4 = newRow.insertCell(3); // Stars

        // Populate cells
        cell1.textContent = hotel.Id;
        cell2.textContent = hotel.Name;
        cell3.textContent = hotel.City;
        cell4.textContent = hotel.Stars;

   
        newRow.scrollIntoView();
    }



function updateUnreadContactCounter(newCount) {
    const navbarCounter = document.getElementById('contactCount');
    if (navbarCounter) {
        console.log("navbarCounter Count:", navbarCounter);
        navbarCounter.textContent = newCount;
    }

    const offcanvasCounter = document.querySelector('.badge.rounded-pill.bg-danger');
    if (offcanvasCounter) {
        console.log("offcanvasCounter Count:", offcanvasCounter);
        offcanvasCounter.textContent = "New!";
    }

    console.log("Updated unread count:", newCount);
}



///////////////////
// Define a function to receive Customer notifications from the hub
connection.on("CustomerNotification", function (messageInfo, messageCount) {
    console.log("Customer notification: " + messageInfo);



    // Update the message counter
    const messageCounter = document.getElementById('message-counter');
    try {
        messageCounter.textContent = messageCount;
        console.log("Message count: " + messageCount);
    } catch (error) {
        console.error("Error updating message counter:", error);
    }

    // Assuming contactUsInfo is JSON
    const message = JSON.parse(messageInfo);

    // Display Toastr notification
    toastr.info(`New Message request ${message.Title}`);

    // Find the chat list container
    const chatList = document.querySelector('.chat-list');

    // Create a new chat item element
    const newChatItem = document.createElement('div');
    newChatItem.classList.add('chat-item', 'align-items-start', 'shadow-sm', 'mb-3');
    newChatItem.innerHTML = `
        <div class="avatar rounded-circle me-3">
            <span class="initials">${message.Title.charAt(0).toUpperCase()}</span>
        </div>
        <div class="chat-content flex-grow-1 mt-2">
            <h5 class="chat-title mb-1 text-truncate fw-bold text-black">${message.Title}</h5>
            <p class="chat-message mb-2 text-dark">${message.MessageString}</p>
            <p class="chat-description mb-0 text-muted"><small>${message.Description}</small></p>
        </div>
        <div class="chat-actions text-end">
            <small class="text-muted">${new Date(message.MessageDateTime).toLocaleString()}</small>
            <div class="justify-content-end">
                <a href="/Customer/Inbox/ReadMessage?messageId=${message.Id}" class="btn btn-outline-primary btn-sm ms-1">
                    <i class="${message.IsReadied ? 'bi bi-check-circle-fill text-success' : 'bi bi-check2-all'}"></i>
                </a>
                <a href="/Customer/Inbox/Delete?id=${message.Id}" class="btn btn-outline-danger btn-sm ms-2">
                    <i class="bi bi-trash"></i>
                </a>
            </div>
        </div>
    `;



    // Append the new chat item to the chat list
    chatList.prepend(newChatItem);

    // Scroll to the new row for visibility
    newChatItem.scrollIntoView();

});

