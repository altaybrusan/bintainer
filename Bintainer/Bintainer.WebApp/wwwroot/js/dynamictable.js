function appendRowToTable(table, name, value) {
    var trHtml = '<td class="dtable-col1">';
    if (name !== null && name !== undefined) {
        trHtml += name;
    }
    trHtml += '</td>' +
        '<td class="dtable-col2">';
    if (value !== null && value !== undefined) {
        trHtml += value;
    }
    trHtml += '</td>' +
        '<td><a data-mode="edit" class="action" href="javascript:void(0)">edit</a></td>' +
        '<td><a class="delete" href="javascript:void(0)">delete</a></td>';


    var tr = $('<tr>' + trHtml + '</tr>');
    table.find('tbody').append(tr);

    //var trHtml = $('#tr_template').html();
    //var tr = $('<tr>' + trHtml + '</tr>');
    //table.find('tbody').append(tr);

    // Show the row in edit mode
    showRowInEditMode(tr);

    // Delete button click handler
    tr.find('a.delete').on('click', function () {
        tr.remove();
    });

    // Update button click handler
    tr.find('.action').on('click', function () {
        toggleRowEditMode(tr); // Toggle edit mode when action button is clicked
    });

    function showRowInEditMode(tr) {
        var action = tr.find('.action');
        action.attr('data-mode', 'update');
        action.text('Update');

        // Replace text with inputs containing current values
        var col1_input = $('<input type="text" class="form-control  input-sharp" value="">');
        var col2_input = $('<input type="text" class="form-control  input-sharp" value="">');

        var col1 = tr.find('.dtable-col1');
        col1.html(col1_input);

        var col2 = tr.find('.dtable-col2');
        col2.html(col2_input);
    }

    function toggleRowEditMode(tr) {
        var action = tr.find('.action');
        var mode = action.attr('data-mode');

        if (mode === 'edit') {
            showRowInEditMode(tr); // Show in edit mode if currently in view mode
        } else {
            // Save input values back to text
            var col1 = tr.find('.dtable-col1');
            var col2 = tr.find('.dtable-col2');

            col1.text(col1.find('input').val());
            col2.text(col2.find('input').val());

            action.attr('data-mode', 'edit');
            action.text('Edit');
        }
    }
}