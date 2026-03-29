const { Command } = require('./Command');

class Tree1Command extends Command {
    constructor(flag) {
        super();
        this._flag = true;
    }

    get type() {
        return 'TREE1_COMMAND';
    }

    switchFlag(){
        this._flag = !this._flag;
        return this._flag;
    }

    execute(ws) {

        if (!ws || ws.readyState !== 1) return false;
        console.log('reached execute tree1 js');
        ws.send(JSON.stringify({ type: this.type, payload: { flag: this.switchFlag() } }));


        return true;
    }
}

module.exports = { Tree1Command };
