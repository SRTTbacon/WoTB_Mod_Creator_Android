using System.Collections.Generic;
using System.IO;

namespace BNKManager
{
    public enum WwiseObjectType : byte
    {
        Settings = 1,
        Sound_SFX__Sound_Voice = 2,
        Event_Action = 3,
        Event = 4,
        Random_Container_or_Sequence_Container = 5,
        Switch_Container = 6,
        Actor_Mixer = 7,
        Audio_Bus = 8,
        Blend_Container = 9,
        Music_Segment = 10,
        Music_Track = 11,
        Music_Switch_Container = 12,
        Music_Playlist_Container = 13,
        Attenuation = 14,
        Dialogue_Event = 15,
        Motion_Bus = 16,
        Motion_FX = 17,
        Effect = 18,
        Auxiliary_Bus = 19,
    }
    public class WwiseObject
    {
        public WwiseObjectType objectType;
        public byte[] objectData;
        public WwiseObject(WwiseObjectType objectType, byte[] objectData)
        {
            this.objectType = objectType;
            this.objectData = objectData;
        }
        protected byte[] GetObjectBytes(byte[] objectData)
        {
            byte[] objectBytes = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write((byte)this.objectType);
                    bw.Write((uint)objectData.Length);
                    bw.Write(objectData);
                }
                objectBytes = mStream.ToArray();
            }
            return objectBytes;
        }
        public virtual byte[] GetBytes()
        {
            return GetObjectBytes(objectData);
        }
    }
    public class SoundSFXVoiceWwiseObject : WwiseObject
    {
        public uint ID;
        public byte[] unk;
        public StreamSound streamType;
        public uint audioFileID;
        public uint sourceID;
        public SFXVoice soundType;
        public byte[] soundStructure;
        public uint fileOffset;
        public uint fileLength;
        public SoundSFXVoiceWwiseObject(BinaryReader br, uint length) : base(WwiseObjectType.Sound_SFX__Sound_Voice, null)
        {
            int bytesRead = 0;
            ID = br.ReadUInt32();
            unk = br.ReadBytes(4);
            streamType = (StreamSound)br.ReadUInt32();
            audioFileID = br.ReadUInt32();
            sourceID = br.ReadUInt32();
            if (streamType == StreamSound.Embedded)
            {
                fileOffset = br.ReadUInt32();
                fileLength = br.ReadUInt32();
                bytesRead += 8;
            }
            soundType = (SFXVoice)br.ReadByte();
            bytesRead += 21;
            soundStructure = br.ReadBytes((int)(length - bytesRead));
        }
        public override byte[] GetBytes()
        {
            byte[] objData = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write(ID);
                    bw.Write(unk);
                    bw.Write((uint)streamType);
                    bw.Write(audioFileID);
                    bw.Write(sourceID);
                    if (streamType == StreamSound.Embedded)
                    {
                        bw.Write(fileOffset);
                        bw.Write(fileLength);
                    }
                    bw.Write((byte)soundType);
                    bw.Write(soundStructure);
                }
                objData = mStream.ToArray();
            }
            return GetObjectBytes(objData);
        }
        public enum StreamSound : uint
        {
            Embedded = 0,
            Streamed = 1,
            StreamedWithZeroLatency = 2,
        }
        public enum SFXVoice : byte
        {
            SFX = 0,
            Voice = 1,
        }
    }
    public class EventActionWwiseObject : WwiseObject
    {
        public uint ID;
        public byte scope;
        public byte type;
        public uint gameObject;
        public byte[] additional;
        public EventActionWwiseObject(BinaryReader br, uint length) : base(WwiseObjectType.Event_Action, null)
        {
            ID = br.ReadUInt32();
            scope = br.ReadByte();
            type = br.ReadByte();
            gameObject = br.ReadUInt32();
            additional = br.ReadBytes((int)length - 10);
        }
        public override byte[] GetBytes()
        {
            byte[] objData = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write(ID);
                    bw.Write(scope);
                    bw.Write(type);
                    bw.Write(gameObject);
                    bw.Write(additional);
                }
                objData = mStream.ToArray();
            }
            return GetObjectBytes(objData);
        }
    }
    //本家WoTのpckファイル読み込むとクラッシュするため廃止
    public class EventWwiseObject : WwiseObject
    {
        public uint ID;
        public List<uint> eventActionList = new List<uint>();
        public EventWwiseObject(BinaryReader br) : base(WwiseObjectType.Event, null)
        {
            ID = br.ReadUInt32();
            uint eventActionsCount = br.ReadUInt32();
            for (uint i = 0; i < eventActionsCount-1; i++)
                eventActionList.Add(br.ReadUInt32());
        }
        public override byte[] GetBytes()
        {
            byte[] objData = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write(ID);
                    bw.Write((uint)eventActionList.Count);
                    foreach (uint eventActionID in eventActionList)
                        bw.Write(eventActionID);
                }
                objData = mStream.ToArray();
            }
            return GetObjectBytes(objData);
        }
    }
}