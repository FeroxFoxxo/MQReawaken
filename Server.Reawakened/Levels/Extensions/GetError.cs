using Server.Reawakened.Levels.Enums;

namespace Server.Reawakened.Levels.Extensions;

public static class GetError
{
    public static string GetErrorValue(this JoinReason rejectReason) =>
        rejectReason switch
        {
            JoinReason.Full => "This room is full!",
            _ =>
                "You seem to have reached an error that shouldn't have happened! Please report this error to the developers."
        };
}
