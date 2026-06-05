document.getElementById('selectReviewPicturesBtn').addEventListener('click', () => {
    reviewPicturesInput.click();
});

reviewPicturesInput.addEventListener('change', function (event) {
    const files = Array.from(event.target.files);
    if (!validatePicturesSize(files)) {
        errorMessageContainer.textContent = `Размер изображений не должен превышать ${picturesMaxSize} МБ`;
        errorMessageContainer.style.display = 'block';
        return;
    }
    errorMessageContainer.style.display = 'none';

    files.forEach(file => {
        if (!file.type.startsWith('image/')) {
            this.value = '';
            errorMessageContainer.textContent = 'Неверный формат изображения';
            errorMessageContainer.style.display = 'block';
            return;
        }

        const fileName = file.name.toLowerCase();
        if (!pictureExtensions.some(x => fileName.endsWith(x))) {
            this.value = '';
            errorMessageContainer.textContent = 'Неверный формат изображения';
            errorMessageContainer.style.display = 'block';
            return;
        }
    });

    filesArray = filesArray.concat(files);
    renderPreviews();
});

function removeImage(index) {
    filesArray.splice(index, 1);
    renderPreviews();
}

function renderPreviews() {
    reviewPicturesPreviewBlock.innerHTML = '';
    filesArray.forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = function (e) {
            const wrapper = document.createElement('div');
            wrapper.style.position = 'relative';
            wrapper.style.display = ' inline-block';
            wrapper.style.margin = '5px';

            const img = document.createElement('img');
            img.src = e.target.result;
            img.style.maxWidth = '200px';
            img.style.maxHeight = '200px';
            img.style.objectFit = 'contain';

            const closeBtn = document.createElement('button');
            closeBtn.type = 'button';
            closeBtn.innerHTML = '&times;';
            closeBtn.style.position = 'absolute';
            closeBtn.style.fontSize = '25px';
            closeBtn.style.top = '0';
            closeBtn.style.right = '0';
            closeBtn.style.background = 'transparent';
            closeBtn.style.color = 'red';
            closeBtn.style.border = 'none';
            closeBtn.style.cursor = 'pointer';

            closeBtn.addEventListener('click', () => {
                removeImage(index);
            });

            wrapper.appendChild(img);
            wrapper.appendChild(closeBtn);
            reviewPicturesPreviewBlock.appendChild(wrapper);
        };
        reader.readAsDataURL(file);
    });
}

function validatePicturesSize(filesToAdd) {
    let reviewPicturesSizeInBytes = 0;
    filesArray.forEach(file => {
        reviewPicturesSizeInBytes += file.size;
    });

    let itemPictureSizeInBytes = 0;
    if (itemPictureInput && itemPictureInput.files && itemPictureInput.files[0])
        itemPictureSizeInBytes = itemPictureInput.files[0].size;

    let newPicturesSizeInBytes = 0;
    if (Array.isArray(filesToAdd)) {
        filesToAdd.forEach(file => {
            newPicturesSizeInBytes += file.size;
        });
    } else if (filesToAdd && filesToAdd.size) {
        newPicturesSizeInBytes = filesToAdd.size;
    }

    return (reviewPicturesSizeInBytes + itemPictureSizeInBytes + newPicturesSizeInBytes) < picturesMaxSize * 1024 * 1024;
}