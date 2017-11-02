using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using oi.core.network;

namespace oi.plugin.audio {

	// Sample and audio from mic and stream to network
	[RequireComponent(typeof(UDPConnector))]
	public class AudioSend : MonoBehaviour {

		public int recFreq = 44100;
		private UDPConnector oiudp;
		private AudioClip mic;
		private int lastRecSample = 0;

		void Start () {
        	oiudp = GetComponent<UDPConnector>();
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