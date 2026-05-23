document.getElementById('addReviewForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const submitBtn = this.querySelector('[type="submit"]');
    if (submitBtn.disabled) return;
    submitBtn.disabled = true;

    reviewTextInput.value = reviewBlock.innerText.trim();
    shortReviewInput.value = shortReviewBlock.innerText.trim();
    document.getElementById('itemNameInput').value = document.getElementById('itemNameInput').value.trim();
    document.getElementById('itemBrandInput').value = document.getElementById('itemBrandInput').value.trim();

    const dataTransfer = new DataTransfer();
    filesArray.forEach(file => dataTransfer.items.add(file));
    reviewPicturesInput.files = dataTransfer.files;

    var formData = new FormData(document.getElementById('addReviewForm'));

    try {
        const response = await fetch('/reviews/add',
            {
                method: 'POST',
                body: formData
            });

        if (response.redirected) {
            window.location.href = response.url;
        }
        else if (response.status === 400) {
            const result = await response.json();

            errorMessageContainer.textContent = '';

            if (result.errors && result.errors.length > 0) {
                result.errors.forEach(error => {
                    const errorItem = document.createElement('div');
                    errorItem.textContent = error;
                    errorMessageContainer.appendChild(errorItem);
                });
                errorMessageContainer.style.display = 'block';
            }
        }
        else {
            errorMessageContainer.textContent = 'Что-то пошло не так';
            errorMessageContainer.style.display = 'block';
        }
    } catch (e) {
        errorMessageContainer.textContent = 'Что-то пошло не так';
        errorMessageContainer.style.display = 'block';
    } finally {
        submitBtn.disabled = false;
    }
});