// src/services/signalr-connection.js

import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
  .withUrl("http://localhost:5087/taskHub", {
    skipNegotiation: true,
    transport: HttpTransportType.WebSockets
  })
  .withAutomaticReconnect()
  .build();

export const startConnection = async () => {
  if (connection.state === "Disconnected") {
    try {
      await connection.start();
      console.log("SignalR connected.");
    } catch (err) {
      console.error("Error connecting SignalR:", err);
      // Try reconnecting in 5 seconds
      setTimeout(() => startConnection(), 5000);
    }
  }
};

export default connection;
