using System;
using System.Diagnostics;
using System.Reflection;

namespace KeePassXC_API
{
    [DebuggerDisplay("{Type}")]
    public class Actions
    {
        public string Type { get; private set; }

        public static readonly Actions SetLogin = new Actions("set-login");
        public static readonly Actions GetLogins = new Actions("get-logins");
        public static readonly Actions GeneratePassword = new Actions("generate-password");
        public static readonly Actions Associate = new Actions("associate");
        public static readonly Actions TestAssociate = new Actions("test-associate");
        public static readonly Actions GetDatabaseHash = new Actions("get-databasehash");
        public static readonly Actions ExchangePublicKeys = new Actions("change-public-keys");
        public static readonly Actions LockDatabase = new Actions("lock-database");
        public static readonly Actions DatabaseLocked = new Actions("database-locked");
        public static readonly Actions DatabaseUnlocked = new Actions("database-unlocked");
        public static readonly Actions GetDatabaseGroups = new Actions("get-database-groups");
        public static readonly Actions CreateNewGroup = new Actions("create-new-group");
        public static readonly Actions GetTOTP = new Actions("get-totp");
        public static readonly Actions LoadKeyring = new Actions("load_keyring");

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
