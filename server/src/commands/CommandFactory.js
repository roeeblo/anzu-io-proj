const { MoveLeftCommand } = require('./MoveLeftCommand');
const { MoveRightCommand } = require('./MoveRightCommand');
const { StopCommand } = require('./StopCommand');
const { JumpCommand } = require('./JumpCommand');
const { SpawnItemCommand } = require('./SpawnItemCommand');
const { AddScoreCommand } = require('./AddScoreCommand');
const { Tree1Command } = require('./Tree1Command');
const { Tree2Command } = require('./Tree2Command');

const _commands = {
  MOVE_LEFT: new MoveLeftCommand(),
  MOVE_RIGHT: new MoveRightCommand(),
  STOP: new StopCommand(),
  JUMP: new JumpCommand(),
};

function create(type, arg, flag, db, bounds) {
  const key = (type || '').toUpperCase();
  if (key === 'SPAWN_ITEM')
    return new SpawnItemCommand(arg, db, bounds);
  if (key === 'ADD_SCORE')
    return new AddScoreCommand(arg);
  if (key === 'TREE1_COMMAND'){
    console.log('reached TREE1COMMAND FACTORY js');
    return new Tree1Command(flag);
    }
  if (key === 'TREE2_COMMAND'){
    console.log('reached TREE2COMMAND FACTORY js');
    return new Tree2Command(flag);
  }

  return _commands[key] || null;
}

function getValidTypes() {
  return [...Object.keys(_commands), 'SPAWN_ITEM itemId x y (e.g. gem 0 0)', 'ADD_SCORE amount'];
}

module.exports = { create, getValidTypes };
