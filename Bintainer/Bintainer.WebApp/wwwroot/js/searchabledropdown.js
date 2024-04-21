const dropdownButton = document.getElementById("dropdownMenuButton1");

const handleInput = () => {
    const inputValue = document.querySelector('.form-control').value;
    const results = ["apple", "banana", "cherry", "date", "elderberry"];
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