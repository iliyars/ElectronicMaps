using ElectronicMaps.Domain.Common;
using ElectronicMaps.Domain.Enums;


namespace ElectronicMaps.Domain.Entities
{
    public class ComponentFamily : DomainObject
    {
        public string Name { get; set; } = null!;

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;

        public DateTimeOffset? VerifiedAt { get; set; }
        public int? VerifiedByUserId { get; set; }
        public string? VerificationNote { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int? CreatedByUserId { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }

        public int Version { get; set; } = 1;

        public int FamilyFormTypeId { get; set; } 
        public FormType? FamilyFormType { get; set; }
        public ICollection<Component> Components { get; set; } = new List<Component>();
        public ICollection<ParameterValue> ParameterValues { get;set; } = new List<ParameterValue>();

        public void MarkAsModified(int? userId = null)
        {
            UpdatedAt = DateTimeOffset.UtcNow;
            UpdatedByUserId = userId;
            Version++; 
        }

        public void Verify(int userId, string? note = null)
        {
            VerificationStatus = VerificationStatus.Verified;
            VerifiedAt = DateTimeOffset.UtcNow;
            VerifiedByUserId = userId;
            VerificationNote = note;
            MarkAsModified(userId);
        }

        public void Reject(int userId, string reason)
        {
            VerificationStatus = VerificationStatus.Rejected;
            VerifiedAt = DateTimeOffset.UtcNow;
            VerifiedByUserId = userId;
            VerificationNote = reason;
            MarkAsModified(userId);
        }
    }
}
