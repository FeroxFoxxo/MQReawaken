using System.Collections.Generic;

namespace AssetStudio;

public class HumanPoseMask
{
    public uint word0;
    public uint word1;
    public uint word2;

    public HumanPoseMask(ObjectReader reader)
    {
        var version = reader.version;

        word0 = reader.ReadUInt32();
        word1 = reader.ReadUInt32();
        if (version[0] > 5 || version[0] == 5 && version[1] >= 2) //5.2 and up
            word2 = reader.ReadUInt32();
    }
}

public class SkeletonMaskElement
{
    public uint m_PathHash;
    public float m_Weight;

    public SkeletonMaskElement(ObjectReader reader)
    {
        m_PathHash = reader.ReadUInt32();
        m_Weight = reader.ReadSingle();
    }
}

public class SkeletonMask
{
    public SkeletonMaskElement[] m_Data;

    public SkeletonMask(ObjectReader reader)
    {
        var numElements = reader.ReadInt32();
        m_Data = new SkeletonMaskElement[numElements];
        for (var i = 0; i < numElements; i++)
            m_Data[i] = new SkeletonMaskElement(reader);
    }
}

public class LayerConstant
{
    public uint m_Binding;
    public HumanPoseMask m_BodyMask;
    public float m_DefaultWeight;
    public bool m_IKPass;
    public int m_LayerBlendingMode;
    public SkeletonMask m_SkeletonMask;
    public uint m_StateMachineIndex;
    public uint m_StateMachineMotionSetIndex;
    public bool m_SyncedLayerAffectsTiming;

    public LayerConstant(ObjectReader reader)
    {
        var version = reader.version;

        m_StateMachineIndex = reader.ReadUInt32();
        m_StateMachineMotionSetIndex = reader.ReadUInt32();
        m_BodyMask = new HumanPoseMask(reader);
        m_SkeletonMask = new SkeletonMask(reader);
        m_Binding = reader.ReadUInt32();
        m_LayerBlendingMode = reader.ReadInt32();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 2) //4.2 and up
            m_DefaultWeight = reader.ReadSingle();
        m_IKPass = reader.ReadBoolean();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 2) //4.2 and up
            m_SyncedLayerAffectsTiming = reader.ReadBoolean();
        reader.AlignStream();
    }
}

public class ConditionConstant
{
    public uint m_ConditionMode;
    public uint m_EventID;
    public float m_EventThreshold;
    public float m_ExitTime;

    public ConditionConstant(ObjectReader reader)
    {
        m_ConditionMode = reader.ReadUInt32();
        m_EventID = reader.ReadUInt32();
        m_EventThreshold = reader.ReadSingle();
        m_ExitTime = reader.ReadSingle();
    }
}

public class TransitionConstant
{
    public bool m_Atomic;
    public bool m_CanTransitionToSelf;
    public ConditionConstant[] m_ConditionConstantArray;
    public uint m_DestinationState;
    public float m_ExitTime;
    public uint m_FullPathID;
    public bool m_HasExitTime;
    public bool m_HasFixedDuration;
    public uint m_ID;
    public int m_InterruptionSource;
    public bool m_OrderedInterruption;
    public float m_TransitionDuration;
    public float m_TransitionOffset;
    public uint m_UserID;

    public TransitionConstant(ObjectReader reader)
    {
        var version = reader.version;

        var numConditions = reader.ReadInt32();
        m_ConditionConstantArray = new ConditionConstant[numConditions];
        for (var i = 0; i < numConditions; i++)
            m_ConditionConstantArray[i] = new ConditionConstant(reader);

        m_DestinationState = reader.ReadUInt32();
        if (version[0] >= 5) //5.0 and up
            m_FullPathID = reader.ReadUInt32();

        m_ID = reader.ReadUInt32();
        m_UserID = reader.ReadUInt32();
        m_TransitionDuration = reader.ReadSingle();
        m_TransitionOffset = reader.ReadSingle();
        if (version[0] >= 5) //5.0 and up
        {
            m_ExitTime = reader.ReadSingle();
            m_HasExitTime = reader.ReadBoolean();
            m_HasFixedDuration = reader.ReadBoolean();
            reader.AlignStream();
            m_InterruptionSource = reader.ReadInt32();
            m_OrderedInterruption = reader.ReadBoolean();
        }
        else
        {
            m_Atomic = reader.ReadBoolean();
        }

        if (version[0] > 4 || version[0] == 4 && version[1] >= 5) //4.5 and up
            m_CanTransitionToSelf = reader.ReadBoolean();

        reader.AlignStream();
    }
}

public class LeafInfoConstant
{
    public uint[] m_IDArray;
    public uint m_IndexOffset;

    public LeafInfoConstant(ObjectReader reader)
    {
        m_IDArray = reader.ReadUInt32Array();
        m_IndexOffset = reader.ReadUInt32();
    }
}

public class MotionNeighborList
{
    public uint[] m_NeighborArray;

    public MotionNeighborList(ObjectReader reader) => m_NeighborArray = reader.ReadUInt32Array();
}

public class Blend2dDataConstant
{
    public float[] m_ChildMagnitudeArray;
    public MotionNeighborList[] m_ChildNeighborListArray;
    public float[] m_ChildPairAvgMagInvArray;
    public Vector2[] m_ChildPairVectorArray;
    public Vector2[] m_ChildPositionArray;

    public Blend2dDataConstant(ObjectReader reader)
    {
        m_ChildPositionArray = reader.ReadVector2Array();
        m_ChildMagnitudeArray = reader.ReadSingleArray();
        m_ChildPairVectorArray = reader.ReadVector2Array();
        m_ChildPairAvgMagInvArray = reader.ReadSingleArray();

        var numNeighbours = reader.ReadInt32();
        m_ChildNeighborListArray = new MotionNeighborList[numNeighbours];
        for (var i = 0; i < numNeighbours; i++)
            m_ChildNeighborListArray[i] = new MotionNeighborList(reader);
    }
}

public class Blend1dDataConstant // wrong labeled
{
    public float[] m_ChildThresholdArray;

    public Blend1dDataConstant(ObjectReader reader) => m_ChildThresholdArray = reader.ReadSingleArray();
}

public class BlendDirectDataConstant
{
    public uint[] m_ChildBlendEventIDArray;
    public bool m_NormalizedBlendValues;

    public BlendDirectDataConstant(ObjectReader reader)
    {
        m_ChildBlendEventIDArray = reader.ReadUInt32Array();
        m_NormalizedBlendValues = reader.ReadBoolean();
        reader.AlignStream();
    }
}

public class BlendTreeNodeConstant
{
    public Blend1dDataConstant m_Blend1dData;
    public Blend2dDataConstant m_Blend2dData;
    public BlendDirectDataConstant m_BlendDirectData;
    public uint m_BlendEventID;
    public uint m_BlendEventYID;
    public uint m_BlendType;
    public uint[] m_ChildIndices;
    public float[] m_ChildThresholdArray;
    public uint m_ClipID;
    public uint m_ClipIndex;
    public float m_CycleOffset;
    public float m_Duration;
    public bool m_Mirror;

    public BlendTreeNodeConstant(ObjectReader reader)
    {
        var version = reader.version;

        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
            m_BlendType = reader.ReadUInt32();
        m_BlendEventID = reader.ReadUInt32();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
            m_BlendEventYID = reader.ReadUInt32();
        m_ChildIndices = reader.ReadUInt32Array();
        if (version[0] < 4 || version[0] == 4 && version[1] < 1) //4.1 down
            m_ChildThresholdArray = reader.ReadSingleArray();

        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
        {
            m_Blend1dData = new Blend1dDataConstant(reader);
            m_Blend2dData = new Blend2dDataConstant(reader);
        }

        if (version[0] >= 5) //5.0 and up
            m_BlendDirectData = new BlendDirectDataConstant(reader);

        m_ClipID = reader.ReadUInt32();
        if (version[0] == 4 && version[1] >= 5) //4.5 - 5.0
            m_ClipIndex = reader.ReadUInt32();

        m_Duration = reader.ReadSingle();

        if (version[0] > 4
            || version[0] == 4 && version[1] > 1
            || version[0] == 4 && version[1] == 1 && version[2] >= 3) //4.1.3 and up
        {
            m_CycleOffset = reader.ReadSingle();
            m_Mirror = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}

public class BlendTreeConstant
{
    public ValueArrayConstant m_BlendEventArrayConstant;
    public BlendTreeNodeConstant[] m_NodeArray;

    public BlendTreeConstant(ObjectReader reader)
    {
        var version = reader.version;

        var numNodes = reader.ReadInt32();
        m_NodeArray = new BlendTreeNodeConstant[numNodes];
        for (var i = 0; i < numNodes; i++)
            m_NodeArray[i] = new BlendTreeNodeConstant(reader);

        if (version[0] < 4 || version[0] == 4 && version[1] < 5) //4.5 down
            m_BlendEventArrayConstant = new ValueArrayConstant(reader);
    }
}

public class StateConstant
{
    public BlendTreeConstant[] m_BlendTreeConstantArray;
    public int[] m_BlendTreeConstantIndexArray;
    public float m_CycleOffset;
    public uint m_CycleOffsetParamID;
    public uint m_FullPathID;
    public bool m_IKOnFeet;
    public LeafInfoConstant[] m_LeafInfoArray;
    public bool m_Loop;
    public bool m_Mirror;
    public uint m_MirrorParamID;
    public uint m_NameID;
    public uint m_PathID;
    public float m_Speed;
    public uint m_SpeedParamID;
    public uint m_TagID;
    public TransitionConstant[] m_TransitionConstantArray;
    public bool m_WriteDefaultValues;

    public StateConstant(ObjectReader reader)
    {
        var version = reader.version;

        var numTransistions = reader.ReadInt32();
        m_TransitionConstantArray = new TransitionConstant[numTransistions];
        for (var i = 0; i < numTransistions; i++)
            m_TransitionConstantArray[i] = new TransitionConstant(reader);

        m_BlendTreeConstantIndexArray = reader.ReadInt32Array();

        if (version[0] < 5 || version[0] == 5 && version[1] < 2) //5.2 down
        {
            var numInfos = reader.ReadInt32();
            m_LeafInfoArray = new LeafInfoConstant[numInfos];
            for (var i = 0; i < numInfos; i++)
                m_LeafInfoArray[i] = new LeafInfoConstant(reader);
        }

        var numBlends = reader.ReadInt32();
        m_BlendTreeConstantArray = new BlendTreeConstant[numBlends];
        for (var i = 0; i < numBlends; i++)
            m_BlendTreeConstantArray[i] = new BlendTreeConstant(reader);

        m_NameID = reader.ReadUInt32();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 3) //4.3 and up
            m_PathID = reader.ReadUInt32();
        if (version[0] >= 5) //5.0 and up
            m_FullPathID = reader.ReadUInt32();

        m_TagID = reader.ReadUInt32();
        if (version[0] > 5 || version[0] == 5 && version[1] >= 1) //5.1 and up
        {
            m_SpeedParamID = reader.ReadUInt32();
            m_MirrorParamID = reader.ReadUInt32();
            m_CycleOffsetParamID = reader.ReadUInt32();
        }

        if (version[0] > 2017 || version[0] == 2017 && version[1] >= 2) //2017.2 and up
        {
            var m_TimeParamID = reader.ReadUInt32();
        }

        m_Speed = reader.ReadSingle();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
            m_CycleOffset = reader.ReadSingle();
        m_IKOnFeet = reader.ReadBoolean();
        if (version[0] >= 5) //5.0 and up
            m_WriteDefaultValues = reader.ReadBoolean();

        m_Loop = reader.ReadBoolean();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
            m_Mirror = reader.ReadBoolean();

        reader.AlignStream();
    }
}

public class SelectorTransitionConstant
{
    public ConditionConstant[] m_ConditionConstantArray;
    public uint m_Destination;

    public SelectorTransitionConstant(ObjectReader reader)
    {
        m_Destination = reader.ReadUInt32();

        var numConditions = reader.ReadInt32();
        m_ConditionConstantArray = new ConditionConstant[numConditions];
        for (var i = 0; i < numConditions; i++)
            m_ConditionConstantArray[i] = new ConditionConstant(reader);
    }
}

public class SelectorStateConstant
{
    public uint m_FullPathID;
    public bool m_isEntry;
    public SelectorTransitionConstant[] m_TransitionConstantArray;

    public SelectorStateConstant(ObjectReader reader)
    {
        var numTransitions = reader.ReadInt32();
        m_TransitionConstantArray = new SelectorTransitionConstant[numTransitions];
        for (var i = 0; i < numTransitions; i++)
            m_TransitionConstantArray[i] = new SelectorTransitionConstant(reader);

        m_FullPathID = reader.ReadUInt32();
        m_isEntry = reader.ReadBoolean();
        reader.AlignStream();
    }
}

public class StateMachineConstant
{
    public TransitionConstant[] m_AnyStateTransitionConstantArray;
    public uint m_DefaultState;
    public uint m_MotionSetCount;
    public SelectorStateConstant[] m_SelectorStateConstantArray;
    public StateConstant[] m_StateConstantArray;

    public StateMachineConstant(ObjectReader reader)
    {
        var version = reader.version;

        var numStates = reader.ReadInt32();
        m_StateConstantArray = new StateConstant[numStates];
        for (var i = 0; i < numStates; i++)
            m_StateConstantArray[i] = new StateConstant(reader);

        var numAnyStates = reader.ReadInt32();
        m_AnyStateTransitionConstantArray = new TransitionConstant[numAnyStates];
        for (var i = 0; i < numAnyStates; i++)
            m_AnyStateTransitionConstantArray[i] = new TransitionConstant(reader);

        if (version[0] >= 5) //5.0 and up
        {
            var numSelectors = reader.ReadInt32();
            m_SelectorStateConstantArray = new SelectorStateConstant[numSelectors];
            for (var i = 0; i < numSelectors; i++)
                m_SelectorStateConstantArray[i] = new SelectorStateConstant(reader);
        }

        m_DefaultState = reader.ReadUInt32();
        m_MotionSetCount = reader.ReadUInt32();
    }
}

public class ValueArray
{
    public bool[] m_BoolValues;
    public float[] m_FloatValues;
    public int[] m_IntValues;
    public Vector3[] m_PositionValues;
    public Vector4[] m_QuaternionValues;
    public Vector3[] m_ScaleValues;
    public Vector4[] m_VectorValues;

    public ValueArray(ObjectReader reader)
    {
        var version = reader.version;

        if (version[0] < 5 || version[0] == 5 && version[1] < 5) //5.5 down
        {
            m_BoolValues = reader.ReadBooleanArray();
            reader.AlignStream();
            m_IntValues = reader.ReadInt32Array();
            m_FloatValues = reader.ReadSingleArray();
        }

        if (version[0] < 4 || version[0] == 4 && version[1] < 3) //4.3 down
        {
            m_VectorValues = reader.ReadVector4Array();
        }
        else
        {
            var numPosValues = reader.ReadInt32();
            m_PositionValues = new Vector3[numPosValues];
            for (var i = 0; i < numPosValues; i++)
                m_PositionValues[i] = version[0] > 5 || version[0] == 5 && version[1] >= 4
                    ? reader.ReadVector3()
                    : reader.ReadVector4(); //5.4 and up

            m_QuaternionValues = reader.ReadVector4Array();

            var numScaleValues = reader.ReadInt32();
            m_ScaleValues = new Vector3[numScaleValues];
            for (var i = 0; i < numScaleValues; i++)
                m_ScaleValues[i] = version[0] > 5 || version[0] == 5 && version[1] >= 4
                    ? reader.ReadVector3()
                    : reader.ReadVector4(); //5.4 and up

            if (version[0] > 5 || version[0] == 5 && version[1] >= 5) //5.5 and up
            {
                m_FloatValues = reader.ReadSingleArray();
                m_IntValues = reader.ReadInt32Array();
                m_BoolValues = reader.ReadBooleanArray();
                reader.AlignStream();
            }
        }
    }
}

public class ControllerConstant
{
    public ValueArray m_DefaultValues;
    public LayerConstant[] m_LayerArray;
    public StateMachineConstant[] m_StateMachineArray;
    public ValueArrayConstant m_Values;

    public ControllerConstant(ObjectReader reader)
    {
        var numLayers = reader.ReadInt32();
        m_LayerArray = new LayerConstant[numLayers];
        for (var i = 0; i < numLayers; i++)
            m_LayerArray[i] = new LayerConstant(reader);

        var numStates = reader.ReadInt32();
        m_StateMachineArray = new StateMachineConstant[numStates];
        for (var i = 0; i < numStates; i++)
            m_StateMachineArray[i] = new StateMachineConstant(reader);

        m_Values = new ValueArrayConstant(reader);
        m_DefaultValues = new ValueArray(reader);
    }
}

public sealed class AnimatorController : RuntimeAnimatorController
{
    public PPtr<AnimationClip>[] m_AnimationClips;

    public AnimatorController(ObjectReader reader) : base(reader)
    {
        var m_ControllerSize = reader.ReadUInt32();
        var m_Controller = new ControllerConstant(reader);

        var tosSize = reader.ReadInt32();
        var m_TOS = new KeyValuePair<uint, string>[tosSize];
        for (var i = 0; i < tosSize; i++)
            m_TOS[i] = new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString());

        var numClips = reader.ReadInt32();
        m_AnimationClips = new PPtr<AnimationClip>[numClips];
        for (var i = 0; i < numClips; i++)
            m_AnimationClips[i] = new PPtr<AnimationClip>(reader);
    }
}
