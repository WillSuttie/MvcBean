document.addEventListener('DOMContentLoaded', function () {
    const useAverageColourButton = document.getElementById('useAverageColourButton');
    const colourHexInput = document.getElementById('colourHexInput');
    const averageColourDisplay = document.getElementById('averageColourDisplay');

    if (useAverageColourButton) {
        // Set initial display colour
        const initialAverageColour = useAverageColourButton.dataset.averageColour || '#FFFFFF';
        averageColourDisplay.style.backgroundColor = initialAverageColour;

        useAverageColourButton.addEventListener('click', function () {
            // Get the average colour from the button's data attribute
            const averageColour = useAverageColourButton.dataset.averageColour || '#FFFFFF';

            // Update the input field and the display box
            colourHexInput.value = averageColour;
            averageColourDisplay.style.backgroundColor = averageColour;
        });
    }
});

document.addEventListener("DOMContentLoaded", function () {
    const inputField = document.getElementById("saleDateInput");
    const today = new Date();

    // Format the date as dd/MM/yyyy
    const formattedDate = today.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });

    // Set the placeholder
    inputField.placeholder = formattedDate;
});

document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    
    // Run validation on input changes
    form.addEventListener("input", toggleSubmitButton);

    // Initial validation check
    toggleSubmitButton();
});