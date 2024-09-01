using SytsBackendGen2.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Domain.Entities.Authentification;

[NotCached]
public class RefreshToken : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Token { get; set; }

    [Required]
    public DateTimeOffset ExpirationDate { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    public bool Invalidated { get; set; } = false;

    public User User { get; set; }

    public RefreshToken()
    {
        
    }

    public RefreshToken(string token, DateTimeOffset expirationDate)
    {
        Update(token, expirationDate);
    }

    public void Update(string token, DateTimeOffset expirationDate)
    {
        Token = token;
        ExpirationDate = expirationDate;
    }
}
