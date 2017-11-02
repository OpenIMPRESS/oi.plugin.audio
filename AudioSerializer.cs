using System.Collections.Generic;
using SysDiag = System.Diagnostics;
using System.IO;
using System;
using UnityEngine;


namespace oi.plugin.audio {

    // Serialize/deserialize audio samples.
    public static class AudioSerializer {

        public static byte[] Serialize(float[] samples, int freq, int chan) {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream()) {
                using (BinaryWriter writer = new BinaryWriter(stream)) {

                    writer.Write(7); // '7' announces audio packet
                    writer.Write(freq);
                    writer.Write(chan);
                    writer.Write(samples.Length);
                    foreach (float sample in samples) {
                        writer.Write((Int16)(sample * 32767));
                    }

                    stream.Position = 0;
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                }
            }

            return data;
        }

        public static void Deserialize(byte[] data, out float[] samples, out int freq, out int chan) {
            samples = new float[0];
            freq = -1;
            chan = -1;

            using (MemoryStream stream = new MemoryStream(data)) {
                using (BinaryReader reader = new BinaryReader(stream)) {
                    int packetID = reader.ReadInt32();
                    if (packetID != 7) return;

                    freq = reader.ReadInt32();
                    chan = reader.ReadInt32();
                    int len = reader.ReadInt32();
                    samples = new float[len];
                    for(int i=0; i< len; i++) {
                        samples[i] = reader.ReadInt16() / 32767f;
                    }
                }
            }
        }

    }
}