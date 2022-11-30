using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;


namespace r1g.plugins
{

	public class OggVorbis
	{
		// imported functions from plugin library
#if UNITY_IOS && !UNITY_EDITOR
		[DllImport ("__Internal")]
#else
		[DllImport ("test")]
#endif
		public static extern int Double(int num);

#if UNITY_IOS && !UNITY_EDITOR
		[DllImport ("__Internal")]
#else
		[DllImport ("test")]
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		public static extern int WriteVorbisData([MarshalAs(UnmanagedType.LPWStr)] byte[] filename, int srate, float q, [In, MarshalAs(UnmanagedType.LPArray)] float[] data, int count, int ch);
#else
		public static extern void WriteVorbisData(string filename, int srate, float q, [In, MarshalAs(UnmanagedType.LPArray)] float[] data, int count, int ch);
#endif
	
	}

}
