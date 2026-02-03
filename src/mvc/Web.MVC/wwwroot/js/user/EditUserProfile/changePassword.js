const passwordErrorBlock = document.getElementById('passwordErrorBlock');
document.getElementById('requestPasswordUpdateSubmit').addEventListener('click', async function (e) {
    e.preventDefault();

    const requestPasswordUpdateForm = document.getElementById('requestPasswordUpdateForm');
    const formData = new FormData(requestPasswordUpdateForm);

    const response = await fetch(requestPasswordUpdateForm.action, { method: 'POST', body: formData });

    if (response.ok) {
        passwordErrorBlock.style.display = 'none';
        document.getElementById('requestPasswordUpdateForm').style.display = 'none';
        document.getElementById('requestPasswordUpdateSuccessText').style.display = 'block';
    }
    else if (response.status === 400) {
        document.getElementById('requestPasswordUpdateInput').value = '';
        passwordErrorBlock.innerHTML = '';

        const json = await response.json();
        json.errorMessages.forEach(error => {
            const div = document.createElement('div');
            div.textContent = error;
            passwordErrorBlock.appendChild(div);
        });

        passwordErrorBlock.style.display = 'block';
    } else if (response.status === 409) {
        document.getElementById('requestPasswordUpdateInput').value = '';
        passwordErrorBlock.textContent = await response.text();
        passwordErrorBlock.style.display = 'block';
    }
    else {
        document.getElementById('requestPasswordUpdateInput').value = '';

        passwordErrorBlock.textContent = "Что-то пошло не так, попробуйте еще раз";
        passwordErrorBlock.style.display = 'block';
    }
});

const passwordModalOverlay = document.getElementById('passwordModalOverlay');
passwordModalOverlay.addEventListener('click', e => {
    if (e.target === passwordModalOverlay) {
        passwordModalOverlay.style.display = 'none';
    }
});
document.getElementById('openPasswordModalBtn').addEventListener('click', () => {
    passwordModalOverlay.style.display = 'flex';
});