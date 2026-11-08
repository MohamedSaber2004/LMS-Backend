using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LMS.Infrastructure.Services.Agora.Token
{
    public class AccessToken2
    {
        private const string Version = "007";
        public const ushort ServiceTypeRtc = 1;
        public const ushort ServiceTypeRtm = 2;

        public string AppId { get; set; } = string.Empty;
        public string AppCert { get; set; } = string.Empty;
        public uint Expire { get; set; }
        public uint IssueTs { get; set; }
        public uint Salt { get; set; }
        public List<Service> Services { get; } = new List<Service>();

        public AccessToken2() { }

        public AccessToken2(string appId, string appCert, uint expire)
        {
            AppId = appId;
            AppCert = appCert;
            Expire = expire;
            IssueTs = (uint)Utils.GetTimestamp();
            Salt = (uint)Utils.RandomInt();
        }

        public void AddService(Service service)
        {
            Services.Add(service);
        }

        public static string GetUidStr(uint uid)
        {
            return uid == 0 ? string.Empty : uid.ToString(CultureInfo.InvariantCulture);
        }

        public byte[] GetSign()
        {
            byte[] signing = DynamicKeyUtil.EncodeHMAC(BitConverter.GetBytes(IssueTs), Encoding.UTF8.GetBytes(AppCert), "SHA256");
            return DynamicKeyUtil.EncodeHMAC(BitConverter.GetBytes(Salt), signing, "SHA256");
        }

        public string Build()
        {
            if (!Utils.IsUUID(AppId) || !Utils.IsUUID(AppCert) || Services.Count == 0)
            {
                return string.Empty;
            }

            var buf = new ByteBuf()
                .put(Encoding.UTF8.GetBytes(AppId))
                .put(IssueTs)
                .put(Expire)
                .put(Salt)
                .put((ushort)Services.Count);

            byte[] signing = GetSign();

            foreach (var service in Services)
            {
                buf.put((ushort)service.GetServiceType()).put(service.Pack());
            }

            byte[] signature = DynamicKeyUtil.EncodeHMAC(signing, buf.asBytes(), "SHA256");
            var bufferContent = new ByteBuf().put(signature).put(buf.asBytes());

            return Version + Utils.Base64Encode(Utils.Compress(bufferContent.asBytes()));
        }

        public abstract class Service
        {
            private ushort _serviceType;
            private readonly Dictionary<ushort, uint> _privileges = new();

            public void SetServiceType(ushort type)
            {
                _serviceType = type;
            }

            public ushort GetServiceType()
            {
                return _serviceType;
            }

            public void AddPrivilege(ushort privilege, uint expire)
            {
                _privileges[privilege] = expire;
            }

            public virtual ByteBuf Pack(ByteBuf buf)
            {
                return buf.putIntMap(_privileges);
            }

            public byte[] Pack()
            {
                var buf = new ByteBuf();
                return Pack(buf).asBytes();
            }
        }

        public enum PrivilegeRtcEnum : ushort
        {
            PrivilegeJoinChannel = 1,
            PrivilegePublishAudioStream = 2,
            PrivilegePublishVideoStream = 3,
            PrivilegePublishDataStream = 4
        }

        public class ServiceRtc : Service
        {
            public string ChannelName { get; set; }
            public string Uid { get; set; }

            public ServiceRtc() : this(string.Empty, string.Empty) { }

            public ServiceRtc(string channelName, string uid)
            {
                SetServiceType(ServiceTypeRtc);
                ChannelName = channelName;
                Uid = uid;
            }

            public void AddPrivilegeRtc(PrivilegeRtcEnum privilege, uint expire)
            {
                AddPrivilege((ushort)privilege, expire);
            }

            public override ByteBuf Pack(ByteBuf buf)
            {
                return base.Pack(buf)
                    .put(Encoding.UTF8.GetBytes(ChannelName))
                    .put(Encoding.UTF8.GetBytes(Uid));
            }
        }
    }
}
