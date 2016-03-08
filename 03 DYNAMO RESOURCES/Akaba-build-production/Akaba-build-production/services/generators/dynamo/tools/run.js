const lib = require('./testLib.js');

const api = {
  'domain': 'http://52.20.23.28',
  'port': 8080,
  'path': '/run'
};

const argv = require('minimist')(process.argv.slice(2));

var main = function main() {
  if (argv.v) {
    if (typeof argv.v === 'string') {
      console.log('Opening file: ' + argv.v);
      lib.test_one(argv.v, api, {v: true});
      return;
    }

    console.log('Error when running script. See usage.');
    return;
  }

  if (argv._.length === 0) {
    console.log('No file specified');
    return;
  }

  console.log('Opening file: ' + argv._[0]);
  lib.test_one(argv._[0], api, {v: false});
  return;
};


main();
