namespace AssetStudio;

public sealed class GameObject : EditorExtension
{
    public Animation m_Animation;
    public Animator m_Animator;
    public PPtr<Component>[] m_Components;
    public MeshFilter m_MeshFilter;
    public MeshRenderer m_MeshRenderer;
    public string m_Name;
    public SkinnedMeshRenderer m_SkinnedMeshRenderer;

    public Transform m_Transform;

    public GameObject(ObjectReader reader) : base(reader)
    {
        var m_Component_size = reader.ReadInt32();
        m_Components = new PPtr<Component>[m_Component_size];
        for (var i = 0; i < m_Component_size; i++)
        {
            if (version[0] == 5 && version[1] < 5 || version[0] < 5) //5.5 down
            {
                var first = reader.ReadInt32();
            }

            m_Components[i] = new PPtr<Component>(reader);
        }

        var m_Layer = reader.ReadInt32();
        m_Name = reader.ReadAlignedString();
    }
}
