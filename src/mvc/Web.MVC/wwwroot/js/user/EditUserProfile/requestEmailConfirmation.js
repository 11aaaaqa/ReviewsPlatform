const confirmationWaitingP = document.getElementById('confirmationWaitingP');
const confirmationSentP = document.getElementById('confirmationSentP');
const unconfirmedP = document.getElementById('unconfirmedP');
const errorInfoP = document.getElementById('errorInfoP');
const requestEmailConfirmationForm = document.getElementById('requestEmailConfirmationForm');
document.getElementById('requestEmailConfirmationSubmit').addEventListener('click', async function (e) {
    e.preventDefault();

    const requestEmailConfirmationForm = document.getElementById('requestEmailConfirmationForm');
    const formData = new FormData(requestEmailConfirmationForm);

    requestEmailConfirmationForm.style.display = 'none';
    unconfirmedP.style.display = 'none';
    confirmationWaitingP.style.display = 'block';

    const response = await fetch(requestEmailConfirmationForm.action, { method: 'POST', body: formData });

    confirmationWaitingP.style.display = 'none';

    if (response.redirected || response.ok) {
        errorInfoP.style.display = 'none';
        confirmationSentP.style.display = 'block';
    }
    else if (response.status === 409) {
        confirmationSentP.style.display = 'none';
        errorInfoP.textContent = await response.text();
        errorInfoP.style.display = 'block';
    } else {
        confirmationSentP.style.display = 'none';
        errorInfoP.textContent = 'Что-то пошло не так';
        errorInfoP.style.display = 'block';
    }
});