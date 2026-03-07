using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class NetworkProvider : MonoBehaviour
{
	[SerializeField] private string _serverUrl = "ws://localhost:8080";
	[SerializeField] private CommandParser _parser;

	private ClientWebSocket _webSocket;
	private CancellationTokenSource _cts;
	private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
	private bool _isShuttingDown = false;
	private bool _quitOnConnectionLost = false;

	private void Awake()
	{
		if (_parser == null)
			_parser = FindObjectOfType<CommandParser>();
	}

	private async void Start()
	{
		await ConnectAsync();
	}

	private void Update()
	{
		if (_quitOnConnectionLost)
			Application.Quit();
	}

	private async Task ConnectAsync()
	{
		_cts = new CancellationTokenSource();
		_webSocket = new ClientWebSocket();

		try
		{
			Uri serverUri = new Uri(_serverUrl);

			Debug.Log("[Network] Connecting to: " + _serverUrl);
			await _webSocket.ConnectAsync(serverUri, _cts.Token);

			Debug.Log("[Network] Socket connected");

			_ = ReceiveLoop();
		}
		catch (Exception e)
		{
			Debug.LogError("[Network] Connection error: " + e.Message);
		}
	}

	private async Task ReceiveLoop()
	{
		byte[] buffer = new byte[2048];

		try
		{
			while (!_isShuttingDown && _webSocket != null && _webSocket.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result = await _webSocket.ReceiveAsync(
					new ArraySegment<byte>(buffer),
					_cts.Token
				);

				if (result.MessageType == WebSocketMessageType.Close)
				{
					Debug.Log("[Network] Server initiated close");

					await _webSocket.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						"Client closing after server close request",
						CancellationToken.None
					);

					break;
				}

				string rawMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
				Debug.Log("[Network] Received: " + rawMessage);

				if (_parser == null)
					_parser = FindObjectOfType<CommandParser>();
				if (_parser != null)
					_parser.ParseAndExecute(rawMessage);
				else
					Debug.LogError("[Network] No CommandParser in scene");
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("[Network] Receive loop cancelled");
		}
		catch (Exception e)
		{
			if (!_isShuttingDown)
			{
				Debug.LogError("[Network] Receive error: " + e.Message);
				_quitOnConnectionLost = true;
			}
		}
	}

	public async Task SendAsync(string json)
	{
		if (_webSocket == null || _webSocket.State != WebSocketState.Open)
		{
			Debug.LogWarning("[Network] Cannot send message, socket is not open");
			return;
		}

		await _sendLock.WaitAsync(_cts.Token);
		try
		{
			byte[] buffer = Encoding.UTF8.GetBytes(json);

			await _webSocket.SendAsync(
				new ArraySegment<byte>(buffer),
				WebSocketMessageType.Text,
				true,
				_cts.Token
			);

			Debug.Log("[Network] Sent: " + json);
		}
		catch (Exception e)
		{
			Debug.LogError("[Network] Send error: " + e.Message);
		}
		finally
		{
			_sendLock.Release();
		}
	}

	private async void OnApplicationQuit()
	{
		await ShutdownAsync();
	}

	private async Task ShutdownAsync()
	{
		_isShuttingDown = true;

		try
		{
			if (_cts != null && !_cts.IsCancellationRequested)
			{
				_cts.Cancel();
			}

			if (_webSocket != null)
			{
				if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
				{
					await _webSocket.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						"Application closing",
						CancellationToken.None
					);
				}

				_webSocket.Dispose();
				_webSocket = null;
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("[Network] Shutdown warning: " + e.Message);
		}
		finally
		{
			if (_cts != null)
			{
				_cts.Dispose();
				_cts = null;
			}
		}
	}
}