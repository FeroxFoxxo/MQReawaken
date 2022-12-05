using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetStudio;

public class Keyframe<T>
{
    public T inSlope;
    public T inWeight;
    public T outSlope;
    public T outWeight;
    public float time;
    public T value;
    public int weightedMode;


    public Keyframe(ObjectReader reader, Func<T> readerFunc)
    {
        time = reader.ReadSingle();
        value = readerFunc();
        inSlope = readerFunc();
        outSlope = readerFunc();
        if (reader.version[0] >= 2018) //2018 and up
        {
            weightedMode = reader.ReadInt32();
            inWeight = readerFunc();
            outWeight = readerFunc();
        }
    }
}

public class AnimationCurve<T>
{
    public Keyframe<T>[] m_Curve;
    public int m_PostInfinity;
    public int m_PreInfinity;
    public int m_RotationOrder;

    public AnimationCurve(ObjectReader reader, Func<T> readerFunc)
    {
        var version = reader.version;
        var numCurves = reader.ReadInt32();
        m_Curve = new Keyframe<T>[numCurves];
        for (var i = 0; i < numCurves; i++)
            m_Curve[i] = new Keyframe<T>(reader, readerFunc);

        m_PreInfinity = reader.ReadInt32();
        m_PostInfinity = reader.ReadInt32();
        if (version[0] > 5 || version[0] == 5 && version[1] >= 3) //5.3 and up
            m_RotationOrder = reader.ReadInt32();
    }
}

public class QuaternionCurve
{
    public AnimationCurve<Quaternion> curve;
    public string path;

    public QuaternionCurve(ObjectReader reader)
    {
        curve = new AnimationCurve<Quaternion>(reader, reader.ReadQuaternion);
        path = reader.ReadAlignedString();
    }
}

public class PackedFloatVector
{
    public byte m_BitSize;
    public byte[] m_Data;
    public uint m_NumItems;
    public float m_Range;
    public float m_Start;

    public PackedFloatVector(ObjectReader reader)
    {
        m_NumItems = reader.ReadUInt32();
        m_Range = reader.ReadSingle();
        m_Start = reader.ReadSingle();

        var numData = reader.ReadInt32();
        m_Data = reader.ReadBytes(numData);
        reader.AlignStream();

        m_BitSize = reader.ReadByte();
        reader.AlignStream();
    }

    public float[] UnpackFloats(int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
    {
        var bitPos = m_BitSize * start;
        var indexPos = bitPos / 8;
        bitPos %= 8;

        var scale = 1.0f / m_Range;
        if (numChunks == -1)
            numChunks = (int)m_NumItems / itemCountInChunk;
        var end = chunkStride * numChunks / 4;
        var data = new List<float>();
        for (var index = 0; index != end; index += chunkStride / 4)
        {
            for (var i = 0; i < itemCountInChunk; ++i)
            {
                uint x = 0;

                var bits = 0;
                while (bits < m_BitSize)
                {
                    x |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                    var num = Math.Min(m_BitSize - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }

                x &= (uint)(1 << m_BitSize) - 1u;
                data.Add(x / (scale * ((1 << m_BitSize) - 1)) + m_Start);
            }
        }

        return data.ToArray();
    }
}

public class PackedIntVector
{
    public byte m_BitSize;
    public byte[] m_Data;
    public uint m_NumItems;

    public PackedIntVector(ObjectReader reader)
    {
        m_NumItems = reader.ReadUInt32();

        var numData = reader.ReadInt32();
        m_Data = reader.ReadBytes(numData);
        reader.AlignStream();

        m_BitSize = reader.ReadByte();
        reader.AlignStream();
    }

    public int[] UnpackInts()
    {
        var data = new int[m_NumItems];
        var indexPos = 0;
        var bitPos = 0;
        for (var i = 0; i < m_NumItems; i++)
        {
            var bits = 0;
            data[i] = 0;
            while (bits < m_BitSize)
            {
                data[i] |= (m_Data[indexPos] >> bitPos) << bits;
                var num = Math.Min(m_BitSize - bits, 8 - bitPos);
                bitPos += num;
                bits += num;
                if (bitPos == 8)
                {
                    indexPos++;
                    bitPos = 0;
                }
            }

            data[i] &= (1 << m_BitSize) - 1;
        }

        return data;
    }
}

public class PackedQuatVector
{
    public byte[] m_Data;
    public uint m_NumItems;

    public PackedQuatVector(ObjectReader reader)
    {
        m_NumItems = reader.ReadUInt32();

        var numData = reader.ReadInt32();
        m_Data = reader.ReadBytes(numData);

        reader.AlignStream();
    }

    public Quaternion[] UnpackQuats()
    {
        var data = new Quaternion[m_NumItems];
        var indexPos = 0;
        var bitPos = 0;

        for (var i = 0; i < m_NumItems; i++)
        {
            uint flags = 0;

            var bits = 0;
            while (bits < 3)
            {
                flags |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                var num = Math.Min(3 - bits, 8 - bitPos);
                bitPos += num;
                bits += num;
                if (bitPos == 8)
                {
                    indexPos++;
                    bitPos = 0;
                }
            }

            flags &= 7;


            var q = new Quaternion();
            float sum = 0;
            for (var j = 0; j < 4; j++)
            {
                if ((flags & 3) != j)
                {
                    var bitSize = ((flags & 3) + 1) % 4 == j ? 9 : 10;
                    uint x = 0;

                    bits = 0;
                    while (bits < bitSize)
                    {
                        x |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                        var num = Math.Min(bitSize - bits, 8 - bitPos);
                        bitPos += num;
                        bits += num;
                        if (bitPos == 8)
                        {
                            indexPos++;
                            bitPos = 0;
                        }
                    }

                    x &= (uint)((1 << bitSize) - 1);
                    q[j] = x / (0.5f * ((1 << bitSize) - 1)) - 1;
                    sum += q[j] * q[j];
                }
            }

            var lastComponent = (int)(flags & 3);
            q[lastComponent] = (float)Math.Sqrt(1 - sum);
            if ((flags & 4) != 0u)
                q[lastComponent] = -q[lastComponent];
            data[i] = q;
        }

        return data;
    }
}

public class CompressedAnimationCurve
{
    public string m_Path;
    public int m_PostInfinity;
    public int m_PreInfinity;
    public PackedFloatVector m_Slopes;
    public PackedIntVector m_Times;
    public PackedQuatVector m_Values;

    public CompressedAnimationCurve(ObjectReader reader)
    {
        m_Path = reader.ReadAlignedString();
        m_Times = new PackedIntVector(reader);
        m_Values = new PackedQuatVector(reader);
        m_Slopes = new PackedFloatVector(reader);
        m_PreInfinity = reader.ReadInt32();
        m_PostInfinity = reader.ReadInt32();
    }
}

public class Vector3Curve
{
    public AnimationCurve<Vector3> curve;
    public string path;

    public Vector3Curve(ObjectReader reader)
    {
        curve = new AnimationCurve<Vector3>(reader, reader.ReadVector3);
        path = reader.ReadAlignedString();
    }
}

public class FloatCurve
{
    public string attribute;
    public ClassIDType classID;
    public AnimationCurve<float> curve;
    public string path;
    public PPtr<MonoScript> script;


    public FloatCurve(ObjectReader reader)
    {
        curve = new AnimationCurve<float>(reader, reader.ReadSingle);
        attribute = reader.ReadAlignedString();
        path = reader.ReadAlignedString();
        classID = (ClassIDType)reader.ReadInt32();
        script = new PPtr<MonoScript>(reader);
    }
}

public class PPtrKeyframe
{
    public float time;
    public PPtr<Object> value;


    public PPtrKeyframe(ObjectReader reader)
    {
        time = reader.ReadSingle();
        value = new PPtr<Object>(reader);
    }
}

public class PPtrCurve
{
    public string attribute;
    public int classID;
    public PPtrKeyframe[] curve;
    public string path;
    public PPtr<MonoScript> script;


    public PPtrCurve(ObjectReader reader)
    {
        var numCurves = reader.ReadInt32();
        curve = new PPtrKeyframe[numCurves];
        for (var i = 0; i < numCurves; i++)
            curve[i] = new PPtrKeyframe(reader);

        attribute = reader.ReadAlignedString();
        path = reader.ReadAlignedString();
        classID = reader.ReadInt32();
        script = new PPtr<MonoScript>(reader);
    }
}

public class AABB
{
    public Vector3 m_Center;
    public Vector3 m_Extent;

    public AABB(ObjectReader reader)
    {
        m_Center = reader.ReadVector3();
        m_Extent = reader.ReadVector3();
    }
}

public class xform
{
    public Quaternion q;
    public Vector3 s;
    public Vector3 t;

    public xform(ObjectReader reader)
    {
        var version = reader.version;
        t = version[0] > 5 || version[0] == 5 && version[1] >= 4
            ? reader.ReadVector3()
            : reader.ReadVector4(); //5.4 and up
        q = reader.ReadQuaternion();
        s = version[0] > 5 || version[0] == 5 && version[1] >= 4
            ? reader.ReadVector3()
            : reader.ReadVector4(); //5.4 and up
    }
}

public class HandPose
{
    public float m_CloseOpen;
    public float[] m_DoFArray;
    public float m_Grab;
    public xform m_GrabX;
    public float m_InOut;
    public float m_Override;

    public HandPose(ObjectReader reader)
    {
        m_GrabX = new xform(reader);
        m_DoFArray = reader.ReadSingleArray();
        m_Override = reader.ReadSingle();
        m_CloseOpen = reader.ReadSingle();
        m_InOut = reader.ReadSingle();
        m_Grab = reader.ReadSingle();
    }
}

public class HumanGoal
{
    public Vector3 m_HintT;
    public float m_HintWeightT;
    public float m_WeightR;
    public float m_WeightT;
    public xform m_X;

    public HumanGoal(ObjectReader reader)
    {
        var version = reader.version;
        m_X = new xform(reader);
        m_WeightT = reader.ReadSingle();
        m_WeightR = reader.ReadSingle();
        if (version[0] >= 5) //5.0 and up
        {
            m_HintT = version[0] > 5 || version[0] == 5 && version[1] >= 4
                ? reader.ReadVector3()
                : reader.ReadVector4(); //5.4 and up
            m_HintWeightT = reader.ReadSingle();
        }
    }
}

public class HumanPose
{
    public float[] m_DoFArray;
    public HumanGoal[] m_GoalArray;
    public HandPose m_LeftHandPose;
    public Vector3 m_LookAtPosition;
    public Vector4 m_LookAtWeight;
    public HandPose m_RightHandPose;
    public xform m_RootX;
    public Vector3[] m_TDoFArray;

    public HumanPose(ObjectReader reader)
    {
        var version = reader.version;
        m_RootX = new xform(reader);
        m_LookAtPosition = version[0] > 5 || version[0] == 5 && version[1] >= 4
            ? reader.ReadVector3()
            : reader.ReadVector4(); //5.4 and up
        m_LookAtWeight = reader.ReadVector4();

        var numGoals = reader.ReadInt32();
        m_GoalArray = new HumanGoal[numGoals];
        for (var i = 0; i < numGoals; i++)
            m_GoalArray[i] = new HumanGoal(reader);

        m_LeftHandPose = new HandPose(reader);
        m_RightHandPose = new HandPose(reader);

        m_DoFArray = reader.ReadSingleArray();

        if (version[0] > 5 || version[0] == 5 && version[1] >= 2) //5.2 and up
        {
            var numTDof = reader.ReadInt32();
            m_TDoFArray = new Vector3[numTDof];
            for (var i = 0; i < numTDof; i++)
                m_TDoFArray[i] = version[0] > 5 || version[0] == 5 && version[1] >= 4
                    ? reader.ReadVector3()
                    : reader.ReadVector4(); //5.4 and up
        }
    }
}

public class StreamedClip
{
    public uint curveCount;
    public uint[] data;

    public StreamedClip(ObjectReader reader)
    {
        data = reader.ReadUInt32Array();
        curveCount = reader.ReadUInt32();
    }

    public List<StreamedFrame> ReadData()
    {
        var frameList = new List<StreamedFrame>();
        var buffer = new byte[data.Length * 4];
        Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);
        using (var reader = new BinaryReader(new MemoryStream(buffer)))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
                frameList.Add(new StreamedFrame(reader));
        }

        for (var frameIndex = 2; frameIndex < frameList.Count - 1; frameIndex++)
        {
            var frame = frameList[frameIndex];
            foreach (var curveKey in frame.keyList)
            {
                for (var i = frameIndex - 1; i >= 0; i--)
                {
                    var preFrame = frameList[i];
                    var preCurveKey = preFrame.keyList.FirstOrDefault(x => x.index == curveKey.index);
                    if (preCurveKey != null)
                    {
                        curveKey.inSlope = preCurveKey.CalculateNextInSlope(frame.time - preFrame.time, curveKey);
                        break;
                    }
                }
            }
        }

        return frameList;
    }

    public class StreamedCurveKey
    {
        public float[] coeff;
        public int index;
        public float inSlope;
        public float outSlope;

        public float value;

        public StreamedCurveKey(BinaryReader reader)
        {
            index = reader.ReadInt32();
            coeff = reader.ReadSingleArray(4);

            outSlope = coeff[2];
            value = coeff[3];
        }

        public float CalculateNextInSlope(float dx, StreamedCurveKey rhs)
        {
            //Stepped
            if (coeff[0] == 0f && coeff[1] == 0f && coeff[2] == 0f)
                return float.PositiveInfinity;

            dx = Math.Max(dx, 0.0001f);
            var dy = rhs.value - value;
            var length = 1.0f / (dx * dx);
            var d1 = outSlope * dx;
            var d2 = dy + dy + dy - d1 - d1 - coeff[1] / length;
            return d2 / dx;
        }
    }

    public class StreamedFrame
    {
        public StreamedCurveKey[] keyList;
        public float time;

        public StreamedFrame(BinaryReader reader)
        {
            time = reader.ReadSingle();

            var numKeys = reader.ReadInt32();
            keyList = new StreamedCurveKey[numKeys];
            for (var i = 0; i < numKeys; i++)
                keyList[i] = new StreamedCurveKey(reader);
        }
    }
}

public class DenseClip
{
    public float m_BeginTime;
    public uint m_CurveCount;
    public int m_FrameCount;
    public float[] m_SampleArray;
    public float m_SampleRate;

    public DenseClip(ObjectReader reader)
    {
        m_FrameCount = reader.ReadInt32();
        m_CurveCount = reader.ReadUInt32();
        m_SampleRate = reader.ReadSingle();
        m_BeginTime = reader.ReadSingle();
        m_SampleArray = reader.ReadSingleArray();
    }
}

public class ConstantClip
{
    public float[] data;

    public ConstantClip(ObjectReader reader) => data = reader.ReadSingleArray();
}

public class ValueConstant
{
    public uint m_ID;
    public uint m_Index;
    public uint m_Type;
    public uint m_TypeID;

    public ValueConstant(ObjectReader reader)
    {
        var version = reader.version;
        m_ID = reader.ReadUInt32();
        if (version[0] < 5 || version[0] == 5 && version[1] < 5) //5.5 down
            m_TypeID = reader.ReadUInt32();
        m_Type = reader.ReadUInt32();
        m_Index = reader.ReadUInt32();
    }
}

public class ValueArrayConstant
{
    public ValueConstant[] m_ValueArray;

    public ValueArrayConstant(ObjectReader reader)
    {
        var numVals = reader.ReadInt32();
        m_ValueArray = new ValueConstant[numVals];
        for (var i = 0; i < numVals; i++)
            m_ValueArray[i] = new ValueConstant(reader);
    }
}

public class Clip
{
    public ValueArrayConstant m_Binding;
    public ConstantClip m_ConstantClip;
    public DenseClip m_DenseClip;
    public StreamedClip m_StreamedClip;

    public Clip(ObjectReader reader)
    {
        var version = reader.version;
        m_StreamedClip = new StreamedClip(reader);
        m_DenseClip = new DenseClip(reader);
        if (version[0] > 4 || version[0] == 4 && version[1] >= 3) //4.3 and up
            m_ConstantClip = new ConstantClip(reader);
        if (version[0] < 2018 || version[0] == 2018 && version[1] < 3) //2018.3 down
            m_Binding = new ValueArrayConstant(reader);
    }

    public AnimationClipBindingConstant ConvertValueArrayToGenericBinding()
    {
        var bindings = new AnimationClipBindingConstant();
        var genericBindings = new List<GenericBinding>();
        var values = m_Binding;
        for (var i = 0; i < values.m_ValueArray.Length;)
        {
            var curveID = values.m_ValueArray[i].m_ID;
            var curveTypeID = values.m_ValueArray[i].m_TypeID;
            var binding = new GenericBinding();
            genericBindings.Add(binding);
            if (curveTypeID == 4174552735) //CRC(PositionX))
            {
                binding.path = curveID;
                binding.attribute = 1; //kBindTransformPosition
                binding.typeID = ClassIDType.Transform;
                i += 3;
            }
            else if (curveTypeID == 2211994246) //CRC(QuaternionX))
            {
                binding.path = curveID;
                binding.attribute = 2; //kBindTransformRotation
                binding.typeID = ClassIDType.Transform;
                i += 4;
            }
            else if (curveTypeID == 1512518241) //CRC(ScaleX))
            {
                binding.path = curveID;
                binding.attribute = 3; //kBindTransformScale
                binding.typeID = ClassIDType.Transform;
                i += 3;
            }
            else
            {
                binding.typeID = ClassIDType.Animator;
                binding.path = 0;
                binding.attribute = curveID;
                i++;
            }
        }

        bindings.genericBindings = genericBindings.ToArray();
        return bindings;
    }
}

public class ValueDelta
{
    public float m_Start;
    public float m_Stop;

    public ValueDelta(ObjectReader reader)
    {
        m_Start = reader.ReadSingle();
        m_Stop = reader.ReadSingle();
    }
}

public class ClipMuscleConstant
{
    public float m_AverageAngularSpeed;
    public Vector3 m_AverageSpeed;
    public Clip m_Clip;
    public float m_CycleOffset;
    public HumanPose m_DeltaPose;
    public bool m_HeightFromFeet;
    public int[] m_IndexArray;
    public bool m_KeepOriginalOrientation;
    public bool m_KeepOriginalPositionXZ;
    public bool m_KeepOriginalPositionY;
    public xform m_LeftFootStartX;
    public float m_Level;
    public bool m_LoopBlend;
    public bool m_LoopBlendOrientation;
    public bool m_LoopBlendPositionXZ;
    public bool m_LoopBlendPositionY;
    public bool m_LoopTime;
    public bool m_Mirror;
    public xform m_MotionStartX;
    public xform m_MotionStopX;
    public float m_OrientationOffsetY;
    public xform m_RightFootStartX;
    public bool m_StartAtOrigin;
    public float m_StartTime;
    public xform m_StartX;
    public float m_StopTime;
    public xform m_StopX;
    public ValueDelta[] m_ValueArrayDelta;
    public float[] m_ValueArrayReferencePose;

    public ClipMuscleConstant(ObjectReader reader)
    {
        var version = reader.version;
        m_DeltaPose = new HumanPose(reader);
        m_StartX = new xform(reader);
        if (version[0] > 5 || version[0] == 5 && version[1] >= 5) //5.5 and up
            m_StopX = new xform(reader);
        m_LeftFootStartX = new xform(reader);
        m_RightFootStartX = new xform(reader);
        if (version[0] < 5) //5.0 down
        {
            m_MotionStartX = new xform(reader);
            m_MotionStopX = new xform(reader);
        }

        m_AverageSpeed = version[0] > 5 || version[0] == 5 && version[1] >= 4
            ? reader.ReadVector3()
            : reader.ReadVector4(); //5.4 and up
        m_Clip = new Clip(reader);
        m_StartTime = reader.ReadSingle();
        m_StopTime = reader.ReadSingle();
        m_OrientationOffsetY = reader.ReadSingle();
        m_Level = reader.ReadSingle();
        m_CycleOffset = reader.ReadSingle();
        m_AverageAngularSpeed = reader.ReadSingle();

        m_IndexArray = reader.ReadInt32Array();
        if (version[0] < 4 || version[0] == 4 && version[1] < 3) //4.3 down
        {
            var m_AdditionalCurveIndexArray = reader.ReadInt32Array();
        }

        var numDeltas = reader.ReadInt32();
        m_ValueArrayDelta = new ValueDelta[numDeltas];
        for (var i = 0; i < numDeltas; i++)
            m_ValueArrayDelta[i] = new ValueDelta(reader);
        if (version[0] > 5 || version[0] == 5 && version[1] >= 3) //5.3 and up
            m_ValueArrayReferencePose = reader.ReadSingleArray();

        m_Mirror = reader.ReadBoolean();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 3) //4.3 and up
            m_LoopTime = reader.ReadBoolean();
        m_LoopBlend = reader.ReadBoolean();
        m_LoopBlendOrientation = reader.ReadBoolean();
        m_LoopBlendPositionY = reader.ReadBoolean();
        m_LoopBlendPositionXZ = reader.ReadBoolean();
        if (version[0] > 5 || version[0] == 5 && version[1] >= 5) //5.5 and up
            m_StartAtOrigin = reader.ReadBoolean();
        m_KeepOriginalOrientation = reader.ReadBoolean();
        m_KeepOriginalPositionY = reader.ReadBoolean();
        m_KeepOriginalPositionXZ = reader.ReadBoolean();
        m_HeightFromFeet = reader.ReadBoolean();
        reader.AlignStream();
    }
}

public class GenericBinding
{
    public uint attribute;
    public byte customType;
    public byte isIntCurve;
    public byte isPPtrCurve;
    public uint path;
    public PPtr<Object> script;
    public ClassIDType typeID;

    public GenericBinding()
    {
    }

    public GenericBinding(ObjectReader reader)
    {
        var version = reader.version;
        path = reader.ReadUInt32();
        attribute = reader.ReadUInt32();
        script = new PPtr<Object>(reader);
        if (version[0] > 5 || version[0] == 5 && version[1] >= 6) //5.6 and up
            typeID = (ClassIDType)reader.ReadInt32();
        else
            typeID = (ClassIDType)reader.ReadUInt16();
        customType = reader.ReadByte();
        isPPtrCurve = reader.ReadByte();
        if (version[0] > 2022 || version[0] == 2022 && version[1] >= 1) //2022.1 and up
            isIntCurve = reader.ReadByte();
        reader.AlignStream();
    }
}

public class AnimationClipBindingConstant
{
    public GenericBinding[] genericBindings;
    public PPtr<Object>[] pptrCurveMapping;

    public AnimationClipBindingConstant()
    {
    }

    public AnimationClipBindingConstant(ObjectReader reader)
    {
        var numBindings = reader.ReadInt32();
        genericBindings = new GenericBinding[numBindings];
        for (var i = 0; i < numBindings; i++)
            genericBindings[i] = new GenericBinding(reader);

        var numMappings = reader.ReadInt32();
        pptrCurveMapping = new PPtr<Object>[numMappings];
        for (var i = 0; i < numMappings; i++)
            pptrCurveMapping[i] = new PPtr<Object>(reader);
    }

    public GenericBinding FindBinding(int index)
    {
        var curves = 0;
        foreach (var b in genericBindings)
        {
            if (b.typeID == ClassIDType.Transform)
                switch (b.attribute)
                {
                    case 1: //kBindTransformPosition
                    case 3: //kBindTransformScale
                    case 4: //kBindTransformEuler
                        curves += 3;
                        break;
                    case 2: //kBindTransformRotation
                        curves += 4;
                        break;
                    default:
                        curves += 1;
                        break;
                }
            else
                curves += 1;

            if (curves > index)
                return b;
        }

        return null;
    }
}

public class AnimationEvent
{
    public string data;
    public float floatParameter;
    public string functionName;
    public int intParameter;
    public int messageOptions;
    public PPtr<Object> objectReferenceParameter;
    public float time;

    public AnimationEvent(ObjectReader reader)
    {
        var version = reader.version;

        time = reader.ReadSingle();
        functionName = reader.ReadAlignedString();
        data = reader.ReadAlignedString();
        objectReferenceParameter = new PPtr<Object>(reader);
        floatParameter = reader.ReadSingle();
        if (version[0] >= 3) //3 and up
            intParameter = reader.ReadInt32();
        messageOptions = reader.ReadInt32();
    }
}

public enum AnimationType
{
    Legacy = 1,
    Generic = 2,
    Humanoid = 3
}

public sealed class AnimationClip : NamedObject
{
    public AnimationType m_AnimationType;
    public AABB m_Bounds;
    public AnimationClipBindingConstant m_ClipBindingConstant;
    public bool m_Compressed;
    public CompressedAnimationCurve[] m_CompressedRotationCurves;
    public Vector3Curve[] m_EulerCurves;
    public AnimationEvent[] m_Events;
    public FloatCurve[] m_FloatCurves;
    public bool m_Legacy;
    public ClipMuscleConstant m_MuscleClip;
    public uint m_MuscleClipSize;
    public Vector3Curve[] m_PositionCurves;
    public PPtrCurve[] m_PPtrCurves;
    public QuaternionCurve[] m_RotationCurves;
    public float m_SampleRate;
    public Vector3Curve[] m_ScaleCurves;
    public bool m_UseHighQualityCurve;
    public int m_WrapMode;


    public AnimationClip(ObjectReader reader) : base(reader)
    {
        if (version[0] >= 5) //5.0 and up
        {
            m_Legacy = reader.ReadBoolean();
        }
        else if (version[0] >= 4) //4.0 and up
        {
            m_AnimationType = (AnimationType)reader.ReadInt32();
            if (m_AnimationType == AnimationType.Legacy)
                m_Legacy = true;
        }
        else
        {
            m_Legacy = true;
        }

        m_Compressed = reader.ReadBoolean();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 3) //4.3 and up
            m_UseHighQualityCurve = reader.ReadBoolean();
        reader.AlignStream();
        var numRCurves = reader.ReadInt32();
        m_RotationCurves = new QuaternionCurve[numRCurves];
        for (var i = 0; i < numRCurves; i++)
            m_RotationCurves[i] = new QuaternionCurve(reader);

        var numCRCurves = reader.ReadInt32();
        m_CompressedRotationCurves = new CompressedAnimationCurve[numCRCurves];
        for (var i = 0; i < numCRCurves; i++)
            m_CompressedRotationCurves[i] = new CompressedAnimationCurve(reader);

        if (version[0] > 5 || version[0] == 5 && version[1] >= 3) //5.3 and up
        {
            var numEulerCurves = reader.ReadInt32();
            m_EulerCurves = new Vector3Curve[numEulerCurves];
            for (var i = 0; i < numEulerCurves; i++)
                m_EulerCurves[i] = new Vector3Curve(reader);
        }

        var numPCurves = reader.ReadInt32();
        m_PositionCurves = new Vector3Curve[numPCurves];
        for (var i = 0; i < numPCurves; i++)
            m_PositionCurves[i] = new Vector3Curve(reader);

        var numSCurves = reader.ReadInt32();
        m_ScaleCurves = new Vector3Curve[numSCurves];
        for (var i = 0; i < numSCurves; i++)
            m_ScaleCurves[i] = new Vector3Curve(reader);

        var numFCurves = reader.ReadInt32();
        m_FloatCurves = new FloatCurve[numFCurves];
        for (var i = 0; i < numFCurves; i++)
            m_FloatCurves[i] = new FloatCurve(reader);

        if (version[0] > 4 || version[0] == 4 && version[1] >= 3) //4.3 and up
        {
            var numPtrCurves = reader.ReadInt32();
            m_PPtrCurves = new PPtrCurve[numPtrCurves];
            for (var i = 0; i < numPtrCurves; i++)
                m_PPtrCurves[i] = new PPtrCurve(reader);
        }

        m_SampleRate = reader.ReadSingle();
        m_WrapMode = reader.ReadInt32();
        if (version[0] > 3 || version[0] == 3 && version[1] >= 4) //3.4 and up
            m_Bounds = new AABB(reader);
        if (version[0] >= 4) //4.0 and up
        {
            m_MuscleClipSize = reader.ReadUInt32();
            m_MuscleClip = new ClipMuscleConstant(reader);
        }

        if (version[0] > 4 || version[0] == 4 && version[1] >= 3) //4.3 and up
            m_ClipBindingConstant = new AnimationClipBindingConstant(reader);
        if (version[0] > 2018 || version[0] == 2018 && version[1] >= 3) //2018.3 and up
        {
            var m_HasGenericRootTransform = reader.ReadBoolean();
            var m_HasMotionFloatCurves = reader.ReadBoolean();
            reader.AlignStream();
        }

        var numEvents = reader.ReadInt32();
        m_Events = new AnimationEvent[numEvents];
        for (var i = 0; i < numEvents; i++)
            m_Events[i] = new AnimationEvent(reader);
        if (version[0] >= 2017) //2017 and up
            reader.AlignStream();
    }
}
