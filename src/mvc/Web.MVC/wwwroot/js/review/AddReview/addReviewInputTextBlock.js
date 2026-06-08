if (reviewTextInput.value)
    reviewBlock.innerHTML = reviewTextInput.value;

if (shortReviewInput.value)
    shortReviewBlock.innerHTML = shortReviewInput.value;

reviewBlock.addEventListener('beforeinput', (e) => {
    const currentLength = reviewBlock.innerHTML.length;

    const selection = window.getSelection();
    const selectedTextLength = selection.toString().length;

    if (currentLength - selectedTextLength >= reviewTextMaxLength && e.inputType.startsWith('insert')) {
        e.preventDefault();
    }
});

shortReviewBlock.addEventListener('beforeinput', (e) => {
    const currentLength = shortReviewBlock.innerHTML.length;

    const selection = window.getSelection();
    const selectedTextLength = selection.toString().length;

    if (currentLength - selectedTextLength >= shortReviewMaxLength && e.inputType.startsWith('insert')) {
        e.preventDefault();
    }
});

reviewBlock.addEventListener('paste', (e) => {
    const paste = (e.clipboardData || window.clipboardData).getData('text');
    const currentLength = reviewBlock.innerHTML.length;
    const selection = window.getSelection().toString().length;

    if (currentLength - selection + paste.length > reviewTextMaxLength) {
        e.preventDefault();
        const allowedLength = reviewTextMaxLength - (currentLength - selection);
        if (allowedLength > 0) {
            document.execCommand('insertText', false, paste.substring(0, allowedLength));
        }
    }
});

shortReviewBlock.addEventListener('paste', (e) => {
    const paste = (e.clipboardData || window.clipboardData).getData('text');
    const currentLength = shortReviewBlock.innerHTML.length;
    const selection = window.getSelection().toString().length;

    if (currentLength - selection + paste.length > shortReviewMaxLength) {
        e.preventDefault();
        const allowedLength = shortReviewMaxLength - (currentLength - selection);
        if (allowedLength > 0) {
            document.execCommand('insertText', false, paste.substring(0, allowedLength));
        }
    }
});