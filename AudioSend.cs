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
using oi.core.network;

namespace oi.plugin.audio {

    // Sample and audio from mic and stream to network
    [RequireComponent(typeof(UDPConnector))]
    public class AudioSend : MonoBehaviour {

        private UDPConnector oiudp;
        private AudioClip mic;
        private int lastRecSample = 0;
        private int pos;
        private int recFreq;


        void Start() {
            oiudp = GetComponent<UDPConnector>();
            int minFreq;
            int maxFreq;
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
            Debug.Log("oi.plugin.audio.AudioSend: Microphone minFreq: "+minFreq);
            if (minFreq == 0) recFreq = 16000;
            else recFreq = minFreq;
            mic = Microphone.Start(null, true, 60, recFreq);
        }

        void Update() {
            SendMicSamples();
        }

        void SendMicSamples() {
            pos = Microphone.GetPosition(null);
            int diff = pos - lastRecSample;
            if (diff < 0) diff = pos;
            if (diff > 0) {
                float[] samples = new float[diff * mic.channels];
                mic.GetData(samples, lastRecSample);
                byte[] serialized = AudioSerializer.Serialize(samples, recFreq, mic.channels);
                oiudp.SendData(serialized);
                lastRecSample = pos;
            }
        }
    }

}