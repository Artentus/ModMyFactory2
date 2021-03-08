//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.Server.Rcon
{
    /// <summary>
    /// Allows connecting to an RCON server
    /// </summary>
    public class RconClient : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly SemaphoreSlim _syncSemaphore;
        private NetworkStream? _stream;
        private int _currentId;

        /// <summary>
        /// Whether the client is connected to a server
        /// </summary>
        public bool Connected => _tcpClient.Connected;

        public RconClient()
        {
            _tcpClient = new TcpClient();
            _syncSemaphore = new SemaphoreSlim(0, 1);
            _currentId = 1;
        }

        // Wait for a packet with a specific ID
        private async Task<Packet> WaitForResponseAsync(int id, int timeout)
        {
            _stream!.ReadTimeout = timeout;

            // We wait until we receive more data using polling
            while (true)
            {
                while (_stream.DataAvailable)
                {
                    var packet = await _stream.ReadPacketAsync();
                    if (packet.Id == id) return packet;
                }

                // Add some delay to save on CPU cycles
                await Task.Delay(100);
            }
        }

        // Wait for the next packet
        private async Task<Packet> WaitForNextResponseAsync(int timeout)
        {
            _stream!.ReadTimeout = timeout;

            // We wait until we receive more data using polling
            while (true)
            {
                if (_stream.DataAvailable)
                    return await _stream.ReadPacketAsync();

                // Add some delay to save on CPU cycles
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Creates a new packet of specified type with specified body
        /// </summary>
        protected virtual Packet CreatePacket(PacketType type, string body)
        {
            int id = _currentId;

            _currentId++;
            if (_currentId < 0) _currentId = 1; // Negatives are not allowed

            return new Packet(id, type, body);
        }

        /// <summary>
        /// Connects to the specified TCP port on the specified host
        /// </summary>
        public async Task ConnectAsync(string hostname, Port port)
        {
            if (string.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port.IsNone) throw new ArgumentException("Port must be defined", nameof(port));

            await _tcpClient.ConnectAsync(hostname, port.Number);
            _stream = _tcpClient.GetStream();
        }

        /// <summary>
        /// Sends a packet to the server and returns the response packet
        /// </summary>
        public async Task<Packet> SendPacketAsync(Packet packet, int timeout = Timeout.Infinite)
        {
            await _syncSemaphore.WaitAsync();
            try
            {
                await _stream!.WriteAsync(packet);
                return await WaitForResponseAsync(packet.Id, timeout);
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        /// <summary>
        /// Authenticates on the server using the specified password and returns if the authentication was successfull
        /// </summary>
        public async Task<bool> AuthenticateAsync(string password, int timeout = Timeout.Infinite)
        {
            var packet = CreatePacket(PacketType.Auth, password);
            await _syncSemaphore.WaitAsync();
            try
            {
                await _stream!.WriteAsync(packet);

                // When authenticating we have to wait for two responses
                // The first is a normal response containing the ID
                // The second is an auth response indicating success or failure

                var first = await WaitForResponseAsync(packet.Id, timeout);
                if (first.Type != PacketType.ResponseValue)
                    throw new PacketTypeException(first);

                var second = await WaitForNextResponseAsync(timeout);
                if (second.Type != PacketType.AuthResponse)
                    throw new PacketTypeException(second);

                // And ID identical to that of the request means success, -1 means failure
                return second.Id == packet.Id;
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        /// <summary>
        /// Sends a command to the server and returns the response
        /// </summary>
        public async Task<string> SendCommandAsync(string command, int timeout = Timeout.Infinite)
        {
            var packet = CreatePacket(PacketType.ExecuteCommand, command);
            var response = await SendPacketAsync(packet, timeout);
            if (response.Type != PacketType.ResponseValue)
                throw new PacketTypeException(response);
            return response.Body;
        }

        /// <summary>
        /// Closes the TCP connection
        /// </summary>
        public void Close()
        {
            if (Connected)
            {
                _syncSemaphore.Wait();

                _stream?.Close();
                _tcpClient.Close();

                _syncSemaphore.Release();
            }
        }

        #region IDisposable

        private bool _disposed;

        ~RconClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _syncSemaphore.Wait();

                    _stream?.Dispose();
                    _tcpClient.Dispose();

                    _syncSemaphore.Release();
                    _syncSemaphore.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
