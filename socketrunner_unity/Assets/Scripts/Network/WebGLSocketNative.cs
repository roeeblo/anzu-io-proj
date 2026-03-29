#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;

internal static class WebGLSocketNative
{
	[DllImport("__Internal")]
	internal static extern int SR_WS_Connect(string url);

	[DllImport("__Internal")]
	internal static extern int SR_WS_ReadyState(int id);

	/// <summary>Writes one UTF-8 message into buffer; returns byte length, 0 if empty, -1 if overflow.</summary>
	[DllImport("__Internal")]
	internal static extern int SR_WS_Dequeue(int id, byte[] buffer, int maxLen);

	[DllImport("__Internal")]
	internal static extern int SR_WS_SendStr(int id, string msg);

	[DllImport("__Internal")]
	internal static extern void SR_WS_Close(int id);
}
#endif
