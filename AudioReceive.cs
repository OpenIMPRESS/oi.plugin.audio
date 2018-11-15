/*
This file is part of the OpenIMPRESS project.

OpenIMPRESS is free software: you can redistribute it and/or modify
it under the terms of the Lesser GNU Lesser General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

OpenIMPRESS is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with OpenIMPRESS. If not, see <https://www.gnu.org/licenses/>.
*/

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
		}
		
		void Update () {
			ParseData(oiudp);
			UpdatePlayer();
		}


		void UpdatePlayer() {
			if (!aud.isPlaying) {
				return;
			}
			if (aud.timeSamples > lastSamplePos && aud.timeSamples - lastSamplePos < 10000) {
				aud.Pause();
			}
		}

		private void ParseData(UDPConnector udpSource) {
			OIMSG msg = udpSource.GetNewData();
			int packetsThisFrame = 0;

			while (msg != null && msg.data != null && msg.data.Length > 0) {
				// Make sure there is data in the stream.
				packetsThisFrame++;

				int packetID = -1;
				using (MemoryStream str = new MemoryStream(msg.data)) {
					using (BinaryReader reader = new BinaryReader(str)) {
						packetID = reader.ReadInt32();
					}
				}

				if (packetID == 7) { // audio packet
					float[] samples;
					int freq;
					int chan;
					AudioSerializer.Deserialize(msg.data, out samples, out freq, out chan);
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
				msg = udpSource.GetNewData();
			}

		}

	}
}