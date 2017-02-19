function initialiseLayout() {
    var header = document.getElementById("header");

    // <img class="centered" src="http://miltoncricket.org.uk/wp-content/themes/mcc-theme/images/milton-cricket-club-logo.png" alt="Milton Cricket Club Logo" />
    var img = document.createElement("img");
    img.src = "http://miltoncricket.org.uk/wp-content/themes/mcc-theme/images/milton-cricket-club-logo.png";
    img.alt = "Milton Cricket Club Logo";
    img.className = "centered";
    header.appendChild(img);

    // <h1 class="centered">Milton CC Scoreboard</h1>
    //var title = document.createElement("h1");
    //title.className = "centered";
    //title.innerText = "Scoreboard";
    //header.appendChild(title);
}

// MCC and controller-specific code
//var scoreboard = new MicroControllerScoreboard();
var scoreboard;

function changeConnection(isConnected) {
    if (isConnected) {
        scoreboard = new MicroControllerScoreboard();
    } else {
        scoreboard = new Scoreboard();
        scoreboard.reset();
    }

    scoreboard.groups.push(scoreboard.createGroup('total', 'totalValue', 3, 'totalNew', 'totalUpdate'));
    scoreboard.groups.push(scoreboard.createGroup('firstInnings', 'firstInningsValue', 3, 'firstInningsNew', 'firstInningsUpdate'));
    scoreboard.groups.push(scoreboard.createGroup('overs', 'oversValue', 2));
    scoreboard.groups.push(scoreboard.createGroup('wickets', 'wicketsValue', 1));

    scoreboard.update();
}

function initialise() {
    changeConnection(false);

    initialiseLayout();
    scoreboard.reset();
}
