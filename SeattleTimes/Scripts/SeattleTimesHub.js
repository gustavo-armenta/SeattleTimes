/// <reference path="jquery-1.6.4.js" />
/// <reference path="jquery.signalR-2.0.0.js" />
/// <reference path="common.js" />

function startSignalR(baseUrl) {
    var activeTransport = getQueryVariable('transport') || 'auto';

    var connection = $.connection.hub;
    connection.url = (baseUrl || "") + "/signalr";
    var hub = $.connection.seattleTimesHub;
    connection.logging = true;

    connection.connectionSlow(function () {
        writeEvent("connectionSlow");
    });

    connection.disconnected(function () {
        writeEvent("disconnected");
    });

    connection.error(function (error) {
        var innerError = error;
        var message = "";
        while (innerError) {
            message += " Message=" + innerError.message + " Stack=" + innerError.stack;
            innerError = innerError.source
        }
        writeError("Error: " + message);
    });

    connection.reconnected(function () {
        writeEvent("reconnected");
    });

    connection.reconnecting(function () {
        writeEvent("reconnecting");
    });

    connection.starting(function () {
        writeEvent("starting");
    });

    connection.received(function (data) {
        //writeLine("received: " + connection.json.stringify(data));
    });

    connection.stateChanged(function (change) {
        writeEvent("stateChanged: " + printState(change.oldState) + " => " + printState(change.newState));
    });

    hub.client.addNews = function (data) {
        var messages = $("#Messages");
        $.each(data, function (index, value) {
            var line = "<a href='" + value.link + "'>" + value.title + "</a><br />" + value.description;            
            messages.append("<li style='color:black;'>" + line + "</li>");
        });
    }

    connection.start({ transport: activeTransport })
        .done(function () {
            writeLine("start.done");
            writeLine("transport=" + connection.transport.name);
            hub.server.sync();
        })
        .fail(function (error) {
            writeError("start.fail " + error);
        });
}
