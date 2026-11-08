namespace LMS.Infrastructure.Services.Agora.Token
{
    public static class RtcTokenBuilder2
    {
        public enum Role
        {
            RolePublisher = 1,
            RoleSubscriber = 2
        }

        public static string BuildTokenWithUid(
            string appId,
            string appCertificate,
            string channelName,
            uint uid,
            Role role,
            uint tokenExpire,
            uint privilegeExpire)
        {
            return BuildTokenWithUserAccount(
                appId,
                appCertificate,
                channelName,
                AccessToken2.GetUidStr(uid),
                role,
                tokenExpire,
                privilegeExpire);
        }

        public static string BuildTokenWithUserAccount(
            string appId,
            string appCertificate,
            string channelName,
            string account,
            Role role,
            uint tokenExpire,
            uint privilegeExpire)
        {
            var accessToken = new AccessToken2(appId, appCertificate, tokenExpire);
            var serviceRtc = new AccessToken2.ServiceRtc(channelName, account);

            serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegeJoinChannel, privilegeExpire);

            if (role == Role.RolePublisher)
            {
                serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegePublishAudioStream, privilegeExpire);
                serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegePublishVideoStream, privilegeExpire);
                serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegePublishDataStream, privilegeExpire);
            }

            accessToken.AddService(serviceRtc);
            return accessToken.Build();
        }
    }
}

