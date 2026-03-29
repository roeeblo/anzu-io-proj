using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Net.WebSockets;
#endif

public class NetworkProvider : MonoBehaviour
{
	[SerializeField] private string _serverUrl = "wss://socketrunner.onrender.com";
	[SerializeField] private CommandParser _parser;

#if UNITY_WEBGL && !UNITY_EDITOR
	private int _webGlSocketId = -1;
#else
	private ClientWebSocket _webSocket;
#endif
	private CancellationTokenSource _cts;
	private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
	private bool _isShuttingDown = false;
	private bool _quitOnConnectionLost = false;

	private void Awake()
	{
		if (_parser == null)
			_parser = FindObjectOfType<CommandParser>();
	}

	private void Start()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		StartCoroutine(ConnectWebGLRoutine());
#else
		ConnectAsync();
#endif
	}

	private void Update()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		PollWebGLMessages();
#endif
		if (_quitOnConnectionLost)
			Application.Quit();
	}

#if UNITY_WEBGL && !UNITY_EDITOR

	private IEnumerator ConnectWebGLRoutine()
	{
		_cts = new CancellationTokenSource();
		Debug.Log("[Network] Connecting to: " + _serverUrl);
		_webGlSocketId = WebGLSocketNative.SR_WS_Connect(_serverUrl);
		if (_webGlSocketId < 0)
		{
			Debug.LogError("[Network] Connection failed (jslib)");
			yield break;
		}

		float deadline = Time.realtimeSinceStartup + 30f;
		while (Time.realtimeSinceStartup < deadline)
		{
			if (_isShuttingDown)
				yield break;

			int st = WebGLSocketNative.SR_WS_ReadyState(_webGlSocketId);
			if (st == 1)
			{
				Debug.Log("[Network] Socket connected (WebGL)");
				yield break;
			}
			if (st == 2 || st == 3)
			{
				Debug.LogError("[Network] WebSocket closed before open");
				WebGLSocketNative.SR_WS_Close(_webGlSocketId);
				_webGlSocketId = -1;
				yield break;
			}
			yield return null;
		}

		Debug.LogError("[Network] WebSocket connect timeout");
		WebGLSocketNative.SR_WS_Close(_webGlSocketId);
		_webGlSocketId = -1;
	}

	private void PollWebGLMessages()
	{
		if (_parser == null)
			_parser = FindObjectOfType<CommandParser>();
		if (_webGlSocketId < 0)
			return;

		int st = WebGLSocketNative.SR_WS_ReadyState(_webGlSocketId);
		if (st != 1)
		{
			if (st == 2 || st == 3)
			{
				if (!_isShuttingDown)
				{
					Debug.Log("[Network] WebSocket closed (WebGL)");
					_quitOnConnectionLost = true;
				}
			}
			return;
		}

		byte[] buffer = new byte[2048];
		while (true)
		{
			int n = WebGLSocketNative.SR_WS_Dequeue(_webGlSocketId, buffer, buffer.Length);
			if (n == 0)
				break;
			if (n < 0)
			{
				Debug.LogError("[Network] Message too large for buffer");
				break;
			}

			string rawMessage = Encoding.UTF8.GetString(buffer, 0, n);
			Debug.Log("[Network] Received: " + rawMessage);

			if (_parser == null)
				_parser = FindObjectOfType<CommandParser>();
			if (_parser != null)
				_parser.ParseAndExecute(rawMessage);
			else
				Debug.LogError("[Network] No CommandParser in scene");
		}
	}

#else

	private async void ConnectAsync()
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

#endif

	public async Task SendAsync(string json)
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		if (_webGlSocketId < 0 || WebGLSocketNative.SR_WS_ReadyState(_webGlSocketId) != 1)
		{
			Debug.LogWarning("[Network] Cannot send message, socket is not open");
			return;
		}

		await _sendLock.WaitAsync(_cts.Token);
		try
		{
			WebGLSocketNative.SR_WS_SendStr(_webGlSocketId, json);
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
#else
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
#endif
	}

	private void OnApplicationQuit()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		ShutdownWebGL();
#else
		ShutdownAsync();
#endif
	}

#if UNITY_WEBGL && !UNITY_EDITOR

	private void ShutdownWebGL()
	{
		_isShuttingDown = true;
		try
		{
			if (_cts != null && !_cts.IsCancellationRequested)
				_cts.Cancel();
			if (_webGlSocketId >= 0)
			{
				WebGLSocketNative.SR_WS_Close(_webGlSocketId);
				_webGlSocketId = -1;
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

#else

	private async void ShutdownAsync()
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

#endif
}
