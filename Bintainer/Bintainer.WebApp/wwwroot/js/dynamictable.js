function appendRowToTable(table)
{
    var trHtml = $('#tr_template').html();
    var tr = $('<tr>' + trHtml + '</tr>');
    table.find('tbody').append(tr);

    tr.find('a.delete').on('click', function () {
        tr.remove();
    });

    tr.find('.action').on('click', function () {
        onActionHandler($(this).closest('tr')); // Pass the row to the handler
    });

    function onActionHandler(tr) {
        var action = tr.find('.action');
        var col1 = tr.find('.dtable-col1');
        var col2 = tr.find('.dtable-col2');
        var state = tr.find('.state'); // Assuming there's a state element, adjust as needed
        var mode = action.attr('data-mode');

        if (mode === 'edit') {
            // Switch from "edit" to "update" mode - inputs to text
            action.attr('data-mode', 'update');
            action.text('Update');

            // Replace text with inputs containing current values
            var col1_text = col1.text();
            var col2_text = col2.text();
            col1.html('<input type="text" value="' + col1_text + '" class="form-control">');
            col2.html('<input type="text" value="' + col2_text + '"  class="form-control">');

            // Optionally handle the state as a dropdown
            if (state.length > 0) {
                var currentState = state.text();
                var selectHtml = '<select class="form-select"><option value="active"' + (currentState === 'active' ? ' selected' : '') + '>Active</option><option value="inactive"' + (currentState === 'inactive' ? ' selected' : '') + '>Inactive</option></select>';
                state.html(selectHtml);
            }
           
        } else {
            // Switch from "update" to "edit" mode - text to inputs
            action.attr('data-mode', 'edit');
            action.text('Edit');

           

            // Save input values back to text
            col1.text(col1.find('input').val());
            col2.text(col2.find('input').val());
            if (state.length > 0 && state.find('select').length > 0) {
                state.text(state.find('select').val()); // Assumes state is handled with a select dropdown
            }
        }
    }

}



