namespace AssetStudio;

public class Transform : Component
{
    public PPtr<Transform>[] m_Children;
    public PPtr<Transform> m_Father;
    public Vector3 m_LocalPosition;
    public Quaternion m_LocalRotation;
    public Vector3 m_LocalScale;

    public Transform(ObjectReader reader) : base(reader)
    {
        m_LocalRotation = reader.ReadQuaternion();
        m_LocalPosition = reader.ReadVector3();
        m_LocalScale = reader.ReadVector3();

        var m_ChildrenCount = reader.ReadInt32();
        m_Children = new PPtr<Transform>[m_ChildrenCount];
        for (var i = 0; i < m_ChildrenCount; i++)
            m_Children[i] = new PPtr<Transform>(reader);
        m_Father = new PPtr<Transform>(reader);
    }
}
