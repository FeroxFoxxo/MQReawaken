using Server.Base.Accounts.Enums;

namespace Server.Base.Accounts.Extensions;

public static class GetError
{
    public static string GetErrorValue(this AlrReason rejectReason) =>
        rejectReason switch
        {
            AlrReason.Invalid =>
                "It seems like you have entered an incorrect username. Please make sure your username is correct and try again!",
            AlrReason.BadComm =>
                "It seems like you've been denied access from this account. Please contact a server administrator if you believe this to be an error!",
            AlrReason.BadPass => "It seems like you've entered an invalid password! Please retry...",
            AlrReason.Blocked => "Your account has been banned from MQReawaken. We apologize for the inconvenience.",
            AlrReason.InUse =>
                "It seems like we've reached our thresh-hold for monkeys! This could be due to a lot of people playing right now. Please try again in an hour.",
            _ =>
                "You seem to have reached an error that shouldn't have happened! Please report this error to the developers."
        };
}
