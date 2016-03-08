const lib = require('./testLib.js');

const api = {
  'domain': 'http://52.20.23.28',
  'port': 8080,
  'path': '/run'
};

/**
 * Sample test graph formatting:
 *
 * {
 *   name: [A quick description of the graph]
 *   path: [Path to the graph file]
 *   expected: [The expected data in the Akaba.Result node, if it exists.]
 *   hint: [A common reason why this graph may not work]
 * }
 */
const files = [
  {
    name: 'four number nodes',
    path: './graphs/FourNumbers.json',
    expected: '1',
    hint: 'Make sure Reach is properly running and that your graph JSON is valid.'
  },
  {
    name: 'number to list create',
    path: './graphs/NumberToListCreate.json',
    expected: null,
    hint: 'This issue may be related to nodes with a variable number of inputs.'
  },
  {
    name: 'number to cuboid to volume',
    path: './graphs/CuboidVolume.json',
    expected: '1728',
    hint: 'This issue is usually related to Reach not being able to load geometries.'
  },
  {
    name: 'simple stuffer space creation',
    path: './graphs/SimpleSpace.json',
    expected: 'stuffer.Space',
    hint: 'It may be that the stuffer library is not being loaded.'
  },
  {
    name: 'stuffer creation using strategy',
    path: './graphs/StufferDesignTools2.json',
    expected: 'stuffer.Space'
  },
  {
    name: 'simple python script that returns 3',
    path: './graphs/Pythree.json',
    expected: '3',
    hint: 'The Python script is likely not being correctly interpreted by Reach'
  }
];

var main = function main(options) {
  lib.test_multiple(files, api, options);
};

main();
