function appendRowToTable(table, name, value) {
    var trHtml =
          '<td class="dtable-col1"></td>' 
        + '<td class="dtable-col2"></td > '
        //+ '<td><a data-mode="edit" class="action" href="javascript:void(0)">edit</a></td>'
        + '<td><a class="delete" href="javascript:void(0)">delete</a></td>';

    var tr = $('<tr>' + trHtml + '</tr>');
    table.find('tbody').append(tr);

    name = name ? name.trim() : ' ';
    value = value ? value.trim() : ' ';

    showRowInEditMode(tr, name, value);

    tr.find('a.delete').on('click', function () {
        tr.remove();
    });

    //tr.find('.action').on('click', function () {
    //    toggleRowEditMode(tr); 
    //});

    function showRowInEditMode(tr,name,value) {
        var action = tr.find('.action');
        action.attr('data-mode', 'update');
        action.text('Update');

        var col1_input = $('<input type="text" class="form-control input-sharp" value="' + (name ? name : '') + '">');
        var col2_input = $('<input type="text" class="form-control input-sharp" value="' + (value ? value : '') + '">');
        var col1 = tr.find('.dtable-col1');
        col1.html(col1_input);

        var col2 = tr.find('.dtable-col2');
        col2.html(col2_input);
    }

    function toggleRowEditMode(tr) {
        var action = tr.find('.action');
        var mode = action.attr('data-mode');

        if (mode === 'edit') {
            showRowInEditMode(tr); 
        } else {
            var col1 = tr.find('.dtable-col1');
            var col2 = tr.find('.dtable-col2');

            col1.text(col1.find('input').val());
            col2.text(col2.find('input').val());

            action.attr('data-mode', 'edit');
            action.text('Edit');
        }
    }
}