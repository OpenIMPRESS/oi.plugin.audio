using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oi.plugin.audio {

	// Sample and audio from mic and stream to network
	[RequireComponent(typeof(IMPRESS_UDPClient))]
	public class AudioSend : MonoBehaviour {

		public int recFreq = 44100;
		private IMPRESS_UDPClient oiudp;
		private AudioClip mic;
		private int lastRecSample = 0;

		void Start () {
        	oiudp = GetComponent<IMPRESS_UDPClient>();
			oiudp.isSender = true;
			mic = Microphone.Start(null, true, 60, recFreq);
		}

		void Update() {
			SendMicSamples();
		}

		void SendMicSamples() {
			int pos = Microphone.GetPosition(null);
			int diff = pos - lastRecSample;
			if (diff > 0) {
				float[] samples = new float[diff * mic.channels];
				mic.GetData(samples, lastRecSample);
				byte[] serialized = AudioSerializer.Serialize(samples, recFreq, mic.channels);
				oiudp.SendData(serialized);
			}
			lastRecSample = pos;
		}
	}

}