using System;
using Dirigent.Auth.Entity;

namespace Dirigent.Auth.Services {
    public interface IAuthService {
        UserInfo GetUser();
    }
}