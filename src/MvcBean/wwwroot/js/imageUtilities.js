document.addEventListener('DOMContentLoaded', function () {
    const deleteImageButton = document.getElementById('deleteImageButton');

    if (deleteImageButton) {
        deleteImageButton.addEventListener('click', function () {
            if (confirm('Are you sure you want to delete this image?')) {
                // Fetch the bean ID from the button's data attribute
                const beanId = deleteImageButton.getAttribute('data-bean-id');

                fetch(`/Beans/RemoveImage/${beanId}`, { method: 'POST' })
                    .then(response => {
                        if (response.ok) {
                            location.reload();
                        } else {
                            alert('Failed to delete the image.');
                        }
                    })
                    .catch(error => {
                        console.error('Error:', error);
                        alert('An error occurred while deleting the image.');
                    });
            }
        });
    }
});


document.addEventListener("DOMContentLoaded", () => {
    const validExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    const fileInput = document.querySelector('input[name="image"]');
    const errorContainer = document.querySelector("#imageError");

    fileInput.addEventListener("change", () => {
        const file = fileInput.files[0];
        if (file) {
            const fileExtension = file.name.split(".").pop().toLowerCase();
            if (!validExtensions.includes(`.${fileExtension}`)) {
                errorContainer.textContent = "Invalid file type. Allowed types are: " + validExtensions.join(", ");
                fileInput.value = ""; // Clear the invalid file
            } else {
                errorContainer.textContent = ""; // Clear the error
            }
        }
    });
});


// Function to show image preview
function showPreview(event) {
    const fileInput = event.target;
    const previewContainer = document.getElementById('imagePreviewContainer');
    const previewImage = document.getElementById('imagePreview');

    // Ensure a file is selected
    if (fileInput.files && fileInput.files[0]) {
        const file = fileInput.files[0];

        // Validate file type (if not already done elsewhere)
        const validTypes = ['image/jpeg', 'image/png', 'image/gif'];
        if (!validTypes.includes(file.type)) {
            alert('Invalid file type. Please select a JPG, PNG, or GIF image.');
            fileInput.value = ''; // Clear the input
            previewContainer.style.display = 'none';
            return;
        }

        const reader = new FileReader();

        // When the file is loaded, set the image source to the preview
        reader.onload = function (e) {
            previewImage.src = e.target.result; // Set preview image source
            previewContainer.style.display = 'block'; // Show the preview container
        };

        reader.readAsDataURL(file); // Read the file as a data URL
    } else {
        // Hide the preview if no file is selected
        previewContainer.style.display = 'none';
    }
}

// Attach the event listener to the file input (if dynamically created, adjust accordingly)
document.addEventListener('DOMContentLoaded', function () {
    const imageInput = document.getElementById('imageInput');
    if (imageInput) {
        imageInput.addEventListener('change', showPreview);
    }
});
