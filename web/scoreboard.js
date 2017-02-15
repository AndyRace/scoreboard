const cTestInterval = 500;

function debugInfo(info) {
    // alert(info);
}

// default controller
function Controller(group) {
    this.group = group;

    this.setValue = function (value) {
        debugInfo("Controller.SetValue: " + this.group + ", value: " + value);

        if (isNaN(value)) { value = ""; }
        this.group.valueElement.innerText = value;
        if (this.group.newValueElement !== undefined) {
            this.group.newValueElement.value = value;
        }
    };

    this.getValue = function () {
        var value = this.valueElement.innerText;
        if (value === "") {
            value = NaN;
        }

        value = Number(value);

        debugInfo("Controller.GetValue: " + this.group + ", value: " + value);

        return value;
    };
}

function Group(group, value, nDigits, newValue, update) {
    var self = this;

    this.controller = new Controller(group);
    this.name = group;
    this.valueElement = document.getElementById(value);
    this.nDigits = nDigits;
    this.maxValue = Math.pow(10, nDigits) - 1;
    this.newValueElement = document.getElementById(newValue);
    this.updateElement = document.getElementById(update);

    this.setValue = function (value) {
        if (value < 0) { value = NaN; }
        if (value > this.maxValue) { value = this.maxValue; }

        // plug in the model here
        this.controller.setValue(value);

        debugInfo("Set: Group: " + this.group + ", value: " + value);
    };

    this.getValue = function () {
        return this.controller.getValue();
    };

    this.inc = function (value) {
        value = Number(value);
        if (isNaN(value)) { value = 0; }

        var current = this.getValue();
        if (isNaN(current)) { current = 0; }

        this.setValue(current + value);
    };

    if (this.updateElement !== null) {
        this.updateElement.onclick = function (event) {
            self.setValue(self.newValueElement.value);
        };
    }
}

function Ticker(groups) {
    var self = this;
    self.groups = groups;
    self.initialValues = groups.map(function (group) { return { group: group, originalValue: group.getValue() }; });

    this.tickAlternate = function () {
        // this == window
        if (isNaN(self.state)) {
            self.state = 0;
        } else {
            self.state++;
        }

        if (self.state === 0) {
            self.groups.forEach(function (value) {
                value.setValue(NaN);
            });
            return;
        }

        self.initialValues.forEach(function (value) {
            value.group.setValue(value.originalValue);
        });

        self.state = NaN;
    };

    this.tick = function () {
        // this == window
        if (isNaN(self.state)) {
            self.multipliers = self.groups.map(function (group) {
                var result = 0;
                for (var digit = 0; digit < group.nDigits; digit++) {
                    result = result * 10 + 1;
                }

                return { group: group, multiplier: result };
            });
            self.state = 0;
        } else {
            self.state++;
            if (self.state === 11) { self.state = 0; }
        }

        self.multipliers.forEach(function (value) {
            if (self.state === 10) {
                value.group.setValue(NaN);
                return;
            }

            value.group.setValue(value.multiplier * self.state);
        });
    };

    this.stopTicker = function (button) {
        self.initialValues.forEach(function (value) {
            value.group.setValue(value.originalValue);
        });

        clearInterval(self.intervalVar);
        self.intervalVar = null;
    };

    this.intervalVar = setInterval(this.tick, cTestInterval);
}

function Scoreboard() {
    this.groups = [];

    this.findGroup = function (group) {
        return this.groups.find(function (value) { return value.name === group; });
    };

    this.reset = function () {
        this.groups.forEach(function (group) {
            group.setValue(NaN);
        });
    };

    this.test = function (button) {
        if (button.locked) { return; }
        try {
            button.locked = true;

            if (button.ticker !== undefined) {
                button.ticker.stopTicker();
                button.ticker = undefined;
                button.innerText = button.initialText;
                return;
            }

            button.initialText = button.innerText;
            button.innerText = "Stop";
            button.ticker = new Ticker(this.groups);
        } finally {
            button.locked = false;
        }
    };

    this.createGroup = function (group, value, nDigits, newValue, update) {
        return new Group(group, value, nDigits, newValue, update);
    };
}

function inc(group, value) {
    scoreboard.findGroup(group).inc(value);
}