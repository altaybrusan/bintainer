function appendRowToTable(table, name, value, guidId = null) {
    // Create the row HTML with placeholders for columns and actions
    var trHtml =
        '<td class="dtable-col1"></td>' +
        '<td class="dtable-col2"></td>' +
        '<td><a class="delete" href="javascript:void(0)">delete</a></td>';

    // Create the row element
    var tr = $('<tr>' + trHtml + '</tr>');

    // If a GuidId is provided, assign it to the row as a data attribute
    if (guidId) {
        tr.attr('data-guid', guidId);
    }

    // Append the row to the table body
    table.find('tbody').append(tr);

    // Trim the name and value if they exist, otherwise set to an empty string
    name = name ? name.trim() : '';
    value = value ? value.trim() : '';

    // Show the row in edit mode by default
    showRowInEditMode(tr, name, value);

    // Attach a delete event handler to the delete link
    tr.find('a.delete').on('click', function () {
        tr.remove();
    });

    // Function to show the row in edit mode
    function showRowInEditMode(tr, name, value) {
        // Create input fields for name and value
        var col1_input = $('<input type="text" class="form-control input-sharp" value="' + name + '">');
        var col2_input = $('<input type="text" class="form-control input-sharp" value="' + value + '">');

        // Populate the first column with the name input
        var col1 = tr.find('.dtable-col1');
        col1.html(col1_input);

        // Populate the second column with the value input
        var col2 = tr.find('.dtable-col2');
        col2.html(col2_input);
    }
}
