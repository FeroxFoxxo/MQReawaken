using System.ComponentModel.DataAnnotations;

namespace Server.Base.Core.Models;

public abstract class JsonData
{
    [Key] public int UserId { get; set; }
}
