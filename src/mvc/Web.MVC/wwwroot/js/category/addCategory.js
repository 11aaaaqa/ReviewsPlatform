const addCategoryModalErrorBlock = document.getElementById('addCategoryModalErrorBlock');
document.getElementById('addCategorySubmit').addEventListener('click', async function (e) {
    e.preventDefault();

    const addCategoryForm = document.getElementById('addCategoryForm');
    const formData = new FormData(addCategoryForm);

    const response = await fetch(addCategoryForm.action, { method: 'POST', body: formData });

    if (response.redirected) {
        window.location.href = response.url;
    } else if (response.status === 400 || response.status === 409) {
        addCategoryModalErrorBlock.textContent = await response.text();
        addCategoryModalErrorBlock.style.display = 'block';
    } else if (response.ok) {
        window.location.reload();
    } else {
        addCategoryModalErrorBlock.textContent = "Что-то пошло не так, попробуйте еще раз";
        addCategoryModalErrorBlock.style.display = 'block';
    }
});

const addCategoryModalOverlay = document.getElementById('addCategoryModalOverlay');
addCategoryModalOverlay.addEventListener('click', e => {
    if (e.target === addCategoryModalOverlay) {
        addCategoryModalOverlay.style.display = 'none';
    }
});
document.getElementById('openAddCategoryModalBtn').addEventListener('click', () => {
    addCategoryModalOverlay.style.display = 'flex';
});