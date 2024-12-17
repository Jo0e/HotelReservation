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

        // Scroll to the new row for visibility
        newRow.scrollIntoView();
    }
});

