function autocomplete(inp, options) {
    var currentFocus;

    // Set the autocompleteData from the injected options
    var autocompleteData = options || [];

    /*execute a function when someone writes in the text field:*/
    inp.addEventListener("input", function (e) {
        var val = this.value;
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;
        createAutocompleteList(val);
    });

    /*execute a function presses a key on the keyboard:*/
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "-autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            currentFocus++;
            addActive(x);
        } else if (e.keyCode == 38) { //up
            currentFocus--;
            addActive(x);
        } else if (e.keyCode == 13) {
            e.preventDefault();
            if (currentFocus > -1 && x) {
                x[currentFocus].click();
            }
        }
    });

    function createAutocompleteList(val) {
        var a, b, i;
        a = document.createElement("DIV");
        a.setAttribute("id", inp.id + "-autocomplete-list");
        a.setAttribute("class", "autocomplete-items");
        inp.parentNode.appendChild(a);

        for (i = 0; i < autocompleteData.length; i++) {
            if (autocompleteData[i].substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                b = document.createElement("DIV");
                b.innerHTML = "<strong>" + autocompleteData[i].substr(0, val.length) + "</strong>";
                b.innerHTML += autocompleteData[i].substr(val.length);
                b.innerHTML += "<input type='hidden' value='" + autocompleteData[i] + "'>";
                b.addEventListener("click", function (e) {
                    inp.value = this.getElementsByTagName("input")[0].value;
                    closeAllLists();
                });
                a.appendChild(b);
            }
        }
    }

    function addActive(x) {
        if (!x) return false;
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        x[currentFocus].classList.add("autocomplete-active");
    }

    function removeActive(x) {
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }

    function closeAllLists(elmnt) {
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }

    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}

document.addEventListener("DOMContentLoaded", function () {
    // Get all autocomplete inputs on the page
    var autocompleteInputs = document.querySelectorAll("[data-autocomplete-options]");

    // For each input, initialize the autocomplete with the corresponding options
    autocompleteInputs.forEach(function (input) {
        var options = JSON.parse(document.getElementById(input.dataset.autocompleteOptions).textContent);
        autocomplete(input, options);
    });
});
