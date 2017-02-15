function TestController(group) {
    this.__proto__ = new Controller(group);
    this.values = {};

    this.setValue = function (value) {
        this.values[this.group.name] = value;

        if (isNaN(value)) { value = ""; }
        this.group.valueElement.innerText = "*" + value + "*";
        if (this.group.newValueElement !== null) {
            this.group.newValueElement.value = value;
        }
    };

    this.getValue = function () {
        return this.values[this.group.name];
    };
}

function TestScoreboard() {
    this.__proto__ = new Scoreboard();

    this.createGroup = function (groupName, value, nDigits, newValue, update) {
        var group = this.__proto__.createGroup(groupName, value, nDigits, newValue, update);
        group.controller = new TestController(group);
        return group;
    };
}
