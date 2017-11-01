using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using oi.core.network;

namespace oi.plugin.audio {
	
	// Parse & play audio stream from network
	[RequireComponent(typeof(UDPConnector))]
	[RequireComponent(typeof(AudioSource))]
	public class AudioReceive : MonoBehaviour {

   		AudioSource aud;
		private int clipFreq = -1;
		private int clipChan = -1;
		private int clipLen = 441000;
		private int lastSamplePos = 0;
		private UDPConnector oiudp;

		void Start () {
			aud = GetComponent<AudioSource>();
        	oiudp = GetComponent<UDPConnector>();
			aud.loop = true;
			oiudp.isSender = false;
		}
		
		void Update () {
			ParseData(oiudp);
			UpdatePlayer();
		}


		void UpdatePlayer() {
			if (!aud.isPlaying) return;
			if (aud.timeSamples > lastSamplePos && aud.timeSamples - lastSamplePos < 10000)
				aud.Pause();
		}

		private void ParseData(UDPConnector udpSource) {
			byte[] data = udpSource.GetNewData();

			while (data != null) {
				// Make sure there is data in the stream.
				if (data.Length != 0) {

					int packetID = -1;
					using (MemoryStream str = new MemoryStream(data)) {
						using (BinaryReader reader = new BinaryReader(str)) {
							packetID = reader.ReadInt32();
						}
					}

					if (packetID == 7) { // audio packet
						float[] samples;
						int freq;
						int chan;
						AudioSerializer.Deserialize(data, out samples, out freq, out chan);
						if (freq != -1 && chan != -1) {

							if (clipFreq != freq || clipChan != chan) {
								clipFreq = freq;
								clipChan = chan;
								aud.clip = AudioClip.Create("RemoteAudio",
									clipLen, clipChan, freq, false);
								lastSamplePos = 0;
								aud.timeSamples = 0;
							}

							aud.clip.SetData(samples, lastSamplePos);

							if (lastSamplePos > aud.timeSamples + 10000 ||
							   (lastSamplePos < aud.timeSamples && lastSamplePos > 10000
								&& aud.timeSamples < clipLen - 10000)) {
								aud.timeSamples = lastSamplePos;
							}
							if (!aud.isPlaying) aud.Play();

							lastSamplePos += samples.Length;
							if (lastSamplePos >= clipLen)
								lastSamplePos -= clipLen;
						}
					}
				}

				data = udpSource.GetNewData();
			}
		}

	}
}