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
});
