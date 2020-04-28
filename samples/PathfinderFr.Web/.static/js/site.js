$(function () {
    $(".sousmenu").hide();
    $("ul.listecachée").hide();
    $(".opachover")
        .each(function () { setopac(this); })
        .hover(function () { entre(this); }, function () { sort(this); });
});

function setopac(o) { var bopac = $(o).css('opacity'); $(o).attr('bopac', bopac); }
function entre(o) { $(o).animate({ opacity: 1 }, 250); }
function sort(o) { var bopac = $(o).attr('bopac'); $(o).animate({ opacity: bopac }, 100); }
function arrayset(a, i, v) { a[i] = v; }
function arrayget(a, i) { return (a[i]); }

function expandMenu(id) {
    var target = document.getElementById(id);
    var bouton = document.getElementById('bouton' + id)
    if (target.style.display == 'none') {
        bouton.src = '/images/pathfinder/wiki/BoutonM.jpg';
        target.opacity = 0;
        $('#' + id).slideDown('normal');
    } else {
        bouton.src = '/images/pathfinder/wiki/BoutonP.jpg';
        $('#' + id).slideUp('normal');
    }
}
function outButton(id) {
    var bouton = document.getElementById('bouton' + id);
    bouton.parentNode.parentNode.style.backgroundColor = '';
}
function overButton(id) {
    var bouton = document.getElementById('bouton' + id);
    var target = document.getElementById(id);
    if (target.style.display == 'none') {
        bouton.parentNode.parentNode.style.backgroundColor = '#ffc';
    }
}
function affichemenu(me) {
    me.children("ul.tag").show();
    me.children("center").css("background-color", "#ffc");
}
function cachemenu(me) {
    me.children("center").css("background-color", "");
    me.children("ul.tag").hide();
}
function tableFilter(triggerElement) {
    var trigger = $(triggerElement);
    var table = trigger.nextAll("table").first();
    if (!table) {
        console.log("TableFilter: no table found!");
        return;
    }
    var filter = prompt("Entrez le filtre à appliquer (ou rien pour tout afficher; mot1|mot2 pour choix multiple).", "");
    var reFilter = new RegExp(filter, "i");
    var rows = table.find("tr").filter(function () {
        return $(this).children("th").length == 0;
    });
    rows.each(function () {
        var inner = $(this).html();
        var keep = inner.match(reFilter) != null;
        $(this).css("display", keep ? "table-row" : "none");
    });
    trigger.attr("title", "Filtre actuel : " + (filter == "" ? "aucun" : filter)
        + " - cliquez pour changer le filtre");
}