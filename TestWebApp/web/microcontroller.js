const cRestfulServiceURL = "/api/";

// POST
const cTestApiUri = cRestfulServiceURL + "test";

// POST, {text: <text to display>}
const cTextApiUri = cRestfulServiceURL + "text";

// GET/PUT, {key: <score group>, value: <score>}
const cValueApiUri = cRestfulServiceURL + "value";

function api(fnUri) {
    var self = this;
    this.request = new XMLHttpRequest();

    this.uri = fnUri;

    this.execute = function (method, parameters = null, obj = null) {
        // I'm worried about multi-threading so ping a timeout here io serialise it
        setTimeout(function () {
            var uri = self.uri;
            if (parameters !== null) {
                uri += "?" + parameters;
            }

            self.request.open(method, uri, true);
            self.request.setRequestHeader("Content-Type", "application/json");
            // this.request.send(JSON.stringify(obj));
            self.request.send(obj);
        }, 100);
    };

    this.put = function (parameters = null, obj = null) {
        this.execute("PUT", parameters, obj);
    };

    this.get = function (parameters, obj = null) {
        this.execute("GET", parameters, obj);
    };

    this.post = function (parameters, obj = null) {
        this.execute("POST", parameters, obj);
    };
}

function MicroController(group, apiGet, apiPut) {
    var self = this;
    this.__proto__ = new Controller(group);

    this.apiGet = new api(cValueApiUri);
    this.apiPut = new api(cValueApiUri);

    this.apiGet.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            var xhttp = this;
            setTimeout(function () {
                var response = JSON.parse(xhttp.responseText);

                // todo: assert response.key == group
                self.updateScore(response.Value);
            }, 100);
        }
    };

    this.updateScore = function (value) {
        if (isNaN(value)) {
            this.group.setDisplayText("*");
            this.group.setNewValueText("");
        } else {
            this.group.setDisplayText("*" + value + "*");
            this.group.setNewValueText(value);
        }
    };

    this.setValue = function (value) {
        this.updateScore(value);
        this.apiPut.put("group=" + self.group.name + "&value=" + value);
    };

    // Keep updating the score.
    // This allows for others updating the score.
    setInterval(function () {
        self.apiGet.get("group=" + self.group.name);
    }, 10000);
}

function MicroControllerScoreboard() {
    this.__proto__ = new Scoreboard();

    this.testApi = new api(cTestApiUri);
    this.textApi = new api(cTextApiUri);

    this.createGroup = function (groupName, value, nDigits, newValue, update) {
        var group = this.__proto__.createGroup(groupName, value, nDigits, newValue, update);
        group.controller = new MicroController(group, this.apiGet, this.apiPut);
        return group;
    };

    this.test = function () {
        this.testApi.post();
    };

    this.displayText = function (text) {
        this.textApi.post(null, text);
    };
}