# Running with Ubuntu

## Installing node.
curl --silent --location https://deb.nodesource.com/setup_0.12
apt-get install -y nodejs

## Installing module dependencies
npm install request express body-parser uuid optimist

## Running the app.
node ./app.js