(function () {
    const newMiddleNameInput = document.getElementById('newMiddleName');
    const newFirstNameInput = document.getElementById('newFirstName');
    const newLastNameInput = document.getElementById('newLastName');
    const btnAddAuthor = document.getElementById('btnAddAuthor');
    const authorsList = document.getElementById('authorsList');

    // Берём текущий максимальный индекс из уже существующих элементов
    let authorIndex = Array.from(authorsList.querySelectorAll('li.author-item')).length;

    function addAuthorToList() {
        const middleName = newMiddleNameInput.value.trim();
        const firstName = newFirstNameInput.value.trim();
        const lastName = newLastNameInput.value.trim();

        if (!middleName || !firstName) {
            alert('Фамилия и Имя обязательны.');
            return;
        }

        const li = document.createElement('li');
        li.className = 'author-item';
        li.innerHTML = `
            <span class="author-display">
                ${middleName} ${firstName} ${lastName ? lastName : ''}
            </span>
            <button type="button" class="btn-remove-author" data-index="${authorIndex}">×</button>
            
            <input type="hidden" name="Authors[${authorIndex}].MiddleName" value="${middleName}" />
            <input type="hidden" name="Authors[${authorIndex}].FirstName" value="${firstName}" />
            <input type="hidden" name="Authors[${authorIndex}].LastName" value="${lastName}" />
        `;

        authorsList.appendChild(li);
        authorIndex++;

        // Очищаем поля
        newMiddleNameInput.value = '';
        newFirstNameInput.value = '';
        newLastNameInput.value = '';
    }

    btnAddAuthor.addEventListener('click', addAuthorToList);

    authorsList.addEventListener('click', (e) => {
        const btn = e.target.closest('.btn-remove-author');
        if (!btn) return;

        const item = btn.closest('li');
        item.remove();
        recalculateIndices();
    });

    function recalculateIndices() {
        const items = Array.from(authorsList.querySelectorAll('li.author-item'));
        items.forEach((li, index) => {
            const inputs = li.querySelectorAll('input[type="hidden"]');
            inputs.forEach(input => {
                const oldName = input.name;
                // Обновляем индекс в имени поля: Authors[x].Prop
                const propPart = oldName.replace(/Authors\[\d+\]/, `Authors[${index}]`);
                input.name = propPart;
            });
            // Также можно обновить data-index у кнопки, если нужно
            const removeBtn = li.querySelector('.btn-remove-author');
            if (removeBtn) removeBtn.dataset.index = index.toString();
        });
        authorIndex = items.length;
    }
})();
