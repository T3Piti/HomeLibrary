function addAuthor() {
    const container = document.getElementById('authors-container');
    const count = container.querySelectorAll('.author-block').length;

    const block = document.createElement('div');
    block.className = 'author-block';

    block.innerHTML = `
        <button type="button" class="btn-remove-author" onclick="removeAuthor(this)">×</button>
        <div class="author-inputs">
            <div>
                <label>Имя</label>
                <input name="Authors[${count}].FirstName" class="form-control" required />
            </div>
            <div>
                <label>Фамилия</label>
                <input name="Authors[${count}].MiddleName" class="form-control" required />
            </div>
            <div>
                <label>Отчество</label>
                <input name="Authors[${count}].LastName" class="form-control" placeholder="необязательно" />
            </div>
        </div>
    `;

    container.appendChild(block);
}

function removeAuthor(btn) {
    const block = btn.parentElement;
    block.remove();
    reindexAuthors();
}

function reindexAuthors() {
    const blocks = document.querySelectorAll('.author-block');
    blocks.forEach((block, index) => {
        const inputs = block.querySelectorAll('input');
        inputs.forEach(input => {
            const match = input.name.match(/Authors\[(\d+)\]\.(\w+)/);
            if (match) {
                input.name = `Authors[${index}].${match[2]}`;
            }
        });
    });
}
