const WebSocket = require("ws");

const server = new WebSocket.Server({ port: 8080 });
const clients = new Map();

server.on("connection", (socket) => {
    console.log("Client connected.");

    // Store client in the map
    clients.set(socket, {});
    
    socket.on("message", (message) => {
        let messageStr;
        if (Buffer.isBuffer(message)) {
            messageStr = message.toString('utf-8');
        } else {
            messageStr = message;
        }

        console.log("Message received");

        if (messageStr.includes("!")) {
            const [messageType, jsonData] = messageStr.split("!");

            try {
                for (const [client] of clients) {
                    if (client !== socket) {
                        client.send(messageStr);
                    }
                }
            } catch (error) {
                console.error("Failed to parse JSON:", error);
            }
        } else {
            console.error("Invalid message format, no '!' found.");
        }
    });

    socket.on("error", (error) => {
        console.error("WebSocket error:", error);
    });

    socket.on("close", () => {
        console.log("Client disconnected.");
        clients.delete(socket);
    });
});
