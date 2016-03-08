var akabaServicePortMap = {
    "cellular": "34568",
    "lsystem": "34569",
    "stuffer": "34570",
    "ExeBridge": "34571",
    "Facilitator": "34572"
}

function getServiceURL(serviceName)
{
    var servicePort = akabaServicePortMap[serviceName];
    var defaultURL = "http://localhost:" + servicePort;
    if (window.location.protocol.indexOf("http") !== 0)
    {
        return defaultURL;
    }
    var serviceURL = window.location.protocol + "//" + window.location.hostname + ":" + servicePort;
    return serviceURL;
}
