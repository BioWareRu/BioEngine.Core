using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Users
{
    public interface IUserBioRepositoryOperationContext<TUserPk> : IBioRepositoryOperationContext
    {
        IUser<TUserPk> User { get; set; }
    }
}
