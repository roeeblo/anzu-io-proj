const { Command } = require('./Command');

class Tree2Command extends Command {
    constructor(flag) {
        super();
        this._flag = true;
    }

    get type() {
        return 'TREE2_COMMAND';
    }

    switchFlag(){
        this._flag = !this._flag;
        return this._flag;
    }

    execute(ws) {

        if (!ws || ws.readyState !== 1) return false;
        console.log('reached execute tree2 js');
        ws.send(JSON.stringify({ type: this.type, payload: { flag: this.switchFlag() } }));


        return true;
    }
}

module.exports = { Tree2Command };
