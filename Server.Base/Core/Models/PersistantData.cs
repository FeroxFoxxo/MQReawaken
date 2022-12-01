using System.ComponentModel.DataAnnotations;

namespace Server.Base.Core.Models;

public abstract class PersistantData
{
    [Key] public int UserId { get; set; }
}
