const select = document.getElementById('categorySelect');
const selected = document.getElementById('categorySelectSelected');
const optionsContainer = document.getElementById('categorySelectItems');
const categorySelectInput = document.getElementById('categorySelectInput');

selected.addEventListener('click', () => {
    optionsContainer.style.display = optionsContainer.style.display === 'block' ? 'none' : 'block';
});

document.addEventListener('click', (e) => {
    if (!select.contains(e.target)) {
        optionsContainer.style.display = 'none';
    }
});

optionsContainer.querySelectorAll('div').forEach(option => {
    option.addEventListener('click', () => {
        selected.textContent = option.textContent;
        optionsContainer.style.display = 'none';
        categorySelectInput.value = option.dataset.value;
    });
});