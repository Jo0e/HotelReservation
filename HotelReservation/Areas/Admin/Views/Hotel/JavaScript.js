document.addEventListener('DOMContentLoaded', function () {
    // Example: Add a tooltip on hover for buttons
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mouseover', function () {
            button.setAttribute('title', 'Click to perform an action');
        });
        button.addEventListener('mouseout', function () {
            button.removeAttribute('title');
        });
    });

    // Example: Confirm Delete action
    const deleteButtons = document.querySelectorAll('a[asp-action="Delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function (event) {
            if (!confirm('Are you sure you want to delete this item?')) {
                event.preventDefault();
            }
        });
    });

    // Example: Form Validation (just a sample for inputs)
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            const inputs = form.querySelectorAll('input, select, textarea');
            let isValid = true;

            inputs.forEach(input => {
                if (input.required && !input.value) {
                    input.classList.add('is-invalid');
                    isValid = false;
                } else {
                    input.classList.remove('is-invalid');
                }
            });

            if (!isValid) {
                event.preventDefault();
            }
        });
    });
});
