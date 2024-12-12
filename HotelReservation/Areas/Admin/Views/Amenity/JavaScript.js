document.querySelectorAll(".table-hover tbody tr").forEach((row) => {
    row.addEventListener("click", () => {
        document.querySelectorAll(".table-hover tbody tr").forEach((r) => r.classList.remove("table-active"));
        row.classList.add("table-active");
    });
});
document.querySelectorAll("form").forEach((form) => {
    form.addEventListener("submit", () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
    });
});
