# Usage

---


## Installing & Running Node

If you haven't already, install [Node.jS](https://nodejs.org/). Then, simply run:

```bash
$ npm install
$ node app.js
```


## Adding a new generator
Generators should all be Dynamo graphs in `.json` format. See `tools` on how to convert a Dynamo graph and what it should look like.

To add a new generator, simply add the json graph to `./generatorGraphs`. Then, make the request in which the `settings.body.generatorName` will be the name of the file with the desired generator.

For example:
```bash
$ ls ./generatorGraphs
> SampleGenerator.json
$ node app.js
```
And the request body should look like:
```
{
  "requirements": {
      "site": {
          "width": 20,
            "height":30,
            "length": 10
        }
    }
  "settings": {
      "generatorName": "SampleGenerator"
    }
}
```   

Don't forget to edit `api` in `computeWithReach.js` to point to the correct URL. :+1:

## Making Requests
This API follows the current [Akaba Wiki REST API Documentation](https://github.com/AutodeskBIG/Akaba/wiki/REST-API-documentation).

The required fields by this generator for POSTing to /generator are:
```
requirements
  site
    width
    height
    length
settings
  generatorName
```

When making a request, the `content-type` header should be `application/json`.

Here is a sample POST request using curl

```curl -i -H "Content-Type: application/json" -X POST -d '{"requirements":{"site":{"width":100,"height":50,"length":20}},"settings":{"generatorName":"SampleGenerator"}}' http://localhost:34574/generator```
