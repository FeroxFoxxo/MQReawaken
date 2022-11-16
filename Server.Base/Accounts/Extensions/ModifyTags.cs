using Server.Base.Accounts.Modals;

namespace Server.Base.Accounts.Extensions;

public static class ModifyTags
{
    public static void RemoveTag(this Account account, string name)
    {
        for (var increment = account.Tags.Count - 1; increment >= 0; --increment)
        {
            if (increment >= account.Tags.Count)
                continue;

            var tag = account.Tags[increment];

            if (tag.Name == name)
                account.Tags.RemoveAt(increment);
        }
    }

    public static void SetTag(this Account account, string name, string value)
    {
        var tag = account.Tags.FirstOrDefault(tags => tags.Name == name);

        if (tag != null)
            tag.Value = value;
        else
            account.AddTag(name, value);
    }

    public static void AddTag(this Account account, string name, string value) =>
        account.Tags.Add(new AccountTag(name, value));

    public static string GetTag(this Account account, string name) =>
        account.Tags.Where(tag => tag.Name == name).Select(tag => tag.Value).FirstOrDefault();

    public static bool GetFlag(this Account account, int index) => (account.Flags & (1 << index)) != 0;

    public static void SetFlag(this Account account, int index, bool value)
    {
        if (value)
            account.Flags |= 1 << index;
        else
            account.Flags &= ~(1 << index);
    }
}
