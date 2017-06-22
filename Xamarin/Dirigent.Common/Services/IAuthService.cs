using System;
using Dirigent.Common.Entity;

namespace Dirigent.Common.Services {
    public interface IAuthService {
        UserInfo GetUser();
    }
}