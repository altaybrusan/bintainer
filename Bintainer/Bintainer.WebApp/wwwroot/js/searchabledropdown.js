const searchInput = document.getElementById('searchInput');
const dropdownButton = document.getElementById("dropdownMenuButton1");

const handleInput = () => {
    const inputValue = searchInput.value;
    const dataAttribute = searchInput.getAttribute('data-source');
    const results = JSON.parse(dataAttribute);
    const parentElement = document.querySelector(".dropdown-menu");
    const elementsToRemove = document.querySelectorAll("li");
    elementsToRemove.forEach(element => {
        element.remove();
    });
    if (inputValue) {
        const matchingWords = results.filter(word => word.toLowerCase().includes(inputValue.toLowerCase()));
        populateList(matchingWords, parentElement, inputValue);
    } else {
        populateList(results, parentElement);
    }
}

const populateList = (words, parentElement, highlightText = '') => {
    words.forEach(word => {
        const listItem = document.createElement("li");
        const link = document.createElement("a");
        link.classList.add("dropdown-item");
        link.href = "#";
        link.textContent = word;
        link.addEventListener("click", function () {
            dropdownButton.textContent = word;
            dropdownButton.focus();
            // If you have a bootstrap dropdown, you might want to update this to change how the display behaves
            //dropdownButton.dataset.bsOriginalTitle = word; // update this if using Bootstrap tooltips or similar
        });
        listItem.appendChild(link);
        parentElement.appendChild(listItem);
    });

    if (words.length === 0) {
        const listItem = document.createElement('li');
        listItem.textContent = "No items found";
        listItem.classList.add('dropdown-item');
        parentElement.appendChild(listItem);
    }
}

// Initial population of the list
handleInput();