﻿const restfulServiceURL = "http://localhost:53024/api/scoreboard/";
var xhttp = new XMLHttpRequest();

function MicroController(group) {
    this.__proto__ = new Controller(group);
    this.values = {};

    this.setValue = function (value) {
        if (isNaN(value)) { value = ""; }
        this.group.valueElement.innerText = "*" + value + "*";
        if (this.group.newValueElement !== null) {
            this.group.newValueElement.value = value;
        }

        xhttp.open("PUT", restfulServiceURL + this.group.name, true);
        xhttp.setRequestHeader("Content-Type", "application/json");
        xhttp.send(JSON.stringify({ key: this.group.name, value: value }));
    };

    this.getValue = function () {
        xhttp.open("GET", restfulServiceURL + this.group.name, false);
        xhttp.send();

        var response = JSON.parse(xhttp.responseText);
        return response.value;
    };
}

function MicroControllerScoreboard() {
    this.__proto__ = new Scoreboard();

    this.createGroup = function (groupName, value, nDigits, newValue, update) {
        var group = this.__proto__.createGroup(groupName, value, nDigits, newValue, update);
        group.controller = new MicroController(group);
        return group;
    };

    function put(path, value) {
        xhttp.open("PUT", restfulServiceURL + path, true);
        xhttp.setRequestHeader("Content-Type", "application/json");
        xhttp.send(value);
    }

    this.test = function (button) {
        if (button.locked) { return; }
        try {
            button.locked = true;

            if (button.innerText === "Stop") {
                button.innerText = button.initialText;
                sendTest("test/stop");
                return;
            }

            button.initialText = button.innerText;
            button.innerText = "Stop";
            put("test/start");
        } finally {
            button.locked = false;
        }
    };

    this.displayText = function (text) {
        put("text", text);
    };
}