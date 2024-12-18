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
        const newRow = tbody.insertRow(0);

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


document.addEventListener('DOMContentLoaded', function () {
    const currentHotelId = document.getElementById("hotel-id").value;

    connection.on("NotifyAdminReservation", function (reservationInfo) {
        console.log("Reservation notification: " + reservationInfo);

        // Assuming contactUsInfo is JSON
        const reserve = JSON.parse(reservationInfo);


        // Update the table if it exists on the page
        if (reserve.Hotel.Id === parseInt(currentHotelId)) {

            const table = document.getElementById("table-reservation");
            if (table) {
                const tbody = table.getElementsByTagName('tbody')[0];
                const newRow = tbody.insertRow(0);
                newRow.classList.add('text-center');

                // Create cells for the new row
                const cell1 = newRow.insertCell(0);
                const cell2 = newRow.insertCell(1);
                const cell3 = newRow.insertCell(2);
                const cell4 = newRow.insertCell(3);
                const cell5 = newRow.insertCell(4);
                const cell6 = newRow.insertCell(5);
                const cell7 = newRow.insertCell(6);
                const cell8 = newRow.insertCell(7);
                const cell9 = newRow.insertCell(8);
                const cell10 = newRow.insertCell(9);

                // Populate cells with reservation data
                cell1.innerHTML = reserve.RoomCount;
                cell2.innerHTML = reserve.NAdult;
                cell3.innerHTML = reserve.NChildren;
                cell4.innerHTML = new Date(reserve.CheckInDate).toLocaleDateString();
                cell5.innerHTML = new Date(reserve.CheckOutDate).toLocaleDateString();
                cell6.innerHTML = reserve.TotalPrice.toFixed(2);
                cell7.innerHTML = reserve.User.Email;
                cell8.innerHTML = reserve.Hotel.Name;
                cell9.innerHTML = reserve.Status;

                // Add action buttons
                cell10.innerHTML = `
            <button type="button" class="btn btn-danger btn-sm" data-bs-toggle="modal" data-bs-target="#deleteModal" data-reservation-id="${reserve.Id}">
                Cancel Reservation
            </button>
        `;

                newRow.scrollIntoView();
                console.log("New row added to the table.");
            }
            else {
                console.error("Table element not found.");
            }
        }
        else {
            console.error(`Hotel ID mismatch. Received: ${reserve.Hotel.Id}, Current: ${currentHotelId}`);
        }
    });
});