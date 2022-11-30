using UnityEngine;
using System.Collections;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System;

namespace r1g.plugins
{


    public class OggVorbisEnc
    {


        public delegate void Callback(bool success);

        private string _filename;
        private AudioClip _clip;
        private float _quality;
        private Callback _cb;
        private Thread _thread;
        private float[] _sampleData;
        private bool _isEncoding;
        private bool _isDone;

        private int _frequency;
        private int _samples;
        private int _channels;

        int retval = -1;


        public OggVorbisEnc(string filename, AudioClip clip, float quality, Callback cb)
        {
            _isDone = false;
            _isEncoding = false;

            _filename = filename;
            _clip = clip;
            _quality = quality;
            _cb = cb;

            _frequency = clip.frequency;
            _samples = clip.samples;
            _channels = clip.channels;
        }


        public void encode()
        {
            if (_isEncoding) return;

            // check if mono or stereo
            if (_clip.channels > 2 || _clip.channels <= 0)
            {
                Debug.Log("Error: only mono or stereo audio is supported");
                if (_cb != null) _cb(false);
                return;
            }

            // check if file is writable
            if (IsFileLocked(_filename))
            {
                Debug.LogError("Error: cannot write to file - " + _filename);
                if (_cb != null) _cb(false);
                return;
            }

            // encode the data
            Debug.Log("Ogg encoder: ac.samples=" + _clip.samples + " | ac.channels=" + _clip.channels + ": " + System.Environment.TickCount);

            _sampleData = new float[_clip.samples * _clip.channels];
            _clip.GetData(_sampleData, 0);

            _thread = new Thread(doEncode);
            _thread.IsBackground = true;
            _thread.Start();
        }


        private void doEncode()
        {


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
       Debug.Log("file name " + _filename);
                retval = OggVorbis.WriteVorbisData( System.Text.Encoding.Unicode.GetBytes(_filename + "\0"), _frequency, _quality, _sampleData, _samples, _channels);
#else
            OggVorbis.WriteVorbisData(_filename, _frequency, _quality, _sampleData, _samples, _channels);
#endif
            _isDone = true;

            //Debug.Log("setting done to true");
        }


        public bool update()
        {
            if (_isDone && _thread != null)
            {

                //Debug.Log("Ogg encoder: End Tick Count: " + System.Environment.TickCount);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (_cb != null && retval == 0)
                {
                    _cb(true);
                }
                else {
                    _cb(false);
                }
#else
                if (_cb != null)
                {
                    _cb(true);
                }
#endif

                _thread.Abort();
                _thread = null;

                return true;
            }

            return false;
        }


        public bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (Exception e)
            {
                if (e is IOException)
                {
                    var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
                    return errorCode == 32 || errorCode == 33;
                }
                else if (e is UnauthorizedAccessException)
                {
                    return true;
                }
            }

            return false;
        }


    }

}