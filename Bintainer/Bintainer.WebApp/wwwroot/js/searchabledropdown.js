function populateList(words, parentElement, dropdownButton) {
    if (words.length === 0) {
        parentElement.append('<li class="dropdown-item">No items found</li>');
    } else {
        words.forEach(function (word) {
            var listItem = $('<li>');
            var link = $('<a>').addClass('dropdown-item').attr('href', '#').text(word);

            link.on('click', function () {
                dropdownButton.text(word);
                dropdownButton.focus();
            });

            listItem.append(link);
            parentElement.append(listItem);
        });
    }
}
