using System;
using System.IO;

namespace Nucleus {
    public static class BinaryReaderExtensions {
        public static bool ReadNucleusHeader(this BinaryReader reader) {
            char N = reader.ReadChar();
            char U = reader.ReadChar();
            char K = reader.ReadChar();
            char E = reader.ReadChar();

            if (N != 'N' || U != 'U' ||
                K != 'K' || E != 'E') {
                throw new Exception(ResourceStrings.WrongHeaderNotNucleus);
            } else {
                return true;
            }
        }

        public static bool ReadNucleusVideoHeader(this BinaryReader reader) {
            char V = reader.ReadChar();
            char I = reader.ReadChar();
            char D = reader.ReadChar();

            if (V != 'V' ||
                I != 'I' || D != 'D') {
                throw new Exception(ResourceStrings.WrongHeaderNotNucleusVideo);
            } else {
                return true;
            }
        }

        public static UInt32 ReadNucleusVersion(this BinaryReader reader) {
            return reader.ReadUInt32();
        }
    }
}
