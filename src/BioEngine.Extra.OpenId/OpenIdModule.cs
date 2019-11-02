using System;
using BioEngine.Core.Users;

namespace BioEngine.Extra.OpenId
{
    public abstract class OpenIdModule<TConfig, TUser, TUserPk, TUserDataProvider> : BaseUsersModule<TConfig, TUserPk,
        TUserDataProvider, OpenIdCurrentUserProvider<TUser, TUserPk>>
        where TConfig : OpenIdModuleConfig
        where TUserDataProvider : class, IUserDataProvider<TUserPk>
        where TUser : IUser<TUserPk>, new()
    {
    }

    public abstract class OpenIdModuleConfig : BaseUsersModuleConfig
    {
        public Uri ServerUri { get; }

        public OpenIdModuleConfig(Uri serverUri)
        {
            ServerUri = serverUri;
        }
    }
}
