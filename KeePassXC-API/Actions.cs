using System;
using System.Diagnostics;
using System.Reflection;

namespace KeePassXC_API
{
    [DebuggerDisplay("{Type}")]
    public class Actions
    {
        public string Type { get; private set; }

        public static readonly Actions SetLogin = new("set-login");
        public static readonly Actions GetLogins = new("get-logins");
        public static readonly Actions GeneratePassword = new("generate-password");
        public static readonly Actions Associate = new("associate");
        public static readonly Actions TestAssociate = new("test-associate");
        public static readonly Actions GetDatabaseHash = new("get-databasehash");
        public static readonly Actions ExchangePublicKeys = new("change-public-keys");
        public static readonly Actions LockDatabase = new("lock-database");
        public static readonly Actions DatabaseLocked = new("database-locked");
        public static readonly Actions DatabaseUnlocked = new("database-unlocked");
        public static readonly Actions GetDatabaseGroups = new("get-database-groups");
        public static readonly Actions CreateNewGroup = new("create-new-group");
        public static readonly Actions GetTOTP = new("get-totp");
        public static readonly Actions LoadKeyring = new("load_keyring");

        private Actions(string type)
        {
            this.Type = type;
        }

        public static Actions TryConvert(string type)
        {
            foreach(FieldInfo prop in typeof(Actions).GetFields())
            {
                Actions action = prop.GetValue(null) as Actions;
                if (action.Type == type)
                {
                    return action;
                }
            }

            throw new Exception("key not found");
        }
    }
}
