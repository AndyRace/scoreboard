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

    this.execute = function (method, parameters /*= null*/, obj /*= null*/) {
        // I'm worried about multi-threading so ping a timeout here io serialise it
        setTimeout(function () {
            var uri = self.uri;
            if (typeof parameters !== "undefined" && parameters !== null) {
                uri += "?" + parameters;
            }

            self.request.open(method, uri, true);
            self.request.setRequestHeader("Content-Type", "application/json");
            // this.request.send(JSON.stringify(obj));
            self.request.send(obj);
        }, 100);
    };

    this.put = function (parameters /*= null*/, obj /*= null*/) {
        this.execute("PUT", parameters, obj);
    };

    this.get = function (parameters, obj /*= null*/) {
        this.execute("GET", parameters, obj);
    };

    this.post = function (parameters, obj /*= null*/) {
        this.execute("POST", parameters, obj);
    };
}

function MicroGroup(scoreboard, groupName, valueElement, nDigits) {
    this.__proto__ = new Group(scoreboard, groupName, valueElement, nDigits);

    var self = this;

    this.apiGet = new api(cValueApiUri);
    this.apiPut = new api(cValueApiUri);

    this.apiGet.request.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            var xhttp = this;
            setTimeout(function () {
                //console.log("Get: group:" + self.name + ", value:'" + xhttp.responseText + "'");

                var response = JSON.parse(xhttp.responseText);

                // todo: assert response.key == group
                self.updateScore(response.Value, true);

                // track last response time
                self.scoreboard.lastResponse = new Date();
            }, 100);
        }
    };

    this.updateScore = function (value, scoreboardHasResponded) {
        this.__proto__.value = value;

        value = this.__proto__.value;

        this.setDisplayText(value, scoreboardHasResponded);
    };

    this.timeoutVar = null;
    this.refresh = function () {
        if (self.timeoutVar !== null) {
            clearTimeout(self.timeoutVar);
            //console.log("clearTimeout(" + self.name + "): " + self.timeoutVar);
        }

        self.timeoutVar = setTimeout(function () {
            // kick another refresh off in case anyone else is updating
            self.timeoutVar = setTimeout(function () {
                self.refresh();
            }, 2000);

            // do a get which itself will call this refresh()
            self.apiGet.get("group=" + self.name);
        }, 500);

        //console.log("setTimeout(" + self.name + "): " + self.timeoutVar);
    };

    // Keep updating the score.
    // This allows for others updating the score.
    // setInterval(this.refresh, 2000);

    Object.defineProperty(this, 'value', {
        set: function (value) {
            this.updateScore(value, false);

            value = this.__proto__.value;

            this.apiPut.put("group=" + self.name + "&value=" + value);

            // do a cheeky get after every put
            this.refresh();
        },

        get: function () {
            return this.__proto__.value;
        }
    });
}

function MicroControllerScoreboard() {
    this.__proto__ = new Scoreboard();

    this.testApi = new api(cTestApiUri);
    this.textApi = new api(cTextApiUri);

    this.createGroup = function (groupName, value, nDigits) {
        return new MicroGroup(this, groupName, value, nDigits);
    };

    this.test = function () {
        this.testApi.post();
    };

    this.displayText = function (text) {
        this.textApi.post(null, text);
    };
}