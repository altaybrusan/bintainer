function autocomplete(inp) {
    /*the autocomplete function takes two arguments,
    the text field element and an array of possible autocompleted values:*/
    var currentFocus;

    // Parse the data-source attribute as JSON
    var autocompleteData = [];

    // Check if data-source attribute exists and is not empty
    if (inp.dataset.source) {
        try {
            autocompleteData = JSON.parse(inp.dataset.source);
        } catch (error) {
            console.error('Error parsing autocomplete data:', error);
        }
    }

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
        var x = document.getElementById(this.id + "-autocomplete-list"); // Added "-" before "autocomplete-list"
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            /*If the arrow DOWN key is pressed,
            increase the currentFocus variable:*/
            currentFocus++;
            /*and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 38) { //up
            /*If the arrow UP key is pressed,
            decrease the currentFocus variable:*/
            currentFocus--;
            /*and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 13) {
            /*If the ENTER key is pressed, prevent the form from being submitted,*/
            e.preventDefault();
            if (currentFocus > -1) {
                /*and simulate a click on the "active" item:*/
                if (x) x[currentFocus].click();
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
        /*a function to classify an item as "active":*/
        if (!x) return false;
        /*start by removing the "active" class on all items:*/
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        /*add class "autocomplete-active":*/
        x[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(x) {
        /*a function to remove the "active" class from all autocomplete items:*/
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(elmnt) {
        /*close all autocomplete lists in the document,
        except the one passed as an argument:*/
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }
    /*execute a function when someone clicks in the document:*/
    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}

document.addEventListener("DOMContentLoaded", function () {
    var autocompleteInputs = document.querySelectorAll(".autocomplete-input");
    autocompleteInputs.forEach(function (input) {
        autocomplete(input);
    });
});
