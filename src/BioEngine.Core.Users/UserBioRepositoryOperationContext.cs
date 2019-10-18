using BioEngine.Core.Repository;

namespace BioEngine.Core.Users
{
    public class UserBioRepositoryOperationContext<TUserPk> : BioRepositoryOperationContext,
        IUserBioRepositoryOperationContext<TUserPk>
    {
        public IUser<TUserPk> User { get; set; }
    }
}
